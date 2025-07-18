using CIResearch.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Data.SqlClient;

using System.Globalization;

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
using Microsoft.EntityFrameworkCore;
namespace CIResearch.Controllers
{
    public class DansoController : Controller
    {
        private string _connectionString = "Server=localhost;Database=sakila;User=root;Password=1234";


        public ActionResult Index(string stt = "", List<string> code = null, List<string> projectName = null, List<string> year = null,
     string contactObject = "", List<string> sbjnum = null, string fullname = "",
     List<string> city = null, string address = "", string street = "", string ward = "",
     List<string> district = null, List<string> phoneNumber = null, string email = "",
     string dateOfBirth = "", List<string> age = null, List<string> sex = null,
     List<string> job = null, List<string> householdIncome = null, List<string> personalIncome = null,
     List<string> maritalStatus = null, string mostFrequentlyUsedBrand = "",
     string source = "", List<string> Classname = null, string education = "",
     List<string> provinces = null, string qc = "", string qa = "", List<string> Khuvuc = null, List<string> Nganhhang = null, List<string> region = null)
        {
            ViewBag.Year = year;
            ViewBag.Projectname = projectName;
            ViewBag.City = city;
            ViewBag.district = district;
            ViewBag.ward = ward;
            ViewBag.Age = age;

            bool isAnyFilterSelected = projectName?.Any() == true || year?.Any() == true || city?.Any() == true || age?.Any() == true ||
                                       sex?.Any() == true || provinces?.Any() == true || job?.Any() == true || sbjnum?.Any() == true ||
                                       phoneNumber?.Any() == true || maritalStatus?.Any() == true || code?.Any() == true ||
                                       Classname?.Any() == true || Nganhhang?.Any() == true;

            if (isAnyFilterSelected)
            {
                List<ALLDATA> adminChart = getadminChart(stt, code, projectName, year, contactObject, sbjnum, fullname, city, address, street, ward, district, phoneNumber, email, dateOfBirth, age, sex, job, householdIncome, personalIncome, maritalStatus, mostFrequentlyUsedBrand, source, Classname, education, provinces, qc, qa, Khuvuc, Nganhhang);

                // Tạo danh sách tỉnh từ dữ liệu lọc
                var provincesFromData = adminChart
                    .Select(a => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(a.Provinces.ToLower()))
                    .Distinct()
                    .ToList();
                ViewBag.Provinces = provincesFromData; // Gán danh sách tỉnh vào ViewBag

                ViewBag.Test = adminChart.Count;





                var provinceSampleCountsSlide = adminChart
                    .GroupBy(a => a.Provinces)
                    .Select(g => new
                    {
                        Province = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(g.Key.ToLower()),
                        SampleCount = g.Count()
                    })
                    .ToList();


                var allDistricts = new List<DistrictData>
{

                    //hà nội nhé
   new DistrictData
{
    City = "Hà Nội",
    District = "Quận Ba Đình",
    Population = 221893,
    AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Phúc Xá", Population = 21946 },
        new WardData { Name = "Phường Trúc Bạch", Population = 7065 },
        new WardData { Name = "Phường Vĩnh Phúc", Population = 23867 },
        new WardData { Name = "Phường Cống Vị", Population = 15979 },
        new WardData { Name = "Phường Liễu Giai", Population = 21084 },
        new WardData { Name = "Phường Nguyễn Trung Trực", Population = 7705 },
        new WardData { Name = "Phường Quán Thánh", Population = 7923 },
        new WardData { Name = "Phường Ngọc Hà", Population = 19357 },
        new WardData { Name = "Phường Điện Biên", Population = 8912 },
        new WardData { Name = "Phường Đội Cấn", Population = 14375 },
        new WardData { Name = "Phường Ngọc Khánh", Population = 20363 },
        new WardData { Name = "Phường Kim Mã", Population = 14259 },
        new WardData { Name = "Phường Giảng Võ", Population = 17665 },
        new WardData { Name = "Phường Thành Công", Population = 21393 }
    }
},

new DistrictData
{
    City = "Hà Nội",
    District = "Quận Hoàn Kiếm",
    Population = 135618,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Phúc Tân", Population = 16856 },
        new WardData { Name = "Phường Đồng Xuân", Population = 9399 },
        new WardData { Name = "Phường Hàng Mã", Population = 6971 },
        new WardData { Name = "Phường Hàng Buồm", Population = 5367 },
        new WardData { Name = "Phường Hàng Đào", Population = 5393 },
        new WardData { Name = "Phường Hàng Bồ", Population = 5243 },
        new WardData { Name = "Phường Cửa Đông", Population = 6170 },
        new WardData { Name = "Phường Lý Thái Tổ", Population = 5152 },
        new WardData { Name = "Phường Hàng Bạc", Population = 5194 },
        new WardData { Name = "Phường Hàng Gai", Population = 5787 },
        new WardData { Name = "Phường Chương Dương", Population = 22946 },
        new WardData { Name = "Phường Hàng Trống", Population = 5524 },
        new WardData { Name = "Phường Cửa Nam", Population = 6385 },
        new WardData { Name = "Phường Hàng Bông", Population = 7206 },
        new WardData { Name = "Phường Tràng Tiền", Population = 4656 },
        new WardData { Name = "Phường Trần Hưng Đạo", Population = 7039 },
        new WardData { Name = "Phường Phan Chu Trinh", Population = 4846 },
        new WardData { Name = "Phường Hàng Bài", Population = 5484 }
    }
},


new DistrictData
{
    City = "Hà Nội",
    District = "Quận Đống Đa",
    Population = 371606,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Cát Linh", Population = 11185 },
        new WardData { Name = "Phường Văn Miếu", Population = 9554 },
        new WardData { Name = "Phường Quốc Tử Giám", Population = 8087 },
        new WardData { Name = "Phường Láng Thượng", Population = 39173 },
        new WardData { Name = "Phường Ô Chợ Dừa", Population = 34039 },
        new WardData { Name = "Phường Văn Chương", Population = 16614 },
        new WardData { Name = "Phường Hàng Bột", Population = 18375 },
        new WardData { Name = "Phường Láng Hạ", Population = 29897 },
        new WardData { Name = "Phường Khâm Thiên", Population = 9595 },
        new WardData { Name = "Phường Thổ Quan", Population = 17398 },
        new WardData { Name = "Phường Nam Đồng", Population = 14721 },
        new WardData { Name = "Phường Trung Phụng", Population = 16607 },
        new WardData { Name = "Phường Quang Trung", Population = 14434 },
        new WardData { Name = "Phường Trung Liệt", Population = 26373 },
        new WardData { Name = "Phường Phương Liên", Population = 17022 },
        new WardData { Name = "Phường Thịnh Quang", Population = 18201 },
        new WardData { Name = "Phường Trung Tự", Population = 13083 },
        new WardData { Name = "Phường Kim Liên", Population = 14092 },
        new WardData { Name = "Phường Phương Mai", Population = 19934 },
        new WardData { Name = "Phường Ngã Tư Sở", Population = 7653 },
        new WardData { Name = "Phường Khương Thượng", Population = 15569 }
    }
},

new DistrictData
{
    City = "Hà Nội",
    District = "Quận Hai Bà Trưng",
    Population = 303586,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Nguyễn Du", Population = 5978 },
        new WardData { Name = "Phường Bạch Đằng", Population = 19795 },
        new WardData { Name = "Phường Phạm Đình Hổ", Population = 6812 },
        new WardData { Name = "Phường Bùi Thị Xuân", Population = 3826 },
        new WardData { Name = "Phường Ngô Thì Nhậm", Population = 5013 },
        new WardData { Name = "Phường Lê Đại Hành", Population = 8464 },
        new WardData { Name = "Phường Đồng Nhân", Population = 8690 },
        new WardData { Name = "Phường Phố Huế", Population = 8955 },
        new WardData { Name = "Phường Đống Mác", Population = 8647 },
        new WardData { Name = "Phường Thanh Lương", Population = 24812 },
        new WardData { Name = "Phường Thanh Nhàn", Population = 21170 },
        new WardData { Name = "Phường Cầu Dền", Population = 11797 },
        new WardData { Name = "Phường Bách Khoa", Population = 13950 },
        new WardData { Name = "Phường Đồng Tâm", Population = 19973 },
        new WardData { Name = "Phường Vĩnh Tuy", Population = 51644 },
        new WardData { Name = "Phường Bạch Mai", Population = 16241 },
        new WardData { Name = "Phường Quỳnh Mai", Population = 11946 },
        new WardData { Name = "Phường Quỳnh Lôi", Population = 14787 },
        new WardData { Name = "Phường Minh Khai", Population = 19822 },
        new WardData { Name = "Phường Trương Định", Population = 21264 }
    }
},

new DistrictData
{
    City = "Hà Nội",
    District = "Quận Thanh Xuân",
    Population = 293524,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Nhân Chính", Population = 51306 },
        new WardData { Name = "Phường Thượng Đình", Population = 29021 },
        new WardData { Name = "Phường Khương Trung", Population = 35687 },
        new WardData { Name = "Phường Khương Mai", Population = 21885 },
        new WardData { Name = "Phường Thanh Xuân Trung", Population = 31370 },
        new WardData { Name = "Phường Phương Liệt", Population = 24619 },
        new WardData { Name = "Phường Hạ Đình", Population = 18111 },
        new WardData { Name = "Phường Khương Đình", Population = 35286 },
        new WardData { Name = "Phường Thanh Xuân Bắc", Population = 20659 },
        new WardData { Name = "Phường Thanh Xuân Nam", Population = 12436 },
        new WardData { Name = "Phường Kim Giang", Population = 13144 }
    }
},




