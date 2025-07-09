using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;
using CIResearch.Models;
using System.Linq;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using CIResearch.Services;
using Microsoft.Extensions.Logging;

namespace CIResearch.Controllers
{
    /// <summary>
    /// 🚀 ULTRA OPTIMIZED DN CONTROLLER FOR MILLIONS OF RECORDS
    /// - Pagination: Load only displayed data
    /// - Lazy Loading: Load charts on demand
    /// - Smart Caching: Multi-level cache strategy
    /// - Database Optimization: Heavy computation moved to DB
    /// - Memory Efficient: Never load all data into memory
    /// </summary>
    public class OptimizedDNController : Controller
    {
        private readonly IOptimizedDNService _dnService;
        private readonly ILogger<OptimizedDNController> _logger;

        public OptimizedDNController(IOptimizedDNService dnService, ILogger<OptimizedDNController> logger)
        {
            _dnService = dnService;
            _logger = logger;
        }

        /// <summary>
        /// 🚀 OPTIMIZED INDEX - LOADS ONLY SUMMARY + FIRST PAGE
        /// </summary>
        public async Task<IActionResult> Index(
            int page = 1,
            int size = 50,
            List<int>? years = null,
            List<string>? provinces = null,
            List<string>? businessTypes = null,
            List<string>? economicZones = null,
            string? searchTerm = null)
        {
            try
            {
                _logger.LogInformation("🚀 Optimized Index called - Page: {Page}, Size: {Size}", page, size);

                // 🔥 STEP 1: Get Dashboard Summary (Cached, Fast)
                var filters = new FilterRequest
                {
                    Year = years?.FirstOrDefault(),
                    Province = provinces?.FirstOrDefault() ?? "",
                    BusinessType = businessTypes?.FirstOrDefault() ?? "",
                    EconomicZone = economicZones?.FirstOrDefault() ?? ""
                };

                var summaryTask = _dnService.GetDashboardSummaryAsync(filters);

                // 🔥 STEP 2: Get Paginated Data (Only current page)
                var paginationRequest = new PaginationRequest
                {
                    Page = page,
                    Size = size,
                    Filters = filters
                };

                var dataTask = _dnService.GetPaginatedDataAsync(paginationRequest);

                // 🔥 PARALLEL EXECUTION
                await Task.WhenAll(summaryTask, dataTask);

                var summary = await summaryTask;
                var paginatedData = await dataTask;

                // 🔥 STEP 3: Assign to ViewBag (Lightweight)
                AssignSummaryToViewBag(summary);
                ViewBag.PaginationInfo = new
                {
                    paginatedData.Page,
                    paginatedData.Size,
                    TotalRecords = paginatedData.TotalCount,
                    paginatedData.TotalPages,
                    paginatedData.HasNext,
                    paginatedData.HasPrevious
                };

                // 🔥 STEP 4: Set filter options for UI
                ViewBag.CurrentFilters = filters;
                ViewBag.AvailableFilters = await GetAvailableFiltersAsync();

                _logger.LogInformation("✅ Index completed successfully - Loaded {RecordCount} records", paginatedData.Data.Count);

                return View(paginatedData.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in optimized Index");
                ViewBag.Error = "Có lỗi xảy ra khi tải dữ liệu. Vui lòng thử lại.";
                return View(new List<QLKH>());
            }
        }

        /// <summary>
        /// 🚀 API: GET CHART DATA ON DEMAND
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType, [FromQuery] FilterRequest filters)
        {
            try
            {
                _logger.LogInformation("🚀 Getting chart data for {ChartType}", chartType);

                var chartData = await _dnService.GetChartDataAsync(chartType, filters);

                return Json(new
                {
                    success = true,
                    chartType = chartType,
                    data = chartData,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting chart data for {ChartType}", chartType);
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    chartType = chartType
                });
            }
        }

