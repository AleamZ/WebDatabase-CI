using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace CIResearch.Services
{
    /// <summary>
    /// OPTIMIZED Background Data Service
    /// Handles heavy data processing without blocking main thread
    /// Perfect for 3-4 million record operations
    /// </summary>
    public class OptimizedDataService
    {
        private readonly ILogger<OptimizedDataService> _logger;
        private readonly string _connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;";
        private const int BATCH_SIZE = 50000; // Process in 50K batches
        private const int MAX_CONCURRENT_OPERATIONS = 3; // Limit concurrent operations

        public OptimizedDataService(ILogger<OptimizedDataService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get database health and performance metrics
        /// </summary>
        public async Task<DatabaseHealth> GetDatabaseHealthAsync()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var health = new DatabaseHealth();

                // Basic counts
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM dn_all", conn))
                {
                    health.TotalRecords = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                // Table size
                using (var cmd = new MySqlCommand(@"
                    SELECT 
                        ROUND(((data_length + index_length) / 1024 / 1024), 2) AS 'SizeMB'
                    FROM information_schema.tables 
                    WHERE table_schema = 'admin_ciresearch' AND table_name = 'dn_all'", conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    health.TableSizeMB = result != DBNull.Value ? Convert.ToDouble(result) : 0;
                }

                // Index usage
                using (var cmd = new MySqlCommand(@"
                    SHOW INDEX FROM dn_all WHERE Key_name != 'PRIMARY'", conn))
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var indexes = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        var keyName = reader["Key_name"]?.ToString();
                        if (!string.IsNullOrEmpty(keyName))
                        {
                            indexes.Add(keyName);
                        }
                    }
                    health.IndexCount = indexes.Count;
                    health.Indexes = indexes;
                }

                health.IsHealthy = true;
                health.CheckTime = DateTime.Now;

                _logger.LogInformation($"Database health check: {health.TotalRecords:N0} records, {health.TableSizeMB:N1} MB");
                return health;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return new DatabaseHealth
                {
                    IsHealthy = false,
                    Error = ex.Message,
                    CheckTime = DateTime.Now
                };
            }
        }

        /// <summary>
        /// Process data analysis in background batches
        /// </summary>
        public async Task<BatchProcessResult> ProcessDataInBatchesAsync(
            string operation,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            var result = new BatchProcessResult
            {
                Operation = operation,
                StartTime = DateTime.Now
            };

            try
            {
                _logger.LogInformation($"Starting batch operation: {operation}");

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken);

                // Get total count for progress tracking
                var totalCountQuery = BuildCountQuery(operation, parameters);
                using (var countCmd = new MySqlCommand(totalCountQuery, conn))
                {
                    AddParameters(countCmd, parameters);
                    result.TotalRecords = Convert.ToInt64(await countCmd.ExecuteScalarAsync(cancellationToken));
                }

                _logger.LogInformation($"Processing {result.TotalRecords:N0} records in batches of {BATCH_SIZE:N0}");

                // Process in batches
                long processed = 0;
                var batchNumber = 0;

                while (processed < result.TotalRecords && !cancellationToken.IsCancellationRequested)
                {
                    batchNumber++;
                    var batchStart = DateTime.Now;

                    var batchQuery = BuildBatchQuery(operation, parameters, processed, BATCH_SIZE);
                    using var batchCmd = new MySqlCommand(batchQuery, conn);
                    AddParameters(batchCmd, parameters);

                    var batchResult = await ProcessBatchAsync(batchCmd, operation, cancellationToken);

                    processed += batchResult.RecordsProcessed;
                    result.BatchesProcessed = batchNumber;
                    result.RecordsProcessed = processed;

                    var batchTime = DateTime.Now - batchStart;
                    var progress = (double)processed / result.TotalRecords * 100;

                    _logger.LogInformation($"Batch {batchNumber}: {batchResult.RecordsProcessed:N0} records, {batchTime.TotalSeconds:N1}s, {progress:N1}% complete");

                    // Add small delay to prevent overwhelming database
                    await Task.Delay(50, cancellationToken);
                }

                result.Success = !cancellationToken.IsCancellationRequested;
                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;

                _logger.LogInformation($"Batch operation completed: {result.RecordsProcessed:N0}/{result.TotalRecords:N0} records in {result.Duration.TotalSeconds:N1}s");

                return result;
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.Error = "Operation was cancelled";
                result.EndTime = DateTime.Now;
                _logger.LogWarning($"Batch operation cancelled: {operation}");
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.EndTime = DateTime.Now;
                _logger.LogError(ex, $"Batch operation failed: {operation}");
                return result;
            }
        }

        /// <summary>
        /// Optimize database performance by updating statistics
        /// </summary>
        public async Task<bool> OptimizeDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Starting database optimization...");

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // Analyze table for better query planning
                using (var cmd = new MySqlCommand("ANALYZE TABLE dn_all", conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // Check if we need to create recommended indexes
                await CreateRecommendedIndexesAsync(conn);

                _logger.LogInformation("Database optimization completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database optimization failed");
                return false;
            }
        }

        private async Task CreateRecommendedIndexesAsync(MySqlConnection conn)
        {
            var recommendedIndexes = new[]
            {
                "CREATE INDEX IF NOT EXISTS idx_nam_masothue ON dn_all(Nam, Masothue)",
                "CREATE INDEX IF NOT EXISTS idx_vungkinhte ON dn_all(Vungkinhte)",
                "CREATE INDEX IF NOT EXISTS idx_loaihinhkte ON dn_all(Loaihinhkte)",
                "CREATE INDEX IF NOT EXISTS idx_manh_dieutra ON dn_all(MaTinh_Dieutra)",
                "CREATE INDEX IF NOT EXISTS idx_revenue ON dn_all(SR_Doanhthu_Thuan_BH_CCDV)",
                "CREATE INDEX IF NOT EXISTS idx_ten_nganh ON dn_all(TEN_NGANH)"
            };

            foreach (var indexSql in recommendedIndexes)
            {
                try
                {
                    using var cmd = new MySqlCommand(indexSql, conn);
                    await cmd.ExecuteNonQueryAsync();
                    _logger.LogInformation($"Created index: {indexSql}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to create index: {indexSql}");
                }
            }
        }

        private string BuildCountQuery(string operation, Dictionary<string, object> parameters)
        {
            return operation switch
            {
                "export" => "SELECT COUNT(*) FROM dn_all WHERE Masothue IS NOT NULL",
                "analysis" => "SELECT COUNT(DISTINCT Masothue) FROM dn_all WHERE Masothue IS NOT NULL",
                _ => "SELECT COUNT(*) FROM dn_all"
            };
        }

        private string BuildBatchQuery(string operation, Dictionary<string, object> parameters, long offset, int batchSize)
        {
            return operation switch
            {
                "export" => $"SELECT * FROM dn_all WHERE Masothue IS NOT NULL ORDER BY STT LIMIT {batchSize} OFFSET {offset}",
                "analysis" => $"SELECT DISTINCT Masothue, TenDN FROM dn_all WHERE Masothue IS NOT NULL ORDER BY Masothue LIMIT {batchSize} OFFSET {offset}",
                _ => $"SELECT * FROM dn_all ORDER BY STT LIMIT {batchSize} OFFSET {offset}"
            };
        }

        private void AddParameters(MySqlCommand cmd, Dictionary<string, object> parameters)
        {
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue($"@{param.Key}", param.Value);
            }
        }

        private async Task<BatchResult> ProcessBatchAsync(MySqlCommand cmd, string operation, CancellationToken cancellationToken)
        {
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var count = 0;

            while (await reader.ReadAsync(cancellationToken))
            {
                count++;
                // Process each record as needed
                // For now, just count them
            }

            return new BatchResult
            {
                RecordsProcessed = count,
                Success = true
            };
        }

        #region Data Models

        public class DatabaseHealth
        {
            public bool IsHealthy { get; set; }
            public long TotalRecords { get; set; }
            public double TableSizeMB { get; set; }
            public int IndexCount { get; set; }
            public List<string> Indexes { get; set; } = new();
            public string? Error { get; set; }
            public DateTime CheckTime { get; set; }
        }

        public class BatchProcessResult
        {
            public string Operation { get; set; } = "";
            public bool Success { get; set; }
            public long TotalRecords { get; set; }
            public long RecordsProcessed { get; set; }
            public int BatchesProcessed { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public TimeSpan Duration { get; set; }
            public string? Error { get; set; }
        }

        public class BatchResult
        {
            public int RecordsProcessed { get; set; }
            public bool Success { get; set; }
            public string? Error { get; set; }
        }

        #endregion
    }
}