new DistrictData
{
    City = "Hà Nội",
    District = "Quận Cầu Giấy",
    Population = 292536,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Nghĩa Đô", Population = 34636 },
        new WardData { Name = "Phường Nghĩa Tân", Population = 21794 },
        new WardData { Name = "Phường Mai Dịch", Population = 40511 },
        new WardData { Name = "Phường Dịch Vọng", Population = 28226 },
        new WardData { Name = "Phường Dịch Vọng Hậu", Population = 31541 },
        new WardData { Name = "Phường Quan Hoa", Population = 34886 },
        new WardData { Name = "Phường Yên Hoà", Population = 46766 },
        new WardData { Name = "Phường Trung Hoà", Population = 54176 }
    }
},



new DistrictData
{
    City = "Hà Nội",
    District = "Quận Long Biên",
    Population = 322549,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Thượng Thanh", Population = 28936 },
        new WardData { Name = "Phường Ngọc Thụy", Population = 39565 },
        new WardData { Name = "Phường Giang Biên", Population = 24315 },
        new WardData { Name = "Phường Đức Giang", Population = 27694 },
        new WardData { Name = "Phường Việt Hưng", Population = 22375 },
        new WardData { Name = "Phường Gia Thụy", Population = 15076 },
        new WardData { Name = "Phường Ngọc Lâm", Population = 23923 },
        new WardData { Name = "Phường Phúc Lợi", Population = 19019 },
        new WardData { Name = "Phường Bồ Đề", Population = 31466 },
        new WardData { Name = "Phường Sài Đồng", Population = 17655 },
        new WardData { Name = "Phường Long Biên", Population = 22028 },
        new WardData { Name = "Phường Thạch Bàn", Population = 26427 },
        new WardData { Name = "Phường Phúc Đồng", Population = 14211 },
        new WardData { Name = "Phường Cự Khối", Population = 9859 }
    }
},


new DistrictData
{
    City = "Hà Nội",
    District = "Quận Hoàng Mai",
    Population = 506347,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Thanh Trì", Population = 25110 },
        new WardData { Name = "Phường Vĩnh Hưng", Population = 38652 },
        new WardData { Name = "Phường Định Công", Population = 48671 },
        new WardData { Name = "Phường Mai Động", Population = 48966 },
        new WardData { Name = "Phường Tương Mai", Population = 28344 },
        new WardData { Name = "Phường Đại Kim", Population = 48505 },
        new WardData { Name = "Phường Tân Mai", Population = 24523 },
        new WardData { Name = "Phường Hoàng Văn Thụ", Population = 39489 },
        new WardData { Name = "Phường Giáp Bát", Population = 16625 },
        new WardData { Name = "Phường Lĩnh Nam", Population = 30900 },
        new WardData { Name = "Phường Thịnh Liệt", Population = 40286 },
        new WardData { Name = "Phường Trần Phú", Population = 14152 },
        new WardData { Name = "Phường Hoàng Liệt", Population = 78845 },
        new WardData { Name = "Phường Yên Sở", Population = 23279 }
    }
},


new DistrictData
{
    City = "Hà Nội",
    District = "Quận Hà Đông",
    Population = 397854,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Nguyễn Trãi", Population = 12459 },
        new WardData { Name = "Phường Mộ Lao", Population = 34072 },
        new WardData { Name = "Phường Văn Quán", Population = 26389 },
        new WardData { Name = "Phường Vạn Phúc", Population = 18877 },
        new WardData { Name = "Phường Yết Kiêu", Population = 6081 },
        new WardData { Name = "Phường Quang Trung", Population = 17242 },
        new WardData { Name = "Phường La Khê", Population = 39405 },
        new WardData { Name = "Phường Phú La", Population = 24038 },
        new WardData { Name = "Phường Phúc La", Population = 32173 },
        new WardData { Name = "Phường Hà Cầu", Population = 22439 },
        new WardData { Name = "Phường Yên Nghĩa", Population = 30824 },
        new WardData { Name = "Phường Kiến Hưng", Population = 34375 },
        new WardData { Name = "Phường Phú Lãm", Population = 15608 },
        new WardData { Name = "Phường Phú Lương", Population = 26888 },
        new WardData { Name = "Phường Dương Nội", Population = 32265 },
        new WardData { Name = "Phường Đồng Mai", Population = 16367 },
        new WardData { Name = "Phường Biên Giang", Population = 8352 }
    }
},


new DistrictData
{
    City = "Hà Nội",
    District = "Quận Tây Hồ",
    Population = 160495,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Phú Thượng", Population = 26071 },
        new WardData { Name = "Phường Nhật Tân", Population = 15597 },
        new WardData { Name = "Phường Tứ Liên", Population = 18360 },
        new WardData { Name = "Phường Quảng An", Population = 10290 },
        new WardData { Name = "Phường Xuân La", Population = 27500 },
        new WardData { Name = "Phường Yên Phụ", Population = 23301 },
        new WardData { Name = "Phường Bưởi", Population = 24778 },
        new WardData { Name = "Phường Thụy Khuê", Population = 14598 }
    }
},