        /// <summary>
        /// 🚀 API: GET PAGINATED DATA (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetPaginatedData([FromBody] PaginationRequest request)
        {
            try
            {
                _logger.LogInformation("🚀 Getting paginated data - Page: {Page}, Size: {Size}", request.Page, request.Size);

                var result = await _dnService.GetPaginatedDataAsync(request);

                return Json(new
                {
                    success = true,
                    data = result.Data,
                    pagination = new
                    {
                        result.Page,
                        result.Size,
                        TotalRecords = result.TotalCount,
                        result.TotalPages,
                        result.HasNext,
                        result.HasPrevious
                    },
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting paginated data");
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 🚀 API: GET DASHBOARD SUMMARY
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] FilterRequest filters)
        {
            try
            {
                var summary = await _dnService.GetDashboardSummaryAsync(filters);

                return Json(new
                {
                    success = true,
                    summary = summary,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting dashboard summary");
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 🚀 API: INVALIDATE CACHE
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InvalidateCache()
        {
            try
            {
                await _dnService.RefreshCacheAsync();

                return Json(new
                {
                    success = true,
                    message = "Cache invalidated successfully",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error invalidating cache");
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 🚀 API: GET PERFORMANCE METRICS
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPerformanceMetrics()
        {
            try
            {
                // Get basic performance info
                var metrics = new
                {
                    totalMemoryUsage = GC.GetTotalMemory(false),
                    gen0Collections = GC.CollectionCount(0),
                    gen1Collections = GC.CollectionCount(1),
                    gen2Collections = GC.CollectionCount(2),
                    timestamp = DateTime.Now
                };

                return Json(new
                {
                    success = true,
                    metrics = metrics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting performance metrics");
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        #region Private Helper Methods

        private void AssignSummaryToViewBag(DashboardSummary summary)
        {
            // Basic stats
            ViewBag.TotalCompanies = summary.TotalCompanies;
            ViewBag.TotalLabor = summary.TotalLabor;
            ViewBag.CurrentAnalysisYear = summary.LatestYear;

            // Financial stats
            ViewBag.CompaniesWithRevenue = summary.CompaniesWithRevenue;
            ViewBag.CompaniesWithAssets = summary.CompaniesWithAssets;
            ViewBag.TotalRevenue = summary.TotalRevenue;
            ViewBag.TotalAssets = summary.TotalAssets;

            // Default values for other stats (will be loaded via AJAX)
            ViewBag.MienBacCount = 0;
            ViewBag.MienTrungCount = 0;
            ViewBag.MienNamCount = 0;
            ViewBag.TopBusinessType1Name = "Loading...";
            ViewBag.TopBusinessType1Count = 0;
            ViewBag.TopBusinessType2Name = "Loading...";
            ViewBag.TopBusinessType2Count = 0;
            ViewBag.TopBusinessType3Name = "Loading...";
            ViewBag.TopBusinessType3Count = 0;

            // Chart data will be loaded via AJAX
            ViewBag.RegionDataJson = "[]";
            ViewBag.BusinessTypeDataJson = "[]";
            ViewBag.IndustryDataJson = "[]";
            ViewBag.CompanySizeDataJson = "[]";
            ViewBag.ProvinceDataJson = "[]";
            ViewBag.RevenueDataJson = "[]";
            ViewBag.ProfitDataJson = "[]";

            _logger.LogInformation("✅ Summary assigned to ViewBag - Companies: {Companies}, Labor: {Labor}",
                summary.TotalCompanies, summary.TotalLabor);
        }

        private async Task<object> GetAvailableFiltersAsync()
        {
            // These will be cached and loaded efficiently
            return new
            {
                years = new[] { 2020, 2021, 2022, 2023, 2024 },
                provinces = new[] { "01", "79", "48" }, // Sample provinces
                businessTypes = new[] { "Công ty TNHH", "Công ty cổ phần", "Doanh nghiệp tư nhân" },
                economicZones = new[] { "Đông Nam Bộ", "Đồng bằng Sông Hồng", "Duyên hải Nam Trung Bộ" }
            };
        }

        #endregion
    }
}