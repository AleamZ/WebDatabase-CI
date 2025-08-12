using CIResearch.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace CIResearch.Controllers
{
    public class ProfileController : Controller
    {
        private readonly string _connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;";

        public IActionResult ViewProfile()
        {
            // Lấy tên người dùng từ session
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "LoginRegister"); // Chuyển hướng đến trang đăng nhập nếu không có người dùng
            }

            User user = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new MySqlCommand("SELECT * FROM users WHERE username = @username", connection);
                cmd.Parameters.AddWithValue("@username", username);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            Id = reader.GetInt32("id"),
                            Username = reader.GetString("username"),
                            Email = reader.GetString("email"),
                            FullName = reader.GetString("full_name"),
                            Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone"),
                            Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString("address"),
                            DateOfBirth = reader.IsDBNull(reader.GetOrdinal("date_of_birth")) ? (DateTime?)null : reader.GetDateTime("date_of_birth"),
                            Phongban = reader.IsDBNull(reader.GetOrdinal("Phongban")) ? null : reader.GetString("Phongban"),
                            Role = reader.IsDBNull(reader.GetOrdinal("role")) ? null : reader.GetString("role")
                        };
                    }
                }
            }

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}