new DistrictData
{
    City = "Hà Nội",
    District = "Quận Nam Từ Liêm",
    Population = 264246,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Cầu Diễn", Population = 26089 },
        new WardData { Name = "Phường Xuân Phương", Population = 17095 },
        new WardData { Name = "Phường Phương Canh", Population = 18089 },
        new WardData { Name = "Phường Mỹ Đình 1", Population = 27147 },
        new WardData { Name = "Phường Mỹ Đình 2", Population = 32207 },
        new WardData { Name = "Phường Tây Mỗ", Population = 25497 },
        new WardData { Name = "Phường Mễ Trì", Population = 30589 },
        new WardData { Name = "Phường Phú Đô", Population = 15334 },
        new WardData { Name = "Phường Đại Mỗ", Population = 30906 },
        new WardData { Name = "Phường Trung Văn", Population = 41293 }
    }
},


new DistrictData
{
    City = "Hà Nội",
    District = "Quận Bắc Từ Liêm",
    Population = 335110,
        AreaType = "Nội thành", // <-- thêm dòng này

    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Thượng Cát", Population = 8074 },
        new WardData { Name = "Phường Liên Mạc", Population = 10529 },
        new WardData { Name = "Phường Đông Ngạc", Population = 24206 },
        new WardData { Name = "Phường Đức Thắng", Population = 19395 },
        new WardData { Name = "Phường Thụy Phương", Population = 13367 },
        new WardData { Name = "Phường Tây Tựu", Population = 20221 },
        new WardData { Name = "Phường Xuân Đỉnh", Population = 40345 },
        new WardData { Name = "Phường Xuân Tảo", Population = 18368 },
        new WardData { Name = "Phường Minh Khai", Population = 29760 },
        new WardData { Name = "Phường Cổ Nhuế 1", Population = 46104 },
        new WardData { Name = "Phường Cổ Nhuế 2", Population = 42510 },
        new WardData { Name = "Phường Phú Diễn", Population = 39512 },
        new WardData { Name = "Phường Phúc Diễn", Population = 22719 },
    }
},
new DistrictData
{
    City = "Hà Nội",
    District = "Huyện Chương Mỹ",
    Population = 337326,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Chúc Sơn", Population = 14177 },
        new WardData { Name = "Thị trấn Xuân Mai", Population = 22252 },
        new WardData { Name = "Xã Phụng Châu", Population = 12936 },
        new WardData { Name = "Xã Tiên Phương", Population = 16764 },
        new WardData { Name = "Xã Đông Sơn", Population = 10822 },
        new WardData { Name = "Xã Đông Phương Yên", Population = 11693 },
        new WardData { Name = "Xã Phú Nghĩa", Population = 11971 },
        new WardData { Name = "Xã Trường Yên", Population = 11954 },
        new WardData { Name = "Xã Ngọc Hòa", Population = 9156 },
        new WardData { Name = "Xã Thủy Xuân Tiên", Population = 18869 },
        new WardData { Name = "Xã Thanh Bình", Population = 7417 },
        new WardData { Name = "Xã Trung Hòa", Population = 11405 },
        new WardData { Name = "Xã Đại Yên", Population = 6523 },
        new WardData { Name = "Xã Thụy Hương", Population = 9614 },
        new WardData { Name = "Xã Tốt Động", Population = 14766 },
        new WardData { Name = "Xã Lam Điền", Population = 11695 },
        new WardData { Name = "Xã Tân Tiến", Population = 11385 },
        new WardData { Name = "Xã Nam Phương Tiến", Population = 10160 },
        new WardData { Name = "Xã Hợp Đồng", Population = 7095 },
        new WardData { Name = "Xã Hoàng Văn Thụ", Population = 12516 },
        new WardData { Name = "Xã Hoàng Diệu", Population = 10924 },
        new WardData { Name = "Xã Hữu Văn", Population = 9509 },
        new WardData { Name = "Xã Quảng Bị", Population = 12010 },
        new WardData { Name = "Xã Mỹ Lương", Population = 8347 },
        new WardData { Name = "Xã Thượng Vực", Population = 6803 },
        new WardData { Name = "Xã Hồng Phong", Population = 4588 },
        new WardData { Name = "Xã Đồng Phú", Population = 6163 },
        new WardData { Name = "Xã Trần Phú", Population = 9550 },
        new WardData { Name = "Xã Văn Võ", Population = 9036 },
        new WardData { Name = "Xã Đồng Lạc", Population = 5449 },
        new WardData { Name = "Xã Hòa Chính", Population = 7127 },
        new WardData { Name = "Xã Phú Nam An", Population = 4650 }
    }
},
new DistrictData
{
    City = "Hà Nội",
    District = "Huyện Thanh Oai",
    Population = 211029,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Kim Bài", Population = 7069 },
        new WardData { Name = "Xã Cự Khê", Population = 16508 },
        new WardData { Name = "Xã Bích Hòa", Population = 11511 },
        new WardData { Name = "Xã Mỹ Hưng", Population = 7126 },
        new WardData { Name = "Xã Cao Viên", Population = 19726 },
        new WardData { Name = "Xã Bình Minh", Population = 14435 },
        new WardData { Name = "Xã Tam Hưng", Population = 12051 },
        new WardData { Name = "Xã Thanh Cao", Population = 10681 },
        new WardData { Name = "Xã Thanh Thùy", Population = 8531 },
        new WardData { Name = "Xã Thanh Mai", Population = 11056 },
        new WardData { Name = "Xã Thanh Văn", Population = 6656 },
        new WardData { Name = "Xã Đỗ Động", Population = 6432 },
        new WardData { Name = "Xã Kim An", Population = 3597 },
        new WardData { Name = "Xã Kim Thư", Population = 5984 },
        new WardData { Name = "Xã Phương Trung", Population = 16424 },
        new WardData { Name = "Xã Tân Ước", Population = 7826 },
        new WardData { Name = "Xã Dân Hòa", Population = 8522 },
        new WardData { Name = "Xã Liên Châu", Population = 8008 },
        new WardData { Name = "Xã Cao Dương", Population = 10947 },
        new WardData { Name = "Xã Xuân Dương", Population = 5895 },
        new WardData { Name = "Xã Hồng Dương", Population = 12044 }
    }
},

