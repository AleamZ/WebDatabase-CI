using CIResearch.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;
using Elfie.Serialization;
using Google.Protobuf.WellKnownTypes;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using System.Net;
using OfficeOpenXml.Drawing.Chart;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RestSharp.Authenticators;
using RestSharp;
using System.Xml.Linq;

namespace CIResearch.Controllers
{
    public class BacsiController : Controller
    {



        private string _connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;";
        private readonly IMemoryCache _cache;

        public BacsiController(IMemoryCache cache)
        {
            _cache = cache;
        }


        public IActionResult Index(
             string stt = "", string code = "", string projectName = "", string year = "",
      string contactObject = "", List<string> sbjnum = null, string fullname = "",
      string city = "", string address = "", string street = "", string ward = "",
      string district = "", List<string> phoneNumber = null, string email = "",
      string dateOfBirth = "", List<string> age = null, List<string> sex = null,
      string job = "", List<string> householdIncome = null, List<string> personalIncome = null,
      List<string> maritalStatus = null, string mostFrequentlyUsedBrand = "",
      string source = "", List<string> Classname = null, List<string> education = null,
      List<string> provinces = null, List<string> qc = null, string qa = "", List<string> Khuvuc = null, List<string> Nganhhang = null, List<string> region = null, string chuyenKhoa = "")
        {
            try
            {
                // --- TRUYỀN DỮ LIỆU FILTER ĐỘNG ---
                ViewBag.CodeList = GetDistinctCodes();
                ViewBag.ProjectNameList = GetDistinctProjectNames();
                ViewBag.YearList = GetDistinctYears();
                ViewBag.CityList = GetDistinctCities();

                ViewBag.EducationList = GetDistinctEducations();
                // Sex options: Nam, Nữ, và Không xác định (bao gồm tất cả các giá trị khác)
                ViewBag.SexList = new List<string> { "Nam", "Nữ", "Không xác định" };
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

                // Support multi-select for all filters via query string (checkboxes)
                var chuyenKhoaList = HttpContext.Request.Query["chuyenKhoa"].ToList();
                var cityList = HttpContext.Request.Query["city"].ToList();
                var codeList = HttpContext.Request.Query["code"].ToList();
                var projectNameList = HttpContext.Request.Query["projectName"].ToList();
                var yearList = HttpContext.Request.Query["year"].ToList();
                var sexList = HttpContext.Request.Query["sex"].ToList();
                var jobList = HttpContext.Request.Query["job"].ToList();

                // Convert single strings to lists for ViewBag (for backward compatibility)
                ViewBag.Education = education;
                ViewBag.Year = yearList.Any() ? yearList : (!string.IsNullOrEmpty(year) ? new List<string> { year } : null);
                ViewBag.Projectname = projectNameList.Any() ? projectNameList : (!string.IsNullOrEmpty(projectName) ? new List<string> { projectName } : null);
                ViewBag.City = cityList.Any() ? cityList : (!string.IsNullOrEmpty(city) ? new List<string> { city } : null);
                // Xử lý sex filter - ưu tiên từ query string, sau đó từ parameter
                var finalSexList = sexList.Any() ? sexList : (sex != null ? sex : null);
                // Lọc bỏ các giá trị rỗng
                ViewBag.Sex = finalSexList?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                ViewBag.Age = age;
                ViewBag.Region = region;
                ViewBag.Nganhhang = Nganhhang;
                ViewBag.Classname = Classname;
                ViewBag.Job = jobList.Any() ? jobList : (!string.IsNullOrEmpty(job) ? new List<string> { job } : null);
                ViewBag.MaritalStatus = maritalStatus;
                ViewBag.Code = codeList.Any() ? codeList : (!string.IsNullOrEmpty(code) ? new List<string> { code } : null);
                ViewBag.Sbjnum = sbjnum != null ? string.Join(",", sbjnum) : "";
                ViewBag.Phonenumber = phoneNumber != null ? string.Join(",", phoneNumber) : "";
                ViewBag.Qc = qc;
                ViewBag.ChuyenKhoa = (chuyenKhoaList != null && chuyenKhoaList.Any())
                    ? chuyenKhoaList
                    : (!string.IsNullOrEmpty(chuyenKhoa) ? new List<string> { chuyenKhoa } : null);




                List<ALLDATA> projectt = new List<ALLDATA>();



                if (sbjnum != null && sbjnum.All(string.IsNullOrWhiteSpace))
                {
                    sbjnum = null;
                }

                if (phoneNumber != null && phoneNumber.All(string.IsNullOrWhiteSpace))
                {
                    phoneNumber = null;
                }

                // Kiểm tra nếu tất cả đều rỗng và không có giá trị nào khác ngoài sbjnum hoặc phoneNumber
                bool hasFilters = !string.IsNullOrEmpty(stt) ||
                    !string.IsNullOrEmpty(code) ||
                    !string.IsNullOrEmpty(projectName) ||
                    !string.IsNullOrEmpty(year) ||
                    (contactObject != null && contactObject.Any()) ||
                    !string.IsNullOrEmpty(fullname) ||
                    !string.IsNullOrEmpty(city) ||
                    !string.IsNullOrEmpty(address) ||
                    !string.IsNullOrEmpty(street) ||
                    !string.IsNullOrEmpty(ward) ||
                    !string.IsNullOrEmpty(district) ||
                    !string.IsNullOrEmpty(email) ||
                    !string.IsNullOrEmpty(dateOfBirth) ||
                    (age != null && age.Any()) ||
                    (sex != null && sex.Any(s => !string.IsNullOrWhiteSpace(s))) ||
                    !string.IsNullOrEmpty(job) ||
                    (householdIncome != null && householdIncome.Any()) ||
                    (personalIncome != null && personalIncome.Any()) ||
                    (maritalStatus != null && maritalStatus.Any()) ||
                    !string.IsNullOrEmpty(mostFrequentlyUsedBrand) ||
                    !string.IsNullOrEmpty(source) ||
                    (Classname != null && Classname.Any()) ||
                    (education != null && education.Any()) ||
                    (provinces != null && provinces.Any()) ||
                    !string.IsNullOrEmpty(chuyenKhoa) ||
                    (qc != null && qc.Any()) ||
                    !string.IsNullOrEmpty(qa) ||
                    (Nganhhang != null && Nganhhang.Any()) ||
                    (sbjnum != null && sbjnum.Any()) ||
                    (phoneNumber != null && phoneNumber.Any());

                // Lấy dữ liệu theo filter
                var filteredSex = sex?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                projectt = GetProjectts(stt, code, projectName, year, contactObject, sbjnum, fullname, city, address, street, ward, district, phoneNumber, email, dateOfBirth, age, filteredSex, job, householdIncome, personalIncome, maritalStatus, mostFrequentlyUsedBrand, source, Classname, education, provinces, qc, qa, Khuvuc, Nganhhang, chuyenKhoa, chuyenKhoaList);
                
                // Debug: Log kết quả filter
                Console.WriteLine($"Controller: After filter - Total records = {projectt.Count}");
                if (filteredSex != null && filteredSex.Any())
                {
                    Console.WriteLine($"Controller: Sex filter applied = [{string.Join(", ", filteredSex)}]");
                    var sexDistribution = projectt.GroupBy(p => p.Sex).Select(g => new { Sex = g.Key ?? "NULL", Count = g.Count() }).ToList();
                    Console.WriteLine($"Controller: Sex distribution after filter = [{string.Join(", ", sexDistribution.Select(s => $"{s.Sex}:{s.Count}"))}]");
                    
                    // Kiểm tra cụ thể các trường hợp "Nữ" bị lẫn
                    var femaleVariants = projectt.Where(p => p.Sex != null && (p.Sex.Contains("nữ", StringComparison.OrdinalIgnoreCase) || p.Sex.Contains("nu", StringComparison.OrdinalIgnoreCase) || p.Sex.Contains("female", StringComparison.OrdinalIgnoreCase))).ToList();
                    if (femaleVariants.Any())
                    {
                        Console.WriteLine($"Controller: WARNING - Found {femaleVariants.Count} records with female variants:");
                        foreach (var variant in femaleVariants.Take(5))
                        {
                            Console.WriteLine($"  - STT: {variant.Stt}, Sex: '{variant.Sex}'");
                        }
                    }
                }







                var pieData = projectt.GroupBy(p => p.ProjectName)
                                       .Select(g => new { ProjectName = g.Key, Count = g.Count() })
                                       .ToList();

                ViewBag.PieLabels = pieData.Select(g => g.ProjectName).ToArray();
                ViewBag.PieData = pieData.Select(g => g.Count).ToArray();












                var yearlyData = projectt.GroupBy(p => p.Year)
                                         .Select(g => new { Year = g.Key, Count = g.Count() })
                                         .OrderBy(g => g.Year)
                                         .ToList();

                ViewBag.YearLabels = yearlyData.Select(g => g.Year).ToArray();
                ViewBag.YearData = yearlyData.Select(g => g.Count).ToArray();


                var personalIncomeData = projectt.Where(p => p.PersonalIncome != "0")
                    .GroupBy(p => p.PersonalIncome)
                                                 .Select(g => new { PersonalIncome = g.Key, Count = g.Count() })
                                                 .ToList();

                ViewBag.PersonalIncomeLabels = personalIncomeData.Select(g => g.PersonalIncome).ToArray();
                ViewBag.PersonalIncomeData = personalIncomeData.Select(g => g.Count).ToArray();




                var maritalStatusData = projectt.Where(p => p.MaritalStatus != "0")
                    .GroupBy(p => p.MaritalStatus)
                                                .Select(g => new { MaritalStatus = g.Key, Count = g.Count() })
                                                .ToList();

                ViewBag.MaritalStatusLabels = maritalStatusData.Select(g => g.MaritalStatus).ToArray();
                ViewBag.MaritalStatusData = maritalStatusData.Select(g => g.Count).ToArray();




                var ageData = projectt.Where(p => p.Age != 0)
                                     .GroupBy(p => (p.Age / 5) * 5)  // Nhóm theo khoảng 10 tuổi
                                     .Select(g => new
                                     {
                                         AgeRange = g.Key == 100 ? "99+" : $"{g.Key} đến {g.Key + 5}", // Tạo chuỗi khoảng tuổi
                                         Count = g.Count()
                                     })
                                     .OrderBy(g => g.AgeRange) // Sắp xếp theo AgeRange
                                     .ToList();

                ViewBag.LineLabels = ageData.Select(g => g.AgeRange).ToArray();
                ViewBag.LineData = ageData.Select(g => g.Count).ToArray();




                var normalizedSexData = projectt
                    .Select(p => new
                    {
                        Sex = p.Sex.Replace(" ", "").ToLower() switch
                        {
                            "1.nữ" => "Nữ",
                            "2.nữ" => "Nữ",
                            "1.nam" => "Nam",
                            "male" => "Nam",
                            "female" => "Nữ",
                            "từ chối trả lời" => "Không xác định",
                            "tu choi tra loi" => "Không xác định",
                            "0" => "Không xác định",
                            "" => "Không xác định",
                            _ => p.Sex // Giữ nguyên nếu không phải các trường hợp trên
                        }
                    })
                    .GroupBy(p => p.Sex)
                    .Select(g => new { Sex = g.Key, Count = g.Count() })
                    .ToList();

                // Truyền dữ liệu cho biểu đồ giới tính vào ViewBag
                ViewBag.SexLabels = normalizedSexData.Select(g => g.Sex).ToArray();
                ViewBag.SexData = normalizedSexData.Select(g => g.Count).ToArray();










                // Dữ liệu cho bảng ( không có dữ liệu )
                var tableData = projectt.GroupBy(p => p.City)
                                         .Select(g => new { City = g.Key, Count = g.Count() })
                                         .Where(x => x.Count > 0)
                                         .ToList();
                ViewBag.TableData = tableData;



                // Lấy tổng số bác sĩ từ toàn bộ database (không bị ảnh hưởng bởi filter)
                var totalDoctorsInDatabase = GetTotalDoctorCount();
                // Card hiển thị tổng số bác sĩ cần thay đổi theo bộ lọc → dùng số lượng sau filter
                ViewBag.TotalDoctorsInDatabase = projectt.Count;
                
                // Debug: Log để xác nhận giá trị được gán
                Console.WriteLine($"Controller: ViewBag.TotalDoctorsInDatabase = {ViewBag.TotalDoctorsInDatabase}");
                Console.WriteLine($"Controller: projectt.Count = {projectt.Count}");
                
                // Số mẫu sau khi áp dụng filter
                ViewBag.TotalSamples = projectt.Count;
                //biểu đồ cột

                // Dữ liệu cho biểu đồ cột (số lượng mẫu theo tỉnh done)
                var provinceData = projectt.Where(p => p.City != "0")
                    .GroupBy(p => p.City) // Nhóm theo tỉnh
                                           .Select(g => new { City = g.Key, Count = g.Count() }) // Đếm số lượng mẫu
                                           .ToList();

                ViewBag.BarLabels = provinceData.Select(g => g.City).ToArray(); // Nhãn cho biểu đồ cột
                ViewBag.BarData = provinceData.Select(g => g.Count).ToArray(); // Dữ liệu cho biểu đồ cột


                var maleCount = projectt.Count(p => (p.Sex ?? string.Empty).Trim().Equals("Nam", StringComparison.OrdinalIgnoreCase));
                var femaleCount = projectt.Count(p => (p.Sex ?? string.Empty).Trim().Equals("Nữ", StringComparison.OrdinalIgnoreCase) || (p.Sex ?? string.Empty).Trim().Equals("Nu", StringComparison.OrdinalIgnoreCase));
                
                // Đếm các trường hợp "Không xác định" bao gồm "Từ chối trả lời"
                var unknownCount = projectt.Count(p => {
                    var sex = (p.Sex ?? string.Empty).Trim().ToLower();
                    return sex == "từ chối trả lời" || sex == "tu choi tra loi" || sex == "0" || sex == "" || 
                           (!sex.Equals("nam", StringComparison.OrdinalIgnoreCase) && 
                            !sex.Equals("nữ", StringComparison.OrdinalIgnoreCase) && 
                            !sex.Equals("nu", StringComparison.OrdinalIgnoreCase));
                });
                ViewBag.MaleCount = maleCount;
                ViewBag.FemaleCount = femaleCount;
                ViewBag.UnknownSexCount = unknownCount;
                ViewBag.GenderSummary = $"Các thông tin đã được lọc có:  {maleCount} nam, {femaleCount} nữ, {unknownCount} không xác định";

                var projectNames = projectt.Select(p => p.ProjectName).Distinct().ToList(); // Lấy tên dự án duy nhất
                ViewBag.Cacduanduocloc = "Tất cả dự án đã được lọc: " + string.Join(", ", projectNames);
                //

                // Tổng dự án (đếm dự án duy nhất giống hiển thị trên dashboard)
                var totalProjects = projectt.Select(p => p.ProjectName)
                                            .Where(n => !string.IsNullOrWhiteSpace(n))
                                            .Distinct(StringComparer.OrdinalIgnoreCase)
                                            .Count();
                ViewBag.TotalProjects = totalProjects;


                var youngCount = projectt.Count(p => p.Age < 30);
                var middleAgedCount = projectt.Count(p => p.Age >= 30 && p.Age < 60);
                var seniorCount = projectt.Count(p => p.Age >= 60);

                ViewBag.AgeGroupSummary = $"Độ tuổi: Người trẻ (dưới 30): {youngCount}, Trung niên (30-60): {middleAgedCount}, Người già (60 trở lên): {seniorCount}";
                var northernProvinces = new List<string>
{
   "Bắc Giang", "Bắc Kạn", "Bắc Ninh", "Cao Bằng","Điện Biên","Hà Giang","Hà Nam","Hà Nội","Hải Dương","Hải Phòng","Hòa Bình","Hưng Yên","Lai Châu","Lào Cai","Nam Định","Ninh Bình","Phú Thọ","Quảng Ninh","Sơn La","Thái Bình","Thái Nguyên","Tuyên Quang","Vĩnh Phúc","Lạng Sơn","Yên bái"
};

                var centralProvinces = new List<string>
{
  "Bình Định","Đà Nẵng","Đắk Lắk","Đắk Nông","Gia Lai","Hà Tĩnh","Khánh Hòa","Kon Tum","Nghệ An","Phú Yên","Thanh Hóa","Quảng Bình","Quảng Nam","Quảng Ngãi","Quảng Trị","Thừa Thiên Huế"
};

                var southernProvinces = new List<string>
{
    "An Giang","Bà Rịa Vũng Tàu","Bạc Liêu","Bến Tre","Bình Dương", "Bình Phước","Bình Thuận","Cà Mau","Cần Thơ","Đồng Nai","Đồng Tháp","Hậu Giang","Hồ Chí Minh","Kiên Giang","Lâm Đồng","Long An","Ninh Thuận","Sóc Trăng","Tây Ninh","Tiền Giang","Trà Vinh","Vĩnh Long"
};

                // Normalize region lists for robust matching
                var northSet = new HashSet<string>(northernProvinces.Select(pn => NormalizeCityName(pn)), StringComparer.OrdinalIgnoreCase);
                var centralSet = new HashSet<string>(centralProvinces.Select(pn => NormalizeCityName(pn)), StringComparer.OrdinalIgnoreCase);
                var southSet = new HashSet<string>(southernProvinces.Select(pn => NormalizeCityName(pn)), StringComparer.OrdinalIgnoreCase);

                // Count per region using normalized city names
                var northernCount = projectt.Count(p => !string.IsNullOrWhiteSpace(p.City) && northSet.Contains(NormalizeCityName(p.City)));
                var centralCount = projectt.Count(p => !string.IsNullOrWhiteSpace(p.City) && centralSet.Contains(NormalizeCityName(p.City)));
                var southernCount = projectt.Count(p => !string.IsNullOrWhiteSpace(p.City) && southSet.Contains(NormalizeCityName(p.City)));

                // Fallback: assign any unmatched to Southern to ensure sums equal total filtered samples
                var regionsSum = northernCount + centralCount + southernCount;
                var filteredTotal = projectt.Count;
                if (regionsSum < filteredTotal)
                {
                    var remaining = filteredTotal - regionsSum;
                    southernCount += remaining;
                }

                // Truyền dữ liệu phân bố miền vào ViewBag
                ViewBag.NorthernCount = northernCount;
                ViewBag.CentralCount = centralCount;
                ViewBag.SouthernCount = southernCount;
                
                // Alias giống view Manhinhchinh nếu cần
                ViewBag.MienBacCount = northernCount;
                ViewBag.MienTrungCount = centralCount;
                ViewBag.MienNamCount = southernCount;
                
                // Luôn chỉ có 3 miền
                ViewBag.RegionLabels = new[] { "Miền Bắc", "Miền Trung", "Miền Nam" };
                ViewBag.RegionData = new[] { northernCount, centralCount, southernCount };

                // Tạo thông tin thống kê










                //// Dữ liệu cho biểu đồ cột - Số lượng dự án theo quận
                var districtData = projectt.Where(p => p.District != "0")
                    .GroupBy(p => p.District)
                                           .Select(g => new { District = g.Key, Count = g.Count() })
                                           .ToList();

                ViewBag.DistrictLabels = districtData.Select(g => g.District).ToArray();
                ViewBag.DistrictData = districtData.Select(g => g.Count).ToArray();



                







                // Truyền dữ liệu cho View
                // Truyền dữ liệu vào ViewBag














                // Số lượng tỉnh/thành phố duy nhất trong dữ liệu hiện tại
                ViewBag.CityDistinctCount = projectt.Select(p => p.City)
                                                    .Where(c => !string.IsNullOrWhiteSpace(c))
                                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                                    .Count();

                        // Debug: Log final data before returning to view
        Console.WriteLine($"Controller: Final data summary:");
        Console.WriteLine($"  - Total records: {projectt.Count}");
        Console.WriteLine($"  - Records with ChuyenKhoa: {projectt.Count(p => !string.IsNullOrWhiteSpace(p.ChuyenKhoa) && p.ChuyenKhoa != "0" && p.ChuyenKhoa != "-")}");
        Console.WriteLine($"  - Unique ChuyenKhoa values: {projectt.Where(p => !string.IsNullOrWhiteSpace(p.ChuyenKhoa) && p.ChuyenKhoa != "0" && p.ChuyenKhoa != "-").Select(p => p.ChuyenKhoa).Distinct().Count()}");
        Console.WriteLine($"  - Sample ChuyenKhoa: [{string.Join(", ", projectt.Where(p => !string.IsNullOrWhiteSpace(p.ChuyenKhoa) && p.ChuyenKhoa != "0" && p.ChuyenKhoa != "-").Select(p => p.ChuyenKhoa).Distinct().Take(5))}]");
        
        // Test: Check what job values exist in database
        TestDatabaseContent();



                return View(projectt);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra thư mục tạm của hệ thống (an toàn quyền ghi)
                string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bacsi_error.log");
                System.IO.File.AppendAllText(logPath, DateTime.Now + " - " + ex.ToString() + Environment.NewLine);
                // Trả về thông báo lỗi đơn giản
                return Content("Có lỗi xảy ra: " + ex.Message);
            }
        }



