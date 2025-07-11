-- =====================================================
-- MIGRATION SCRIPT: OPTIMIZE DATABASE FOR 3-4 MILLION RECORDS
-- File: Scripts/migrate_to_optimized.sql
-- Purpose: Set up indexes and configurations for optimal performance
-- =====================================================

USE admin_ciresearch;

-- =====================================================
-- 1. BACKUP EXISTING INDEXES (Optional - for safety)
-- =====================================================
-- SHOW CREATE TABLE dn_all; -- Run this manually to backup current structure

-- =====================================================
-- 2. CREATE PERFORMANCE INDEXES
-- =====================================================
-- These indexes will dramatically improve query performance

-- Index for year-based filtering (most common)
CREATE INDEX IF NOT EXISTS idx_nam_masothue ON dn_all(Nam, Masothue);

-- Index for regional analysis
CREATE INDEX IF NOT EXISTS idx_vungkinhte ON dn_all(Vungkinhte);

-- Index for business type filtering
CREATE INDEX IF NOT EXISTS idx_loaihinhkte ON dn_all(Loaihinhkte);

-- Index for province-level analysis
CREATE INDEX IF NOT EXISTS idx_manh_dieutra ON dn_all(MaTinh_Dieutra);

-- Index for financial data queries
CREATE INDEX IF NOT EXISTS idx_revenue ON dn_all(SR_Doanhthu_Thuan_BH_CCDV);

-- Index for industry analysis
CREATE INDEX IF NOT EXISTS idx_ten_nganh ON dn_all(TEN_NGANH);

-- Composite index for complex filtering
CREATE INDEX IF NOT EXISTS idx_year_region_type ON dn_all(Nam, Vungkinhte, Loaihinhkte);

-- Index for pagination queries
CREATE INDEX IF NOT EXISTS idx_pagination ON dn_all(STT, Masothue);

-- Index for financial analysis
CREATE INDEX IF NOT EXISTS idx_financial ON dn_all(Nam, SR_Doanhthu_Thuan_BH_CCDV, SR_Loinhuan_TruocThue);

-- =====================================================
-- 3. OPTIMIZE EXISTING TABLE
-- =====================================================

-- Update table statistics for better query planning
ANALYZE TABLE dn_all;

-- Optimize table structure (removes fragmentation)
-- Note: This may take time on large tables
-- OPTIMIZE TABLE dn_all; -- Uncomment if needed

-- =====================================================
-- 4. CREATE SUMMARY TABLES (Optional - for extreme performance)
-- =====================================================

