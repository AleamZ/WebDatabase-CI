using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using CIResearch.Models;
using System.Text;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace CIResearch.Controllers
{
    /// <summary>
    /// OPTIMIZED DN Controller for handling 3-4 million records
    /// Key optimizations:
    /// - Database-level aggregations instead of loading all data
    /// - Streaming data processing
    /// - Smart caching for summary data only
    /// - Chunked processing
    /// - Pagination at database level
    /// </summary>
    public class OptimizedDNController : Controller
    {
        private readonly IMemoryCache _cache;
        private string _connectionString = "Server=localhost;Database=sakila;User=root;Password=123456;";

        // Cache keys for lightweight summary data only
        private const string SUMMARY_CACHE_KEY = "dn_summary_optimized";
        private const string STATS_CACHE_KEY = "dn_stats_optimized";
        private const int CACHE_DURATION_MINUTES = 15; // Shorter cache for fresh data
        private const int CHUNK_SIZE = 10000; // Process in chunks of 10K records

        public OptimizedDNController(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// OPTIMIZED Index method - loads summary data first, detailed data on demand
        /// </summary>
        public async Task<IActionResult> Index(string stt = "",
            List<string>? Nam = null,
            List<string>? MaTinh_Dieutra = null,
            List<string>? Masothue = null,
            List<string>? Loaihinhkte = null,
            List<string>? Vungkinhte = null)
        {
            try
            {
                Console.WriteLine("🚀 OPTIMIZED CONTROLLER - Loading summary data first...");

                // Step 1: Test database connection
                var connectionTest = await TestDatabaseConnectionAsync();
                ViewBag.DatabaseConnected = connectionTest.IsConnected;
                ViewBag.DatabaseMessage = connectionTest.Message;
                ViewBag.DatabaseDetails = connectionTest.Details;

                if (!connectionTest.IsConnected)
                {
                    ViewBag.Error = "Không thể kết nối database";
                    InitializeEmptyViewBag();
                    return View();
                }

                // Step 2: Get lightweight summary statistics (cached)
                var summaryStats = await GetOptimizedSummaryStatsAsync(Nam, MaTinh_Dieutra, Loaihinhkte, Vungkinhte);
                AssignStatsToViewBag(summaryStats);

                // Step 3: Return view with summary data - detailed data loads via AJAX
                ViewBag.CurrentStt = stt;
                ViewBag.CurrentNam = Nam;
                ViewBag.CurrentMaTinh = MaTinh_Dieutra;
                ViewBag.CurrentMasothue = Masothue;
                ViewBag.CurrentLoaihinhkte = Loaihinhkte;
                ViewBag.CurrentVungkinhte = Vungkinhte;

                // For initial page load, show empty data - AJAX will load paginated data
                ViewBag.Data = new List<QLKH>();

                Console.WriteLine("✅ OPTIMIZED: Summary data loaded successfully");
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OPTIMIZED ERROR: {ex.Message}");
                ViewBag.DatabaseConnected = false;
                ViewBag.DatabaseMessage = "❌ Lỗi xử lý dữ liệu!";
                ViewBag.DatabaseDetails = $"Chi tiết lỗi: {ex.Message}";
                ViewBag.Error = "Không thể kết nối hoặc lấy dữ liệu từ database";
                ViewBag.Data = new List<QLKH>();
                InitializeEmptyViewBag();
                return View();
            }
        }

        #region OPTIMIZED Data Access - Database Aggregations

        /// <summary>
        /// Get optimized summary statistics using database aggregations
        /// NO MORE loading millions of records into memory!
        /// </summary>
        private async Task<OptimizedStats> GetOptimizedSummaryStatsAsync(
            List<string>? Nam = null,
            List<string>? MaTinh_Dieutra = null,
            List<string>? Loaihinhkte = null,
            List<string>? Vungkinhte = null)
        {
            // Check cache first
            string cacheKey = GenerateCacheKey(Nam, MaTinh_Dieutra, Loaihinhkte, Vungkinhte);
            if (_cache.TryGetValue(cacheKey, out OptimizedStats? cachedStats) && cachedStats != null)
            {
                Console.WriteLine("✅ Using cached optimized stats");
                return cachedStats;
            }

            Console.WriteLine("🔄 Calculating optimized stats from database...");

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var stats = new OptimizedStats();

            // Build WHERE clause for filters
            var whereClause = BuildWhereClause(Nam, MaTinh_Dieutra, Loaihinhkte, Vungkinhte);
            var parameters = BuildParameters(Nam, MaTinh_Dieutra, Loaihinhkte, Vungkinhte);

            // Get current analysis year
            stats.CurrentYear = await GetCurrentAnalysisYearFromDBAsync(conn, whereClause, parameters);

            // Apply year filter for current year analysis
            var yearWhereClause = whereClause.Length > 0 ? $"{whereClause} AND Nam = {stats.CurrentYear}" : $"WHERE Nam = {stats.CurrentYear}";

            // OPTIMIZATION 1: Get basic counts using database aggregation
            await GetBasicCountsAsync(conn, stats, yearWhereClause);

            // OPTIMIZATION 2: Get regional distribution using database GROUP BY
            await GetRegionalDistributionAsync(conn, stats, yearWhereClause);

            // OPTIMIZATION 3: Get business type distribution using database GROUP BY
            await GetBusinessTypeDistributionAsync(conn, stats, yearWhereClause);

            // OPTIMIZATION 4: Get industry distribution using database GROUP BY
            await GetIndustryDistributionAsync(conn, stats, yearWhereClause);

            // OPTIMIZATION 5: Get financial stats using database SUM/AVG functions
            await GetFinancialStatsAsync(conn, stats, yearWhereClause);

            // OPTIMIZATION 6: Get trend data using database aggregation
            await GetTrendDataAsync(conn, stats, whereClause, parameters);

            // Cache the results
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                .SetSize(1);
            _cache.Set(cacheKey, stats, cacheOptions);

            Console.WriteLine($"✅ Optimized stats calculated and cached");
            return stats;
        }

        /// <summary>
        /// Get basic counts using efficient database queries
        /// </summary>
        private async Task GetBasicCountsAsync(MySqlConnection conn, OptimizedStats stats, string whereClause)
        {
            var query = $@"
                SELECT 
                    COUNT(DISTINCT CASE WHEN Masothue IS NOT NULL AND Masothue != '' THEN Masothue END) as UniqueCompanies,
                    SUM(CASE WHEN SoLaodong_CuoiNam IS NOT NULL THEN SoLaodong_CuoiNam ELSE 0 END) as TotalLabor,
                    COUNT(*) as TotalRecords
                FROM dn_all 
                {whereClause}";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                stats.TotalCompanies = reader.IsDBNull("UniqueCompanies") ? 0 : reader.GetInt32("UniqueCompanies");
                var laborSum = reader.IsDBNull("TotalLabor") ? 0L : reader.GetInt64("TotalLabor");
                stats.TotalLabor = laborSum > int.MaxValue ? int.MaxValue : (int)laborSum;
                stats.TotalRecords = reader.GetInt32("TotalRecords");
            }

            Console.WriteLine($"📊 Basic counts: {stats.TotalCompanies} companies, {stats.TotalLabor:N0} labor");
        }

        /// <summary>
        /// Get regional distribution using database GROUP BY
        /// </summary>
        private async Task GetRegionalDistributionAsync(MySqlConnection conn, OptimizedStats stats, string whereClause)
        {
            var query = $@"
                SELECT 
                    COALESCE(Vungkinhte, 'Unknown') as Region,
                    COUNT(DISTINCT CASE WHEN Masothue IS NOT NULL AND Masothue != '' THEN Masothue END) as CompanyCount
                FROM dn_all 
                {whereClause}
                  AND Vungkinhte IS NOT NULL AND TRIM(Vungkinhte) != ''
                GROUP BY Vungkinhte
                ORDER BY CompanyCount DESC";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            stats.RegionData = new List<object>();
            var regionCounts = new Dictionary<string, int>();

            while (await reader.ReadAsync())
            {
                var region = reader.GetString("Region");
                var count = reader.GetInt32("CompanyCount");

                regionCounts[region] = count;
                stats.RegionData.Add(new { Region = region, SoLuong = count });
            }

            stats.RegionCounts = regionCounts;

            // Map to traditional 3 regions for ViewBag compatibility
            var dongBangSongHong = regionCounts.GetValueOrDefault("Đồng bằng Sông Hồng", 0);
            var trungDuMienNui = regionCounts.GetValueOrDefault("Trung du và Miền núi Bắc Bộ", 0);
            var bacTrungBo = regionCounts.GetValueOrDefault("Bắc Trung Bộ", 0);
            var duyenHaiNamTrungBo = regionCounts.GetValueOrDefault("Duyên hải Nam Trung Bộ", 0);
            var tayNguyen = regionCounts.GetValueOrDefault("Tây Nguyên", 0);
            var dongNamBo = regionCounts.GetValueOrDefault("Đông Nam Bộ", 0);
            var dongBangSongCuuLong = regionCounts.GetValueOrDefault("Đồng bằng Sông Cửu Long", 0);

            stats.MienBacCount = dongBangSongHong + trungDuMienNui;
            stats.MienTrungCount = bacTrungBo + duyenHaiNamTrungBo + tayNguyen;
            stats.MienNamCount = dongNamBo + dongBangSongCuuLong;

            Console.WriteLine($"📊 Regional: {stats.RegionData.Count} regions found");
        }

        /// <summary>
        /// Get business type distribution using database GROUP BY
        /// </summary>
        private async Task GetBusinessTypeDistributionAsync(MySqlConnection conn, OptimizedStats stats, string whereClause)
        {
            var query = $@"
                SELECT 
                    Loaihinhkte as BusinessType,
                    COUNT(DISTINCT CASE WHEN Masothue IS NOT NULL AND Masothue != '' THEN Masothue END) as CompanyCount
                FROM dn_all 
                {whereClause}
                  AND Loaihinhkte IS NOT NULL AND TRIM(Loaihinhkte) != ''
                GROUP BY Loaihinhkte
                ORDER BY CompanyCount DESC";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            stats.BusinessTypeData = new List<object>();
            stats.BusinessTypeCounts = new Dictionary<string, int>();

            while (await reader.ReadAsync())
            {
                var businessType = reader.GetString("BusinessType");
                var count = reader.GetInt32("CompanyCount");

                stats.BusinessTypeCounts[businessType] = count;
                stats.BusinessTypeData.Add(new { TinhTrang = businessType, SoLuong = count });
            }

            Console.WriteLine($"📊 Business types: {stats.BusinessTypeData.Count} types found");
        }

        /// <summary>
        /// Get industry distribution using database GROUP BY with LIMIT
        /// </summary>
        private async Task GetIndustryDistributionAsync(MySqlConnection conn, OptimizedStats stats, string whereClause)
        {
            var query = $@"
                SELECT 
                    TEN_NGANH as Industry,
                    COUNT(DISTINCT CASE WHEN Masothue IS NOT NULL AND Masothue != '' THEN Masothue END) as CompanyCount
                FROM dn_all 
                {whereClause}
                  AND TEN_NGANH IS NOT NULL AND TRIM(TEN_NGANH) != ''
                GROUP BY TEN_NGANH
                ORDER BY CompanyCount DESC
                LIMIT 20";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            stats.IndustryData = new List<object>();

            while (await reader.ReadAsync())
            {
                var industry = reader.GetString("Industry");
                var count = reader.GetInt32("CompanyCount");

                stats.IndustryData.Add(new { TEN_NGANH = industry, SoLuong = count });
            }

            Console.WriteLine($"📊 Industries: {stats.IndustryData.Count} industries found (top 20)");
        }

        #endregion

        #region Helper Methods

        private string BuildWhereClause(List<string>? Nam, List<string>? MaTinh_Dieutra,
            List<string>? Loaihinhkte, List<string>? Vungkinhte)
        {
            var conditions = new List<string>();

            if (Nam?.Any() == true)
            {
                var years = string.Join(",", Nam.Where(n => int.TryParse(n, out _)));
                if (!string.IsNullOrEmpty(years))
                    conditions.Add($"Nam IN ({years})");
            }

            if (MaTinh_Dieutra?.Any() == true)
            {
                var provinces = string.Join("','", MaTinh_Dieutra.Select(p => p.Replace("'", "''")));
                conditions.Add($"MaTinh_Dieutra IN ('{provinces}')");
            }

            if (Loaihinhkte?.Any() == true)
            {
                var types = string.Join("','", Loaihinhkte.Select(t => t.Replace("'", "''")));
                conditions.Add($"Loaihinhkte IN ('{types}')");
            }

            if (Vungkinhte?.Any() == true)
            {
                var regions = string.Join("','", Vungkinhte.Select(r => r.Replace("'", "''")));
                conditions.Add($"Vungkinhte IN ('{regions}')");
            }

            return conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
        }

        private Dictionary<string, object> BuildParameters(List<string>? Nam, List<string>? MaTinh_Dieutra,
            List<string>? Loaihinhkte, List<string>? Vungkinhte)
        {
            var parameters = new Dictionary<string, object>();

            if (Nam?.Any() == true) parameters["Nam"] = Nam;
            if (MaTinh_Dieutra?.Any() == true) parameters["MaTinh_Dieutra"] = MaTinh_Dieutra;
            if (Loaihinhkte?.Any() == true) parameters["Loaihinhkte"] = Loaihinhkte;
            if (Vungkinhte?.Any() == true) parameters["Vungkinhte"] = Vungkinhte;

            return parameters;
        }

        private string GenerateCacheKey(List<string>? Nam, List<string>? MaTinh_Dieutra,
            List<string>? Loaihinhkte, List<string>? Vungkinhte)
        {
            var keyParts = new List<string>
            {
                $"nam:{string.Join(",", Nam ?? new List<string>())}",
                $"tinh:{string.Join(",", MaTinh_Dieutra ?? new List<string>())}",
                $"loai:{string.Join(",", Loaihinhkte ?? new List<string>())}",
                $"vung:{string.Join(",", Vungkinhte ?? new List<string>())}"
            };

            return STATS_CACHE_KEY + "_" + string.Join("_", keyParts).GetHashCode();
        }

        private async Task<int> GetCurrentAnalysisYearFromDBAsync(MySqlConnection conn, string whereClause, Dictionary<string, object> parameters)
        {
            var yearQuery = $@"
                SELECT MAX(Nam) as MaxYear 
                FROM dn_all 
                {whereClause}
                  AND Nam IS NOT NULL AND Nam > 1990 AND Nam <= YEAR(NOW()) + 1";

            using var cmd = new MySqlCommand(yearQuery, conn);
            var result = await cmd.ExecuteScalarAsync();
            return result != DBNull.Value ? Convert.ToInt32(result) : DateTime.Now.Year;
        }

        #endregion

        #region Data Models

        public class OptimizedStats
        {
            public int TotalCompanies { get; set; }
            public int TotalLabor { get; set; }
            public int TotalRecords { get; set; }
            public int CurrentYear { get; set; }
            public Dictionary<string, int> RegionCounts { get; set; } = new();
            public Dictionary<string, int> BusinessTypeCounts { get; set; } = new();
            public Dictionary<string, decimal> FinancialStats { get; set; } = new();
            public List<object> RegionData { get; set; } = new();
            public List<object> BusinessTypeData { get; set; } = new();
            public List<object> IndustryData { get; set; } = new();
            public List<object> CompanySizeData { get; set; } = new();
            public List<int> Years { get; set; } = new();
            public List<double> RevenueData { get; set; } = new();
            public List<double> ProfitData { get; set; } = new();
            public int MienBacCount { get; set; }
            public int MienTrungCount { get; set; }
            public int MienNamCount { get; set; }
        }

        #endregion

        /// <summary>
        /// Get financial statistics using database aggregations
        /// </summary>
        private async Task GetFinancialStatsAsync(MySqlConnection conn, OptimizedStats stats, string whereClause)
        {
            var query = $@"
                SELECT 
                    COUNT(DISTINCT CASE WHEN SR_Doanhthu_Thuan_BH_CCDV IS NOT NULL AND SR_Doanhthu_Thuan_BH_CCDV > 0 AND Masothue IS NOT NULL THEN Masothue END) as CompaniesWithRevenue,
                    COUNT(DISTINCT CASE WHEN Taisan_Tong_CK IS NOT NULL AND Taisan_Tong_CK > 0 AND Masothue IS NOT NULL THEN Masothue END) as CompaniesWithAssets,
                    COUNT(DISTINCT CASE WHEN SR_Loinhuan_TruocThue IS NOT NULL AND Masothue IS NOT NULL THEN Masothue END) as CompaniesWithProfit,
                    SUM(CASE WHEN SR_Doanhthu_Thuan_BH_CCDV IS NOT NULL AND SR_Doanhthu_Thuan_BH_CCDV > 0 THEN SR_Doanhthu_Thuan_BH_CCDV ELSE 0 END) as TotalRevenue,
                    SUM(CASE WHEN Taisan_Tong_CK IS NOT NULL AND Taisan_Tong_CK > 0 THEN Taisan_Tong_CK ELSE 0 END) as TotalAssets,
                    SUM(CASE WHEN SR_Loinhuan_TruocThue IS NOT NULL THEN SR_Loinhuan_TruocThue ELSE 0 END) as TotalProfit
                FROM dn_all 
                {whereClause}";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                stats.FinancialStats["CompaniesWithRevenue"] = reader.IsDBNull("CompaniesWithRevenue") ? 0 : reader.GetInt32("CompaniesWithRevenue");
                stats.FinancialStats["CompaniesWithAssets"] = reader.IsDBNull("CompaniesWithAssets") ? 0 : reader.GetInt32("CompaniesWithAssets");
                stats.FinancialStats["CompaniesWithProfit"] = reader.IsDBNull("CompaniesWithProfit") ? 0 : reader.GetInt32("CompaniesWithProfit");
                stats.FinancialStats["TotalRevenue"] = reader.IsDBNull("TotalRevenue") ? 0 : reader.GetDecimal("TotalRevenue");
                stats.FinancialStats["TotalAssets"] = reader.IsDBNull("TotalAssets") ? 0 : reader.GetDecimal("TotalAssets");
                stats.FinancialStats["TotalProfit"] = reader.IsDBNull("TotalProfit") ? 0 : reader.GetDecimal("TotalProfit");

                // Calculate averages
                var companiesWithRevenue = (int)stats.FinancialStats["CompaniesWithRevenue"];
                var companiesWithAssets = (int)stats.FinancialStats["CompaniesWithAssets"];
                var companiesWithProfit = (int)stats.FinancialStats["CompaniesWithProfit"];

                stats.FinancialStats["AverageRevenue"] = companiesWithRevenue > 0 ? stats.FinancialStats["TotalRevenue"] / companiesWithRevenue : 0;
                stats.FinancialStats["AverageAssets"] = companiesWithAssets > 0 ? stats.FinancialStats["TotalAssets"] / companiesWithAssets : 0;
                stats.FinancialStats["AverageProfit"] = companiesWithProfit > 0 ? stats.FinancialStats["TotalProfit"] / companiesWithProfit : 0;

                // Duplicate for compatibility
                stats.FinancialStats["TotalAssetsCK"] = stats.FinancialStats["TotalAssets"];
                stats.FinancialStats["CompaniesWithAssetsCK"] = stats.FinancialStats["CompaniesWithAssets"];
            }

            Console.WriteLine($"📊 Financial stats calculated using database aggregation");
        }

        /// <summary>
        /// Get trend data using database GROUP BY year
        /// </summary>
        private async Task GetTrendDataAsync(MySqlConnection conn, OptimizedStats stats, string whereClause, Dictionary<string, object> parameters)
        {
            var query = $@"
                SELECT 
                    Nam as Year,
                    SUM(CASE WHEN SR_Doanhthu_Thuan_BH_CCDV IS NOT NULL THEN SR_Doanhthu_Thuan_BH_CCDV ELSE 0 END) as YearRevenue,
                    SUM(CASE WHEN SR_Loinhuan_TruocThue IS NOT NULL THEN SR_Loinhuan_TruocThue ELSE 0 END) as YearProfit
                FROM dn_all 
                {whereClause}
                  AND Nam IS NOT NULL AND Nam > 1990 AND Nam <= YEAR(NOW()) + 1
                  AND (SR_Doanhthu_Thuan_BH_CCDV IS NOT NULL OR SR_Loinhuan_TruocThue IS NOT NULL)
                GROUP BY Nam
                ORDER BY Nam";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            stats.Years = new List<int>();
            stats.RevenueData = new List<double>();
            stats.ProfitData = new List<double>();

            while (await reader.ReadAsync())
            {
                var year = reader.GetInt32("Year");
                var revenue = reader.IsDBNull("YearRevenue") ? 0.0 : Convert.ToDouble(reader.GetDecimal("YearRevenue"));
                var profit = reader.IsDBNull("YearProfit") ? 0.0 : Convert.ToDouble(reader.GetDecimal("YearProfit"));

                stats.Years.Add(year);
                stats.RevenueData.Add(revenue);
                stats.ProfitData.Add(profit);
            }

            Console.WriteLine($"📊 Trend data: {stats.Years.Count} years found");
        }

        /// <summary>
        /// Assign optimized stats to ViewBag
        /// </summary>
        private void AssignStatsToViewBag(OptimizedStats stats)
        {
            // Basic stats
            ViewBag.TotalCompanies = stats.TotalCompanies;
            ViewBag.TotalLabor = stats.TotalLabor;
            ViewBag.CurrentAnalysisYear = stats.CurrentYear;

            // Regional stats
            ViewBag.MienBacCount = stats.MienBacCount;
            ViewBag.MienTrungCount = stats.MienTrungCount;
            ViewBag.MienNamCount = stats.MienNamCount;
            ViewBag.RegionData = stats.RegionData;
            ViewBag.RegionCounts = stats.RegionCounts;

            // Business type stats
            ViewBag.BusinessTypeData = stats.BusinessTypeData;
            ViewBag.BusinessTypeCounts = stats.BusinessTypeCounts;

            // Top 3 business types
            var top3BusinessTypes = stats.BusinessTypeCounts
                .OrderByDescending(x => x.Value)
                .Take(3)
                .ToList();

            ViewBag.TopBusinessType1Name = top3BusinessTypes.Count > 0 ? ShortenBusinessTypeName(top3BusinessTypes[0].Key) : "N/A";
            ViewBag.TopBusinessType1Count = top3BusinessTypes.Count > 0 ? top3BusinessTypes[0].Value : 0;
            ViewBag.TopBusinessType2Name = top3BusinessTypes.Count > 1 ? ShortenBusinessTypeName(top3BusinessTypes[1].Key) : "N/A";
            ViewBag.TopBusinessType2Count = top3BusinessTypes.Count > 1 ? top3BusinessTypes[1].Value : 0;
            ViewBag.TopBusinessType3Name = top3BusinessTypes.Count > 2 ? ShortenBusinessTypeName(top3BusinessTypes[2].Key) : "N/A";
            ViewBag.TopBusinessType3Count = top3BusinessTypes.Count > 2 ? top3BusinessTypes[2].Value : 0;

            // Financial stats
            ViewBag.FinancialStats = stats.FinancialStats;
            ViewBag.CompaniesWithRevenue = (int)(stats.FinancialStats.GetValueOrDefault("CompaniesWithRevenue", 0));
            ViewBag.CompaniesWithAssets = (int)(stats.FinancialStats.GetValueOrDefault("CompaniesWithAssets", 0));
            ViewBag.CompaniesWithProfit = (int)(stats.FinancialStats.GetValueOrDefault("CompaniesWithProfit", 0));
            ViewBag.TotalAssetsCK = stats.FinancialStats.GetValueOrDefault("TotalAssetsCK", 0);
            ViewBag.CompaniesWithAssetsCK = (int)(stats.FinancialStats.GetValueOrDefault("CompaniesWithAssetsCK", 0));

            // Chart data
            ViewBag.IndustryData = stats.IndustryData;
            ViewBag.Years = stats.Years;
            ViewBag.RevenueData = stats.RevenueData;
            ViewBag.ProfitData = stats.ProfitData;

            // JSON data for charts
            ViewBag.RegionDataJson = JsonConvert.SerializeObject(stats.RegionData);
            ViewBag.BusinessTypeDataJson = JsonConvert.SerializeObject(stats.BusinessTypeData);
            ViewBag.IndustryDataJson = JsonConvert.SerializeObject(stats.IndustryData);
            ViewBag.RevenueDataJson = JsonConvert.SerializeObject(stats.RevenueData);
            ViewBag.ProfitDataJson = JsonConvert.SerializeObject(stats.ProfitData);

            // Technology stats (set to 0 for now)
            ViewBag.CoInternet = 0;
            ViewBag.CoWebsite = 0;
            ViewBag.CoPhanmem = 0;
            ViewBag.CoTudonghoa = 0;

            Console.WriteLine("✅ ViewBag assigned with optimized stats");
        }

        /// <summary>
        /// Initialize empty ViewBag for error cases
        /// </summary>
        private void InitializeEmptyViewBag()
        {
            ViewBag.TotalCompanies = 0;
            ViewBag.TotalLabor = 0;
            ViewBag.MienBacCount = 0;
            ViewBag.MienTrungCount = 0;
            ViewBag.MienNamCount = 0;
            ViewBag.CompaniesWithRevenue = 0;
            ViewBag.CompaniesWithAssets = 0;
            ViewBag.CompaniesWithProfit = 0;
            ViewBag.TotalAssetsCK = 0m;
            ViewBag.CompaniesWithAssetsCK = 0;
            ViewBag.TopBusinessType1Name = "N/A";
            ViewBag.TopBusinessType1Count = 0;
            ViewBag.TopBusinessType2Name = "N/A";
            ViewBag.TopBusinessType2Count = 0;
            ViewBag.TopBusinessType3Name = "N/A";
            ViewBag.TopBusinessType3Count = 0;
            ViewBag.RegionData = new List<object>();
            ViewBag.BusinessTypeData = new List<object>();
            ViewBag.IndustryData = new List<object>();
            ViewBag.Years = new List<int>();
            ViewBag.RevenueData = new List<double>();
            ViewBag.ProfitData = new List<double>();
            ViewBag.RegionDataJson = "[]";
            ViewBag.BusinessTypeDataJson = "[]";
            ViewBag.IndustryDataJson = "[]";
            ViewBag.RevenueDataJson = "[]";
            ViewBag.ProfitDataJson = "[]";
        }

        /// <summary>
        /// Test database connection
        /// </summary>
        private async Task<(bool IsConnected, string Message, string Details)> TestDatabaseConnectionAsync()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("SELECT COUNT(*) as RecordCount FROM dn_all", conn);
                var result = await cmd.ExecuteScalarAsync();
                var recordCount = Convert.ToInt64(result);

                return (true, "✅ Kết nối cơ sở dữ liệu thành công!",
                       $"Server: 127.0.0.1 | Database: sakila | Records: {recordCount:N0}");
            }
            catch (Exception ex)
            {
                return (false, "❌ Kết nối cơ sở dữ liệu thất bại!",
                       $"Lỗi: {ex.Message}");
            }
        }

        #region OPTIMIZED Pagination APIs

        /// <summary>
        /// OPTIMIZED: Get paginated data with server-side processing
        /// NO MORE loading millions of records into memory!
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetOptimizedPaginatedData()
        {
            try
            {
                var draw = int.Parse(Request.Form["draw"]);
                var start = int.Parse(Request.Form["start"]);
                var length = int.Parse(Request.Form["length"]);
                var searchValue = Request.Form["search[value]"];
                var sortColumn = int.Parse(Request.Form["order[0][column]"]);
                var sortDirection = Request.Form["order[0][dir]"];

                Console.WriteLine($"🚀 OPTIMIZED Pagination: start={start}, length={length}, search='{searchValue}'");

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // Build search conditions
                var searchCondition = "";
                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchCondition = @"
                        AND (TenDN LIKE @search 
                         OR Masothue LIKE @search 
                         OR MaTinh_Dieutra LIKE @search 
                         OR Loaihinhkte LIKE @search 
                         OR Diachi LIKE @search)";
                }

                // Get total count with search filter
                var countQuery = $@"
                    SELECT COUNT(*) 
                    FROM dn_all 
                    WHERE Masothue IS NOT NULL AND Masothue != '' 
                    {searchCondition}";

                int totalFiltered;
                using (var countCmd = new MySqlCommand(countQuery, conn))
                {
                    if (!string.IsNullOrEmpty(searchValue))
                        countCmd.Parameters.AddWithValue("@search", $"%{searchValue}%");
                    totalFiltered = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                }

                // Build sort column mapping
                var sortColumnName = sortColumn switch
                {
                    0 => "STT",
                    1 => "Nam",
                    2 => "Masothue",
                    3 => "TenDN",
                    4 => "Loaihinhkte",
                    5 => "MaTinh_Dieutra",
                    _ => "STT"
                };

                // Get paginated data
                var dataQuery = $@"
                    SELECT STT, Nam, Masothue, TenDN, Loaihinhkte, MaTinh_Dieutra, 
                           MaHuyen_Dieutra, MaXa_Dieutra, Diachi, Dienthoai, Email, Region
                    FROM dn_all 
                    WHERE Masothue IS NOT NULL AND Masothue != '' 
                    {searchCondition}
                    ORDER BY {sortColumnName} {sortDirection}
                    LIMIT @limit OFFSET @offset";

                var pagedData = new List<object>();
                using (var dataCmd = new MySqlCommand(dataQuery, conn))
                {
                    if (!string.IsNullOrEmpty(searchValue))
                        dataCmd.Parameters.AddWithValue("@search", $"%{searchValue}%");
                    dataCmd.Parameters.AddWithValue("@limit", length);
                    dataCmd.Parameters.AddWithValue("@offset", start);

                    using var reader = await dataCmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        pagedData.Add(new
                        {
                            STT = reader.IsDBNull("STT") ? 0 : reader.GetInt32("STT"),
                            Nam = reader.IsDBNull("Nam") ? (int?)null : reader.GetInt32("Nam"),
                            Masothue = reader.IsDBNull("Masothue") ? "N/A" : reader.GetString("Masothue"),
                            TenDN = reader.IsDBNull("TenDN") ? "N/A" : reader.GetString("TenDN"),
                            Loaihinhkte = reader.IsDBNull("Loaihinhkte") ? "N/A" : reader.GetString("Loaihinhkte"),
                            MaTinh_Dieutra = reader.IsDBNull("MaTinh_Dieutra") ? "N/A" : reader.GetString("MaTinh_Dieutra"),
                            MaHuyen_Dieutra = reader.IsDBNull("MaHuyen_Dieutra") ? "N/A" : reader.GetString("MaHuyen_Dieutra"),
                            MaXa_Dieutra = reader.IsDBNull("MaXa_Dieutra") ? "N/A" : reader.GetString("MaXa_Dieutra"),
                            Diachi = reader.IsDBNull("Diachi") ? "N/A" : reader.GetString("Diachi"),
                            Dienthoai = reader.IsDBNull("Dienthoai") ? "N/A" : reader.GetString("Dienthoai"),
                            Email = reader.IsDBNull("Email") ? "N/A" : reader.GetString("Email"),
                            Region = reader.IsDBNull("Region") ? "N/A" : reader.GetString("Region")
                        });
                    }
                }

                Console.WriteLine($"✅ OPTIMIZED: Returned {pagedData.Count} records from database directly");

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalFiltered, // Use filtered count for both
                    recordsFiltered = totalFiltered,
                    data = pagedData
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OPTIMIZED Pagination error: {ex.Message}");
                return Json(new
                {
                    draw = 0,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// OPTIMIZED: Get filter options directly from database
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOptimizedFilterOptions()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var filterOptions = new
                {
                    success = true,
                    message = "✅ Filter options loaded efficiently from database",

                    filters = new
                    {
                        years = await GetDistinctValuesAsync(conn, "Nam", "Nam IS NOT NULL AND Nam > 1990 AND Nam <= YEAR(NOW()) + 1", "Nam DESC"),
                        businessTypes = await GetDistinctValuesAsync(conn, "Loaihinhkte", "Loaihinhkte IS NOT NULL AND TRIM(Loaihinhkte) != ''"),
                        provinces = await GetDistinctValuesAsync(conn, "MaTinh_Dieutra", "MaTinh_Dieutra IS NOT NULL AND TRIM(MaTinh_Dieutra) != ''"),
                        economicZones = await GetDistinctValuesAsync(conn, "Vungkinhte", "Vungkinhte IS NOT NULL AND TRIM(Vungkinhte) != ''"),
                        regions = await GetDistinctValuesAsync(conn, "Region", "Region IS NOT NULL AND TRIM(Region) != ''")
                    },

                    timestamp = DateTime.Now
                };

                return Json(filterOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting optimized filter options: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to load filter options",
                    timestamp = DateTime.Now
                });
            }
        }

        private async Task<List<string>> GetDistinctValuesAsync(MySqlConnection conn, string column, string whereClause, string orderBy = null)
        {
            var query = $@"
                SELECT DISTINCT {column} as Value 
                FROM dn_all 
                WHERE {whereClause}
                {(orderBy != null ? $"ORDER BY {orderBy}" : $"ORDER BY {column}")}
                LIMIT 500"; // Limit to prevent too many options

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var values = new List<string>();
            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull("Value"))
                {
                    values.Add(reader.GetString("Value"));
                }
            }

            return values;
        }

        #endregion

        #region Helper Methods for Compatibility

        private static string ShortenBusinessTypeName(string businessTypeName)
        {
            if (string.IsNullOrEmpty(businessTypeName))
                return "N/A";

            var shortenedNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"Công ty cổ phần có vốn Nhà nước <= 50%", "CP vốn NN ≤50%"},
                {"Công ty cổ phần không có vốn Nhà nước", "CP không vốn NN"},
                {"Công ty cổ phần", "Cổ phần"},
                {"Công ty trách nhiệm hữu hạn một thành viên", "TNHH 1TV"},
                {"Công ty trách nhiệm hữu hạn hai thành viên trở lên", "TNHH 2TV+"},
                {"Công ty TNHH một thành viên", "TNHH 1TV"},
                {"Công ty TNHH hai thành viên trở lên", "TNHH 2TV+"},
                {"Công ty TNHH", "TNHH"},
                {"Doanh nghiệp tư nhân", "DN tư nhân"},
                {"Hộ kinh doanh cá thể", "Hộ KD cá thể"}
            };

            if (shortenedNames.TryGetValue(businessTypeName, out string shortName))
                return shortName;

            if (businessTypeName.Length > 20)
            {
                var shortened = businessTypeName
                    .Replace("Công ty ", "")
                    .Replace("Doanh nghiệp ", "DN ")
                    .Replace("trách nhiệm hữu hạn", "TNHH")
                    .Replace("cổ phần", "CP");

                if (shortened.Length > 20)
                    shortened = shortened.Substring(0, 17) + "...";

                return shortened;
            }

            return businessTypeName;
        }

        #endregion
    }
}