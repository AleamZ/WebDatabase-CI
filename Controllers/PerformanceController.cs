using Microsoft.AspNetCore.Mvc;
using CIResearch.Models;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace CIResearch.Controllers;

/// <summary>
/// 🚀 PERFORMANCE MONITORING CONTROLLER
/// Real-time system monitoring & optimization metrics
/// </summary>
public class PerformanceController : Controller
{
    private readonly IGlobalCacheService _cache;
    private readonly ILogger<PerformanceController> _logger;
    private readonly IMemoryCache _memoryCache;

    public PerformanceController(
        IGlobalCacheService cache,
        ILogger<PerformanceController> logger,
        IMemoryCache memoryCache)
    {
        _cache = cache;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// 🚀 PERFORMANCE DASHBOARD
    /// </summary>
    public IActionResult Index()
    {
        var metrics = GetSystemMetrics();
        return View(metrics);
    }

    /// <summary>
    /// 🚀 REAL-TIME METRICS API
    /// </summary>
    [HttpGet]
    public IActionResult GetMetrics()
    {
        var metrics = GetSystemMetrics();
        return Json(metrics);
    }

    /// <summary>
    /// 🚀 CACHE STATISTICS API
    /// </summary>
    [HttpGet]
    public IActionResult CacheStats()
    {
        var stats = _cache.GetStatistics();
        return Json(new
        {
            stats.Hits,
            stats.Misses,
            stats.Total,
            HitRatio = Math.Round(stats.HitRatio, 2),
            stats.TotalKeys,
            UptimeMinutes = (DateTime.UtcNow - stats.StartTime).TotalMinutes
        });
    }

    /// <summary>
    /// 🚀 CLEAR CACHE ACTION
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ClearCache(string? pattern = null)
    {
        try
        {
            await _cache.ClearAsync(pattern);
            _logger.LogInformation("🧹 Cache cleared by admin. Pattern: {Pattern}", pattern ?? "ALL");

            return Json(new
            {
                success = true,
                message = $"Cache cleared successfully" + (pattern != null ? $" (pattern: {pattern})" : "")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to clear cache");
            return Json(new { success = false, message = "Failed to clear cache" });
        }
    }

    /// <summary>
    /// 🚀 MEMORY USAGE API
    /// </summary>
    [HttpGet]
    public IActionResult MemoryUsage()
    {
        var process = Process.GetCurrentProcess();
        var gcMemory = GC.GetTotalMemory(false);

        return Json(new
        {
            ProcessMemoryMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
            GCMemoryMB = Math.Round(gcMemory / 1024.0 / 1024.0, 2),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount
        });
    }

    /// <summary>
    /// 🚀 FORCE GARBAGE COLLECTION
    /// </summary>
    [HttpPost]
    public IActionResult ForceGC()
    {
        var before = GC.GetTotalMemory(false);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var after = GC.GetTotalMemory(false);

        return Json(new
        {
            success = true,
            FreedMemoryMB = Math.Round((before - after) / 1024.0 / 1024.0, 2),
            BeforeMB = Math.Round(before / 1024.0 / 1024.0, 2),
            AfterMB = Math.Round(after / 1024.0 / 1024.0, 2)
        });
    }

    /// <summary>
    /// 🚀 SYSTEM HEALTH CHECK
    /// </summary>
    [HttpGet]
    public IActionResult HealthCheck()
    {
        var process = Process.GetCurrentProcess();
        var uptime = DateTime.Now - process.StartTime;

        var health = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            UptimeHours = Math.Round(uptime.TotalHours, 2),
            MemoryUsageMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
            ThreadCount = process.Threads.Count,
            Version = "2.0-Optimized",
            Environment = Environment.MachineName,
            DotNetVersion = Environment.Version.ToString()
        };

        return Json(health);
    }

    /// <summary>
    /// 🚀 PERFORMANCE OPTIMIZATION SUGGESTIONS
    /// </summary>
    [HttpGet]
    public IActionResult OptimizationSuggestions()
    {
        var suggestions = new List<object>();
        var cacheStats = _cache.GetStatistics();
        var process = Process.GetCurrentProcess();
        var memoryMB = process.WorkingSet64 / 1024.0 / 1024.0;

        // Cache optimization suggestions
        if (cacheStats.HitRatio < 80)
        {
            suggestions.Add(new
            {
                Type = "Cache",
                Priority = "High",
                Message = $"Cache hit ratio is {cacheStats.HitRatio:F1}%. Consider caching more frequently accessed data.",
                Action = "Increase cache expiration times for stable data"
            });
        }

        // Memory optimization suggestions
        if (memoryMB > 500)
        {
            suggestions.Add(new
            {
                Type = "Memory",
                Priority = "Medium",
                Message = $"Memory usage is {memoryMB:F1}MB. Consider optimization.",
                Action = "Run garbage collection or reduce cache size"
            });
        }

        // General suggestions
        if (cacheStats.TotalKeys < 50)
        {
            suggestions.Add(new
            {
                Type = "Performance",
                Priority = "Low",
                Message = "Low cache utilization detected.",
                Action = "Implement caching for frequently accessed database queries"
            });
        }

        return Json(suggestions);
    }

    /// <summary>
    /// 🚀 COMPREHENSIVE SYSTEM METRICS
    /// </summary>
    private object GetSystemMetrics()
    {
        var process = Process.GetCurrentProcess();
        var cacheStats = _cache.GetStatistics();
        var uptime = DateTime.Now - process.StartTime;

        return new
        {
            // 🚀 SYSTEM INFO
            System = new
            {
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                OSVersion = Environment.OSVersion.ToString(),
                DotNetVersion = Environment.Version.ToString(),
                UptimeHours = Math.Round(uptime.TotalHours, 2)
            },

            // 🚀 MEMORY METRICS
            Memory = new
            {
                WorkingSetMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                PrivateMemoryMB = Math.Round(process.PrivateMemorySize64 / 1024.0 / 1024.0, 2),
                GCMemoryMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2),
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2)
            },

            // 🚀 PERFORMANCE METRICS
            Performance = new
            {
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount,
                TotalProcessorTime = process.TotalProcessorTime.TotalMilliseconds,
                UserProcessorTime = process.UserProcessorTime.TotalMilliseconds
            },

            // 🚀 CACHE METRICS
            Cache = new
            {
                cacheStats.Hits,
                cacheStats.Misses,
                cacheStats.Total,
                HitRatio = Math.Round(cacheStats.HitRatio, 2),
                cacheStats.TotalKeys,
                UptimeMinutes = Math.Round((DateTime.UtcNow - cacheStats.StartTime).TotalMinutes, 2)
            },

            // 🚀 TIMESTAMPS
            LastUpdate = DateTime.UtcNow,
            ServerTime = DateTime.Now,
            Timezone = TimeZoneInfo.Local.DisplayName
        };
    }
}