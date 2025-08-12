using Microsoft.AspNetCore.Mvc;
using CIResearch.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace CIResearch.Controllers
{

    public class ManageUsersController : Controller
    {
        private readonly string _connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;";

        // Action xem danh sách nhân viên
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("Role");

            // Kiểm tra xem Role có phải là "admin" hay không
            if (userRole != "Admin")
            {
                // Nếu không phải admin, chuyển hướng người dùng đến trang khác (ví dụ: trang lỗi hoặc login)
                return RedirectToAction("Index", "Home"); // Thay "Home" bằng controller phù hợp nếu cần
            }
            List<User> users = new List<User>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT id, username, password ,role, full_name, email, phone, address, date_of_birth, Phongban FROM users", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new User
                        {
                            Id = reader.GetInt32("id"),
                            Password = reader.GetString("password"),
                            Username = reader.GetString("username"),
                            Role = reader.GetString("role"),
                            FullName = reader.GetString("full_name"),
                            Email = reader.GetString("email"),
                            Phone = reader.GetString("phone"),
                            Address = reader.GetString("address"),
                            DateOfBirth = reader.GetDateTime("date_of_birth"),
                            Phongban = reader.GetString("Phongban")
                        };
                        users.Add(user);
                    }
                }
            }

            return View(users); // Trả về view hiển thị danh sách nhân viên
        }
        [HttpPost]
        public JsonResult DeleteUser(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new MySqlCommand("DELETE FROM users WHERE id = @id", connection);
                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();
                }

                return Json(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }

        // API để cập nhật thông tin người dùng
        [HttpPost]
        public IActionResult UpdateUser(User updatedUser)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new MySqlCommand("UPDATE users SET full_name = @fullName, email = @Email, phone = @phone, address = @address, date_of_birth = @dateOfBirth, role = @role, Phongban = @phongban WHERE id = @id", connection);
                    command.Parameters.AddWithValue("@fullName", updatedUser.FullName);
                    command.Parameters.AddWithValue("@Email", updatedUser.Email);
                    command.Parameters.AddWithValue("@phone", updatedUser.Phone);
                    command.Parameters.AddWithValue("@address", updatedUser.Address);
                    command.Parameters.AddWithValue("@dateOfBirth", updatedUser.DateOfBirth);
                    command.Parameters.AddWithValue("@role", updatedUser.Role);
                    command.Parameters.AddWithValue("@phongban", updatedUser.Phongban);
                    command.Parameters.AddWithValue("@id", updatedUser.Id);

                    command.ExecuteNonQuery();
                }

                TempData["Message"] = "Cập nhật thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi khi cập nhật: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }

}
