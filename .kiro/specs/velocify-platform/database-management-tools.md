# Database Management Tools for Velocify Platform

## Overview
This document provides information about database management tools for the Velocify platform's Azure SQL Database.

## Recommended Tools

### 1. Azure Data Studio (Recommended)
**Download:** https://aka.ms/azuredatastudio

**Why Azure Data Studio:**
- Cross-platform (Windows, macOS, Linux)
- Modern, lightweight interface
- Built-in support for Azure SQL Database
- Integrated Jupyter notebooks for documentation
- Git integration
- Extensions ecosystem
- Better performance than SSMS for cloud databases
- Free and open-source

**Key Features:**
- IntelliSense for T-SQL
- Query execution and results visualization
- Database schema explorer
- Integrated terminal
- Source control integration
- Extensions for PostgreSQL, MySQL, and more

### 2. SQL Server Management Studio (SSMS)
**Download:** https://aka.ms/ssmsfullsetup

**Why SSMS:**
- Full-featured database management
- Advanced debugging capabilities
- Comprehensive administration tools
- Execution plan analysis
- Database diagram designer
- Free from Microsoft

**Key Features:**
- Complete T-SQL editor with IntelliSense
- Object Explorer for database navigation
- Query execution plans and performance tuning
- Database backup and restore
- User and permission management
- SQL Server Agent for job scheduling

## Installation Instructions

### Azure Data Studio
(Its retired as of Feb, 2026. Replaced by VS Code)
1. Download from https://aka.ms/azuredatastudio
2. Run the installer
3. Launch Azure Data Studio
4. Click "New Connection"
5. Enter Azure SQL Database connection details:
   - Server: `<your-server>.database.windows.net`
   - Authentication: SQL Login or Azure Active Directory
   - Database: `velocify-db`

### SQL Server Management Studio
1. Download from https://aka.ms/ssmsfullsetup
2. Run the installer (requires ~1GB disk space)
3. Launch SSMS
4. Connect to Azure SQL Database:
   - Server name: `<your-server>.database.windows.net`
   - Authentication: SQL Server Authentication or Azure Active Directory
   - Login: Your SQL username
   - Password: Your SQL password

## Connection String Format
```
Server=tcp:<your-server>.database.windows.net,1433;Initial Catalog=velocify-db;Persist Security Info=False;User ID=<username>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Common Tasks

### View Tables and Data
- Azure Data Studio: Use the Connections panel → Expand database → Tables
- SSMS: Object Explorer → Databases → velocify-db → Tables

### Execute Migrations
```bash
# From backend directory
dotnet ef database update
```

### Run Stored Procedures
```sql
-- Recalculate productivity scores
EXEC usp_RecalculateUserProductivityScores;
```

### Query Indexed View
```sql
-- Dashboard summary
SELECT * FROM vw_UserTaskSummary WHERE UserId = '<user-guid>';
```

### Check Partition Information
```sql
-- View partition details for TaskAuditLog
SELECT 
    p.partition_number,
    p.rows,
    rv.value AS boundary_value
FROM sys.partitions p
JOIN sys.partition_schemes ps ON p.partition_number = ps.data_space_id
JOIN sys.partition_range_values rv ON ps.function_id = rv.function_id
WHERE p.object_id = OBJECT_ID('TaskAuditLog');
```

## Next Steps
1. Choose and install your preferred tool (Azure Data Studio recommended for cloud-first development)
2. Configure connection to Azure SQL Database
3. Test connection and explore the schema
4. Bookmark this document for reference

## Related Requirements
- Requirement 30.4: Code quality and infrastructure setup
- Requirement 30.5: Comprehensive documentation
