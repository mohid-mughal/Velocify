using Velocify.Application.DTOs.Import;

namespace Velocify.Application.Interfaces;

/// <summary>
/// Service for AI-powered CSV import with intelligent column mapping and normalization.
/// Requirements: 13.1-13.7
/// </summary>
public interface IAiImportService
{
    /// <summary>
    /// Analyzes CSV headers and maps them to internal schema fields.
    /// Normalizes non-standard values to valid enum values.
    /// Returns normalized list for user review before import.
    /// Logs to AiInteractionLog with FeatureType.Import.
    /// </summary>
    /// <param name="csvData">Raw CSV data as string</param>
    /// <returns>List of normalized task import rows ready for review</returns>
    Task<List<TaskImportRow>> NormalizeImport(string csvData);
}
