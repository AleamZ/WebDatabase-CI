using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using CIResearch.Models;
using System.Text;
using System.Drawing;
using System.Linq; // Added for .Where() and .Any()

namespace CIResearch.Controllers
{
    public class Admin : Controller
    {
        private string _connectionString = "Server=localhost;Database=sakila;User=root;Password=123456;DefaultCommandTimeout=1000;ConnectionTimeout=1000;";

        public ActionResult Index(string stt = "", List<string> code = null, List<string> projectName = null, List<string> year = null,
    string contactObject = "", List<string> sbjnum = null, string fullname = "",
    List<string> city = null, string address = "", string street = "", string ward = "",
    string district = "", List<string> phoneNumber = null, string email = "",
    string dateOfBirth = "", List<string> age = null, List<string> sex = null,
    List<string> job = null, List<string> householdIncome = null, List<string> personalIncome = null,
    List<string> maritalStatus = null, string mostFrequentlyUsedBrand = "",
    string source = "", List<string> Classname = null, string education = "",
    List<string> provinces = null, string qc = "", string qa = "", List<string> Khuvuc = null, List<String> Nganhhang = null, List<string> region = null, List<string> chuyenKhoa = null)
        {
            // Chuẩn hóa filter đầu vào
            code = code?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            projectName = projectName?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            year = year?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            city = city?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            sex = sex?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            age = age?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            job = job?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            maritalStatus = maritalStatus?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            Classname = Classname?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            Nganhhang = Nganhhang?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            provinces = provinces?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            Khuvuc = Khuvuc?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            chuyenKhoa = chuyenKhoa?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            // Truyền lại filter về ViewBag
            ViewBag.Code = code;
            ViewBag.Projectname = projectName;
            ViewBag.Year = year;
            ViewBag.City = city;
            ViewBag.Sex = sex;
            ViewBag.Age = age;
            ViewBag.Job = job;
            ViewBag.MaritalStatus = maritalStatus;
            ViewBag.Classname = Classname;
            ViewBag.Nganhhang = Nganhhang;
            ViewBag.Provinces = provinces;
            ViewBag.Khuvuc = Khuvuc;
            ViewBag.ChuyenKhoa = chuyenKhoa;

            // --- TRUYỀN DỮ LIỆU FILTER ĐỘNG ---
            ViewBag.CodeList = GetDistinctCodes();
            ViewBag.ProjectNameList = GetDistinctProjectNames();
            ViewBag.YearList = GetDistinctYears();
            ViewBag.CityList = GetDistinctCities();
            ViewBag.JobList = GetDistinctJobs();
            ViewBag.EducationList = GetDistinctEducations();
            ViewBag.SexList = GetDistinctSexes();
            ViewBag.MaritalStatusList = GetDistinctMaritalStatuses();
            ViewBag.HouseholdIncomeList = GetDistinctHouseholdIncomes();
            ViewBag.PersonalIncomeList = GetDistinctPersonalIncomes();
            ViewBag.DistrictList = GetDistinctDistricts();
            ViewBag.WardList = GetDistinctWards();
            ViewBag.ProvincesList = GetDistinctProvinces();
            ViewBag.ClassList = GetDistinctClasses();
            ViewBag.NganhhangList = GetDistinctNganhhangs();
            ViewBag.QcList = GetDistinctQcs();
            ViewBag.QaList = GetDistinctQas();
            ViewBag.KhuvucList = GetDistinctKhuvucs();
            ViewBag.ChuyenKhoaList = GetDistinctChuyenKhoas();

            var userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Admin")
            {
                // Nếu không phải admin, chuyển hướng người dùng đến trang khác (ví dụ: trang lỗi hoặc login)
                return RedirectToAction("Index", "Home"); // Thay "Home" bằng controller phù hợp nếu cần
            }

            // Lọc dữ liệu
            var adminChart = getadminChart(stt, code, projectName, year, contactObject, sbjnum, fullname, city, address, street, ward, district, phoneNumber, email, dateOfBirth, age, sex, job, householdIncome, personalIncome, maritalStatus, mostFrequentlyUsedBrand, source, Classname, education, provinces, qc, qa, Khuvuc, Nganhhang, chuyenKhoa);

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







            // Log ra file tạm để kiểm tra dữ liệu đã lọc
            var logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "admin_filter_debug.log");
            var logContent = $"[{DateTime.Now}] Tổng số mẫu sau lọc: {adminChart.Count}\n";
            logContent += string.Join("\n", adminChart.Take(10).Select(x => $"STT: {x.Stt}, Code: {x.Code}, ProjectName: {x.ProjectName}, Year: {x.Year}, City: {x.City}, Sex: {x.Sex}"));
            System.IO.File.AppendAllText(logPath, logContent + "\n-------------------\n");

            return View(adminChart);
        }

