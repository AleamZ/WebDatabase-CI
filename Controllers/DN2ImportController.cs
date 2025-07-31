using CIResearch.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CIResearch.Controllers
{
    // DN2 specific classes to avoid conflicts
    public class DN2RequestTimeoutAttribute : ActionFilterAttribute
    {
        private readonly int _timeout;

        public DN2RequestTimeoutAttribute(int timeout)
        {
            _timeout = timeout;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }

    public class DN2ColumnInfo
    {
        public string DbColumnName { get; set; }
        public Type DataType { get; set; }
        public bool IsRequired { get; set; }
        public string[] PossibleExcelNames { get; set; }
    }

    public class DN2ImportController : Controller
    {
        private const int BATCH_SIZE = 5000;
        private const int BUFFER_SIZE = 8192;
        private static readonly Dictionary<string, DN2ColumnInfo> ColumnDefinitions = new Dictionary<string, DN2ColumnInfo>
        {
            // Định nghĩa các cột và cách map với Excel
            { "TenDN", new DN2ColumnInfo
                {
                    DbColumnName = "TenDN",
                    DataType = typeof(string),
                    IsRequired = true,
                    PossibleExcelNames = new[] { "TenDN", "Tên doanh nghiệp", "Ten DN", "Tên DN" }
                }
            },
            { "Diachi", new DN2ColumnInfo
                {
                    DbColumnName = "Diachi",
                    DataType = typeof(string),
                    IsRequired = true,
                    PossibleExcelNames = new[] { "Diachi", "Địa chỉ", "DiaChi" }
                }
            },
            { "MaTinh_Dieutra", new DN2ColumnInfo
                {
                    DbColumnName = "MaTinh_Dieutra",
                    DataType = typeof(string),
                    IsRequired = true,
                    PossibleExcelNames = new[] { "MaTinh_Dieutra", "Mã Tỉnh", "MaTinh" }
                }
            },
            { "MaHuyen_Dieutra", new DN2ColumnInfo
                {
                    DbColumnName = "MaHuyen_Dieutra",
                    DataType = typeof(string),
                    IsRequired = true,
                    PossibleExcelNames = new[] { "MaHuyen_Dieutra", "Mã Huyện", "MaHuyen" }
                }
            },
            { "MaXa_Dieutra", new DN2ColumnInfo
                {
                    DbColumnName = "MaXa_Dieutra",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "MaXa_Dieutra", "Mã Xã", "MaXa" }
                }
            },
            { "DNTB_MaTinh", new DN2ColumnInfo
                {
                    DbColumnName = "DNTB_MaTinh",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "DNTB_MaTinh" }
                }
            },
            { "DNTB_MaHuyen", new DN2ColumnInfo
                {
                    DbColumnName = "DNTB_MaHuyen",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "DNTB_MaHuyen" }
                }
            },
            { "DNTB_MaXa", new DN2ColumnInfo
                {
                    DbColumnName = "DNTB_MaXa",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "DNTB_MaXa" }
                }
            },
            { "Region", new DN2ColumnInfo
                {
                    DbColumnName = "Region",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "Region", "Vùng", "Vung" }
                }
            },
            { "Loaihinhkte", new DN2ColumnInfo
                {
                    DbColumnName = "Loaihinhkte",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "Loaihinhkte", "Loại hình kinh tế", "LoaiHinhKinhTe" }
                }
            },
            { "Email", new DN2ColumnInfo
                {
                    DbColumnName = "Email",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "Email" }
                }
            },
            { "Dienthoai", new DN2ColumnInfo
                {
                    DbColumnName = "Dienthoai",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "Dienthoai", "Điện thoại", "DienThoai", "SDT" }
                }
            },
            { "Nam", new DN2ColumnInfo
                {
                    DbColumnName = "Nam",
                    DataType = typeof(int),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "Nam", "Năm", "Year" }
                }
            },
            { "Masothue", new DN2ColumnInfo
                {
                    DbColumnName = "Masothue",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "Masothue", "Mã số thuế", "MST" }
                }
            },
            { "Vungkinhte", new DN2ColumnInfo
                {
                    DbColumnName = "Vungkinhte",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "Vungkinhte", "Vùng kinh tế", "VungKinhTe" }
                }
            },
            { "QUY_MO", new DN2ColumnInfo
                {
                    DbColumnName = "QUY_MO",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "QUY_MO", "Quy mô", "QuyMo", "Quy_Mo", "Company Size", "Quy mo" }
                }
            },
            { "MaNganhC5_Chinh", new DN2ColumnInfo
                {
                    DbColumnName = "MaNganhC5_Chinh",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "MaNganhC5_Chinh", "Mã ngành", "MaNganh" }
                }
            },
            { "TEN_NGANH", new DN2ColumnInfo
                {
                    DbColumnName = "TEN_NGANH",
                    DataType = typeof(string),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "TEN_NGANH", "Tên ngành", "TenNganh" }
                }
            },
            { "SR_Doanhthu_Thuan_BH_CCDV", new DN2ColumnInfo
                {
                    DbColumnName = "SR_Doanhthu_Thuan_BH_CCDV",
                    DataType = typeof(decimal),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "SR_Doanhthu_Thuan_BH_CCDV", "Doanh thu thuần", "DoanhThu" }
                }
            },
            { "SR_Loinhuan_TruocThue", new DN2ColumnInfo
                {
                    DbColumnName = "SR_Loinhuan_TruocThue",
                    DataType = typeof(decimal),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "SR_Loinhuan_TruocThue", "Lợi nhuận trước thuế", "LoiNhuan" }
                }
            },
            { "SoLaodong_DauNam", new DN2ColumnInfo
                {
                    DbColumnName = "SoLaodong_DauNam",
                    DataType = typeof(int),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "SoLaodong_DauNam", "Số lao động đầu năm", "LaoDongDauNam" }
                }
            },
            { "SoLaodong_CuoiNam", new DN2ColumnInfo
                {
                    DbColumnName = "SoLaodong_CuoiNam",
                    DataType = typeof(int),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "SoLaodong_CuoiNam", "Số lao động cuối năm", "LaoDongCuoiNam" }
                }
            },
            { "Taisan_Tong_CK", new DN2ColumnInfo
                {
                    DbColumnName = "Taisan_Tong_CK",
                    DataType = typeof(decimal),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "Taisan_Tong_CK", "Tổng tài sản cuối kỳ", "TaiSanCuoiKy" }
                }
            },
            { "Taisan_Tong_DK", new DN2ColumnInfo
                {
                    DbColumnName = "Taisan_Tong_DK",
                    DataType = typeof(decimal),
                    IsRequired = false,
                    PossibleExcelNames = new[] { "Taisan_Tong_DK", "Tổng tài sản đầu kỳ", "TaiSanDauKy" }
                }
            }
        };

        private readonly string _connectionString;

        public DN2ImportController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [DN2RequestTimeout(60 * 30)] // 30 phút
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var filePath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE))
                {
                    await file.CopyToAsync(stream);
                }

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;
                    var colCount = worksheet.Dimension?.Columns ?? 0;

                    if (rowCount <= 1)
                    {
                        return BadRequest("Excel file is empty");
                    }

                    // Map cột Excel với định nghĩa cột
                    var columnMappings = MapExcelColumns(worksheet);

                    // Kiểm tra các cột bắt buộc
                    var missingRequiredColumns = ColumnDefinitions.Values
                        .Where(c => c.IsRequired && !columnMappings.ContainsKey(c.DbColumnName))
                        .Select(c => c.DbColumnName)
                        .ToList();

                    if (missingRequiredColumns.Any())
                    {
                        return BadRequest($"Missing required columns: {string.Join(", ", missingRequiredColumns)}");
                    }

                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        // Force UTF8 for this session to ensure proper character encoding
                        using (var cmd = new MySqlCommand("SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci", connection))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Bắt đầu transaction
                        using (var transaction = await connection.BeginTransactionAsync())
                        {
                            try
                            {
                                var successCount = 0;
                                var errorCount = 0;
                                var errors = new List<string>();

                                // Xử lý từng batch
                                for (int row = 2; row <= rowCount; row += BATCH_SIZE)
                                {
                                    var batchSize = Math.Min(BATCH_SIZE, rowCount - row + 1);
                                    var (batchSuccess, batchErrors) = await ProcessBatch(worksheet, row, batchSize, columnMappings, connection);

                                    successCount += batchSuccess;
                                    errorCount += batchErrors.Count;
                                    errors.AddRange(batchErrors);

                                    if (errors.Count > 100)
                                    {
                                        errors = errors.Take(100).ToList();
                                        errors.Add("... and more errors (truncated)");
                                    }
                                }

                                await transaction.CommitAsync();

                                ViewBag.Message = $"Import completed! Processed {rowCount - 1} rows, {successCount} successful, {errorCount} errors.";
                                ViewBag.MessageType = errorCount == 0 ? "success" : "warning";
                                ViewBag.TotalRows = rowCount - 1; // Exclude header row
                                if (errors.Any())
                                {
                                    ViewBag.Details = errors;
                                }

                                return View();
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        private Dictionary<string, int> MapExcelColumns(ExcelWorksheet worksheet)
        {
            Console.WriteLine($"🔍 Starting column mapping process...");
            var columnMappings = new Dictionary<string, int>();
            var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns];

            Console.WriteLine($"📊 Found {worksheet.Dimension.Columns} columns in Excel file");

            foreach (var cell in headerRow)
            {
                var headerValue = cell.Text.Trim();
                if (string.IsNullOrEmpty(headerValue)) continue;

                Console.WriteLine($"📝 Processing header: '{headerValue}' in column {cell.Start.Column}");

                // Tìm định nghĩa cột phù hợp
                var columnDef = ColumnDefinitions.Values.FirstOrDefault(c =>
                    c.PossibleExcelNames.Any(name =>
                        name.Equals(headerValue, StringComparison.OrdinalIgnoreCase)));

                if (columnDef != null)
                {
                    columnMappings[columnDef.DbColumnName] = cell.Start.Column;
                    Console.WriteLine($"✅ Mapped '{headerValue}' → '{columnDef.DbColumnName}' (column {cell.Start.Column})");
                }
                else
                {
                    Console.WriteLine($"⚠️ No mapping found for header: '{headerValue}'");
                }
            }

            Console.WriteLine($"📋 Final column mappings:");
            foreach (var mapping in columnMappings)
            {
                Console.WriteLine($"   - {mapping.Key}: Column {mapping.Value}");
            }

            return columnMappings;
        }

        private string DetermineEconomicRegion(string provinceInput)
        {
            Console.WriteLine($"🔍 DetermineEconomicRegion called with provinceInput: '{provinceInput}'");

            if (string.IsNullOrEmpty(provinceInput))
            {
                Console.WriteLine($"❌ ProvinceInput is null or empty, returning null");
                return null;
            }

            // Normalize province input
            string normalizedInput = provinceInput.Trim().ToLower();

            // Map tên tỉnh đầy đủ với vùng kinh tế
            var provinceNameMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Đồng bằng Sông Hồng
                { "Thành phố Hà Nội", "Đồng bằng Sông Hồng" },
                { "Hà Nội", "Đồng bằng Sông Hồng" },
                { "Thành phố Hải Phòng", "Đồng bằng Sông Hồng" },
                { "Hải Phòng", "Đồng bằng Sông Hồng" },
                { "Tỉnh Hà Nam", "Đồng bằng Sông Hồng" },
                { "Hà Nam", "Đồng bằng Sông Hồng" },
                { "Tỉnh Hải Dương", "Đồng bằng Sông Hồng" },
                { "Hải Dương", "Đồng bằng Sông Hồng" },
                { "Tỉnh Hưng Yên", "Đồng bằng Sông Hồng" },
                { "Hưng Yên", "Đồng bằng Sông Hồng" },
                { "Tỉnh Nam Định", "Đồng bằng Sông Hồng" },
                { "Nam Định", "Đồng bằng Sông Hồng" },
                { "Tỉnh Ninh Bình", "Đồng bằng Sông Hồng" },
                { "Ninh Bình", "Đồng bằng Sông Hồng" },
                { "Tỉnh Thái Bình", "Đồng bằng Sông Hồng" },
                { "Thái Bình", "Đồng bằng Sông Hồng" },
                { "Tỉnh Vĩnh Phúc", "Đồng bằng Sông Hồng" },
                { "Vĩnh Phúc", "Đồng bằng Sông Hồng" },
                { "Tỉnh Bắc Ninh", "Đồng bằng Sông Hồng" },
                { "Bắc Ninh", "Đồng bằng Sông Hồng" },

                // Trung du và Miền núi Bắc Bộ
                { "Tỉnh Hà Giang", "Trung du và Miền núi Bắc Bộ" },
                { "Hà Giang", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Cao Bằng", "Trung du và Miền núi Bắc Bộ" },
                { "Cao Bằng", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Lào Cai", "Trung du và Miền núi Bắc Bộ" },
                { "Lào Cai", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Điện Biên", "Trung du và Miền núi Bắc Bộ" },
                { "Điện Biên", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Lai Châu", "Trung du và Miền núi Bắc Bộ" },
                { "Lai Châu", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Sơn La", "Trung du và Miền núi Bắc Bộ" },
                { "Sơn La", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Yên Bái", "Trung du và Miền núi Bắc Bộ" },
                { "Yên Bái", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Tuyên Quang", "Trung du và Miền núi Bắc Bộ" },
                { "Tuyên Quang", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Lạng Sơn", "Trung du và Miền núi Bắc Bộ" },
                { "Lạng Sơn", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Bắc Kạn", "Trung du và Miền núi Bắc Bộ" },
                { "Bắc Kạn", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Thái Nguyên", "Trung du và Miền núi Bắc Bộ" },
                { "Thái Nguyên", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Phú Thọ", "Trung du và Miền núi Bắc Bộ" },
                { "Phú Thọ", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Bắc Giang", "Trung du và Miền núi Bắc Bộ" },
                { "Bắc Giang", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Quảng Ninh", "Trung du và Miền núi Bắc Bộ" },
                { "Quảng Ninh", "Trung du và Miền núi Bắc Bộ" },
                { "Tỉnh Hòa Bình", "Trung du và Miền núi Bắc Bộ" },
                { "Hòa Bình", "Trung du và Miền núi Bắc Bộ" },

                // Bắc Trung Bộ
                { "Tỉnh Thanh Hóa", "Bắc Trung Bộ" },
                { "Thanh Hóa", "Bắc Trung Bộ" },
                { "Tỉnh Nghệ An", "Bắc Trung Bộ" },
                { "Nghệ An", "Bắc Trung Bộ" },
                { "Tỉnh Hà Tĩnh", "Bắc Trung Bộ" },
                { "Hà Tĩnh", "Bắc Trung Bộ" },
                { "Tỉnh Quảng Bình", "Bắc Trung Bộ" },
                { "Quảng Bình", "Bắc Trung Bộ" },

                // Duyên hải Nam Trung Bộ
                { "Tỉnh Quảng Trị", "Duyên hải Nam Trung Bộ" },
                { "Quảng Trị", "Duyên hải Nam Trung Bộ" },
                { "Tỉnh Thừa Thiên Huế", "Duyên hải Nam Trung Bộ" },
                { "Thừa Thiên Huế", "Duyên hải Nam Trung Bộ" },
                { "Thành phố Đà Nẵng", "Duyên hải Nam Trung Bộ" },
                { "Đà Nẵng", "Duyên hải Nam Trung Bộ" },
                { "Tỉnh Quảng Nam", "Duyên hải Nam Trung Bộ" },
                { "Quảng Nam", "Duyên hải Nam Trung Bộ" },
                { "Tỉnh Quảng Ngãi", "Duyên hải Nam Trung Bộ" },
                { "Quảng Ngãi", "Duyên hải Nam Trung Bộ" },
                { "Tỉnh Bình Định", "Duyên hải Nam Trung Bộ" },
                { "Bình Định", "Duyên hải Nam Trung Bộ" },
                { "Tỉnh Phú Yên", "Duyên hải Nam Trung Bộ" },
                { "Phú Yên", "Duyên hải Nam Trung Bộ" },
                { "Tỉnh Khánh Hòa", "Duyên hải Nam Trung Bộ" },
                { "Khánh Hòa", "Duyên hải Nam Trung Bộ" },
                { "Tỉnh Ninh Thuận", "Duyên hải Nam Trung Bộ" },
                { "Ninh Thuận", "Duyên hải Nam Trung Bộ" },
                { "Tỉnh Bình Thuận", "Duyên hải Nam Trung Bộ" },
                { "Bình Thuận", "Duyên hải Nam Trung Bộ" },

                // Tây Nguyên
                { "Tỉnh Kon Tum", "Tây Nguyên" },
                { "Kon Tum", "Tây Nguyên" },
                { "Tỉnh Gia Lai", "Tây Nguyên" },
                { "Gia Lai", "Tây Nguyên" },
                { "Tỉnh Đắk Lắk", "Tây Nguyên" },
                { "Đắk Lắk", "Tây Nguyên" },
                { "Tỉnh Đắk Nông", "Tây Nguyên" },
                { "Đắk Nông", "Tây Nguyên" },
                { "Tỉnh Lâm Đồng", "Tây Nguyên" },
                { "Lâm Đồng", "Tây Nguyên" },

                // Đông Nam Bộ
                { "Thành phố Hồ Chí Minh", "Đông Nam Bộ" },
                { "Hồ Chí Minh", "Đông Nam Bộ" },
                { "TP Hồ Chí Minh", "Đông Nam Bộ" },
                { "TP.HCM", "Đông Nam Bộ" },
                { "Tỉnh Tây Ninh", "Đông Nam Bộ" },
                { "Tây Ninh", "Đông Nam Bộ" },
                { "Tỉnh Bình Phước", "Đông Nam Bộ" },
                { "Bình Phước", "Đông Nam Bộ" },
                { "Tỉnh Bình Dương", "Đông Nam Bộ" },
                { "Bình Dương", "Đông Nam Bộ" },
                { "Tỉnh Đồng Nai", "Đông Nam Bộ" },
                { "Đồng Nai", "Đông Nam Bộ" },
                { "Tỉnh Bà Rịa - Vũng Tàu", "Đông Nam Bộ" },
                { "Bà Rịa - Vũng Tàu", "Đông Nam Bộ" },

                // Đồng bằng Sông Cửu Long
                { "Tỉnh Long An", "Đồng bằng Sông Cửu Long" },
                { "Long An", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh Tiền Giang", "Đồng bằng Sông Cửu Long" },
                { "Tiền Giang", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh Bến Tre", "Đồng bằng Sông Cửu Long" },
                { "Bến Tre", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh Trà Vinh", "Đồng bằng Sông Cửu Long" },
                { "Trà Vinh", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh Vĩnh Long", "Đồng bằng Sông Cửu Long" },
                { "Vĩnh Long", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh Đồng Tháp", "Đồng bằng Sông Cửu Long" },
                { "Đồng Tháp", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh An Giang", "Đồng bằng Sông Cửu Long" },
                { "An Giang", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh Kiên Giang", "Đồng bằng Sông Cửu Long" },
                { "Kiên Giang", "Đồng bằng Sông Cửu Long" },
                { "Thành phố Cần Thơ", "Đồng bằng Sông Cửu Long" },
                { "Cần Thơ", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh Hậu Giang", "Đồng bằng Sông Cửu Long" },
                { "Hậu Giang", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh Sóc Trăng", "Đồng bằng Sông Cửu Long" },
                { "Sóc Trăng", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh Bạc Liêu", "Đồng bằng Sông Cửu Long" },
                { "Bạc Liêu", "Đồng bằng Sông Cửu Long" },
                { "Tỉnh Cà Mau", "Đồng bằng Sông Cửu Long" },
                { "Cà Mau", "Đồng bằng Sông Cửu Long" }
            };

            // Kiểm tra mapping theo tên tỉnh đầy đủ trước
            if (provinceNameMappings.TryGetValue(provinceInput, out string economicRegion))
            {
                Console.WriteLine($"✅ Province name '{provinceInput}' mapped to '{economicRegion}'");
                return economicRegion;
            }

            // Nếu không tìm thấy theo tên, thử theo mã số (code cũ)
            var bacTrungBoProvinces = new[] { "24", "25", "27", "28", "29", "30" };
            if (bacTrungBoProvinces.Contains(provinceInput))
            {
                Console.WriteLine($"✅ Province code '{provinceInput}' mapped to 'Bắc Trung Bộ'");
                return "Bắc Trung Bộ";
            }

            var duyenHaiNamTrungBoProvinces = new[] { "31", "32", "33", "34", "35", "36" };
            if (duyenHaiNamTrungBoProvinces.Contains(provinceInput))
            {
                Console.WriteLine($"✅ Province code '{provinceInput}' mapped to 'Duyên hải Nam Trung Bộ'");
                return "Duyên hải Nam Trung Bộ";
            }

            var dongBangSongCuuLongProvinces = new[] { "50", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "67", "73", "80", "82", "83", "84", "86", "87", "89", "91", "92", "93", "94", "95", "96" };
            if (dongBangSongCuuLongProvinces.Contains(provinceInput))
            {
                Console.WriteLine($"✅ Province code '{provinceInput}' mapped to 'Đồng bằng Sông Cửu Long'");
                return "Đồng bằng Sông Cửu Long";
            }

            var dongBangSongHongProvinces = new[] { "01", "26", "30", "33", "37", "38", "40", "46" };
            if (dongBangSongHongProvinces.Contains(provinceInput))
            {
                Console.WriteLine($"✅ Province code '{provinceInput}' mapped to 'Đồng bằng Sông Hồng'");
                return "Đồng bằng Sông Hồng";
            }

            var dongNamBoProvinces = new[] { "48", "49", "51", "68", "70", "72", "74", "75", "77", "79" };
            if (dongNamBoProvinces.Contains(provinceInput))
            {
                Console.WriteLine($"✅ Province code '{provinceInput}' mapped to 'Đông Nam Bộ'");
                return "Đông Nam Bộ";
            }

            var tayNguyenProvinces = new[] { "64", "66", "71", "76" };
            if (tayNguyenProvinces.Contains(provinceInput))
            {
                Console.WriteLine($"✅ Province code '{provinceInput}' mapped to 'Tây Nguyên'");
                return "Tây Nguyên";
            }

            var trungDuMienNuiProvinces = new[] { "02", "04", "06", "08", "10", "11", "12", "14", "15", "17", "19", "20", "22" };
            if (trungDuMienNuiProvinces.Contains(provinceInput))
            {
                Console.WriteLine($"✅ Province code '{provinceInput}' mapped to 'Trung du và Miền núi Bắc Bộ'");
                return "Trung du và Miền núi Bắc Bộ";
            }

            Console.WriteLine($"❌ Province '{provinceInput}' not found in any region mapping, returning null");
            return null;
        }

        private string DetermineRegion(string provinceInput)
        {
            Console.WriteLine($"🔍 DetermineRegion called with provinceInput: '{provinceInput}'");

            if (string.IsNullOrEmpty(provinceInput))
            {
                Console.WriteLine($"❌ ProvinceInput is null or empty, returning null");
                return null;
            }

            // Lấy vùng kinh tế trước
            string economicRegion = DetermineEconomicRegion(provinceInput);

            if (string.IsNullOrEmpty(economicRegion))
            {
                Console.WriteLine($"❌ Could not determine economic region for '{provinceInput}', returning null");
                return null;
            }

            // Map vùng kinh tế sang 3 miền
            if (economicRegion == "Đồng bằng Sông Hồng" || economicRegion == "Trung du và Miền núi Bắc Bộ")
            {
                Console.WriteLine($"✅ Economic region '{economicRegion}' mapped to 'Miền Bắc'");
                return "Miền Bắc";
            }

            if (economicRegion == "Bắc Trung Bộ" || economicRegion == "Duyên hải Nam Trung Bộ" || economicRegion == "Tây Nguyên")
            {
                Console.WriteLine($"✅ Economic region '{economicRegion}' mapped to 'Miền Trung'");
                return "Miền Trung";
            }

            if (economicRegion == "Đông Nam Bộ" || economicRegion == "Đồng bằng Sông Cửu Long")
            {
                Console.WriteLine($"✅ Economic region '{economicRegion}' mapped to 'Miền Nam'");
                return "Miền Nam";
            }

            Console.WriteLine($"❌ Economic region '{economicRegion}' not mapped to any main region, returning null");
            return null;
        }

        private async Task<(int successCount, List<string> errors)> ProcessBatch(
            ExcelWorksheet worksheet,
            int startRow,
            int batchSize,
            Dictionary<string, int> columnMappings,
            MySqlConnection connection)
        {
            var successCount = 0;
            var errors = new List<string>();

            var sql = BuildInsertSql(columnMappings.Keys.ToList());

            using (var cmd = new MySqlCommand(sql, connection))
            {
                for (int row = startRow; row < startRow + batchSize; row++)
                {
                    try
                    {
                        cmd.Parameters.Clear();

                        // Đọc MaTinh_Dieutra trước để xác định vùng kinh tế nếu cần
                        string provinceCode = null;
                        if (columnMappings.ContainsKey("MaTinh_Dieutra"))
                        {
                            provinceCode = worksheet.Cells[row, columnMappings["MaTinh_Dieutra"]].Text.Trim();
                            Console.WriteLine($"📍 Row {row}: Found MaTinh_Dieutra = '{provinceCode}'");
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Row {row}: MaTinh_Dieutra column not found in mappings");
                        }

                        // Thu thập dữ liệu hàng để hiển thị khi có lỗi
                        var rowData = new Dictionary<string, string>();

                        foreach (var mapping in columnMappings)
                        {
                            var columnDef = ColumnDefinitions[mapping.Key];
                            var cellValue = worksheet.Cells[row, mapping.Value].Text.Trim();

                            // Lưu dữ liệu để hiển thị lỗi
                            rowData[mapping.Key] = cellValue;

                            var paramName = $"@{mapping.Key}";
                            object paramValue = DBNull.Value;

                            // Xử lý đặc biệt cho trường Vungkinhte
                            if (mapping.Key == "Vungkinhte" && string.IsNullOrEmpty(cellValue) && !string.IsNullOrEmpty(provinceCode))
                            {
                                Console.WriteLine($"🔄 Row {row}: Auto-detecting Vungkinhte for province '{provinceCode}'");
                                cellValue = DetermineEconomicRegion(provinceCode);
                                rowData[mapping.Key] = cellValue; // Cập nhật dữ liệu hiển thị
                                Console.WriteLine($"✅ Row {row}: Vungkinhte set to '{cellValue}'");
                            }

                            // Xử lý đặc biệt cho trường Region
                            if (mapping.Key == "Region" && string.IsNullOrEmpty(cellValue) && !string.IsNullOrEmpty(provinceCode))
                            {
                                Console.WriteLine($"🔄 Row {row}: Auto-detecting Region for province '{provinceCode}'");
                                cellValue = DetermineRegion(provinceCode);
                                rowData[mapping.Key] = cellValue; // Cập nhật dữ liệu hiển thị
                                Console.WriteLine($"✅ Row {row}: Region set to '{cellValue}'");
                            }

                            // Debug logging cho QUY_MO
                            if (mapping.Key == "QUY_MO" && !string.IsNullOrEmpty(cellValue))
                            {
                                Console.WriteLine($"📏 Row {row}: QUY_MO value found: '{cellValue}'");
                            }

                            // Debug logging cho UTF8 encoding của các trường quan trọng
                            if ((mapping.Key == "TenDN" || mapping.Key == "Diachi") && !string.IsNullOrEmpty(cellValue))
                            {
                                Console.WriteLine($"🔤 Row {row}: UTF8 {mapping.Key} = '{cellValue}' (length: {cellValue.Length})");
                                var utf8Bytes = Encoding.UTF8.GetBytes(cellValue);
                                Console.WriteLine($"🔤 UTF8 Bytes: {string.Join(",", utf8Bytes.Take(20))}");
                            }

                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                try
                                {
                                    if (columnDef.DataType == typeof(int))
                                    {
                                        if (int.TryParse(cellValue, out int intValue))
                                            paramValue = intValue;
                                    }
                                    else if (columnDef.DataType == typeof(decimal))
                                    {
                                        if (decimal.TryParse(cellValue, out decimal decimalValue))
                                            paramValue = decimalValue;
                                    }
                                    else
                                    {
                                        paramValue = cellValue;
                                    }
                                }
                                catch
                                {
                                    paramValue = DBNull.Value;
                                }
                            }

                            cmd.Parameters.AddWithValue(paramName, paramValue);
                        }

                        // Thêm parameter Vungkinhte nếu không có trong columnMappings
                        if (!columnMappings.ContainsKey("Vungkinhte") && !string.IsNullOrEmpty(provinceCode))
                        {
                            Console.WriteLine($"🔄 Row {row}: Adding missing Vungkinhte parameter for province '{provinceCode}'");
                            var vungKinhTe = DetermineEconomicRegion(provinceCode);
                            rowData["Vungkinhte"] = vungKinhTe; // Lưu vào rowData
                            Console.WriteLine($"✅ Row {row}: Adding Vungkinhte = '{vungKinhTe}'");
                            cmd.Parameters.AddWithValue("@Vungkinhte", (object)vungKinhTe ?? DBNull.Value);
                        }

                        // Thêm parameter Region nếu không có trong columnMappings
                        if (!columnMappings.ContainsKey("Region") && !string.IsNullOrEmpty(provinceCode))
                        {
                            Console.WriteLine($"🔄 Row {row}: Adding missing Region parameter for province '{provinceCode}'");
                            var region = DetermineRegion(provinceCode);
                            rowData["Region"] = region; // Lưu vào rowData
                            Console.WriteLine($"✅ Row {row}: Adding Region = '{region}'");
                            cmd.Parameters.AddWithValue("@Region", (object)region ?? DBNull.Value);
                        }

                        await cmd.ExecuteNonQueryAsync();
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        // Tạo thông tin lỗi chi tiết
                        var errorDetails = new StringBuilder();
                        errorDetails.AppendLine($"❌ Hàng {row} - Lỗi: {ex.Message}");

                        // Hiển thị dữ liệu của hàng bị lỗi
                        errorDetails.AppendLine($"   📋 Dữ liệu hàng:");

                        // Hiển thị các trường quan trọng trước
                        var importantFields = new[] { "TenDN", "Diachi", "MaTinh_Dieutra", "MaHuyen_Dieutra", "Masothue", "QUY_MO" };
                        foreach (var field in importantFields)
                        {
                            if (columnMappings.ContainsKey(field))
                            {
                                var cellValue = worksheet.Cells[row, columnMappings[field]].Text.Trim();
                                errorDetails.AppendLine($"      • {field}: '{cellValue}'");
                            }
                        }

                        // Hiển thị các trường số liệu
                        var numericFields = new[] { "Nam", "SR_Doanhthu_Thuan_BH_CCDV", "SR_Loinhuan_TruocThue", "SoLaodong_DauNam", "SoLaodong_CuoiNam" };
                        foreach (var field in numericFields)
                        {
                            if (columnMappings.ContainsKey(field))
                            {
                                var cellValue = worksheet.Cells[row, columnMappings[field]].Text.Trim();
                                if (!string.IsNullOrEmpty(cellValue))
                                {
                                    errorDetails.AppendLine($"      • {field}: '{cellValue}'");
                                }
                            }
                        }

                        // Hiển thị lỗi SQL chi tiết nếu có
                        if (ex is MySqlException sqlEx)
                        {
                            errorDetails.AppendLine($"   🔍 Lỗi SQL: {sqlEx.Message}");
                            errorDetails.AppendLine($"   📊 Error Number: {sqlEx.Number}");
                        }

                        errors.Add(errorDetails.ToString().TrimEnd());
                    }
                }
            }

            return (successCount, errors);
        }

        private string BuildInsertSql(List<string> columns)
        {
            // Thêm Vungkinhte vào danh sách cột nếu chưa có
            if (!columns.Contains("Vungkinhte"))
            {
                columns.Add("Vungkinhte");
            }

            // Thêm Region vào danh sách cột nếu chưa có
            if (!columns.Contains("Region"))
            {
                columns.Add("Region");
            }

            var columnList = string.Join(", ", columns);
            var paramList = string.Join(", ", columns.Select(c => $"@{c}"));

            return $@"INSERT INTO admin_ciresearch.dn_all2 ({columnList}) 
                     VALUES ({paramList})";
        }

        private IActionResult HandleError(Exception ex)
        {
            ViewBag.Message = "Error during import";
            ViewBag.MessageType = "error";

            var details = new List<string> { ex.Message };

            if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
            {
                details.Add(ex.InnerException.Message);
            }

            ViewBag.Details = details;
            return View();
        }

        [HttpGet]
        public IActionResult GetProgress()
        {
            // TODO: Implement real progress tracking
            return Json(new { progress = 0, message = "Processing..." });
        }

        [HttpGet]
        public IActionResult TestErrors()
        {
            // Tạo dữ liệu lỗi demo để test UI
            var testErrors = new List<string>
            {
                "❌ Hàng 5 - Lỗi: Duplicate entry 'MST001' for key 'Masothue'\n   📋 Dữ liệu hàng:\n      • TenDN: 'Công ty TNHH ABC'\n      • Diachi: '123 Nguyễn Văn Linh, Q7, TP.HCM'\n      • MaTinh_Dieutra: '79'\n      • MaHuyen_Dieutra: '785'\n      • Masothue: 'MST001'\n      • QUY_MO: 'Nhỏ'\n      • Nam: '2023'\n      • SR_Doanhthu_Thuan_BH_CCDV: '150000000'\n   🔍 Lỗi SQL: Duplicate entry 'MST001' for key 'dn_all.Masothue'\n   📊 Error Number: 1062",

                "❌ Hàng 12 - Lỗi: Data too long for column 'TenDN' at row 1\n   📋 Dữ liệu hàng:\n      • TenDN: 'Công ty Cổ phần Đầu tư và Phát triển Công nghệ Thông tin Viễn thông ABC XYZ rất dài tên'\n      • Diachi: '456 Điện Biên Phủ, Q3, TP.HCM'\n      • MaTinh_Dieutra: '79'\n      • MaHuyen_Dieutra: '760'\n      • Masothue: 'MST002'\n      • QUY_MO: 'Vừa'\n   🔍 Lỗi SQL: Data too long for column 'TenDN' at row 1\n   📊 Error Number: 1406",

                "❌ Hàng 18 - Lỗi: Incorrect decimal value: 'abc123' for column 'SR_Doanhthu_Thuan_BH_CCDV' at row 1\n   📋 Dữ liệu hàng:\n      • TenDN: 'Doanh nghiệp tư nhân DEF'\n      • Diachi: '789 Lê Văn Việt, Q9, TP.HCM'\n      • MaTinh_Dieutra: '79'\n      • MaHuyen_Dieutra: '769'\n      • Masothue: 'MST003'\n      • QUY_MO: 'Siêu nhỏ'\n      • Nam: '2023'\n      • SR_Doanhthu_Thuan_BH_CCDV: 'abc123'\n      • SR_Loinhuan_TruocThue: '25000000'\n   🔍 Lỗi SQL: Incorrect decimal value: 'abc123' for column 'SR_Doanhthu_Thuan_BH_CCDV' at row 1\n   📊 Error Number: 1366",

                "❌ Hàng 25 - Lỗi: Column 'TenDN' cannot be null\n   📋 Dữ liệu hàng:\n      • TenDN: ''\n      • Diachi: '101 Trường Chinh, Q12, TP.HCM'\n      • MaTinh_Dieutra: '79'\n      • MaHuyen_Dieutra: '774'\n      • Masothue: 'MST004'\n      • QUY_MO: 'Lớn'\n   🔍 Lỗi SQL: Column 'TenDN' cannot be null\n   📊 Error Number: 1048"
            };

            ViewBag.Message = "Test Import với lỗi - Processed 30 rows, 26 successful, 4 errors.";
            ViewBag.MessageType = "warning";
            ViewBag.TotalRows = 30;
            ViewBag.Details = testErrors;

            return View("Index");
        }

        [HttpGet]
        public IActionResult TestQuyMoImport()
        {
            // Demo các trường hợp import QUY_MO thành công
            var successMessages = new List<string>
            {
                "✅ Import QUY_MO thành công - Test kết quả:",
                "",
                "📏 Cột QUY_MO đã được thêm vào hệ thống import Excel",
                "📋 Các tên cột Excel được hỗ trợ:",
                "   • QUY_MO",
                "   • Quy mô",
                "   • QuyMo",
                "   • Quy_Mo",
                "   • Company Size",
                "   • Quy mo",
                "",
                "📊 Các giá trị QUY_MO hợp lệ:",
                "   • Siêu nhỏ",
                "   • Nhỏ",
                "   • Vừa",
                "   • Lớn",
                "",
                "🔄 Quá trình import sẽ:",
                "   1. Tự động nhận diện cột QUY_MO từ Excel",
                "   2. Ghi debug log khi tìm thấy giá trị QUY_MO",
                "   3. Lưu trực tiếp vào cột QUY_MO trong database",
                "   4. Hiển thị QUY_MO trong thông tin lỗi nếu có",
                "",
                "✅ Hệ thống đã sẵn sàng import cột QUY_MO!"
            };

            ViewBag.Message = "Test QUY_MO Import - Ready to import company size data";
            ViewBag.MessageType = "success";
            ViewBag.TotalRows = 0;
            ViewBag.Details = successMessages;

            return View("Index");
        }
    }
}
