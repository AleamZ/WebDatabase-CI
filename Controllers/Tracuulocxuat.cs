using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace CIResearch.Controllers
{

    public class Tracuulocxuat : Controller
    {

        private readonly string _connectionString = "Server=localhost;Database=sakila;User=root;Password=1234";

        public IActionResult Index(string searchUser = "")
        {

            var userRole = HttpContext.Session.GetString("Role");

            //// Kiểm tra xem Role có phải là "admin" hay không
            //if (userRole != "Admin")
            //{
            //    // Nếu không phải admin, chuyển hướng người dùng đến trang khác (ví dụ: trang lỗi hoặc login)
            //    return RedirectToAction("Index", "Home"); // Thay "Home" bằng controller phù hợp nếu cần
            //}
            var userActions = new List<UserAction>();
            var notifications = new List<string>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT Id, Username, Action, Timestamp FROM useraction_loc_xuat WHERE Username LIKE @SearchUser ORDER BY Timestamp DESC";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchUser", "%" + searchUser + "%");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var action = new UserAction
                            {
                                Id = reader.GetInt32("Id"),
                                Username = reader.GetString("Username"),
                                Action = reader.GetString("Action"),
                                Timestamp = reader.GetDateTime("Timestamp")
                            };
                            userActions.Add(action);


                        }
                    }
                }
            }


            ViewBag.SearchUser = searchUser; // Lưu giá trị tìm kiếm vào ViewBag
            return View(userActions);
        }
    }

    // Lớp model cho dữ liệu của bảng useraction_loc_xuat
    public class UserAction
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }

        public string Status { get; set; }
    }
}