        // Action hiển thị trang tìm kiếm số điện thoại và xử lý tìm kiếm
        [HttpGet]
        public IActionResult SearchPhoneNumber(string phoneNumber = null)
        {
            List<ALLDATA> results = null;
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                // Tìm kiếm theo số điện thoại (LIKE hoặc =)
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM all_data_final WHERE PHONENUMBER LIKE @phoneNumber";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@phoneNumber", "%" + phoneNumber + "%");
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            results = new List<ALLDATA>();
                            while (reader.Read())
                            {
                                ALLDATA data = new ALLDATA
                                {
                                    Stt = reader.IsDBNull(reader.GetOrdinal("STT")) ? 0 : reader.GetInt32("STT"),
                                    Code = reader.IsDBNull(reader.GetOrdinal("CODE")) ? null : reader.GetString("CODE"),
                                    ProjectName = reader.IsDBNull(reader.GetOrdinal("PROJECTNAME")) ? null : reader.GetString("PROJECTNAME"),
                                    Year = reader.IsDBNull(reader.GetOrdinal("YEAR")) ? 0 : reader.GetInt32("YEAR"),
                                    Fullname = reader.IsDBNull(reader.GetOrdinal("FULLNAME")) ? null : reader.GetString("FULLNAME"),
                                    City = reader.IsDBNull(reader.GetOrdinal("CITY")) ? null : reader.GetString("CITY"),
                                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PHONENUMBER")) ? null : reader.GetString("PHONENUMBER")
                                    // Có thể bổ sung các trường khác nếu muốn hiển thị
                                };
                                results.Add(data);
                            }
                        }
                    }
                }
            }
            ViewBag.PhoneNumber = phoneNumber;
            return View(results);
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
  List<string> provinces, string qc, string qa, List<string> Khuvuc, List<string> Nganhhang, List<string> chuyenKhoa)
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
                if (chuyenKhoa != null && chuyenKhoa.Any())
                {
                    var chuyenKhoaParams = chuyenKhoa.Select((_, i) => $"@chuyenKhoa{i}").ToArray();
                    queryBuilder.Append(" AND ChuyenKhoa IN (" + string.Join(", ", chuyenKhoaParams) + ")");
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
                    if (chuyenKhoa != null && chuyenKhoa.Any())
                        for (int i = 0; i < chuyenKhoa.Count; i++)
                            command.Parameters.AddWithValue($"@chuyenKhoa{i}", chuyenKhoa[i]);






                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ALLDATA Alldatas = new ALLDATA
                            {
                                Stt = reader.IsDBNull(reader.GetOrdinal("STT")) ? 0 : reader.GetInt32("STT"),
                                Code = reader.IsDBNull(reader.GetOrdinal("CODE")) ? null : reader.GetString("CODE"),
                                ProjectName = reader.IsDBNull(reader.GetOrdinal("PROJECTNAME")) ? null : reader.GetString("PROJECTNAME"),
                                Year = reader.IsDBNull(reader.GetOrdinal("YEAR")) ? 0 : reader.GetInt32("YEAR"),
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
                                Age = reader.IsDBNull(reader.GetOrdinal("AGE")) ? 0 : reader.GetInt32("AGE"),
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
                                Nganhhang = reader.IsDBNull(reader.GetOrdinal("Nganhhang")) ? null : reader.GetString("Nganhhang"),
                                ChuyenKhoa = reader.IsDBNull(reader.GetOrdinal("ChuyenKhoa")) ? null : reader.GetString("ChuyenKhoa")
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

        // --- DISTINCT FILTER VALUE HELPERS ---
        private List<string> GetDistinctCodes()
        {
            return GetDistinctValuesFromDb("CODE");
        }
        private List<string> GetDistinctProjectNames()
        {
            return GetDistinctValuesFromDb("PROJECTNAME");
        }
        private List<string> GetDistinctYears()
        {
            return GetDistinctValuesFromDb("YEAR");
        }
        private List<string> GetDistinctCities()
        {
            return GetDistinctValuesFromDb("CITY");
        }
        private List<string> GetDistinctJobs()
        {
            return GetDistinctValuesFromDb("JOB");
        }
        private List<string> GetDistinctEducations()
        {
            return GetDistinctValuesFromDb("EDUCATION");
        }
        private List<string> GetDistinctSexes()
        {
            return GetDistinctValuesFromDb("SEX");
        }
        private List<string> GetDistinctMaritalStatuses()
        {
            return GetDistinctValuesFromDb("MARITALSTATUS");
        }
        private List<string> GetDistinctHouseholdIncomes()
        {
            return GetDistinctValuesFromDb("HOUSEHOLDINCOME");
        }
        private List<string> GetDistinctPersonalIncomes()
        {
            return GetDistinctValuesFromDb("PERSONALINCOME");
        }
        private List<string> GetDistinctDistricts()
        {
            return GetDistinctValuesFromDb("DISTRICT");
        }
        private List<string> GetDistinctWards()
        {
            return GetDistinctValuesFromDb("WARD");
        }
        private List<string> GetDistinctProvinces()
        {
            return GetDistinctValuesFromDb("PROVINCES");
        }
        private List<string> GetDistinctClasses()
        {
            return GetDistinctValuesFromDb("Class");
        }
        private List<string> GetDistinctNganhhangs()
        {
            return GetDistinctValuesFromDb("Nganhhang");
        }
        private List<string> GetDistinctQcs()
        {
            return GetDistinctValuesFromDb("QC");
        }
        private List<string> GetDistinctQas()
        {
            return GetDistinctValuesFromDb("QA");
        }
        private List<string> GetDistinctKhuvucs()
        {
            return GetDistinctValuesFromDb("KHUVUC");
        }
        private List<string> GetDistinctChuyenKhoas()
        {
            return GetDistinctValuesFromDb("ChuyenKhoa");
        }
        // Helper for all distinct value queries
        private List<string> GetDistinctValuesFromDb(string column)
        {
            var values = new List<string>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = $"SELECT DISTINCT `{column}` FROM all_data_final WHERE `{column}` IS NOT NULL AND `{column}` != '' ORDER BY `{column}`";
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var val = reader[0]?.ToString();
                        if (!string.IsNullOrWhiteSpace(val))
                            values.Add(val);
                    }
                }
            }
            return values;
        }


    }
}
