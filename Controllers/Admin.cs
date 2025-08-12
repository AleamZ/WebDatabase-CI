using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using CIResearch.Models;
using System.Text;
using System.Drawing;
using System.Linq; // Added for .All()
using System.IO; // Added for MemoryStream
using ClosedXML.Excel; // Added for Excel export
using CIResearch.Services; // Added for ExportRequestRepository
using System.Threading.Tasks; // Added for async/await

namespace CIResearch.Controllers
{
    public class Admin : Controller
    {
        private string _connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;DefaultCommandTimeout=1000;ConnectionTimeout=1000;";

        public ActionResult Index(string stt = "", List<string> code = null, List<string> projectName = null, List<string> year = null,
string contactObject = "", List<string> sbjnum = null, string fullname = "",
List<string> city = null, string address = "", string street = "", string ward = "",
string district = "", List<string> phoneNumber = null, string email = "",
string dateOfBirth = "", List<string> age = null, List<string> sex = null,
List<string> job = null, List<string> householdIncome = null, List<string> personalIncome = null,
List<string> maritalStatus = null, string mostFrequentlyUsedBrand = "",
string source = "", List<string> Classname = null, string education = "",
List<string> provinces = null, string qc = "", string qa = "", List<string> Khuvuc = null, List<String> Nganhhang = null, List<string> region = null)
        {
            try
            {
                // Cho phép truy cập như guest - không cần kiểm tra role
                var userRole = HttpContext.Session.GetString("Role");
                var username = HttpContext.Session.GetString("Username");

                // Kiểm tra session có hợp lệ không
                var isLoggedIn = !string.IsNullOrEmpty(userRole) && !string.IsNullOrEmpty(username);

                // Nếu session không hợp lệ, clear session để tránh lỗi
                if (!isLoggedIn && (!string.IsNullOrEmpty(userRole) || !string.IsNullOrEmpty(username)))
                {
                    HttpContext.Session.Clear();
                    isLoggedIn = false;
                    userRole = null;
                }

                // Lưu trạng thái đăng nhập vào ViewBag để view có thể hiển thị phù hợp
                ViewBag.IsLoggedIn = isLoggedIn;
                ViewBag.UserRole = userRole;

                ViewBag.Year = year;
                ViewBag.Projectname = projectName;
                ViewBag.City = city;
                ViewBag.Sex = sex;
                ViewBag.Age = age;
                ViewBag.Region = region;
                ViewBag.Job = job;
                ViewBag.Classname = Classname;
                ViewBag.MaritalStatus = maritalStatus;
                ViewBag.Code = code;
                ViewBag.Nganhhang = Nganhhang;


                if (sbjnum != null && sbjnum.All(string.IsNullOrWhiteSpace))
                {
                    sbjnum = null;
                }

                ViewBag.Sbjnum = sbjnum != null ? string.Join(",", sbjnum) : "";

                if (phoneNumber != null && phoneNumber.All(string.IsNullOrWhiteSpace))
                {
                    phoneNumber = null;
                }

                ViewBag.Phonenumber = phoneNumber != null ? string.Join(",", phoneNumber) : "";

                List<ALLDATA> adminChart = new List<ALLDATA>();

                adminChart = getadminChart(stt, code, projectName, year, contactObject, sbjnum, fullname, city, address, street, ward, district, phoneNumber, email, dateOfBirth, age, sex, job, householdIncome, personalIncome, maritalStatus, mostFrequentlyUsedBrand, source, Classname, education, provinces, qc, qa, Khuvuc, Nganhhang);

                //lọc theo 3 miền


                var provinceSampleCounts = adminChart
           .GroupBy(a => a.City)
           .Select(g => new
           {
               City = g.Key,
               SampleCount = g.Count()
           })
           .ToList();

                ViewBag.ProvinceData = JsonConvert.SerializeObject(provinceSampleCounts);


                var totalSamples = adminChart.Count(); // Tổng số mẫu
                var provinceSampleCountsSlide = adminChart
                    .GroupBy(a => a.City)
                    .Select(g => new
                    {
                        City = g.Key,
                        SampleCount = g.Count(),
                        Percentage = (double)g.Count() / totalSamples * 100 // Tính phần trăm
                    })
                    .ToList();

                ViewBag.ProvinceDataSlide = JsonConvert.SerializeObject(provinceSampleCounts);









                //phần chính
                //tổng số mẫu
                ViewBag.TotalRows = adminChart.Count.ToString("N0");
                //tổng số dự án
                ViewBag.TotalProjects = adminChart.Select(x => x.ProjectName).Distinct().Count();
                //tổng số mẫu 3 miền
                var northernProvinces = new HashSet<string>
{
    "BẮC GIANG", "BẮC KẠN", "BẮC NINH", "CAO BẰNG", "ĐIỆN BIÊN",
    "HÀ GIANG", "HÀ NAM", "HÀ NỘI", "HẢI DƯƠNG", "HẢI PHÒNG",
    "HÒA BÌNH", "HƯNG YÊN", "LAI CHÂU", "LÀO CAI", "NAM ĐỊNH",
    "NINH BÌNH", "PHÚ THỌ", "QUẢNG NINH", "SƠN LA", "THÁI BÌNH",
    "THÁI NGUYÊN", "TUYÊN QUANG", "VĨNH PHÚC", "YÊN BÁI"
};
                var centralProvinces = new HashSet<string>
{
    "BÌNH ĐỊNH",
    "ĐÀ NẴNG",
    "ĐẮK LẮK",
    "ĐẮK NÔNG",
    "GIA LAI",
    "HÀ TĨNH",
    "KHÁNH HÒA",
    "KON TUM",
    "NGHỆ AN",
    "PHÚ YÊN",
    "QUẢNG BÌNH",
    "QUẢNG NAM",
    "QUẢNG NGÃI",
    "QUẢNG TRỊ",
    "THỪA THIÊN HUẾ"
};

                var southernProvinces = new HashSet<string>
{
    "AN GIANG",
    "BÀ RỊA VŨNG TÀU",
    "BẠC LIÊU",
    "BẾN TRE",
    "BÌNH DƯƠNG",
    "BÌNH PHƯỚC",
    "BÌNH THUẬN",
    "CÀ MAU",
    "CẦN THƠ",
    "ĐỒNG NAI",
    "ĐỒNG THÁP",
    "HẬU GIANG",
    "HỒ CHÍ MINH",
    "KIÊN GIANG",
    "LÂM ĐỒNG",
    "LONG AN",
    "NINH THUẬN",
    "SÓC TRĂNG",
    "TÂY NINH",
    "TIỀN GIANG",
    "TRÀ VINH",
    "VĨNH LONG"
};


                ViewBag.NorthernSampleCount = adminChart.Count(x => northernProvinces.Contains(x.City));
                ViewBag.CentralSampleCount = adminChart.Count(x => centralProvinces.Contains(x.City));
                ViewBag.SouthernSampleCount = adminChart.Count(x => southernProvinces.Contains(x.City));

                int mienBacCount = adminChart.Count(x => x.City != null && northernProvinces.Contains(x.City.Trim().ToUpper()));
                int mienTrungCount = adminChart.Count(x => x.City != null && centralProvinces.Contains(x.City.Trim().ToUpper()));
                int mienNamCount = adminChart.Count(x => x.City != null && southernProvinces.Contains(x.City.Trim().ToUpper()));


                int totalCalculated = mienBacCount + mienTrungCount + mienNamCount;
                int totalRows = adminChart.Count(); // Tổng số dòng thực tế
                                                    // Kiểm tra số dư (nếu có)
                int soDu = totalRows - totalCalculated;

                if (soDu > 0)
                {
                    // Chia đều số dư cho 3 miền
                    int chiaDu = soDu / 3;
                    int duLe = soDu % 3; // Phần dư nếu không chia hết

                    // Cộng phần dư cho từng miền
                    mienBacCount += chiaDu + (duLe > 0 ? 1 : 0);
                    mienTrungCount += chiaDu + (duLe > 1 ? 1 : 0);
                    mienNamCount += chiaDu;
                }

                // Xuất kết quả ra ViewBag
                ViewBag.mienbac = mienBacCount.ToString("N0");
                ViewBag.mientrung = mienTrungCount.ToString("N0");
                ViewBag.miennam = mienNamCount.ToString("N0");
                ViewBag.TotalRows = totalRows.ToString("N0");





                //đếm tổng số mẫu nam nữ,


                ViewBag.namCount = adminChart.Count(x => x.Sex == "Nam").ToString("N0");
                ViewBag.nuCount = adminChart.Count(x => x.Sex == "Nữ").ToString("N0");
                ViewBag.KXDCount = adminChart.Count(x => string.IsNullOrEmpty(x.Sex) || (x.Sex != "Nam" && x.Sex != "Nữ")).ToString("N0");

                ViewBag.MaleCount = adminChart.Count(x => x.Sex == "Nam");
                ViewBag.FemaleCount = adminChart.Count(x => x.Sex == "Nữ");
                ViewBag.UndefinedCount = adminChart.Count(x => string.IsNullOrEmpty(x.Sex) || (x.Sex != "Nam" && x.Sex != "Nữ"));




                // Tổng số lượng mẫu  theo từng năm
                var yearlyData = adminChart.GroupBy(p => p.Year)
                                         .Select(g => new { Year = g.Key, Count = g.Count() })
                                         .OrderBy(g => g.Year)
                                         .ToList();

                ViewBag.YearLabels = yearlyData.Select(g => g.Year).ToArray();
                ViewBag.YearData = yearlyData.Select(g => g.Count).ToArray();
                //tổng số dự án 
                // Nhóm theo năm và tên dự án, sau đó đếm số lượng dự án riêng biệt trong mỗi năm
                var yearlyProject = adminChart
                    .GroupBy(p => p.Year) // Nhóm theo năm
                    .Select(g => new
                    {
                        Year = g.Key,
                        ProjectCount = g.Select(p => p.ProjectName).Distinct().Count() // Đếm số dự án riêng biệt trong năm
                    })
                    .OrderBy(g => g.Year) // Sắp xếp theo năm
                    .ToList();

                // Truyền dữ liệu vào ViewBag để dùng trong View
                ViewBag.YearLabelsProject = yearlyProject.Select(g => g.Year).ToArray(); // Năm
                ViewBag.YearDataProject = yearlyProject.Select(g => g.ProjectCount).ToArray(); // Số lượng dự án riêng biệt trong từng năm



                // Gom nhóm tình trạng hôn nhân
                var maritalStatusData = adminChart
                    .Where(p => p.MaritalStatus != "0")
                    .GroupBy(p =>
                    {
                        if (p.MaritalStatus.Contains("Độc thân")) return "Độc thân";
                        if (p.MaritalStatus.Contains("Đã kết hôn")) return "Đã kết hôn";
                        if (p.MaritalStatus.Contains("Ly hôn")) return "Đã ly hôn";

                        return "Khác";
                    })
                    .Select(g => new { MaritalStatus = g.Key, Count = g.Count() })
                    .ToList();

                // Đưa dữ liệu vào ViewBag
                ViewBag.MaritalStatusLabels = maritalStatusData.Select(g => g.MaritalStatus).ToArray();
                ViewBag.MaritalStatusData = maritalStatusData.Select(g => g.Count).ToArray();


                //biểu đồ cột nghề nghiệp 
                var jobdata = adminChart.Where(p => p.Job != "0")  // Giả sử "0" là giá trị không hợp lệ
         .GroupBy(p => p.Job)  // Nhóm theo nghề nghiệp
         .Select(g => new { Job = g.Key, Count = g.Count() })  // Tính số lượng cho mỗi nhóm
         .ToList();

                // Chuyển các giá trị nhóm thành mảng để sử dụng trong view
                ViewBag.JobLabels = jobdata.Select(g => g.Job).ToArray();
                ViewBag.JobData = jobdata.Select(g => g.Count).ToArray();



                //biểu đồ cột ngành hàng
                var nganhdata = adminChart.Where(p => p.Nganhhang != "0")  // Giả sử "0" là giá trị không hợp lệ
         .GroupBy(p => p.Nganhhang)  // Nhóm theo nghề nghiệp
         .Select(g => new { Nganhhang = g.Key, Count = g.Count() })  // Tính số lượng cho mỗi nhóm
         .ToList();

                // Chuyển các giá trị nhóm thành mảng để sử dụng trong view
                ViewBag.nganhLabels = nganhdata.Select(g => g.Nganhhang).ToArray();
                ViewBag.nganhData = nganhdata.Select(g => g.Count).ToArray();


                //biểu đồ cột ngành hàng
                var totalCountCl = adminChart.Where(p => p.Class != "0").Count();  // Tổng số mẫu hợp lệ

                var classdata = adminChart.Where(p => p.Class != "0")  // Giả sử "0" là giá trị không hợp lệ
                    .GroupBy(p => p.Class)  // Nhóm theo nghề nghiệp
                    .Select(g => new
                    {
                        Class = g.Key,
                        Count = g.Count(),
                        Percentage = (double)g.Count() / totalCountCl * 100  // Tính tỉ lệ phần trăm
                    })
                    .ToList();

                // Chuyển các giá trị nhóm thành mảng để sử dụng trong view
                ViewBag.classLabels = classdata.Select(g => g.Class).ToArray();
                ViewBag.classData = classdata.Select(g => g.Count).ToArray();
                ViewBag.classPercentages = classdata.Select(g => g.Percentage).ToArray();








                // Dữ liệu cho biểu đồ đường (done) 
                var ageData = adminChart.Where(p => p.Age != 0)
                    .GroupBy(p => p.Age > 100 ? 100 : (p.Age / 5) * 5)  // Nhóm thành các khoảng 10 tuổi, lớn hơn 100 là 99+
                    .Select(g => new
                    {
                        AgeRange = g.Key == 100 ? "99+" : $"{g.Key} đến {g.Key + 5}", // Tạo chuỗi khoảng tuổi
                        Count = g.Count()
                    })
                    .OrderBy(g => g.AgeRange) // Sắp xếp theo AgeRange
                    .ToList();

                ViewBag.ageLabels = ageData.Select(g => g.AgeRange).ToArray();
                ViewBag.ageData = ageData.Select(g => g.Count).ToArray();

                return View(adminChart);
            }
            catch (Exception ex)
            {
                // Log lỗi và clear session nếu có vấn đề
                Console.WriteLine($"Error in Admin Index: {ex.Message}");
                HttpContext.Session.Clear();

                // Set default values
                ViewBag.IsLoggedIn = false;
                ViewBag.UserRole = null;

                // Vẫn trả về view với dữ liệu rỗng
                return View(new List<ALLDATA>());
            }
        }






