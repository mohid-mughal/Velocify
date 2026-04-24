using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Velocify.Application.DTOs.Import;
using Velocify.Application.Interfaces;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using Velocify.Infrastructure.Data;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Infrastructure.Services.AiServices;

/// <summary>
/// Service for AI-powered CSV import with intelligent column mapping and normalization.
/// Analyzes CSV headers and maps them to internal schema fields, normalizes non-standard values.
/// Requirements: 13.1-13.7
/// </summary>
public class AiImportService : IAiImportService
{
    private readonly VelocifyDbContext _context;
    private readonly ILogger<AiImportService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly AsyncRetryPolicy _retryPolicy;

    public AiImportService(
        VelocifyDbContext context,
        ILogger<AiImportService> logger,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;

        // RETRY POLICY EXPLANATION:
        // AI services can experience transient failures due to rate limiting, network issues, or temporary service degradation.
        // Without retries, users would see errors for requests that could succeed on a second attempt.
        //
        // EXPONENTIAL BACKOFF STRATEGY:
        // - Attempt 1: Immediate execution
        // - Attempt 2: Wait 1 second (2^0 = 1)
        // - Attempt 3: Wait 2 seconds (2^1 = 2)
        // - Attempt 4: Wait 4 seconds (2^2 = 4)
        //
        // This pattern prevents overwhelming the AI service during outages while giving it time to recover.
        // Total maximum wait time: 1 + 2 + 4 = 7 seconds across 3 retries (4 total attempts).
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "CSV import normalization failed on attempt {RetryCount}. Retrying after {RetryDelay}ms. Error: {ErrorMessage}",
                        retryCount,
                        timeSpan.TotalMilliseconds,
                        exception.Message);
                });
    }

    /// <summary>
    /// Analyzes CSV headers and maps them to internal schema fields.
    /// Normalizes non-standard values to valid enum values.
    /// Returns normalized list for user review before import.
    /// REQUIREMENT 13.1: Analyze the column headers and map them to internal schema fields
    /// REQUIREMENT 13.2: Normalize non-standard values to valid enum values
    /// REQUIREMENT 13.3: Return the mapped and normalized task list for user review
    /// REQUIREMENT 13.7: Log import operations to AiInteractionLog with FeatureType.Import
    /// </summary>
    public async Task<List<TaskImportRow>> NormalizeImport(string csvData)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = GetCurrentUserId();

        try
        {
            _logger.LogInformation(
                "Starting CSV import normalization for user {UserId}. CSV length: {CsvLength} characters",
                userId,
                csvData.Length);

            // Parse CSV data into raw rows
            var rawRows = ParseCsvData(csvData);

            if (rawRows.Count == 0)
            {
                throw new InvalidOperationException("CSV file is empty or contains no valid data rows");
            }

            _logger.LogInformation(
                "Parsed {RowCount} rows from CSV for user {UserId}",
                rawRows.Count,
                userId);

            // Execute AI normalization with retry policy
            var normalizedRows = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await NormalizeWithLangChain(rawRows);
            });

            stopwatch.Stop();

            // REQUIREMENT 13.7: Log to AiInteractionLog with FeatureType.Import
            await LogAiInteraction(
                userId,
                rawRows.Count,
                normalizedRows.Count,
                tokensUsed: null, // LangChain 0.13.0 may not expose token counts directly
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            _logger.LogInformation(
                "Successfully normalized {NormalizedCount} rows from CSV for user {UserId} in {ElapsedMs}ms",
                normalizedRows.Count,
                userId,
                stopwatch.ElapsedMilliseconds);

            return normalizedRows;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Failed to normalize CSV import after all retry attempts for user {UserId}. Elapsed: {ElapsedMs}ms",
                userId,
                stopwatch.ElapsedMilliseconds);

            // Log failed interaction
            await LogFailedAiInteraction(
                userId,
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            throw new InvalidOperationException(
                "Unable to normalize CSV import. Please check the file format and try again.",
                ex);
        }
    }

    /// <summary>
    /// Parses CSV data into a list of dictionaries representing raw rows.
    /// Each dictionary maps column headers to cell values.
    /// </summary>
    private List<Dictionary<string, string>> ParseCsvData(string csvData)
    {
        var rows = new List<Dictionary<string, string>>();
        var lines = csvData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
        {
            return rows; // No data rows
        }

        // Parse header row
        var headers = ParseCsvLine(lines[0]);

        // Parse data rows
        for (int i = 1; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i]);
            
            if (values.Count != headers.Count)
            {
                _logger.LogWarning(
                    "Row {RowNumber} has {ValueCount} values but header has {HeaderCount} columns. Skipping row.",
                    i + 1,
                    values.Count,
                    headers.Count);
                continue;
            }

            var row = new Dictionary<string, string>();
            for (int j = 0; j < headers.Count; j++)
            {
                row[headers[j]] = values[j];
            }
            rows.Add(row);
        }

        return rows;
    }

    /// <summary>
    /// Parses a single CSV line, handling quoted values and commas within quotes.
    /// </summary>
    private List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString().Trim());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        // Add the last value
        values.Add(currentValue.ToString().Trim());

        return values;
    }

    /// <summary>
    /// Performs the actual AI normalization using LangChain structured output parser.
    /// Uses OpenAI GPT model to map non-standard column headers and normalize enum values.
    /// REQUIREMENT 13.1: Analyze CSV headers with LangChain and map to internal schema fields
    /// REQUIREMENT 13.2: Normalize non-standard values to valid enum values
    /// </summary>
    private async Task<List<TaskImportRow>> NormalizeWithLangChain(List<Dictionary<string, string>> rawRows)
    {
        // Get OpenAI API key from configuration
        var apiKey = _configuration["OpenAI:ApiKey"] 
            ?? throw new InvalidOperationException("OpenAI API key not configured");

        // Initialize OpenAI provider and chat model
        // Using gpt-3.5-turbo for fast, cost-effective normalization
        var provider = new OpenAiProvider(apiKey);
        var model = new OpenAiChatModel(provider, id: "gpt-3.5-turbo");

        // Extract sample rows for AI analysis (first 3 rows to understand the data)
        var sampleRows = rawRows.Take(3).ToList();
        var sampleJson = System.Text.Json.JsonSerializer.Serialize(sampleRows, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        // LANGCHAIN STRUCTURED OUTPUT PARSER:
        // We use a carefully crafted prompt to instruct the model to analyze CSV headers
        // and provide a mapping from non-standard column names to our internal schema fields.
        //
        // REQUIREMENT 13.1: Analyze the column headers and map them to internal schema fields
        // REQUIREMENT 13.2: Detect non-standard values and normalize them to valid enum values
        var prompt = $@"You are a CSV import assistant. Analyze the following CSV sample data and provide a mapping from the CSV column headers to our internal task schema fields.

Internal Schema Fields:
- Title (required): Task title
- Description: Task description
- Status: One of: Pending, InProgress, Completed, Cancelled, Blocked
- Priority: One of: Critical, High, Medium, Low
- Category: One of: Development, Design, Marketing, Operations, Research, Other
- AssignedToEmail: Email address of assignee
- DueDate: Due date (ISO 8601 format)
- EstimatedHours: Estimated hours (decimal)
- Tags: Comma-separated tags

Sample CSV Data (first 3 rows):
{sampleJson}

Instructions:
1. Analyze the column headers and map them to internal schema fields
2. For each CSV column, identify which internal field it corresponds to
3. Provide normalization rules for enum values (Status, Priority, Category)
4. Return ONLY valid JSON, no additional text

Return JSON in this exact format:
{{
  ""columnMapping"": {{
    ""CSV_Column_Name"": ""InternalFieldName"",
    ""Another_CSV_Column"": ""AnotherInternalField""
  }},
  ""statusNormalization"": {{
    ""todo"": ""Pending"",
    ""in progress"": ""InProgress"",
    ""done"": ""Completed"",
    ""canceled"": ""Cancelled""
  }},
  ""priorityNormalization"": {{
    ""urgent"": ""Critical"",
    ""very high"": ""Critical"",
    ""high"": ""High"",
    ""normal"": ""Medium"",
    ""low"": ""Low""
  }},
  ""categoryNormalization"": {{
    ""dev"": ""Development"",
    ""development"": ""Development"",
    ""design"": ""Design"",
    ""marketing"": ""Marketing"",
    ""ops"": ""Operations"",
    ""operations"": ""Operations"",
    ""research"": ""Research""
  }}
}}";

        var response = await model.GenerateAsync(prompt);
        var jsonResponse = response.LastMessageContent ?? "{}";

        // Parse the JSON response into mapping configuration
        var mappingConfig = System.Text.Json.JsonSerializer.Deserialize<CsvMappingConfig>(
            jsonResponse,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new CsvMappingConfig();

        // Apply the mapping to all rows
        var normalizedRows = new List<TaskImportRow>();

        foreach (var rawRow in rawRows)
        {
            try
            {
                var normalizedRow = ApplyMapping(rawRow, mappingConfig);
                normalizedRows.Add(normalizedRow);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to normalize row. Skipping. Row data: {RowData}",
                    System.Text.Json.JsonSerializer.Serialize(rawRow));
            }
        }

        return normalizedRows;
    }

    /// <summary>
    /// Applies the AI-generated mapping configuration to a single raw CSV row.
    /// REQUIREMENT 13.2: Normalize enum values using AI-provided normalization rules
    /// </summary>
    private TaskImportRow ApplyMapping(Dictionary<string, string> rawRow, CsvMappingConfig config)
    {
        var row = new TaskImportRow();

        foreach (var kvp in rawRow)
        {
            var csvColumn = kvp.Key;
            var csvValue = kvp.Value;

            // Find the internal field name for this CSV column
            if (!config.ColumnMapping.TryGetValue(csvColumn, out var internalField))
            {
                // Try case-insensitive match
                var matchingKey = config.ColumnMapping.Keys
                    .FirstOrDefault(k => k.Equals(csvColumn, StringComparison.OrdinalIgnoreCase));
                
                if (matchingKey != null)
                {
                    internalField = config.ColumnMapping[matchingKey];
                }
                else
                {
                    continue; // Skip unmapped columns
                }
            }

            // Map the value to the appropriate field
            switch (internalField)
            {
                case "Title":
                    row.Title = csvValue;
                    break;

                case "Description":
                    row.Description = csvValue;
                    break;

                case "Status":
                    row.Status = NormalizeStatus(csvValue, config.StatusNormalization);
                    break;

                case "Priority":
                    row.Priority = NormalizePriority(csvValue, config.PriorityNormalization);
                    break;

                case "Category":
                    row.Category = NormalizeCategory(csvValue, config.CategoryNormalization);
                    break;

                case "AssignedToEmail":
                    row.AssignedToEmail = csvValue;
                    break;

                case "DueDate":
                    if (DateTime.TryParse(csvValue, out var dueDate))
                    {
                        row.DueDate = dueDate;
                    }
                    break;

                case "EstimatedHours":
                    if (decimal.TryParse(csvValue, out var estimatedHours))
                    {
                        row.EstimatedHours = estimatedHours;
                    }
                    break;

                case "Tags":
                    row.Tags = csvValue;
                    break;
            }
        }

        return row;
    }

    /// <summary>
    /// Normalizes a status value using AI-provided normalization rules.
    /// Falls back to direct enum parsing if no normalization rule matches.
    /// </summary>
    private TaskStatus NormalizeStatus(string value, Dictionary<string, string> normalizationRules)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return TaskStatus.Pending; // Default
        }

        var lowerValue = value.ToLowerInvariant();

        // Try normalization rules first
        if (normalizationRules.TryGetValue(lowerValue, out var normalizedValue))
        {
            if (Enum.TryParse<TaskStatus>(normalizedValue, true, out var status))
            {
                return status;
            }
        }

        // Try direct enum parsing
        if (Enum.TryParse<TaskStatus>(value, true, out var directStatus))
        {
            return directStatus;
        }

        // Default to Pending if unable to parse
        return TaskStatus.Pending;
    }

    /// <summary>
    /// Normalizes a priority value using AI-provided normalization rules.
    /// Falls back to direct enum parsing if no normalization rule matches.
    /// </summary>
    private TaskPriority NormalizePriority(string value, Dictionary<string, string> normalizationRules)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return TaskPriority.Medium; // Default
        }

        var lowerValue = value.ToLowerInvariant();

        // Try normalization rules first
        if (normalizationRules.TryGetValue(lowerValue, out var normalizedValue))
        {
            if (Enum.TryParse<TaskPriority>(normalizedValue, true, out var priority))
            {
                return priority;
            }
        }

        // Try direct enum parsing
        if (Enum.TryParse<TaskPriority>(value, true, out var directPriority))
        {
            return directPriority;
        }

        // Default to Medium if unable to parse
        return TaskPriority.Medium;
    }

    /// <summary>
    /// Normalizes a category value using AI-provided normalization rules.
    /// Falls back to direct enum parsing if no normalization rule matches.
    /// </summary>
    private TaskCategory NormalizeCategory(string value, Dictionary<string, string> normalizationRules)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return TaskCategory.Other; // Default
        }

        var lowerValue = value.ToLowerInvariant();

        // Try normalization rules first
        if (normalizationRules.TryGetValue(lowerValue, out var normalizedValue))
        {
            if (Enum.TryParse<TaskCategory>(normalizedValue, true, out var category))
            {
                return category;
            }
        }

        // Try direct enum parsing
        if (Enum.TryParse<TaskCategory>(value, true, out var directCategory))
        {
            return directCategory;
        }

        // Default to Other if unable to parse
        return TaskCategory.Other;
    }

    /// <summary>
    /// Logs AI interaction to the AiInteractionLog table for tracking and analytics.
    /// REQUIREMENT 13.7: Log import operations to AiInteractionLog with FeatureType.Import
    /// </summary>
    private async Task LogAiInteraction(
        Guid userId,
        int rawRowCount,
        int normalizedRowCount,
        int? tokensUsed,
        int latencyMs)
    {
        try
        {
            var log = new AiInteractionLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureType = AiFeatureType.Import,
                InputSummary = $"CSV import with {rawRowCount} rows",
                OutputSummary = $"Normalized {normalizedRowCount} rows successfully",
                TokensUsed = tokensUsed,
                LatencyMs = latencyMs,
                CreatedAt = DateTime.UtcNow
            };

            _context.AiInteractionLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Don't fail the main operation if logging fails
            _logger.LogError(ex, "Failed to log AI interaction for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Logs a failed AI interaction attempt.
    /// </summary>
    private async Task LogFailedAiInteraction(Guid userId, int latencyMs)
    {
        try
        {
            var log = new AiInteractionLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureType = AiFeatureType.Import,
                InputSummary = "CSV import attempt",
                OutputSummary = "Failed to normalize CSV import",
                TokensUsed = null,
                LatencyMs = latencyMs,
                CreatedAt = DateTime.UtcNow
            };

            _context.AiInteractionLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Don't fail the main operation if logging fails
            _logger.LogError(ex, "Failed to log failed AI interaction for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Gets the current user ID from the HTTP context claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            // For background jobs or system operations, use a system user ID
            return Guid.Empty;
        }

        return userId;
    }

    /// <summary>
    /// Internal class for deserializing AI-generated mapping configuration.
    /// </summary>
    private class CsvMappingConfig
    {
        public Dictionary<string, string> ColumnMapping { get; set; } = new();
        public Dictionary<string, string> StatusNormalization { get; set; } = new();
        public Dictionary<string, string> PriorityNormalization { get; set; } = new();
        public Dictionary<string, string> CategoryNormalization { get; set; } = new();
    }
}
