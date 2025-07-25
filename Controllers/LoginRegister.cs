// Controllers/LoginRegister.cs
using CIResearch.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace CIResearch.Controllers
{
    public class LoginRegister : Controller
    {
        private readonly string _connectionString = "Server=localhost;Database=sakila;User=root;Password=123456;";

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password, string returnUrl = null)
        {
            User user = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT * FROM users WHERE username = @username AND password = @password", connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password); // So sánh mật khẩu trực tiếp

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            Id = reader.GetInt32("id"),
                            Username = reader.GetString("username"),
                            Role = reader.GetString("role"),
                            FullName = reader.GetString("full_name")
                        };
                    }
                }
            }

            if (user != null)
            {
                // Lưu tên người dùng vào session
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role); // Lưu vai trò vào session


                // Log để debug
                Console.WriteLine($"🔍 Login: User {user.Username} (Role: {user.Role}) logged in");
                Console.WriteLine($"🔍 Login: returnUrl = '{returnUrl}'");
                Console.WriteLine($"🔍 Login: IsLocalUrl = {Url.IsLocalUrl(returnUrl)}");

                // Nếu có returnUrl, chuyển hướng về đó, nếu không thì chuyển hướng dựa trên vai trò
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    Console.WriteLine($"🔍 Login: Redirecting to returnUrl: {returnUrl}");
                    return Redirect(returnUrl);
                }
                else
                {
                    Console.WriteLine($"🔍 Login: No valid returnUrl, redirecting based on role: {user.Role}");
                    // Chuyển hướng dựa trên vai trò
                    if (user.Role == "Manager" || user.Role == "Execute" || user.Role == "Assistant")
                    {
                        return RedirectToAction("Index", "Bacsi");
                    }
                    else if (user.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View("Login");
            }

            return View("Login");
        }



        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Kiểm tra xem username đã tồn tại chưa
                    var checkUserCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", connection);
                    checkUserCmd.Parameters.AddWithValue("@username", newUser.Username);
                    var userExists = Convert.ToInt32(checkUserCmd.ExecuteScalar()) > 0;

                    if (userExists)
                    {
                        ViewBag.ErrorMessage = "Username already exists.";
                        return View(newUser);
                    }

                    // Kiểm tra xem email đã tồn tại chưa
                    var checkEmailCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @Email", connection);
                    checkEmailCmd.Parameters.AddWithValue("@Email", newUser.Email);
                    var emailExists = Convert.ToInt32(checkEmailCmd.ExecuteScalar()) > 0;

                    if (emailExists)
                    {
                        ViewBag.ErrorMessage = "Email already exists.";
                        return View(newUser);
                    }

                    // Thêm người dùng mới vào cơ sở dữ liệu
                    var insertCmd = new MySqlCommand("INSERT INTO users (username, password, role, email, full_name, phone, address, date_of_birth, Phongban) VALUES (@username, @password, @role, @Email, @fullName, @phone, @address, @dateOfBirth, @phongban)", connection);
                    insertCmd.Parameters.AddWithValue("@username", newUser.Username);
                    insertCmd.Parameters.AddWithValue("@password", newUser.Password); // Lưu mật khẩu trực tiếp
                    insertCmd.Parameters.AddWithValue("@role", newUser.Role); // Gán vai trò người dùng
                    insertCmd.Parameters.AddWithValue("@Email", newUser.Email);
                    insertCmd.Parameters.AddWithValue("@fullName", newUser.FullName);
                    insertCmd.Parameters.AddWithValue("@phone", newUser.Phone);
                    insertCmd.Parameters.AddWithValue("@address", newUser.Address);
                    insertCmd.Parameters.AddWithValue("@dateOfBirth", newUser.DateOfBirth);
                    insertCmd.Parameters.AddWithValue("@phongban", newUser.Phongban);

                    insertCmd.ExecuteNonQuery();
                }

                return RedirectToAction("Login");
            }

            return View(newUser);
        }




        // GET: Change Password
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ViewBag.ErrorMessage = "Tất cả các trường là bắt buộc.";
                return View();
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Kiểm tra thông tin người dùng
                var cmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username AND password = @oldPassword", connection);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@oldPassword", oldPassword); // Lưu ý: Mã hóa mật khẩu trong thực tế

                var userExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

                if (!userExists)
                {
                    ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu cũ không đúng.";
                    return View();
                }

                // Cập nhật mật khẩu mới
                var updateCmd = new MySqlCommand("UPDATE users SET password = @newPassword WHERE username = @username", connection);
                updateCmd.Parameters.AddWithValue("@newPassword", newPassword); // Lưu mật khẩu mới
                updateCmd.Parameters.AddWithValue("@username", username);

                updateCmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang chính sau khi đổi mật khẩu thành công
        }





        // Thêm một action logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            return RedirectToAction("Login");
        }
    }
}