        private List<ALLDATA> getadminChart(
          string stt, List<string> code, List<string> projectName, List<string> year,
  string contactObject, List<string> sbjnum, string fullname,
  List<string> city, string address, string street, string ward,
  string district, List<string> phoneNumber, string email,
  string dateOfBirth, List<string> age, List<string> sex,
  List<string> job, List<string> householdIncome, List<string> personalIncome,
  List<string> maritalStatus, string mostFrequentlyUsedBrand,
  string source, List<string> className, string education,
  List<string> provinces, string qc, string qa, List<string> Khuvuc, List<string> Nganhhang)
        {
            List<ALLDATA> project = new List<ALLDATA>();



            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var queryBuilder = new StringBuilder("SELECT * FROM all_data_final WHERE 1=1");


                // Thêm di?u ki?n l?c cho t?ng tham s?


                if (projectName != null && projectName.Any())
                {
                    var projectNameParams = projectName.Select((_, i) => $"@projectName{i}").ToArray();
                    queryBuilder.Append(" AND PROJECTNAME IN (" + string.Join(", ", projectNameParams) + ")");
                }

                if (year != null && year.Any())
                {
                    var yearParams = year.Select((_, i) => $"@year{i}").ToArray();
                    queryBuilder.Append(" AND YEAR IN (" + string.Join(", ", yearParams) + ")");
                }

                if (city != null && city.Any())
                {
                    var cityParams = city.Select((_, i) => $"@city{i}").ToArray();
                    queryBuilder.Append(" AND CITY IN (" + string.Join(", ", cityParams) + ")");
                }

                if (age != null && age.Any())
                {
                    var ageParams = age.Select((_, i) => $"@age{i}").ToArray();
                    queryBuilder.Append(" AND AGE IN (" + string.Join(", ", ageParams) + ")");
                }
                if (sex != null && sex.Any())
                {
                    var sexParams = sex.Select((_, i) => $"@sex{i}").ToArray();
                    queryBuilder.Append(" AND SEX IN (" + string.Join(", ", sexParams) + ")");
                }
                if (provinces != null && provinces.Any())
                {
                    var provincesParams = provinces.Select((_, i) => $"@provinces{i}").ToArray();
                    queryBuilder.Append(" AND PROVINCES IN (" + string.Join(", ", provincesParams) + ")");
                }
                if (job != null && job.Any())
                {
                    var jobParams = job.Select((_, i) => $"@job{i}").ToArray();
                    queryBuilder.Append(" AND JOB IN (" + string.Join(", ", jobParams) + ")");
                }
                if (sbjnum != null && sbjnum.Count > 0)
                {
                    var sbjnumParams = sbjnum.Select((_, i) => $"@sbjnum{i}").ToArray();
                    queryBuilder.Append(" AND SBJNUM IN (" + string.Join(", ", sbjnumParams) + ")");
                }
                if (phoneNumber != null && phoneNumber.Count > 0)
                {
                    var phoneNumberParams = phoneNumber.Select((_, i) => $"@phoneNumber{i}").ToArray();
                    queryBuilder.Append(" AND PHONENUMBER IN (" + string.Join(", ", phoneNumberParams) + ")");
                }
                if (maritalStatus != null && maritalStatus.Any())
                {
                    var maritalStatusParams = maritalStatus.Select((_, i) => $"@maritalStatus{i}").ToArray();
                    queryBuilder.Append(" AND MARITALSTATUS IN (" + string.Join(", ", maritalStatusParams) + ")");
                }
                if (code != null && code.Any())
                {
                    var codeParams = code.Select((_, i) => $"@code{i}").ToArray();
                    queryBuilder.Append(" AND CODE IN (" + string.Join(", ", codeParams) + ")");
                }
                if (className != null && className.Any())
                {
                    var classParams = className.Select((_, i) => $"@className{i}").ToArray();
                    queryBuilder.Append(" AND Class IN (" + string.Join(", ", classParams) + ")");
                }
                if (Nganhhang != null && Nganhhang.Any())
                {
                    var NganhhangParams = Nganhhang.Select((_, i) => $"@Nganhhang{i}").ToArray();
                    queryBuilder.Append(" AND Nganhhang IN (" + string.Join(", ", NganhhangParams) + ")");
                }


                // Lấy tổng số dòng
                var countQuery = new StringBuilder("SELECT COUNT(*) FROM all_data_final WHERE 1=1");
                countQuery.Append(queryBuilder.ToString().Substring("SELECT * FROM all_data_final WHERE 1=1".Length));





                using (MySqlCommand command = new MySqlCommand(queryBuilder.ToString(), connection))
                {

                    // Thêm tham số vào MySqlCommand
                    if (projectName != null && projectName.Any())
                        for (int i = 0; i < projectName.Count; i++)
                            command.Parameters.AddWithValue($"@projectName{i}", projectName[i]);

                    if (year != null && year.Any())
                        for (int i = 0; i < year.Count; i++)
                            command.Parameters.AddWithValue($"@year{i}", year[i]);

                    if (city != null && city.Any())
                        for (int i = 0; i < city.Count; i++)
                            command.Parameters.AddWithValue($"@city{i}", city[i]);

                    if (age != null && age.Any())
                        for (int i = 0; i < age.Count; i++)
                            command.Parameters.AddWithValue($"@age{i}", age[i]);

                    if (sex != null && sex.Any())
                        for (int i = 0; i < sex.Count; i++)
                            command.Parameters.AddWithValue($"@sex{i}", sex[i]);

                    if (provinces != null && provinces.Any())
                        for (int i = 0; i < provinces.Count; i++)
                            command.Parameters.AddWithValue($"@provinces{i}", provinces[i]);
                    if (job != null && job.Any())
                        for (int i = 0; i < job.Count; i++)
                            command.Parameters.AddWithValue($"@job{i}", job[i]);
                    if (sbjnum != null && sbjnum.Any())
                        for (int i = 0; i < sbjnum.Count; i++)
                            command.Parameters.AddWithValue($"@sbjnum{i}", sbjnum[i]);
                    if (phoneNumber != null && phoneNumber.Any())
                        for (int i = 0; i < phoneNumber.Count; i++)
                            command.Parameters.AddWithValue($"@phoneNumber{i}", phoneNumber[i]);
                    if (maritalStatus != null && maritalStatus.Any())
                        for (int i = 0; i < maritalStatus.Count; i++)
                            command.Parameters.AddWithValue($"@maritalStatus{i}", maritalStatus[i]);
                    if (code != null && code.Any())
                        for (int i = 0; i < code.Count; i++)
                            command.Parameters.AddWithValue($"@code{i}", code[i]);
                    if (className != null && className.Any())
                        for (int i = 0; i < className.Count; i++)
                            command.Parameters.AddWithValue($"@className{i}", className[i]);
                    if (Nganhhang != null && Nganhhang.Any())
                        for (int i = 0; i < Nganhhang.Count; i++)
                            command.Parameters.AddWithValue($"@Nganhhang{i}", Nganhhang[i]);






                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ALLDATA Alldatas = new ALLDATA
                            {
                                Stt = reader.GetInt32("STT"),
                                Code = reader.GetString("CODE"),
                                ProjectName = reader.GetString("PROJECTNAME"),
                                Year = reader.GetInt32("YEAR"),
                                ContactObject = reader.GetString("CONTACTOBJECT"),
                                Sbjnum = reader.GetInt32("SBJNUM"),
                                Fullname = reader.GetString("FULLNAME"),
                                City = reader.GetString("CITY"),
                                Address = reader.GetString("ADDRESS"),
                                Street = reader.GetString("STREET"),
                                Ward = reader.GetString("WARD"),
                                District = reader.GetString("DISTRICT"),
                                PhoneNumber = reader.GetString("PHONENUMBER"),
                                Email = reader.GetString("EMAIL"),
                                DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DATEOFBIRTH")) ? (int?)null : reader.GetInt32("DATEOFBIRTH"),
                                Age = reader.GetInt32("AGE"),
                                Sex = reader.GetString("SEX"),
                                Job = reader.GetString("JOB"),
                                HouseholdIncome = reader.GetString("HOUSEHOLDINCOME"),
                                PersonalIncome = reader.GetString("PERSONALINCOME"),
                                MaritalStatus = reader.GetString("MARITALSTATUS"),
                                MostFrequentlyUsedBrand = reader.GetString("MOSTFREQUENTLYUSEDBRAND"),
                                Source = reader.GetString("SOURCE"),
                                Class = reader.GetString("Class"),
                                Education = reader.GetString("EDUCATION"),
                                Provinces = reader.GetString("PROVINCES"),
                                Qc = reader.GetString("QC"),
                                Qa = reader.GetString("QA"),
                                Nganhhang = reader.GetString("Nganhhang")
                            };
                            project.Add(Alldatas);
                        }
                    }
                }
            }


            return project;
        }
        private void StoreUserActionsInViewBag()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT Id, Username, Action, Timestamp FROM useraction_loc_xuat ORDER BY Timestamp DESC";

                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var userActions = new List<string>(); // Danh sách lưu thông tin
                        while (reader.Read())
                        {
                            var actionInfo = $"{reader.GetString("Username")} - {reader.GetString("Action")} at {reader.GetDateTime("Timestamp"):HH:mm:ss}";
                            userActions.Add(actionInfo);
                        }

                        // Lưu thông tin vào ViewBag
                        ViewBag.UserActions = userActions;
                    }
                }
            }
        }











        public class UserStatus
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Action { get; set; }
            public DateTime Timestamp { get; set; }

        }

        // Action để hiển thị trang tìm kiếm số điện thoại
        public ActionResult SearchPhoneNumber(string phoneNumber = "")
        {
            try
            {
                // Cho phép truy cập như guest - không cần kiểm tra role
                var userRole = HttpContext.Session.GetString("Role");
                var username = HttpContext.Session.GetString("Username");

                // Kiểm tra session có hợp lệ không
                var isLoggedIn = !string.IsNullOrEmpty(userRole) && !string.IsNullOrEmpty(username);

                // Nếu session không hợp lệ, clear session để tránh lỗi
                if (!isLoggedIn && (!string.IsNullOrEmpty(userRole) || !string.IsNullOrEmpty(username)))
                {
                    HttpContext.Session.Clear();
                    isLoggedIn = false;
                    userRole = null;
                }

                // Lưu trạng thái đăng nhập vào ViewBag để view có thể hiển thị phù hợp
                ViewBag.IsLoggedIn = isLoggedIn;
                ViewBag.UserRole = userRole;

                ViewBag.PhoneNumber = phoneNumber;
                List<ALLDATA> results = new List<ALLDATA>();

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    results = SearchPhoneNumberInDatabase(phoneNumber);
                }

                return View(results);
            }
            catch (Exception ex)
            {
                // Log lỗi và clear session nếu có vấn đề
                Console.WriteLine($"Error in SearchPhoneNumber: {ex.Message}");
                HttpContext.Session.Clear();

                // Set default values
                ViewBag.IsLoggedIn = false;
                ViewBag.UserRole = null;
                ViewBag.PhoneNumber = phoneNumber;

                // Vẫn trả về view với dữ liệu rỗng
                return View(new List<ALLDATA>());
            }
        }

        // Action để gửi request xuất Excel cho admin duyệt
        public async Task<ActionResult> RequestPhoneSearchExport(string phoneNumber)
        {
            // Yêu cầu đăng nhập để xuất dữ liệu
            var userRole = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(userRole))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thực hiện chức năng xuất dữ liệu.";
                return RedirectToAction("Login", "LoginRegister");
            }

            if (string.IsNullOrEmpty(phoneNumber))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập số điện thoại để tìm kiếm.";
                return RedirectToAction("SearchPhoneNumber");
            }

            try
            {
                var results = SearchPhoneNumberInDatabase(phoneNumber);

                if (results.Count == 0)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy kết quả phù hợp.";
                    return RedirectToAction("SearchPhoneNumber", new { phoneNumber = phoneNumber });
                }

                // Tạo file Excel
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Kết quả tìm kiếm");

                // Thêm header
                worksheet.Cell("A1").Value = "STT";
                worksheet.Cell("B1").Value = "Code";
                worksheet.Cell("C1").Value = "Tên dự án";
                worksheet.Cell("D1").Value = "Năm";
                worksheet.Cell("E1").Value = "Đối tượng liên hệ";
                worksheet.Cell("F1").Value = "Số thứ tự";
                worksheet.Cell("G1").Value = "Họ tên";
                worksheet.Cell("H1").Value = "Thành phố";
                worksheet.Cell("I1").Value = "Địa chỉ";
                worksheet.Cell("J1").Value = "Đường";
                worksheet.Cell("K1").Value = "Phường";
                worksheet.Cell("L1").Value = "Quận/Huyện";
                worksheet.Cell("M1").Value = "Số điện thoại";
                worksheet.Cell("N1").Value = "Email";
                worksheet.Cell("O1").Value = "Năm sinh";
                worksheet.Cell("P1").Value = "Tuổi";
                worksheet.Cell("Q1").Value = "Giới tính";
                worksheet.Cell("R1").Value = "Nghề nghiệp";
                worksheet.Cell("S1").Value = "Thu nhập hộ gia đình";
                worksheet.Cell("T1").Value = "Thu nhập cá nhân";
                worksheet.Cell("U1").Value = "Tình trạng hôn nhân";
                worksheet.Cell("V1").Value = "Thương hiệu sử dụng nhiều nhất";
                worksheet.Cell("W1").Value = "Nguồn";
                worksheet.Cell("X1").Value = "Lớp";
                worksheet.Cell("Y1").Value = "Học vấn";
                worksheet.Cell("Z1").Value = "Tỉnh";
                worksheet.Cell("AA1").Value = "QC";
                worksheet.Cell("AB1").Value = "QA";
                worksheet.Cell("AC1").Value = "Khu vực";
                worksheet.Cell("AD1").Value = "Ngành hàng";
                worksheet.Cell("AE1").Value = "Chuyên khoa";

                // Thêm dữ liệu
                for (int i = 0; i < results.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cell($"A{row}").Value = results[i].Stt;
                    worksheet.Cell($"B{row}").Value = results[i].Code;
                    worksheet.Cell($"C{row}").Value = results[i].ProjectName;
                    worksheet.Cell($"D{row}").Value = results[i].Year;
                    worksheet.Cell($"E{row}").Value = results[i].ContactObject;
                    worksheet.Cell($"F{row}").Value = results[i].Sbjnum;
                    worksheet.Cell($"G{row}").Value = results[i].Fullname;
                    worksheet.Cell($"H{row}").Value = results[i].City;
                    worksheet.Cell($"I{row}").Value = results[i].Address;
                    worksheet.Cell($"J{row}").Value = results[i].Street;
                    worksheet.Cell($"K{row}").Value = results[i].Ward;
                    worksheet.Cell($"L{row}").Value = results[i].District;
                    worksheet.Cell($"M{row}").Value = results[i].PhoneNumber;
                    worksheet.Cell($"N{row}").Value = results[i].Email;
                    worksheet.Cell($"O{row}").Value = results[i].DateOfBirth;
                    worksheet.Cell($"P{row}").Value = results[i].Age;
                    worksheet.Cell($"Q{row}").Value = results[i].Sex;
                    worksheet.Cell($"R{row}").Value = results[i].Job;
                    worksheet.Cell($"S{row}").Value = results[i].HouseholdIncome;
                    worksheet.Cell($"T{row}").Value = results[i].PersonalIncome;
                    worksheet.Cell($"U{row}").Value = results[i].MaritalStatus;
                    worksheet.Cell($"V{row}").Value = results[i].MostFrequentlyUsedBrand;
                    worksheet.Cell($"W{row}").Value = results[i].Source;
                    worksheet.Cell($"X{row}").Value = results[i].Class;
                    worksheet.Cell($"Y{row}").Value = results[i].Education;
                    worksheet.Cell($"Z{row}").Value = results[i].Provinces;
                    worksheet.Cell($"AA{row}").Value = results[i].Qc;
                    worksheet.Cell($"AB{row}").Value = results[i].Qa;
                    worksheet.Cell($"AC{row}").Value = results[i].Khuvuc;
                    worksheet.Cell($"AD{row}").Value = results[i].Nganhhang;
                    worksheet.Cell($"AE{row}").Value = results[i].ChuyenKhoa;
                }

                // Định dạng header
                var headerRange = worksheet.Range("A1:AE1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Tự động điều chỉnh cột
                worksheet.Columns().AdjustToContents();

                // Lưu file vào memory stream
                byte[] fileData;
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    fileData = stream.ToArray();
                }

                // Lấy thông tin user đang đăng nhập
                var username = HttpContext.Session.GetString("Username");
                var userEmail = GetUserEmail(username); // Lấy email từ database

                // Tạo export request
                var exportRequest = new ExportRequest
                {
                    Username = username,
                    Email = userEmail,
                    RequestTime = DateTime.Now,
                    Status = "pending",
                    FilterParams = $"PhoneNumber: {phoneNumber}, Results: {results.Count} records",
                    FileData = fileData,
                    Source = "PhoneSearch"
                };

                // Lưu request vào database
                var repo = new ExportRequestRepository(_connectionString);
                var requestId = await repo.AddRequestAsync(exportRequest);

                TempData["SuccessMessage"] = $"Đã gửi yêu cầu xuất Excel thành công! Request ID: {requestId}. Admin sẽ duyệt và gửi file qua email: {userEmail}";
                return RedirectToAction("SearchPhoneNumber", new { phoneNumber = phoneNumber });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi gửi yêu cầu xuất Excel: {ex.Message}";
                return RedirectToAction("SearchPhoneNumber", new { phoneNumber = phoneNumber });
            }
        }

        // Phương thức tìm kiếm số điện thoại trong database
        private List<ALLDATA> SearchPhoneNumberInDatabase(string phoneNumber)
        {
            List<ALLDATA> results = new List<ALLDATA>();

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"SELECT STT, CODE, PROJECTNAME, YEAR, CONTACTOBJECT, SBJNUM, FULLNAME, 
                                    CITY, ADDRESS, STREET, WARD, DISTRICT, PHONENUMBER, EMAIL, 
                                    DATEOFBIRTH, AGE, SEX, JOB, HOUSEHOLDINCOME, PERSONALINCOME, 
                                    MARITALSTATUS, MOSTFREQUENTLYUSEDBRAND, SOURCE, Class, EDUCATION, 
                                    PROVINCES, QC, QA, KHUVUC, NGANHHANG, CHUYENKHOA 
                             FROM all_data_final WHERE PHONENUMBER LIKE @phoneNumber";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@phoneNumber", $"%{phoneNumber}%");

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ALLDATA data = new ALLDATA
                            {
                                Stt = reader.IsDBNull(reader.GetOrdinal("STT")) ? (int?)null : reader.GetInt32("STT"),
                                Code = reader.IsDBNull(reader.GetOrdinal("CODE")) ? null : reader.GetString("CODE"),
                                ProjectName = reader.IsDBNull(reader.GetOrdinal("PROJECTNAME")) ? null : reader.GetString("PROJECTNAME"),
                                Year = reader.IsDBNull(reader.GetOrdinal("YEAR")) ? (int?)null : reader.GetInt32("YEAR"),
                                ContactObject = reader.IsDBNull(reader.GetOrdinal("CONTACTOBJECT")) ? null : reader.GetString("CONTACTOBJECT"),
                                Sbjnum = reader.IsDBNull(reader.GetOrdinal("SBJNUM")) ? 0 : reader.GetInt32("SBJNUM"),
                                Fullname = reader.IsDBNull(reader.GetOrdinal("FULLNAME")) ? null : reader.GetString("FULLNAME"),
                                City = reader.IsDBNull(reader.GetOrdinal("CITY")) ? null : reader.GetString("CITY"),
                                Address = reader.IsDBNull(reader.GetOrdinal("ADDRESS")) ? null : reader.GetString("ADDRESS"),
                                Street = reader.IsDBNull(reader.GetOrdinal("STREET")) ? null : reader.GetString("STREET"),
                                Ward = reader.IsDBNull(reader.GetOrdinal("WARD")) ? null : reader.GetString("WARD"),
                                District = reader.IsDBNull(reader.GetOrdinal("DISTRICT")) ? null : reader.GetString("DISTRICT"),
                                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PHONENUMBER")) ? null : reader.GetString("PHONENUMBER"),
                                Email = reader.IsDBNull(reader.GetOrdinal("EMAIL")) ? null : reader.GetString("EMAIL"),
                                DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DATEOFBIRTH")) ? (int?)null : reader.GetInt32("DATEOFBIRTH"),
                                Age = reader.IsDBNull(reader.GetOrdinal("AGE")) ? (int?)null : reader.GetInt32("AGE"),
                                Sex = reader.IsDBNull(reader.GetOrdinal("SEX")) ? null : reader.GetString("SEX"),
                                Job = reader.IsDBNull(reader.GetOrdinal("JOB")) ? null : reader.GetString("JOB"),
                                HouseholdIncome = reader.IsDBNull(reader.GetOrdinal("HOUSEHOLDINCOME")) ? null : reader.GetString("HOUSEHOLDINCOME"),
                                PersonalIncome = reader.IsDBNull(reader.GetOrdinal("PERSONALINCOME")) ? null : reader.GetString("PERSONALINCOME"),
                                MaritalStatus = reader.IsDBNull(reader.GetOrdinal("MARITALSTATUS")) ? null : reader.GetString("MARITALSTATUS"),
                                MostFrequentlyUsedBrand = reader.IsDBNull(reader.GetOrdinal("MOSTFREQUENTLYUSEDBRAND")) ? null : reader.GetString("MOSTFREQUENTLYUSEDBRAND"),
                                Source = reader.IsDBNull(reader.GetOrdinal("SOURCE")) ? null : reader.GetString("SOURCE"),
                                Class = reader.IsDBNull(reader.GetOrdinal("Class")) ? null : reader.GetString("Class"),
                                Education = reader.IsDBNull(reader.GetOrdinal("EDUCATION")) ? null : reader.GetString("EDUCATION"),
                                Provinces = reader.IsDBNull(reader.GetOrdinal("PROVINCES")) ? null : reader.GetString("PROVINCES"),
                                Qc = reader.IsDBNull(reader.GetOrdinal("QC")) ? null : reader.GetString("QC"),
                                Qa = reader.IsDBNull(reader.GetOrdinal("QA")) ? null : reader.GetString("QA"),
                                Khuvuc = reader.IsDBNull(reader.GetOrdinal("KHUVUC")) ? null : reader.GetString("KHUVUC"),
                                Nganhhang = reader.IsDBNull(reader.GetOrdinal("NGANHHANG")) ? null : reader.GetString("NGANHHANG"),
                                ChuyenKhoa = reader.IsDBNull(reader.GetOrdinal("CHUYENKHOA")) ? null : reader.GetString("CHUYENKHOA")
                            };
                            results.Add(data);
                        }
                    }
                }
            }

            return results;
        }

        // Phương thức lấy email của user từ database
        private string GetUserEmail(string username)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT Email FROM users WHERE Username = @username";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        var result = command.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error và trả về email mặc định
                return "";
            }
        }

        // Action để xuất dữ liệu từ trang admin chính
        public async Task<ActionResult> ExportData(string stt = "", List<string> code = null, List<string> projectName = null, List<string> year = null,
    string contactObject = "", List<string> sbjnum = null, string fullname = "",
    List<string> city = null, string address = "", string street = "", string ward = "",
    string district = "", List<string> phoneNumber = null, string email = "",
    string dateOfBirth = "", List<string> age = null, List<string> sex = null,
    List<string> job = null, List<string> householdIncome = null, List<string> personalIncome = null,
    List<string> maritalStatus = null, string mostFrequentlyUsedBrand = "",
    string source = "", List<string> className = null, string education = "",
    List<string> provinces = null, string qc = "", string qa = "", List<string> Khuvuc = null, List<string> Nganhhang = null, List<string> region = null)
        {
            // Yêu cầu đăng nhập để xuất dữ liệu
            var userRole = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(userRole))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thực hiện chức năng xuất dữ liệu.";
                return RedirectToAction("Login", "LoginRegister");
            }

            try
            {
                // Lấy dữ liệu theo bộ lọc
                var results = getadminChart(stt, code, projectName, year, contactObject, sbjnum, fullname, city, address, street, ward, district, phoneNumber, email, dateOfBirth, age, sex, job, householdIncome, personalIncome, maritalStatus, mostFrequentlyUsedBrand, source, className, education, provinces, qc, qa, Khuvuc, Nganhhang);

                if (results.Count == 0)
                {
                    TempData["ErrorMessage"] = "Không có dữ liệu để xuất.";
                    return RedirectToAction("Index");
                }

                // Tạo file Excel
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Dữ liệu");

                // Thêm header
                worksheet.Cell("A1").Value = "STT";
                worksheet.Cell("B1").Value = "Code";
                worksheet.Cell("C1").Value = "Tên dự án";
                worksheet.Cell("D1").Value = "Năm";
                worksheet.Cell("E1").Value = "Đối tượng liên hệ";
                worksheet.Cell("F1").Value = "Số thứ tự";
                worksheet.Cell("G1").Value = "Họ tên";
                worksheet.Cell("H1").Value = "Thành phố";
                worksheet.Cell("I1").Value = "Địa chỉ";
                worksheet.Cell("J1").Value = "Đường";
                worksheet.Cell("K1").Value = "Phường";
                worksheet.Cell("L1").Value = "Quận/Huyện";
                worksheet.Cell("M1").Value = "Số điện thoại";
                worksheet.Cell("N1").Value = "Email";
                worksheet.Cell("O1").Value = "Năm sinh";
                worksheet.Cell("P1").Value = "Tuổi";
                worksheet.Cell("Q1").Value = "Giới tính";
                worksheet.Cell("R1").Value = "Nghề nghiệp";
                worksheet.Cell("S1").Value = "Thu nhập hộ gia đình";
                worksheet.Cell("T1").Value = "Thu nhập cá nhân";
                worksheet.Cell("U1").Value = "Tình trạng hôn nhân";
                worksheet.Cell("V1").Value = "Thương hiệu sử dụng nhiều nhất";
                worksheet.Cell("W1").Value = "Nguồn";
                worksheet.Cell("X1").Value = "Lớp";
                worksheet.Cell("Y1").Value = "Học vấn";
                worksheet.Cell("Z1").Value = "Tỉnh";
                worksheet.Cell("AA1").Value = "QC";
                worksheet.Cell("AB1").Value = "QA";
                worksheet.Cell("AC1").Value = "Khu vực";
                worksheet.Cell("AD1").Value = "Ngành hàng";
                worksheet.Cell("AE1").Value = "Chuyên khoa";

                // Thêm dữ liệu
                for (int i = 0; i < results.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cell($"A{row}").Value = results[i].Stt;
                    worksheet.Cell($"B{row}").Value = results[i].Code;
                    worksheet.Cell($"C{row}").Value = results[i].ProjectName;
                    worksheet.Cell($"D{row}").Value = results[i].Year;
                    worksheet.Cell($"E{row}").Value = results[i].ContactObject;
                    worksheet.Cell($"F{row}").Value = results[i].Sbjnum;
                    worksheet.Cell($"G{row}").Value = results[i].Fullname;
                    worksheet.Cell($"H{row}").Value = results[i].City;
                    worksheet.Cell($"I{row}").Value = results[i].Address;
                    worksheet.Cell($"J{row}").Value = results[i].Street;
                    worksheet.Cell($"K{row}").Value = results[i].Ward;
                    worksheet.Cell($"L{row}").Value = results[i].District;
                    worksheet.Cell($"M{row}").Value = results[i].PhoneNumber;
                    worksheet.Cell($"N{row}").Value = results[i].Email;
                    worksheet.Cell($"O{row}").Value = results[i].DateOfBirth;
                    worksheet.Cell($"P{row}").Value = results[i].Age;
                    worksheet.Cell($"Q{row}").Value = results[i].Sex;
                    worksheet.Cell($"R{row}").Value = results[i].Job;
                    worksheet.Cell($"S{row}").Value = results[i].HouseholdIncome;
                    worksheet.Cell($"T{row}").Value = results[i].PersonalIncome;
                    worksheet.Cell($"U{row}").Value = results[i].MaritalStatus;
                    worksheet.Cell($"V{row}").Value = results[i].MostFrequentlyUsedBrand;
                    worksheet.Cell($"W{row}").Value = results[i].Source;
                    worksheet.Cell($"X{row}").Value = results[i].Class;
                    worksheet.Cell($"Y{row}").Value = results[i].Education;
                    worksheet.Cell($"Z{row}").Value = results[i].Provinces;
                    worksheet.Cell($"AA{row}").Value = results[i].Qc;
                    worksheet.Cell($"AB{row}").Value = results[i].Qa;
                    worksheet.Cell($"AC{row}").Value = results[i].Khuvuc;
                    worksheet.Cell($"AD{row}").Value = results[i].Nganhhang;
                    worksheet.Cell($"AE{row}").Value = results[i].ChuyenKhoa;
                }

                // Định dạng header
                var headerRange = worksheet.Range("A1:AE1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Tự động điều chỉnh cột
                worksheet.Columns().AdjustToContents();

                // Lưu file vào memory stream
                byte[] fileData;
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    fileData = stream.ToArray();
                }

                // Lấy thông tin user đang đăng nhập
                var username = HttpContext.Session.GetString("Username");
                var userEmail = GetUserEmail(username);

                // Tạo export request
                var exportRequest = new ExportRequest
                {
                    Username = username,
                    Email = userEmail,
                    RequestTime = DateTime.Now,
                    Status = "pending",
                    FilterParams = $"Filtered data export, Results: {results.Count} records",
                    FileData = fileData,
                    Source = "AdminDashboard"
                };

                // Lưu request vào database
                var repo = new ExportRequestRepository(_connectionString);
                var requestId = await repo.AddRequestAsync(exportRequest);

                TempData["SuccessMessage"] = $"Đã gửi yêu cầu xuất Excel thành công! Request ID: {requestId}. Admin sẽ duyệt và gửi file qua email: {userEmail}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi gửi yêu cầu xuất Excel: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Action để clear session (debug purpose)
        public ActionResult ClearSession()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Session đã được xóa thành công.";
            return RedirectToAction("Index");
        }

    }
}