new DistrictData
{
    City = "Hà Nội",
    District = "Huyện Thường Tín",
    Population = 254702,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Thường Tín", Population = 6178 },
        new WardData { Name = "Xã Ninh Sở", Population = 9956 },
        new WardData { Name = "Xã Nhị Khê", Population = 7750 },
        new WardData { Name = "Xã Duyên Thái", Population = 12037 },
        new WardData { Name = "Xã Khánh Hà", Population = 11925 },
        new WardData { Name = "Xã Hòa Bình", Population = 7254 },
        new WardData { Name = "Xã Văn Bình", Population = 11478 },
        new WardData { Name = "Xã Hiền Giang", Population = 4815 },
        new WardData { Name = "Xã Hồng Vân", Population = 6241 },
        new WardData { Name = "Xã Vân Tảo", Population = 11318 },
        new WardData { Name = "Xã Liên Phương", Population = 8588 },
        new WardData { Name = "Xã Văn Phú", Population = 8151 },
        new WardData { Name = "Xã Tự Nhiên", Population = 9629 },
        new WardData { Name = "Xã Tiền Phong", Population = 10264 },
        new WardData { Name = "Xã Hà Hồi", Population = 10618 },
        new WardData { Name = "Xã Thư Phú", Population = 6486 },
        new WardData { Name = "Xã Nguyễn Trãi", Population = 9739 },
        new WardData { Name = "Xã Quất Động", Population = 8432 },
        new WardData { Name = "Xã Chương Dương", Population = 5403 },
        new WardData { Name = "Xã Tân Minh", Population = 9255 },
        new WardData { Name = "Xã Lê Lợi", Population = 8125 },
        new WardData { Name = "Xã Thắng Lợi", Population = 9592 },
        new WardData { Name = "Xã Dũng Tiến", Population = 8568 },
        new WardData { Name = "Xã Thống Nhất", Population = 7238 },
        new WardData { Name = "Xã Nghiêm Xuyên", Population = 6125 },
        new WardData { Name = "Xã Tô Hiệu", Population = 12383 },
        new WardData { Name = "Xã Văn Tự", Population = 9502 },
        new WardData { Name = "Xã Vạn Điểm", Population = 8117 },
        new WardData { Name = "Xã Minh Cường", Population = 9535 }
    }
},
new DistrictData
{
    City = "Hà Nội",
    District = "Huyện Phú Xuyên",
    Population = 213984,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Phú Minh", Population = 5340 },
        new WardData { Name = "Thị trấn Phú Xuyên", Population = 11311 },
        new WardData { Name = "Xã Hồng Minh", Population = 8648 },
        new WardData { Name = "Xã Phượng Dực", Population = 9518 },
        new WardData { Name = "Xã Văn Nhân", Population = 6010 },
        new WardData { Name = "Xã Thụy Phú", Population = 2699 },
        new WardData { Name = "Xã Tri Trung", Population = 4423 },
        new WardData { Name = "Xã Đại Thắng", Population = 6543 },
        new WardData { Name = "Xã Phú Túc", Population = 9215 },
        new WardData { Name = "Xã Văn Hoàng", Population = 7329 },
        new WardData { Name = "Xã Hồng Thái", Population = 7767 },
        new WardData { Name = "Xã Hoàng Long", Population = 10425 },
        new WardData { Name = "Xã Quang Trung", Population = 4385 },
        new WardData { Name = "Xã Nam Phong", Population = 5386 },
        new WardData { Name = "Xã Nam Triều", Population = 6661 },
        new WardData { Name = "Xã Tân Dân", Population = 9615 },
        new WardData { Name = "Xã Sơn Hà", Population = 5676 },
        new WardData { Name = "Xã Chuyên Mỹ", Population = 10154 },
        new WardData { Name = "Xã Khai Thái", Population = 8265 },
        new WardData { Name = "Xã Phúc Tiến", Population = 9244 },
        new WardData { Name = "Xã Vân Từ", Population = 5556 },
        new WardData { Name = "Xã Tri Thủy", Population = 10040 },
        new WardData { Name = "Xã Đại Xuyên", Population = 8708 },
        new WardData { Name = "Xã Phú Yên", Population = 5586 },
        new WardData { Name = "Xã Bạch Hạ", Population = 6711 },
        new WardData { Name = "Xã Quang Lãng", Population = 5389 },
        new WardData { Name = "Xã Châu Can", Population = 10092 },
        new WardData { Name = "Xã Minh Tân", Population = 13288 }
    }
},

new DistrictData
{
    City = "Hà Nội",
    District = "Huyện Ứng Hòa",
    Population = 210869,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Vân Đình", Population = 14340 },
        new WardData { Name = "Xã Viên An", Population = 7388 },
        new WardData { Name = "Xã Viên Nội", Population = 4402 },
        new WardData { Name = "Xã Hoa Sơn", Population = 7443 },
        new WardData { Name = "Xã Quảng Phú Cầu", Population = 12443 },
        new WardData { Name = "Xã Trường Thịnh", Population = 7254 },
        new WardData { Name = "Xã Cao Thành", Population = 4081 },
        new WardData { Name = "Xã Liên Bạt", Population = 7138 },
        new WardData { Name = "Xã Sơn Công", Population = 6077 },
        new WardData { Name = "Xã Đồng Tiến", Population = 7732 },
        new WardData { Name = "Xã Phương Tú", Population = 13125 },
        new WardData { Name = "Xã Trung Tú", Population = 7775 },
        new WardData { Name = "Xã Đồng Tân", Population = 6138 },
        new WardData { Name = "Xã Tảo Dương Văn", Population = 7049 },
        new WardData { Name = "Xã Vạn Thái", Population = 10434 },
        new WardData { Name = "Xã Minh Đức", Population = 5671 },
        new WardData { Name = "Xã Hòa Lâm", Population = 6644 },
        new WardData { Name = "Xã Hòa Xá", Population = 4328 },
        new WardData { Name = "Xã Trầm Lộng", Population = 4733 },
        new WardData { Name = "Xã Kim Đường", Population = 7008 },
        new WardData { Name = "Xã Hòa Nam", Population = 10975 },
        new WardData { Name = "Xã Hòa Phú", Population = 7473 },
        new WardData { Name = "Xã Đội Bình", Population = 8313 },
        new WardData { Name = "Xã Đại Hùng", Population = 5099 },
        new WardData { Name = "Xã Đông Lỗ", Population = 5889 },
        new WardData { Name = "Xã Phù Lưu", Population = 5751 },
        new WardData { Name = "Xã Đại Cường", Population = 4742 },
        new WardData { Name = "Xã Lưu Hoàng", Population = 5125 },
        new WardData { Name = "Xã Hồng Quang", Population = 6299 }
    }
},



new DistrictData
{
    City = "Hà Nội",
    District = "Huyện Mỹ Đức",
    Population = 199901,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Đại Nghĩa", Population = 8015 },
        new WardData { Name = "Xã Đồng Tâm", Population = 9459 },
        new WardData { Name = "Xã Thượng Lâm", Population = 6337 },
        new WardData { Name = "Xã Tuy Lai", Population = 13658 },
        new WardData { Name = "Xã Phúc Lâm", Population = 8808 },
        new WardData { Name = "Xã Mỹ Thành", Population = 3763 },
        new WardData { Name = "Xã Bột Xuyên", Population = 8138 },
        new WardData { Name = "Xã An Mỹ", Population = 5976 },
        new WardData { Name = "Xã Hồng Sơn", Population = 7977 },
        new WardData { Name = "Xã Lê Thanh", Population = 12770 },
        new WardData { Name = "Xã Xuy Xá", Population = 9575 },
        new WardData { Name = "Xã Phùng Xá", Population = 8181 },
        new WardData { Name = "Xã Phù Lưu Tế", Population = 8132 },
        new WardData { Name = "Xã Đại Hưng", Population = 7486 },
        new WardData { Name = "Xã Vạn Kim", Population = 6804 },
        new WardData { Name = "Xã Đốc Tín", Population = 4515 },
        new WardData { Name = "Xã Hương Sơn", Population = 20806 },
        new WardData { Name = "Xã Hùng Tiến", Population = 7528 },
        new WardData { Name = "Xã An Tiến", Population = 6846 },
        new WardData { Name = "Xã Hợp Tiến", Population = 14071 },
        new WardData { Name = "Xã Hợp Thanh", Population = 12172 },
        new WardData { Name = "Xã An Phú", Population = 8884 }
    }
},




