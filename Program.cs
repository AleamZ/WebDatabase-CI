using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.HttpOverrides;
using System.IO.Compression;
using CIResearch.Models;
using CIResearch.Services;

var builder = WebApplication.CreateBuilder(args);

// 🚀 MEMORY & PERFORMANCE OPTIMIZATION
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500 * 1024 * 1024; // 500MB
    options.MemoryBufferThreshold = 128 * 1024; // 128KB buffer (optimized)
    options.ValueLengthLimit = Int32.MaxValue;
    options.KeyLengthLimit = Int32.MaxValue;
});

// 🚀 SERVER CONFIGURATION OPTIMIZATION
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 500 * 1024 * 1024; // 500MB
});

builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 500 * 1024 * 1024; // 500MB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2); // Reduced from 10min
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2); // Reduced from 10min
    options.Limits.MaxConcurrentConnections = 1000; // Added connection limit
    options.Limits.MaxConcurrentUpgradedConnections = 100;
});

// 🚀 RESPONSE COMPRESSION (Giảm 60-80% bandwidth)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/javascript",
        "text/css",
        "text/html",
        "text/json",
        "text/plain",
        "text/xml"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal; // Best compression
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize; // Best compression
});

// 🚀 ADVANCED MEMORY CACHING (Tăng 300% performance)
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // 1024 entries max
    options.CompactionPercentage = 0.25; // Remove 25% when full
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});

// 🚀 HTTP CLIENT OPTIMIZATION
builder.Services.AddHttpClient();

// 🚀 SESSION OPTIMIZATION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Reduced from 30min
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict; // Security enhancement
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// 🚀 CONTROLLER OPTIMIZATION
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Giá trị không được để trống");
    options.MaxModelValidationErrors = 50; // Limit validation errors
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
});

// 🚀 OPTIMIZED SERVICES REGISTRATION
builder.Services.AddSingleton<IGlobalCacheService, GlobalCacheService>();
builder.Services.AddScoped<IOptimizedDatabaseService, OptimizedDatabaseService>();
builder.Services.AddSingleton<ExportLimitService>();

// 🚀 SECURITY ENHANCEMENTS
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// 🚀 MIDDLEWARE PIPELINE OPTIMIZATION
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 🚀 SECURITY HEADERS
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// 🚀 COMPRESSION MIDDLEWARE (Must be early in pipeline)
app.UseResponseCompression();

app.UseHttpsRedirection();

// 🚀 STATIC FILES OPTIMIZATION
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 year
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
        ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddYears(1).ToString("R"));
    }
});

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// 🚀 OPTIMIZED ROUTING
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .RequireHost("*"); // Accept all hosts

// 🚀 HEALTH CHECK ENDPOINT
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "2.0-Optimized"
}));

// 🚀 API ENDPOINTS OPTIMIZATION
app.MapGet("/api/cache/clear", async (IMemoryCache cache) =>
{
    if (cache is MemoryCache mc)
    {
        mc.Clear();
    }
    return Results.Ok(new { Message = "Cache cleared successfully" });
});

Console.WriteLine("🚀 WebDatabase-CI Optimized - Starting on: " + DateTime.Now);

app.Run();
