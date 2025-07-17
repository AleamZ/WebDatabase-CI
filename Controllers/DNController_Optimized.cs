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
    public class DNController_Optimized : Controller
    {
        private readonly IMemoryCache _cache;
        private string _connectionString = "Server=localhost;Database=sakila;User=root;Password=1234";
        private const string DATA_CACHE_KEY = "dn_all_data";
        private const int CACHE_DURATION_MINUTES = 30;

        public DNController_Optimized(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<IActionResult> Index(string stt = "",
            List<string> Nam = null,
            List<string> MaTinh_Dieutra = null,
            List<string> Masothue = null,
            List<string> Loaihinhkte = null,
            List<string> Vungkinhte = null)
        {
            try
            {
                // 1. Get cached data or fetch from database
                var allData = await GetCachedDataAsync();
                if (!allData.Any())
                {
                    ViewBag.ErrorMessage = "Không có dữ liệu để hiển thị";
                    return View(new List<QLKH>());
                }

                // 2. Apply filters efficiently
                var filteredData = ApplyFiltersOptimized(allData, stt, Nam, MaTinh_Dieutra, Masothue, Loaihinhkte, Vungkinhte);

                // 3. Calculate all statistics in one pass
                var stats = CalculateAllStatistics(filteredData);
                AssignStatsToViewBag(stats);

                // 4. Set last import time
                ViewBag.LastImportTime = await GetLastImportTimeAsync();

                return View(filteredData);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi xử lý dữ liệu: {ex.Message}";
                return View(new List<QLKH>());
            }
        }

        #region Optimized Data Access

        private async Task<List<QLKH>> GetCachedDataAsync()
        {
            if (_cache.TryGetValue(DATA_CACHE_KEY, out List<QLKH> cachedData))
            {
                return cachedData;
            }

            var data = await GetDataFromDatabaseAsync();
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                .SetSize(1); // Add size for cache entry
            _cache.Set(DATA_CACHE_KEY, data, cacheOptions);
            return data;
        }

        private async Task<List<QLKH>> GetDataFromDatabaseAsync()
        {
            var result = new List<QLKH>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT STT, TenDN, Diachi, MaTinh_Dieutra, MaHuyen_Dieutra, MaXa_Dieutra,
                           DNTB_MaTinh, DNTB_MaHuyen, DNTB_MaXa, Region, Loaihinhkte, 
                           Nam, Masothue, Vungkinhte, SoLaodong_DauNam, SoLaodong_CuoiNam,
                           SR_Doanhthu_Thuan_BH_CCDV, SR_Loinhuan_TruocThue, 
                           Taisan_Tong_CK, Taisan_Tong_DK, MaNganhC5_Chinh, TEN_NGANH,
                           Email, Dienthoai
                    FROM dn_all 
                    ORDER BY STT";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(CreateQLKHFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
            }

            return result;
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
                Nam = GetSafeNullableInt(reader, "Nam"),
                Masothue = GetSafeString(reader, "Masothue"),
                Vungkinhte = GetSafeString(reader, "Vungkinhte"),
                SoLaodong_DauNam = GetSafeNullableInt(reader, "SoLaodong_DauNam"),
                SoLaodong_CuoiNam = GetSafeNullableInt(reader, "SoLaodong_CuoiNam"),
                SR_Doanhthu_Thuan_BH_CCDV = GetSafeNullableDecimal(reader, "SR_Doanhthu_Thuan_BH_CCDV"),
                SR_Loinhuan_TruocThue = GetSafeNullableDecimal(reader, "SR_Loinhuan_TruocThue"),
                Taisan_Tong_CK = GetSafeNullableDecimal(reader, "Taisan_Tong_CK"),
                Taisan_Tong_DK = GetSafeNullableDecimal(reader, "Taisan_Tong_DK"),
                MaNganhC5_Chinh = GetSafeString(reader, "MaNganhC5_Chinh"),
                TEN_NGANH = GetSafeString(reader, "TEN_NGANH"),
                Email = GetSafeString(reader, "Email"),
                Dienthoai = GetSafeString(reader, "Dienthoai")
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
                return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
            }
            catch
            {
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
            List<string> Nam, List<string> MaTinh_Dieutra, List<string> Masothue,
            List<string> Loaihinhkte, List<string> Vungkinhte)
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
            public Dictionary<string, int> TechStats { get; set; } = new();
            public Dictionary<string, decimal> FinancialStats { get; set; } = new();
            public List<object> ProvinceData { get; set; } = new();
            public List<object> RegionData { get; set; } = new();
            public List<object> BusinessTypeData { get; set; } = new();
            public List<object> CompanySizeData { get; set; } = new();
            public List<object> CapitalData { get; set; } = new();
            public List<int> Years { get; set; } = new();
            public List<double> RevenueData { get; set; } = new();
            public List<double> ProfitData { get; set; } = new();
        }

        private ComprehensiveStats CalculateAllStatistics(List<QLKH> data)
        {
            var stats = new ComprehensiveStats
            {
                TotalCompanies = data.Count
            };

            // Single pass through data for basic stats
            foreach (var item in data)
            {
                // Labor calculation
                if (item.SoLaodong_CuoiNam.HasValue)
                    stats.TotalLabor += item.SoLaodong_CuoiNam.Value;

                // Region counting
                if (!string.IsNullOrEmpty(item.Vungkinhte))
                {
                    foreach (var region in GetRegionMappings().Keys)
                    {
                        if (item.Vungkinhte.Contains(region, StringComparison.OrdinalIgnoreCase))
                        {
                            var key = GetRegionMappings()[region];
                            stats.RegionCounts[key] = stats.RegionCounts.GetValueOrDefault(key) + 1;
                        }
                    }
                }

                // Tech stats
                stats.TechStats["Internet"] = stats.TechStats.GetValueOrDefault("Internet") +
                    (HasTech(item.Taisan_Tong_CK) ? 1 : 0);
                stats.TechStats["Website"] = stats.TechStats.GetValueOrDefault("Website") +
                    (HasTech(item.Taisan_Tong_DK) ? 1 : 0);
                stats.TechStats["Software"] = stats.TechStats.GetValueOrDefault("Software") +
                    (HasTech(item.MaNganhC5_Chinh) ? 1 : 0);
            }

            // LINQ-based grouped calculations
            stats.ProvinceData = data
                .Where(x => !string.IsNullOrEmpty(x.DNTB_MaTinh))
                .GroupBy(x => x.DNTB_MaTinh)
                .Select(g => new { MaTinh_Dieutra = g.Key, SampleCount = g.Count() })
                .OrderByDescending(x => x.SampleCount)
                .Cast<object>()
                .ToList();

            stats.RegionData = GetRegionMappings().Values.Distinct()
                .Select(region => new { Region = region, SoLuong = stats.RegionCounts.GetValueOrDefault(region, 0) })
                .Cast<object>()
                .ToList();

            stats.BusinessTypeData = data
                .Where(x => !string.IsNullOrEmpty(x.Loaihinhkte))
                .GroupBy(x => x.Loaihinhkte)
                .Select(g => new { TinhTrang = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .Cast<object>()
                .ToList();

            stats.CompanySizeData = CalculateCompanySizeOptimized(data);

            stats.CapitalData = data
                .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue)
                .GroupBy(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value.ToString("N0"))
                .Select(g => new { TinhTrang = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .Cast<object>()
                .ToList();

            // Financial and revenue data
            CalculateFinancialData(data, stats);
            CalculateRevenueData(data, stats);

            return stats;
        }

        private static Dictionary<string, string> GetRegionMappings()
        {
            return new Dictionary<string, string>
            {
                ["Trung du và Miền núi Bắc Bộ"] = "Miền Bắc",
                ["Đồng bằng Sông Hồng"] = "Miền Bắc",
                ["Bắc Trung Bộ"] = "Miền Trung",
                ["Duyên hải Nam Trung Bộ"] = "Miền Trung",
                ["Tây Nguyên"] = "Miền Trung",
                ["Đông Nam Bộ"] = "Miền Nam",
                ["Đồng bằng Sông Cửu Long"] = "Miền Nam"
            };
        }

        private static bool HasTech(decimal? value)
        {
            return value.HasValue && value.Value > 0;
        }

        private static bool HasTech(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                   (value.Equals("Có", StringComparison.OrdinalIgnoreCase) || value == "1");
        }

        private static List<object> CalculateCompanySizeOptimized(List<QLKH> data)
        {
            var sizes = new int[4]; // [sieuNho, nho, vua, lon]

            foreach (var item in data)
            {
                if (!item.SR_Doanhthu_Thuan_BH_CCDV.HasValue)
                    continue;

                var doanhthuTy = item.SR_Doanhthu_Thuan_BH_CCDV.Value / 1000;

                switch (doanhthuTy)
                {
                    case <= 3:
                        sizes[0]++;
                        break;
                    case <= 50:
                        sizes[1]++;
                        break;
                    case <= 300:
                        sizes[2]++;
                        break;
                    default:
                        var hasLargeAssets = item.Taisan_Tong_CK.HasValue &&
                                           (item.Taisan_Tong_CK.Value / 1000) > 100;
                        sizes[hasLargeAssets ? 3 : 2]++;
                        break;
                }
            }

            return new List<object>
            {
                new { QuyMo = "Siêu nhỏ (≤ 3 tỷ)", SoLuong = sizes[0] },
                new { QuyMo = "Nhỏ (3-50 tỷ)", SoLuong = sizes[1] },
                new { QuyMo = "Vừa (50-300 tỷ)", SoLuong = sizes[2] },
                new { QuyMo = "Lớn (>300 tỷ & >100 tỷ TS)", SoLuong = sizes[3] }
            };
        }

        private static void CalculateFinancialData(List<QLKH> data, ComprehensiveStats stats)
        {
            var revenueData = data
                .Where(x => x.SR_Doanhthu_Thuan_BH_CCDV.HasValue)
                .Select(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value)
                .ToList();

            var assetData = data
                .Where(x => x.Taisan_Tong_CK.HasValue)
                .Select(x => x.Taisan_Tong_CK.Value)
                .ToList();

            stats.FinancialStats["TotalRevenue"] = revenueData.Sum();
            stats.FinancialStats["AverageRevenue"] = revenueData.Any() ? revenueData.Average() : 0;
            stats.FinancialStats["CompaniesWithRevenue"] = revenueData.Count;
            stats.FinancialStats["TotalAssets"] = assetData.Sum();
            stats.FinancialStats["AverageAssets"] = assetData.Any() ? assetData.Average() : 0;
            stats.FinancialStats["CompaniesWithAssets"] = assetData.Count;
        }

        private static void CalculateRevenueData(List<QLKH> data, ComprehensiveStats stats)
        {
            var revenueAndProfitByYear = data
                .Where(x => x.Nam.HasValue && x.SR_Doanhthu_Thuan_BH_CCDV.HasValue)
                .GroupBy(x => x.Nam.Value)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalRevenue = g.Sum(x => x.SR_Doanhthu_Thuan_BH_CCDV.Value)
                })
                .OrderBy(x => x.Year)
                .ToList();

            if (!revenueAndProfitByYear.Any())
            {
                // Demo data if no real data
                var demoData = new[]
                {
                    new { Year = 2020, TotalRevenue = 1500000m },
                    new { Year = 2021, TotalRevenue = 1800000m },
                    new { Year = 2022, TotalRevenue = 2200000m },
                    new { Year = 2023, TotalRevenue = 2800000m },
                    new { Year = 2024, TotalRevenue = 3200000m }
                };
                revenueAndProfitByYear = demoData.ToList();
            }

            stats.Years = revenueAndProfitByYear.Select(x => x.Year).ToList();
            stats.RevenueData = revenueAndProfitByYear.Select(x => (double)x.TotalRevenue).ToList();
        }

        private void AssignStatsToViewBag(ComprehensiveStats stats)
        {
            // Basic stats
            ViewBag.TotalCompanies = stats.TotalCompanies;
            ViewBag.TotalLabor = stats.TotalLabor;

            // Region stats
            ViewBag.MienBacCount = stats.RegionCounts.GetValueOrDefault("Miền Bắc", 0);
            ViewBag.MienTrungCount = stats.RegionCounts.GetValueOrDefault("Miền Trung", 0);
            ViewBag.MienNamCount = stats.RegionCounts.GetValueOrDefault("Miền Nam", 0);

            // Tech stats
            ViewBag.CoInternet = stats.TechStats.GetValueOrDefault("Internet", 0);
            ViewBag.CoWebsite = stats.TechStats.GetValueOrDefault("Website", 0);
            ViewBag.CoPhanmem = stats.TechStats.GetValueOrDefault("Software", 0);

            // Tech percentages
            var totalCompanies = stats.TotalCompanies;
            ViewBag.InternetPercent = totalCompanies > 0 ? Math.Round((double)ViewBag.CoInternet / totalCompanies * 100, 1) : 0;
            ViewBag.WebsitePercent = totalCompanies > 0 ? Math.Round((double)ViewBag.CoWebsite / totalCompanies * 100, 1) : 0;
            ViewBag.PhanmemPercent = totalCompanies > 0 ? Math.Round((double)ViewBag.CoPhanmem / totalCompanies * 100, 1) : 0;

            // Financial stats
            ViewBag.TotalRevenue = stats.FinancialStats.GetValueOrDefault("TotalRevenue", 0);
            ViewBag.AverageRevenue = stats.FinancialStats.GetValueOrDefault("AverageRevenue", 0);
            ViewBag.CompaniesWithRevenue = (int)stats.FinancialStats.GetValueOrDefault("CompaniesWithRevenue", 0);
            ViewBag.TotalAssets = stats.FinancialStats.GetValueOrDefault("TotalAssets", 0);
            ViewBag.AverageAssets = stats.FinancialStats.GetValueOrDefault("AverageAssets", 0);
            ViewBag.CompaniesWithAssets = (int)stats.FinancialStats.GetValueOrDefault("CompaniesWithAssets", 0);

            // Chart data
            ViewBag.ProvinceData = JsonConvert.SerializeObject(stats.ProvinceData);
            ViewBag.RegionData = JsonConvert.SerializeObject(stats.RegionData);
            ViewBag.loaihinhData = JsonConvert.SerializeObject(stats.BusinessTypeData);
            ViewBag.CompanySizeData = JsonConvert.SerializeObject(stats.CompanySizeData);
            ViewBag.VonNNTWData = JsonConvert.SerializeObject(stats.CapitalData);
            ViewBag.Years = stats.Years;
            ViewBag.RevenueData = stats.RevenueData;
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
                    Internet = stats.TechStats.GetValueOrDefault("Internet", 0),
                    Website = stats.TechStats.GetValueOrDefault("Website", 0),
                    Software = stats.TechStats.GetValueOrDefault("Software", 0)
                },
                Provinces = stats.ProvinceData.Count,
                Regions = stats.RegionData.Count,
                BusinessTypes = stats.BusinessTypeData.Count
            };

            return Json(summary);
        }

        #endregion

        #region Export Functions

        public async Task<IActionResult> ExportToExcel(string stt = "", List<string> Nam = null,
            List<string> MaTinh_Dieutra = null, List<string> Masothue = null,
            List<string> Loaihinhkte = null, List<string> Vungkinhte = null)
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

        #region Cache Management

        [HttpPost]
        public IActionResult ClearCache()
        {
            _cache.Remove(DATA_CACHE_KEY);
            return Json(new { success = true, message = "Cache cleared successfully" });
        }

        #endregion


    }
}