using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace CIResearch.Controllers
{
    public class BaseController : Controller
    {
        protected readonly string _connectionString;
        protected readonly ILogger<BaseController> _logger;

        public BaseController(IConfiguration configuration, ILogger<BaseController>? logger = null)
        {
            _connectionString = "Server=localhost;Database=sakila;User=root;Password=123456;CharSet=utf8mb4;SslMode=none;";
            _logger = logger ?? new NullLogger<BaseController>();
        }

        /// <summary>
        /// Hash password using SHA256
        /// </summary>
        protected string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "CIResearch_Salt_2024"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        protected bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        /// <summary>
        /// Test database connection
        /// </summary>
        protected async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                using var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Database connection failed: {ConnectionString}", _connectionString.Replace(_connectionString.Split("Password=")[1].Split(";")[0], "***"));
                return false;
            }
        }

        /// <summary>
        /// Get safe connection string for logging (password masked)
        /// </summary>
        protected string GetSafeConnectionString()
        {
            try
            {
                var parts = _connectionString.Split(';');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                    {
                        parts[i] = "Password=***";
                    }
                }
                return string.Join(";", parts);
            }
            catch
            {
                return "ConnectionString parse error";
            }
        }
    }

    // Null logger for backwards compatibility
    public class NullLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