new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 1",
    Population = 142625,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Tân Định", Population = 23258 },
        new WardData { Name = "Phường Đa Kao", Population = 14970 },
        new WardData { Name = "Phường Bến Nghé", Population = 10633 },
        new WardData { Name = "Phường Bến Thành", Population = 11714 },
        new WardData { Name = "Phường Nguyễn Thái Bình", Population = 9716 },
        new WardData { Name = "Phường Phạm Ngũ Lão", Population = 15183 },
        new WardData { Name = "Phường Cầu Ông Lãnh", Population = 10737 },
        new WardData { Name = "Phường Cô Giang", Population = 11517 },
        new WardData { Name = "Phường Nguyễn Cư Trinh", Population = 21191 },
        new WardData { Name = "Phường Cầu Kho", Population = 13706 }
    }
},


new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 12",
    Population = 620146,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Thạnh Xuân", Population = 54220 },
        new WardData { Name = "Phường Thạnh Lộc", Population = 59163 },
        new WardData { Name = "Phường Hiệp Thành", Population = 99417 },
        new WardData { Name = "Phường Thới An", Population = 46893 },
        new WardData { Name = "Phường Tân Chánh Hiệp", Population = 78595 },
        new WardData { Name = "Phường An Phú Đông", Population = 46512 },
        new WardData { Name = "Phường Tân Thới Hiệp", Population = 53137 },
        new WardData { Name = "Phường Trung Mỹ Tây", Population = 42037 },
        new WardData { Name = "Phường Tân Hưng Thuận", Population = 33467 },
        new WardData { Name = "Phường Đông Hưng Thuận", Population = 45793 },
        new WardData { Name = "Phường Tân Thới Nhất", Population = 60912 }
    }
},





new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Thành phố Thủ Đức",
    Population = 592686,
    AreaType = "Nội thành",
    Wards = new List <WardData>
    {
        new WardData { Name = "Phường Linh Xuân", Population = 61852 },
        new WardData { Name = "Phường Bình Chiểu", Population = 77984 },
        new WardData { Name = "Phường Linh Trung", Population = 64886 },
        new WardData { Name = "Phường Tam Bình", Population = 29411 },
        new WardData { Name = "Phường Tam Phú", Population = 32926 },
        new WardData { Name = "Phường Hiệp Bình Phước", Population = 63454 },
        new WardData { Name = "Phường Hiệp Bình Chánh", Population = 101223 },
        new WardData { Name = "Phường Linh Chiểu", Population = 30609 },
        new WardData { Name = "Phường Linh Tây", Population = 24627 },
        new WardData { Name = "Phường Linh Đông", Population = 43429 },
        new WardData { Name = "Phường Bình Thọ", Population = 14953 },
        new WardData { Name = "Phường Trường Thọ", Population = 47332 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 9",
    Population = 397006,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Long Bình", Population = 23505 },
        new WardData { Name = "Phường Long Thạnh Mỹ", Population = 35502 },
        new WardData { Name = "Phường Tân Phú", Population = 37857 },
        new WardData { Name = "Phường Hiệp Phú", Population = 25409 },
        new WardData { Name = "Phường Tăng Nhơn Phú A", Population = 46661 },
        new WardData { Name = "Phường Tăng Nhơn Phú B", Population = 37381 },
        new WardData { Name = "Phường Phước Long B", Population = 64983 },
        new WardData { Name = "Phường Phước Long A", Population = 26883 },
        new WardData { Name = "Phường Trường Thạnh", Population = 23431 },
        new WardData { Name = "Phường Long Phước", Population = 11857 },
        new WardData { Name = "Phường Long Trường", Population = 25575 },
        new WardData { Name = "Phường Phước Bình", Population = 15746 },
        new WardData { Name = "Phường Phú Hữu", Population = 22216 }
    }
},

new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận Gò Vấp",
    Population = 676899,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 15", Population = 40289 },
        new WardData { Name = "Phường 13", Population = 24229 },
        new WardData { Name = "Phường 17", Population = 54728 },
        new WardData { Name = "Phường 6", Population = 34340 },
        new WardData { Name = "Phường 16", Population = 54520 },
        new WardData { Name = "Phường 12", Population = 60502 },
        new WardData { Name = "Phường 14", Population = 48491 },
        new WardData { Name = "Phường 10", Population = 47330 },
        new WardData { Name = "Phường 05", Population = 55765 },
        new WardData { Name = "Phường 07", Population = 35118 },
        new WardData { Name = "Phường 04", Population = 20520 },
        new WardData { Name = "Phường 01", Population = 21934 },
        new WardData { Name = "Phường 9", Population = 34596 },
        new WardData { Name = "Phường 8", Population = 32778 },
        new WardData { Name = "Phường 11", Population = 56130 },
        new WardData { Name = "Phường 03", Population = 55629 }
    }
},

new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận Bình Thạnh",
    Population = 499164,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 13", Population = 37192 },
        new WardData { Name = "Phường 11", Population = 31609 },
        new WardData { Name = "Phường 27", Population = 21659 },
        new WardData { Name = "Phường 26", Population = 42067 },
        new WardData { Name = "Phường 12", Population = 36701 },
        new WardData { Name = "Phường 25", Population = 42674 },
        new WardData { Name = "Phường 05", Population = 16342 },
        new WardData { Name = "Phường 07", Population = 14555 },
        new WardData { Name = "Phường 24", Population = 24447 },
        new WardData { Name = "Phường 06", Population = 10300 },
        new WardData { Name = "Phường 14", Population = 11106 },
        new WardData { Name = "Phường 15", Population = 24947 },
        new WardData { Name = "Phường 02", Population = 17567 },
        new WardData { Name = "Phường 01", Population = 12482 },
        new WardData { Name = "Phường 03", Population = 22603 },
        new WardData { Name = "Phường 17", Population = 23155 },
        new WardData { Name = "Phường 21", Population = 23164 },
        new WardData { Name = "Phường 22", Population = 49842 },
        new WardData { Name = "Phường 19", Population = 19997 },
        new WardData { Name = "Phường 28", Population = 16755 }
    }
},


new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận Tân Bình",
    Population = 474792,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 02", Population = 36529 },
        new WardData { Name = "Phường 04", Population = 31105 },
        new WardData { Name = "Phường 12", Population = 39857 },
        new WardData { Name = "Phường 13", Population = 57522 },
        new WardData { Name = "Phường 01", Population = 12120 },
        new WardData { Name = "Phường 03", Population = 17962 },
        new WardData { Name = "Phường 11", Population = 28662 },
        new WardData { Name = "Phường 07", Population = 15771 },
        new WardData { Name = "Phường 05", Population = 18800 },
        new WardData { Name = "Phường 10", Population = 47648 },
        new WardData { Name = "Phường 06", Population = 26593 },
        new WardData { Name = "Phường 08", Population = 19081 },
        new WardData { Name = "Phường 09", Population = 30998 },
        new WardData { Name = "Phường 14", Population = 26476 },
        new WardData { Name = "Phường 15", Population = 65668 }
    }
},

