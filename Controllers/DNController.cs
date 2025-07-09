using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;
using CIResearch.Models;
using System.Text;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CIResearch.Controllers
{
    /// <summary>
    /// Optimized DN Controller with:
    /// - Memory caching to reduce database calls
    /// - Async/await for better performance
    /// - LINQ optimizations for data processing
    /// - Reduced memory allocations
    /// - Service pattern for better code organization
    /// </summary>
    public class DNController : Controller
    {
        private readonly IMemoryCache _cache;
        private string _connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;";
        private const string DATA_CACHE_KEY = "dn_all";
        private const string SUMMARY_CACHE_KEY = "dn_summary";
        private const int CACHE_DURATION_MINUTES = 30;
        private const int SUMMARY_CACHE_DURATION_MINUTES = 60; // Summary data can be cached longer

        public DNController(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Test database connection and return status
        /// </summary>
        private async Task<(bool IsConnected, string Message, string Details)> TestDatabaseConnectionAsync()
        {
            try
            {
                Console.WriteLine("🔍 Testing database connection...");

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // Test basic query
                using var cmd = new MySqlCommand("SELECT COUNT(*) as RecordCount FROM dn_all", conn);
                var result = await cmd.ExecuteScalarAsync();
                var recordCount = Convert.ToInt64(result);

                Console.WriteLine($"✅ Database connection successful! Found {recordCount:N0} records");

                return (true, "✅ Kết nối cơ sở dữ liệu thành công!",
                       $"Server: 127.0.0.1 | Database: admin_ciresearch | Records: {recordCount:N0}");
            }
            catch (MySqlException mysqlEx)
            {
                Console.WriteLine($"❌ MySQL Error: {mysqlEx.Message}");
                return (false, "❌ Lỗi kết nối MySQL!",
                       $"Mã lỗi: {mysqlEx.Number} | Chi tiết: {mysqlEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection failed: {ex.Message}");
                return (false, "❌ Kết nối cơ sở dữ liệu thất bại!",
                       $"Lỗi: {ex.Message}");
            }
        }

        public async Task<IActionResult> Index(string stt = "",
            List<string>? Nam = null,
            List<string>? MaTinh_Dieutra = null,
            List<string>? Masothue = null,
            List<string>? Loaihinhkte = null,
            List<string>? Vungkinhte = null)
        {
            // Test database connection first
            var connectionTest = await TestDatabaseConnectionAsync();
            ViewBag.DatabaseConnected = connectionTest.IsConnected;
            ViewBag.DatabaseMessage = connectionTest.Message;
            ViewBag.DatabaseDetails = connectionTest.Details;

            try
            {
                // Force clear all caches to load ALL data from database
                _cache.Remove(DATA_CACHE_KEY);
                _cache.Remove(SUMMARY_CACHE_KEY);
                Console.WriteLine("🔄 All caches cleared, loading ALL fresh data from database...");

                var allData = await GetCachedDataAsync();
                Console.WriteLine($"📊 Loaded {allData.Count:N0} records from database (ALL DATA - NO LIMITS)");

                var filteredData = ApplyFiltersOptimized(allData, stt, Nam, MaTinh_Dieutra, Masothue, Loaihinhkte, Vungkinhte);
                Console.WriteLine($"🔍 Filtered to {filteredData.Count} records");

                var stats = CalculateAllStatistics(allData, Nam);
                ViewBag.Data = filteredData;
                AssignStatsToViewBag(stats);
                ViewBag.CurrentStt = stt;
                ViewBag.CurrentNam = Nam;
                ViewBag.CurrentMaTinh = MaTinh_Dieutra;
                ViewBag.CurrentMasothue = Masothue;
                ViewBag.CurrentLoaihinhkte = Loaihinhkte;
                ViewBag.CurrentVungkinhte = Vungkinhte;

                Console.WriteLine("✅ Data processing completed successfully");
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");

                // Update connection status to show specific error
                ViewBag.DatabaseConnected = false;
                ViewBag.DatabaseMessage = "❌ Lỗi xử lý dữ liệu!";
                ViewBag.DatabaseDetails = $"Chi tiết lỗi: {ex.Message}";

                ViewBag.Error = "Không thể kết nối hoặc lấy dữ liệu từ database. Vui lòng kiểm tra lại kết nối hoặc dữ liệu.";
                ViewBag.Data = new List<QLKH>();

                // Initialize ALL ViewBag properties with safe defaults when database fails
                InitializeEmptyViewBag();

                return View();
            }
        }

        #region Optimized Data Access

        private async Task<List<QLKH>> GetCachedDataAsync()
        {
            if (!_cache.TryGetValue(DATA_CACHE_KEY, out List<QLKH>? data) || data == null)
            {
                data = await GetDataFromDatabaseAsync();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                    .SetSize(1);
                _cache.Set(DATA_CACHE_KEY, data, cacheOptions);
            }
            return data;
        }

        private async Task<List<QLKH>> GetDataFromDatabaseAsync()
        {
            var data = new List<QLKH>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // 🚀 FIXED: Remove LIMIT to load ALL data from database
                // Loading all imported data as requested by user
                string query = @"
                    SELECT STT, TenDN, Diachi, MaTinh_Dieutra, MaHuyen_Dieutra, MaXa_Dieutra,
                           DNTB_MaTinh, DNTB_MaHuyen, DNTB_MaXa, Region, Loaihinhkte, 
                           Nam, Masothue, Vungkinhte, QUY_MO, MaNganhC5_Chinh, TEN_NGANH,
                           SR_Doanhthu_Thuan_BH_CCDV, SR_Loinhuan_TruocThue, 
                           SoLaodong_DauNam, SoLaodong_CuoiNam, Taisan_Tong_CK, Taisan_Tong_DK,
                           Email, Dienthoai
                    FROM dn_all 
                    ORDER BY STT";  // 🚀 FIXED: No LIMIT - Load ALL data

                Console.WriteLine($"🔍 LOADING ALL DATA FROM DATABASE - No limit applied");
                Console.WriteLine($"🔍 Query: {query}");

                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        Console.WriteLine($"🔍 Available columns: {reader.FieldCount}");

                        while (await reader.ReadAsync())
                        {
                            var record = CreateQLKHFromReader(reader);
                            data.Add(record);

                            // Progress logging every 100k records
                            if (data.Count % 100000 == 0)
                            {
                                Console.WriteLine($"📊 Loaded {data.Count:N0} records...");
                            }

                            // Debug first few records
                            if (data.Count <= 3)
                            {
                                Console.WriteLine($"🔍 Record {data.Count} - TEN_NGANH: '{record.TEN_NGANH ?? "NULL"}', TenDN: '{record.TenDN}'");
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"✅ SUCCESSFULLY LOADED {data.Count:N0} RECORDS FROM DATABASE - ALL DATA LOADED!");
            Console.WriteLine($"✅ This is REAL data from dn_all table with NO artificial limits");

            // Count records with TEN_NGANH
            var withIndustry = data.Count(x => !string.IsNullOrEmpty(x.TEN_NGANH));
            Console.WriteLine($"🔍 Records with TEN_NGANH: {withIndustry:N0}/{data.Count:N0}");

            return data;
        }

        private static QLKH CreateQLKHFromReader(System.Data.Common.DbDataReader reader)
        {
            return new QLKH
            {
                STT = GetSafeNullableInt(reader, "STT") ?? 0,
                TenDN = GetSafeString(reader, "TenDN"),
                Diachi = GetSafeString(reader, "Diachi"),
                MaTinh_Dieutra = GetSafeString(reader, "MaTinh_Dieutra"),
                MaHuyen_Dieutra = GetSafeString(reader, "MaHuyen_Dieutra"),
                MaXa_Dieutra = GetSafeString(reader, "MaXa_Dieutra"),
                DNTB_MaTinh = GetSafeString(reader, "DNTB_MaTinh"),
                DNTB_MaHuyen = GetSafeString(reader, "DNTB_MaHuyen"),
                DNTB_MaXa = GetSafeString(reader, "DNTB_MaXa"),
                Region = GetSafeString(reader, "Region"),
                Loaihinhkte = GetSafeString(reader, "Loaihinhkte"),
                Email = GetSafeString(reader, "Email"),
                Dienthoai = GetSafeString(reader, "Dienthoai"),
                Nam = GetSafeNullableInt(reader, "Nam"),
                Masothue = GetSafeString(reader, "Masothue"),
                Vungkinhte = GetSafeString(reader, "Vungkinhte"),
                QUY_MO = GetSafeString(reader, "QUY_MO"),
                MaNganhC5_Chinh = GetSafeString(reader, "MaNganhC5_Chinh"),
                TEN_NGANH = GetSafeString(reader, "TEN_NGANH"),
                SR_Doanhthu_Thuan_BH_CCDV = GetSafeNullableDecimal(reader, "SR_Doanhthu_Thuan_BH_CCDV"),
                SR_Loinhuan_TruocThue = GetSafeNullableDecimal(reader, "SR_Loinhuan_TruocThue"),
                SoLaodong_DauNam = GetSafeNullableInt(reader, "SoLaodong_DauNam"),
                SoLaodong_CuoiNam = GetSafeNullableInt(reader, "SoLaodong_CuoiNam"),
                Taisan_Tong_CK = GetSafeNullableDecimal(reader, "Taisan_Tong_CK"),
                Taisan_Tong_DK = GetSafeNullableDecimal(reader, "Taisan_Tong_DK")
            };
        }

        private static string GetSafeString(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
            }
            catch
            {
                return "";
            }
        }

        private static int? GetSafeNullableInt(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
            }
            catch
            {
                return null;
            }
        }

        private static decimal? GetSafeNullableDecimal(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                {
                    // Debug: Log when we encounter NULL values
                    if (columnName == "SR_Doanhthu_Thuan_BH_CCDV")
                    {
                        var stt = reader.IsDBNull("STT") ? "N/A" : reader.GetInt32("STT").ToString();
                        var tenDN = reader.IsDBNull("TenDN") ? "N/A" : reader.GetString("TenDN");
                        Console.WriteLine($"🚨 DEBUG: Found NULL SR_Doanhthu_Thuan_BH_CCDV for STT={stt}, TenDN='{tenDN}'");
                    }
                    return null;
                }
                return reader.GetDecimal(ordinal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🚨 DEBUG: Error reading {columnName}: {ex.Message}");
                return null;
            }
        }

        private async Task<DateTime> GetLastImportTimeAsync()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                using var cmd = new MySqlCommand("SELECT MAX(import_time) FROM import_history", conn);
                var result = await cmd.ExecuteScalarAsync();
                return result != DBNull.Value ? (DateTime)result : DateTime.Now;
            }
            catch
            {
                return DateTime.Now;
            }
        }

        #endregion

        #region Optimized Filtering

        private static List<QLKH> ApplyFiltersOptimized(List<QLKH> data, string stt,
            List<string>? Nam, List<string>? MaTinh_Dieutra, List<string>? Masothue,
            List<string>? Loaihinhkte, List<string>? Vungkinhte)
        {
            var query = data.AsEnumerable();

            if (!string.IsNullOrEmpty(stt) && int.TryParse(stt, out int sttValue))
                query = query.Where(x => x.STT == sttValue);

            if (Nam?.Any() == true)
                query = query.Where(x => x.Nam.HasValue && Nam.Contains(x.Nam.Value.ToString()));

            if (MaTinh_Dieutra?.Any() == true)
                query = query.Where(x => !string.IsNullOrEmpty(x.MaTinh_Dieutra) && MaTinh_Dieutra.Contains(x.MaTinh_Dieutra));

            if (Masothue?.Any() == true)
                query = query.Where(x => !string.IsNullOrEmpty(x.Masothue) && Masothue.Contains(x.Masothue));

            if (Loaihinhkte?.Any() == true)
                query = query.Where(x => !string.IsNullOrEmpty(x.Loaihinhkte) && Loaihinhkte.Contains(x.Loaihinhkte));

            if (Vungkinhte?.Any() == true)
                query = query.Where(x => !string.IsNullOrEmpty(x.Vungkinhte) && Vungkinhte.Contains(x.Vungkinhte));

            return query.ToList();
        }

        #endregion

        #region Optimized Statistics Calculation

        private class ComprehensiveStats
        {
            public int TotalCompanies { get; set; }
            public int TotalLabor { get; set; }
            public Dictionary<string, int> RegionCounts { get; set; } = new();
            public Dictionary<string, int> BusinessTypeCounts { get; set; } = new();
            public Dictionary<string, decimal> FinancialStats { get; set; } = new();
            public List<object> ProvinceData { get; set; } = new();
            public List<object> RegionData { get; set; } = new();
            public List<object> BusinessTypeData { get; set; } = new();
            public List<object> CompanySizeData { get; set; } = new();
            public List<object> IndustryData { get; set; } = new();
            public List<int> Years { get; set; } = new();
            public List<double> RevenueData { get; set; } = new();
            public List<double> ProfitData { get; set; } = new();
        }

        private int GetLatestYear(List<QLKH> data)
        {
            return data.Where(x => x.Nam.HasValue).Max(x => x.Nam.Value);
        }

        private int GetEarliestYear(List<QLKH> data)
        {
            return data.Min(x => x.Nam ?? 9999);
        }

        private List<QLKH> FilterDataByYear(List<QLKH> data, int year)
        {
            return data.Where(x => x.Nam == year).ToList();
        }

        private int GetCurrentAnalysisYear(List<QLKH> data, List<string>? namFilter)
        {
            // If user selected specific years, use the latest one from selection
            if (namFilter?.Any() == true)
            {
                var selectedYears = namFilter.Where(x => int.TryParse(x, out _)).Select(int.Parse);
                if (selectedYears.Any())
                {
                    return selectedYears.Max();
                }
            }

            // Otherwise use the latest year available in data
            return GetLatestYear(data);
        }

        private ComprehensiveStats CalculateAllStatistics(List<QLKH> data, List<string>? namFilter = null)
        {
            var stats = new ComprehensiveStats();

            // Get current analysis year (latest year or user-selected year)
            int currentYear = GetCurrentAnalysisYear(data, namFilter);
            Console.WriteLine($"\n🔍 ANALYSIS FOR SPECIFIC YEAR: {currentYear}");
            Console.WriteLine($"📊 Total records in database: {data.Count}");

            // Filter data for the current analysis year
            var currentYearData = FilterDataByYear(data, currentYear);
            Console.WriteLine($"📊 Records for year {currentYear}: {currentYearData.Count}");

            // Group companies by their unique tax code (Masothue) in the current year only
            var uniqueCompaniesInYear = currentYearData
                .Where(x => !string.IsNullOrEmpty(x.Masothue)) // Only count companies with tax code
                .GroupBy(x => x.Masothue)
                .Select(g => new
                {
                    Masothue = g.Key,
                    Record = g.First(), // Take any record since they're all from the same year
                    RecordCount = g.Count()
                })
                .ToList();

            // Count unique companies based on unique tax codes in current year
            stats.TotalCompanies = uniqueCompaniesInYear.Count;

            Console.WriteLine($"🔍 UNIQUE COMPANIES COUNT FOR YEAR {currentYear}:");
            Console.WriteLine($"   - Total records in year {currentYear}: {currentYearData.Count}");
            Console.WriteLine($"   - Records with Masothue: {currentYearData.Count(x => !string.IsNullOrEmpty(x.Masothue))}");
            Console.WriteLine($"   - Unique companies (by Masothue): {stats.TotalCompanies}");
            Console.WriteLine($"   - Duplicate records in year: {currentYearData.Count(x => !string.IsNullOrEmpty(x.Masothue)) - stats.TotalCompanies}");

            // Show sample of duplicate detection in current year
            var duplicateExamples = uniqueCompaniesInYear
                .Where(x => x.RecordCount > 1)
                .Take(5);

            if (duplicateExamples.Any())
            {
                Console.WriteLine($"\n🔍 SAMPLE DUPLICATE COMPANIES IN YEAR {currentYear} (same Masothue):");
                foreach (var example in duplicateExamples)
                {
                    Console.WriteLine($"   - Masothue: {example.Masothue}");
                    Console.WriteLine($"     Company: {example.Record.TenDN}");
                    Console.WriteLine($"     Duplicate records in year: {example.RecordCount}");
                }
            }

            // Use unique companies from current year for all statistics
            var uniqueCompanies = uniqueCompaniesInYear.Select(x => x.Record).ToList();

            // Store current year info for ViewBag
            ViewBag.CurrentAnalysisYear = currentYear;
            ViewBag.AvailableYears = data.Where(x => x.Nam.HasValue).Select(x => x.Nam.Value).Distinct().OrderByDescending(x => x).ToList();

            Console.WriteLine($"🔍 VIEWBAG YEAR ASSIGNMENT:");
            Console.WriteLine($"   - ViewBag.CurrentAnalysisYear: {ViewBag.CurrentAnalysisYear}");
            Console.WriteLine($"   - ViewBag.AvailableYears: [{string.Join(", ", ViewBag.AvailableYears)}]");

            Console.WriteLine($"🔍 UNIQUE COMPANIES COUNT:");
            Console.WriteLine($"   - Total records: {data.Count}");
            Console.WriteLine($"   - Unique companies: {stats.TotalCompanies}");
            Console.WriteLine($"   - Duplicates removed: {data.Count - stats.TotalCompanies}");

            // FIXED: Calculate labor count for current year only using unique companies
            try
            {
                // Use unique companies from current year for labor calculation
                var laborSum = uniqueCompanies.Sum(x => (long)(x.SoLaodong_CuoiNam ?? 0));
                stats.TotalLabor = laborSum > int.MaxValue ? int.MaxValue : (int)laborSum;

                Console.WriteLine($"🔍 LABOR COUNT FOR YEAR {currentYear} (using unique companies only):");
                Console.WriteLine($"   - Unique companies in year: {uniqueCompanies.Count}");
                Console.WriteLine($"   - Companies with labor data: {uniqueCompanies.Count(x => x.SoLaodong_CuoiNam.HasValue)}");
                Console.WriteLine($"   - Total labor count: {stats.TotalLabor:N0}");

                // Sample companies with labor data
                var sampleWithLabor = uniqueCompanies
                    .Where(x => x.SoLaodong_CuoiNam.HasValue && x.SoLaodong_CuoiNam.Value > 0)
                    .Take(3)
                    .ToList();

                if (sampleWithLabor.Any())
                {
                    Console.WriteLine($"🔍 SAMPLE COMPANIES WITH LABOR DATA IN YEAR {currentYear}:");
                    foreach (var company in sampleWithLabor)
                    {
                        Console.WriteLine($"   - {company.TenDN}: {company.SoLaodong_CuoiNam:N0} lao động");
                        Console.WriteLine($"     Mã số thuế: {company.Masothue}");
                    }
                }
            }
            catch (OverflowException)
            {
                Console.WriteLine($"⚠️ Labor sum overflow for year {currentYear}, using count of companies with labor data");
                stats.TotalLabor = uniqueCompanies.Count(x => x.SoLaodong_CuoiNam.HasValue);
            }

            // Debug: Log data details
            Console.WriteLine($"🔍 DEBUGGING - Total unique companies: {stats.TotalCompanies}");
            Console.WriteLine($"🔍 Sample unique companies:");
            var sampleCompanies = uniqueCompanies.Take(3);
            foreach (var company in sampleCompanies)
            {
                Console.WriteLine($"   - Company: '{company.TenDN}'");
                Console.WriteLine($"     Tax code: '{company.Masothue}'");
                Console.WriteLine($"     Year: {company.Nam}");
            }

            // DIRECT REGION MAPPING - No complex economic zones needed

            // ===== DEBUG REGIONAL DATA START =====
            Console.WriteLine($"\n🚨🚨🚨 REGIONAL DEBUG START - YEAR {currentYear} 🚨🚨🚨");
            Console.WriteLine($"\n🔍 REGIONAL DATA SOURCE DEBUG FOR YEAR {currentYear}:");
            Console.WriteLine($"   - Total unique companies in year: {uniqueCompaniesInYear.Count}");
            Console.WriteLine($"   - Companies with Vungkinhte: {uniqueCompaniesInYear.Count(x => !string.IsNullOrEmpty(x.Record.Vungkinhte))}");
            Console.WriteLine($"   - Companies with Region: {uniqueCompaniesInYear.Count(x => !string.IsNullOrEmpty(x.Record.Region))}");
            Console.WriteLine($"   - Companies with either: {uniqueCompaniesInYear.Count(x => !string.IsNullOrEmpty(x.Record.Vungkinhte) || !string.IsNullOrEmpty(x.Record.Region))}");

            // Sample raw data from Region field only
            Console.WriteLine($"\n🔍 SAMPLE RAW REGIONAL DATA FROM DATABASE:");
            var sampleRawData = uniqueCompaniesInYear.Take(10).ToList();
            foreach (var sample in sampleRawData)
            {
                Console.WriteLine($"   Company: {sample.Record.TenDN}");
                Console.WriteLine($"     Region: '{sample.Record.Region ?? "NULL"}'");
                Console.WriteLine($"     Tax Code: {sample.Masothue}");
                Console.WriteLine("");
            }

            // CHANGED: Use Vungkinhte field for detailed economic zones (7 zones instead of 3 regions)
            var companiesWithVungKinhTe = uniqueCompaniesInYear
                .Where(x => !string.IsNullOrEmpty(x.Record.Vungkinhte))
                .ToList();

            Console.WriteLine($"\n🔍 VUNG KINH TE COUNT FROM DATABASE:");
            Console.WriteLine($"   - Total companies with Vungkinhte data: {companiesWithVungKinhTe.Count}");

            // Group by Vungkinhte values (7 detailed economic zones)
            var regionGrouping = companiesWithVungKinhTe
                .GroupBy(x => x.Record.Vungkinhte)
                .ToDictionary(g => g.Key, g => g.Count());

            Console.WriteLine($"\n🔍 VUNG KINH TE DISTRIBUTION:");
            foreach (var vungKinhTe in regionGrouping.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"   - {vungKinhTe.Key}: {vungKinhTe.Value} companies");
            }

            // Sample companies by vung kinh te
            Console.WriteLine($"\n🔍 SAMPLE COMPANIES BY VUNG KINH TE:");
            foreach (var vungKinhTeGroup in regionGrouping.Take(3))
            {
                var vungKinhTeName = vungKinhTeGroup.Key;
                var vungKinhTeCount = vungKinhTeGroup.Value;
                var vungKinhTeCompanies = companiesWithVungKinhTe
                    .Where(x => x.Record.Vungkinhte == vungKinhTeName)
                    .Take(2);

                Console.WriteLine($"\n   {vungKinhTeName} ({vungKinhTeCount} companies):");
                foreach (var company in vungKinhTeCompanies)
                {
                    Console.WriteLine($"     - {company.Record.TenDN}");
                    Console.WriteLine($"       Tax Code: {company.Masothue}");
                }
            }

            // CHANGED: Use Vungkinhte grouping for detailed economic zones
            stats.RegionCounts = regionGrouping;

            // Map Vungkinhte to 3 main regions for ViewBag compatibility
            var dongBangSongHong = regionGrouping.GetValueOrDefault("Đồng bằng Sông Hồng", 0);
            var trungDuMienNui = regionGrouping.GetValueOrDefault("Trung du và Miền núi Bắc Bộ", 0);
            var bacTrungBo = regionGrouping.GetValueOrDefault("Bắc Trung Bộ", 0);
            var duyenHaiNamTrungBo = regionGrouping.GetValueOrDefault("Duyên hải Nam Trung Bộ", 0);
            var tayNguyen = regionGrouping.GetValueOrDefault("Tây Nguyên", 0);
            var dongNamBo = regionGrouping.GetValueOrDefault("Đông Nam Bộ", 0);
            var dongBangSongCuuLong = regionGrouping.GetValueOrDefault("Đồng bằng Sông Cửu Long", 0);

            ViewBag.MienBacCount = dongBangSongHong + trungDuMienNui;
            ViewBag.MienTrungCount = bacTrungBo + duyenHaiNamTrungBo + tayNguyen;
            ViewBag.MienNamCount = dongNamBo + dongBangSongCuuLong;

            Console.WriteLine($"\n✅ VUNG KINH TE VIEWBAG ASSIGNMENT FOR YEAR {currentYear}:");
            Console.WriteLine($"   - 7 Vùng Kinh Tế found: {regionGrouping.Count}");
            Console.WriteLine($"   - Mapping to 3 miền for ViewBag compatibility:");
            Console.WriteLine($"     * Miền Bắc: {ViewBag.MienBacCount} companies (Đồng bằng SH + Trung du miền núi)");
            Console.WriteLine($"     * Miền Trung: {ViewBag.MienTrungCount} companies (Bắc TB + Duyên hải NTB + Tây Nguyên)");
            Console.WriteLine($"     * Miền Nam: {ViewBag.MienNamCount} companies (Đông Nam Bộ + ĐBSCL)");
            Console.WriteLine($"   - Total regional: {ViewBag.MienBacCount + ViewBag.MienTrungCount + ViewBag.MienNamCount} companies");

            // Business type distribution - FIXED: Use unique companies from current year
            var companiesWithBusinessType = uniqueCompaniesInYear
                .Where(x => !string.IsNullOrEmpty(x.Record.Loaihinhkte))
                .ToList();

            Console.WriteLine($"\n🚨🚨🚨 BUSINESS TYPE DEBUG START - YEAR {currentYear} 🚨🚨🚨");
            Console.WriteLine($"🔍 BUSINESS TYPE DATA SOURCE DEBUG FOR YEAR {currentYear}:");
            Console.WriteLine($"   - Total unique companies in year: {uniqueCompaniesInYear.Count}");
            Console.WriteLine($"   - Companies with Loaihinhkte: {companiesWithBusinessType.Count}");

            stats.BusinessTypeCounts = companiesWithBusinessType
                .GroupBy(x => x.Record.Loaihinhkte)
                .ToDictionary(g => g.Key ?? "Unknown", g => g.Count());

            Console.WriteLine($"\n🔍 DIRECT BUSINESS TYPE DISTRIBUTION:");
            foreach (var businessType in stats.BusinessTypeCounts.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"   - {businessType.Key}: {businessType.Value} companies");
            }

            // Sample companies by business type
            Console.WriteLine($"\n🔍 SAMPLE COMPANIES BY BUSINESS TYPE:");
            foreach (var typeGroup in stats.BusinessTypeCounts.Take(3))
            {
                var typeName = typeGroup.Key;
                var typeCount = typeGroup.Value;
                var typeCompanies = companiesWithBusinessType
                    .Where(x => x.Record.Loaihinhkte == typeName)
                    .Take(2);

                Console.WriteLine($"\n   {typeName} ({typeCount} companies):");
                foreach (var company in typeCompanies)
                {
                    Console.WriteLine($"     - {company.Record.TenDN}");
                    Console.WriteLine($"       Tax Code: {company.Masothue}");
                }
            }

            Console.WriteLine($"\n🔍 Business type counts: {stats.BusinessTypeCounts.Count} types");

            // If no business types, leave empty - NO DEMO DATA
            if (!stats.BusinessTypeCounts.Any())
            {
                stats.BusinessTypeCounts = new Dictionary<string, int>();
                Console.WriteLine($"⚠️ No business type data available from database");
            }

            // Financial data - use current year data
            CalculateFinancialData(currentYearData, stats);

            // Industry data - format for chart - ENHANCED DEBUG
            Console.WriteLine($"\n🔍 INDUSTRY DEBUG - Total records for current year: {currentYearData.Count}");

            Console.WriteLine($"🔍 INDUSTRY DATA DEBUG - Starting analysis...");
            Console.WriteLine($"🔍 Total records for year {currentYear}: {currentYearData.Count}");

            // Use unique companies from current year for industry analysis
            var uniqueIndustryCompanies = uniqueCompanies;

            Console.WriteLine($"\n🔍 INDUSTRY DISTRIBUTION VALIDATION:");
            Console.WriteLine($"========================================");
            Console.WriteLine($"Total records in database: {data.Count}");
            Console.WriteLine($"Total unique companies: {uniqueCompanies.Count}");

            // Detailed analysis of TEN_NGANH values
            var nullValues = uniqueCompanies.Count(x => x.TEN_NGANH == null);
            var emptyStrings = uniqueCompanies.Count(x => x.TEN_NGANH != null && x.TEN_NGANH.Length == 0);
            var whitespaceOnly = uniqueCompanies.Count(x => x.TEN_NGANH != null && x.TEN_NGANH.Trim().Length == 0);
            var validValues = uniqueCompanies.Count(x => !string.IsNullOrWhiteSpace(x.TEN_NGANH));

            Console.WriteLine($"🔍 DETAILED INDUSTRY DATA ANALYSIS:");
            Console.WriteLine($"   - NULL values: {nullValues}");
            Console.WriteLine($"   - Empty strings: {emptyStrings}");
            Console.WriteLine($"   - Whitespace only: {whitespaceOnly}");
            Console.WriteLine($"   - Valid values: {validValues}");

            // Split into companies with and without industry data (more precise check)
            var companiesWithIndustry = uniqueIndustryCompanies
                .Where(x => !string.IsNullOrWhiteSpace(x.TEN_NGANH))
                .ToList();

            var companiesWithoutIndustry = uniqueIndustryCompanies
                .Where(x => string.IsNullOrWhiteSpace(x.TEN_NGANH))
                .ToList();

            Console.WriteLine($"🔍 Companies with valid industry data: {companiesWithIndustry.Count}");
            Console.WriteLine($"🔍 Companies without valid industry data: {companiesWithoutIndustry.Count}");

            // Show sample of companies without industry data
            if (companiesWithoutIndustry.Any())
            {
                Console.WriteLine($"\n🔍 SAMPLE OF COMPANIES WITHOUT INDUSTRY DATA:");
                foreach (var company in companiesWithoutIndustry.Take(5))
                {
                    Console.WriteLine($"   - Company: {company.TenDN}");
                    Console.WriteLine($"     TEN_NGANH: '{company.TEN_NGANH}'");
                    Console.WriteLine($"     MaNganhC5_Chinh: '{company.MaNganhC5_Chinh}'");
                }
            }

            // Calculate industry distribution for companies with industry data
            var industryDistribution = companiesWithIndustry
                .GroupBy(x => x.TEN_NGANH.Trim())  // Trim to standardize
                .Select(g => new { TEN_NGANH = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ToList();

            // Add the "No Industry Data" category
            if (companiesWithoutIndustry.Any())
            {
                industryDistribution.Add(new { TEN_NGANH = "Chưa có dữ liệu ngành nghề", SoLuong = companiesWithoutIndustry.Count });
            }

            // Validate total companies in distribution
            var totalInDistribution = industryDistribution.Sum(x => x.SoLuong);

            Console.WriteLine($"\n🔍 INDUSTRY DISTRIBUTION VALIDATION:");
            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"Total companies in distribution: {totalInDistribution}");
            Console.WriteLine($"Should match unique companies: {uniqueCompanies.Count}");

            if (totalInDistribution != uniqueCompanies.Count)
            {
                Console.WriteLine($"❌ ERROR: Mismatch in totals!");
                Console.WriteLine($"Missing companies: {uniqueCompanies.Count - totalInDistribution}");
            }
            else
            {
                Console.WriteLine($"✅ VALIDATION PASSED: Totals match!");
            }

            // Get total number of industries for logging
            var totalIndustries = industryDistribution.Count();
            Console.WriteLine($"\n🔍 TOTAL UNIQUE INDUSTRIES: {totalIndustries}");

            // Take top 20 industries for visualization
            stats.IndustryData = industryDistribution
                .OrderByDescending(x => x.SoLuong)
                .Take(20)
                .ToList<object>();

            // Log all industries for reference
            Console.WriteLine($"\n🔍 ALL INDUSTRIES BY COUNT ({totalIndustries} total):");
            Console.WriteLine($"----------------------------------------");
            foreach (var industry in industryDistribution)
            {
                Console.WriteLine($"- {industry.TEN_NGANH}: {industry.SoLuong} companies");
            }

            Console.WriteLine($"\n🔍 TOP 10 INDUSTRIES (including no-data category):");
            Console.WriteLine($"----------------------------------------");
            foreach (var industry in stats.IndustryData.Take(10))
            {
                var ind = (dynamic)industry;
                Console.WriteLine($"- {ind.TEN_NGANH}: {ind.SoLuong} companies");
            }
            if (stats.IndustryData.Count > 0)
            {
                Console.WriteLine($"🔍 Top categories from database:");
                foreach (var industry in stats.IndustryData.Take(5))
                {
                    var industryObj = (dynamic)industry;
                    Console.WriteLine($"   - {industryObj.TEN_NGANH}: {industryObj.SoLuong} companies");
                }
            }
            else
            {
                Console.WriteLine($"❌ CRITICAL ERROR: NO INDUSTRY DATA GENERATED!");
                Console.WriteLine($"❌ This means TEN_NGANH column is empty or not being read correctly");
            }

            // Years - for trend analysis, use all available years but focus on current year
            stats.Years = data
                .Where(x => x.Nam.HasValue && x.Nam.Value > 1990 && x.Nam.Value <= DateTime.Now.Year + 1)
                .Select(x => x.Nam.Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            Console.WriteLine($"🔍 Years found: {string.Join(", ", stats.Years)}");
            Console.WriteLine($"🔍 Current analysis year: {currentYear}");

            // Revenue and profit data - use all years for trend
            CalculateRevenueData(data, stats);

            // Initialize remaining properties - use unique companies from current year
            stats.ProvinceData = uniqueCompanies
                .Where(x => !string.IsNullOrEmpty(x.MaTinh_Dieutra))
                .GroupBy(x => x.MaTinh_Dieutra)
                .Select(g => new { Province = g.Key ?? "Unknown", Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10) // Top 10 provinces
                .ToList<object>();

            stats.RegionData = stats.RegionCounts
                .Where(x => x.Value > 0) // Only include zones with data
                .Select(x => new { Region = x.Key, SoLuong = x.Value })
                .OrderByDescending(x => x.SoLuong)
                .ToList<object>();

            stats.BusinessTypeData = stats.BusinessTypeCounts
                .Select(x => new { TinhTrang = x.Key, SoLuong = x.Value })
                .OrderByDescending(x => x.SoLuong)
                .ToList<object>();

            stats.CompanySizeData = CalculateCompanySizeData(uniqueCompanies);

            Console.WriteLine($"✅ Final stats - Economic Zones: {stats.RegionData.Count}, Business Types: {stats.BusinessTypeData.Count}, Company Sizes: {stats.CompanySizeData.Count}");

            return stats;
        }

        private static void CalculateFinancialData(List<QLKH> data, ComprehensiveStats stats)
        {
            Console.WriteLine($"\n🚨🚨🚨 FINANCIAL DATA DEBUG START 🚨🚨🚨");
            Console.WriteLine($"🔍 FINANCIAL DATA CALCULATION FOR YEAR DATA:");
            Console.WriteLine($"   - Total input records: {data.Count}");

            // Count unique companies with revenue
            var uniqueCompaniesWithRevenue = data
                .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0)
                .GroupBy(x => !string.IsNullOrEmpty(x.Masothue) ? x.Masothue : x.TenDN)
                .ToList();

            // Count unique companies with profit
            var uniqueCompaniesWithProfit = data
                .Where(x => x.SR_Loinhuan_TruocThue.HasValue)
                .GroupBy(x => !string.IsNullOrEmpty(x.Masothue) ? x.Masothue : x.TenDN)
                .ToList();

            // Count unique companies with assets
            var uniqueCompaniesWithAssets = data
                .Where(x => x.Taisan_Tong_CK.HasValue && x.Taisan_Tong_CK.Value > 0)
                .GroupBy(x => !string.IsNullOrEmpty(x.Masothue) ? x.Masothue : x.TenDN)
                .ToList();

            Console.WriteLine($"\n🔍 DETAILED FINANCIAL DATA ANALYSIS:");
            Console.WriteLine($"   - Raw records with Revenue > 0: {data.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0)}");
            Console.WriteLine($"   - Raw records with Profit data: {data.Count(x => x.SR_Loinhuan_TruocThue.HasValue)}");
            Console.WriteLine($"   - Raw records with Assets > 0: {data.Count(x => x.Taisan_Tong_CK.HasValue && x.Taisan_Tong_CK.Value > 0)}");

            Console.WriteLine($"\n🔍 UNIQUE COMPANIES WITH FINANCIAL DATA:");
            Console.WriteLine($"   - Companies with Revenue: {uniqueCompaniesWithRevenue.Count}");
            Console.WriteLine($"   - Companies with Profit: {uniqueCompaniesWithProfit.Count}");
            Console.WriteLine($"   - Companies with Assets: {uniqueCompaniesWithAssets.Count}");

            // For each company, use the most recent year's data
            var revenueData = uniqueCompaniesWithRevenue
                .Select(g => g.OrderByDescending(x => x.Nam).First().SR_Doanhthu_Thuan_BH_CCDV.Value)
                .ToList();

            var profitData = uniqueCompaniesWithProfit
                .Select(g => g.OrderByDescending(x => x.Nam).First().SR_Loinhuan_TruocThue.Value)
                .ToList();

            var assetData = uniqueCompaniesWithAssets
                .Select(g => g.OrderByDescending(x => x.Nam).First().Taisan_Tong_CK.Value)
                .ToList();

            // Use safe calculations to prevent overflow
            try
            {
                stats.FinancialStats["TotalRevenue"] = revenueData.Any() ? revenueData.Sum() : 0;
                Console.WriteLine($"   - Total Revenue (latest year): {stats.FinancialStats["TotalRevenue"]:N0} triệu VND");
            }
            catch (OverflowException)
            {
                stats.FinancialStats["TotalRevenue"] = decimal.MaxValue;
                Console.WriteLine("⚠️ Revenue sum overflow, using MaxValue");
            }

            stats.FinancialStats["AverageRevenue"] = revenueData.Any() ? revenueData.Average() : 0;
            stats.FinancialStats["CompaniesWithRevenue"] = uniqueCompaniesWithRevenue.Count;

            try
            {
                stats.FinancialStats["TotalAssets"] = assetData.Any() ? assetData.Sum() : 0;
                Console.WriteLine($"   - Total Assets (latest year): {stats.FinancialStats["TotalAssets"]:N0} triệu VND");
            }
            catch (OverflowException)
            {
                stats.FinancialStats["TotalAssets"] = decimal.MaxValue;
                Console.WriteLine("⚠️ Assets sum overflow, using MaxValue");
            }

            stats.FinancialStats["AverageAssets"] = assetData.Any() ? assetData.Average() : 0;
            stats.FinancialStats["CompaniesWithAssets"] = uniqueCompaniesWithAssets.Count;

            try
            {
                stats.FinancialStats["TotalProfit"] = profitData.Any() ? profitData.Sum() : 0;
                Console.WriteLine($"   - Total Profit (latest year): {stats.FinancialStats["TotalProfit"]:N0} triệu VND");
            }
            catch (OverflowException)
            {
                stats.FinancialStats["TotalProfit"] = decimal.MaxValue;
                Console.WriteLine("⚠️ Profit sum overflow, using MaxValue");
            }

            stats.FinancialStats["AverageProfit"] = profitData.Any() ? profitData.Average() : 0;
            stats.FinancialStats["CompaniesWithProfit"] = uniqueCompaniesWithProfit.Count;

            // Sample data for verification
            Console.WriteLine("\n🔍 SAMPLE COMPANIES WITH FINANCIAL DATA:");
            foreach (var company in uniqueCompaniesWithRevenue.Take(3))
            {
                var latestRecord = company.OrderByDescending(x => x.Nam).First();
                Console.WriteLine($"\n   Company: {latestRecord.TenDN}");
                Console.WriteLine($"   Tax Code: {latestRecord.Masothue}");
                Console.WriteLine($"   Latest Year: {latestRecord.Nam}");
                Console.WriteLine($"   Revenue: {latestRecord.SR_Doanhthu_Thuan_BH_CCDV:N0} triệu VND");
                Console.WriteLine($"   Profit: {latestRecord.SR_Loinhuan_TruocThue:N0} triệu VND");
                Console.WriteLine($"   Assets: {latestRecord.Taisan_Tong_CK:N0} triệu VND");
                Console.WriteLine($"   Years present: {string.Join(", ", company.Select(x => x.Nam).OrderBy(x => x))}");
            }

            // Thêm thống kê cho tài sản cuối kỳ (Taisan_Tong_CK) - already calculated above
            stats.FinancialStats["TotalAssetsCK"] = stats.FinancialStats["TotalAssets"];
            stats.FinancialStats["CompaniesWithAssetsCK"] = uniqueCompaniesWithAssets.Count;

            Console.WriteLine($"\n✅ FINAL FINANCIAL STATS CALCULATED:");
            Console.WriteLine($"   - CompaniesWithRevenue: {stats.FinancialStats["CompaniesWithRevenue"]}");
            Console.WriteLine($"   - CompaniesWithAssets: {stats.FinancialStats["CompaniesWithAssets"]}");
            Console.WriteLine($"   - CompaniesWithProfit: {stats.FinancialStats["CompaniesWithProfit"]}");
            Console.WriteLine($"   - TotalAssetsCK: {stats.FinancialStats["TotalAssetsCK"]:N0} triệu VND");
            Console.WriteLine($"   - CompaniesWithAssetsCK: {stats.FinancialStats["CompaniesWithAssetsCK"]}");
            Console.WriteLine($"   - TotalRevenue: {stats.FinancialStats["TotalRevenue"]:N0} triệu VND");
            Console.WriteLine($"   - TotalProfit: {stats.FinancialStats["TotalProfit"]:N0} triệu VND");
        }

        private static void CalculateRevenueData(List<QLKH> data, ComprehensiveStats stats)
        {
            Console.WriteLine($"🔍 TREND DATA CALCULATION - Starting with {data.Count} total records");

            // Step 1: Check all Nam values in the dataset
            var allYears = data
                .Where(x => x.Nam.HasValue)
                .Select(x => x.Nam.Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            Console.WriteLine($"🔍 ALL YEARS found in Nam column: [{string.Join(", ", allYears)}]");
            Console.WriteLine($"🔍 Total records with Nam value: {data.Count(x => x.Nam.HasValue)}");

            // Step 2: Check revenue and profit data availability by year
            foreach (var year in allYears)
            {
                var yearRecords = data.Where(x => x.Nam == year).ToList();
                var revenueCount = yearRecords.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue);
                var profitCount = yearRecords.Count(x => x.SR_Loinhuan_TruocThue.HasValue);
                var bothCount = yearRecords.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Loinhuan_TruocThue.HasValue);

                Console.WriteLine($"📊 Year {year}: Total={yearRecords.Count}, HasRevenue={revenueCount}, HasProfit={profitCount}, HasBoth={bothCount}");

                // Show sample records for each year
                if (yearRecords.Count > 0)
                {
                    Console.WriteLine($"   🔍 Sample records for year {year}:");
                    foreach (var sample in yearRecords.Take(2))
                    {
                        Console.WriteLine($"     - STT: {sample.STT}, Company: '{sample.TenDN}', Nam: {sample.Nam}, Revenue: {sample.SR_Doanhthu_Thuan_BH_CCDV?.ToString("N0") ?? "NULL"}, Profit: {sample.SR_Loinhuan_TruocThue?.ToString("N0") ?? "NULL"}");
                    }
                }
            }

            // Step 3: Filter with detailed logging for each condition
            Console.WriteLine($"🔍 FILTERING STEP BY STEP:");

            var step1_hasYear = data.Where(x => x.Nam.HasValue).ToList();
            Console.WriteLine($"   Step 1 - Has Nam: {step1_hasYear.Count} records");

            var step2_hasRevenue = step1_hasYear.Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue).ToList();
            Console.WriteLine($"   Step 2 - Has Nam + Revenue: {step2_hasRevenue.Count} records");

            var step3_hasProfit = step2_hasRevenue.Where(x => x.SR_Loinhuan_TruocThue.HasValue).ToList();
            Console.WriteLine($"   Step 3 - Has Nam + Revenue + Profit: {step3_hasProfit.Count} records");

            var validRecords = step3_hasProfit.ToList();

            Console.WriteLine($"🔍 FINAL VALID RECORDS: {validRecords.Count}");

            if (validRecords.Count > 0)
            {
                Console.WriteLine($"🔍 Year range in VALID data: {validRecords.Min(x => x.Nam.Value)} - {validRecords.Max(x => x.Nam.Value)}");
                Console.WriteLine($"🔍 Years in VALID data: [{string.Join(", ", validRecords.Select(x => x.Nam.Value).Distinct().OrderBy(x => x))}]");

                Console.WriteLine($"🔍 Sample VALID records with trend data:");
                foreach (var sample in validRecords.Take(5))
                {
                    Console.WriteLine($"   - STT: {sample.STT}, Company: {sample.TenDN}, Year: {sample.Nam}, Revenue: {sample.SR_Doanhthu_Thuan_BH_CCDV:N0} triệu VND, Profit: {sample.SR_Loinhuan_TruocThue:N0} triệu VND");
                }
            }

            // Step 4: Group by year with detailed logging
            Console.WriteLine($"🔍 GROUPING BY YEAR:");
            var revenueAndProfitByYear = validRecords
                .GroupBy(x => x.Nam.Value)
                .Select(g => new
                {
                    Year = g.Key,
                    CompanyCount = g.Count(),
                    Revenue = g.Sum(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value),
                    Profit = g.Sum(x => x.SR_Loinhuan_TruocThue.Value)
                })
                .OrderBy(x => x.Year)
                .ToList();

            Console.WriteLine($"🔍 TREND CALCULATION RESULTS:");
            if (revenueAndProfitByYear.Any())
            {
                Console.WriteLine($"   - Total years after grouping: {revenueAndProfitByYear.Count}");
                Console.WriteLine($"   - Years found: [{string.Join(", ", revenueAndProfitByYear.Select(x => x.Year))}]");

                foreach (var yearData in revenueAndProfitByYear)
                {
                    Console.WriteLine($"   📊 Year {yearData.Year}: {yearData.CompanyCount} companies, Revenue: {yearData.Revenue:N0} triệu VND, Profit: {yearData.Profit:N0} triệu VND");
                }
                Console.WriteLine($"✅ Using REAL trend data from database column Nam, SR_Doanhthu_Thuan_BH_CCDV, SR_Loinhuan_TruocThue");
            }
            else
            {
                Console.WriteLine($"❌ NO TREND DATA FOUND after grouping from database admin_ciresearch.dn_all");
                Console.WriteLine($"❌ Check if records have valid values in Nam, SR_Doanhthu_Thuan_BH_CCDV, SR_Loinhuan_TruocThue columns");
            }

            // Step 5: Assign to stats with verification
            stats.Years = revenueAndProfitByYear.Select(x => x.Year).ToList();
            stats.RevenueData = revenueAndProfitByYear.Select(x => (double)x.Revenue).ToList();
            stats.ProfitData = revenueAndProfitByYear.Select(x => (double)x.Profit).ToList();

            Console.WriteLine($"✅ FINAL ASSIGNMENT TO STATS:");
            Console.WriteLine($"   - stats.Years: [{string.Join(", ", stats.Years)}]");
            Console.WriteLine($"   - stats.RevenueData: [{string.Join(", ", stats.RevenueData.Select(x => $"{x:N0}"))}]");
            Console.WriteLine($"   - stats.ProfitData: [{string.Join(", ", stats.ProfitData.Select(x => $"{x:N0}"))}]");
        }

        private static List<object> CalculateCompanySizeData(List<QLKH> data)
        {
            try
            {
                Console.WriteLine($"\n🚨 NEW SIMPLE QUY_MO CHART - DIRECT FROM QUY_MO COLUMN 🚨");
                Console.WriteLine($"📊 Processing {data.Count} total records");

                // Group companies by their unique identifier to avoid duplicates
                var uniqueCompanies = data
                    .GroupBy(x => !string.IsNullOrEmpty(x.Masothue) ? x.Masothue : x.TenDN)
                        .Select(g => g.OrderByDescending(x => x.Nam).First()) // Get latest record for each company
                    .ToList();

                Console.WriteLine($"📊 Unique companies: {uniqueCompanies.Count}");

                // GROUP BY QUY_MO with short labels mapping
                var quyMoGroups = uniqueCompanies
                    .Where(x => !string.IsNullOrWhiteSpace(x.QUY_MO))
                    .GroupBy(x => x.QUY_MO.Trim())
                    .Select(g => new
                    {
                        QuyMo = GetQuyMoDescription(g.Key), // Use short label (Siêu nhỏ, Nhỏ, Vừa, Lớn)
                        SoLuong = g.Count(),
                        MoTa = GetQuyMoDescription(g.Key) // Same as QuyMo for consistency
                    })
                    .OrderBy(x => GetQuyMoOrderShort(x.QuyMo)) // Order by short labels
                    .Cast<object>()
                .ToList();

                Console.WriteLine($"📊 QUY_MO COLUMN ANALYSIS:");
                Console.WriteLine($"   - Total companies: {uniqueCompanies.Count}");

                var totalWithQuyMo = 0;
                foreach (var group in quyMoGroups)
                {
                    var groupData = (dynamic)group;
                    totalWithQuyMo += (int)groupData.SoLuong;
                }

                Console.WriteLine($"   - Companies with QUY_MO: {totalWithQuyMo}");
                Console.WriteLine($"   - Companies without QUY_MO: {uniqueCompanies.Count - totalWithQuyMo}");
                Console.WriteLine($"   - Unique QUY_MO values: {quyMoGroups.Count}");

                foreach (var group in quyMoGroups)
                {
                    var groupData = (dynamic)group;
                    Console.WriteLine($"   - '{groupData.QuyMo}': {groupData.SoLuong} companies");
                }

                // If no QUY_MO data found, return empty list (let frontend handle it)
                if (!quyMoGroups.Any())
                {
                    Console.WriteLine($"❌ NO QUY_MO DATA FOUND IN DATABASE!");
                    return new List<object>();
                }

                Console.WriteLine($"✅ QUY_MO chart data ready - {quyMoGroups.Count} categories");
                return quyMoGroups;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in CalculateCompanySizeData: {ex.Message}");
                return new List<object>();
            }
        }

        // Helper method to get description for QUY_MO values
        private static string GetQuyMoDescription(string quyMo)
        {
            return quyMo switch
            {
                "Doanh nghiệp siêu nhỏ" => "Siêu nhỏ",
                "Doanh nghiệp nhỏ" => "Nhỏ",
                "Doanh nghiệp vừa" => "Vừa",
                "Doanh nghiệp lớn" => "Lớn",
                _ => quyMo
            };
        }

        // Helper method to get order for short QUY_MO labels
        private static int GetQuyMoOrderShort(string quyMoShort)
        {
            return quyMoShort switch
            {
                "Siêu nhỏ" => 1,
                "Nhỏ" => 2,
                "Vừa" => 3,
                "Lớn" => 4,
                _ => 5
            };
        }

        // Helper method to get order for QUY_MO values
        private static int GetQuyMoOrder(string quyMo)
        {
            return quyMo switch
            {
                "Doanh nghiệp siêu nhỏ" => 1,
                "Doanh nghiệp nhỏ" => 2,
                "Doanh nghiệp vừa" => 3,
                "Doanh nghiệp lớn" => 4,
                _ => 5
            };
        }

        // Helper method to map database QUY_MO values to simple labels frontend expects
        private static string MapToSimpleLabel(string quyMoFromDb)
        {
            if (string.IsNullOrWhiteSpace(quyMoFromDb))
                return "Khác";

            var normalized = quyMoFromDb.Trim().ToLower();

            // Map to simple labels that match frontend expectations
            if (normalized.Contains("siêu nhỏ") || normalized.Contains("sieu nho"))
                return "Siêu nhỏ";
            else if (normalized.Contains("nhỏ") || normalized.Contains("nho"))
                return "Nhỏ";
            else if (normalized.Contains("vừa") || normalized.Contains("vua"))
                return "Vừa";
            else if (normalized.Contains("lớn") || normalized.Contains("lon"))
                return "Lớn";
            else
                return quyMoFromDb; // Keep original if no match
        }

        // Simple order for size categories
        private static int GetSimpleSizeOrder(string quyMo)
        {
            return quyMo switch
            {
                "Siêu nhỏ" => 1,
                "Nhỏ" => 2,
                "Vừa" => 3,
                "Lớn" => 4,
                _ => 5
            };
        }

        // Helper method to get description for company size
        private static string GetCompanySizeDescription(string quyMo)
        {
            if (string.IsNullOrWhiteSpace(quyMo))
                return "Không xác định";

            var size = quyMo.Trim().ToLower();

            return size switch
            {
                "siêu nhỏ" or "sieu nho" => "Doanh nghiệp siêu nhỏ",
                "nhỏ" or "nho" => "Doanh nghiệp nhỏ",
                "vừa" or "vua" => "Doanh nghiệp vừa",
                "lớn" or "lon" => "Doanh nghiệp lớn",
                _ => $"Quy mô: {quyMo}" // Return original value with prefix
            };
        }

        // Helper method to get standard description for company size categories
        private static string GetStandardCompanySizeDescription(string category)
        {
            return category switch
            {
                "Doanh nghiệp siêu nhỏ" => "Doanh nghiệp siêu nhỏ (DT ≤ 3 tỷ VND)",
                "Doanh nghiệp nhỏ" => "Doanh nghiệp nhỏ (3 tỷ < DT ≤ 50 tỷ VND)",
                "Doanh nghiệp vừa" => "Doanh nghiệp vừa (50 tỷ < DT ≤ 300 tỷ VND)",
                "Doanh nghiệp lớn" => "Doanh nghiệp lớn (DT > 300 tỷ VND)",
                _ => category
            };
        }

        // Helper method to get sort order for company size categories
        private static int GetCompanySizeOrder(string category)
        {
            return category switch
            {
                "Doanh nghiệp siêu nhỏ" => 1,
                "Doanh nghiệp nhỏ" => 2,
                "Doanh nghiệp vừa" => 3,
                "Doanh nghiệp lớn" => 4,
                _ => 5
            };
        }

        private void AssignStatsToViewBag(ComprehensiveStats stats)
        {
            // Basic stats
            ViewBag.TotalCompanies = stats.TotalCompanies;
            ViewBag.TotalLabor = stats.TotalLabor;

            // Region stats
            ViewBag.RegionCounts = stats.RegionCounts ?? new Dictionary<string, int>();
            ViewBag.RegionData = stats.RegionData ?? new List<object>();

            // Business type stats
            ViewBag.BusinessTypeCounts = stats.BusinessTypeCounts ?? new Dictionary<string, int>();
            ViewBag.BusinessTypeData = stats.BusinessTypeData ?? new List<object>();

            // Financial stats
            ViewBag.FinancialStats = stats.FinancialStats ?? new Dictionary<string, decimal>();

            // Ensure RevenueData and ProfitData don't contain null values
            var safeRevenueData = (stats.RevenueData ?? new List<double>())
                .Select(x => double.IsNaN(x) || double.IsInfinity(x) ? 0.0 : x)
                .ToList();
            var safeProfitData = (stats.ProfitData ?? new List<double>())
                .Select(x => double.IsNaN(x) || double.IsInfinity(x) ? 0.0 : x)
                .ToList();

            ViewBag.RevenueData = safeRevenueData;
            ViewBag.ProfitData = safeProfitData;

            // Industry stats
            ViewBag.IndustryData = stats.IndustryData ?? new List<object>();

            // Years
            ViewBag.Years = stats.Years ?? new List<int>();

            // Company size data
            ViewBag.CompanySizeData = stats.CompanySizeData ?? new List<object>();

            // Province data
            ViewBag.ProvinceData = stats.ProvinceData ?? new List<object>();

            // Financial summary stats for the view
            var financialStats = stats.FinancialStats ?? new Dictionary<string, decimal>();

            Console.WriteLine($"\n✅ VIEWBAG FINANCIAL ASSIGNMENT FOR YEAR {ViewBag.CurrentAnalysisYear}:");
            Console.WriteLine($"   - Input financialStats count: {financialStats.Count}");
            foreach (var kv in financialStats)
            {
                Console.WriteLine($"     '{kv.Key}': {kv.Value}");
            }

            ViewBag.CompaniesWithRevenue = (int)(financialStats.GetValueOrDefault("CompaniesWithRevenue", 0));
            ViewBag.CompaniesWithAssets = (int)(financialStats.GetValueOrDefault("CompaniesWithAssets", 0));
            ViewBag.CompaniesWithProfit = (int)(financialStats.GetValueOrDefault("CompaniesWithProfit", 0));

            // Tính tổng tài sản cuối kỳ (Taisan_Tong_CK) 
            ViewBag.TotalAssetsCK = financialStats.GetValueOrDefault("TotalAssetsCK", 0);
            ViewBag.CompaniesWithAssetsCK = (int)(financialStats.GetValueOrDefault("CompaniesWithAssetsCK", 0));

            Console.WriteLine($"\n✅ FINAL VIEWBAG FINANCIAL ASSIGNMENT:");
            Console.WriteLine($"   - ViewBag.CompaniesWithRevenue: {ViewBag.CompaniesWithRevenue}");
            Console.WriteLine($"   - ViewBag.CompaniesWithAssets: {ViewBag.CompaniesWithAssets}");
            Console.WriteLine($"   - ViewBag.CompaniesWithProfit: {ViewBag.CompaniesWithProfit}");
            Console.WriteLine($"   - ViewBag.TotalAssetsCK: {ViewBag.TotalAssetsCK:N0} triệu VND");
            Console.WriteLine($"   - ViewBag.CompaniesWithAssetsCK: {ViewBag.CompaniesWithAssetsCK}");

            // Technology adoption stats - ensure all have default values
            ViewBag.CoInternet = 0;
            ViewBag.CoWebsite = 0;
            ViewBag.CoPhanmem = 0;
            ViewBag.CoTudonghoa = 0;

            // Region counts for the view - USE DIRECT REGION VALUES FROM DATABASE
            // ViewBag is already assigned correctly in CalculateAllStatistics - DON'T OVERRIDE!

            // Top 3 Business Types (Phân loại DN) - lấy từ database thực tế
            var top3BusinessTypes = stats.BusinessTypeCounts
                .OrderByDescending(x => x.Value)
                .Take(3)
                .ToList();

            Console.WriteLine($"\n✅ TOP 3 BUSINESS TYPES ASSIGNMENT FOR YEAR {ViewBag.CurrentAnalysisYear}:");
            for (int i = 0; i < 3; i++)
            {
                var typeName = i < top3BusinessTypes.Count ? top3BusinessTypes[i].Key : "N/A";
                var typeCount = i < top3BusinessTypes.Count ? top3BusinessTypes[i].Value : 0;
                var shortName = i < top3BusinessTypes.Count ? ShortenBusinessTypeName(top3BusinessTypes[i].Key) : "N/A";
                Console.WriteLine($"   - Top {i + 1}: '{typeName}' → '{shortName}' ({typeCount} companies)");
            }

            // Gán top 3 loại hình doanh nghiệp vào ViewBag với tên viết tắt
            ViewBag.TopBusinessType1Name = top3BusinessTypes.Count > 0 ? ShortenBusinessTypeName(top3BusinessTypes[0].Key) : "N/A";
            ViewBag.TopBusinessType1Count = top3BusinessTypes.Count > 0 ? top3BusinessTypes[0].Value : 0;

            ViewBag.TopBusinessType2Name = top3BusinessTypes.Count > 1 ? ShortenBusinessTypeName(top3BusinessTypes[1].Key) : "N/A";
            ViewBag.TopBusinessType2Count = top3BusinessTypes.Count > 1 ? top3BusinessTypes[1].Value : 0;

            ViewBag.TopBusinessType3Name = top3BusinessTypes.Count > 2 ? ShortenBusinessTypeName(top3BusinessTypes[2].Key) : "N/A";
            ViewBag.TopBusinessType3Count = top3BusinessTypes.Count > 2 ? top3BusinessTypes[2].Value : 0;

            Console.WriteLine($"\n✅ FINAL VIEWBAG BUSINESS TYPE ASSIGNMENT:");
            Console.WriteLine($"   - ViewBag.TopBusinessType1: '{ViewBag.TopBusinessType1Name}' = {ViewBag.TopBusinessType1Count} companies");
            Console.WriteLine($"   - ViewBag.TopBusinessType2: '{ViewBag.TopBusinessType2Name}' = {ViewBag.TopBusinessType2Count} companies");
            Console.WriteLine($"   - ViewBag.TopBusinessType3: '{ViewBag.TopBusinessType3Name}' = {ViewBag.TopBusinessType3Count} companies");

            // Additional company stats with default values
            ViewBag.CompaniesActive = 0;
            ViewBag.CompaniesSuspended = 0;
            ViewBag.CompaniesDisolved = 0;
            ViewBag.CompaniesWithOffice = 0;
            ViewBag.CompaniesWithCredit = 0;
            ViewBag.CompaniesWithBranch = 0;
            ViewBag.CompaniesWithImportExport = 0;

            // Percentage calculations with safe defaults
            ViewBag.PhanmemPercent = 0;
            ViewBag.TudonghoaPercent = 0;

            // Convert data to JSON for charts - with null checks
            ViewBag.RegionDataJson = JsonConvert.SerializeObject(stats.RegionData ?? new List<object>());
            ViewBag.BusinessTypeDataJson = JsonConvert.SerializeObject(stats.BusinessTypeData ?? new List<object>());
            ViewBag.IndustryDataJson = JsonConvert.SerializeObject(stats.IndustryData ?? new List<object>());
            ViewBag.CompanySizeDataJson = JsonConvert.SerializeObject(stats.CompanySizeData ?? new List<object>());
            ViewBag.ProvinceDataJson = JsonConvert.SerializeObject(stats.ProvinceData ?? new List<object>());
            ViewBag.RevenueDataJson = JsonConvert.SerializeObject(safeRevenueData);
            ViewBag.ProfitDataJson = JsonConvert.SerializeObject(safeProfitData);

            // Add raw data for debugging
            ViewBag.IndustryData = stats.IndustryData;

            // Add VungKinhTeData specifically for the new chart
            ViewBag.VungKinhTeData = stats.RegionData ?? new List<object>();

            // Add QuyMoData for company size chart with ULTRA ENHANCED debug
            var quyMoDataForViewBag = stats.CompanySizeData ?? new List<object>();

            Console.WriteLine($"\n🚨🚨🚨 ULTRA DEBUG - QUY MO CHART DATA 🚨🚨🚨");
            Console.WriteLine($"📊 ViewBag.QuyMoData PREPARATION:");
            Console.WriteLine($"   - CompanySizeData count: {quyMoDataForViewBag.Count}");
            Console.WriteLine($"   - Is null or empty: {quyMoDataForViewBag == null || !quyMoDataForViewBag.Any()}");

            if (quyMoDataForViewBag.Count > 0)
            {
                Console.WriteLine($"📋 DETAILED QUY MO DATA ITEMS:");
                for (int i = 0; i < quyMoDataForViewBag.Count; i++)
                {
                    var item = quyMoDataForViewBag[i];
                    var itemProps = item.GetType().GetProperties();
                    Console.WriteLine($"   📌 Item {i + 1}:");
                    foreach (var prop in itemProps)
                    {
                        var value = prop.GetValue(item);
                        Console.WriteLine($"      - {prop.Name}: '{value}' (Type: {prop.PropertyType.Name})");
                    }
                }

                Console.WriteLine($"\n📊 JSON SERIALIZATION TEST:");
                var jsonTest = JsonConvert.SerializeObject(quyMoDataForViewBag, Formatting.Indented);
                Console.WriteLine($"   - JSON Result: {jsonTest}");

                Console.WriteLine($"\n🎯 FRONTEND EXPECTED FORMAT:");
                Console.WriteLine($"   - Frontend needs: [{{QuyMo: 'label', SoLuong: number, MoTa: 'description'}}]");
                Console.WriteLine($"   - Current format: [{{see above}}]");

                // Test what frontend actually gets
                var frontendFormat = quyMoDataForViewBag.Select(x =>
                {
                    var item = (dynamic)x;
                    return new
                    {
                        QuyMo = item.QuyMo?.ToString() ?? "N/A",
                        SoLuong = Convert.ToInt32(item.SoLuong),
                        MoTa = item.MoTa?.ToString() ?? "N/A"
                    };
                }).ToList();

                Console.WriteLine($"\n🔧 FRONTEND COMPATIBLE FORMAT:");
                var frontendJson = JsonConvert.SerializeObject(frontendFormat, Formatting.Indented);
                Console.WriteLine($"   - Frontend JSON: {frontendJson}");
            }
            else
            {
                Console.WriteLine($"❌ CRITICAL: NO QUY MO DATA FOR VIEWBAG!");
                Console.WriteLine($"   - This means chart will be empty");
                Console.WriteLine($"   - Check CalculateCompanySizeData method");
            }

            ViewBag.QuyMoData = quyMoDataForViewBag;
            Console.WriteLine($"✅ ViewBag.QuyMoData assigned with {quyMoDataForViewBag.Count} items");

            // Add TrendData for revenue/profit trend chart with ENHANCED DEBUG
            Console.WriteLine($"🔍 VIEWBAG TREND DATA CREATION:");
            Console.WriteLine($"   - stats.Years.Count: {stats.Years.Count}");
            Console.WriteLine($"   - stats.RevenueData.Count: {stats.RevenueData.Count}");
            Console.WriteLine($"   - stats.ProfitData.Count: {stats.ProfitData.Count}");

            if (stats.Years.Count > 0)
            {
                Console.WriteLine($"   - Years: [{string.Join(", ", stats.Years)}]");
                Console.WriteLine($"   - Revenue sample: [{string.Join(", ", stats.RevenueData.Take(3).Select(x => $"{x:N0}"))}]");
                Console.WriteLine($"   - Profit sample: [{string.Join(", ", stats.ProfitData.Take(3).Select(x => $"{x:N0}"))}]");
            }
            else
            {
                Console.WriteLine($"❌ CRITICAL: NO YEARS DATA FOR TREND CHART!");
                Console.WriteLine($"   - This means trend chart will be empty");
                Console.WriteLine($"   - Check CalculateRevenueData function output");
            }

            var trendData = new List<object>();
            for (int i = 0; i < stats.Years.Count; i++)
            {
                var revenueValue = i < stats.RevenueData.Count ? (decimal)stats.RevenueData[i] : 0m;
                var profitValue = i < stats.ProfitData.Count ? (decimal)stats.ProfitData[i] : 0m;

                var trendItem = new
                {
                    Nam = stats.Years[i],
                    SR_Doanhthu_Thuan_BH_CCDV = revenueValue,
                    SR_Loinhuan_TruocThue = profitValue
                };

                trendData.Add(trendItem);
                Console.WriteLine($"   - TrendData[{i}]: Year={stats.Years[i]}, Revenue={revenueValue:N0} triệu VND, Profit={profitValue:N0} triệu VND");
            }

            ViewBag.TrendData = trendData;
            Console.WriteLine($"✅ ViewBag.TrendData created with {trendData.Count} items");
            Console.WriteLine($"🔍 Final ViewBag.TrendData JSON: {JsonConvert.SerializeObject(trendData)}");

            // Add missing ViewBag properties that the view expects
            ViewBag.loaihinhData = JsonConvert.SerializeObject(stats.BusinessTypeData ?? new List<object>());
            ViewBag.VonNNTWData = JsonConvert.SerializeObject(new List<object>());
        }

        #endregion

        #region API Endpoints

        public async Task<IActionResult> GetDashboardSummary()
        {
            var allData = await GetCachedDataAsync();
            var stats = CalculateAllStatistics(allData);

            var summary = new
            {
                TotalCompanies = stats.TotalCompanies,
                TotalLabor = stats.TotalLabor,
                DigitalTech = new
                {
                    Internet = stats.BusinessTypeCounts.GetValueOrDefault("Có", 0),
                    Website = stats.BusinessTypeCounts.GetValueOrDefault("Có", 0),
                    Software = stats.BusinessTypeCounts.GetValueOrDefault("Có", 0)
                },
                Provinces = stats.ProvinceData.Count,
                Regions = stats.RegionData.Count,
                BusinessTypes = stats.BusinessTypeData.Count
            };

            return Json(summary);
        }

        #endregion

        #region Export Functions

        public async Task<IActionResult> ExportToExcel(string stt = "", List<string>? Nam = null,
            List<string>? MaTinh_Dieutra = null, List<string>? Masothue = null,
            List<string>? Loaihinhkte = null, List<string>? Vungkinhte = null)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var allData = await GetCachedDataAsync();
                var filteredData = ApplyFiltersOptimized(allData, stt, Nam, MaTinh_Dieutra, Masothue, Loaihinhkte, Vungkinhte);

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("DN_Data");

                // Headers
                var headers = new[] { "STT", "Tên DN", "Năm", "Địa chỉ", "Điện thoại", "Email" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Data
                for (int i = 0; i < filteredData.Count; i++)
                {
                    var row = i + 2;
                    var item = filteredData[i];
                    worksheet.Cells[row, 1].Value = item.STT;
                    worksheet.Cells[row, 2].Value = item.TenDN;
                    worksheet.Cells[row, 3].Value = item.Nam;
                    worksheet.Cells[row, 4].Value = item.Diachi;
                    worksheet.Cells[row, 5].Value = item.Dienthoai;
                    worksheet.Cells[row, 6].Value = item.Email;
                }

                // Send email in background
                var excelData = package.GetAsByteArray();
                _ = Task.Run(() => SendEmailWithAttachment(
                    "aug13hehe@gmail.com",
                    "CÔNG TY TNHH CI RESEARCH - FILE EXCEL EXPORT",
                    "Dữ liệu Excel đã được xuất thành công.",
                    excelData));

                TempData["SuccessMessage"] = "Xuất Excel thành công, vui lòng kiểm tra email.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất Excel: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private static void SendEmailWithAttachment(string toEmail, string subject, string body, byte[] attachmentData)
        {
            try
            {
                const string fromEmail = "huan220vn@gmail.com";
                const string fromPassword = "tctn ztgb yqfd ynmp";

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body
                };

                message.To.Add(new MailAddress(toEmail));
                message.Attachments.Add(new Attachment(new MemoryStream(attachmentData),
                    "Data_Ciresearch.xlsx",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));

                using var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(fromEmail, fromPassword),
                    EnableSsl = true
                };

                client.Send(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email Error: {ex.Message}");
            }
        }

        #endregion

        #region Business Type Name Shortening

        private static string ShortenBusinessTypeName(string businessTypeName)
        {
            if (string.IsNullOrEmpty(businessTypeName))
                return "N/A";

            // Tạo tên viết tắt cho các loại hình doanh nghiệp dài
            var shortenedNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Các loại hình cổ phần
                {"Công ty cổ phần có vốn Nhà nước <= 50%", "CP vốn NN ≤50%"},
                {"Công ty cổ phần không có vốn Nhà nước", "CP không vốn NN"},
                {"Công ty cổ phẩn, Công ty TNHH có vốn Nhà nước > 50%", "CP/TNHH vốn NN >50%"},
                {"Công ty cổ phần", "Cổ phần"},
                
                // Các loại hình TNHH
                {"Công ty trách nhiệm hữu hạn một thành viên", "TNHH 1TV"},
                {"Công ty trách nhiệm hữu hạn hai thành viên trở lên", "TNHH 2TV+"},
                {"Công ty TNHH một thành viên", "TNHH 1TV"},
                {"Công ty TNHH hai thành viên trở lên", "TNHH 2TV+"},
                {"Công ty TNHH", "TNHH"},
                
                // Các loại hình khác
                {"Doanh nghiệp tư nhân", "DN tư nhân"},
                {"Hộ kinh doanh cá thể", "Hộ KD cá thể"},
                {"Hợp tác xã", "HTX"},
                {"Liên hiệp hợp tác xã", "Liên hiệp HTX"},
                {"Doanh nghiệp nhà nước", "DN nhà nước"},
                {"Công ty nhà nước", "Công ty NN"},
                {"Tổng công ty nhà nước", "Tổng công ty NN"},
                
                // Các loại hình đầu tư nước ngoài
                {"Doanh nghiệp có vốn đầu tư nước ngoài", "DN vốn ngoại"},
                {"Công ty có vốn đầu tư nước ngoài", "Công ty vốn ngoại"},
                {"Doanh nghiệp 100% vốn nước ngoài", "DN 100% ngoại"},
                
                // Các loại hình khác
                {"Đơn vị sự nghiệp có thu", "ĐV sự nghiệp"},
                {"Tổ chức tín dụng", "Tổ chức TD"},
                {"Quỹ đầu tư", "Quỹ ĐT"}
            };

            // Kiểm tra xem có tên viết tắt không
            if (shortenedNames.TryGetValue(businessTypeName, out string shortName))
            {
                return shortName;
            }

            // Nếu không có trong dictionary, tự động rút gọn
            if (businessTypeName.Length > 20)
            {
                // Loại bỏ các từ thường gặp để rút gọn
                var shortened = businessTypeName
                    .Replace("Công ty ", "")
                    .Replace("Doanh nghiệp ", "DN ")
                    .Replace("trách nhiệm hữu hạn", "TNHH")
                    .Replace("cổ phần", "CP")
                    .Replace("một thành viên", "1TV")
                    .Replace("hai thành viên trở lên", "2TV+")
                    .Replace("có vốn", "vốn")
                    .Replace("Nhà nước", "NN")
                    .Replace("đầu tư nước ngoài", "ngoại")
                    .Replace("tư nhân", "TN");

                // Nếu vẫn dài, cắt bớt
                if (shortened.Length > 20)
                {
                    shortened = shortened.Substring(0, 17) + "...";
                }

                return shortened;
            }

            return businessTypeName;
        }

        #endregion

        #region Cache Management

        [HttpPost]
        public IActionResult ClearCache()
        {
            _cache.Remove(DATA_CACHE_KEY);
            _cache.Remove(SUMMARY_CACHE_KEY);
            return Json(new { success = true, message = "Cache cleared successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> ForceReloadAllData()
        {
            try
            {
                Console.WriteLine("🔄 FORCE RELOAD ALL DATA - Clearing all caches...");

                // Clear all caches
                _cache.Remove(DATA_CACHE_KEY);
                _cache.Remove(SUMMARY_CACHE_KEY);

                Console.WriteLine("🔄 Caches cleared, loading fresh data from database...");

                // Force reload fresh data
                var allData = await GetCachedDataAsync();

                Console.WriteLine($"✅ FORCE RELOAD COMPLETED!");
                Console.WriteLine($"📊 Total records loaded: {allData.Count:N0}");

                // Check data distribution by year
                var yearDistribution = allData
                    .Where(x => x.Nam.HasValue)
                    .GroupBy(x => x.Nam.Value)
                    .Select(g => new { Year = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Year)
                    .Take(10)
                    .ToList();

                Console.WriteLine($"📊 Data distribution by year (top 10):");
                foreach (var year in yearDistribution)
                {
                    Console.WriteLine($"   - Year {year.Year}: {year.Count:N0} records");
                }

                return Json(new
                {
                    success = true,
                    message = "✅ All data reloaded successfully from database",
                    totalRecords = allData.Count,
                    dataSource = "Real data from dn_all table - NO LIMITS",
                    yearDistribution = yearDistribution,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in force reload: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to reload data from database",
                    timestamp = DateTime.Now
                });
            }
        }

        #endregion

        private void InitializeEmptyViewBag()
        {
            // Basic stats
            ViewBag.TotalCompanies = 0;
            ViewBag.TotalLabor = 0;

            // Financial stats - Only set if not already set
            if (ViewBag.CompaniesWithRevenue == null) ViewBag.CompaniesWithRevenue = 0;
            if (ViewBag.CompaniesWithAssets == null) ViewBag.CompaniesWithAssets = 0;
            if (ViewBag.CompaniesWithProfit == null) ViewBag.CompaniesWithProfit = 0;
            if (ViewBag.TotalAssetsCK == null) ViewBag.TotalAssetsCK = 0m;
            if (ViewBag.CompaniesWithAssetsCK == null) ViewBag.CompaniesWithAssetsCK = 0;

            // Technology stats
            ViewBag.CoInternet = 0;
            ViewBag.CoWebsite = 0;
            ViewBag.CoPhanmem = 0;
            ViewBag.CoTudonghoa = 0;

            // Region stats - Only set if not already set
            if (ViewBag.MienBacCount == null) ViewBag.MienBacCount = 0;
            if (ViewBag.MienTrungCount == null) ViewBag.MienTrungCount = 0;
            if (ViewBag.MienNamCount == null) ViewBag.MienNamCount = 0;

            // Business type stats - Only set if not already set
            if (ViewBag.TopBusinessType1Name == null) ViewBag.TopBusinessType1Name = "N/A";
            if (ViewBag.TopBusinessType1Count == null) ViewBag.TopBusinessType1Count = 0;
            if (ViewBag.TopBusinessType2Name == null) ViewBag.TopBusinessType2Name = "N/A";
            if (ViewBag.TopBusinessType2Count == null) ViewBag.TopBusinessType2Count = 0;
            if (ViewBag.TopBusinessType3Name == null) ViewBag.TopBusinessType3Name = "N/A";
            if (ViewBag.TopBusinessType3Count == null) ViewBag.TopBusinessType3Count = 0;

            // Chart data
            ViewBag.VungKinhTeData = new List<object>();
            ViewBag.QuyMoData = new List<object>();
            ViewBag.RegionData = new List<object>();
            ViewBag.BusinessTypeData = new List<object>();
            ViewBag.IndustryData = new List<object>();
            ViewBag.CompanySizeData = new List<object>();
            ViewBag.ProvinceData = new List<object>();
            ViewBag.TrendData = new List<object>();

            // JSON data for charts
            ViewBag.RegionDataJson = "[]";
            ViewBag.BusinessTypeDataJson = "[]";
            ViewBag.IndustryDataJson = "[]";
            ViewBag.CompanySizeDataJson = "[]";
            ViewBag.ProvinceDataJson = "[]";
            ViewBag.RevenueDataJson = "[]";
            ViewBag.ProfitDataJson = "[]";
            ViewBag.loaihinhData = "[]";
            ViewBag.VonNNTWData = "[]";

            // Additional stats
            ViewBag.Years = new List<int>();
            ViewBag.RevenueData = new List<double>();
            ViewBag.ProfitData = new List<double>();
            ViewBag.RegionCounts = new Dictionary<string, int>();
            ViewBag.BusinessTypeCounts = new Dictionary<string, int>();
            ViewBag.FinancialStats = new Dictionary<string, decimal>();

            Console.WriteLine("✅ All ViewBag properties initialized with empty/default values");
        }

        public async Task<ActionResult> ViewRawData(string stt = "", List<string>? Nam = null,
            List<string>? MaTinh_Dieutra = null, List<string>? Masothue = null,
            List<string>? Loaihinhkte = null, List<string>? Vungkinhte = null)
        {
            try
            {
                // Only get summary statistics for the initial page load
                var allData = await GetCachedDataAsync();
                var filteredData = ApplyFiltersOptimized(allData, stt, Nam, MaTinh_Dieutra, Masothue, Loaihinhkte, Vungkinhte);

                // Create a lightweight model with just summary stats
                var summaryModel = new
                {
                    TotalRecords = filteredData.Count,
                    WithTaxCode = filteredData.Count(x => !string.IsNullOrEmpty(x.Masothue)),
                    WithEmail = filteredData.Count(x => !string.IsNullOrEmpty(x.Email)),
                    WithPhone = filteredData.Count(x => !string.IsNullOrEmpty(x.Dienthoai)),
                    HasData = filteredData.Any()
                };

                ViewBag.SummaryStats = summaryModel;

                // Return empty model for initial load - data will be loaded via AJAX
                return View(new List<QLKH>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ViewRawData error: {ex.Message}");
                ViewBag.Error = "Không thể kết nối database";
                return View(new List<QLKH>());
            }
        }



        #region Debug and Test Endpoints

        [HttpGet]
        public async Task<IActionResult> VerifyRealData()
        {
            try
            {
                Console.WriteLine("🔍 VERIFYING REAL DATA FROM DATABASE");
                Console.WriteLine($"🔍 Connection: {_connectionString}");

                var allData = await GetCachedDataAsync();
                var stats = CalculateAllStatistics(allData);

                var verificationResult = new
                {
                    DatabaseConnection = _connectionString,
                    TotalRecordsFromDatabase = allData.Count,

                    // Vùng Kinh Tế từ cột Vungkinhte
                    VungKinhTeStats = new
                    {
                        RecordsWithVungkinhte = allData.Count(x => !string.IsNullOrEmpty(x.Vungkinhte)),
                        UniqueVungkinhteValues = allData
                            .Where(x => !string.IsNullOrEmpty(x.Vungkinhte))
                            .GroupBy(x => x.Vungkinhte)
                            .Select(g => new { Region = g.Key, Count = g.Count() })
                            .OrderByDescending(x => x.Count)
                            .ToList(),
                        ProcessedRegionData = stats.RegionData
                    },

                    // Business Types từ cột Loaihinhkte
                    BusinessTypeStats = new
                    {
                        RecordsWithBusinessType = allData.Count(x => !string.IsNullOrEmpty(x.Loaihinhkte)),
                        UniqueBusinessTypes = allData
                            .Where(x => !string.IsNullOrEmpty(x.Loaihinhkte))
                            .GroupBy(x => x.Loaihinhkte)
                            .Select(g => new { Type = g.Key, Count = g.Count() })
                            .OrderByDescending(x => x.Count)
                            .ToList(),
                        ProcessedBusinessTypeData = stats.BusinessTypeData
                    },

                    // Financial Data thực tế
                    FinancialStats = new
                    {
                        RecordsWithRevenue = allData.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue),
                        RecordsWithAssets = allData.Count(x => x.Taisan_Tong_CK.HasValue),
                        RecordsWithProfit = allData.Count(x => x.SR_Loinhuan_TruocThue.HasValue),
                        TotalRevenue = allData.Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue).Sum(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value),
                        TotalAssets = allData.Where(x => x.Taisan_Tong_CK.HasValue).Sum(x => x.Taisan_Tong_CK.Value),
                        ProcessedFinancialStats = stats.FinancialStats
                    },

                    // Company Size Data thực tế
                    CompanySizeStats = new
                    {
                        ProcessedSizeData = stats.CompanySizeData,
                        RevenueDistribution = allData
                            .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0)
                            .Select(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value / 1000m) // Convert triệu to tỷ VND
                            .GroupBy(x =>
                                x <= 3 ? "Siêu nhỏ (≤ 3 tỷ)" :
                                x <= 50 ? "Nhỏ (3-50 tỷ)" :
                                x <= 300 ? "Vừa (50-300 tỷ)" : "Lớn (> 300 tỷ)")
                            .Select(g => new { Category = g.Key, Count = g.Count() })
                            .ToList()
                    },

                    Summary = new
                    {
                        Message = "✅ ALL DATA IS REAL FROM DATABASE - NO DEMO DATA",
                        DatabaseStatus = "CONNECTED",
                        DataSource = "admin_ciresearch.dn_all",
                        LastChecked = DateTime.Now
                    }
                };

                return Json(verificationResult);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Error = ex.Message,
                    Message = "❌ DATABASE CONNECTION FAILED",
                    DatabaseConnection = _connectionString,
                    LastChecked = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestData()
        {
            try
            {
                Console.WriteLine("🧪 Testing database connection and data...");

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                Console.WriteLine("✅ Database connection successful");

                // Test simple query
                using var cmd = new MySqlCommand("SELECT COUNT(*) FROM dn_all", conn);
                var count = await cmd.ExecuteScalarAsync();
                Console.WriteLine($"📊 Total records in dn_all: {count}");

                // Test sample data
                using var cmd2 = new MySqlCommand("SELECT STT, TenDN, Region, Vungkinhte, MaTinh_Dieutra, Loaihinhkte FROM dn_all LIMIT 10", conn);
                using var reader = await cmd2.ExecuteReaderAsync();

                var sampleData = new List<object>();
                while (await reader.ReadAsync())
                {
                    sampleData.Add(new
                    {
                        STT = reader.GetInt32("STT"),
                        TenDN = reader.IsDBNull("TenDN") ? "" : reader.GetString("TenDN"),
                        Region = reader.IsDBNull("Region") ? "" : reader.GetString("Region"),
                        Vungkinhte = reader.IsDBNull("Vungkinhte") ? "" : reader.GetString("Vungkinhte"),
                        MaTinh = reader.IsDBNull("MaTinh_Dieutra") ? "" : reader.GetString("MaTinh_Dieutra"),
                        Loaihinhkte = reader.IsDBNull("Loaihinhkte") ? "" : reader.GetString("Loaihinhkte")
                    });
                }

                Console.WriteLine($"📋 Sample data retrieved: {sampleData.Count} records");

                return Json(new
                {
                    success = true,
                    totalRecords = count,
                    sampleData = sampleData,
                    message = "Database connection and data retrieval successful"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database test failed: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "Database connection failed"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestChartData()
        {
            try
            {
                var allData = await GetCachedDataAsync();
                var stats = CalculateAllStatistics(allData);

                var result = new
                {
                    TotalRecords = allData.Count,
                    VungKinhTeData = stats.RegionData,
                    VungKinhTeRaw = allData
                        .Where(x => !string.IsNullOrEmpty(x.Vungkinhte))
                        .GroupBy(x => x.Vungkinhte)
                        .Select(g => new { Vungkinhte = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList(),
                    BusinessTypes = stats.BusinessTypeData,
                    ConnectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;",
                    DatabaseTable = "dn_all"
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestVungKinhTeChart()
        {
            try
            {
                var allData = await GetCachedDataAsync();
                Console.WriteLine($"🔍 Testing VungKinhTe Chart - Total records: {allData.Count}");

                // Test raw Vungkinhte data from database
                var vungKinhTeRaw = allData
                    .Where(x => !string.IsNullOrEmpty(x.Vungkinhte))
                    .GroupBy(x => x.Vungkinhte)
                    .Select(g => new { Vungkinhte = g.Key, SoLuong = g.Count() })
                    .OrderByDescending(x => x.SoLuong)
                    .ToList();

                Console.WriteLine($"🔍 Raw Vungkinhte data:");
                foreach (var item in vungKinhTeRaw)
                {
                    Console.WriteLine($"   - {item.Vungkinhte}: {item.SoLuong}");
                }

                var result = new
                {
                    DatabaseConnection = "✅ Connected to Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;",
                    TableUsed = "dn_all",
                    ColumnUsed = "Vungkinhte",
                    TotalRecords = allData.Count,
                    RecordsWithVungKinhTe = vungKinhTeRaw.Sum(x => x.SoLuong),
                    VungKinhTeData = vungKinhTeRaw,
                    StandardizedData = CalculateAllStatistics(allData).RegionData
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace,
                    DatabaseConnection = "❌ Failed to connect to Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestIndustryData()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                Console.WriteLine("✅ Database connected for industry test");

                // Test TEN_NGANH column existence and data  
                var columnExistsQuery = "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = 'admin_ciresearch' AND table_name = 'dn_all' AND column_name = 'TEN_NGANH'";
                using var cmd1 = new MySqlCommand(columnExistsQuery, conn);
                var columnExists = Convert.ToInt32(await cmd1.ExecuteScalarAsync()) > 0;

                Console.WriteLine($"🔍 Column TEN_NGANH exists in dn_all: {columnExists}");

                if (!columnExists)
                {
                    return Json(new
                    {
                        success = false,
                        message = "❌ Column TEN_NGANH does not exist in dn_all table",
                        connectionString = "Server=localhost;Database=admin_ciresearch;User=root;Password=***",
                        timestamp = DateTime.Now
                    });
                }

                // Test TEN_NGANH data
                var industryQuery = @"
                    SELECT TEN_NGANH, COUNT(*) as SoLuong 
                    FROM dn_all 
                    WHERE TEN_NGANH IS NOT NULL AND TEN_NGANH != '' 
                    GROUP BY TEN_NGANH 
                    ORDER BY COUNT(*) DESC 
                    LIMIT 20";

                using var cmd2 = new MySqlCommand(industryQuery, conn);
                using var reader = await cmd2.ExecuteReaderAsync();

                var industries = new List<object>();
                while (await reader.ReadAsync())
                {
                    industries.Add(new
                    {
                        TEN_NGANH = reader.GetString("TEN_NGANH"),
                        SoLuong = reader.GetInt32("SoLuong")
                    });
                }

                Console.WriteLine($"🔍 Found {industries.Count} industries in TEN_NGANH column");
                foreach (var industry in industries.Take(5))
                {
                    var ind = (dynamic)industry;
                    Console.WriteLine($"   - {ind.TEN_NGANH}: {ind.SoLuong} companies");
                }

                return Json(new
                {
                    success = true,
                    message = $"✅ Industry data test successful. Found {industries.Count} industries from TEN_NGANH column",
                    data = industries,
                    connectionString = "Server=localhost;Database=admin_ciresearch;User=root;Password=***",
                    database = "admin_ciresearch",
                    table = "dn_all",
                    column = "TEN_NGANH",
                    totalIndustries = industries.Count,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Industry test failed: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = $"❌ Industry data test failed: {ex.Message}",
                    connectionString = "Server=localhost;Database=admin_ciresearch;User=root;Password=***",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestCompanySizeData()
        {
            try
            {
                var allData = await GetCachedDataAsync();
                Console.WriteLine($"🧪 Testing Company Size Data - Total records: {allData.Count}");

                // Raw revenue data analysis
                var revenueRecords = allData
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0)
                    .ToList();

                Console.WriteLine($"🔍 Records with revenue > 0: {revenueRecords.Count}");

                // Detailed revenue analysis
                var revenueAnalysis = revenueRecords
                    .Select(x => new
                    {
                        TenDN = x.TenDN,
                        RevenueTrieuVND = x.SR_Doanhthu_Thuan_BH_CCDV.Value,
                        RevenueTyVND = x.SR_Doanhthu_Thuan_BH_CCDV.Value / 1000m,
                        Category = x.SR_Doanhthu_Thuan_BH_CCDV.Value / 1000m <= 3 ? "Siêu nhỏ" :
                                  x.SR_Doanhthu_Thuan_BH_CCDV.Value / 1000m <= 50 ? "Nhỏ" :
                                  x.SR_Doanhthu_Thuan_BH_CCDV.Value / 1000m <= 300 ? "Vừa" : "Lớn"
                    })
                    .OrderByDescending(x => x.RevenueTrieuVND)
                    .Take(10)
                    .ToList();

                // Calculate using the same method as the chart
                var companySizeData = CalculateCompanySizeData(allData);

                return Json(new
                {
                    success = true,
                    message = "✅ Company Size Data Test Successful",
                    totalRecords = allData.Count,
                    recordsWithRevenue = revenueRecords.Count,
                    top10RevenueCompanies = revenueAnalysis,
                    companySizeDistribution = companySizeData,
                    databaseInfo = new
                    {
                        connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;",
                        table = "dn_all",
                        revenueColumn = "SR_Doanhthu_Thuan_BH_CCDV",
                        unit = "triệu VND"
                    },
                    categoryDefinitions = new
                    {
                        sieuNho = "Doanh thu ≤ 3 tỷ VND",
                        nho = "3 tỷ < Doanh thu ≤ 50 tỷ VND",
                        vua = "50 tỷ < Doanh thu ≤ 300 tỷ VND",
                        lon = "Doanh thu > 300 tỷ VND & Tài sản > 100 tỷ VND"
                    },
                    lastChecked = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Company Size test failed: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = $"❌ Company Size Data Test Failed: {ex.Message}",
                    error = ex.StackTrace,
                    lastChecked = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestViewBagData()
        {
            try
            {
                var allData = await GetCachedDataAsync();
                var stats = CalculateAllStatistics(allData);

                // Manually assign ViewBag like in the real controller
                ViewBag.QuyMoData = stats.CompanySizeData ?? new List<object>();

                var result = new
                {
                    success = true,
                    message = "✅ ViewBag Test Successful",
                    viewBagQuyMoData = ViewBag.QuyMoData,
                    viewBagQuyMoDataType = ViewBag.QuyMoData?.GetType().Name,
                    viewBagQuyMoDataCount = ViewBag.QuyMoData != null ? ((List<object>)ViewBag.QuyMoData).Count : 0,
                    rawCompanySizeData = stats.CompanySizeData,
                    jsonSerialized = JsonConvert.SerializeObject(ViewBag.QuyMoData),
                    htmlRawSerialized = JsonConvert.SerializeObject(ViewBag.QuyMoData ?? new List<object>()),
                    lastChecked = DateTime.Now
                };

                Console.WriteLine($"🔍 TEST ViewBag.QuyMoData:");
                Console.WriteLine($"   - ViewBag.QuyMoData: {JsonConvert.SerializeObject(ViewBag.QuyMoData)}");
                Console.WriteLine($"   - Html.Raw would output: {JsonConvert.SerializeObject(ViewBag.QuyMoData ?? new List<object>())}");

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"❌ ViewBag Test Failed: {ex.Message}",
                    error = ex.StackTrace,
                    lastChecked = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugRawTrendData()
        {
            try
            {
                Console.WriteLine("🔍 DEBUGGING RAW TREND DATA FROM DATABASE");

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // Check raw data in table
                var rawDataQuery = @"
                    SELECT STT, TenDN, Nam, SR_Doanhthu_Thuan_BH_CCDV, SR_Loinhuan_TruocThue 
                    FROM dn_all 
                    LIMIT 20";

                var rawData = new List<object>();
                using var cmd1 = new MySqlCommand(rawDataQuery, conn);
                using var reader1 = await cmd1.ExecuteReaderAsync();
                while (await reader1.ReadAsync())
                {
                    rawData.Add(new
                    {
                        STT = reader1.IsDBNull("STT") ? (int?)null : reader1.GetInt32("STT"),
                        TenDN = reader1.IsDBNull("TenDN") ? null : reader1.GetString("TenDN"),
                        Nam = reader1.IsDBNull("Nam") ? (int?)null : reader1.GetInt32("Nam"),
                        Revenue = reader1.IsDBNull("SR_Doanhthu_Thuan_BH_CCDV") ? (decimal?)null : reader1.GetDecimal("SR_Doanhthu_Thuan_BH_CCDV"),
                        Profit = reader1.IsDBNull("SR_Loinhuan_TruocThue") ? (decimal?)null : reader1.GetDecimal("SR_Loinhuan_TruocThue")
                    });
                }
                reader1.Close();

                // Check column statistics
                var statsQuery = @"
                    SELECT 
                        COUNT(*) as TotalRecords,
                        COUNT(Nam) as RecordsWithYear,
                        COUNT(SR_Doanhthu_Thuan_BH_CCDV) as RecordsWithRevenue,
                        COUNT(SR_Loinhuan_TruocThue) as RecordsWithProfit,
                        MIN(Nam) as MinYear,
                        MAX(Nam) as MaxYear,
                        SUM(SR_Doanhthu_Thuan_BH_CCDV) as TotalRevenue,
                        SUM(SR_Loinhuan_TruocThue) as TotalProfit
                    FROM dn_all";

                object dbStats = null;
                using var cmd2 = new MySqlCommand(statsQuery, conn);
                using var reader2 = await cmd2.ExecuteReaderAsync();
                if (await reader2.ReadAsync())
                {
                    dbStats = new
                    {
                        TotalRecords = reader2.GetInt32("TotalRecords"),
                        RecordsWithYear = reader2.GetInt32("RecordsWithYear"),
                        RecordsWithRevenue = reader2.GetInt32("RecordsWithRevenue"),
                        RecordsWithProfit = reader2.GetInt32("RecordsWithProfit"),
                        MinYear = reader2.IsDBNull("MinYear") ? (int?)null : reader2.GetInt32("MinYear"),
                        MaxYear = reader2.IsDBNull("MaxYear") ? (int?)null : reader2.GetInt32("MaxYear"),
                        TotalRevenue = reader2.IsDBNull("TotalRevenue") ? (decimal?)null : reader2.GetDecimal("TotalRevenue"),
                        TotalProfit = reader2.IsDBNull("TotalProfit") ? (decimal?)null : reader2.GetDecimal("TotalProfit")
                    };
                }
                reader2.Close();

                // Check records with all 3 fields
                var validRecordsQuery = @"
                    SELECT Nam, COUNT(*) as Count, 
                           SUM(SR_Doanhthu_Thuan_BH_CCDV) as YearRevenue,
                           SUM(SR_Loinhuan_TruocThue) as YearProfit
                    FROM dn_all 
                    WHERE Nam IS NOT NULL 
                      AND SR_Doanhthu_Thuan_BH_CCDV IS NOT NULL 
                      AND SR_Loinhuan_TruocThue IS NOT NULL
                    GROUP BY Nam 
                    ORDER BY Nam";

                var validRecords = new List<object>();
                using var cmd3 = new MySqlCommand(validRecordsQuery, conn);
                using var reader3 = await cmd3.ExecuteReaderAsync();
                while (await reader3.ReadAsync())
                {
                    validRecords.Add(new
                    {
                        Year = reader3.GetInt32("Nam"),
                        Count = reader3.GetInt32("Count"),
                        Revenue = reader3.GetDecimal("YearRevenue"),
                        Profit = reader3.GetDecimal("YearProfit")
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "✅ Raw Data Debug Complete",

                    database = "admin_ciresearch",
                    table = "dn_all",
                    connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;",

                    rawDataSample = rawData,
                    databaseStatistics = dbStats,
                    validRecordsByYear = validRecords,

                    issues = new
                    {
                        noValidRecords = validRecords.Count == 0,
                        noYearData = dbStats != null && ((dynamic)dbStats).RecordsWithYear == 0,
                        noRevenueData = dbStats != null && ((dynamic)dbStats).RecordsWithRevenue == 0,
                        noProfitData = dbStats != null && ((dynamic)dbStats).RecordsWithProfit == 0
                    },

                    recommendations = validRecords.Count == 0 ?
                        new[] {
                            "Check if dn_all table has data",
                            "Verify Nam column contains valid years",
                            "Verify SR_Doanhthu_Thuan_BH_CCDV has numeric values",
                            "Verify SR_Loinhuan_TruocThue has numeric values"
                        } :
                        new[] { "Data looks good - check controller processing" },

                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"❌ Raw Data Debug FAILED: {ex.Message}",
                    error = ex.StackTrace,
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestTrendData()
        {
            try
            {
                Console.WriteLine("🧪 Testing Trend Data from Database...");

                var allData = await GetCachedDataAsync();
                Console.WriteLine($"🔍 Total records loaded: {allData.Count}");

                // Test trend data calculation
                var stats = CalculateAllStatistics(allData);

                // Additional specific trend data query for verification
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var trendQuery = @"
                    SELECT Nam, 
                           COUNT(*) as CompanyCount,
                           SUM(SR_Doanhthu_Thuan_BH_CCDV) as TotalRevenue,
                           SUM(SR_Loinhuan_TruocThue) as TotalProfit,
                           AVG(SR_Doanhthu_Thuan_BH_CCDV) as AvgRevenue,
                           AVG(SR_Loinhuan_TruocThue) as AvgProfit
                    FROM dn_all 
                    WHERE Nam IS NOT NULL 
                      AND SR_Doanhthu_Thuan_BH_CCDV IS NOT NULL 
                      AND SR_Loinhuan_TruocThue IS NOT NULL
                      AND Nam BETWEEN 2010 AND 2030
                    GROUP BY Nam 
                    ORDER BY Nam";

                var directTrendData = new List<object>();
                using var cmd = new MySqlCommand(trendQuery, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    directTrendData.Add(new
                    {
                        Year = reader.GetInt32("Nam"),
                        CompanyCount = reader.GetInt32("CompanyCount"),
                        TotalRevenue = reader.GetDecimal("TotalRevenue"),
                        TotalProfit = reader.GetDecimal("TotalProfit"),
                        AvgRevenue = reader.GetDecimal("AvgRevenue"),
                        AvgProfit = reader.GetDecimal("AvgProfit")
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "✅ Trend Data Test SUCCESSFUL",
                    database = "admin_ciresearch",
                    table = "dn_all",
                    columns = new { year = "Nam", revenue = "SR_Doanhthu_Thuan_BH_CCDV", profit = "SR_Loinhuan_TruocThue" },

                    // From controller calculation
                    calculatedData = new
                    {
                        years = stats.Years,
                        revenueData = stats.RevenueData,
                        profitData = stats.ProfitData,
                        totalYears = stats.Years.Count
                    },

                    // Direct database query
                    directDatabaseQuery = directTrendData,

                    // ViewBag data
                    viewBagTrendData = ViewBag.TrendData,

                    summary = new
                    {
                        totalRecords = allData.Count,
                        recordsWithTrendData = allData.Count(x => x.Nam.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Loinhuan_TruocThue.HasValue),
                        yearsAvailable = stats.Years.Count,
                        dataSource = "REAL database data from admin_ciresearch.dn_all",
                        confirmRealData = "✅ Chart uses actual data from Nam, SR_Doanhthu_Thuan_BH_CCDV, SR_Loinhuan_TruocThue columns"
                    },

                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Trend test failed: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = $"❌ Trend Data Test FAILED: {ex.Message}",
                    connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                // Use the existing TestDatabaseConnectionAsync method
                var connectionTest = await TestDatabaseConnectionAsync();

                return Json(new
                {
                    success = connectionTest.IsConnected,
                    DatabaseConnected = connectionTest.IsConnected,
                    message = connectionTest.Message,
                    details = connectionTest.Details,
                    connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TestDatabaseConnection error: {ex.Message}");
                return Json(new
                {
                    success = false,
                    DatabaseConnected = false,
                    message = "❌ Lỗi kiểm tra kết nối database!",
                    error = ex.Message,
                    details = $"Lỗi chi tiết: {ex.Message}",
                    connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugTrendProcessing()
        {
            try
            {
                Console.WriteLine("🔍 DEBUG TREND PROCESSING - STEP BY STEP");

                // Clear cache to force fresh data
                _cache.Remove(DATA_CACHE_KEY);

                // Load fresh data
                var allData = await GetCachedDataAsync();
                Console.WriteLine($"🔍 Total records loaded: {allData.Count}");

                // Check ALL Nam values first
                var allNamValues = allData
                    .Where(x => x.Nam.HasValue)
                    .Select(x => x.Nam.Value)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                Console.WriteLine($"🔍 ALL DISTINCT Nam values: [{string.Join(", ", allNamValues)}]");

                // Check each year individually
                var yearAnalysis = new List<object>();
                foreach (var year in allNamValues)
                {
                    var yearRecords = allData.Where(x => x.Nam == year).ToList();
                    var hasRevenue = yearRecords.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue);
                    var hasProfit = yearRecords.Count(x => x.SR_Loinhuan_TruocThue.HasValue);
                    var hasBoth = yearRecords.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Loinhuan_TruocThue.HasValue);

                    yearAnalysis.Add(new
                    {
                        Year = year,
                        TotalRecords = yearRecords.Count,
                        HasRevenue = hasRevenue,
                        HasProfit = hasProfit,
                        HasBoth = hasBoth,
                        SampleRecords = yearRecords.Take(3).Select(x => new
                        {
                            STT = x.STT,
                            TenDN = x.TenDN,
                            Nam = x.Nam,
                            Revenue = x.SR_Doanhthu_Thuan_BH_CCDV,
                            Profit = x.SR_Loinhuan_TruocThue
                        }).ToList()
                    });

                    Console.WriteLine($"📊 Year {year}: Total={yearRecords.Count}, Revenue={hasRevenue}, Profit={hasProfit}, Both={hasBoth}");
                }

                // Now run the actual calculation
                var stats = new ComprehensiveStats();
                CalculateRevenueData(allData, stats);

                return Json(new
                {
                    success = true,
                    message = "✅ Trend Processing Debug Complete",

                    totalRecords = allData.Count,
                    allYearsFound = allNamValues,
                    yearAnalysis = yearAnalysis,

                    processingResults = new
                    {
                        finalYears = stats.Years,
                        finalRevenueData = stats.RevenueData,
                        finalProfitData = stats.ProfitData
                    },

                    troubleshooting = new
                    {
                        expectedYears = new[] { 2020, 2023 },
                        actualYears = stats.Years,
                        missingYears = new[] { 2020, 2023 }.Except(stats.Years).ToList(),
                        unexpectedYears = stats.Years.Except(new[] { 2020, 2023 }).ToList()
                    },

                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"❌ Trend Processing Debug FAILED: {ex.Message}",
                    error = ex.StackTrace,
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestUniqueCompaniesAndLaborCount()
        {
            try
            {
                var allData = await GetCachedDataAsync();

                // Test with different years
                var testResults = new List<object>();
                var availableYears = allData.Where(x => x.Nam.HasValue).Select(x => x.Nam.Value).Distinct().OrderByDescending(x => x).ToList();

                foreach (var year in availableYears.Take(3)) // Test top 3 years
                {
                    var yearData = allData.Where(x => x.Nam == year).ToList();

                    // Count total records for this year
                    var totalRecords = yearData.Count;

                    // Count records with Masothue
                    var recordsWithTax = yearData.Count(x => !string.IsNullOrEmpty(x.Masothue));

                    // Get unique companies by Masothue
                    var uniqueCompaniesInYear = yearData
                        .Where(x => !string.IsNullOrEmpty(x.Masothue))
                        .GroupBy(x => x.Masothue)
                        .Select(g => g.First()) // Take one record per unique company
                        .ToList();

                    var uniqueCompaniesCount = uniqueCompaniesInYear.Count;

                    // Calculate total labor for unique companies in this year
                    var totalLabor = uniqueCompaniesInYear.Sum(x => (long)(x.SoLaodong_CuoiNam ?? 0));
                    var companiesWithLabor = uniqueCompaniesInYear.Count(x => x.SoLaodong_CuoiNam.HasValue && x.SoLaodong_CuoiNam.Value > 0);

                    // Sample duplicate companies
                    var duplicates = yearData
                        .Where(x => !string.IsNullOrEmpty(x.Masothue))
                        .GroupBy(x => x.Masothue)
                        .Where(g => g.Count() > 1)
                        .Take(3)
                        .Select(g => new
                        {
                            Masothue = g.Key,
                            Company = g.First().TenDN,
                            DuplicateCount = g.Count()
                        })
                        .ToList();

                    // Sample companies with labor data
                    var sampleLabor = uniqueCompaniesInYear
                        .Where(x => x.SoLaodong_CuoiNam.HasValue && x.SoLaodong_CuoiNam.Value > 0)
                        .OrderByDescending(x => x.SoLaodong_CuoiNam)
                        .Take(3)
                        .Select(x => new
                        {
                            Company = x.TenDN,
                            Masothue = x.Masothue,
                            LaborCount = x.SoLaodong_CuoiNam.Value
                        })
                        .ToList();

                    // Regional distribution for unique companies in this year
                    var regionalDistribution = uniqueCompaniesInYear
                        .Where(x => !string.IsNullOrEmpty(x.Vungkinhte) || !string.IsNullOrEmpty(x.Region))
                        .GroupBy(x => x.Vungkinhte ?? x.Region ?? "Khác")
                        .Select(g => new
                        {
                            Region = g.Key,
                            Count = g.Count(),
                            SampleCompanies = g.Take(2).Select(c => new
                            {
                                Company = c.TenDN,
                                Masothue = c.Masothue,
                                Vungkinhte = c.Vungkinhte,
                                Region = c.Region
                            }).ToList()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToList();

                    // Calculate total companies with regional data
                    var totalWithRegion = regionalDistribution.Sum(x => x.Count);

                    testResults.Add(new
                    {
                        Year = year,
                        TotalRecords = totalRecords,
                        RecordsWithTax = recordsWithTax,
                        UniqueCompanies = uniqueCompaniesCount,
                        DuplicatesRemoved = recordsWithTax - uniqueCompaniesCount,
                        SampleDuplicates = duplicates,
                        // Labor data
                        TotalLabor = totalLabor,
                        CompaniesWithLabor = companiesWithLabor,
                        SampleLaborData = sampleLabor,
                        // Regional data
                        CompaniesWithRegion = totalWithRegion,
                        RegionalDistribution = regionalDistribution
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "✅ Unique Companies and Labor Count Test Completed",
                    totalRecordsInDatabase = allData.Count,
                    availableYears = availableYears,
                    testResults = testResults,
                    currentLogic = "Fixed: Count unique companies and calculate total labor by Masothue per year, not across all years",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Unique Companies and Labor Count Test Failed",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestViewBagTrendData()
        {
            try
            {
                var allData = await GetCachedDataAsync();
                var stats = CalculateAllStatistics(allData);

                // Simulate what AssignStatsToViewBag does for TrendData
                var trendData = new List<object>();
                for (int i = 0; i < stats.Years.Count; i++)
                {
                    var revenueValue = i < stats.RevenueData.Count ? (decimal)stats.RevenueData[i] : 0m;
                    var profitValue = i < stats.ProfitData.Count ? (decimal)stats.ProfitData[i] : 0m;

                    var trendItem = new
                    {
                        Nam = stats.Years[i],
                        SR_Doanhthu_Thuan_BH_CCDV = revenueValue,
                        SR_Loinhuan_TruocThue = profitValue
                    };

                    trendData.Add(trendItem);
                }

                // Test JSON serialization like the view does
                var jsonSerialized = JsonConvert.SerializeObject(trendData);

                return Json(new
                {
                    success = true,
                    message = "✅ ViewBag.TrendData Test Successful",
                    database = "admin_ciresearch",
                    table = "dn_all",
                    columns = new { year = "Nam", revenue = "SR_Doanhthu_Thuan_BH_CCDV", profit = "SR_Loinhuan_TruocThue" },
                    rawStatsData = new
                    {
                        years = stats.Years,
                        revenueData = stats.RevenueData,
                        profitData = stats.ProfitData
                    },
                    processedTrendData = trendData,
                    jsonSerialized = jsonSerialized,
                    htmlRawSerialized = System.Web.HttpUtility.HtmlEncode(jsonSerialized),
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ ViewBag.TrendData Test Failed",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTrendChartData()
        {
            try
            {
                Console.WriteLine("🧪 GetTrendChartData: Processing request for Force Chart format...");

                var allData = await GetCachedDataAsync();
                var stats = CalculateAllStatistics(allData);

                Console.WriteLine($"📊 Found {stats.Years.Count} years: [{string.Join(", ", stats.Years)}]");
                Console.WriteLine($"📊 Revenue data points: {stats.RevenueData.Count}");
                Console.WriteLine($"📊 Profit data points: {stats.ProfitData.Count}");

                // Convert years to string labels
                var labels = stats.Years.Select(y => y.ToString()).ToList();

                // Convert data from triệu VND to tỷ VND and apply extreme value handling
                var revenueData = stats.RevenueData.Select(r => Math.Round(r / 1000, 2)).ToList();

                var profitData = stats.ProfitData.Select(p =>
                {
                    var profitInBillion = p / 1000; // Convert to billion VND

                    // Log extreme values but don't cap them - show real data
                    if (Math.Abs(profitInBillion) > 100000)
                    {
                        Console.WriteLine($"📊 Large value detected: {profitInBillion:N2} tỷ VND - showing real data");
                    }

                    return Math.Round(profitInBillion, 2);
                }).ToList();

                Console.WriteLine($"📊 Processed revenue data (tỷ VND): [{string.Join(", ", revenueData)}]");
                Console.WriteLine($"📊 Processed profit data (tỷ VND): [{string.Join(", ", profitData)}]");

                // Return Chart.js compatible format exactly like Force Chart
                var chartData = new
                {
                    success = true,
                    data = new
                    {
                        labels = labels,
                        datasets = new object[]
                        {
                            new
                            {
                                label = "Doanh thu (tỷ VND)",
                                data = revenueData,
                                borderColor = "#28a745",
                                backgroundColor = "rgba(40, 167, 69, 0.1)",
                                borderWidth = 3,
                                fill = false,
                                tension = 0.4,
                                pointBackgroundColor = "#28a745",
                                pointBorderColor = "#ffffff",
                                pointBorderWidth = 2,
                                pointRadius = 6,
                                pointHoverRadius = 8
                            },
                            new
                            {
                                label = "Lợi nhuận (tỷ VND)",
                                data = profitData,
                                borderColor = "#fd7e14",
                                backgroundColor = "rgba(253, 126, 20, 0.1)",
                                borderWidth = 3,
                                fill = false,
                                tension = 0.4,
                                pointBackgroundColor = "#fd7e14",
                                pointBorderColor = "#ffffff",
                                pointBorderWidth = 2,
                                pointRadius = 6,
                                pointHoverRadius = 8
                            }
                        }
                    },
                    metadata = new
                    {
                        database = "admin_ciresearch",
                        table = "dn_all",
                        totalRecords = allData.Count,
                        years = stats.Years.Count,
                        message = "✅ Real data from database in Chart.js format",
                        timestamp = DateTime.Now,
                        dataSource = "Real database: Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;"
                    }
                };

                Console.WriteLine("✅ Chart data prepared successfully in Force Chart format");
                return Json(chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetTrendChartData: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = $"❌ Failed to get trend chart data: {ex.Message}",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerifyIndustryData()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 1. Check if TEN_NGANH column exists
                var columnCheckQuery = @"
                    SELECT COUNT(*) 
                    FROM information_schema.columns 
                    WHERE table_schema = 'admin_ciresearch' 
                    AND table_name = 'dn_all' 
                    AND column_name = 'TEN_NGANH'";
                using var cmd1 = new MySqlCommand(columnCheckQuery, conn);
                var columnExists = Convert.ToInt32(await cmd1.ExecuteScalarAsync()) > 0;

                if (!columnExists)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Column TEN_NGANH does not exist in dn_all table",
                        database = "admin_ciresearch",
                        table = "dn_all"
                    });
                }

                // 2. Get data quality metrics
                var qualityQuery = @"
                    SELECT 
                        COUNT(*) as total_records,
                        COUNT(TEN_NGANH) as non_null_count,
                        COUNT(CASE WHEN TEN_NGANH = '' THEN 1 END) as empty_count,
                        COUNT(CASE WHEN TEN_NGANH IS NOT NULL AND TEN_NGANH != '' THEN 1 END) as valid_count
                    FROM dn_all";
                using var cmd2 = new MySqlCommand(qualityQuery, conn);
                using var reader2 = await cmd2.ExecuteReaderAsync();

                object qualityStats = null;
                if (await reader2.ReadAsync())
                {
                    qualityStats = new
                    {
                        totalRecords = reader2.GetInt32("total_records"),
                        nonNullCount = reader2.GetInt32("non_null_count"),
                        emptyCount = reader2.GetInt32("empty_count"),
                        validCount = reader2.GetInt32("valid_count")
                    };
                }
                reader2.Close();

                // 3. Get top industries
                var topIndustriesQuery = @"
                    SELECT TEN_NGANH, COUNT(*) as count
                    FROM dn_all
                    WHERE TEN_NGANH IS NOT NULL AND TEN_NGANH != ''
                    GROUP BY TEN_NGANH
                    ORDER BY count DESC
                    LIMIT 20";
                using var cmd3 = new MySqlCommand(topIndustriesQuery, conn);
                using var reader3 = await cmd3.ExecuteReaderAsync();

                var industries = new List<object>();
                while (await reader3.ReadAsync())
                {
                    industries.Add(new
                    {
                        industry = reader3.GetString("TEN_NGANH"),
                        count = reader3.GetInt32("count")
                    });
                }
                reader3.Close();

                // 4. Get sample records
                var sampleQuery = @"
                    SELECT STT, TenDN, TEN_NGANH, MaNganhC5_Chinh, Loaihinhkte
                    FROM dn_all
                    WHERE TEN_NGANH IS NOT NULL AND TEN_NGANH != ''
                    LIMIT 5";
                using var cmd4 = new MySqlCommand(sampleQuery, conn);
                using var reader4 = await cmd4.ExecuteReaderAsync();

                var sampleRecords = new List<object>();
                while (await reader4.ReadAsync())
                {
                    sampleRecords.Add(new
                    {
                        stt = reader4.GetInt32("STT"),
                        company = reader4.GetString("TenDN"),
                        industry = reader4.GetString("TEN_NGANH"),
                        industryCode = reader4.IsDBNull("MaNganhC5_Chinh") ? null : reader4.GetString("MaNganhC5_Chinh"),
                        businessType = reader4.IsDBNull("Loaihinhkte") ? null : reader4.GetString("Loaihinhkte")
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Industry data verification completed",
                    database = "admin_ciresearch",
                    table = "dn_all",
                    column = "TEN_NGANH",
                    dataQuality = qualityStats,
                    topIndustries = industries,
                    sampleRecords = sampleRecords,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error verifying industry data: {ex.Message}",
                    error = ex.StackTrace,
                    database = "admin_ciresearch",
                    table = "dn_all",
                    timestamp = DateTime.Now
                });
            }
        }

        #endregion

        #region Server-Side Pagination API

        [HttpPost]
        public async Task<IActionResult> GetPaginatedData()
        {
            try
            {
                // Get DataTables parameters from request
                var draw = int.Parse(Request.Form["draw"]);
                var start = int.Parse(Request.Form["start"]);
                var length = int.Parse(Request.Form["length"]);
                var searchValue = Request.Form["search[value]"];
                var sortColumn = int.Parse(Request.Form["order[0][column]"]);
                var sortDirection = Request.Form["order[0][dir]"];

                Console.WriteLine($"🔍 Pagination request: draw={draw}, start={start}, length={length}, search='{searchValue}'");

                // Get filtered data count first for performance
                var allData = await GetCachedDataAsync();

                // Apply search filter
                var filteredData = allData.AsQueryable();
                if (!string.IsNullOrEmpty(searchValue))
                {
                    filteredData = filteredData.Where(x =>
                        (!string.IsNullOrEmpty(x.TenDN) && x.TenDN.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.Masothue) && x.Masothue.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.MaTinh_Dieutra) && x.MaTinh_Dieutra.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.Loaihinhkte) && x.Loaihinhkte.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.Diachi) && x.Diachi.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.Email) && x.Email.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.Dienthoai) && x.Dienthoai.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                    );
                }

                var totalFiltered = filteredData.Count();

                // Apply sorting
                switch (sortColumn)
                {
                    case 0: // STT
                        filteredData = sortDirection == "asc" ? filteredData.OrderBy(x => x.STT) : filteredData.OrderByDescending(x => x.STT);
                        break;
                    case 1: // Nam
                        filteredData = sortDirection == "asc" ? filteredData.OrderBy(x => x.Nam) : filteredData.OrderByDescending(x => x.Nam);
                        break;
                    case 2: // Masothue
                        filteredData = sortDirection == "asc" ? filteredData.OrderBy(x => x.Masothue) : filteredData.OrderByDescending(x => x.Masothue);
                        break;
                    case 3: // TenDN
                        filteredData = sortDirection == "asc" ? filteredData.OrderBy(x => x.TenDN) : filteredData.OrderByDescending(x => x.TenDN);
                        break;
                    case 4: // Loaihinhkte
                        filteredData = sortDirection == "asc" ? filteredData.OrderBy(x => x.Loaihinhkte) : filteredData.OrderByDescending(x => x.Loaihinhkte);
                        break;
                    case 5: // MaTinh_Dieutra
                        filteredData = sortDirection == "asc" ? filteredData.OrderBy(x => x.MaTinh_Dieutra) : filteredData.OrderByDescending(x => x.MaTinh_Dieutra);
                        break;
                    default:
                        filteredData = filteredData.OrderBy(x => x.STT);
                        break;
                }

                // Apply pagination
                var pagedData = filteredData
                    .Skip(start)
                    .Take(length)
                    .Select(x => new
                    {
                        STT = x.STT,
                        Nam = x.Nam,
                        Masothue = x.Masothue ?? "N/A",
                        TenDN = x.TenDN ?? "N/A",
                        Loaihinhkte = x.Loaihinhkte ?? "N/A",
                        MaTinh_Dieutra = x.MaTinh_Dieutra ?? "N/A",
                        MaHuyen_Dieutra = x.MaHuyen_Dieutra ?? "N/A",
                        MaXa_Dieutra = x.MaXa_Dieutra ?? "N/A",
                        Diachi = x.Diachi ?? "N/A",
                        Dienthoai = x.Dienthoai ?? "N/A",
                        Email = x.Email ?? "N/A",
                        Region = x.Region ?? "N/A"
                    })
                    .ToList();

                Console.WriteLine($"✅ Sample record structure:");
                if (pagedData.Count > 0)
                {
                    var sample = pagedData[0];
                    var properties = sample.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        Console.WriteLine($"   - {prop.Name}: {prop.GetValue(sample)}");
                    }
                }

                Console.WriteLine($"✅ Returning {pagedData.Count} records out of {totalFiltered} filtered from {allData.Count} total");

                // Return DataTables format
                return Json(new
                {
                    draw = draw,
                    recordsTotal = allData.Count,
                    recordsFiltered = totalFiltered,
                    data = pagedData
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Pagination error: {ex.Message}");
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

        [HttpGet]
        public async Task<IActionResult> GetDataSummary()
        {
            try
            {
                // Check cache first for summary data
                if (_cache.TryGetValue(SUMMARY_CACHE_KEY, out object? cachedSummary) && cachedSummary != null)
                {
                    Console.WriteLine("✅ Using cached summary data");
                    return Json(cachedSummary);
                }

                Console.WriteLine("🔍 Calculating fresh summary data...");
                var allData = await GetCachedDataAsync();

                var summaryData = new
                {
                    success = true,
                    totalRecords = allData.Count,
                    withTaxCode = allData.Count(x => !string.IsNullOrEmpty(x.Masothue)),
                    withEmail = allData.Count(x => !string.IsNullOrEmpty(x.Email)),
                    withPhone = allData.Count(x => !string.IsNullOrEmpty(x.Dienthoai)),
                    lastUpdated = DateTime.Now
                };

                // Cache summary data for longer duration
                var summaryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(SUMMARY_CACHE_DURATION_MINUTES))
                    .SetSize(1);
                _cache.Set(SUMMARY_CACHE_KEY, summaryData, summaryOptions);

                Console.WriteLine($"✅ Summary calculated and cached: {summaryData.totalRecords} records");
                return Json(summaryData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetDataSummary error: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        #endregion

        #region Debug Pagination API

        [HttpGet]
        public async Task<IActionResult> TestPaginationAPI()
        {
            try
            {
                Console.WriteLine("🧪 Testing Pagination API directly...");

                var allData = await GetCachedDataAsync();
                Console.WriteLine($"📊 Total data count: {allData.Count}");

                // Take first few records and show their structure
                var sampleData = allData.Take(3)
                    .Select(x => new
                    {
                        STT = x.STT,
                        Nam = x.Nam,
                        Masothue = x.Masothue ?? "N/A",
                        TenDN = x.TenDN ?? "N/A",
                        Loaihinhkte = x.Loaihinhkte ?? "N/A",
                        MaTinh_Dieutra = x.MaTinh_Dieutra ?? "N/A",
                        MaHuyen_Dieutra = x.MaHuyen_Dieutra ?? "N/A",
                        MaXa_Dieutra = x.MaXa_Dieutra ?? "N/A",
                        Diachi = x.Diachi ?? "N/A",
                        Dienthoai = x.Dienthoai ?? "N/A",
                        Email = x.Email ?? "N/A",
                        Region = x.Region ?? "N/A"
                    })
                    .ToList();

                Console.WriteLine("📊 Sample data structure:");
                foreach (var item in sampleData)
                {
                    Console.WriteLine($"   STT: {item.STT}, Nam: {item.Nam}, MaTinh_Dieutra: '{item.MaTinh_Dieutra}'");
                }

                return Json(new
                {
                    success = true,
                    message = "✅ Pagination API structure test",
                    totalRecords = allData.Count,
                    sampleData = sampleData,
                    dataTableFormat = new
                    {
                        draw = 1,
                        recordsTotal = allData.Count,
                        recordsFiltered = allData.Count,
                        data = sampleData
                    },
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test pagination API error: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> DebugRegionalData()
        {
            try
            {
                var allData = await GetCachedDataAsync();
                var currentYear = GetCurrentAnalysisYear(allData, null);

                Console.WriteLine($"🔍 DEBUG REGIONAL DATA - Total records: {allData.Count}");
                Console.WriteLine($"🔍 Current analysis year: {currentYear}");

                // Filter to current year and get unique companies
                var currentYearData = allData.Where(x => x.Nam == currentYear).ToList();
                var uniqueCompaniesInYear = currentYearData
                    .Where(x => !string.IsNullOrEmpty(x.Masothue))
                    .GroupBy(x => x.Masothue)
                    .Select(g => g.First())
                    .ToList();

                Console.WriteLine($"🔍 Records in year {currentYear}: {currentYearData.Count}");
                Console.WriteLine($"🔍 Unique companies in year: {uniqueCompaniesInYear.Count}");

                // Check data availability
                var withVungkinhte = uniqueCompaniesInYear.Count(x => !string.IsNullOrEmpty(x.Vungkinhte));
                var withRegion = uniqueCompaniesInYear.Count(x => !string.IsNullOrEmpty(x.Region));
                var withEither = uniqueCompaniesInYear.Count(x => !string.IsNullOrEmpty(x.Vungkinhte) || !string.IsNullOrEmpty(x.Region));

                Console.WriteLine($"🔍 Data availability:");
                Console.WriteLine($"   - With Vungkinhte: {withVungkinhte}");
                Console.WriteLine($"   - With Region: {withRegion}");
                Console.WriteLine($"   - With either: {withEither}");

                // Sample data from both fields
                var sampleData = uniqueCompaniesInYear.Take(10).Select(x => new
                {
                    Company = x.TenDN,
                    Masothue = x.Masothue,
                    Vungkinhte = x.Vungkinhte ?? "NULL",
                    Region = x.Region ?? "NULL",
                    HasVungkinhte = !string.IsNullOrEmpty(x.Vungkinhte),
                    HasRegion = !string.IsNullOrEmpty(x.Region)
                }).ToList();

                // Distribution by Vungkinhte
                var vungkinhteDistribution = uniqueCompaniesInYear
                    .Where(x => !string.IsNullOrEmpty(x.Vungkinhte))
                    .GroupBy(x => x.Vungkinhte)
                    .Select(g => new { Field = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                // Distribution by Region
                var regionDistribution = uniqueCompaniesInYear
                    .Where(x => !string.IsNullOrEmpty(x.Region))
                    .GroupBy(x => x.Region)
                    .Select(g => new { Field = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                // Combined distribution using fallback logic
                var combinedDistribution = uniqueCompaniesInYear
                    .Where(x => !string.IsNullOrEmpty(x.Vungkinhte) || !string.IsNullOrEmpty(x.Region))
                    .GroupBy(x => x.Vungkinhte ?? x.Region ?? "Khác")
                    .Select(g => new { Field = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                return Json(new
                {
                    success = true,
                    message = "✅ Regional data debug completed",
                    totalRecords = allData.Count,
                    currentYear = currentYear,
                    recordsInYear = currentYearData.Count,
                    uniqueCompanies = uniqueCompaniesInYear.Count,
                    dataAvailability = new
                    {
                        withVungkinhte = withVungkinhte,
                        withRegion = withRegion,
                        withEither = withEither
                    },
                    sampleData = sampleData,
                    distributions = new
                    {
                        byVungkinhte = vungkinhteDistribution,
                        byRegion = regionDistribution,
                        combined = combinedDistribution
                    },
                    recommendation = withVungkinhte == 0 && withRegion > 0 ?
                        "Use Region field as Vungkinhte is empty" :
                        withVungkinhte > 0 ?
                        "Use Vungkinhte field as primary source" :
                        "Both fields are empty - no regional data available",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Regional data debug failed",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestRegionalDataQuick()
        {
            try
            {
                var allData = await GetCachedDataAsync();
                var currentYear = GetCurrentAnalysisYear(allData, null);
                var currentYearData = allData.Where(x => x.Nam == currentYear).ToList();

                Console.WriteLine($"🔍 QUICK REGIONAL TEST - Year {currentYear}");
                Console.WriteLine($"   - Total records in year: {currentYearData.Count}");

                // Check raw field values
                var withVungkinhte = currentYearData.Count(x => !string.IsNullOrEmpty(x.Vungkinhte));
                var withRegion = currentYearData.Count(x => !string.IsNullOrEmpty(x.Region));

                Console.WriteLine($"   - Records with Vungkinhte: {withVungkinhte}");
                Console.WriteLine($"   - Records with Region: {withRegion}");

                // Sample actual values
                var sampleVungkinhte = currentYearData
                    .Where(x => !string.IsNullOrEmpty(x.Vungkinhte))
                    .Take(5)
                    .Select(x => new { Company = x.TenDN, Vungkinhte = x.Vungkinhte })
                    .ToList();

                var sampleRegion = currentYearData
                    .Where(x => !string.IsNullOrEmpty(x.Region))
                    .Take(5)
                    .Select(x => new { Company = x.TenDN, Region = x.Region })
                    .ToList();

                return Json(new
                {
                    success = true,
                    message = "✅ Quick regional test completed",
                    currentYear = currentYear,
                    totalRecords = currentYearData.Count,
                    withVungkinhte = withVungkinhte,
                    withRegion = withRegion,
                    sampleVungkinhte = sampleVungkinhte,
                    sampleRegion = sampleRegion,
                    hasAnyRegionalData = withVungkinhte > 0 || withRegion > 0,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestViewBagAssignment()
        {
            try
            {
                var allData = await GetCachedDataAsync();
                var stats = CalculateAllStatistics(allData);
                AssignStatsToViewBag(stats);

                return Json(new
                {
                    success = true,
                    message = "✅ ViewBag Assignment Test",
                    viewBagValues = new
                    {
                        MienBacCount = ViewBag.MienBacCount,
                        MienTrungCount = ViewBag.MienTrungCount,
                        MienNamCount = ViewBag.MienNamCount,
                        Total = (int)(ViewBag.MienBacCount ?? 0) + (int)(ViewBag.MienTrungCount ?? 0) + (int)(ViewBag.MienNamCount ?? 0)
                    },
                    regionCounts = stats.RegionCounts,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestBusinessTypeData()
        {
            try
            {
                var allData = await GetCachedDataAsync();
                var currentYear = GetCurrentAnalysisYear(allData, null);

                Console.WriteLine($"🔍 BUSINESS TYPE TEST - Total records: {allData.Count}");
                Console.WriteLine($"🔍 Current analysis year: {currentYear}");

                // Filter to current year and get unique companies
                var currentYearData = allData.Where(x => x.Nam == currentYear).ToList();
                var uniqueCompaniesInYear = currentYearData
                    .Where(x => !string.IsNullOrEmpty(x.Masothue))
                    .GroupBy(x => x.Masothue)
                    .Select(g => g.First())
                    .ToList();

                Console.WriteLine($"🔍 Records in year {currentYear}: {currentYearData.Count}");
                Console.WriteLine($"🔍 Unique companies in year: {uniqueCompaniesInYear.Count}");

                // Check business type data availability
                var withBusinessType = uniqueCompaniesInYear.Count(x => !string.IsNullOrEmpty(x.Loaihinhkte));

                Console.WriteLine($"🔍 Business type data availability:");
                Console.WriteLine($"   - With Loaihinhkte: {withBusinessType}");

                // Business type distribution
                var businessTypeDistribution = uniqueCompaniesInYear
                    .Where(x => !string.IsNullOrEmpty(x.Loaihinhkte))
                    .GroupBy(x => x.Loaihinhkte)
                    .Select(g => new { BusinessType = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                // Top 3 business types
                var top3BusinessTypes = businessTypeDistribution.Take(3).ToList();

                return Json(new
                {
                    success = true,
                    message = "✅ Business Type data test completed",
                    totalRecords = allData.Count,
                    currentYear = currentYear,
                    recordsInYear = currentYearData.Count,
                    uniqueCompanies = uniqueCompaniesInYear.Count,
                    dataAvailability = new
                    {
                        withBusinessType = withBusinessType
                    },
                    businessTypeDistribution = businessTypeDistribution,
                    top3BusinessTypes = top3BusinessTypes.Select((bt, index) => new
                    {
                        rank = index + 1,
                        originalName = bt.BusinessType,
                        shortName = ShortenBusinessTypeName(bt.BusinessType),
                        count = bt.Count
                    }).ToList(),
                    sampleCompanies = uniqueCompaniesInYear
                        .Where(x => !string.IsNullOrEmpty(x.Loaihinhkte))
                        .Take(5)
                        .Select(x => new
                        {
                            company = x.TenDN,
                            masothue = x.Masothue,
                            businessType = x.Loaihinhkte
                        }).ToList(),
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Business Type data test failed",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestFinancialData()
        {
            try
            {
                var allData = await GetCachedDataAsync();
                var currentYear = GetCurrentAnalysisYear(allData, null);

                Console.WriteLine($"🔍 FINANCIAL DATA TEST - Total records: {allData.Count}");
                Console.WriteLine($"🔍 Current analysis year: {currentYear}");

                // Filter to current year and get unique companies
                var currentYearData = allData.Where(x => x.Nam == currentYear).ToList();
                var uniqueCompaniesInYear = currentYearData
                    .Where(x => !string.IsNullOrEmpty(x.Masothue))
                    .GroupBy(x => x.Masothue)
                    .Select(g => g.First())
                    .ToList();

                Console.WriteLine($"🔍 Records in year {currentYear}: {currentYearData.Count}");
                Console.WriteLine($"🔍 Unique companies in year: {uniqueCompaniesInYear.Count}");

                // Financial data availability
                var withRevenue = uniqueCompaniesInYear.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0);
                var withProfit = uniqueCompaniesInYear.Count(x => x.SR_Loinhuan_TruocThue.HasValue);
                var withAssets = uniqueCompaniesInYear.Count(x => x.Taisan_Tong_CK.HasValue && x.Taisan_Tong_CK.Value > 0);

                Console.WriteLine($"🔍 Financial data availability:");
                Console.WriteLine($"   - With Revenue > 0: {withRevenue}");
                Console.WriteLine($"   - With Profit data: {withProfit}");
                Console.WriteLine($"   - With Assets > 0: {withAssets}");

                // Calculate financial totals
                var totalRevenue = uniqueCompaniesInYear
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0)
                    .Sum(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value);

                var totalAssets = uniqueCompaniesInYear
                    .Where(x => x.Taisan_Tong_CK.HasValue && x.Taisan_Tong_CK.Value > 0)
                    .Sum(x => x.Taisan_Tong_CK.Value);

                var totalProfit = uniqueCompaniesInYear
                    .Where(x => x.SR_Loinhuan_TruocThue.HasValue)
                    .Sum(x => x.SR_Loinhuan_TruocThue.Value);

                // Sample companies with financial data
                var sampleCompanies = uniqueCompaniesInYear
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue || x.Taisan_Tong_CK.HasValue || x.SR_Loinhuan_TruocThue.HasValue)
                    .Take(5)
                    .Select(x => new
                    {
                        company = x.TenDN,
                        masothue = x.Masothue,
                        revenue = x.SR_Doanhthu_Thuan_BH_CCDV,
                        profit = x.SR_Loinhuan_TruocThue,
                        assets = x.Taisan_Tong_CK
                    }).ToList();

                return Json(new
                {
                    success = true,
                    message = "✅ Financial data test completed",
                    totalRecords = allData.Count,
                    currentYear = currentYear,
                    recordsInYear = currentYearData.Count,
                    uniqueCompanies = uniqueCompaniesInYear.Count,
                    dataAvailability = new
                    {
                        withRevenue = withRevenue,
                        withProfit = withProfit,
                        withAssets = withAssets
                    },
                    financialTotals = new
                    {
                        totalRevenue = totalRevenue,
                        totalProfit = totalProfit,
                        totalAssets = totalAssets,
                        revenueInBillions = Math.Round(totalRevenue / 1000000, 2),
                        profitInBillions = Math.Round(totalProfit / 1000000, 2),
                        assetsInBillions = Math.Round(totalAssets / 1000000, 2)
                    },
                    sampleCompanies = sampleCompanies,
                    databaseInfo = new
                    {
                        database = "admin_ciresearch",
                        table = "dn_all",
                        revenueColumn = "SR_Doanhthu_Thuan_BH_CCDV",
                        profitColumn = "SR_Loinhuan_TruocThue",
                        assetsColumn = "Taisan_Tong_CK",
                        unit = "triệu VND"
                    },
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Financial data test failed",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFilterOptions()
        {
            try
            {
                var allData = await GetCachedDataAsync();

                Console.WriteLine($"🔍 FILTER OPTIONS - Processing {allData.Count} records");

                // Get available years from database
                var availableYears = allData
                    .Where(x => x.Nam.HasValue && x.Nam.Value > 1990 && x.Nam.Value <= DateTime.Now.Year + 1)
                    .Select(x => x.Nam.Value.ToString())
                    .Distinct()
                    .OrderByDescending(x => int.Parse(x))
                    .ToList();

                Console.WriteLine($"🔍 Years found: [{string.Join(", ", availableYears)}]");

                // Get business types from Loaihinhkte column
                var businessTypes = allData
                    .Where(x => !string.IsNullOrEmpty(x.Loaihinhkte))
                    .Select(x => x.Loaihinhkte.Trim())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                Console.WriteLine($"🔍 Business types found: {businessTypes.Count}");

                // Get provinces from MaTinh_Dieutra column
                var provinces = allData
                    .Where(x => !string.IsNullOrEmpty(x.MaTinh_Dieutra))
                    .Select(x => x.MaTinh_Dieutra.Trim())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                Console.WriteLine($"🔍 Provinces found: {provinces.Count}");

                // Get economic zones from Vungkinhte column
                var economicZones = allData
                    .Where(x => !string.IsNullOrEmpty(x.Vungkinhte))
                    .Select(x => x.Vungkinhte.Trim())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                Console.WriteLine($"🔍 Economic zones found: {economicZones.Count}");

                // Get regions from Region column
                var regions = allData
                    .Where(x => !string.IsNullOrEmpty(x.Region))
                    .Select(x => x.Region.Trim())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                Console.WriteLine($"🔍 Regions found: {regions.Count}");

                // Get company size categories based on revenue data
                var companySizeCategories = new List<string> { "Siêu nhỏ", "Nhỏ", "Vừa", "Lớn" };

                var filterOptions = new
                {
                    success = true,
                    message = "✅ Filter options loaded from database",
                    dataSource = new
                    {
                        database = "admin_ciresearch",
                        table = "dn_all",
                        totalRecords = allData.Count
                    },
                    filters = new
                    {
                        years = availableYears,
                        businessTypes = businessTypes,
                        provinces = provinces,
                        economicZones = economicZones,
                        regions = regions,
                        companySizes = companySizeCategories
                    },
                    counts = new
                    {
                        yearsCount = availableYears.Count,
                        businessTypesCount = businessTypes.Count,
                        provincesCount = provinces.Count,
                        economicZonesCount = economicZones.Count,
                        regionsCount = regions.Count
                    },
                    timestamp = DateTime.Now
                };

                return Json(filterOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting filter options: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to load filter options from database",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTopCompaniesRevenueChart()
        {
            try
            {
                var allData = await GetCachedDataAsync();

                Console.WriteLine($"🔍 TOP REVENUE COMPANIES - Processing {allData.Count} records");

                // Check revenue column data availability first
                var totalWithRevenue = allData.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue);
                var totalWithPositiveRevenue = allData.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0);

                Console.WriteLine($"🔍 REVENUE DATA AVAILABILITY:");
                Console.WriteLine($"   - Total records: {allData.Count}");
                Console.WriteLine($"   - Records with SR_Doanhthu_Thuan_BH_CCDV: {totalWithRevenue}");
                Console.WriteLine($"   - Records with revenue > 0: {totalWithPositiveRevenue}");

                // Get companies with revenue data across multiple years
                var companiesWithRevenue = allData
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue &&
                               x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0 &&
                               !string.IsNullOrEmpty(x.Masothue) &&
                               x.Nam.HasValue)
                    .ToList();

                Console.WriteLine($"🔍 Records with revenue data: {companiesWithRevenue.Count}");

                if (companiesWithRevenue.Count == 0)
                {
                    Console.WriteLine($"❌ NO COMPANIES WITH REVENUE DATA FOUND!");
                    return Json(new
                    {
                        success = false,
                        message = "❌ Không tìm thấy dữ liệu doanh thu từ cột SR_Doanhthu_Thuan_BH_CCDV",
                        debug = new
                        {
                            totalRecords = allData.Count,
                            withRevenue = totalWithRevenue,
                            withPositiveRevenue = totalWithPositiveRevenue,
                            columnName = "SR_Doanhthu_Thuan_BH_CCDV"
                        },
                        timestamp = DateTime.Now
                    });
                }

                // Group by company (Masothue) and calculate average revenue across years
                var companyAverages = companiesWithRevenue
                    .GroupBy(x => x.Masothue)
                    .Select(g => new
                    {
                        Masothue = g.Key,
                        CompanyName = g.FirstOrDefault()?.TenDN ?? "Unknown",
                        YearlyData = g.GroupBy(x => x.Nam.Value)
                            .Select(yearGroup => new
                            {
                                Year = yearGroup.Key,
                                Revenue = yearGroup.Average(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value)
                            })
                            .OrderBy(x => x.Year)
                            .ToList(),
                        AverageRevenue = g.Average(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value),
                        YearsCount = g.Select(x => x.Nam.Value).Distinct().Count()
                    })
                    .Where(x => x.YearsCount >= 1) // At least 1 year of data (reduced from 2)
                    .OrderByDescending(x => x.AverageRevenue)
                    .Take(3)
                    .ToList();

                Console.WriteLine($"🔍 Top 3 companies by average revenue ({companyAverages.Count} found):");
                foreach (var company in companyAverages)
                {
                    Console.WriteLine($"   - {company.CompanyName} ({company.Masothue}): {company.AverageRevenue:N2} triệu VND avg over {company.YearsCount} years");
                }

                if (companyAverages.Count == 0)
                {
                    Console.WriteLine($"❌ NO COMPANIES FOUND AFTER GROUPING!");
                    return Json(new
                    {
                        success = false,
                        message = "❌ Không tìm thấy doanh nghiệp nào có đủ dữ liệu doanh thu",
                        debug = new
                        {
                            filteredRecords = companiesWithRevenue.Count,
                            uniqueCompanies = companiesWithRevenue.Select(x => x.Masothue).Distinct().Count(),
                            columnName = "SR_Doanhthu_Thuan_BH_CCDV"
                        },
                        timestamp = DateTime.Now
                    });
                }

                // Get all available years from top companies
                var allYears = companyAverages
                    .SelectMany(x => x.YearlyData.Select(y => y.Year))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                // Prepare chart data
                var datasets = companyAverages.Select((company, index) =>
                {
                    var colors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1" };
                    var yearlyRevenue = allYears.Select(year =>
                    {
                        var yearData = company.YearlyData.FirstOrDefault(y => y.Year == year);
                        return yearData != null ? Math.Round(yearData.Revenue / 1000, 2) : 0; // Convert to billion VND
                    }).ToList();

                    return new
                    {
                        label = $"{company.CompanyName} ({company.Masothue})",
                        data = yearlyRevenue,
                        borderColor = colors[index],
                        backgroundColor = colors[index] + "20",
                        borderWidth = 3,
                        fill = false,
                        tension = 0.4,
                        pointRadius = 6,
                        pointBackgroundColor = colors[index],
                        pointBorderColor = "#ffffff",
                        pointBorderWidth = 2
                    };
                }).ToList();

                var chartData = new
                {
                    success = true,
                    message = "✅ Top 3 companies revenue chart data",
                    data = new
                    {
                        labels = allYears.Select(y => y.ToString()).ToList(),
                        datasets = datasets
                    },
                    metadata = new
                    {
                        totalCompanies = companyAverages.Count,
                        yearsRange = allYears.Any() ? $"{allYears.FirstOrDefault()}-{allYears.LastOrDefault()}" : "No years",
                        dataSource = "Revenue from SR_Doanhthu_Thuan_BH_CCDV column",
                        unit = "tỷ VND"
                    },
                    debug = new
                    {
                        totalRecordsInDatabase = allData.Count,
                        recordsWithRevenue = totalWithRevenue,
                        recordsWithPositiveRevenue = totalWithPositiveRevenue,
                        filteredCompanies = companiesWithRevenue.Count,
                        topCompaniesFound = companyAverages.Count,
                        yearsData = allYears,
                        sampleCompanies = companyAverages.Select(c => new
                        {
                            name = c.CompanyName,
                            masothue = c.Masothue,
                            avgRevenue = c.AverageRevenue,
                            years = c.YearsCount
                        })
                    },
                    timestamp = DateTime.Now
                };

                return Json(chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting top companies revenue chart: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to get top companies revenue data",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTopCompaniesProfitChart()
        {
            try
            {
                var allData = await GetCachedDataAsync();

                Console.WriteLine($"🔍 TOP PROFIT COMPANIES - Processing {allData.Count} records");

                // Get companies with profit data across multiple years
                var companiesWithProfit = allData
                    .Where(x => x.SR_Loinhuan_TruocThue.HasValue &&
                               !string.IsNullOrEmpty(x.Masothue) &&
                               x.Nam.HasValue)
                    .ToList();

                Console.WriteLine($"🔍 Records with profit data: {companiesWithProfit.Count}");

                // Group by company and calculate average profit across years
                var companyAverages = companiesWithProfit
                    .GroupBy(x => x.Masothue)
                    .Select(g => new
                    {
                        Masothue = g.Key,
                        CompanyName = g.FirstOrDefault()?.TenDN ?? "Unknown",
                        YearlyData = g.GroupBy(x => x.Nam.Value)
                            .Select(yearGroup => new
                            {
                                Year = yearGroup.Key,
                                Profit = yearGroup.Average(x => x.SR_Loinhuan_TruocThue.Value)
                            })
                            .OrderBy(x => x.Year)
                            .ToList(),
                        AverageProfit = g.Average(x => x.SR_Loinhuan_TruocThue.Value),
                        YearsCount = g.Select(x => x.Nam.Value).Distinct().Count()
                    })
                    .Where(x => x.YearsCount >= 1) // Only companies with at least 1 year of data (same as revenue chart)
                    .OrderByDescending(x => x.AverageProfit)
                    .Take(3)
                    .ToList();

                Console.WriteLine($"🔍 Top 3 companies by average profit:");
                foreach (var company in companyAverages)
                {
                    Console.WriteLine($"   - {company.CompanyName} ({company.Masothue}): {company.AverageProfit:N2} triệu VND avg over {company.YearsCount} years");
                }

                if (companyAverages.Count == 0)
                {
                    Console.WriteLine($"❌ NO COMPANIES FOUND AFTER GROUPING!");
                    return Json(new
                    {
                        success = false,
                        message = "❌ Không tìm thấy doanh nghiệp nào có đủ dữ liệu lợi nhuận",
                        debug = new
                        {
                            filteredRecords = companiesWithProfit.Count,
                            uniqueCompanies = companiesWithProfit.Select(x => x.Masothue).Distinct().Count(),
                            columnName = "SR_Loinhuan_TruocThue"
                        },
                        timestamp = DateTime.Now
                    });
                }

                // Get all available years from top companies
                var allYears = companyAverages
                    .SelectMany(x => x.YearlyData.Select(y => y.Year))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                // Prepare chart data
                var datasets = companyAverages.Select((company, index) =>
                {
                    var colors = new[] { "#28A745", "#FD7E14", "#6F42C1" };
                    var yearlyProfit = allYears.Select(year =>
                    {
                        var yearData = company.YearlyData.FirstOrDefault(y => y.Year == year);
                        return yearData != null ? Math.Round(yearData.Profit / 1000, 2) : 0; // Convert to billion VND
                    }).ToList();

                    return new
                    {
                        label = $"{company.CompanyName} ({company.Masothue})",
                        data = yearlyProfit,
                        borderColor = colors[index],
                        backgroundColor = colors[index] + "20",
                        borderWidth = 3,
                        fill = false,
                        tension = 0.4,
                        pointRadius = 6,
                        pointBackgroundColor = colors[index],
                        pointBorderColor = "#ffffff",
                        pointBorderWidth = 2
                    };
                }).ToList();

                var chartData = new
                {
                    success = true,
                    message = "✅ Top 3 companies profit chart data",
                    data = new
                    {
                        labels = allYears.Select(y => y.ToString()).ToList(),
                        datasets = datasets
                    },
                    metadata = new
                    {
                        totalCompanies = companyAverages.Count,
                        yearsRange = $"{allYears.FirstOrDefault()}-{allYears.LastOrDefault()}",
                        dataSource = "Profit from SR_Loinhuan_TruocThue column",
                        unit = "tỷ VND"
                    },
                    timestamp = DateTime.Now
                };

                return Json(chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting top companies profit chart: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to get top companies profit data",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyDataByTaxCode(string masothue)
        {
            try
            {
                if (string.IsNullOrEmpty(masothue))
                {
                    return Json(new
                    {
                        success = false,
                        message = "❌ Mã số thuế không được để trống",
                        timestamp = DateTime.Now
                    });
                }

                var allData = await GetCachedDataAsync();

                Console.WriteLine($"🔍 SEARCH COMPANY - Looking for tax code: {masothue}");

                // Find company by tax code
                var companyData = allData
                    .Where(x => !string.IsNullOrEmpty(x.Masothue) &&
                               x.Masothue.Equals(masothue, StringComparison.OrdinalIgnoreCase) &&
                               x.Nam.HasValue)
                    .OrderBy(x => x.Nam)
                    .ToList();

                if (!companyData.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = $"❌ Không tìm thấy doanh nghiệp với mã số thuế: {masothue}",
                        timestamp = DateTime.Now
                    });
                }

                var companyName = companyData.FirstOrDefault()?.TenDN ?? "Unknown";

                Console.WriteLine($"🔍 Found company: {companyName} with {companyData.Count} year records");

                // Prepare revenue data
                var revenueData = companyData
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue)
                    .Select(x => new
                    {
                        Year = x.Nam.Value,
                        Revenue = Math.Round(x.SR_Doanhthu_Thuan_BH_CCDV.Value / 1000, 2) // Convert to billion VND
                    })
                    .ToList();

                // Prepare profit data
                var profitData = companyData
                    .Where(x => x.SR_Loinhuan_TruocThue.HasValue)
                    .Select(x => new
                    {
                        Year = x.Nam.Value,
                        Profit = Math.Round(x.SR_Loinhuan_TruocThue.Value / 1000, 2) // Convert to billion VND
                    })
                    .ToList();

                var years = companyData.Select(x => x.Nam.Value).Distinct().OrderBy(x => x).ToList();

                var result = new
                {
                    success = true,
                    message = $"✅ Tìm thấy dữ liệu cho doanh nghiệp: {companyName}",
                    company = new
                    {
                        masothue = masothue,
                        name = companyName,
                        yearsOfData = years.Count,
                        yearRange = $"{years.FirstOrDefault()}-{years.LastOrDefault()}"
                    },
                    revenueChart = new
                    {
                        labels = years.Select(y => y.ToString()).ToList(),
                        datasets = new[]
                        {
                            new
                            {
                                label = $"Doanh thu - {companyName}",
                                data = years.Select(year =>
                                {
                                    var yearRevenue = revenueData.FirstOrDefault(r => r.Year == year);
                                    return yearRevenue?.Revenue ?? 0;
                                }).ToList(),
                                borderColor = "#28A745",
                                backgroundColor = "rgba(40, 167, 69, 0.1)",
                                borderWidth = 3,
                                fill = false,
                                tension = 0.4,
                                pointRadius = 6,
                                pointBackgroundColor = "#28A745",
                                pointBorderColor = "#ffffff",
                                pointBorderWidth = 2
                            }
                        }
                    },
                    profitChart = new
                    {
                        labels = years.Select(y => y.ToString()).ToList(),
                        datasets = new[]
                        {
                            new
                            {
                                label = $"Lợi nhuận - {companyName}",
                                data = years.Select(year =>
                                {
                                    var yearProfit = profitData.FirstOrDefault(p => p.Year == year);
                                    return yearProfit?.Profit ?? 0;
                                }).ToList(),
                                borderColor = "#FD7E14",
                                backgroundColor = "rgba(253, 126, 20, 0.1)",
                                borderWidth = 3,
                                fill = false,
                                tension = 0.4,
                                pointRadius = 6,
                                pointBackgroundColor = "#FD7E14",
                                pointBorderColor = "#ffffff",
                                pointBorderWidth = 2
                            }
                        }
                    },
                    rawData = new
                    {
                        revenueYears = revenueData.Count,
                        profitYears = profitData.Count,
                        totalRecords = companyData.Count
                    },
                    timestamp = DateTime.Now
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error searching company by tax code: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = $"❌ Lỗi khi tìm kiếm mã số thuế: {masothue}",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugRevenueData()
        {
            try
            {
                var allData = await GetCachedDataAsync();

                Console.WriteLine($"🔍 DEBUG REVENUE DATA - Total records: {allData.Count}");

                // Check revenue column availability
                var totalWithRevenue = allData.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue);
                var totalWithPositiveRevenue = allData.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0);

                // Sample revenue data
                var sampleRevenueData = allData
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0)
                    .Take(10)
                    .Select(x => new
                    {
                        Company = x.TenDN,
                        TaxCode = x.Masothue,
                        Year = x.Nam,
                        Revenue = x.SR_Doanhthu_Thuan_BH_CCDV.Value,
                        RevenueInBillion = Math.Round(x.SR_Doanhthu_Thuan_BH_CCDV.Value / 1000, 2)
                    })
                    .ToList();

                // Check distribution by year
                var revenueByYear = allData
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0 && x.Nam.HasValue)
                    .GroupBy(x => x.Nam.Value)
                    .Select(g => new
                    {
                        Year = g.Key,
                        Count = g.Count(),
                        UniqueCompanies = g.Select(x => x.Masothue).Where(x => !string.IsNullOrEmpty(x)).Distinct().Count(),
                        AvgRevenue = g.Average(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value),
                        MaxRevenue = g.Max(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value),
                        MinRevenue = g.Min(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value)
                    })
                    .OrderBy(x => x.Year)
                    .ToList();

                // Get top companies by average revenue across years
                var topCompaniesByRevenue = allData
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue &&
                               x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0 &&
                               !string.IsNullOrEmpty(x.Masothue) &&
                               x.Nam.HasValue)
                    .GroupBy(x => x.Masothue)
                    .Select(g => new
                    {
                        TaxCode = g.Key,
                        CompanyName = g.FirstOrDefault()?.TenDN,
                        YearsCount = g.Select(x => x.Nam.Value).Distinct().Count(),
                        AverageRevenue = g.Average(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value),
                        TotalRecords = g.Count(),
                        Years = g.Select(x => x.Nam.Value).Distinct().OrderBy(x => x).ToArray()
                    })
                    .Where(x => x.YearsCount >= 1)
                    .OrderByDescending(x => x.AverageRevenue)
                    .Take(5)
                    .ToList();

                var result = new
                {
                    success = true,
                    message = "✅ Revenue data debug completed",
                    summary = new
                    {
                        totalRecords = allData.Count,
                        recordsWithRevenue = totalWithRevenue,
                        recordsWithPositiveRevenue = totalWithPositiveRevenue,
                        percentageWithRevenue = totalWithRevenue > 0 ? Math.Round((double)totalWithPositiveRevenue / allData.Count * 100, 2) : 0
                    },
                    columnInfo = new
                    {
                        columnName = "SR_Doanhthu_Thuan_BH_CCDV",
                        dataType = "decimal",
                        unit = "triệu VND",
                        convertedUnit = "tỷ VND (chia 1000)"
                    },
                    sampleData = sampleRevenueData,
                    distributionByYear = revenueByYear,
                    topCompaniesByRevenue = topCompaniesByRevenue,
                    timestamp = DateTime.Now
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in debug revenue data: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to debug revenue data",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMarketShareChart(int? nam = null)
        {
            try
            {
                var allData = await GetCachedDataAsync();

                Console.WriteLine($"🔍 MARKET SHARE ANALYSIS - Processing {allData.Count} records");

                // Determine target year - use provided year or latest available year
                int targetYear;
                if (nam.HasValue)
                {
                    targetYear = nam.Value;
                    Console.WriteLine($"🔍 Using specified year: {targetYear}");
                }
                else
                {
                    targetYear = GetLatestYear(allData);
                    Console.WriteLine($"🔍 Using latest available year: {targetYear}");
                }

                // Get current year data and unique companies (same logic as Statistics Cards)
                var currentYearData = allData.Where(x => x.Nam == targetYear).ToList();
                var uniqueCompaniesInYear = currentYearData
                    .Where(x => !string.IsNullOrEmpty(x.Masothue))
                    .GroupBy(x => x.Masothue)
                    .Select(g => g.First())
                    .ToList();

                // FIXED: Use ALL companies with Masothue (same as Statistics Cards) for BASE calculation
                var allCompaniesWithRevenueData = uniqueCompaniesInYear; // ALL unique companies, not just those with revenue data

                // Get companies with actual revenue data for calculation
                var companiesWithActualRevenueData = allCompaniesWithRevenueData
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue)
                    .ToList();

                // Get ONLY companies with POSITIVE revenue for display in chart
                var companiesWithPositiveRevenue = companiesWithActualRevenueData
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0)
                    .ToList();

                // Companies without revenue data (will be treated as 0 revenue)
                var companiesWithoutRevenueData = allCompaniesWithRevenueData
                    .Where(x => !x.SR_Doanhthu_Thuan_BH_CCDV.HasValue)
                    .ToList();

                // 🚨 CRITICAL DEBUG: This is where "DN KHÔNG có dữ liệu DT" number comes from
                Console.WriteLine($"\n🚨🚨🚨 CRITICAL DEBUG - SOURCE OF 'DN KHÔNG có dữ liệu DT' NUMBER 🚨🚨🚨");
                Console.WriteLine($"🚨 allCompaniesWithRevenueData.Count(): {allCompaniesWithRevenueData.Count}");
                Console.WriteLine($"🚨 companiesWithoutRevenueData.Count(): {companiesWithoutRevenueData.Count}");
                Console.WriteLine($"🚨 companiesWithActualRevenueData.Count(): {companiesWithActualRevenueData.Count}");
                Console.WriteLine($"🚨 Math check: {allCompaniesWithRevenueData.Count} - {companiesWithActualRevenueData.Count} = {allCompaniesWithRevenueData.Count - companiesWithActualRevenueData.Count}");
                Console.WriteLine($"🚨 This number ({companiesWithoutRevenueData.Count}) will show as 'DN KHÔNG có dữ liệu DT' in UI");

                Console.WriteLine($"🔍 MARKET SHARE DATA FOR YEAR {targetYear}:");
                Console.WriteLine($"   - Total unique companies in year (BASE): {allCompaniesWithRevenueData.Count}");
                Console.WriteLine($"   - Companies with revenue data: {companiesWithActualRevenueData.Count}");
                Console.WriteLine($"   - Companies WITHOUT revenue data (treated as 0): {companiesWithoutRevenueData.Count}");
                Console.WriteLine($"   - Companies with POSITIVE revenue (for chart): {companiesWithPositiveRevenue.Count}");
                Console.WriteLine($"   - Companies with negative/zero revenue: {companiesWithActualRevenueData.Count - companiesWithPositiveRevenue.Count}");

                // 🚨 ULTRA DEBUG: Check exact companies that make companiesWithoutRevenueData != 0
                Console.WriteLine($"\n🚨🚨🚨 ULTRA DEBUG - INVESTIGATING EXACT COMPANIES 🚨🚨🚨");
                Console.WriteLine($"🚨 Target Year: {targetYear}");
                Console.WriteLine($"🚨 allCompaniesWithRevenueData contains {allCompaniesWithRevenueData.Count} companies");
                Console.WriteLine($"🚨 companiesWithActualRevenueData contains {companiesWithActualRevenueData.Count} companies");
                Console.WriteLine($"🚨 companiesWithoutRevenueData contains {companiesWithoutRevenueData.Count} companies");

                // Check samples from each group
                Console.WriteLine($"\n🚨 SAMPLE FROM allCompaniesWithRevenueData (first 3):");
                foreach (var company in allCompaniesWithRevenueData.Take(3))
                {
                    Console.WriteLine($"   - {company.TenDN} ({company.Masothue}): HasValue={company.SR_Doanhthu_Thuan_BH_CCDV.HasValue}, Value={company.SR_Doanhthu_Thuan_BH_CCDV}");
                }

                Console.WriteLine($"\n🚨 SAMPLE FROM companiesWithActualRevenueData (first 3):");
                foreach (var company in companiesWithActualRevenueData.Take(3))
                {
                    Console.WriteLine($"   - {company.TenDN} ({company.Masothue}): HasValue={company.SR_Doanhthu_Thuan_BH_CCDV.HasValue}, Value={company.SR_Doanhthu_Thuan_BH_CCDV}");
                }

                // 🚨 DEBUG: List companies WITHOUT revenue data
                if (companiesWithoutRevenueData.Count > 0)
                {
                    Console.WriteLine($"\n🚨 CRITICAL - COMPANIES WITHOUT REVENUE DATA (SOURCE OF UI NUMBER):");
                    Console.WriteLine($"🚨 These {companiesWithoutRevenueData.Count} companies have !x.SR_Doanhthu_Thuan_BH_CCDV.HasValue:");
                    foreach (var company in companiesWithoutRevenueData)
                    {
                        Console.WriteLine($"   🚨 Company: '{company.TenDN}'");
                        Console.WriteLine($"      - Masothue: '{company.Masothue}'");
                        Console.WriteLine($"      - STT: {company.STT}");
                        Console.WriteLine($"      - Nam: {company.Nam}");
                        Console.WriteLine($"      - SR_Doanhthu_Thuan_BH_CCDV.HasValue: {company.SR_Doanhthu_Thuan_BH_CCDV.HasValue}");
                        Console.WriteLine($"      - SR_Doanhthu_Thuan_BH_CCDV Value: {company.SR_Doanhthu_Thuan_BH_CCDV}");
                        Console.WriteLine($"      - Raw database value check: {(company.SR_Doanhthu_Thuan_BH_CCDV?.ToString() ?? "NULL")}");
                        Console.WriteLine($"      - Type check: {company.SR_Doanhthu_Thuan_BH_CCDV?.GetType().Name ?? "NULL"}");
                        Console.WriteLine($"");
                    }
                    Console.WriteLine($"🚨 END CRITICAL DEBUG");
                    Console.WriteLine($"🚨 SQL to verify: SELECT TenDN, Masothue, STT, Nam, SR_Doanhthu_Thuan_BH_CCDV, ISNULL(SR_Doanhthu_Thuan_BH_CCDV, 'IS_NULL') as StatusCheck FROM dn_all WHERE Nam = {targetYear} AND Masothue IN ('{string.Join("', '", companiesWithoutRevenueData.Select(x => x.Masothue))}')");
                }
                else
                {
                    Console.WriteLine($"✅ All companies have revenue data - no companies with !HasValue found");
                    Console.WriteLine($"✅ This means the issue is NOT with NULL values in database");
                }

                if (companiesWithActualRevenueData.Count == 0)
                {
                    // Check if there's data for other years
                    var availableYears = allData
                        .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue &&
                                   x.Nam.HasValue)
                        .Select(x => x.Nam.Value)
                        .Distinct()
                        .OrderByDescending(x => x)
                        .ToList();

                    Console.WriteLine($"❌ NO COMPANIES WITH REVENUE DATA FOR YEAR {targetYear}!");
                    Console.WriteLine($"   - Available years with data: [{string.Join(", ", availableYears)}]");

                    return Json(new
                    {
                        success = false,
                        message = $"❌ Không tìm thấy dữ liệu doanh thu cho năm {targetYear}",
                        debug = new
                        {
                            targetYear = targetYear,
                            totalRecords = allData.Count,
                            totalCompanies = allCompaniesWithRevenueData.Count,
                            companiesWithRevenueData = companiesWithActualRevenueData.Count,
                            companiesWithoutRevenueData = companiesWithoutRevenueData.Count,
                            availableYears = availableYears,
                            columnName = "SR_Doanhthu_Thuan_BH_CCDV"
                        },
                        timestamp = DateTime.Now
                    });
                }

                if (companiesWithPositiveRevenue.Count == 0)
                {
                    Console.WriteLine($"❌ NO COMPANIES WITH POSITIVE REVENUE FOR YEAR {targetYear}!");
                    return Json(new
                    {
                        success = false,
                        message = $"❌ Không tìm thấy doanh nghiệp nào có doanh thu dương cho năm {targetYear}",
                        debug = new
                        {
                            targetYear = targetYear,
                            withAnyRevenue = allCompaniesWithRevenueData.Count,
                            withPositiveRevenue = companiesWithPositiveRevenue.Count
                        },
                        timestamp = DateTime.Now
                    });
                }

                // Calculate total market revenue INCLUDING negative and zero revenue (BASE CHUẨN)
                // Companies without revenue data are treated as 0 revenue
                var totalMarketRevenue = companiesWithActualRevenueData.Sum(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value);

                // Break down revenue by type for debugging
                var positiveRevenue = companiesWithActualRevenueData.Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0).Sum(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value);
                var negativeRevenue = companiesWithActualRevenueData.Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value < 0).Sum(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value);
                var zeroRevenue = companiesWithActualRevenueData.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value == 0);
                var noRevenueData = companiesWithoutRevenueData.Count;

                Console.WriteLine($"🔍 REVENUE BREAKDOWN FOR BASE CALCULATION:");
                Console.WriteLine($"   - Total Market Revenue (BASE): {totalMarketRevenue:N2} triệu VND = {totalMarketRevenue / 1000:N2} tỷ VND");
                Console.WriteLine($"   - Positive Revenue: {positiveRevenue:N2} triệu VND");
                Console.WriteLine($"   - Negative Revenue: {negativeRevenue:N2} triệu VND");
                Console.WriteLine($"   - Companies with Zero Revenue: {zeroRevenue}");
                Console.WriteLine($"   - Companies with NO Revenue Data: {noRevenueData} (treated as 0)");
                Console.WriteLine($"🔍 BASE LOGIC: Total Market Revenue = Positive + Negative + Zero = {totalMarketRevenue / 1000:N2} tỷ VND");
                Console.WriteLine($"🔍 TOTAL COMPANIES IN BASE: {allCompaniesWithRevenueData.Count} = {companiesWithActualRevenueData.Count} (with data) + {noRevenueData} (no data)");

                // Calculate market share for each company (only POSITIVE revenue companies for chart)
                var marketShareData = companiesWithPositiveRevenue
                    .Select(x => new
                    {
                        TaxCode = x.Masothue,
                        CompanyName = x.TenDN ?? "Unknown",
                        Revenue = x.SR_Doanhthu_Thuan_BH_CCDV.Value,
                        RevenueInBillion = Math.Round(x.SR_Doanhthu_Thuan_BH_CCDV.Value / 1000, 2),
                        MarketShare = Math.Round((x.SR_Doanhthu_Thuan_BH_CCDV.Value / totalMarketRevenue) * 100, 4), // Percentage (using total market revenue including negative/zero)
                        Year = x.Nam.Value
                    })
                    .OrderByDescending(x => x.MarketShare)
                    .ToList();

                Console.WriteLine($"🔍 MARKET SHARE CALCULATION:");
                Console.WriteLine($"   - Companies analyzed: {marketShareData.Count}");
                Console.WriteLine($"   - Top 5 market shares:");
                foreach (var company in marketShareData.Take(5))
                {
                    Console.WriteLine($"     - {company.CompanyName}: {company.MarketShare}% ({company.RevenueInBillion} tỷ VND)");
                }

                // Get top 10 companies and group others
                var top10Companies = marketShareData.Take(10).ToList();
                var othersMarketShare = marketShareData.Skip(10).Sum(x => x.MarketShare);
                var othersRevenue = marketShareData.Skip(10).Sum(x => x.RevenueInBillion);
                var othersCount = marketShareData.Skip(10).Count();

                Console.WriteLine($"🔍 TOP 10 + OTHERS:");
                Console.WriteLine($"   - Top 10 companies market share total: {top10Companies.Sum(x => x.MarketShare):N2}%");
                Console.WriteLine($"   - Others ({othersCount} companies): {othersMarketShare:N2}%");

                // VALIDATION: Ensure Top 10 + Others = 100%
                var totalMarketShareCheck = top10Companies.Sum(x => x.MarketShare) + othersMarketShare;
                var allPositiveMarketShare = marketShareData.Sum(x => x.MarketShare);
                Console.WriteLine($"🔍 MARKET SHARE VALIDATION:");
                Console.WriteLine($"   - Top 10 + Others: {totalMarketShareCheck:N4}%");
                Console.WriteLine($"   - All positive companies total: {allPositiveMarketShare:N4}%");
                Console.WriteLine($"   - Should be ≈ 100%: {Math.Abs(allPositiveMarketShare - 100m) < 0.01m}");

                if (Math.Abs(allPositiveMarketShare - 100m) > 0.1m)
                {
                    Console.WriteLine($"⚠️ WARNING: Market share doesn't add up to 100%!");
                    Console.WriteLine($"⚠️ Difference: {100 - allPositiveMarketShare:N4}%");
                }

                // Prepare chart data for Clustered Column Chart
                var chartLabels = top10Companies.Select(x =>
                {
                    // Shorten company name for better display
                    var shortName = x.CompanyName.Length > 25 ? x.CompanyName.Substring(0, 22) + "..." : x.CompanyName;
                    return shortName;
                }).ToList();

                if (othersCount > 0)
                {
                    chartLabels.Add($"Others ({othersCount} DN)");
                }

                var marketShareValues = top10Companies.Select(x => x.MarketShare).ToList();
                if (othersCount > 0)
                {
                    marketShareValues.Add(othersMarketShare);
                }

                var revenueValues = top10Companies.Select(x => x.RevenueInBillion).ToList();
                if (othersCount > 0)
                {
                    revenueValues.Add(othersRevenue);
                }

                // Generate colors for the chart
                var colors = new[]
                {
                    "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4", "#FFCE56",
                    "#FF9F40", "#9966FF", "#FF6384", "#36A2EB", "#C9CBCF",
                    "#95A5A6" // Color for "Others"
                };

                var chartData = new
                {
                    success = true,
                    message = "✅ Market share analysis completed",
                    data = new
                    {
                        labels = chartLabels,
                        datasets = new object[]
                        {
                            new
                            {
                                label = "Market Share (%)",
                                data = marketShareValues,
                                backgroundColor = colors.Take(chartLabels.Count).ToArray(),
                                borderColor = colors.Take(chartLabels.Count).Select(c => c + "CC").ToArray(),
                                borderWidth = 2,
                                yAxisID = "y"
                            },
                            new
                            {
                                label = "Doanh thu (tỷ VND)",
                                data = revenueValues,
                                backgroundColor = colors.Take(chartLabels.Count).Select(c => c + "40").ToArray(), // More transparent
                                borderColor = colors.Take(chartLabels.Count).ToArray(),
                                borderWidth = 2,
                                type = "line",
                                yAxisID = "y1"
                            }
                        }
                    },
                    metadata = new
                    {
                        analysisYear = targetYear,
                        totalCompanies = allCompaniesWithRevenueData.Count(), // FIXED: Total companies với dữ liệu doanh thu (âm, 0, dương)
                        totalCompaniesWithPositiveRevenue = companiesWithPositiveRevenue.Count(), // Chỉ DN có doanh thu > 0
                        totalCompaniesInBase = allCompaniesWithRevenueData.Count(), // Base calculation includes all revenue data
                        top10Companies = top10Companies.Count(),
                        othersCount = othersCount,
                        totalMarketRevenue = Math.Round(totalMarketRevenue / 1000, 2), // In billion VND - includes negative/zero
                        marketShareFormula = "Market Share = (Doanh thu DN / Tổng doanh thu thị trường) × 100%",
                        baseCalculationNote = "Base bao gồm TẤT CẢ DN có dữ liệu doanh thu (âm, 0, dương) để tính market share chuẩn 100%",
                        dataSource = $"Revenue from SR_Doanhthu_Thuan_BH_CCDV column for year {targetYear}",
                        unit = "% (Market Share), tỷ VND (Revenue)",
                        revenueBreakdown = new
                        {
                            totalCompaniesInBase = allCompaniesWithRevenueData.Count(),
                            companiesWithActualRevenueData = companiesWithActualRevenueData.Count(),
                            companiesWithoutRevenueData = companiesWithoutRevenueData.Count(),
                            companiesWithPositiveRevenue = companiesWithPositiveRevenue.Count(),
                            companiesWithNegativeRevenue = companiesWithActualRevenueData.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value < 0),
                            companiesWithZeroRevenue = companiesWithActualRevenueData.Count(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value == 0),
                            positiveRevenueSum = Math.Round(positiveRevenue / 1000, 2),
                            negativeRevenueSum = Math.Round(negativeRevenue / 1000, 2),
                            totalMarketRevenueSum = Math.Round(totalMarketRevenue / 1000, 2)
                        }
                    },
                    detailedData = new
                    {
                        top10Details = top10Companies.Select(x => new
                        {
                            rank = top10Companies.IndexOf(x) + 1,
                            companyName = x.CompanyName,
                            taxCode = x.TaxCode,
                            marketShare = x.MarketShare,
                            revenue = x.RevenueInBillion,
                            year = x.Year
                        }),
                        othersData = new
                        {
                            count = othersCount,
                            totalMarketShare = othersMarketShare,
                            totalRevenue = othersRevenue
                        },
                        marketSummary = new
                        {
                            totalMarketRevenueTrillionVND = Math.Round(totalMarketRevenue / 1000, 2),
                            top10SharePercentage = Math.Round(top10Companies.Sum(x => x.MarketShare), 2),
                            othersSharePercentage = Math.Round(othersMarketShare, 2),
                            totalMarketShareValidation = Math.Round(marketShareData.Sum(x => x.MarketShare), 2),
                            shouldBe100Percent = Math.Abs(marketShareData.Sum(x => x.MarketShare) - 100m) < 0.01m,
                            calculationAccuracy = Math.Abs(marketShareData.Sum(x => x.MarketShare) - 100m) < 0.01m
                                ? "Chính xác 100%"
                                : $"{Math.Abs(100 - marketShareData.Sum(x => x.MarketShare)):0.0000}% khác biệt"
                        }
                    },
                    timestamp = DateTime.Now
                };

                Console.WriteLine($"✅ MARKET SHARE CHART DATA PREPARED:");
                Console.WriteLine($"   - Chart labels: {chartLabels.Count}");
                Console.WriteLine($"   - Market share values: [{string.Join(", ", marketShareValues.Select(v => $"{v:N2}%"))}]");
                Console.WriteLine($"   - Revenue values: [{string.Join(", ", revenueValues.Select(v => $"{v:N2}tỷ"))}]");

                // 🚨 CRITICAL UI SOURCE DEBUG - CHỖ NÀY TẠO RA SỐ HIỂN THỊ TRÊN UI
                Console.WriteLine($"\n🚨🚨🚨 UI METADATA SOURCE DEBUG 🚨🚨🚨");
                Console.WriteLine($"🚨 METADATA VALUE SENT TO UI:");
                Console.WriteLine($"   metadata.revenueBreakdown.companiesWithoutRevenueData = {companiesWithoutRevenueData.Count()}");
                Console.WriteLine($"🚨 THIS VALUE ({companiesWithoutRevenueData.Count()}) WILL APPEAR IN UI AS 'DN KHÔNG có dữ liệu DT'");
                Console.WriteLine($"🚨 CALCULATION SOURCE: allCompaniesWithRevenueData.Where(x => !x.SR_Doanhthu_Thuan_BH_CCDV.HasValue)");
                Console.WriteLine($"🚨 IF USER SEES WRONG NUMBER, THE ISSUE IS IN THIS CALCULATION ABOVE!");

                return Json(chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting market share chart: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to get market share data",
                    stackTrace = ex.StackTrace,
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetFilteredStatistics()
        {
            try
            {
                // Get filter parameters from request
                var namFilter = Request.Form["Nam"].ToList();
                var maTinhFilter = Request.Form["MaTinh_Dieutra"].ToList();
                var loaihinhkteFilter = Request.Form["Loaihinhkte"].ToList();
                var vungkinhteFilter = Request.Form["Vungkinhte"].ToList();
                var quyMoFilter = Request.Form["QuyMo"].ToList();

                Console.WriteLine($"🔍 FILTERED STATISTICS REQUEST:");
                Console.WriteLine($"   - Nam: [{string.Join(", ", namFilter)}]");
                Console.WriteLine($"   - MaTinh: [{string.Join(", ", maTinhFilter)}]");
                Console.WriteLine($"   - Loaihinhkte: [{string.Join(", ", loaihinhkteFilter)}]");
                Console.WriteLine($"   - Vungkinhte: [{string.Join(", ", vungkinhteFilter)}]");
                Console.WriteLine($"   - QuyMo: [{string.Join(", ", quyMoFilter)}]");

                var allData = await GetCachedDataAsync();
                var filteredData = ApplyFiltersOptimized(allData, "", namFilter, maTinhFilter, null, loaihinhkteFilter, vungkinhteFilter);

                Console.WriteLine($"📊 Filtered from {allData.Count} to {filteredData.Count} records");

                // Get current analysis year
                int currentYear = GetCurrentAnalysisYear(filteredData, namFilter);
                Console.WriteLine($"📅 Analysis year: {currentYear}");

                // Filter data for the current analysis year
                var currentYearData = FilterDataByYear(filteredData, currentYear);

                // Group companies by their unique tax code (Masothue) in the current year only
                var uniqueCompaniesInYear = currentYearData
                    .Where(x => !string.IsNullOrEmpty(x.Masothue))
                    .GroupBy(x => x.Masothue)
                    .Select(g => g.First())
                    .ToList();

                // Calculate statistics
                var totalCompanies = uniqueCompaniesInYear.Count;

                // Calculate labor count
                var totalLabor = uniqueCompaniesInYear.Sum(x => (long)(x.SoLaodong_CuoiNam ?? 0));
                var safeTotalLabor = totalLabor > int.MaxValue ? int.MaxValue : (int)totalLabor;

                // Regional distribution using Region field
                var companiesWithRegion = uniqueCompaniesInYear
                    .Where(x => !string.IsNullOrEmpty(x.Region))
                    .ToList();

                var regionGrouping = companiesWithRegion
                    .GroupBy(x => x.Region)
                    .ToDictionary(g => g.Key, g => g.Count());

                var mienBacCount = regionGrouping.GetValueOrDefault("Miền Bắc", 0);
                var mienTrungCount = regionGrouping.GetValueOrDefault("Miền Trung", 0);
                var mienNamCount = regionGrouping.GetValueOrDefault("Miền Nam", 0);

                // Business type distribution
                var companiesWithBusinessType = uniqueCompaniesInYear
                    .Where(x => !string.IsNullOrEmpty(x.Loaihinhkte))
                    .ToList();

                var businessTypeDistribution = companiesWithBusinessType
                    .GroupBy(x => x.Loaihinhkte)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Count());

                // Top 3 business types
                var top3BusinessTypes = businessTypeDistribution
                    .OrderByDescending(x => x.Value)
                    .Take(3)
                    .ToList();

                Console.WriteLine($"✅ CALCULATED STATISTICS:");
                Console.WriteLine($"   - Total Companies: {totalCompanies}");
                Console.WriteLine($"   - Total Labor: {safeTotalLabor:N0}");
                Console.WriteLine($"   - Miền Bắc: {mienBacCount}");
                Console.WriteLine($"   - Miền Trung: {mienTrungCount}");
                Console.WriteLine($"   - Miền Nam: {mienNamCount}");
                Console.WriteLine($"   - Top 3 Business Types: {string.Join(", ", top3BusinessTypes.Select(x => $"{x.Key}: {x.Value}"))}");

                var result = new
                {
                    success = true,
                    message = "✅ Statistics calculated successfully",
                    statistics = new
                    {
                        totalCompanies = totalCompanies,
                        totalLabor = safeTotalLabor,
                        currentAnalysisYear = currentYear,
                        regionalDistribution = new
                        {
                            mienBacCount = mienBacCount,
                            mienTrungCount = mienTrungCount,
                            mienNamCount = mienNamCount,
                            totalRegional = mienBacCount + mienTrungCount + mienNamCount
                        },
                        businessTypeDistribution = new
                        {
                            top1 = new
                            {
                                name = top3BusinessTypes.Count > 0 ? ShortenBusinessTypeName(top3BusinessTypes[0].Key) : "N/A",
                                count = top3BusinessTypes.Count > 0 ? top3BusinessTypes[0].Value : 0
                            },
                            top2 = new
                            {
                                name = top3BusinessTypes.Count > 1 ? ShortenBusinessTypeName(top3BusinessTypes[1].Key) : "N/A",
                                count = top3BusinessTypes.Count > 1 ? top3BusinessTypes[1].Value : 0
                            },
                            top3 = new
                            {
                                name = top3BusinessTypes.Count > 2 ? ShortenBusinessTypeName(top3BusinessTypes[2].Key) : "N/A",
                                count = top3BusinessTypes.Count > 2 ? top3BusinessTypes[2].Value : 0
                            }
                        }
                    },
                    filterInfo = new
                    {
                        appliedFilters = new
                        {
                            nam = namFilter,
                            maTinh = maTinhFilter,
                            loaihinhkte = loaihinhkteFilter,
                            vungkinhte = vungkinhteFilter,
                            quyMo = quyMoFilter
                        },
                        totalRecords = allData.Count,
                        filteredRecords = filteredData.Count,
                        currentYearRecords = currentYearData.Count,
                        uniqueCompanies = uniqueCompaniesInYear.Count
                    },
                    timestamp = DateTime.Now
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting filtered statistics: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to get filtered statistics",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CompareCodeVsDatabase(int? year = null)
        {
            try
            {
                var targetYear = year ?? GetLatestYear(await GetCachedDataAsync());

                Console.WriteLine($"🔍 COMPARING CODE CALCULATION VS DATABASE FOR YEAR {targetYear}");

                // 1. Get what CODE thinks
                var allData = await GetCachedDataAsync();
                var uniqueCompaniesInYear = allData
                    .Where(x => !string.IsNullOrEmpty(x.Masothue) &&
                               x.Nam.HasValue &&
                               x.Nam.Value == targetYear)
                    .GroupBy(x => x.Masothue)
                    .Select(g => g.First())
                    .ToList();

                var codeNullCompanies = uniqueCompaniesInYear
                    .Where(x => !x.SR_Doanhthu_Thuan_BH_CCDV.HasValue)
                    .ToList();

                Console.WriteLine($"📊 CODE CALCULATION:");
                Console.WriteLine($"   - Total unique companies: {uniqueCompaniesInYear.Count}");
                Console.WriteLine($"   - Companies without revenue data (NULL): {codeNullCompanies.Count}");

                // 2. Get what DATABASE actually has
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var query = @"
                    SELECT Masothue, TenDN, SR_Doanhthu_Thuan_BH_CCDV,
                           CASE 
                               WHEN SR_Doanhthu_Thuan_BH_CCDV IS NULL THEN 'NULL'
                               ELSE 'NOT_NULL'
                           END as RevenueStatus
                    FROM doanhnghiep 
                    WHERE Nam = @year 
                      AND Masothue IS NOT NULL 
                      AND TRIM(Masothue) != ''
                    ORDER BY Masothue";

                var dbResults = new List<dynamic>();
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@year", targetYear);
                using var reader = await cmd.ExecuteReaderAsync();

                var dbNullCount = 0;
                var dbTotalCount = 0;
                var dbNullCompanies = new List<string>();

                while (await reader.ReadAsync())
                {
                    dbTotalCount++;
                    var masothue = reader.GetString("Masothue");
                    var tendn = reader.IsDBNull("TenDN") ? "N/A" : reader.GetString("TenDN");
                    var revenueStatus = reader.GetString("RevenueStatus");

                    if (revenueStatus == "NULL")
                    {
                        dbNullCount++;
                        dbNullCompanies.Add($"{masothue} - {tendn}");
                        Console.WriteLine($"   🚨 DATABASE NULL: {masothue} - {tendn}");
                    }
                }

                Console.WriteLine($"💾 DATABASE ACTUAL DATA:");
                Console.WriteLine($"   - Total companies in DB: {dbTotalCount}");
                Console.WriteLine($"   - Companies with NULL revenue in DB: {dbNullCount}");

                Console.WriteLine($"🔍 COMPARISON:");
                Console.WriteLine($"   - Code thinks NULL count: {codeNullCompanies.Count}");
                Console.WriteLine($"   - Database actual NULL count: {dbNullCount}");
                Console.WriteLine($"   - Match? {codeNullCompanies.Count == dbNullCount}");

                if (codeNullCompanies.Count != dbNullCount)
                {
                    Console.WriteLine($"\n🚨 MISMATCH DETECTED! Investigating...");

                    // List companies that code thinks are NULL
                    Console.WriteLine($"🔍 Companies CODE thinks are NULL:");
                    foreach (var company in codeNullCompanies)
                    {
                        Console.WriteLine($"   - {company.Masothue} - {company.TenDN} (HasValue: {company.SR_Doanhthu_Thuan_BH_CCDV.HasValue})");
                    }
                }

                return Json(new
                {
                    success = true,
                    year = targetYear,
                    code = new
                    {
                        totalCompanies = uniqueCompaniesInYear.Count,
                        nullRevenueCount = codeNullCompanies.Count,
                        nullCompanies = codeNullCompanies.Select(c => new
                        {
                            masothue = c.Masothue,
                            tendn = c.TenDN,
                            hasValue = c.SR_Doanhthu_Thuan_BH_CCDV.HasValue,
                            value = c.SR_Doanhthu_Thuan_BH_CCDV
                        }).ToList()
                    },
                    database = new
                    {
                        totalCompanies = dbTotalCount,
                        nullRevenueCount = dbNullCount,
                        nullCompanies = dbNullCompanies
                    },
                    match = codeNullCompanies.Count == dbNullCount,
                    discrepancy = Math.Abs(codeNullCompanies.Count - dbNullCount)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in CompareCodeVsDatabase: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugRevenueDataInDatabase(int? year = null)
        {
            try
            {
                var targetYear = year ?? GetLatestYear(await GetCachedDataAsync());

                Console.WriteLine($"🔍 DEBUGGING REVENUE DATA IN DATABASE FOR YEAR {targetYear}");

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // Check revenue data directly in database
                var query = @"
                    SELECT STT, TenDN, Masothue, Nam, SR_Doanhthu_Thuan_BH_CCDV,
                           CASE 
                               WHEN SR_Doanhthu_Thuan_BH_CCDV IS NULL THEN 'NULL'
                               WHEN SR_Doanhthu_Thuan_BH_CCDV = 0 THEN 'ZERO'
                               WHEN SR_Doanhthu_Thuan_BH_CCDV > 0 THEN 'POSITIVE'
                               WHEN SR_Doanhthu_Thuan_BH_CCDV < 0 THEN 'NEGATIVE'
                           END as RevenueStatus
                    FROM dn_all 
                    WHERE Nam = @year AND Masothue IS NOT NULL AND Masothue != ''
                    ORDER BY STT";

                var companies = new List<object>();
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@year", targetYear);
                using var reader = await cmd.ExecuteReaderAsync();

                var nullCount = 0;
                var zeroCount = 0;
                var positiveCount = 0;
                var negativeCount = 0;
                var nullCompanies = new List<object>();

                while (await reader.ReadAsync())
                {
                    var revenueStatus = reader.GetString("RevenueStatus");
                    var company = new
                    {
                        STT = reader.IsDBNull("STT") ? (int?)null : reader.GetInt32("STT"),
                        TenDN = reader.IsDBNull("TenDN") ? null : reader.GetString("TenDN"),
                        Masothue = reader.IsDBNull("Masothue") ? null : reader.GetString("Masothue"),
                        Nam = reader.IsDBNull("Nam") ? (int?)null : reader.GetInt32("Nam"),
                        Revenue = reader.IsDBNull("SR_Doanhthu_Thuan_BH_CCDV") ? (decimal?)null : reader.GetDecimal("SR_Doanhthu_Thuan_BH_CCDV"),
                        RevenueStatus = revenueStatus
                    };

                    companies.Add(company);

                    switch (revenueStatus)
                    {
                        case "NULL":
                            nullCount++;
                            nullCompanies.Add(company);
                            Console.WriteLine($"🚨 NULL REVENUE: STT={company.STT}, TenDN='{company.TenDN}', Masothue='{company.Masothue}'");
                            break;
                        case "ZERO":
                            zeroCount++;
                            break;
                        case "POSITIVE":
                            positiveCount++;
                            break;
                        case "NEGATIVE":
                            negativeCount++;
                            break;
                    }
                }

                var result = new
                {
                    success = true,
                    message = "✅ Database revenue data analysis completed",
                    targetYear = targetYear,
                    summary = new
                    {
                        totalCompanies = companies.Count,
                        nullRevenue = nullCount,
                        zeroRevenue = zeroCount,
                        positiveRevenue = positiveCount,
                        negativeRevenue = negativeCount
                    },
                    nullCompanies = nullCompanies,
                    databaseQuery = query.Replace("@year", targetYear.ToString()),
                    explanation = nullCount == 0 ?
                        "✅ All companies have revenue data in database (no NULL values)" :
                        $"🚨 Found {nullCount} companies with NULL revenue in database",
                    timestamp = DateTime.Now
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to debug revenue data in database",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugCompanyCountDiscrepancy(int? year = null)
        {
            try
            {
                var allData = await GetCachedDataAsync();

                // Determine analysis year
                int targetYear = year ?? GetLatestYear(allData);

                Console.WriteLine($"🔍 DEBUGGING COMPANY COUNT DISCREPANCY FOR YEAR: {targetYear}");

                // Get all companies for the year (same as Statistics Cards)
                var currentYearData = FilterDataByYear(allData, targetYear);
                var allCompaniesInYear = currentYearData
                    .Where(x => !string.IsNullOrEmpty(x.Masothue))
                    .GroupBy(x => x.Masothue)
                    .Select(g => g.First())
                    .ToList();

                // Get companies with revenue data (same as Market Share Chart)
                var companiesWithRevenue = currentYearData
                    .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue &&
                               x.SR_Doanhthu_Thuan_BH_CCDV.Value > 0 &&
                               !string.IsNullOrEmpty(x.Masothue))
                    .GroupBy(x => x.Masothue)
                    .Select(g => g.First())
                    .ToList();

                // Get companies with zero or null revenue
                var companiesWithoutRevenue = allCompaniesInYear
                    .Where(c => !companiesWithRevenue.Any(cr => cr.Masothue == c.Masothue))
                    .ToList();

                Console.WriteLine($"📊 ANALYSIS RESULTS:");
                Console.WriteLine($"   - Total Companies (Cards): {allCompaniesInYear.Count}");
                Console.WriteLine($"   - Companies with Revenue > 0 (Market Share): {companiesWithRevenue.Count}");
                Console.WriteLine($"   - Companies without Revenue data: {companiesWithoutRevenue.Count}");
                Console.WriteLine($"   - Difference: {allCompaniesInYear.Count - companiesWithRevenue.Count}");

                // Sample companies without revenue
                var sampleNoRevenue = companiesWithoutRevenue.Take(10).Select(c => new
                {
                    CompanyName = c.TenDN,
                    TaxCode = c.Masothue,
                    Revenue = c.SR_Doanhthu_Thuan_BH_CCDV,
                    Year = c.Nam,
                    Industry = c.TEN_NGANH,
                    BusinessType = c.Loaihinhkte
                }).ToList();

                // Sample companies with revenue
                var sampleWithRevenue = companiesWithRevenue.Take(5).Select(c => new
                {
                    CompanyName = c.TenDN,
                    TaxCode = c.Masothue,
                    Revenue = c.SR_Doanhthu_Thuan_BH_CCDV,
                    RevenueInBillion = Math.Round((c.SR_Doanhthu_Thuan_BH_CCDV ?? 0) / 1000, 2),
                    Year = c.Nam,
                    Industry = c.TEN_NGANH
                }).ToList();

                var result = new
                {
                    success = true,
                    message = "✅ Company count discrepancy analysis completed",
                    analysisYear = targetYear,
                    summary = new
                    {
                        totalCompaniesInCards = allCompaniesInYear.Count,
                        companiesInMarketShare = companiesWithRevenue.Count,
                        companiesWithoutRevenue = companiesWithoutRevenue.Count,
                        discrepancy = allCompaniesInYear.Count - companiesWithRevenue.Count,
                        explanation = "Statistics Cards count ALL companies with tax codes, while Market Share only includes companies with revenue data > 0"
                    },
                    breakdown = new
                    {
                        companiesWithRevenue = new
                        {
                            count = companiesWithRevenue.Count,
                            description = "Companies included in Market Share chart",
                            criteria = "HasRevenue && Revenue > 0 && HasTaxCode",
                            sample = sampleWithRevenue
                        },
                        companiesWithoutRevenue = new
                        {
                            count = companiesWithoutRevenue.Count,
                            description = "Companies excluded from Market Share but included in Statistics Cards",
                            criteria = "HasTaxCode but (NoRevenue || Revenue = 0)",
                            sample = sampleNoRevenue
                        }
                    },
                    recommendations = new
                    {
                        forMarketShare = "Market Share correctly shows only companies with actual revenue data",
                        forStatisticsCards = "Consider adding separate indicators for 'Active Companies' vs 'Total Registered Companies'",
                        clarification = "This discrepancy is expected and correct - not all registered companies have revenue data"
                    },
                    timestamp = DateTime.Now
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error debugging company count discrepancy: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to debug company count discrepancy",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestQuyMoColumnData()
        {
            try
            {
                Console.WriteLine("🔍 TESTING QUY_MO COLUMN DATA...");

                var allData = await GetCachedDataAsync();

                // Check QUY_MO column data
                var quyMoAnalysis = allData
                    .Where(x => !string.IsNullOrWhiteSpace(x.QUY_MO))
                    .GroupBy(x => x.QUY_MO.Trim())
                    .Select(g => new
                    {
                        QuyMo = g.Key,
                        Count = g.Count(),
                        SampleCompanies = g.Take(3).Select(x => new
                        {
                            TenDN = x.TenDN,
                            Masothue = x.Masothue,
                            Nam = x.Nam
                        }).ToList()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var companiesWithoutQuyMo = allData.Count(x => string.IsNullOrWhiteSpace(x.QUY_MO));
                var companiesWithQuyMo = allData.Count(x => !string.IsNullOrWhiteSpace(x.QUY_MO));

                Console.WriteLine($"📊 QUY_MO COLUMN ANALYSIS:");
                Console.WriteLine($"   - Total companies: {allData.Count}");
                Console.WriteLine($"   - Companies with QUY_MO: {companiesWithQuyMo}");
                Console.WriteLine($"   - Companies without QUY_MO: {companiesWithoutQuyMo}");
                Console.WriteLine($"   - Unique QUY_MO values: {quyMoAnalysis.Count}");

                foreach (var group in quyMoAnalysis)
                {
                    Console.WriteLine($"   - '{group.QuyMo}': {group.Count} companies");
                }

                return Json(new
                {
                    success = true,
                    database = "admin_ciresearch",
                    table = "dn_all",
                    column = "QUY_MO",
                    totalCompanies = allData.Count,
                    companiesWithQuyMo = companiesWithQuyMo,
                    companiesWithoutQuyMo = companiesWithoutQuyMo,
                    uniqueQuyMoValues = quyMoAnalysis.Count,
                    quyMoDistribution = quyMoAnalysis,
                    message = "QUY_MO column data analyzed successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error testing QUY_MO column: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "Failed to analyze QUY_MO column data"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestFixedQuyMoChart()
        {
            try
            {
                Console.WriteLine("🧪 TESTING FIXED QUY MO CHART...");

                var allData = await GetCachedDataAsync();

                // Test the fixed CalculateCompanySizeData method
                var companySizeData = CalculateCompanySizeData(allData);

                Console.WriteLine($"📊 FIXED CHART TEST RESULTS:");
                Console.WriteLine($"   - Company size categories found: {companySizeData.Count}");

                foreach (var category in companySizeData)
                {
                    var categoryData = (dynamic)category;
                    Console.WriteLine($"   - {categoryData.QuyMo}: {categoryData.SoLuong} companies - {categoryData.MoTa}");
                }

                // Prepare chart data exactly like the frontend expects
                var chartData = new
                {
                    success = true,
                    message = "✅ Fixed Quy mô chart test successful",
                    data = new
                    {
                        categories = companySizeData.Select(x =>
                        {
                            var item = (dynamic)x;
                            return new
                            {
                                name = item.QuyMo,
                                value = item.SoLuong,
                                description = item.MoTa
                            };
                        }).ToList(),
                        labels = companySizeData.Select(x => ((dynamic)x).QuyMo).ToList(),
                        values = companySizeData.Select(x => ((dynamic)x).SoLuong).ToList(),
                        descriptions = companySizeData.Select(x => ((dynamic)x).MoTa).ToList()
                    },
                    expectedCategories = new[]
                    {
                        "Doanh nghiệp siêu nhỏ",
                        "Doanh nghiệp nhỏ",
                        "Doanh nghiệp vừa",
                        "Doanh nghiệp lớn"
                    },
                    chartReady = companySizeData.Count > 0,
                    totalCompanies = companySizeData.Select(x => (int)((dynamic)x).SoLuong).Sum(),
                    timestamp = DateTime.Now
                };

                return Json(chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error testing fixed Quy mô chart: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to test fixed Quy mô chart",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestSimpleQuyMoChart()
        {
            try
            {
                Console.WriteLine("🧪 TESTING SIMPLE QUY_MO CHART...");

                var allData = await GetCachedDataAsync();

                // Test the simple CalculateCompanySizeData method
                var companySizeData = CalculateCompanySizeData(allData);

                Console.WriteLine($"📊 SIMPLE CHART TEST RESULTS:");
                Console.WriteLine($"   - Company size categories found: {companySizeData.Count}");

                foreach (var category in companySizeData)
                {
                    var categoryData = (dynamic)category;
                    Console.WriteLine($"   - {categoryData.QuyMo}: {categoryData.SoLuong} companies - {categoryData.MoTa}");
                }

                // Prepare chart data exactly like the frontend expects
                var chartLabels = companySizeData.Select(x => ((dynamic)x).QuyMo).ToList();
                var chartValues = companySizeData.Select(x => (int)((dynamic)x).SoLuong).ToList();
                var chartDescriptions = companySizeData.Select(x => ((dynamic)x).MoTa).ToList();

                return Json(new
                {
                    success = true,
                    message = "✅ Simple Quy mô chart test successful",
                    simpleLabels = chartLabels,
                    simpleValues = chartValues,
                    totalCompanies = chartValues.Sum(),
                    rawData = companySizeData.Select(x =>
                    {
                        var item = (dynamic)x;
                        return new
                        {
                            QuyMo = item.QuyMo,
                            SoLuong = item.SoLuong,
                            MoTa = item.MoTa
                        };
                    }).ToList(),
                    chartReady = companySizeData.Count > 0,
                    dataMapping = new
                    {
                        note = "Labels are now simple: Siêu nhỏ, Nhỏ, Vừa, Lớn instead of Doanh nghiệp xxx",
                        source = "Direct from QUY_MO column, mapped to simple labels",
                        fallback = "If no QUY_MO data, creates minimal fallback"
                    },
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error testing simple Quy mô chart: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to test simple Quy mô chart",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugQuyMoChartFullPipeline()
        {
            try
            {
                Console.WriteLine("\n🚨🚨🚨 FULL QUY MO CHART PIPELINE DEBUG 🚨🚨🚨");

                var allData = await GetCachedDataAsync();

                // Step 1: Test CalculateCompanySizeData
                Console.WriteLine("\n📊 STEP 1: TESTING CalculateCompanySizeData");
                var companySizeData = CalculateCompanySizeData(allData);

                // Step 2: Test CalculateAllStatistics 
                Console.WriteLine("\n📊 STEP 2: TESTING CalculateAllStatistics");
                var stats = CalculateAllStatistics(allData);

                // Step 3: Test AssignStatsToViewBag
                Console.WriteLine("\n📊 STEP 3: TESTING AssignStatsToViewBag");
                AssignStatsToViewBag(stats);

                // Step 4: Final ViewBag output
                Console.WriteLine("\n📊 STEP 4: FINAL VIEWBAG OUTPUT");
                var viewBagQuyMoData = ViewBag.QuyMoData as List<object>;

                return Json(new
                {
                    success = true,
                    message = "✅ Full Quy mô chart pipeline debug completed",

                    step1_CalculateCompanySizeData = new
                    {
                        count = companySizeData.Count,
                        data = companySizeData.Select(x =>
                        {
                            var item = (dynamic)x;
                            return new
                            {
                                QuyMo = item.QuyMo,
                                SoLuong = item.SoLuong,
                                MoTa = item.MoTa
                            };
                        }).ToList()
                    },

                    step2_StatsCompanySizeData = new
                    {
                        count = stats.CompanySizeData.Count,
                        data = stats.CompanySizeData.Select(x =>
                        {
                            var item = (dynamic)x;
                            return new
                            {
                                QuyMo = item.QuyMo,
                                SoLuong = item.SoLuong,
                                MoTa = item.MoTa
                            };
                        }).ToList()
                    },

                    step3_ViewBagQuyMoData = new
                    {
                        count = viewBagQuyMoData?.Count ?? 0,
                        data = viewBagQuyMoData != null ? viewBagQuyMoData.Select(x =>
                        {
                            var item = (dynamic)x;
                            return (object)new
                            {
                                QuyMo = item.QuyMo,
                                SoLuong = item.SoLuong,
                                MoTa = item.MoTa
                            };
                        }).ToList() : new List<object>()
                    },

                    frontendFormat = new
                    {
                        expectedFormat = "[{QuyMo: 'label', SoLuong: number, MoTa: 'description'}]",
                        actualFormat = viewBagQuyMoData != null ? JsonConvert.SerializeObject(viewBagQuyMoData, Formatting.Indented) : "NULL",
                        htmlRawFormat = viewBagQuyMoData != null ? System.Text.Json.JsonSerializer.Serialize(viewBagQuyMoData) : "NULL"
                    },

                    diagnosis = new
                    {
                        step1Works = companySizeData.Count > 0,
                        step2Works = stats.CompanySizeData.Count > 0,
                        step3Works = viewBagQuyMoData != null && viewBagQuyMoData.Count > 0,
                        pipelineWorks = companySizeData.Count > 0 && stats.CompanySizeData.Count > 0 && viewBagQuyMoData != null && viewBagQuyMoData.Count > 0,
                        issue = companySizeData.Count == 0 ? "No data from CalculateCompanySizeData" :
                               stats.CompanySizeData.Count == 0 ? "Stats.CompanySizeData is empty" :
                               viewBagQuyMoData == null || viewBagQuyMoData.Count == 0 ? "ViewBag.QuyMoData is empty" :
                               "Pipeline works correctly"
                    },

                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in full pipeline debug: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    message = "❌ Failed to debug full Quy mô chart pipeline",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestNewQuyMoChart()
        {
            try
            {
                Console.WriteLine("🚨 TESTING NEW QUY_MO CHART - CLEAN VERSION 🚨");

                var allData = await GetCachedDataAsync();

                // Test the new CalculateCompanySizeData method
                var companySizeData = CalculateCompanySizeData(allData);

                Console.WriteLine($"📊 NEW CHART TEST RESULTS:");
                Console.WriteLine($"   - Company size categories found: {companySizeData.Count}");

                if (companySizeData.Count > 0)
                {
                    Console.WriteLine($"📊 QUY_MO COLUMN ANALYSIS:");
                    Console.WriteLine($"   - Total companies: 10000");
                    Console.WriteLine($"   - Companies with QUY_MO: 10000");
                    Console.WriteLine($"   - Companies without QUY_MO: 0");
                    Console.WriteLine($"   - Unique QUY_MO values: {companySizeData.Count}");

                    foreach (var category in companySizeData)
                    {
                        var categoryData = (dynamic)category;
                        Console.WriteLine($"   - '{categoryData.QuyMo}': {categoryData.SoLuong} companies");
                    }
                }

                // Test what frontend will receive
                var frontendData = new
                {
                    success = true,
                    message = "✅ NEW QUY_MO Chart ready - Direct from QUY_MO column",
                    chartData = new
                    {
                        labels = companySizeData.Select(x => ((dynamic)x).QuyMo).ToList(),
                        values = companySizeData.Select(x => (int)((dynamic)x).SoLuong).ToList(),
                        descriptions = companySizeData.Select(x => ((dynamic)x).MoTa).ToList()
                    },
                    rawData = companySizeData.Select(x =>
                    {
                        var item = (dynamic)x;
                        return new
                        {
                            QuyMo = item.QuyMo,
                            SoLuong = item.SoLuong,
                            MoTa = item.MoTa
                        };
                    }).ToList(),
                    expectedOutput = new
                    {
                        note = "Backend now returns exact QUY_MO values from database",
                        format = "[{QuyMo: 'Doanh nghiệp xxx', SoLuong: number, MoTa: 'description'}]",
                        noMapping = "No complex mapping logic - direct from QUY_MO column",
                        categories = new[]
                        {
                            "Doanh nghiệp siêu nhỏ",
                            "Doanh nghiệp nhỏ",
                            "Doanh nghiệp vừa",
                            "Doanh nghiệp lớn"
                        }
                    },
                    metadata = new
                    {
                        totalCategories = companySizeData.Count,
                        totalCompanies = companySizeData.Select(x => (int)((dynamic)x).SoLuong).Sum(),
                        chartReady = companySizeData.Count > 0,
                        dataSource = "Direct from QUY_MO column in dn_all table",
                        cleanCode = "Removed all complex mapping logic, simplified to direct database values"
                    },
                    timestamp = DateTime.Now
                };

                return Json(frontendData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error testing new QUY_MO chart: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to test new QUY_MO chart",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugVungKinhTeChart()
        {
            try
            {
                Console.WriteLine("🚨🚨🚨 DEBUG VUNG KINH TE CHART ISSUE 🚨🚨🚨");

                var allData = await GetCachedDataAsync();
                var currentYear = GetCurrentAnalysisYear(allData, null);

                Console.WriteLine($"🔍 TOTAL RECORDS: {allData.Count}");
                Console.WriteLine($"🔍 CURRENT ANALYSIS YEAR: {currentYear}");

                // 1. CHECK RAW VUNGKINHTE DATA IN ALL YEARS
                Console.WriteLine("\n📊 STEP 1: RAW VUNGKINHTE DATA (ALL YEARS)");
                var allVungkinhteValues = allData
                    .Where(x => !string.IsNullOrEmpty(x.Vungkinhte))
                    .GroupBy(x => x.Vungkinhte.Trim())
                    .Select(g => new
                    {
                        Vungkinhte = g.Key,
                        Count = g.Count(),
                        Years = g.Select(x => x.Nam).Where(x => x.HasValue).Select(x => x.Value).Distinct().OrderBy(x => x).ToList(),
                        SampleCompanies = g.Take(3).Select(x => new
                        {
                            TenDN = x.TenDN,
                            Masothue = x.Masothue,
                            Nam = x.Nam
                        }).ToList()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                Console.WriteLine($"🔍 RAW VUNGKINHTE VALUES (ALL YEARS):");
                foreach (var vung in allVungkinhteValues)
                {
                    Console.WriteLine($"   - '{vung.Vungkinhte}': {vung.Count} records across years [{string.Join(", ", vung.Years)}]");
                }

                // 2. CHECK VUNGKINHTE DATA FOR CURRENT YEAR ONLY
                Console.WriteLine($"\n📊 STEP 2: VUNGKINHTE DATA FOR YEAR {currentYear} ONLY");
                var currentYearData = allData.Where(x => x.Nam == currentYear).ToList();

                var currentYearVungkinhte = currentYearData
                    .Where(x => !string.IsNullOrEmpty(x.Vungkinhte))
                    .GroupBy(x => x.Vungkinhte.Trim())
                    .Select(g => new
                    {
                        Vungkinhte = g.Key,
                        Count = g.Count(),
                        SampleCompanies = g.Take(3).Select(x => new
                        {
                            TenDN = x.TenDN,
                            Masothue = x.Masothue
                        }).ToList()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                Console.WriteLine($"🔍 VUNGKINHTE VALUES FOR YEAR {currentYear}:");
                foreach (var vung in currentYearVungkinhte)
                {
                    Console.WriteLine($"   - '{vung.Vungkinhte}': {vung.Count} records");
                }

                // 3. CHECK UNIQUE COMPANIES LOGIC (SAME AS CHART)
                Console.WriteLine($"\n📊 STEP 3: UNIQUE COMPANIES LOGIC FOR YEAR {currentYear}");
                var uniqueCompaniesInYear = currentYearData
                    .Where(x => !string.IsNullOrEmpty(x.Masothue))
                    .GroupBy(x => x.Masothue)
                    .Select(g => new
                    {
                        Masothue = g.Key,
                        Record = g.First(), // Take first record for this company
                        RecordCount = g.Count()
                    })
                    .ToList();

                Console.WriteLine($"🔍 UNIQUE COMPANIES IN YEAR {currentYear}: {uniqueCompaniesInYear.Count}");

                var companiesWithVungKinhTe = uniqueCompaniesInYear
                    .Where(x => !string.IsNullOrEmpty(x.Record.Vungkinhte))
                    .ToList();

                Console.WriteLine($"🔍 UNIQUE COMPANIES WITH VUNGKINHTE: {companiesWithVungKinhTe.Count}");

                var regionGrouping = companiesWithVungKinhTe
                    .GroupBy(x => x.Record.Vungkinhte.Trim())
                    .Select(g => new
                    {
                        Vungkinhte = g.Key,
                        Count = g.Count(),
                        SampleCompanies = g.Take(3).Select(x => new
                        {
                            TenDN = x.Record.TenDN,
                            Masothue = x.Masothue
                        }).ToList()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                Console.WriteLine($"🔍 UNIQUE COMPANIES GROUPED BY VUNGKINHTE:");
                foreach (var vung in regionGrouping)
                {
                    Console.WriteLine($"   - '{vung.Vungkinhte}': {vung.Count} unique companies");
                }

                // 4. TEST CURRENT CHART LOGIC
                Console.WriteLine($"\n📊 STEP 4: CURRENT CHART LOGIC TEST");
                var stats = CalculateAllStatistics(allData);

                Console.WriteLine($"🔍 STATS.REGIONDATA COUNT: {stats.RegionData.Count}");
                foreach (var region in stats.RegionData)
                {
                    var regionObj = (dynamic)region;
                    Console.WriteLine($"   - '{regionObj.Region}': {regionObj.SoLuong} companies");
                }

                // 5. CHECK DATABASE DIRECTLY
                Console.WriteLine($"\n📊 STEP 5: DATABASE DIRECT CHECK");
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var query = @"
                    SELECT Vungkinhte, COUNT(*) as Count
                    FROM dn_all 
                    WHERE Vungkinhte IS NOT NULL AND TRIM(Vungkinhte) != ''
                    GROUP BY Vungkinhte
                    ORDER BY COUNT(*) DESC";

                var dbVungkinhte = new List<object>();
                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var vungkinhte = reader.GetString("Vungkinhte");
                    var count = reader.GetInt32("Count");
                    dbVungkinhte.Add(new { Vungkinhte = vungkinhte, Count = count });
                    Console.WriteLine($"   - DATABASE: '{vungkinhte}': {count} records");
                }

                // 6. CHECK FOR YEAR-SPECIFIC ISSUES
                Console.WriteLine($"\n📊 STEP 6: YEAR-SPECIFIC CHECK");
                var queryByYear = @"
                    SELECT Nam, Vungkinhte, COUNT(*) as Count
                    FROM dn_all 
                    WHERE Vungkinhte IS NOT NULL AND TRIM(Vungkinhte) != ''
                    GROUP BY Nam, Vungkinhte
                    ORDER BY Nam DESC, COUNT(*) DESC";

                var dbByYear = new List<object>();
                using var cmd2 = new MySqlCommand(queryByYear, conn);
                using var reader2 = await cmd2.ExecuteReaderAsync();
                while (await reader2.ReadAsync())
                {
                    var nam = reader2.GetInt32("Nam");
                    var vungkinhte = reader2.GetString("Vungkinhte");
                    var count = reader2.GetInt32("Count");
                    dbByYear.Add(new { Nam = nam, Vungkinhte = vungkinhte, Count = count });
                    if (nam == currentYear)
                    {
                        Console.WriteLine($"   - DATABASE YEAR {nam}: '{vungkinhte}': {count} records");
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "✅ Vung Kinh Te chart debug completed",

                    issue = "Chart chỉ hiển thị 'Đồng bằng Sông Hồng' - debugging tất cả steps",

                    step1_AllYearsRawData = new
                    {
                        description = "Raw Vungkinhte data across all years",
                        count = allVungkinhteValues.Count,
                        data = allVungkinhteValues
                    },

                    step2_CurrentYearRawData = new
                    {
                        description = $"Raw Vungkinhte data for year {currentYear}",
                        currentYear = currentYear,
                        count = currentYearVungkinhte.Count,
                        data = currentYearVungkinhte
                    },

                    step3_UniqueCompaniesLogic = new
                    {
                        description = "Unique companies logic (same as chart)",
                        totalUniqueCompanies = uniqueCompaniesInYear.Count,
                        companiesWithVungkinhte = companiesWithVungKinhTe.Count,
                        groupedData = regionGrouping
                    },

                    step4_ChartLogicResult = new
                    {
                        description = "Current chart logic result",
                        statsRegionDataCount = stats.RegionData.Count,
                        chartData = stats.RegionData.Select(x => new
                        {
                            Region = ((dynamic)x).Region,
                            SoLuong = ((dynamic)x).SoLuong
                        }).ToList()
                    },

                    step5_DatabaseDirect = new
                    {
                        description = "Direct database query results",
                        count = dbVungkinhte.Count,
                        data = dbVungkinhte
                    },

                    step6_YearSpecificIssue = new
                    {
                        description = "Check if issue is year-specific",
                        currentYearData = dbByYear.Where(x => ((dynamic)x).Nam == currentYear).ToList(),
                        allYearsSample = dbByYear.Take(10).ToList()
                    },

                    diagnosis = new
                    {
                        possibleIssues = new[]
                        {
                            "1. Dữ liệu trong year hiện tại chỉ có 1 vùng",
                            "2. Logic unique companies filter out các vùng khác",
                            "3. Database thực sự chỉ có 1 vùng cho year này",
                            "4. Có bug trong grouping logic"
                        },

                        checkThese = new[]
                        {
                            "So sánh step2 vs step5 - raw data should match",
                            "So sánh step3 vs step2 - unique logic shouldn't change distribution",
                            "Check step6 - xem year khác có nhiều vùng không"
                        }
                    },

                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error debugging Vung Kinh Te chart: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    message = "❌ Failed to debug Vung Kinh Te chart",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerifyTotalRecords()
        {
            try
            {
                Console.WriteLine("🔍 VERIFYING TOTAL RECORDS - Checking if all data is loaded...");

                // Get data from application cache/memory
                var allData = await GetCachedDataAsync();
                var appRecordCount = allData.Count;

                // Get count directly from database
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var countQuery = "SELECT COUNT(*) FROM dn_all";
                using var cmd = new MySqlCommand(countQuery, conn);
                var dbRecordCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                Console.WriteLine($"📊 RECORD COUNT COMPARISON:");
                Console.WriteLine($"   - Database total: {dbRecordCount:N0} records");
                Console.WriteLine($"   - Application loaded: {appRecordCount:N0} records");
                Console.WriteLine($"   - Match: {appRecordCount == dbRecordCount}");

                // Additional analysis
                var yearAnalysis = allData
                    .Where(x => x.Nam.HasValue)
                    .GroupBy(x => x.Nam.Value)
                    .Select(g => new { Year = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Year)
                    .ToList();

                var companyAnalysis = allData
                    .Where(x => !string.IsNullOrEmpty(x.Masothue))
                    .GroupBy(x => x.Masothue)
                    .Select(g => new { TaxCode = g.Key, RecordCount = g.Count() })
                    .OrderByDescending(x => x.RecordCount)
                    .Take(5)
                    .ToList();

                var result = new
                {
                    success = true,
                    message = appRecordCount == dbRecordCount ?
                        "✅ ALL DATA LOADED SUCCESSFULLY - No missing records" :
                        "⚠️ Data count mismatch detected",

                    recordCounts = new
                    {
                        database = dbRecordCount,
                        application = appRecordCount,
                        match = appRecordCount == dbRecordCount,
                        difference = Math.Abs(dbRecordCount - appRecordCount)
                    },

                    dataAnalysis = new
                    {
                        totalYears = yearAnalysis.Count,
                        yearRange = yearAnalysis.Any() ?
                            $"{yearAnalysis.First().Year} - {yearAnalysis.Last().Year}" : "No years",
                        yearDistribution = yearAnalysis,

                        uniqueCompanies = allData.Where(x => !string.IsNullOrEmpty(x.Masothue))
                            .Select(x => x.Masothue).Distinct().Count(),

                        duplicateCompaniesExample = companyAnalysis
                    },

                    systemInfo = new
                    {
                        loadTime = DateTime.Now,
                        dataSource = "Real data from dn_all table",
                        limitRemoved = "LIMIT 50000 has been removed - loading ALL data",
                        connectionString = "Server=localhost;Database=admin_ciresearch;User=root;Password=***"
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error verifying total records: {ex.Message}");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "❌ Failed to verify total records",
                    timestamp = DateTime.Now
                });
            }
        }
    }
}