-- Create summary table for annual statistics
CREATE TABLE IF NOT EXISTS dn_annual_summary (
    Nam INT PRIMARY KEY,
    TotalCompanies INT,
    TotalLabor BIGINT,
    TotalRevenue DECIMAL(20,2),
    TotalProfit DECIMAL(20,2),
    TotalAssets DECIMAL(20,2),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create summary table for regional statistics
CREATE TABLE IF NOT EXISTS dn_regional_summary (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nam INT,
    Vungkinhte VARCHAR(255),
    CompanyCount INT,
    LaborCount BIGINT,
    TotalRevenue DECIMAL(20,2),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY unique_year_region (Nam, Vungkinhte)
);

-- =====================================================
-- 5. STORED PROCEDURES FOR OPTIMIZED QUERIES
-- =====================================================

-- Procedure to get annual summary efficiently
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS GetAnnualSummary(IN input_year INT)
BEGIN
    SELECT 
        COUNT(DISTINCT CASE WHEN Masothue IS NOT NULL AND Masothue != '' THEN Masothue END) as TotalCompanies,
        SUM(CASE WHEN SoLaodong_CuoiNam IS NOT NULL THEN SoLaodong_CuoiNam ELSE 0 END) as TotalLabor,
        SUM(CASE WHEN SR_Doanhthu_Thuan_BH_CCDV IS NOT NULL AND SR_Doanhthu_Thuan_BH_CCDV > 0 THEN SR_Doanhthu_Thuan_BH_CCDV ELSE 0 END) as TotalRevenue,
        SUM(CASE WHEN SR_Loinhuan_TruocThue IS NOT NULL THEN SR_Loinhuan_TruocThue ELSE 0 END) as TotalProfit,
        SUM(CASE WHEN Taisan_Tong_CK IS NOT NULL AND Taisan_Tong_CK > 0 THEN Taisan_Tong_CK ELSE 0 END) as TotalAssets
    FROM dn_all 
    WHERE Nam = input_year;
END //
DELIMITER ;

-- Procedure to get regional distribution efficiently
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS GetRegionalDistribution(IN input_year INT)
BEGIN
    SELECT 
        COALESCE(Vungkinhte, 'Unknown') as Region,
        COUNT(DISTINCT CASE WHEN Masothue IS NOT NULL AND Masothue != '' THEN Masothue END) as CompanyCount,
        SUM(CASE WHEN SoLaodong_CuoiNam IS NOT NULL THEN SoLaodong_CuoiNam ELSE 0 END) as LaborCount
    FROM dn_all 
    WHERE Nam = input_year 
      AND Vungkinhte IS NOT NULL 
      AND TRIM(Vungkinhte) != ''
    GROUP BY Vungkinhte
    ORDER BY CompanyCount DESC;
END //
DELIMITER ;

-- =====================================================
-- 6. DATABASE CONFIGURATION OPTIMIZATIONS
-- =====================================================

-- Note: These settings may require DBA privileges and server restart

-- Increase buffer pool size for better caching
-- SET GLOBAL innodb_buffer_pool_size = 2147483648; -- 2GB, adjust based on server RAM

-- Optimize for read-heavy workloads
-- SET GLOBAL innodb_read_io_threads = 8;

-- Increase connection limits if needed
-- SET GLOBAL max_connections = 500;

-- Optimize query cache (if using MySQL 5.7 or earlier)
-- SET GLOBAL query_cache_size = 268435456; -- 256MB

-- =====================================================
-- 7. VERIFY INDEX CREATION
-- =====================================================

-- Check that all indexes were created successfully
SELECT 
    TABLE_NAME,
    INDEX_NAME,
    COLUMN_NAME,
    CARDINALITY
FROM INFORMATION_SCHEMA.STATISTICS 
WHERE TABLE_SCHEMA = 'admin_ciresearch' 
  AND TABLE_NAME = 'dn_all'
  AND INDEX_NAME LIKE 'idx_%'
ORDER BY INDEX_NAME, SEQ_IN_INDEX;

-- =====================================================
-- 8. PERFORMANCE MONITORING QUERIES
-- =====================================================

-- Check table size and index usage
SELECT 
    TABLE_NAME,
    ROUND(((DATA_LENGTH + INDEX_LENGTH) / 1024 / 1024), 2) AS 'SizeMB',
    ROUND((INDEX_LENGTH / 1024 / 1024), 2) AS 'IndexSizeMB',
    TABLE_ROWS
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'admin_ciresearch' 
  AND TABLE_NAME = 'dn_all';

-- Check for slow queries (enable slow query log first)
-- SHOW VARIABLES LIKE 'slow_query_log';
-- SET GLOBAL slow_query_log = 'ON';
-- SET GLOBAL long_query_time = 2; -- Log queries taking more than 2 seconds

-- =====================================================
-- 9. TEST PERFORMANCE IMPROVEMENTS
-- =====================================================

-- Test query without indexes (comment out after testing)
-- EXPLAIN SELECT COUNT(DISTINCT Masothue) FROM dn_all WHERE Nam = 2023;

-- Test query with indexes
EXPLAIN SELECT COUNT(DISTINCT Masothue) FROM dn_all WHERE Nam = 2023;

-- Test regional grouping performance
EXPLAIN SELECT 
    Vungkinhte, 
    COUNT(DISTINCT Masothue) as CompanyCount 
FROM dn_all 
WHERE Nam = 2023 AND Vungkinhte IS NOT NULL 
GROUP BY Vungkinhte;

-- =====================================================
-- 10. CLEANUP AND MAINTENANCE
-- =====================================================

-- Create scheduled event for regular maintenance (optional)
DELIMITER //
CREATE EVENT IF NOT EXISTS optimize_dn_table
ON SCHEDULE EVERY 1 WEEK
STARTS CURRENT_TIMESTAMP
DO
BEGIN
    ANALYZE TABLE dn_all;
    -- OPTIMIZE TABLE dn_all; -- Uncomment if needed weekly
END //
DELIMITER ;

-- Enable event scheduler
-- SET GLOBAL event_scheduler = ON;

-- =====================================================
-- MIGRATION COMPLETE
-- =====================================================

SELECT 
    'MIGRATION COMPLETED SUCCESSFULLY!' AS Status,
    NOW() AS CompletedAt,
    COUNT(*) AS TotalRecords
FROM dn_all;

-- Show memory usage
SHOW STATUS LIKE 'Innodb_buffer_pool%';

-- Final message
SELECT 
    '✅ Database optimized for 3-4 million records!' AS Message,
    'Next step: Deploy OptimizedDNController.cs' AS NextAction; 