new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận Tân Phú",
    Population = 485348,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Tân Sơn Nhì", Population = 43267 },
        new WardData { Name = "Phường Tây Thạnh", Population = 61120 },
        new WardData { Name = "Phường Sơn Kỳ", Population = 42667 },
        new WardData { Name = "Phường Tân Quý", Population = 65568 },
        new WardData { Name = "Phường Tân Thành", Population = 37904 },
        new WardData { Name = "Phường Phú Thọ Hòa", Population = 54967 },
        new WardData { Name = "Phường Phú Thạnh", Population = 45020 },
        new WardData { Name = "Phường Phú Trung", Population = 43147 },
        new WardData { Name = "Phường Hòa Thạnh", Population = 33822 },
        new WardData { Name = "Phường Hiệp Tân", Population = 30074 },
        new WardData { Name = "Phường Tân Thới Hòa", Population = 27792 }
    }
},


new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận Phú Nhuận",
    Population = 163961,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 04", Population = 12365 },
        new WardData { Name = "Phường 05", Population = 15492 },
        new WardData { Name = "Phường 09", Population = 18205 },
        new WardData { Name = "Phường 07", Population = 22964 },
        new WardData { Name = "Phường 03", Population = 7749 },
        new WardData { Name = "Phường 01", Population = 10299 },
        new WardData { Name = "Phường 02", Population = 11527 },
        new WardData { Name = "Phường 08", Population = 7950 },
        new WardData { Name = "Phường 15", Population = 10560 },
        new WardData { Name = "Phường 10", Population = 9391 },
        new WardData { Name = "Phường 11", Population = 8840 },
        new WardData { Name = "Phường 17", Population = 7777 },
        new WardData { Name = "Phường 14", Population = 6791 },
        new WardData { Name = "Phường 12", Population = 6364 },
        new WardData { Name = "Phường 13", Population = 7687 }
    }
},


new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 2",
    Population = 180275,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Thảo Điền", Population = 22639 },
        new WardData { Name = "Phường An Phú", Population = 35825 },
        new WardData { Name = "Phường Bình An", Population = 18977 },
        new WardData { Name = "Phường Bình Trưng Đông", Population = 26050 },
        new WardData { Name = "Phường Bình Trưng Tây", Population = 27536 },
        new WardData { Name = "Phường Bình Khánh", Population = 5130 },
        new WardData { Name = "Phường An Khánh", Population = 223 },
        new WardData { Name = "Phường Cát Lái", Population = 19542 },
        new WardData { Name = "Phường Thạnh Mỹ Lợi", Population = 23101 },
        new WardData { Name = "Phường An Lợi Đông", Population = 508 },
        new WardData { Name = "Phường Thủ Thiêm", Population = 744 }
    }
},

