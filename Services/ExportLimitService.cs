using System.Collections.Concurrent;

namespace CIResearch.Services
{
    /// <summary>
    /// Service quản lý giới hạn export theo role
    /// </summary>
    public class ExportLimitService
    {
        private readonly ConcurrentDictionary<string, ExportLimit> _roleLimits;
        private readonly ConcurrentDictionary<string, UserExportStats> _userExportStats;

        public ExportLimitService()
        {
            _roleLimits = new ConcurrentDictionary<string, ExportLimit>();
            _userExportStats = new ConcurrentDictionary<string, UserExportStats>();

            // Khởi tạo giới hạn cho từng role
            InitializeRoleLimits();
        }

        private void InitializeRoleLimits()
        {
            // Admin: Không giới hạn
            _roleLimits["Admin"] = new ExportLimit
            {
                MaxRecordsPerExport = int.MaxValue,
                MaxExportsPerDay = int.MaxValue,
                IsAllowed = true
            };

            // Manager: 5000 records/ngày
            _roleLimits["Manager"] = new ExportLimit
            {
                MaxRecordsPerExport = 5000,
                MaxExportsPerDay = int.MaxValue,
                IsAllowed = true
            };

            // Execute: 1000 records/lần/ngày
            _roleLimits["Execute"] = new ExportLimit
            {
                MaxRecordsPerExport = 1000,
                MaxExportsPerDay = int.MaxValue,
                IsAllowed = true
            };

            // Assistant: Không được export
            _roleLimits["Assistant"] = new ExportLimit
            {
                MaxRecordsPerExport = 0,
                MaxExportsPerDay = 0,
                IsAllowed = false
            };

            // Các role khác: Không được export
            _roleLimits["default"] = new ExportLimit
            {
                MaxRecordsPerExport = 0,
                MaxExportsPerDay = 0,
                IsAllowed = false
            };
        }

        /// <summary>
        /// Kiểm tra xem user có thể export không
        /// </summary>
        public ExportValidationResult ValidateExport(string username, string role, int recordCount)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
            {
                return new ExportValidationResult
                {
                    IsAllowed = false,
                    Message = "Thông tin người dùng không hợp lệ"
                };
            }

            // Lấy giới hạn cho role
            if (!_roleLimits.TryGetValue(role, out var limit))
            {
                limit = _roleLimits["default"];
            }

            if (!limit.IsAllowed)
            {
                return new ExportValidationResult
                {
                    IsAllowed = false,
                    Message = $"Role '{role}' không có quyền export dữ liệu. Vui lòng liên hệ Admin để được cấp quyền."
                };
            }

            // Kiểm tra số lượng records
            if (recordCount > limit.MaxRecordsPerExport)
            {
                return new ExportValidationResult
                {
                    IsAllowed = false,
                    Message = $"Số lượng records yêu cầu ({recordCount:N0}) vượt quá giới hạn cho role '{role}' ({limit.MaxRecordsPerExport:N0} records/lần export). Vui lòng giảm số lượng records hoặc liên hệ Admin để tăng giới hạn."
                };
            }

            // Kiểm tra giới hạn hàng ngày
            var today = DateTime.Today;
            var userKey = $"{username}_{today:yyyyMMdd}";

            if (!_userExportStats.TryGetValue(userKey, out var stats))
            {
                stats = new UserExportStats
                {
                    Username = username,
                    Date = today,
                    TotalRecordsExported = 0,
                    ExportCount = 0
                };
                _userExportStats[userKey] = stats;
            }

            // Kiểm tra tổng records đã export hôm nay
            if (stats.TotalRecordsExported + recordCount > limit.MaxRecordsPerExport)
            {
                var remainingRecords = limit.MaxRecordsPerExport - stats.TotalRecordsExported;
                var message = remainingRecords > 0
                    ? $"Tổng records export hôm nay ({stats.TotalRecordsExported:N0} + {recordCount:N0}) vượt quá giới hạn cho role '{role}' ({limit.MaxRecordsPerExport:N0} records/ngày). Bạn còn {remainingRecords:N0} records có thể export."
                    : $"Bạn đã đạt giới hạn export hôm nay ({stats.TotalRecordsExported:N0}/{limit.MaxRecordsPerExport:N0} records). Vui lòng thử lại vào ngày mai.";

                return new ExportValidationResult
                {
                    IsAllowed = false,
                    Message = message
                };
            }

            return new ExportValidationResult
            {
                IsAllowed = true,
                Message = "Export được phép",
                CurrentStats = stats,
                Limit = limit
            };
        }

        /// <summary>
        /// Ghi nhận export thành công
        /// </summary>
        public void RecordExport(string username, int recordCount)
        {
            var today = DateTime.Today;
            var userKey = $"{username}_{today:yyyyMMdd}";

            _userExportStats.AddOrUpdate(userKey,
                new UserExportStats
                {
                    Username = username,
                    Date = today,
                    TotalRecordsExported = recordCount,
                    ExportCount = 1
                },
                (key, existing) =>
                {
                    existing.TotalRecordsExported += recordCount;
                    existing.ExportCount++;
                    return existing;
                });
        }

        /// <summary>
        /// Lấy thống kê export của user
        /// </summary>
        public UserExportStats GetUserStats(string username)
        {
            var today = DateTime.Today;
            var userKey = $"{username}_{today:yyyyMMdd}";

            return _userExportStats.TryGetValue(userKey, out var stats) ? stats : new UserExportStats
            {
                Username = username,
                Date = today,
                TotalRecordsExported = 0,
                ExportCount = 0
            };
        }

        /// <summary>
        /// Lấy giới hạn cho role
        /// </summary>
        public ExportLimit GetRoleLimit(string role)
        {
            return _roleLimits.TryGetValue(role, out var limit) ? limit : _roleLimits["default"];
        }
    }

    public class ExportLimit
    {
        public int MaxRecordsPerExport { get; set; }
        public int MaxExportsPerDay { get; set; }
        public bool IsAllowed { get; set; }
    }

    public class UserExportStats
    {
        public string Username { get; set; } = "";
        public DateTime Date { get; set; }
        public int TotalRecordsExported { get; set; }
        public int ExportCount { get; set; }
    }

    public class ExportValidationResult
    {
        public bool IsAllowed { get; set; }
        public string Message { get; set; } = "";
        public UserExportStats? CurrentStats { get; set; }
        public ExportLimit? Limit { get; set; }
    }
}