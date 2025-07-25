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



        private string _connectionString = "Server=localhost;Database=sakila;User=root;Password=1234;DefaultCommandTimeout=1000;ConnectionTimeout=1000;";
        private readonly IMemoryCache _cache;

        public BacsiController(IMemoryCache cache)
        {
            _cache = cache;
        }


        public IActionResult Index(
             string stt = "", List<string> code = null, List<string> projectName = null, List<string> year = null,
      string contactObject = "", List<string> sbjnum = null, string fullname = "",
      List<string> city = null, string address = "", string street = "", string ward = "",
      string district = "", List<string> phoneNumber = null, string email = "",
      string dateOfBirth = "", List<string> age = null, List<string> sex = null,
      List<string> job = null, List<string> householdIncome = null, List<string> personalIncome = null,
      List<string> maritalStatus = null, string mostFrequentlyUsedBrand = "",
      string source = "", List<string> Classname = null, List<string> education = null,
      List<string> provinces = null, List<string> qc = null, string qa = "", List<string> Khuvuc = null, List<string> Nganhhang = null, List<string> region = null, List<string> chuyenKhoa = null)
        {
            try
            {
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


                ViewBag.Education = education;
                ViewBag.Year = year;
                ViewBag.Projectname = projectName;
                ViewBag.City = city;
                ViewBag.Sex = sex;
                ViewBag.Age = age;
                ViewBag.Region = region;
                ViewBag.Nganhhang = Nganhhang;
                ViewBag.Classname = Classname;
                ViewBag.Job = job;
                ViewBag.MaritalStatus = maritalStatus;
                ViewBag.Code = code;
                ViewBag.Sbjnum = sbjnum != null ? string.Join(",", sbjnum) : "";
                ViewBag.Phonenumber = phoneNumber != null ? string.Join(",", phoneNumber) : "";
                ViewBag.Qc = qc;
                ViewBag.ChuyenKhoa = chuyenKhoa;




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
                if (string.IsNullOrEmpty(stt) &&
                    (code == null || !code.Any()) &&
                    (projectName == null || !projectName.Any()) &&
                    (year == null || !year.Any()) &&
                    (contactObject == null || !contactObject.Any()) &&
                    (string.IsNullOrEmpty(fullname)) &&
                    (city == null || !city.Any()) &&
                    (string.IsNullOrEmpty(address)) &&
                    (string.IsNullOrEmpty(street)) &&
                    (string.IsNullOrEmpty(ward)) &&
                    (string.IsNullOrEmpty(district)) &&
                    (string.IsNullOrEmpty(email)) &&
                    (string.IsNullOrEmpty(dateOfBirth)) &&
                    (age == null || !age.Any()) &&
                    (sex == null || !sex.Any()) &&
                    (job == null || !job.Any()) &&
                    (householdIncome == null || !householdIncome.Any()) &&
                    (personalIncome == null || !personalIncome.Any()) &&
                    (maritalStatus == null || !maritalStatus.Any()) &&
                    (string.IsNullOrEmpty(mostFrequentlyUsedBrand)) &&
                    (string.IsNullOrEmpty(source)) &&
                    (Classname == null || !Classname.Any()) &&
                    (education == null || !education.Any()) &&
                    (provinces == null || !provinces.Any()) &&
                    (chuyenKhoa == null || !chuyenKhoa.Any()) &&
                    (qc == null || !qc.Any()) && (string.IsNullOrEmpty(qa)) &&
                    (Nganhhang == null || !Nganhhang.Any()) &&
                    sbjnum == null &&
                    phoneNumber == null)
                {
                    ViewBag.TotalSamples = 0;
                    return View(projectt);
                }

                projectt = GetProjectts(stt, code, projectName, year, contactObject, sbjnum, fullname, city, address, street, ward, district, phoneNumber, email, dateOfBirth, age, sex, job, householdIncome, personalIncome, maritalStatus, mostFrequentlyUsedBrand, source, Classname, education, provinces, qc, qa, Khuvuc, Nganhhang, chuyenKhoa);







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
                            _ => p.Sex // Giữ nguyên nếu không phải male hoặc female
                        }
                    })
                    .GroupBy(p => p.Sex)
                    .Select(g => new { Sex = g.Key, Count = g.Count() })
                    .Where(g => g.Sex != "0")  // Lọc các giới tính có số lượng lớn hơn 0
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



                ViewBag.TotalSamples = projectt.Count;
                //biểu đồ cột

                // Dữ liệu cho biểu đồ cột (số lượng mẫu theo tỉnh done)
                var provinceData = projectt.Where(p => p.City != "0")
                    .GroupBy(p => p.City) // Nhóm theo tỉnh
                                           .Select(g => new { City = g.Key, Count = g.Count() }) // Đếm số lượng mẫu
                                           .ToList();

                ViewBag.BarLabels = provinceData.Select(g => g.City).ToArray(); // Nhãn cho biểu đồ cột
                ViewBag.BarData = provinceData.Select(g => g.Count).ToArray(); // Dữ liệu cho biểu đồ cột


                var maleCount = projectt.Count(p => p.Sex == "Nam");
                var femaleCount = projectt.Count(p => p.Sex == "Nữ");
                ViewBag.GenderSummary = $"Các thông tin đã được lọc có:  {maleCount} nam, {femaleCount} nữ";

                var projectNames = projectt.Select(p => p.ProjectName).Distinct().ToList(); // Lấy tên dự án duy nhất
                ViewBag.Cacduanduocloc = "Tất cả dự án đã được lọc: " + string.Join(", ", projectNames);
                //

                var totalProjects = projectt.Count();
                ViewBag.TotalProjects = $"Tổng số dự án đã lọc: {totalProjects}";


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

                // Đếm số lượng mẫu ở từng miền
                var northernCount = projectt.Count(p => northernProvinces.Contains(p.City));
                var centralCount = projectt.Count(p => centralProvinces.Contains(p.City));
                var southernCount = projectt.Count(p => southernProvinces.Contains(p.City));

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



        private List<ALLDATA> GetProjectts(
           string stt, List<string> code, List<string> projectName, List<string> year,
  string contactObject, List<string> sbjnum, string fullname,
  List<string> city, string address, string street, string ward,
  string district, List<string> phoneNumber, string email,
  string dateOfBirth, List<string> age, List<string> sex,
  List<string> job, List<string> householdIncome, List<string> personalIncome,
  List<string> maritalStatus, string mostFrequentlyUsedBrand,
  string source, List<string> className, List<string> education,
  List<string> provinces, List<string> qc, string qa, List<string> Khuvuc, List<string> Nganhhang, List<string> chuyenKhoa)
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



        public IActionResult ExportToExcel(
      string stt = "", List<string> code = null, List<string> projectName = null, List<string> year = null,
      string contactObject = "", List<string> sbjnum = null, string fullname = "",
      List<string> city = null, string address = "", string street = "", string ward = "",
      string district = "", List<string> phoneNumber = null, string email = "",
      string dateOfBirth = "", List<string> age = null, List<string> sex = null,
      List<string> job = null, List<string> householdIncome = null, List<string> personalIncome = null,
      List<string> maritalStatus = null, string mostFrequentlyUsedBrand = "",
      string source = "", List<string> className = null, List<string> education = null,
      List<string> provinces = null, List<string> qc = null, string qa = "", List<string> Khuvuc = null, List<string> Nganhhang = null, List<string> chuyenKhoa = null)
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xuất file excel.";
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
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "Không tìm thấy email của bạn trong hệ thống. Vui lòng cập nhật email trong hồ sơ cá nhân.";
                return RedirectToAction("Index", "Bacsi");
            }

            var projectList = GetProjectts(stt, code, projectName, year, contactObject, sbjnum, fullname, city, address, street, ward, district, phoneNumber, email, dateOfBirth, age, sex, job, householdIncome, personalIncome, maritalStatus, mostFrequentlyUsedBrand, source, className, education, provinces, qc, qa, Khuvuc, Nganhhang, chuyenKhoa);

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
            var limitedProjectList = projectList.OrderBy(x => Guid.NewGuid()).Take(maxRows).ToList();

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
                };
                repo.AddRequestAsync(exportRequest).Wait();

                TempData["SuccessMessage"] = "Yêu cầu xuất file đã được gửi và đang chờ admin duyệt. Bạn sẽ nhận được email khi được phê duyệt.";
                return RedirectToAction("Index", "Bacsi");
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
            var fromEmail = "huan220vn@gmail.com";
            var fromPassword = "tctn ztgb yqfd ynmp";

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