new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 3",
    Population = 190375,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 08", Population = 16481 },
        new WardData { Name = "Phường 07", Population = 13052 },
        new WardData { Name = "Phường 14", Population = 16265 },
        new WardData { Name = "Phường 12", Population = 12398 },
        new WardData { Name = "Phường 11", Population = 22383 },
        new WardData { Name = "Phường 13", Population = 6988 },
        new WardData { Name = "Phường 06", Population = 7072 },
        new WardData { Name = "Phường 09", Population = 17472 },
        new WardData { Name = "Phường 10", Population = 9166 },
        new WardData { Name = "Phường 04", Population = 18930 },
        new WardData { Name = "Phường 05", Population = 14408 },
        new WardData { Name = "Phường 03", Population = 10604 },
        new WardData { Name = "Phường 02", Population = 11413 },
        new WardData { Name = "Phường 01", Population = 13743 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 10",
    Population = 234819,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 15", Population = 25346 },
        new WardData { Name = "Phường 13", Population = 25657 },
        new WardData { Name = "Phường 14", Population = 32727 },
        new WardData { Name = "Phường 12", Population = 24342 },
        new WardData { Name = "Phường 11", Population = 9263 },
        new WardData { Name = "Phường 10", Population = 11135 },
        new WardData { Name = "Phường 09", Population = 17800 },
        new WardData { Name = "Phường 01", Population = 14257 },
        new WardData { Name = "Phường 08", Population = 11142 },
        new WardData { Name = "Phường 02", Population = 16841 },
        new WardData { Name = "Phường 04", Population = 13354 },
        new WardData { Name = "Phường 07", Population = 7569 },
        new WardData { Name = "Phường 05", Population = 10783 },
        new WardData { Name = "Phường 06", Population = 7358 },
        new WardData { Name = "Phường 03", Population = 7245 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 11",
    Population = 209867,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 15", Population = 12396 },
        new WardData { Name = "Phường 05", Population = 32226 },
        new WardData { Name = "Phường 14", Population = 15292 },
        new WardData { Name = "Phường 11", Population = 12176 },
        new WardData { Name = "Phường 03", Population = 21423 },
        new WardData { Name = "Phường 10", Population = 9818 },
        new WardData { Name = "Phường 13", Population = 11190 },
        new WardData { Name = "Phường 08", Population = 13254 },
        new WardData { Name = "Phường 09", Population = 8249 },
        new WardData { Name = "Phường 12", Population = 8761 },
        new WardData { Name = "Phường 07", Population = 12191 },
        new WardData { Name = "Phường 06", Population = 9472 },
        new WardData { Name = "Phường 04", Population = 8794 },
        new WardData { Name = "Phường 01", Population = 12549 },
        new WardData { Name = "Phường 02", Population = 9658 },
        new WardData { Name = "Phường 16", Population = 12418 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 4",
    Population = 175329,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 12", Population = 6415 },
        new WardData { Name = "Phường 13", Population = 11289 },
        new WardData { Name = "Phường 09", Population = 10473 },
        new WardData { Name = "Phường 06", Population = 10602 },
        new WardData { Name = "Phường 08", Population = 15321 },
        new WardData { Name = "Phường 10", Population = 9595 },
        new WardData { Name = "Phường 05", Population = 5115 },
        new WardData { Name = "Phường 18", Population = 9842 },
        new WardData { Name = "Phường 14", Population = 14922 },
        new WardData { Name = "Phường 04", Population = 15401 },
        new WardData { Name = "Phường 03", Population = 10958 },
        new WardData { Name = "Phường 16", Population = 19933 },
        new WardData { Name = "Phường 02", Population = 10852 },
        new WardData { Name = "Phường 15", Population = 12402 },
        new WardData { Name = "Phường 01", Population = 12209 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 5",
    Population = 159073,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 04", Population = 10134 },
        new WardData { Name = "Phường 09", Population = 14720 },
        new WardData { Name = "Phường 03", Population = 7630 },
        new WardData { Name = "Phường 12", Population = 5654 },
        new WardData { Name = "Phường 02", Population = 15695 },
        new WardData { Name = "Phường 08", Population = 7868 },
        new WardData { Name = "Phường 15", Population = 9610 },
        new WardData { Name = "Phường 07", Population = 11769 },
        new WardData { Name = "Phường 01", Population = 17199 },
        new WardData { Name = "Phường 11", Population = 11601 },
        new WardData { Name = "Phường 14", Population = 12564 },
        new WardData { Name = "Phường 05", Population = 10847 },
        new WardData { Name = "Phường 06", Population = 7915 },
        new WardData { Name = "Phường 10", Population = 7040 },
        new WardData { Name = "Phường 13", Population = 8827 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 6",
    Population = 233561,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 14", Population = 20674 },
        new WardData { Name = "Phường 13", Population = 25007 },
        new WardData { Name = "Phường 09", Population = 12693 },
        new WardData { Name = "Phường 06", Population = 13474 },
        new WardData { Name = "Phường 12", Population = 25068 },
        new WardData { Name = "Phường 05", Population = 12844 },
        new WardData { Name = "Phường 11", Population = 25484 },
        new WardData { Name = "Phường 02", Population = 7532 },
        new WardData { Name = "Phường 01", Population = 9900 },
        new WardData { Name = "Phường 04", Population = 12596 },
        new WardData { Name = "Phường 08", Population = 22325 },
        new WardData { Name = "Phường 03", Population = 8999 },
        new WardData { Name = "Phường 07", Population = 11682 },
        new WardData { Name = "Phường 10", Population = 25283 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 8",
    Population = 424667,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường 08", Population = 10817 },
        new WardData { Name = "Phường 02", Population = 21392 },
        new WardData { Name = "Phường 01", Population = 23110 },
        new WardData { Name = "Phường 03", Population = 25887 },
        new WardData { Name = "Phường 11", Population = 7077 },
        new WardData { Name = "Phường 09", Population = 20295 },
        new WardData { Name = "Phường 10", Population = 15462 },
        new WardData { Name = "Phường 04", Population = 46597 },
        new WardData { Name = "Phường 13", Population = 8521 },
        new WardData { Name = "Phường 12", Population = 16579 },
        new WardData { Name = "Phường 05", Population = 45063 },
        new WardData { Name = "Phường 14", Population = 19453 },
        new WardData { Name = "Phường 06", Population = 33390 },
        new WardData { Name = "Phường 15", Population = 43125 },
        new WardData { Name = "Phường 16", Population = 50215 },
        new WardData { Name = "Phường 07", Population = 37684 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận Bình Tân",
    Population = 784173,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Bình Hưng Hòa", Population = 84966 },
        new WardData { Name = "Phường Bình Hưng Hoà A", Population = 123961 },
        new WardData { Name = "Phường Bình Hưng Hoà B", Population = 88849 },
        new WardData { Name = "Phường Bình Trị Đông", Population = 83116 },
        new WardData { Name = "Phường Bình Trị Đông A", Population = 73902 },
        new WardData { Name = "Phường Bình Trị Đông B", Population = 63394 },
        new WardData { Name = "Phường Tân Tạo", Population = 74883 },
        new WardData { Name = "Phường Tân Tạo A", Population = 73956 },
        new WardData { Name = "Phường An Lạc", Population = 84719 },
        new WardData { Name = "Phường An Lạc A", Population = 32427 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Quận 7",
    Population = 360155,
    AreaType = "Nội thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Phường Tân Thuận Đông", Population = 44117 },
        new WardData { Name = "Phường Tân Thuận Tây", Population = 31442 },
        new WardData { Name = "Phường Tân Kiểng", Population = 35853 },
        new WardData { Name = "Phường Tân Hưng", Population = 44244 },
        new WardData { Name = "Phường Bình Thuận", Population = 36062 },
        new WardData { Name = "Phường Tân Quy", Population = 27368 },
        new WardData { Name = "Phường Phú Thuận", Population = 45988 },
        new WardData { Name = "Phường Tân Phú", Population = 34734 },
        new WardData { Name = "Phường Tân Phong", Population = 28266 },
        new WardData { Name = "Phường Phú Mỹ", Population = 32081 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Huyện Củ Chi",
    Population = 462047,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Củ Chi", Population = 23176 },
        new WardData { Name = "Xã Phú Mỹ Hưng", Population = 7406 },
        new WardData { Name = "Xã An Phú", Population = 11093 },
        new WardData { Name = "Xã Trung Lập Thượng", Population = 12669 },
        new WardData { Name = "Xã An Nhơn Tây", Population = 17770 },
        new WardData { Name = "Xã Nhuận Đức", Population = 13655 },
        new WardData { Name = "Xã Phạm Văn Cội", Population = 9534 },
        new WardData { Name = "Xã Phú Hòa Đông", Population = 25028 },
        new WardData { Name = "Xã Trung Lập Hạ", Population = 14859 },
        new WardData { Name = "Xã Trung An", Population = 26928 },
        new WardData { Name = "Xã Phước Thạnh", Population = 17611 },
        new WardData { Name = "Xã Phước Hiệp", Population = 13662 },
        new WardData { Name = "Xã Tân An Hội", Population = 31561 },
        new WardData { Name = "Xã Phước Vĩnh An", Population = 21060 },
        new WardData { Name = "Xã Thái Mỹ", Population = 13383 },
        new WardData { Name = "Xã Tân Thạnh Tây", Population = 16911 },
        new WardData { Name = "Xã Hòa Phú", Population = 17723 },
        new WardData { Name = "Xã Tân Thạnh Đông", Population = 45171 },
        new WardData { Name = "Xã Bình Mỹ", Population = 37247 },
        new WardData { Name = "Xã Tân Phú Trung", Population = 43262 },
        new WardData { Name = "Xã Tân Thông Hội", Population = 42338 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Huyện Hóc Môn",
    Population = 542243,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Hóc Môn", Population = 18145 },
        new WardData { Name = "Xã Tân Hiệp", Population = 34031 },
        new WardData { Name = "Xã Nhị Bình", Population = 14627 },
        new WardData { Name = "Xã Đông Thạnh", Population = 71243 },
        new WardData { Name = "Xã Tân Thới Nhì", Population = 30186 },
        new WardData { Name = "Xã Thới Tam Thôn<%@", Population = 81094 },
        new WardData { Name = "Xã Xuân Thới Sơn", Population = 35586 },
        new WardData { Name = "Xã Tân Xuân", Population = 28964 },
        new WardData { Name = "Xã Xuân Thới Đông", Population = 34344 },
        new WardData { Name = "Xã Trung Chánh", Population = 35463 },
        new WardData { Name = "Xã Xuân Thới Thượng", Population = 72721 },
        new WardData { Name = "Xã Bà Điểm", Population = 85839 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Huyện Bình Chánh",
    Population = 705508,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Tân Túc", Population = 22573 },
        new WardData { Name = "Xã Phạm Văn Hai", Population = 33435 },
        new WardData { Name = "Xã Vĩnh Lộc A", Population = 124709 },
        new WardData { Name = "Xã Vĩnh Lộc B", Population = 121490 },
        new WardData { Name = "Xã Bình Lợi", Population = 12328 },
        new WardData { Name = "Xã Lê Minh Xuân", Population = 40704 },
        new WardData { Name = "Xã Tân Nhựt", Population = 27925 },
        new WardData { Name = "Xã Tân Kiên", Population = 60603 },
        new WardData { Name = "Xã Bình Hưng", Population = 87001 },
        new WardData { Name = "Xã Phong Phú", Population = 37058 },
        new WardData { Name = "Xã An Phú Tây", Population = 21152 },
        new WardData { Name = "Xã Hưng Long", Population = 25677 },
        new WardData { Name = "Xã Đa Phước", Population = 24643 },
        new WardData { Name = "Xã Tân Quý Tây", Population = 24326 },
        new WardData { Name = "Xã Bình Chánh", Population = 27698 },
        new WardData { Name = "Xã Quy Đức", Population = 14186 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Huyện Nhà Bè",
    Population = 206837,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Nhà Bè", Population = 45524 },
        new WardData { Name = "Xã Phước Kiển", Population = 52752 },
        new WardData { Name = "Xã Phước Lộc", Population = 15004 },
        new WardData { Name = "Xã Nhơn Đức", Population = 26280 },
        new WardData { Name = "Xã Phú Xuân", Population = 32350 },
        new WardData { Name = "Xã Long Thới", Population = 14754 },
        new WardData { Name = "Xã Hiệp Phước", Population = 20173 }
    }
},
new DistrictData
{
    City = "Hồ Chí Minh",
    District = "Huyện Cần Giờ",
    Population = 71526,
    AreaType = "Ngoại thành",
    Wards = new List<WardData>
    {
        new WardData { Name = "Thị trấn Cần Thạnh", Population = 11154 },
        new WardData { Name = "Xã Bình Khánh", Population = 20961 },
        new WardData { Name = "Xã Tam Thôn Hiệp", Population = 5637 },
        new WardData { Name = "Xã An Thới Đông", Population = 13281 },
        new WardData { Name = "Xã Thạnh An", Population = 4512 },
        new WardData { Name = "Xã Long Hòa", Population = 10715 },
        new WardData { Name = "Xã Lý Nhơn", Population = 5266 }
    }
}



};


                //     Hồ Chí Minh
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận Thủ Đức", Population = 592686 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận 9", Population = 397006 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận 2", Population = 180275 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận Bình Tân", Population = 784173 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận Tân Phú", Population = 485348 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận 12", Population = 620146 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận Gò Vấp", Population = 676899 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận Tân Bình", Population = 474792 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận Bình Thạnh", Population = 499164 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận Phú Nhuận", Population = 163961 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận 1", Population = 142625 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận 3", Population = 190375 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận 10", Population = 234819 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận 11", Population = 209867 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận 6", Population = 233561 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận 5", Population = 159073 },
                //    new DistrictData { City = "Hồ Chí Minh", District = "Quận 7", Population = 360155 },
                //     new DistrictData { City = "Hồ Chí Minh", District = "Quận 8", Population = 424667 },
                //      new DistrictData { City = "Hồ Chí Minh", District = "Quận 4", Population = 175329 },

                //     Hải Phòng
                //    new DistrictData { City = "Hải Phòng", District = "Quận Hồng Bàng", Population = 96111 },
                //    new DistrictData { City = "Hải Phòng", District = "Quận Ngô Quyền", Population = 165309 },
                //    new DistrictData { City = "Hải Phòng", District = "Quận Lê Chân", Population = 219762 },
                //    new DistrictData { City = "Hải Phòng", District = "Quận Hải An", Population = 132943 },
                //    new DistrictData { City = "Hải Phòng", District = "Quận Kiến An", Population = 118047 },
                //    new DistrictData { City = "Hải Phòng", District = "Quận Dương Kinh", Population = 60319 },

                //     Đà Nẵng
                //    new DistrictData { City = "Đà Nẵng", District = "Quận Hải Châu", Population = 201522 },
                //    new DistrictData { City = "Đà Nẵng", District = "Quận Thanh Khê", Population = 185064 },
                //    new DistrictData { City = "Đà Nẵng", District = "Quận Sơn Trà", Population = 157415 },
                //    new DistrictData { City = "Đà Nẵng", District = "Quận Ngũ Hành Sơn", Population = 90352 },
                //    new DistrictData { City = "Đà Nẵng", District = "Quận Liên Chiểu", Population = 194913 },
                //    new DistrictData { City = "Đà Nẵng", District = "Quận Cẩm Lệ", Population = 159295 },
                //};

                ViewBag.AllDistricts = allDistricts;


                // Nếu người dùng đã chọn thành phố
                if (city != null && city.Any())
                {
                    var filteredDistricts = allDistricts
                        .Where(d => city.Contains(d.City))
                        .ToList();

                    ViewBag.UniqueDistricts = filteredDistricts;
                }
                else
                {
                    ViewBag.UniqueDistricts = null;
                }




                return View(adminChart);
            }

            // Nếu không có bộ lọc, trả về danh sách tỉnh rỗng
            ViewBag.Provinces = new List<string>();
            return View(new List<ALLDATA>());
        }


        [HttpPost]
        public IActionResult Create(Sampling sampling)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"INSERT INTO sampling 
                            (name, code, loaihinh, tenkhachhang, Thoigianbatdau, Thoigianketthuc, 
                             somautotal, khuvuc, somaunoithanh, somaungoaithanh, Somautoida, Somautoithieu, Sophuongextra)
                            VALUES 
                            (@name, @code, @loaihinh, @tenkhachhang, @Thoigianbatdau, @Thoigianketthuc, 
                             @somautotal, @khuvuc, @somaunoithanh, @somaungoaithanh, @Somautoida, @Somautoithieu, @Sophuongextra)";

                    using (var command = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", sampling.name);
                        command.Parameters.AddWithValue("@code", sampling.code);
                        command.Parameters.AddWithValue("@loaihinh", sampling.loaihinh);
                        command.Parameters.AddWithValue("@tenkhachhang", sampling.tenkhachhang);
                        command.Parameters.AddWithValue("@Thoigianbatdau", sampling.Thoigianbatdau);
                        command.Parameters.AddWithValue("@Thoigianketthuc", sampling.Thoigianketthuc);
                        command.Parameters.AddWithValue("@somautotal", sampling.somautotal);
                        command.Parameters.AddWithValue("@khuvuc", sampling.khuvuc);
                        command.Parameters.AddWithValue("@somaunoithanh", sampling.somaunoithanh);
                        command.Parameters.AddWithValue("@somaungoaithanh", sampling.somaungoaithanh);
                        command.Parameters.AddWithValue("@Somautoida", sampling.Somautoida);
                        command.Parameters.AddWithValue("@Somautoithieu", sampling.Somautoithieu);
                        command.Parameters.AddWithValue("@Sophuongextra", sampling.Sophuongextra);

                        command.ExecuteNonQuery();
                    }
                }
                // Sau khi lưu xong có thể redirect hoặc báo thành công
                return RedirectToAction("Index", "Danso");
            }
            else
            {
                return View(sampling);
            }
        }






        private List<ALLDATA> getadminChart(
          string stt, List<string> code, List<string> projectName, List<string> year,
  string contactObject, List<string> sbjnum, string fullname,
  List<string> city, string address, string street, string ward,
  List<string> district, List<string> phoneNumber, string email,
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



        public class WardData
        {
            public string Name { get; set; }
            public int Population { get; set; }
        }
        public class DistrictData
        {
            public string City { get; set; }
            public string District { get; set; }
            public int Population { get; set; }
            public string AreaType { get; set; } // "Nội thành" hoặc "Ngoại thành"
            public List<WardData> Wards { get; set; }
        }





    }
}