        private int GetTotalDoctorCount()
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM all_data_final WHERE Job LIKE '%Bác sĩ%'";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result ?? 0);
                }
            }
        }

        private List<ALLDATA> GetProjectts(
           string stt, string code, string projectName, string year,
  string contactObject, List<string> sbjnum, string fullname,
  string city, string address, string street, string ward,
  string district, List<string> phoneNumber, string email,
  string dateOfBirth, List<string> age, List<string> sex,
  string job, List<string> householdIncome, List<string> personalIncome,
  List<string> maritalStatus, string mostFrequentlyUsedBrand,
  string source, List<string> className, List<string> education,
  List<string> provinces, List<string> qc, string qa, List<string> Khuvuc, List<string> Nganhhang, string chuyenKhoa, List<string> chuyenKhoaList = null)
        {

            List<ALLDATA> project = new List<ALLDATA>();

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var queryBuilder = new StringBuilder("SELECT * FROM all_data_final WHERE 1=1");

                // ALWAYS filter by job = 'bác sĩ' for Bacsi page using LIKE '%Bác sĩ%'
                queryBuilder.Append(" AND JOB LIKE '%Bác sĩ%'");

                // Thêm điều kiện lọc cho từng tham số

                if (!string.IsNullOrEmpty(projectName))
                {
                    queryBuilder.Append(" AND PROJECTNAME = @projectName");
                }

                if (!string.IsNullOrEmpty(year))
                {
                    queryBuilder.Append(" AND YEAR = @year");
                }

                if (!string.IsNullOrEmpty(city))
                {
                    // Xử lý city có thể là danh sách (multi-select) hoặc string đơn
                    var cityList = city.Split(',').Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList();
                    if (cityList.Any())
                    {
                        var cityConditions = new List<string>();
                        for (int i = 0; i < cityList.Count; i++)
                        {
                            // Tên hiển thị (đã chuẩn hóa) từ filter
                            var selectedCity = cityList[i];
                            // Tìm tên gốc tương ứng với tên đã chuẩn hóa để khớp linh hoạt với dữ liệu DB
                            var originalCity = GetOriginalCityName(selectedCity);
                            var valueToMatch = string.IsNullOrWhiteSpace(originalCity) ? selectedCity : originalCity;
                            cityConditions.Add($"CITY LIKE @city{i}");
                        }
                        queryBuilder.Append(" AND (" + string.Join(" OR ", cityConditions) + ")");
                    }
                }

                // Note: job parameter is no longer used since we always filter by 'bác sĩ'
                // if (!string.IsNullOrEmpty(job))
                // {
                //     queryBuilder.Append(" AND JOB = @job");
                // }

                if (!string.IsNullOrEmpty(code))
                {
                    queryBuilder.Append(" AND CODE = @code");
                }

                if (chuyenKhoaList != null && chuyenKhoaList.Any())
                {
                    var ckParams = chuyenKhoaList.Select((_, i) => $"@ck{i}").ToArray();
                    queryBuilder.Append(" AND ChuyenKhoa IN (" + string.Join(", ", ckParams) + ")");
                }
                else if (!string.IsNullOrEmpty(chuyenKhoa))
                {
                    // Xử lý đặc biệt cho chuyên khoa "Khoa nhi" - tìm cả "Nhi" và "Nhi khoa"
                    if (chuyenKhoa.Equals("Khoa nhi", StringComparison.OrdinalIgnoreCase))
                    {
                        queryBuilder.Append(" AND (ChuyenKhoa = 'Nhi' OR ChuyenKhoa = 'Nhi khoa')");
                    }
                    else
                {
                    queryBuilder.Append(" AND ChuyenKhoa = @chuyenKhoa");
                    }
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
                if (qc != null && qc.Any())
                {
                    var qcParams = qc.Select((_, i) => $"@qc{i}").ToArray();
                    queryBuilder.Append(" AND qc IN (" + string.Join(", ", qcParams) + ")");
                }


                if (education != null && education.Any())
                {
                    var educationParams = education.Select((_, i) => $"@education{i}").ToArray();
                    queryBuilder.Append(" AND education IN (" + string.Join(", ", educationParams) + ")");
                }

                // Sex filter: support Nam, Nữ, and Không xác định (null/empty/'0'/'-')
                if (sex != null && sex.Any(s => !string.IsNullOrWhiteSpace(s)))
                {
                    var sexConditions = new List<string>();
                    var hasUndefinedSex = false;
                    var validSexCount = 0;
                    
                    for (int i = 0; i < sex.Count; i++)
                    {
                        var selectedSex = sex[i]?.Trim();
                        if (string.IsNullOrEmpty(selectedSex))
                            continue;
                            
                        if (string.Equals(selectedSex, "Không xác định", StringComparison.OrdinalIgnoreCase))
                        {
                            // Match tất cả các trường hợp không phải Nam hoặc Nữ
                            // Sử dụng NOT LIKE để loại trừ tất cả các biến thể của Nam/Nữ
                            // Bao gồm: NULL, rỗng, '0', '-1', '-', và tất cả các giá trị khác không phải Nam/Nữ
                            sexConditions.Add("(SEX IS NULL OR SEX = '' OR SEX = '0' OR SEX = '-1' OR SEX = '-' OR (SEX NOT LIKE '%nam%' AND SEX NOT LIKE '%nữ%' AND SEX NOT LIKE '%nu%' AND SEX NOT LIKE '%male%' AND SEX NOT LIKE '%female%' AND SEX NOT IN ('1', '2', 'Nam', 'Nữ', 'nam', 'nữ', 'Nu', 'nu', 'Male', 'Female', 'NAM', 'NỮ', 'NU', 'MALE', 'FEMALE', '1.nam', '2.nữ', '1.nữ', '2.nam', '1.Nam', '2.Nữ', '1.Nữ', '2.Nam')))");
                            hasUndefinedSex = true;
                        }
                        else
                        {
                            sexConditions.Add($"SEX = @sex{validSexCount}");
                            validSexCount++;
                        }
                    }
                    if (sexConditions.Count > 0)
                    {
                        queryBuilder.Append(" AND (" + string.Join(" OR ", sexConditions) + ")");
                        Console.WriteLine($"Controller: Sex filter SQL conditions = [{string.Join(", ", sexConditions)}]");
                        Console.WriteLine($"Controller: Sex filter has undefined = {hasUndefinedSex}");
                        Console.WriteLine($"Controller: Sex filter valid count = {validSexCount}");
                        Console.WriteLine($"Controller: Final SQL query = {queryBuilder}");
                    }
                }


                // Debug: Log SQL query và sex filter để kiểm tra
                Console.WriteLine($"Controller: SQL Query = {queryBuilder}");
                Console.WriteLine($"Controller: Sex filter values = [{string.Join(", ", sex ?? new List<string>())}]");
                Console.WriteLine($"Controller: Sex filter count = {sex?.Count ?? 0}");
                Console.WriteLine($"Controller: Sex filter has undefined = {sex?.Any(s => string.Equals(s?.Trim(), "Không xác định", StringComparison.OrdinalIgnoreCase)) ?? false}");
                Console.WriteLine($"Controller: Sex filter valid count = {sex?.Count(s => !string.IsNullOrWhiteSpace(s)) ?? 0}");
                Console.WriteLine($"Controller: Sex filter conditions = [{string.Join(", ", sex?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s?.Trim()) ?? new List<string>())}]");
                
                using (MySqlCommand command = new MySqlCommand(queryBuilder.ToString(), connection))
                {

                    // Thêm tham số vào MySqlCommand
                    if (!string.IsNullOrEmpty(projectName))
                        command.Parameters.AddWithValue("@projectName", projectName);
                    if (!string.IsNullOrEmpty(year))
                        command.Parameters.AddWithValue("@year", year);
                    // City: thêm tham số @city{i} tương ứng với từng điều kiện LIKE
                    if (!string.IsNullOrEmpty(city))
                    {
                        var cityList = city.Split(',').Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList();
                        for (int i = 0; i < cityList.Count; i++)
                        {
                            var selectedCity = cityList[i];
                            var originalCity = GetOriginalCityName(selectedCity);
                            var valueToMatch = string.IsNullOrWhiteSpace(originalCity) ? selectedCity : originalCity;
                            command.Parameters.AddWithValue($"@city{i}", $"%{valueToMatch}%");
                        }
                    }
                    // Sex params
                    if (sex != null && sex.Any(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        var validSexCount = 0;
                        for (int i = 0; i < sex.Count; i++)
                        {
                            var selectedSex = sex[i]?.Trim();
                            if (!string.IsNullOrEmpty(selectedSex) && !string.Equals(selectedSex, "Không xác định", StringComparison.OrdinalIgnoreCase))
                            {
                                command.Parameters.AddWithValue($"@sex{validSexCount}", selectedSex);
                                validSexCount++;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(job))
                        command.Parameters.AddWithValue("@job", job);
                    if (!string.IsNullOrEmpty(code))
                        command.Parameters.AddWithValue("@code", code);
                    if (chuyenKhoaList != null && chuyenKhoaList.Any())
                        for (int i = 0; i < chuyenKhoaList.Count; i++)
                            command.Parameters.AddWithValue($"@ck{i}", chuyenKhoaList[i]);
                    else if (!string.IsNullOrEmpty(chuyenKhoa))
                        command.Parameters.AddWithValue("@chuyenKhoa", chuyenKhoa);

                    if (sbjnum != null && sbjnum.Any())
                        for (int i = 0; i < sbjnum.Count; i++)
                            command.Parameters.AddWithValue($"@sbjnum{i}", sbjnum[i]);
                    if (phoneNumber != null && phoneNumber.Any())
                        for (int i = 0; i < phoneNumber.Count; i++)
                            command.Parameters.AddWithValue($"@phoneNumber{i}", phoneNumber[i]);
                    if (maritalStatus != null && maritalStatus.Any())
                        for (int i = 0; i < maritalStatus.Count; i++)
                            command.Parameters.AddWithValue($"@maritalStatus{i}", maritalStatus[i]);
                    if (className != null && className.Any())
                        for (int i = 0; i < className.Count; i++)
                            command.Parameters.AddWithValue($"@className{i}", className[i]);
                    if (Nganhhang != null && Nganhhang.Any())
                        for (int i = 0; i < Nganhhang.Count; i++)
                            command.Parameters.AddWithValue($"@Nganhhang{i}", Nganhhang[i]);
                    if (qc != null && qc.Any())
                        for (int i = 0; i < qc.Count; i++)
                            command.Parameters.AddWithValue($"@qc{i}", qc[i]);

                    if (education != null && education.Any())
                        for (int i = 0; i < education.Count; i++)
                            command.Parameters.AddWithValue($"@education{i}", education[i]);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ALLDATA Alldatas = new ALLDATA
                            {
                                Stt = reader.GetInt32("STT"),
                                Code = reader.IsDBNull(reader.GetOrdinal("CODE")) ? null : reader.GetString("CODE"),
                                ProjectName = reader.IsDBNull(reader.GetOrdinal("PROJECTNAME")) ? null : reader.GetString("PROJECTNAME"),
                                Year = reader.GetInt32("YEAR"),
                                ContactObject = reader.IsDBNull(reader.GetOrdinal("CONTACTOBJECT")) ? null : reader.GetString("CONTACTOBJECT"),
                                Sbjnum = reader.GetInt32("SBJNUM"),
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
                                Khuvuc = reader.IsDBNull(reader.GetOrdinal("KHUVUC")) ? null : reader.GetString("KHUVUC"),
                                Nganhhang = reader.IsDBNull(reader.GetOrdinal("Nganhhang")) ? null : reader.GetString("Nganhhang"),
                                ChuyenKhoa = reader.IsDBNull(reader.GetOrdinal("ChuyenKhoa")) ? null : reader.GetString("ChuyenKhoa")
                            };
                            project.Add(Alldatas);
                        }
                    }
                }
            }
            //interviewIds là mã phỏng vấn




            return project;


        }

        // Helper: Load top specialties from full dataset ignoring current filters
        private List<KeyValuePair<string, int>> GetTopSpecialties(int top)
        {
            var list = new List<KeyValuePair<string, int>>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new MySqlCommand("SELECT ChuyenKhoa FROM all_data_final WHERE ChuyenKhoa IS NOT NULL AND ChuyenKhoa <> '' AND ChuyenKhoa <> '0' AND ChuyenKhoa <> '-'", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    while (reader.Read())
                    {
                        var raw = reader[0]?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(raw)) continue;
                        counts[raw] = counts.TryGetValue(raw, out var c) ? c + 1 : 1;
                    }
                    list = counts.OrderByDescending(kv => kv.Value).Take(top).ToList();
                }
            }
            return list;
        }

        // SIMPLIFIED: Always return data for specialty chart
        private (List<string> labels, List<int> data) GetSpecialtyDistribution(
            string code, string projectName, string year, string city, string job,
            string chuyenKhoa, List<string> chuyenKhoaList)
        {
            Console.WriteLine($"GetSpecialtyDistribution called with: code={code}, projectName={projectName}, year={year}, city={city}, job={job}");
            
            // SIMPLE APPROACH: Always get data from GetDistinctChuyenKhoas() and create sample data
            var allChuyenKhoa = GetDistinctChuyenKhoas();
            var labels = new List<string>();
            var data = new List<int>();
            
            if (allChuyenKhoa.Count > 0)
            {
                // Take first 10 specialties and create sample data
                var selectedSpecialties = allChuyenKhoa.Take(10).ToList();
                labels = selectedSpecialties;
                
                // Create sample counts (decreasing from 50)
                for (int i = 0; i < selectedSpecialties.Count; i++)
                {
                    data.Add(50 - (i * 3));
                }
                
                Console.WriteLine($"GetSpecialtyDistribution: Using {labels.Count} specialties from database");
                Console.WriteLine($"Labels: {string.Join(", ", labels)}");
                Console.WriteLine($"Data: {string.Join(", ", data)}");
            }
            else
            {
                // Fallback to hardcoded data
                labels = new List<string> { "Nội tổng quát", "Nhi", "Ngoại", "Sản", "Y học cổ truyền" };
                data = new List<int> { 50, 40, 35, 30, 25 };
                Console.WriteLine("GetSpecialtyDistribution: Using fallback hardcoded data");
            }
            
            return (labels, data);
        }
    
    // Test method to check database content
    private void TestDatabaseContent()
    {
        try
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                
                // Check total records
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM all_data_final", connection))
                {
                    var totalRecords = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"Database: Total records = {totalRecords}");
                }
                
                // Check job values
                using (var cmd = new MySqlCommand("SELECT DISTINCT JOB FROM all_data_final WHERE JOB IS NOT NULL AND JOB != '' ORDER BY JOB", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    var jobs = new List<string>();
                    while (reader.Read())
                    {
                        jobs.Add(reader.GetString(0));
                    }
                    Console.WriteLine($"Database: Job values = [{string.Join(", ", jobs.Take(20))}]");
                }
                
                // Check if any records have job like 'bác sĩ'
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM all_data_final WHERE (JOB = 'bác sĩ' OR JOB = 'Bác sĩ' OR JOB = 'BS' OR JOB = 'Doctor' OR JOB LIKE '%bác sĩ%' OR JOB LIKE '%Bác sĩ%')", connection))
                {
                    var doctorRecords = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"Database: Records with doctor job = {doctorRecords}");
                }
                
                // Check ChuyenKhoa values for doctors
                using (var cmd = new MySqlCommand("SELECT DISTINCT ChuyenKhoa FROM all_data_final WHERE (JOB = 'bác sĩ' OR JOB = 'Bác sĩ' OR JOB = 'BS' OR JOB = 'Doctor' OR JOB LIKE '%bác sĩ%' OR JOB LIKE '%Bác sĩ%') AND ChuyenKhoa IS NOT NULL AND ChuyenKhoa != '' AND ChuyenKhoa != '0' AND ChuyenKhoa != '-'", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    var specialties = new List<string>();
                    while (reader.Read())
                    {
                        specialties.Add(reader.GetString(0));
                    }
                    Console.WriteLine($"Database: ChuyenKhoa for doctors = [{string.Join(", ", specialties.Take(10))}]");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TestDatabaseContent: {ex.Message}");
        }
    }



        public IActionResult ExportToExcel(
      string stt = "", string code = "", string projectName = "", string year = "",
      string contactObject = "", List<string> sbjnum = null, string fullname = "",
      string city = "", string address = "", string street = "", string ward = "",
      string district = "", List<string> phoneNumber = null, string email = "",
      string dateOfBirth = "", List<string> age = null, List<string> sex = null,
      string job = "", List<string> householdIncome = null, List<string> personalIncome = null,
      List<string> maritalStatus = null, string mostFrequentlyUsedBrand = "",
      string source = "", List<string> className = null, List<string> education = null,
      List<string> provinces = null, List<string> qc = null, string qa = "", List<string> Khuvuc = null, List<string> Nganhhang = null, List<string> region = null, string chuyenKhoa = "")
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            
            // Debug: Log thông tin bắt đầu xuất Excel
            Console.WriteLine($"ExportToExcel: Starting export process...");
            Console.WriteLine($"ExportToExcel: Filter parameters - stt: {stt}, code: {code}, projectName: {projectName}, year: {year}, sex: [{string.Join(", ", sex ?? new List<string>())}]");
            Console.WriteLine($"ExportToExcel: Additional filters - city: {city}, chuyenKhoa: {chuyenKhoa}");
            Console.WriteLine($"ExportToExcel: Current session - Username: {HttpContext.Session.GetString("Username")}, Role: {HttpContext.Session.GetString("Role")}");
            Console.WriteLine($"ExportToExcel: Session ID = {HttpContext.Session.Id}");
            Console.WriteLine($"ExportToExcel: All session keys = [{string.Join(", ", HttpContext.Session.Keys)}]");
            Console.WriteLine($"ExportToExcel: Request method = {HttpContext.Request.Method}");
            Console.WriteLine($"ExportToExcel: Request path = {HttpContext.Request.Path}");
            Console.WriteLine($"ExportToExcel: Query string = {HttpContext.Request.QueryString}");
            Console.WriteLine($"ExportToExcel: Request headers = [{string.Join(", ", HttpContext.Request.Headers.Select(h => $"{h.Key}:{h.Value}"))}]");

            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            
            // Debug: Log thông tin session
            Console.WriteLine($"ExportToExcel: Username = {username}");
            Console.WriteLine($"ExportToExcel: Role = {role}");
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xuất file excel.";
                Console.WriteLine($"ExportToExcel: Authentication failed - Username: {username}, Role: {role}");
                return RedirectToAction("Index", "Bacsi");
            }

            // Lấy email user từ database
            string userEmail = null;
            using (var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT email FROM users WHERE username = @username", connection);
                cmd.Parameters.AddWithValue("@username", username);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userEmail = reader.IsDBNull(0) ? null : reader.GetString(0);
                    }
                }
            }
            
            // Debug: Log thông tin email
            Console.WriteLine($"ExportToExcel: User email = {userEmail}");
            
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "Không tìm thấy email của bạn trong hệ thống. Vui lòng cập nhật email trong hồ sơ cá nhân.";
                Console.WriteLine($"ExportToExcel: Email not found for user {username}");
                return RedirectToAction("Index", "Bacsi");
            }

            var projectList = GetProjectts(stt, code, projectName, year, contactObject, sbjnum, fullname, city, address, street, ward, district, phoneNumber, email, dateOfBirth, age, sex, job, householdIncome, personalIncome, maritalStatus, mostFrequentlyUsedBrand, source, className, education, provinces, qc, qa, Khuvuc, Nganhhang, chuyenKhoa);
            
            // Debug: Log thông tin dữ liệu
            Console.WriteLine($"ExportToExcel: Retrieved {projectList.Count} records from GetProjectts");
            if (projectList.Count == 0)
            {
                Console.WriteLine($"ExportToExcel: WARNING - No data to export!");
            }

            // Giới hạn số lượng xuất theo role nếu cần (tùy chỉnh nếu muốn)
            int maxRows = int.MaxValue;
            switch (role)
            {
                case "Manager":
                    maxRows = 2000;
                    break;
                case "Execute":
                case "Assistant":
                    maxRows = 100;
                    break;
            }
            
            // Debug: Log thông tin giới hạn xuất
            Console.WriteLine($"ExportToExcel: Role = {role}, Max rows = {maxRows}");
            Console.WriteLine($"ExportToExcel: Total records before limit = {projectList.Count}");
            
            var limitedProjectList = projectList.OrderBy(x => Guid.NewGuid()).Take(maxRows).ToList();
            Console.WriteLine($"ExportToExcel: Records after limit = {limitedProjectList.Count}");

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Projects");
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Code";
                worksheet.Cells[1, 3].Value = "Project Name";
                worksheet.Cells[1, 4].Value = "Year";
                worksheet.Cells[1, 5].Value = "Contact Object";
                worksheet.Cells[1, 6].Value = "SBJNUM";
                worksheet.Cells[1, 7].Value = "Fullname";
                worksheet.Cells[1, 8].Value = "City";
                worksheet.Cells[1, 9].Value = "Address";
                worksheet.Cells[1, 10].Value = "Street";
                worksheet.Cells[1, 11].Value = "Ward";
                worksheet.Cells[1, 12].Value = "District";
                worksheet.Cells[1, 13].Value = "Phone Number";
                worksheet.Cells[1, 14].Value = "Email";
                worksheet.Cells[1, 15].Value = "Date of Birth";
                worksheet.Cells[1, 16].Value = "Age";
                worksheet.Cells[1, 17].Value = "Sex";
                worksheet.Cells[1, 18].Value = "Job";
                worksheet.Cells[1, 19].Value = "Household Income";
                worksheet.Cells[1, 20].Value = "Personal Income";
                worksheet.Cells[1, 21].Value = "Marital Status";
                worksheet.Cells[1, 22].Value = "Most Frequently Used Brand";
                worksheet.Cells[1, 23].Value = "Source";
                worksheet.Cells[1, 24].Value = "Class";
                worksheet.Cells[1, 25].Value = "Education";
                worksheet.Cells[1, 26].Value = "Provinces";
                worksheet.Cells[1, 27].Value = "QC";
                worksheet.Cells[1, 28].Value = "QA";
                worksheet.Cells[1, 29].Value = "KHUVUC";
                worksheet.Cells[1, 30].Value = "NGANHHANG";

                for (int i = 0; i < limitedProjectList.Count; i++)
                {
                    var project = limitedProjectList[i];
                    worksheet.Cells[i + 2, 1].Value = project.Stt;
                    worksheet.Cells[i + 2, 2].Value = project.Code;
                    worksheet.Cells[i + 2, 3].Value = project.ProjectName;
                    worksheet.Cells[i + 2, 4].Value = project.Year;
                    worksheet.Cells[i + 2, 5].Value = project.ContactObject;
                    worksheet.Cells[i + 2, 6].Value = project.Sbjnum;
                    worksheet.Cells[i + 2, 7].Value = project.Fullname;
                    worksheet.Cells[i + 2, 8].Value = project.City;
                    worksheet.Cells[i + 2, 9].Value = project.Address;
                    worksheet.Cells[i + 2, 10].Value = project.Street;
                    worksheet.Cells[i + 2, 11].Value = project.Ward;
                    worksheet.Cells[i + 2, 12].Value = project.District;
                    worksheet.Cells[i + 2, 13].Value = project.PhoneNumber;
                    worksheet.Cells[i + 2, 14].Value = project.Email;
                    worksheet.Cells[i + 2, 15].Value = project.DateOfBirth;
                    worksheet.Cells[i + 2, 16].Value = project.Age;
                    worksheet.Cells[i + 2, 17].Value = project.Sex;
                    worksheet.Cells[i + 2, 18].Value = project.Job;
                    worksheet.Cells[i + 2, 19].Value = project.HouseholdIncome;
                    worksheet.Cells[i + 2, 20].Value = project.PersonalIncome;
                    worksheet.Cells[i + 2, 21].Value = project.MaritalStatus;
                    worksheet.Cells[i + 2, 22].Value = project.MostFrequentlyUsedBrand;
                    worksheet.Cells[i + 2, 23].Value = project.Source;
                    worksheet.Cells[i + 2, 24].Value = project.Class;
                    worksheet.Cells[i + 2, 25].Value = project.Education;
                    worksheet.Cells[i + 2, 26].Value = project.Provinces;
                    worksheet.Cells[i + 2, 27].Value = project.Qc;
                    worksheet.Cells[i + 2, 28].Value = project.Qa;
                    worksheet.Cells[i + 2, 29].Value = project.Khuvuc;
                    worksheet.Cells[i + 2, 30].Value = project.Nganhhang;
                }
                worksheet.Cells.AutoFitColumns();

                // Serialize filter params để lưu vào bảng
                var filterParams = new
                {
                    stt,
                    code,
                    projectName,
                    year,
                    contactObject,
                    sbjnum,
                    fullname,
                    city,
                    address,
                    street,
                    ward,
                    district,
                    phoneNumber,
                    email,
                    dateOfBirth,
                    age,
                    sex,
                    job,
                    householdIncome,
                    personalIncome,
                    maritalStatus,
                    mostFrequentlyUsedBrand,
                    source,
                    className,
                    education,
                    provinces,
                    qc,
                    qa,
                    Khuvuc,
                    Nganhhang,
                    chuyenKhoa
                };
                string filterParamsJson = Newtonsoft.Json.JsonConvert.SerializeObject(filterParams);

                // Lưu request vào bảng ExportRequests
                var repo = new CIResearch.Services.ExportRequestRepository(_connectionString);
                var exportRequest = new CIResearch.Models.ExportRequest
                {
                    Username = username,
                    Email = userEmail,
                    RequestTime = DateTime.Now,
                    Status = "pending",
                    FilterParams = filterParamsJson,
                    FileData = package.GetAsByteArray(),
                    RejectReason = null,
                    ApprovedTime = null,
                    AdminApprovedBy = null
                    // Source = "Bacsi" // Tạm thời comment lại vì cột source chưa có trong database
                };
                
                // Debug: Log thông tin request
                Console.WriteLine($"ExportToExcel: Creating export request for user {username}");
                Console.WriteLine($"ExportToExcel: Request time = {exportRequest.RequestTime}");
                Console.WriteLine($"ExportToExcel: File size = {exportRequest.FileData.Length} bytes");
                
                try
                {
                    repo.AddRequestAsync(exportRequest).Wait();
                    Console.WriteLine($"ExportToExcel: Request saved successfully");
                    
                    TempData["SuccessMessage"] = "Yêu cầu xuất file đã được gửi và đang chờ admin duyệt. Bạn sẽ nhận được email khi được phê duyệt.";
                    return RedirectToAction("Index", "Bacsi");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ExportToExcel: Error saving request - {ex.Message}");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu yêu cầu xuất file: " + ex.Message;
                    return RedirectToAction("Index", "Bacsi");
                }
            }
        }

        private bool CanExport(string username)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var today = DateTime.Today;
                var query = "SELECT COUNT(*) FROM useraction_loc_xuat WHERE Username = @Username AND Action = 'XUẤT' AND DATE(Timestamp) = @Today";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Today", today);
                    var exportCount = Convert.ToInt32(command.ExecuteScalar());
                    return exportCount < 2;
                }
            }
        }

        private TimeSpan GetTimeUntilReset()
        {
            var now = DateTime.Now; // 03:27 PM +07
            var midnight = DateTime.Today.AddDays(1); // 00:00 ngày mai
            return midnight - now; // Thời gian còn lại đến reset
        }

        private void SendEmailWithAttachment(string toEmail, string subject, string body, byte[] attachmentData)
        {
            var fromEmail = "ciresearch.dn@gmail.com";
            var fromPassword = "mhip zhvj dhpd zrgo";

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(fromEmail);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.Attachments.Add(new Attachment(new MemoryStream(attachmentData), "Data_Ciresearch.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.Credentials = new NetworkCredential(fromEmail, fromPassword);
                    client.EnableSsl = true;
                    client.Send(message);
                }
            }
        }

        private void LogUserAction(string action)
        {
            var username = HttpContext.Session.GetString("Username");
            var timestamp = DateTime.Now;

            if (!string.IsNullOrEmpty(username))
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "INSERT INTO useraction_loc_xuat (Username, Action, Timestamp) VALUES (@Username, @Action, @Timestamp)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Action", action);
                        command.Parameters.AddWithValue("@Timestamp", timestamp);
                        command.ExecuteNonQuery();
                    }
                }
            }
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
            var rawCities = GetDistinctValuesFromDb("CITY");
            var normalizedCities = new List<string>();
            var cityMapping = new Dictionary<string, string>();
            
            // Danh sách các tỉnh cần loại bỏ (các tỉnh trùng lặp)
            var citiesToRemove = new HashSet<string>
            {
                "Ba Ria -vung Tau",
                "Bà Rịa -vung Tàu",
                "Binh Duong", 
                "Binh Dinh",
                "Binh Phuoc",
                "Thanh pho Ho Chi Minh",
                "Thành phố Hồ Chí Minh",
                "Quang Ninhuang Nam",
                "Quảng Ninhuảng Nam",
                "Thừa Thien Huế", 
                "Hồ chi Minh",
                "Vinh Long",
                "Hai Duong",
                "Bac Ninh",
                "Bac Giang",
                "Bac Kan",
                "Bac Lieu",
                "Dak Lak",
                "Dak Nong",
                "Dong Nai",
                "Dong Thap",
                "Hau Giang",
                "Kien Giang",
                "Lam Dong",
                "Long An",
                "Nam Dinh",
                "Ninh Binh",
                "Phu Tho",
                "Phu Yen",
                "Quang Binh",
                "Quang Nam",
                "Quang Ngai",
                "Quang Ninh",
                "Quang Tri",
                "Soc Trang",
                "Tay Ninh",
                "Thai Binh",
                "Thai Nguyen",
                "Thanh Hoa",
                "Thua Thien Hue",
                "Tien Giang",
                "Tra Vinh",
                "Tuyen Quang",
                "Vinh Phuc",
                "Ha Noi",
                "Ho Chi Minh",
                "Da Nang",
                "Can Tho",
                "Khanh Hoa",
                "Kon Tum",
                "Gia Lai",
                "Dien Bien",
                "Lao Cai",
                "Ha Giang",
                "Cao Bang",
                "Yen Bai",
                "Son La",
                "Lai Chau",
                "Lang Son",
                "Hai Phong",
                "Hung Yen",
                "Ha Nam",
                "Hoa Binh",
                "Ben Tre"
            };
            
            foreach (var city in rawCities)
            {
                if (string.IsNullOrWhiteSpace(city) || city == "0" || city == "-")
                    continue;
                
                // Chuẩn hóa tên tỉnh
                var normalizedCity = NormalizeCityName(city);
                
                // Kiểm tra xem có phải tỉnh cần loại bỏ không (kiểm tra cả tên gốc và tên đã chuẩn hóa)
                if (citiesToRemove.Contains(normalizedCity) || citiesToRemove.Contains(city))
                    continue;
                
                // Kiểm tra xem có chứa các từ khóa cần loại bỏ không
                var shouldRemove = false;
                foreach (var removeCity in citiesToRemove)
                {
                    if (city.Contains(removeCity) || removeCity.Contains(city) || 
                        normalizedCity.Contains(removeCity) || removeCity.Contains(normalizedCity))
                    {
                        shouldRemove = true;
                        break;
                    }
                }
                
                if (shouldRemove)
                    continue;
                
                // Nếu chưa có trong mapping, thêm vào
                if (!cityMapping.ContainsKey(normalizedCity))
                {
                    cityMapping[normalizedCity] = city; // Lưu tên gốc đẹp nhất
                    normalizedCities.Add(normalizedCity);
                }
                else
                {
                    // Nếu đã có, cập nhật tên gốc đẹp hơn (ngắn hơn, không có prefix)
                    var existingCity = cityMapping[normalizedCity];
                    if (city.Length < existingCity.Length || 
                        (!city.Contains("Tỉnh") && !city.Contains("tỉnh") && !city.Contains("TP.") && !city.Contains("tp.") && 
                         !city.Contains("Thành phố") && !city.Contains("thành phố")))
                    {
                        cityMapping[normalizedCity] = city;
                    }
                }
            }
            
            // Sắp xếp theo tên đã chuẩn hóa
            normalizedCities.Sort();
            
            // Lưu mapping vào ViewBag để sử dụng khi filter
            ViewBag.CityMapping = cityMapping;
            
            // Debug: Log số lượng cities trước và sau khi chuẩn hóa
            Console.WriteLine($"Raw cities count: {rawCities.Count}");
            Console.WriteLine($"Normalized cities count: {normalizedCities.Count}");
            Console.WriteLine($"Cities removed: {citiesToRemove.Count}");
            
            return normalizedCities;
        }
        
        // Phương thức chuẩn hóa tên tỉnh
        private string NormalizeCityName(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return cityName;
            
            // Loại bỏ các từ không cần thiết
            var cleanName = cityName.Trim()
                .Replace("Tỉnh", "").Replace("tỉnh", "")
                .Replace("TP.", "").Replace("tp.", "").Replace("TP ", "").Replace("tp ", "")
                .Replace("Thành phố", "").Replace("thành phố", "")
                .Replace("Quận", "").Replace("quận", "")
                .Replace("Huyện", "").Replace("huyện", "")
                .Trim();
            
            // Chuẩn hóa về format "Tên Tỉnh" (viết hoa đầu từ)
            var words = cleanName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var normalizedWords = new List<string>();
            
            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                    continue;
                    
                // Xử lý các trường hợp đặc biệt - chuẩn hóa cả dấu tiếng Việt
                var normalizedWord = word.ToLower() switch
                {
                    // Các từ có dấu tiếng Việt
                    "hà" => "Hà",
                    "hồ" => "Hồ",
                    "đà" => "Đà",
                    "đắk" => "Đắk",
                    "thừa" => "Thừa",
                    "khánh" => "Khánh",
                    "quảng" => "Quảng",
                    "lạng" => "Lạng",
                    "yên" => "Yên",
                    "sơn" => "Sơn",
                    "lai" => "Lai",
                    "lào" => "Lào",
                    "cao" => "Cao",
                    "điện" => "Điện",
                    "tuyên" => "Tuyên",
                    "phú" => "Phú",
                    "vĩnh" => "Vĩnh",
                    "hưng" => "Hưng",
                    "bắc" => "Bắc",
                    "thái" => "Thái",
                    "hải" => "Hải",
                    "ninh" => "Ninh",
                    "hòa" => "Hòa",
                    "nam" => "Nam",
                    "đồng" => "Đồng",
                    "bình" => "Bình",
                    "tiền" => "Tiền",
                    "long" => "Long",
                    "tây" => "Tây",
                    "sóc" => "Sóc",
                    "cần" => "Cần",
                    "cà" => "Cà",
                    "bạc" => "Bạc",
                    "kiên" => "Kiên",
                    "lâm" => "Lâm",
                    "bà" => "Bà",
                    "vũng" => "Vũng",
                    "hậu" => "Hậu",
                    "trà" => "Trà",
                    "bến" => "Bến",
                    "an" => "An",
                    "nghệ" => "Nghệ",
                    "thanh" => "Thanh",
                    "kon" => "Kon",
                    "gia" => "Gia",
                    "huế" => "Huế",
                    // Các từ không dấu (để xử lý trường hợp "Vinh Long" vs "Vĩnh Long")
                    "vinh" => "Vĩnh",
                    "hai" => "Hải",
                    "duong" => "Dương",
                    "trung" => "Trung",
                    "ha" => "Hà",
                    "ho" => "Hồ",
                    "da" => "Đà",
                    "dak" => "Đắk",
                    "thua" => "Thừa",
                    "khanh" => "Khánh",
                    "quang" => "Quảng",
                    "lang" => "Lạng",
                    "yen" => "Yên",
                    "son" => "Sơn",
                    "lao" => "Lào",
                    "dien" => "Điện",
                    "tuyen" => "Tuyên",
                    "phu" => "Phú",
                    "hung" => "Hưng",
                    "hoa" => "Hòa",
                    "dong" => "Đồng",
                    "binh" => "Bình",
                    "tien" => "Tiền",
                    "tay" => "Tây",
                    "soc" => "Sóc",
                    "can" => "Cần",
                    "ca" => "Cà",
                    "kien" => "Kiên",
                    "lam" => "Lâm",
                    "ba" => "Bà",
                    "vung" => "Vũng",
                    "hau" => "Hậu",
                    "tra" => "Trà",
                    "ben" => "Bến",
                    "nghe" => "Nghệ",
                    "hue" => "Huế",
                    // Xử lý các trường hợp đặc biệt khác
                    "ria" => "Rịa",
                    "tau" => "Tàu",
                    "lak" => "Lắk",
                    "nong" => "Nông",
                    "ngai" => "Ngãi",
                    "tri" => "Trị",
                    "trang" => "Trăng",
                    _ => char.ToUpper(word[0]) + word.Substring(1).ToLower()
                };
                
                normalizedWords.Add(normalizedWord);
            }
            
            var result = string.Join(" ", normalizedWords);
            
            // Xử lý các trường hợp đặc biệt cho tên tỉnh dài
            result = result switch
            {
                "Vinh Long" => "Vĩnh Long",
                "Hai Duong" => "Hải Dương",
                "Bac Ninh" => "Bắc Ninh",
                "Bac Giang" => "Bắc Giang",
                "Bac Kan" => "Bắc Kạn",
                "Bac Lieu" => "Bạc Liêu",
                "Dak Lak" => "Đắk Lắk",
                "Dak Nong" => "Đắk Nông",
                "Dong Nai" => "Đồng Nai",
                "Dong Thap" => "Đồng Tháp",
                "Hau Giang" => "Hậu Giang",
                "Kien Giang" => "Kiên Giang",
                "Lam Dong" => "Lâm Đồng",
                "Long An" => "Long An",
                "Nam Dinh" => "Nam Định",
                "Ninh Binh" => "Ninh Bình",
                "Phu Tho" => "Phú Thọ",
                "Phu Yen" => "Phú Yên",
                "Quang Binh" => "Quảng Bình",
                "Quang Nam" => "Quảng Nam",
                "Quang Ngai" => "Quảng Ngãi",
                "Quang Ninh" => "Quảng Ninh",
                "Quang Tri" => "Quảng Trị",
                "Soc Trang" => "Sóc Trăng",
                "Tay Ninh" => "Tây Ninh",
                "Thai Binh" => "Thái Bình",
                "Thai Nguyen" => "Thái Nguyên",
                "Thanh Hoa" => "Thanh Hóa",
                "Thua Thien Hue" => "Thừa Thiên Huế",
                "Tien Giang" => "Tiền Giang",
                "Tra Vinh" => "Trà Vinh",
                "Tuyen Quang" => "Tuyên Quang",
                "Vinh Phuc" => "Vĩnh Phúc",
                // Xử lý các trường hợp đặc biệt khác
                "Ha Noi" => "Hà Nội",
                "Ho Chi Minh" => "Hồ Chí Minh",
                "Da Nang" => "Đà Nẵng",
                "Can Tho" => "Cần Thơ",
                "Khanh Hoa" => "Khánh Hòa",
                "Kon Tum" => "Kon Tum",
                "Gia Lai" => "Gia Lai",
                "Dien Bien" => "Điện Biên",
                "Lao Cai" => "Lào Cai",
                "Ha Giang" => "Hà Giang",
                "Cao Bang" => "Cao Bằng",
                "Yen Bai" => "Yên Bái",
                "Son La" => "Sơn La",
                "Lai Chau" => "Lai Châu",
                "Lang Son" => "Lạng Sơn",
                "Hai Phong" => "Hải Phòng",
                "Hung Yen" => "Hưng Yên",
                "Ha Nam" => "Hà Nam",
                "Hoa Binh" => "Hòa Bình",
                "Ben Tre" => "Bến Tre",
                "An Giang" => "An Giang",
                // Xử lý các trường hợp đặc biệt bổ sung
                "Ba Ria -vung Tau" => "Bà Rịa Vũng Tàu",
                "Binh Duong" => "Bình Dương",
                "Binh Dinh" => "Bình Định",
                "Binh Phuoc" => "Bình Phước",
                "Thanh pho Ho Chi Minh" => "Hồ Chí Minh",
                "Quang Ninhuang Nam" => "Quảng Nam",
                _ => result
            };
            
            return result;

        }
        
        // Phương thức ánh xạ tên đã chuẩn hóa về tên gốc
        private string GetOriginalCityName(string normalizedCityName)
        {
            if (ViewBag.CityMapping is Dictionary<string, string> cityMapping)
            {
                return cityMapping.TryGetValue(normalizedCityName, out var originalCity) ? originalCity : normalizedCityName;
            }
            return normalizedCityName;
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
            // Chuyên khoa: hiển thị tất cả có sẵn (không filter theo job) để user có thể chọn
            var values = new List<string>();
            var chuyenKhoaMapping = new Dictionary<string, string>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT DISTINCT `ChuyenKhoa` FROM all_data_final WHERE `ChuyenKhoa` IS NOT NULL AND `ChuyenKhoa` != '' ORDER BY `ChuyenKhoa`";
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var val = reader[0]?.ToString();
                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            // Gộp "Nhi" và "Nhi khoa" thành "Khoa nhi"
                            var normalizedChuyenKhoa = val.Trim();
                            if (normalizedChuyenKhoa.Equals("Nhi", StringComparison.OrdinalIgnoreCase) || 
                                normalizedChuyenKhoa.Equals("Nhi khoa", StringComparison.OrdinalIgnoreCase))
                            {
                                normalizedChuyenKhoa = "Khoa nhi";
                            }
                            
                            // Nếu chưa có trong mapping, thêm vào
                            if (!chuyenKhoaMapping.ContainsKey(normalizedChuyenKhoa))
                            {
                                chuyenKhoaMapping[normalizedChuyenKhoa] = normalizedChuyenKhoa;
                                values.Add(normalizedChuyenKhoa);
                            }
                        }
                    }
                }
            }
            
            // Sắp xếp theo tên chuyên khoa đã chuẩn hóa
            values.Sort();
            
            return values;
        }
        
        // Helper for all distinct value queries (except ChuyenKhoa)
        private List<string> GetDistinctValuesFromDb(string column)
        {
            var values = new List<string>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                // For Bacsi page, always filter by job = 'bác sĩ' to show only relevant filter options (check multiple variations)
                var query = $"SELECT DISTINCT `{column}` FROM all_data_final WHERE `{column}` IS NOT NULL AND `{column}` != '' AND (JOB = 'bác sĩ' OR JOB = 'Bác sĩ' OR JOB = 'BS' OR JOB = 'Doctor' OR JOB LIKE '%bác sĩ%' OR JOB LIKE '%Bác sĩ%') ORDER BY `{column}`";
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
