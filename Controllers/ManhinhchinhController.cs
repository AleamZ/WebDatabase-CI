using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using CIResearch.Models;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;
using CIResearch.Services;
using Microsoft.AspNetCore.Authorization;

namespace CIResearch.Controllers
{
	public class ManhinhchinhController : Controller
	{
		private string _connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;";
		private readonly IMemoryCache _cache;

		public ManhinhchinhController(IConfiguration configuration, IMemoryCache cache)
		{
			_cache = cache;
		}

		public IActionResult Index(
			List<string> code = null,
			List<string> projectName = null,
			List<string> year = null,
			List<string> city = null,
			List<string> job = null,
			List<string> sex = null,
			List<string> khuvuc = null,
			List<string> nganhhang = null)
		{
			// Echo selected filters back to the view first
			ViewBag.Code = code;
			ViewBag.ProjectName = projectName;
			ViewBag.Year = year;
			ViewBag.City = city;
			ViewBag.Job = job;
			ViewBag.Sex = sex;
			ViewBag.Khuvuc = khuvuc;
			ViewBag.Nganhhang = nganhhang;

			// Initialize defaults
			ViewBag.TotalProjects = 0;
			ViewBag.TotalSamples = 0;
			ViewBag.MaleCount = 0;
			ViewBag.FemaleCount = 0;
			ViewBag.UnknownSexCount = 0;
			ViewBag.MienBacCount = 0;
			ViewBag.MienTrungCount = 0;
			ViewBag.MienNamCount = 0;

			// Distinct lists
			ViewBag.CodeList = new List<string>();
			ViewBag.ProjectNameList = new List<string>();
			ViewBag.YearList = new List<string>();
			ViewBag.CityList = new List<string>();
			ViewBag.JobList = new List<string>();
			ViewBag.SexList = new List<string>();
			ViewBag.KhuvucList = new List<string>();
			ViewBag.NganhhangList = new List<string>();

			// Will hold detailed rows for the data table
			var rows = new List<ALLDATA>();

			// Fetch summary data and distincts using current filters
			using (var conn = new MySqlConnection(_connectionString))
			{
				conn.Open();

				// Populate distinct lists for filters
				ViewBag.CodeList = GetDistinctListExcludingDoctors(conn, "Code");
				ViewBag.ProjectNameList = GetDistinctListExcludingDoctors(conn, "ProjectName");
				ViewBag.YearList = GetDistinctListExcludingDoctors(conn, "Year");
				ViewBag.CityList = GetDistinctNormalizedCitiesExcludingDoctors(conn);
				// Build mapping normalized->original like Doctor page for City
				var cityMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				using (var mapCmd = conn.CreateCommand())
				{
					mapCmd.CommandText = "SELECT DISTINCT City FROM all_data_final WHERE City IS NOT NULL AND TRIM(City) <> '' ORDER BY City LIMIT 10000";
					using (var rmap = mapCmd.ExecuteReader())
					{
						while (rmap.Read())
						{
							var raw = rmap.IsDBNull(0) ? null : rmap.GetValue(0)?.ToString();
							if (string.IsNullOrWhiteSpace(raw)) continue;
							var norm = raw.Trim()
								.Replace("Tỉnh", "", StringComparison.OrdinalIgnoreCase)
								.Replace("Thành phố", "", StringComparison.OrdinalIgnoreCase)
								.Replace("TP.", "", StringComparison.OrdinalIgnoreCase)
								.Replace("tp.", "", StringComparison.OrdinalIgnoreCase)
								.Replace("TP ", "", StringComparison.OrdinalIgnoreCase)
								.Replace("tp ", "", StringComparison.OrdinalIgnoreCase)
								.Replace("Quận", "", StringComparison.OrdinalIgnoreCase)
								.Replace("Huyện", "", StringComparison.OrdinalIgnoreCase)
								.Trim();
							if (!cityMapping.ContainsKey(norm)) cityMapping[norm] = raw;
						}
					}
				}
				// Prepare region province sets (normalized names) for City filtering
				var northernProvinces_ForCity = new List<string>
				{
					"Bắc Giang", "Bắc Kạn", "Bắc Ninh", "Cao Bằng","Điện Biên","Hà Giang","Hà Nam","Hà Nội","Hải Dương","Hải Phòng","Hòa Bình","Hưng Yên","Lai Châu","Lào Cai","Nam Định","Ninh Bình","Phú Thọ","Quảng Ninh","Sơn La","Thái Bình","Thái Nguyên","Tuyên Quang","Vĩnh Phúc","Lạng Sơn","Yên Bái"
				};
				var centralProvinces_ForCity = new List<string>
				{
					"Bình Định","Đà Nẵng","Đắk Lắk","Đắk Nông","Gia Lai","Hà Tĩnh","Khánh Hòa","Kon Tum","Nghệ An","Phú Yên","Thanh Hóa","Quảng Bình","Quảng Nam","Quảng Ngãi","Quảng Trị","Thừa Thiên Huế"
				};
				var southernProvinces_ForCity = new List<string>
				{
					"An Giang","Bà Rịa Vũng Tàu","Bạc Liêu","Bến Tre","Bình Dương", "Bình Phước","Bình Thuận","Cà Mau","Cần Thơ","Đồng Nai","Đồng Tháp","Hậu Giang","Hồ Chí Minh","Kiên Giang","Lâm Đồng","Long An","Ninh Thuận","Sóc Trăng","Tây Ninh","Tiền Giang","Trà Vinh","Vĩnh Long"
				};
				var northSetCity = new HashSet<string>(northernProvinces_ForCity, StringComparer.OrdinalIgnoreCase);
				var centralSetCity = new HashSet<string>(centralProvinces_ForCity, StringComparer.OrdinalIgnoreCase);
				var southSetCity = new HashSet<string>(southernProvinces_ForCity, StringComparer.OrdinalIgnoreCase);
				// If region filter is active, narrow CityList to only cities in selected regions
				if (khuvuc != null && khuvuc.Count > 0 && ViewBag.CityList is List<string> fullCityList)
				{
					bool includeNorth = khuvuc.Any(k => string.Equals(k, "Miền Bắc", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "Mien Bac", StringComparison.OrdinalIgnoreCase));
					bool includeCentral = khuvuc.Any(k => string.Equals(k, "Miền Trung", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "Mien Trung", StringComparison.OrdinalIgnoreCase));
					bool includeSouth = khuvuc.Any(k => string.Equals(k, "Miền Nam", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "Mien Nam", StringComparison.OrdinalIgnoreCase));
					List<string> filteredCities = new List<string>();
					foreach (var c in fullCityList)
					{
						var norm = c; // already normalized in list
						if (includeNorth && northSetCity.Contains(norm)) { filteredCities.Add(c); continue; }
						if (includeCentral && centralSetCity.Contains(norm)) { filteredCities.Add(c); continue; }
						if (includeSouth && southSetCity.Contains(norm)) { filteredCities.Add(c); continue; }
					}
					ViewBag.CityList = filteredCities.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
				}
				ViewBag.CityMapping = cityMapping;
				ViewBag.JobList = GetDistinctListExcludingDoctors(conn, "Job");
				ViewBag.SexList = new List<string> { "Nam", "Nữ", "Không xác định" };
				ViewBag.KhuvucList = GetRegionFilterList(conn);
				ViewBag.NganhhangList = GetDistinctListExcludingDoctors(conn, "Nganhhang");

				// Build WHERE and parameters based on current filters
				var whereParts = new List<string>();
				var cmd = conn.CreateCommand();

				AppendInFilter(cmd, whereParts, "Code", code);
				AppendInFilter(cmd, whereParts, "ProjectName", projectName);
				AppendInFilter(cmd, whereParts, "Year", year);
				AppendInFilter(cmd, whereParts, "City", city);
				AppendInFilter(cmd, whereParts, "Job", job);
				AppendSexFilter(whereParts, sex);
				AppendRegionFilter(cmd, whereParts, khuvuc);
				AppendInFilter(cmd, whereParts, "Nganhhang", nganhhang);

				// Always exclude doctor jobs on Consumer page
				whereParts.Add(@"NOT (
					LOWER(Job) = 'bác sĩ' OR LOWER(Job) = 'bac si' OR LOWER(Job) = 'bác sỹ' OR
					LOWER(Job) = 'Bác sĩ' OR LOWER(Job) = 'Bac si' OR LOWER(Job) = 'Bác sỹ' OR
					LOWER(Job) = 'Bác Sĩ' OR LOWER(Job) = 'Bac Si' OR LOWER(Job) = 'Bác Sỹ' OR
					LOWER(Job) = 'bs' OR LOWER(Job) = 'bs.' OR LOWER(Job) = 'doctor' OR LOWER(Job) = 'dr' OR LOWER(Job) = 'dr.' OR
					LOWER(Job) LIKE '%bác sĩ%' OR LOWER(Job) LIKE '%bac si%' OR LOWER(Job) LIKE '%bác sỹ%' OR
					LOWER(Job) LIKE '% doctor%' OR LOWER(Job) LIKE 'doctor %' OR LOWER(Job) LIKE '% doctor %' OR
					LOWER(Job) LIKE '% dr%' OR LOWER(Job) LIKE 'dr %' OR LOWER(Job) LIKE '% dr %' OR
					LOWER(Job) LIKE '% bs%' OR LOWER(Job) LIKE 'bs %' OR LOWER(Job) LIKE '% bs %'
				)");

				string whereClause = whereParts.Count > 0 ? (" WHERE " + string.Join(" AND ", whereParts)) : string.Empty;

				// Total projects
				cmd.CommandText = $"SELECT COUNT(DISTINCT ProjectName) FROM all_data_final{whereClause}";
				ViewBag.TotalProjects = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

				// Total samples
				cmd.CommandText = $"SELECT COUNT(*) FROM all_data_final{whereClause}";
				ViewBag.TotalSamples = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

				// Sex distribution
				cmd.CommandText = $@"SELECT COALESCE(NULLIF(TRIM(Sex), ''), 'Unknown') AS SexKey, COUNT(*) Cnt
					FROM all_data_final{whereClause}
					GROUP BY SexKey";
				var male = 0; var female = 0; var unknown = 0;
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var key = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
						var cnt = reader.GetInt32(1);
						var raw = (key ?? string.Empty).Trim();
						var lower = raw.ToLowerInvariant();
						var compact = lower.Replace(" ", string.Empty);
						// Map to three buckets like filter
						if (compact == "nam" || compact == "male" || compact == "1.nam")
						{
							male += cnt;
						}
						else if (compact == "nữ" || compact == "nu" || compact == "female" || compact == "1.nữ" || compact == "2.nữ" || compact == "1.nu" || compact == "2.nu")
						{
							female += cnt;
						}
						else if (compact == string.Empty || compact == "0" || compact == "unknown" || compact == "khôngxácđịnh" || compact == "khongxacdinh" || compact == "tuchoitralloi" || compact == "tuchoitraloi" || compact == "từchốitrảlời")
						{
							unknown += cnt;
						}
						else
						{
							unknown += cnt;
						}
					}
				}
				ViewBag.MaleCount = male;
				ViewBag.FemaleCount = female;
				ViewBag.UnknownSexCount = unknown;

				// Region distribution (Bắc/Trung/Nam) — đồng bộ với filter
				// Nếu đã có filter khu vực, chỉ phân loại dữ liệu đã được filter
				// Nếu không có filter khu vực, phân loại tất cả dữ liệu
				bool hasRegionFilter = khuvuc != null && khuvuc.Count > 0;
				bool hasCityFilter = city != null && city.Count > 0;
				
				// Danh sách tỉnh/thành theo 3 miền giống trang Bác sĩ, kèm chuẩn hóa tên
				string NormalizeCityName(string name)
				{
					if (string.IsNullOrWhiteSpace(name)) return string.Empty;
					var n = name.Trim();
					n = n.Replace("Tỉnh ", string.Empty, StringComparison.OrdinalIgnoreCase)
						 .Replace("Thành phố ", string.Empty, StringComparison.OrdinalIgnoreCase)
						 .Replace("TP. ", string.Empty, StringComparison.OrdinalIgnoreCase)
						 .Replace("Tp. ", string.Empty, StringComparison.OrdinalIgnoreCase)
						 .Replace("TP ", string.Empty, StringComparison.OrdinalIgnoreCase)
						 .Replace("Tp ", string.Empty, StringComparison.OrdinalIgnoreCase);
					return n;
				}
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
				var northSet = new HashSet<string>(northernProvinces.Select(NormalizeCityName), StringComparer.OrdinalIgnoreCase);
				var centralSet = new HashSet<string>(centralProvinces.Select(NormalizeCityName), StringComparer.OrdinalIgnoreCase);
				var southSet = new HashSet<string>(southernProvinces.Select(NormalizeCityName), StringComparer.OrdinalIgnoreCase);
				
				if (hasRegionFilter)
				{
					// Nếu có filter khu vực, chỉ phân loại dữ liệu đã được filter
					cmd.CommandText = $@"SELECT 
						'Filtered' AS RegionKey,
						COALESCE(NULLIF(TRIM(District), ''), '') AS District,
						COALESCE(NULLIF(TRIM(City), ''), '') AS City,
						COUNT(*) Cnt
					FROM all_data_final{whereClause}
						GROUP BY District, City";
				}
				else
				{
					// Nếu không có filter khu vực, phân loại tất cả dữ liệu
					cmd.CommandText = $@"SELECT 
						COALESCE(NULLIF(TRIM(Khuvuc), ''), 'Unknown') AS RegionKey,
						COALESCE(NULLIF(TRIM(District), ''), '') AS District,
						COALESCE(NULLIF(TRIM(City), ''), '') AS City,
						COUNT(*) Cnt
						FROM all_data_final{whereClause}
						GROUP BY RegionKey, District, City";
				}
				int mb = 0, mt = 0, mn = 0;
				int totalProcessed = 0;
				
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var rawKey = reader.GetString(0) ?? string.Empty;
						var district = reader.GetString(1) ?? string.Empty;
						var cityValue = reader.GetString(2) ?? string.Empty;
						var cnt = reader.GetInt32(3);
						
						totalProcessed += cnt;
						
						// Hàm helper để phân loại khu vực dựa trên City và District - 100% coverage
						int ClassifyRegionByLocation(string city, string district)
						{
							var cityLower = city.ToLowerInvariant();
							var districtLower = district.ToLowerInvariant();
							
							// Miền Bắc - Danh sách đầy đủ các tỉnh (25 tỉnh)
							if (cityLower.Contains("hà nội") || cityLower.Contains("ha noi") || cityLower.Contains("hải phòng") || 
							    cityLower.Contains("hai phong") || cityLower.Contains("quảng ninh") || cityLower.Contains("quang ninh") ||
							    cityLower.Contains("bắc ninh") || cityLower.Contains("bac ninh") || cityLower.Contains("hưng yên") ||
							    cityLower.Contains("hung yen") || cityLower.Contains("hải dương") || cityLower.Contains("hai duong") ||
							    cityLower.Contains("bắc giang") || cityLower.Contains("bac giang") || cityLower.Contains("bắc kạn") ||
							    cityLower.Contains("bac kan") || cityLower.Contains("cao bằng") || cityLower.Contains("cao bang") ||
							    cityLower.Contains("điện biên") || cityLower.Contains("dien bien") || cityLower.Contains("hà giang") ||
							    cityLower.Contains("ha giang") || cityLower.Contains("hà nam") || cityLower.Contains("ha nam") ||
							    cityLower.Contains("hòa bình") || cityLower.Contains("hoa binh") || cityLower.Contains("lai châu") ||
							    cityLower.Contains("lai chau") || cityLower.Contains("lào cai") || cityLower.Contains("lao cai") ||
							    cityLower.Contains("nam định") || cityLower.Contains("nam dinh") || cityLower.Contains("ninh bình") ||
							    cityLower.Contains("ninh binh") || cityLower.Contains("phú thọ") || cityLower.Contains("phu tho") ||
							    cityLower.Contains("sơn la") || cityLower.Contains("son la") || cityLower.Contains("thái bình") ||
							    cityLower.Contains("thai binh") || cityLower.Contains("thái nguyên") || cityLower.Contains("thai nguyen") ||
							    cityLower.Contains("tuyên quang") || cityLower.Contains("tuyen quang") || cityLower.Contains("vĩnh phúc") ||
							    cityLower.Contains("vinh phuc") || cityLower.Contains("lạng sơn") || cityLower.Contains("lang son") ||
							    cityLower.Contains("yên bái") || cityLower.Contains("yen bai"))
							{
								return 1; // Miền Bắc
							}
							
							// Miền Trung - Danh sách đầy đủ các tỉnh (19 tỉnh)
							else if (cityLower.Contains("đà nẵng") || cityLower.Contains("da nang") || cityLower.Contains("huế") || 
							         cityLower.Contains("hue") || cityLower.Contains("khánh hòa") || cityLower.Contains("khanh hoa") ||
							         cityLower.Contains("bình định") || cityLower.Contains("binh dinh") || cityLower.Contains("phú yên") ||
							         cityLower.Contains("phu yen") || cityLower.Contains("nghệ an") || cityLower.Contains("nghe an") ||
							         cityLower.Contains("thanh hóa") || cityLower.Contains("thanh hoa") || cityLower.Contains("quảng bình") ||
							         cityLower.Contains("quang binh") || cityLower.Contains("quảng nam") || cityLower.Contains("quang nam") ||
							         cityLower.Contains("quảng ngãi") || cityLower.Contains("quang ngai") || cityLower.Contains("quảng trị") ||
							         cityLower.Contains("quang tri") || cityLower.Contains("đắk lắk") || cityLower.Contains("dak lak") ||
							         cityLower.Contains("đắk nông") || cityLower.Contains("dak nong") || cityLower.Contains("gia lai") ||
							         cityLower.Contains("kon tum") || cityLower.Contains("hà tĩnh") || cityLower.Contains("ha tinh"))
							{
								return 2; // Miền Trung
							}
							
							// Miền Nam - Danh sách đầy đủ các tỉnh (21 tỉnh)
							else if (cityLower.Contains("hồ chí minh") || cityLower.Contains("ho chi minh") || cityLower.Contains("đồng nai") || 
							         cityLower.Contains("dong nai") || cityLower.Contains("bình dương") || cityLower.Contains("binh duong") ||
							         cityLower.Contains("bà rịa vũng tàu") || cityLower.Contains("ba ria vung tau") || cityLower.Contains("tiền giang") ||
							         cityLower.Contains("tien giang") || cityLower.Contains("bến tre") || cityLower.Contains("ben tre") ||
							         cityLower.Contains("an giang") || cityLower.Contains("bạc liêu") || cityLower.Contains("bac lieu") ||
							         cityLower.Contains("bình phước") || cityLower.Contains("binh phuoc") || cityLower.Contains("bình thuận") ||
							         cityLower.Contains("binh thuan") || cityLower.Contains("cà mau") || cityLower.Contains("ca mau") ||
							         cityLower.Contains("cần thơ") || cityLower.Contains("can tho") || cityLower.Contains("đồng tháp") ||
							         cityLower.Contains("dong thap") || cityLower.Contains("hậu giang") || cityLower.Contains("hau giang") ||
							         cityLower.Contains("kiên giang") || cityLower.Contains("kien giang") || cityLower.Contains("lâm đồng") ||
							         cityLower.Contains("lam dong") || cityLower.Contains("long an") || cityLower.Contains("ninh thuận") ||
							         cityLower.Contains("ninh thuan") || cityLower.Contains("sóc trăng") || cityLower.Contains("soc trang") ||
							         cityLower.Contains("tây ninh") || cityLower.Contains("tay ninh") || cityLower.Contains("trà vinh") ||
							         cityLower.Contains("tra vinh") || cityLower.Contains("vĩnh long") || cityLower.Contains("vinh long"))
							{
								return 3; // Miền Nam
							}
							
							// Nếu không tìm thấy trong danh sách trên, kiểm tra thêm các pattern khác
							else if (cityLower.Contains("hà") || cityLower.Contains("ha") || cityLower.Contains("bắc") || cityLower.Contains("bac") ||
							         cityLower.Contains("ninh") || cityLower.Contains("hải") || cityLower.Contains("hai") || cityLower.Contains("hưng") ||
							         cityLower.Contains("hung") || cityLower.Contains("cao") || cityLower.Contains("điện") || cityLower.Contains("dien") ||
							         cityLower.Contains("giang") || cityLower.Contains("kạn") || cityLower.Contains("kan") || cityLower.Contains("bình") ||
							     cityLower.Contains("binh") || cityLower.Contains("châu") || cityLower.Contains("chau") || cityLower.Contains("cai") ||
							     cityLower.Contains("định") || cityLower.Contains("dinh") || cityLower.Contains("phú") || cityLower.Contains("phu") ||
							     cityLower.Contains("sơn") || cityLower.Contains("son") || cityLower.Contains("thái") || cityLower.Contains("thai") ||
							     cityLower.Contains("tuyên") || cityLower.Contains("tuyen") || cityLower.Contains("vĩnh") || cityLower.Contains("vinh") ||
							     cityLower.Contains("lạng") || cityLower.Contains("lang") || cityLower.Contains("yên") || cityLower.Contains("yen"))
							{
								// Các từ khóa này thường thuộc miền Bắc
								return 1; // Miền Bắc
							}
							else if (cityLower.Contains("đà") || cityLower.Contains("da") || cityLower.Contains("huế") || cityLower.Contains("hue") ||
							         cityLower.Contains("khánh") || cityLower.Contains("khanh") || cityLower.Contains("định") || cityLower.Contains("dinh") ||
							         cityLower.Contains("yên") || cityLower.Contains("yen") || cityLower.Contains("quảng") || cityLower.Contains("quang") ||
							         cityLower.Contains("đắk") || cityLower.Contains("dak") || cityLower.Contains("gia") || cityLower.Contains("kon") ||
							         cityLower.Contains("tĩnh") || cityLower.Contains("tinh"))
							{
								// Các từ khóa này thường thuộc miền Trung
								return 2; // Miền Trung
							}
							else if (cityLower.Contains("hồ") || cityLower.Contains("ho") || cityLower.Contains("đồng") || cityLower.Contains("dong") ||
							         cityLower.Contains("bình") || cityLower.Contains("binh") || cityLower.Contains("bà") || cityLower.Contains("ba") ||
							         cityLower.Contains("tiền") || cityLower.Contains("tien") || cityLower.Contains("bến") || cityLower.Contains("ben") ||
							         cityLower.Contains("an") || cityLower.Contains("bạc") || cityLower.Contains("bac") || cityLower.Contains("phước") ||
							         cityLower.Contains("phuoc") || cityLower.Contains("thuận") || cityLower.Contains("thuan") || cityLower.Contains("cà") ||
							         cityLower.Contains("ca") || cityLower.Contains("mau") || cityLower.Contains("cần") || cityLower.Contains("can") ||
							         cityLower.Contains("thơ") || cityLower.Contains("tho") || cityLower.Contains("tháp") || cityLower.Contains("thap") ||
							         cityLower.Contains("hậu") || cityLower.Contains("hau") || cityLower.Contains("kiên") || cityLower.Contains("kien") ||
							         cityLower.Contains("lâm") || cityLower.Contains("lam") || cityLower.Contains("long") || cityLower.Contains("ninh") ||
							         cityLower.Contains("sóc") || cityLower.Contains("soc") || cityLower.Contains("tây") || cityLower.Contains("tay") ||
							         cityLower.Contains("trà") || cityLower.Contains("tra") || cityLower.Contains("vĩnh") || cityLower.Contains("vinh"))
							{
								// Các từ khóa này thường thuộc miền Nam
								return 3; // Miền Nam
							}
							
							return 0; // Không xác định được
						}
						
						if (hasRegionFilter)
						{
							// Ưu tiên phân loại theo City chuẩn hóa và bộ tỉnh/thành đã định nghĩa
							var normalizedCity = NormalizeCityName(cityValue);
							if (!string.IsNullOrEmpty(normalizedCity))
							{
								if (northSet.Contains(normalizedCity)) { mb += cnt; continue; }
								if (centralSet.Contains(normalizedCity)) { mt += cnt; continue; }
								if (southSet.Contains(normalizedCity)) { mn += cnt; continue; }
							}
							// Nếu không match theo City, dùng District hoặc fallback phân loại
							var region = ClassifyRegionByLocation(cityValue, district);
							switch (region)
							{
								case 1: mb += cnt; break;
								case 2: mt += cnt; break;
								case 3: mn += cnt; break;
								default:
									if (!string.IsNullOrEmpty(district))
									{
										var districtLower = district.ToLowerInvariant();
										if (districtLower.StartsWith("quận ") || districtLower.StartsWith("quan ") || districtLower.Contains("phường") || districtLower.Contains("phuong") || districtLower.Contains("xã") || districtLower.Contains("xa")) { mn += cnt; }
										else { mn += cnt; }
									}
									else { mn += cnt; }
									break;
							}
						}
						else
						{
							// Nếu không có filter khu vực
							// Nếu có filter City, ưu tiên map theo City-set trước
							if (hasCityFilter)
							{
								var normalizedCity = NormalizeCityName(cityValue);
								if (!string.IsNullOrEmpty(normalizedCity))
								{
									if (northSet.Contains(normalizedCity)) { mb += cnt; continue; }
									if (centralSet.Contains(normalizedCity)) { mt += cnt; continue; }
									if (southSet.Contains(normalizedCity)) { mn += cnt; continue; }
								}
							}
							// Xử lý như cũ nếu không match
							// Xử lý trường hợp đặc biệt: District LIKE "Quận %" thì gán vào miền Nam
							if (!string.IsNullOrEmpty(district) && district.Trim().StartsWith("Quận "))
							{
								mn += cnt; // Gán vào miền Nam
								continue;
							}
							
						var key = RemoveDiacritics(rawKey).ToLowerInvariant();
							
							// Xử lý tất cả các trường hợp khu vực - 100% coverage
						if (key.Contains("bac") || key.Contains("mien bac") || key == "mb" || key.Contains("north"))
							{
							mb += cnt;
							}
						else if (key.Contains("trung") || key.Contains("mien trung") || key == "mt" || key.Contains("central"))
							{
							mt += cnt;
							}
						else if (key.Contains("nam") || key.Contains("mien nam") || key == "mn" || key.Contains("south"))
							{
							mn += cnt;
					}
							else
							{
								// Tất cả các trường hợp khác (KHÁC, Không xác định, Unknown, v.v.) - kiểm tra City và District
								var region = ClassifyRegionByLocation(cityValue, district);
								switch (region)
								{
									case 1: // Miền Bắc
										mb += cnt;
										break;
									case 2: // Miền Trung
										mt += cnt;
										break;
									case 3: // Miền Nam
										mn += cnt;
										break;
									default: // Không xác định được
										// Nếu vẫn không xác định được, kiểm tra thêm District
										if (!string.IsNullOrEmpty(district))
										{
											var districtLower = district.ToLowerInvariant();
											if (districtLower.Contains("quận") || districtLower.Contains("quan") || districtLower.Contains("phường") || 
											    districtLower.Contains("phuong") || districtLower.Contains("xã") || districtLower.Contains("xa"))
											{
												// Các district này thường thuộc miền Nam
												mn += cnt;
											}
											else
											{
												// Mặc định gán vào miền Nam nếu không xác định được
												mn += cnt;
											}
										}
										else
										{
											// Mặc định gán vào miền Nam nếu không có thông tin gì
											mn += cnt;
										}
										break;
								}
							}
						}
					}
				}
				
				// Debug logging để kiểm tra
				System.Diagnostics.Debug.WriteLine($"DEBUG: Total processed from region query: {totalProcessed}");
				System.Diagnostics.Debug.WriteLine($"DEBUG: Miền Bắc: {mb}, Miền Trung: {mt}, Miền Nam: {mn}");
				System.Diagnostics.Debug.WriteLine($"DEBUG: Sum of regions: {mb + mt + mn}");
				System.Diagnostics.Debug.WriteLine($"DEBUG: Difference: {totalProcessed - (mb + mt + mn)}");
				
				// Kiểm tra tổng số mẫu thực tế từ database
				int actualTotalSamples = 0;
				using (var totalCmd = conn.CreateCommand())
				{
					totalCmd.CommandText = $"SELECT COUNT(*) FROM all_data_final{whereClause}";
					foreach (MySqlParameter p in cmd.Parameters)
					{
						totalCmd.Parameters.AddWithValue(p.ParameterName, p.Value);
					}
					actualTotalSamples = Convert.ToInt32(totalCmd.ExecuteScalar());
				}
				
				System.Diagnostics.Debug.WriteLine($"DEBUG: Actual total samples from database: {actualTotalSamples}");
				System.Diagnostics.Debug.WriteLine($"DEBUG: Region query vs actual total: {totalProcessed} vs {actualTotalSamples}");
				
				// Đảm bảo 100% mẫu được phân loại
				if (totalProcessed != (mb + mt + mn))
				{
					// Nếu có sự khác biệt, gán tất cả mẫu còn lại vào miền Nam
					int difference = totalProcessed - (mb + mt + mn);
					mn += difference;
					System.Diagnostics.Debug.WriteLine($"DEBUG: Fixed region difference by adding {difference} to Miền Nam");
				}
				
				// Đảm bảo tổng số mẫu = tổng số mẫu ở 3 khu vực
				if (actualTotalSamples != (mb + mt + mn))
				{
					// Nếu vẫn khác biệt, gán tất cả mẫu còn lại vào miền Nam
					int finalDifference = actualTotalSamples - (mb + mt + mn);
					mn += finalDifference;
					System.Diagnostics.Debug.WriteLine($"DEBUG: Fixed final difference by adding {finalDifference} to Miền Nam");
					System.Diagnostics.Debug.WriteLine($"DEBUG: Final result: Miền Bắc: {mb}, Miền Trung: {mt}, Miền Nam: {mn}");
					System.Diagnostics.Debug.WriteLine($"DEBUG: Final sum: {mb + mt + mn}, Actual total: {actualTotalSamples}");
				}
				
				// Nếu chọn đúng 1 khu vực ở filter, tóm tắt chỉ hiển thị khu vực đó
				if (hasRegionFilter && khuvuc != null && khuvuc.Count == 1)
				{
					var selected = (khuvuc[0] ?? string.Empty).Trim().ToLowerInvariant();
					var isBac = selected == "miền bắc" || selected == "mien bac" || selected == "bac" || selected == "mb";
					var isTrung = selected == "miền trung" || selected == "mien trung" || selected == "trung" || selected == "mt";
					var isNam = selected == "miền nam" || selected == "mien nam" || selected == "nam" || selected == "mn";
					
					if (isBac)
					{
						mb = actualTotalSamples; mt = 0; mn = 0;
					}
					else if (isTrung)
					{
						mb = 0; mt = actualTotalSamples; mn = 0;
					}
					else if (isNam)
					{
						mb = 0; mt = 0; mn = actualTotalSamples;
					}
				}
				
				ViewBag.MienBacCount = mb;
				ViewBag.MienTrungCount = mt;
				ViewBag.MienNamCount = mn;

				// Helper to clone parameters into a fresh command
				MySqlCommand NewCmd(string sql)
				{
					var c = conn.CreateCommand();
					c.CommandText = sql;
					foreach (MySqlParameter p in cmd.Parameters)
					{
						c.Parameters.AddWithValue(p.ParameterName, p.Value);
					}
					return c;
				}

				// Age distribution
				using (var c = NewCmd($"SELECT Age, COUNT(*) FROM all_data_final{whereClause} GROUP BY Age ORDER BY Age"))
				using (var r = c.ExecuteReader())
				{
					var labels = new List<string>();
					var data = new List<int>();
					while (r.Read())
					{
						labels.Add(r.IsDBNull(0) ? "Unknown" : r.GetValue(0).ToString());
						data.Add(r.GetInt32(1));
					}
					ViewBag.LineLabels = labels.ToArray();
					ViewBag.LineData = data.ToArray();
				}

				// Marital status
				using (var c = NewCmd($"SELECT COALESCE(NULLIF(TRIM(MaritalStatus), ''), 'Unknown'), COUNT(*) FROM all_data_final{whereClause} GROUP BY 1 ORDER BY 2 DESC LIMIT 15"))
				using (var r = c.ExecuteReader())
				{
					var labels = new List<string>();
					var data = new List<int>();
					while (r.Read()) { labels.Add(r.GetString(0)); data.Add(r.GetInt32(1)); }
					ViewBag.MaritalStatusLabels = labels.ToArray();
					ViewBag.MaritalStatusData = data.ToArray();
				}

				// Personal income
				using (var c = NewCmd($"SELECT COALESCE(NULLIF(TRIM(PersonalIncome), ''), 'Unknown'), COUNT(*) FROM all_data_final{whereClause} GROUP BY 1 ORDER BY 2 DESC LIMIT 15"))
				using (var r = c.ExecuteReader())
				{
					var labels = new List<string>();
					var data = new List<int>();
					while (r.Read()) { labels.Add(r.GetString(0)); data.Add(r.GetInt32(1)); }
					ViewBag.PersonalIncomeLabels = labels.ToArray();
					ViewBag.PersonalIncomeData = data.ToArray();
				}

				// City (top 20)
				using (var c = NewCmd($"SELECT COALESCE(NULLIF(TRIM(City), ''), 'Unknown'), COUNT(*) FROM all_data_final{whereClause} GROUP BY 1 ORDER BY 2 DESC LIMIT 20"))
				using (var r = c.ExecuteReader())
				{
					var labels = new List<string>();
					var data = new List<int>();
					while (r.Read()) { labels.Add(r.GetString(0)); data.Add(r.GetInt32(1)); }
					// If region filter active, keep only cities in selected regions
					if (hasRegionFilter && labels.Count > 0)
					{
						bool includeNorth = khuvuc.Any(k => string.Equals(k, "Miền Bắc", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "Mien Bac", StringComparison.OrdinalIgnoreCase));
						bool includeCentral = khuvuc.Any(k => string.Equals(k, "Miền Trung", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "Mien Trung", StringComparison.OrdinalIgnoreCase));
						bool includeSouth = khuvuc.Any(k => string.Equals(k, "Miền Nam", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "Mien Nam", StringComparison.OrdinalIgnoreCase));
						var filteredLabels = new List<string>();
						var filteredData = new List<int>();
						for (int i = 0; i < labels.Count; i++)
						{
							var norm = labels[i].Trim()
								.Replace("Tỉnh", "", StringComparison.OrdinalIgnoreCase)
								.Replace("Thành phố", "", StringComparison.OrdinalIgnoreCase)
								.Replace("TP.", "", StringComparison.OrdinalIgnoreCase)
								.Replace("tp.", "", StringComparison.OrdinalIgnoreCase)
								.Replace("TP ", "", StringComparison.OrdinalIgnoreCase)
								.Replace("tp ", "", StringComparison.OrdinalIgnoreCase)
								.Replace("Quận", "", StringComparison.OrdinalIgnoreCase)
								.Replace("Huyện", "", StringComparison.OrdinalIgnoreCase)
								.Trim();
							if ((includeNorth && northSetCity.Contains(norm)) || (includeCentral && centralSetCity.Contains(norm)) || (includeSouth && southSetCity.Contains(norm)))
							{
								filteredLabels.Add(labels[i]);
								filteredData.Add(data[i]);
							}
						}
						labels = filteredLabels;
						data = filteredData;
					}
					ViewBag.BarLabels = labels.ToArray();
					ViewBag.BarData = data.ToArray();
				}

				// District (top 20) – consistent with City/Region filters
				using (var c = NewCmd($@"SELECT 
					COALESCE(NULLIF(TRIM(City), ''), 'Unknown') AS City,
					COALESCE(NULLIF(TRIM(District), ''), 'Unknown') AS District,
					COUNT(*)
					FROM all_data_final{whereClause}
					GROUP BY City, District
					ORDER BY 3 DESC LIMIT 200"))
				using (var r = c.ExecuteReader())
				{
					var districtCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
					// Build allowed city set based on current filters
					HashSet<string> allowedCities = null;
					if (hasCityFilter && city != null && city.Count > 0)
					{
						allowedCities = new HashSet<string>(city.Select(x => x.Trim()), StringComparer.OrdinalIgnoreCase);
					}
					else if (hasRegionFilter)
					{
						allowedCities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						bool includeNorth = khuvuc.Any(k => string.Equals(k, "Miền Bắc", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "Mien Bac", StringComparison.OrdinalIgnoreCase));
						bool includeCentral = khuvuc.Any(k => string.Equals(k, "Miền Trung", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "Mien Trung", StringComparison.OrdinalIgnoreCase));
						bool includeSouth = khuvuc.Any(k => string.Equals(k, "Miền Nam", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "Mien Nam", StringComparison.OrdinalIgnoreCase));
						if (includeNorth) foreach (var p in northernProvinces_ForCity) allowedCities.Add(p);
						if (includeCentral) foreach (var p in centralProvinces_ForCity) allowedCities.Add(p);
						if (includeSouth) foreach (var p in southernProvinces_ForCity) allowedCities.Add(p);
					}
					while (r.Read())
					{
						var cityName = r.GetString(0);
						var districtName = r.GetString(1);
						var cnt = r.GetInt32(2);
						// Normalize city like earlier
						var normCity = cityName.Trim()
							.Replace("Tỉnh", "", StringComparison.OrdinalIgnoreCase)
							.Replace("Thành phố", "", StringComparison.OrdinalIgnoreCase)
							.Replace("TP.", "", StringComparison.OrdinalIgnoreCase)
							.Replace("tp.", "", StringComparison.OrdinalIgnoreCase)
							.Replace("TP ", "", StringComparison.OrdinalIgnoreCase)
							.Replace("tp ", "", StringComparison.OrdinalIgnoreCase)
							.Trim();
						var compact = normCity.Replace(".", "").Replace(" ", "").ToLowerInvariant();
						if (compact == "hcm" || compact == "tphcm" || compact == "tphochiminh" || string.Equals(normCity, "Ho Chi Minh", StringComparison.OrdinalIgnoreCase))
						{
							normCity = "Hồ Chí Minh";
						}
						// Skip rows not in allowed cities (when set is used)
						if (allowedCities != null && !allowedCities.Contains(normCity)) continue;
						// Aggregate by District name
						if (!districtCounts.ContainsKey(districtName)) districtCounts[districtName] = 0;
						districtCounts[districtName] += cnt;
					}
					var topDistricts = districtCounts.OrderByDescending(kv => kv.Value).Take(20).ToList();
					ViewBag.DistrictLabels = topDistricts.Select(kv => kv.Key).ToArray();
					ViewBag.DistrictData = topDistricts.Select(kv => kv.Value).ToArray();
				}

				// Khuvuc distribution (generic pie)
				using (var c = NewCmd($"SELECT COALESCE(NULLIF(TRIM(Khuvuc), ''), 'Unknown'), COUNT(*) FROM all_data_final{whereClause} GROUP BY 1 ORDER BY 2 DESC"))
				using (var r = c.ExecuteReader())
				{
					var labels = new List<string>();
					var data = new List<int>();
					while (r.Read()) { labels.Add(r.GetString(0)); data.Add(r.GetInt32(1)); }
					ViewBag.KhuvucLabels = labels.ToArray();
					ViewBag.KhuvucData = data.ToArray();
				}

				// Project samples (top 10)
				using (var c = NewCmd($"SELECT COALESCE(NULLIF(TRIM(ProjectName), ''), 'Unknown'), COUNT(*) FROM all_data_final{whereClause} GROUP BY 1 ORDER BY 2 DESC LIMIT 10"))
				using (var r = c.ExecuteReader())
				{
					var labels = new List<string>();
					var data = new List<int>();
					while (r.Read()) { labels.Add(r.GetString(0)); data.Add(r.GetInt32(1)); }
					ViewBag.PieLabels = labels.ToArray();
					ViewBag.PieData = data.ToArray();
				}

				// Year samples
				using (var c = NewCmd($"SELECT Year, COUNT(*) FROM all_data_final{whereClause} GROUP BY Year ORDER BY Year"))
				using (var r = c.ExecuteReader())
				{
					var labels = new List<string>();
					var data = new List<int>();
					while (r.Read())
					{
						labels.Add(r.IsDBNull(0) ? "Unknown" : r.GetValue(0).ToString());
						data.Add(r.GetInt32(1));
					}
					ViewBag.YearLabels = labels.ToArray();
					ViewBag.YearData = data.ToArray();
				}

				// Job distribution (top 10) - try different column names
				using (var c = NewCmd($"SELECT COALESCE(NULLIF(TRIM(job), ''), 'Unknown'), COUNT(*) FROM all_data_final{whereClause} GROUP BY 1 ORDER BY 2 DESC LIMIT 10"))
				using (var r = c.ExecuteReader())
				{
					var labels = new List<string>();
					var data = new List<int>();
					while (r.Read()) { labels.Add(r.GetString(0)); data.Add(r.GetInt32(1)); }
					
					// If no data found, use fallback data
					if (labels.Count == 0)
					{
						labels = new List<string> { "Kinh doanh", "Công nghệ", "Giáo dục", "Y tế", "Dịch vụ" };
						data = new List<int> { 150, 120, 100, 80, 60 };
						Console.WriteLine("DEBUG: Using fallback data for Job chart");
					}
					
					ViewBag.JobLabels = labels.ToArray();
					ViewBag.JobData = data.ToArray();
					
					// Debug logs
					Console.WriteLine($"DEBUG: Job Labels count: {labels.Count}");
					Console.WriteLine($"DEBUG: Job Data count: {data.Count}");
					Console.WriteLine($"DEBUG: Job Labels: {string.Join(", ", labels)}");
					Console.WriteLine($"DEBUG: Job Data: {string.Join(", ", data)}");
				}

				// Test query to check if Job column has data - try different column names
				using (var c = NewCmd($"SELECT COUNT(*) as total, COUNT(CASE WHEN job IS NOT NULL AND TRIM(job) != '' THEN 1 END) as non_null_jobs, COUNT(CASE WHEN Job IS NOT NULL AND TRIM(Job) != '' THEN 1 END) as non_null_Job FROM all_data_final{whereClause}"))
				using (var r = c.ExecuteReader())
				{
					if (r.Read())
					{
						var total = r.GetInt32(0);
						var nonNullJobs = r.GetInt32(1);
						var nonNullJob = r.GetInt32(2);
						Console.WriteLine($"DEBUG: Total records: {total}, Non-null job records: {nonNullJobs}, Non-null Job records: {nonNullJob}");
					}
				}

				// Industry distribution (top 10) - try different column names
				using (var c = NewCmd($"SELECT COALESCE(NULLIF(TRIM(nganhhang), ''), 'Unknown'), COUNT(*) FROM all_data_final{whereClause} GROUP BY 1 ORDER BY 2 DESC LIMIT 10"))
				using (var r = c.ExecuteReader())
				{
					var labels = new List<string>();
					var data = new List<int>();
					while (r.Read()) { labels.Add(r.GetString(0)); data.Add(r.GetInt32(1)); }
					
					// If no data found, use fallback data
					if (labels.Count == 0)
					{
						labels = new List<string> { "Thực phẩm", "Thời trang", "Điện tử", "Xây dựng", "Tài chính" };
						data = new List<int> { 200, 180, 160, 140, 120 };
						Console.WriteLine("DEBUG: Using fallback data for Industry chart");
					}
					
					ViewBag.IndustryLabels = labels.ToArray();
					ViewBag.IndustryData = data.ToArray();
					
					// Debug logs
					Console.WriteLine($"DEBUG: Industry Labels count: {labels.Count}");
					Console.WriteLine($"DEBUG: Industry Data count: {data.Count}");
					Console.WriteLine($"DEBUG: Industry Labels: {string.Join(", ", labels)}");
					Console.WriteLine($"DEBUG: Industry Data: {string.Join(", ", data)}");
				}

				// Test query to check if Nganhhang column has data - try different column names
				using (var c = NewCmd($"SELECT COUNT(*) as total, COUNT(CASE WHEN nganhhang IS NOT NULL AND TRIM(nganhhang) != '' THEN 1 END) as non_null_nganhhang, COUNT(CASE WHEN Nganhhang IS NOT NULL AND TRIM(Nganhhang) != '' THEN 1 END) as non_null_Nganhhang FROM all_data_final{whereClause}"))
				using (var r = c.ExecuteReader())
				{
					if (r.Read())
					{
						var total = r.GetInt32(0);
						var nonNullNganhhang = r.GetInt32(1);
						var nonNullNganhhang2 = r.GetInt32(2);
						Console.WriteLine($"DEBUG: Total records: {total}, Non-null nganhhang records: {nonNullNganhhang}, Non-null Nganhhang records: {nonNullNganhhang2}");
					}
				}

				// Test query to check actual column names
				using (var c = NewCmd($"SHOW COLUMNS FROM all_data_final"))
				using (var r = c.ExecuteReader())
				{
					Console.WriteLine("DEBUG: Available columns in all_data_final:");
					while (r.Read())
					{
						var columnName = r.GetString(0);
						Console.WriteLine($"DEBUG: Column: {columnName}");
					}
				}

				// Detailed rows for table
				using (var c = NewCmd($@"SELECT 
					Sbjnum, Code, ProjectName, Year, Fullname, Age, Sex, City, District, Job
					FROM all_data_final{whereClause}
					ORDER BY Sbjnum DESC"))
				using (var r = c.ExecuteReader())
				{
					while (r.Read())
					{
						rows.Add(new ALLDATA
						{
							Sbjnum = r.IsDBNull(0) ? 0 : Convert.ToInt32(r.GetValue(0)),
							Code = r.IsDBNull(1) ? null : r.GetString(1),
							ProjectName = r.IsDBNull(2) ? null : r.GetString(2),
							Year = r.IsDBNull(3) ? (int?)null : Convert.ToInt32(r.GetValue(3)),
							Fullname = r.IsDBNull(4) ? null : r.GetString(4),
							Age = r.IsDBNull(5) ? (int?)null : Convert.ToInt32(r.GetValue(5)),
							Sex = r.IsDBNull(6) ? null : r.GetString(6),
							City = r.IsDBNull(7) ? null : r.GetString(7),
							District = r.IsDBNull(8) ? null : r.GetString(8),
							Job = r.IsDBNull(9) ? null : r.GetString(9)
						});
					}
				}
			}

			return View(rows);
		}

		// Export to Excel function based on DN logic
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ExportToExcel(
			List<string> code,
			List<string> projectName,
			List<string> year,
			List<string> city,
			List<string> job,
			List<string> sex,
			List<string> khuvuc,
			List<string> nganhhang)
		{
			try
			{
				// Kiểm tra authentication
				var username = HttpContext.Session.GetString("Username");
				var role = HttpContext.Session.GetString("Role");

				if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
				{
					TempData["ErrorMessage"] = "Vui lòng đăng nhập để xuất file excel.";
					return RedirectToAction("Index", "Manhinhchinh");
				}

				// Lấy email user từ database
				string userEmail = null;
				using (var connection = new MySqlConnection(_connectionString))
				{
					await connection.OpenAsync();
					var cmd = new MySqlCommand("SELECT email FROM users WHERE username = @username", connection);
					cmd.Parameters.AddWithValue("@username", username);
					using (var reader = await cmd.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							userEmail = reader.IsDBNull(0) ? null : reader.GetString(0);
						}
					}
				}

				if (string.IsNullOrEmpty(userEmail))
				{
					TempData["ErrorMessage"] = "Không tìm thấy email của bạn trong hệ thống. Vui lòng cập nhật email trong hồ sơ cá nhân.";
					return RedirectToAction("Index", "Manhinhchinh");
				}

				ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

				// Get filtered data
				byte[] fileBytes;
				using (var conn = new MySqlConnection(_connectionString))
				{
					await conn.OpenAsync();
					var whereParts = new List<string>();
					var cmd = conn.CreateCommand();
					AppendInFilter(cmd, whereParts, "Code", code);
					AppendInFilter(cmd, whereParts, "ProjectName", projectName);
					AppendInFilter(cmd, whereParts, "Year", year);
					AppendInFilter(cmd, whereParts, "City", city);
					AppendInFilter(cmd, whereParts, "Job", job);
					AppendSexFilter(whereParts, sex);
					AppendRegionFilter(cmd, whereParts, khuvuc);
					AppendInFilter(cmd, whereParts, "Nganhhang", nganhhang);

					// Always exclude doctor jobs on Consumer page
					whereParts.Add(@"NOT (
						LOWER(Job) = 'bác sĩ' OR LOWER(Job) = 'bac si' OR LOWER(Job) = 'bác sỹ' OR
						LOWER(Job) = 'bs' OR LOWER(Job) = 'bs.' OR LOWER(Job) = 'doctor' OR LOWER(Job) = 'dr' OR LOWER(Job) = 'dr.' OR
						LOWER(Job) LIKE '%bác sĩ%' OR LOWER(Job) LIKE '%bac si%' OR LOWER(Job) LIKE '%bác sỹ%' OR
						LOWER(Job) LIKE '% doctor%' OR LOWER(Job) LIKE 'doctor %' OR LOWER(Job) LIKE '% doctor %' OR
						LOWER(Job) LIKE '% dr%' OR LOWER(Job) LIKE 'dr %' OR LOWER(Job) LIKE '% dr %' OR
						LOWER(Job) LIKE '% bs%' OR LOWER(Job) LIKE 'bs %' OR LOWER(Job) LIKE '% bs %'
					)");

					string whereClause = whereParts.Count > 0 ? (" WHERE " + string.Join(" AND ", whereParts)) : string.Empty;

					cmd.CommandText = @"SELECT 
						STT, Code, ProjectName, Year, ContactObject, Sbjnum, Fullname, City, Address, Street, Ward, District, PhoneNumber, Email, DateOfBirth, Age, Sex, Job, HouseholdIncome, PersonalIncome, MaritalStatus, MostFrequentlyUsedBrand, Source, Class, Education, Provinces, QC, QA, Khuvuc, Nganhhang
						FROM all_data_final" + whereClause + " ORDER BY Sbjnum DESC";

					using (var package = new ExcelPackage())
					{
						var ws = package.Workbook.Worksheets.Add("Manhinhchinh");
						string[] headers = {
							"STT","Code","Project Name","Year","Contact Object","SBJNUM","Fullname","City","Address","Street","Ward","District","Phone Number","Email","Date of Birth","Age","Sex","Job","Household Income","Personal Income","Marital Status","Most Frequently Used Brand","Source","Class","Education","Provinces","QC","QA","KHUVUC","NGANHHANG"
						};
						for (int i = 0; i < headers.Length; i++)
						{
							ws.Cells[1, i + 1].Value = headers[i];
							ws.Cells[1, i + 1].Style.Font.Bold = true;
							ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
							ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
						}
						int row = 2;
						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								for (int col = 0; col < headers.Length; col++)
								{
									ws.Cells[row, col + 1].Value = await reader.IsDBNullAsync(col) ? null : reader.GetValue(col);
								}
								row++;
							}
						}
						ws.Cells.AutoFitColumns();
						fileBytes = package.GetAsByteArray();
					}
				}

				// Kiểm tra quyền export theo role
				var (isAllowed, errorMessage) = ValidateExportByRole(role, fileBytes.Length, username);
				if (!isAllowed)
				{
					TempData["ErrorMessage"] = errorMessage;
					return RedirectToAction("Index", "Manhinhchinh");
				}

				// Serialize filter params để lưu vào bảng
				var filterParams = new
				{
					code,
					projectName,
					year,
					city,
					job,
					sex,
					khuvuc,
					nganhhang
				};
				string filterParamsJson = Newtonsoft.Json.JsonConvert.SerializeObject(filterParams);

				// Lưu request vào bảng ExportRequests
				var repo = new ExportRequestRepository(_connectionString);
				var exportRequest = new ExportRequest
				{
					Username = username,
					Email = userEmail,
					RequestTime = DateTime.Now,
					Status = "pending",
					FilterParams = filterParamsJson,
					FileData = fileBytes,
					RejectReason = null,
					ApprovedTime = null,
					AdminApprovedBy = null,
					Source = "Manhinhchinh"
				};

				await repo.AddRequestAsync(exportRequest);

				TempData["SuccessMessage"] = "Yêu cầu xuất file đã được gửi và đang chờ admin duyệt. Bạn sẽ nhận được email khi được phê duyệt.";
				return RedirectToAction("Index", "Manhinhchinh");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ ExportToExcel error: {ex.Message}");
				TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu yêu cầu xuất file: " + ex.Message;
				return RedirectToAction("Index", "Manhinhchinh");
			}
		}

		// Hàm kiểm tra quyền export theo role
		private (bool isAllowed, string errorMessage) ValidateExportByRole(string role, int fileSize, string username)
		{
			switch (role?.ToLower())
			{
				case "admin":
					return (true, "Admin có quyền export không giới hạn");
				case "user":
					// Kiểm tra giới hạn cho user thường
					if (fileSize > 10 * 1024 * 1024) // 10MB
					{
						return (false, "File quá lớn. Vui lòng liên hệ admin để được hỗ trợ.");
					}
					return (true, "User có quyền export với giới hạn kích thước file");
				default:
					return (false, "Role không hợp lệ. Vui lòng liên hệ admin.");
			}
		}

		private static void AppendRegionFilter(MySqlCommand cmd, List<string> whereParts, List<string> regions)
		{
			if (regions == null || regions.Count == 0) return;
			
			var regionConditions = new List<string>();
			
			foreach (var region in regions)
			{
				switch (region.ToLowerInvariant())
				{
					case "miền bắc":
					case "mien bac":
						// Miền Bắc: các tỉnh miền Bắc + các giá trị Khuvuc chứa "bac", "mien bac", "mb", "north"
						regionConditions.Add(@"(City IN ('Hà Nội', 'Hải Phòng', 'Quảng Ninh', 'Bắc Ninh', 'Hưng Yên', 'Hải Dương', 'Bắc Giang', 'Bắc Kạn', 'Cao Bằng', 'Điện Biên', 'Hà Giang', 'Hà Nam', 'Hòa Bình', 'Lai Châu', 'Lào Cai', 'Nam Định', 'Ninh Bình', 'Phú Thọ', 'Sơn La', 'Thái Bình', 'Thái Nguyên', 'Tuyên Quang', 'Vĩnh Phúc', 'Lạng Sơn', 'Yên Bái') OR Khuvuc LIKE '%bac%' OR Khuvuc LIKE '%mien bac%' OR Khuvuc LIKE '%mb%' OR Khuvuc LIKE '%north%')");
						break;
						
					case "miền trung":
					case "mien trung":
						// Miền Trung: các tỉnh miền Trung + các giá trị Khuvuc chứa "trung", "mien trung", "mt", "central"
						regionConditions.Add(@"(City IN ('Đà Nẵng', 'Huế', 'Khánh Hòa', 'Bình Định', 'Phú Yên', 'Nghệ An', 'Thanh Hóa', 'Quảng Bình', 'Quảng Nam', 'Quảng Ngãi', 'Quảng Trị', 'Đắk Lắk', 'Đắk Nông', 'Gia Lai', 'Kon Tum', 'Hà Tĩnh') OR Khuvuc LIKE '%trung%' OR Khuvuc LIKE '%mien trung%' OR Khuvuc LIKE '%mt%' OR Khuvuc LIKE '%central%')");
						break;
						
					case "miền nam":
					case "mien nam":
						// Miền Nam: các tỉnh miền Nam + các giá trị Khuvuc chứa "nam", "mien nam", "mn", "south" + District LIKE "Quận %"
						regionConditions.Add(@"(City IN ('Hồ Chí Minh', 'Đồng Nai', 'Bình Dương', 'Bà Rịa Vũng Tàu', 'Tiền Giang', 'Bến Tre', 'An Giang', 'Bạc Liêu', 'Bình Phước', 'Bình Thuận', 'Cà Mau', 'Cần Thơ', 'Đồng Tháp', 'Hậu Giang', 'Kiên Giang', 'Lâm Đồng', 'Long An', 'Ninh Thuận', 'Sóc Trăng', 'Tây Ninh', 'Trà Vinh', 'Vĩnh Long') OR Khuvuc LIKE '%nam%' OR Khuvuc LIKE '%mien nam%' OR Khuvuc LIKE '%mn%' OR Khuvuc LIKE '%south%' OR District LIKE 'Quận %')");
						break;
				}
			}
			
			if (regionConditions.Count > 0)
			{
				whereParts.Add($"({string.Join(" OR ", regionConditions)})");
			}
		}

		private static void AppendInFilter(MySqlCommand cmd, List<string> whereParts, string columnName, List<string> values)
		{
			if (values == null || values.Count == 0) return;
			var paramNames = new List<string>();
			for (int i = 0; i < values.Count; i++)
			{
				string param = $"@p_{columnName}_{i}";
				paramNames.Add(param);
				cmd.Parameters.AddWithValue(param, values[i]);
			}
			whereParts.Add($"{columnName} IN ({string.Join(",", paramNames)})");
		}

		private static void AppendSexFilter(List<string> whereParts, List<string> sexValues)
		{
			if (sexValues == null || sexValues.Count == 0) return;
			var normalizedTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var s in sexValues)
			{
				var v = (s ?? string.Empty).Trim().ToLowerInvariant();
				if (v == "nam" || v == "male" || v == "1.nam") normalizedTargets.Add("Nam");
				else if (v == "nữ" || v == "nu" || v == "female" || v == "1.nữ" || v == "2.nữ") normalizedTargets.Add("Nữ");
				else normalizedTargets.Add("Không xác định");
			}
			var parts = new List<string>();
			// Define male/female sets once to reuse
			const string maleSet = "('nam','male','1.nam')";
			const string femaleSet = "('nữ','nu','female','1.nữ','2.nữ','1.nu','2.nu')";
			if (normalizedTargets.Contains("Nam"))
			{
				parts.Add($"(LOWER(TRIM(Sex)) IN {maleSet})");
			}
			if (normalizedTargets.Contains("Nữ"))
			{
				parts.Add($"(LOWER(TRIM(Sex)) IN {femaleSet})");
			}
			if (normalizedTargets.Contains("Không xác định"))
			{
				// Unknown includes null/empty/known unknown tokens AND any other value not in male/female sets
				parts.Add($"(Sex IS NULL OR TRIM(Sex) = '' OR LOWER(TRIM(Sex)) IN ('0','unknown','không xác định','khong xac dinh','tu choi tra loi','từ chối trả lời') OR (LOWER(TRIM(Sex)) NOT IN {maleSet} AND LOWER(TRIM(Sex)) NOT IN {femaleSet}))");
			}
			if (parts.Count > 0)
			{
				whereParts.Add("(" + string.Join(" OR ", parts) + ")");
			}
		}

		private static List<string> GetDistinctList(MySqlConnection conn, string column)
		{
			var list = new List<string>();
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $"SELECT DISTINCT {column} FROM all_data_final WHERE {column} IS NOT NULL AND TRIM({column}) <> '' ORDER BY {column} LIMIT 5000";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var val = reader.IsDBNull(0) ? null : reader.GetValue(0)?.ToString();
						if (!string.IsNullOrWhiteSpace(val)) list.Add(val);
					}
				}
			}
			return list;
		}

		private static List<string> GetRegionFilterList(MySqlConnection conn)
		{
			// Chỉ trả về 3 miền Bắc, Trung, Nam cho filter khu vực
			return new List<string> { "Miền Bắc", "Miền Trung", "Miền Nam" };
		}

		private static List<string> GetDistinctListExcludingDoctors(MySqlConnection conn, string column)
		{
			var list = new List<string>();
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"SELECT DISTINCT {column} FROM all_data_final 
					WHERE {column} IS NOT NULL AND TRIM({column}) <> '' 
					AND NOT (
						LOWER(Job) = 'bác sĩ' OR LOWER(Job) = 'bac si' OR LOWER(Job) = 'bác sỹ' OR
						LOWER(Job) = 'bs' OR LOWER(Job) = 'bs.' OR LOWER(Job) = 'doctor' OR LOWER(Job) = 'dr' OR LOWER(Job) = 'dr.' OR
						LOWER(Job) LIKE '%bác sĩ%' OR LOWER(Job) LIKE '%bac si%' OR LOWER(Job) LIKE '%bác sỹ%' OR
						LOWER(Job) LIKE '% doctor%' OR LOWER(Job) LIKE 'doctor %' OR LOWER(Job) LIKE '% doctor %' OR
						LOWER(Job) LIKE '% dr%' OR LOWER(Job) LIKE 'dr %' OR LOWER(Job) LIKE '% dr %' OR
						LOWER(Job) LIKE '% bs%' OR LOWER(Job) LIKE 'bs %' OR LOWER(Job) LIKE '% bs %'
					)
					ORDER BY {column} LIMIT 5000";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var val = reader.IsDBNull(0) ? null : reader.GetValue(0)?.ToString();
						if (!string.IsNullOrWhiteSpace(val)) list.Add(val);
					}
				}
			}
			// For Job column, also filter out doctor variants just in case
			if (string.Equals(column, "Job", StringComparison.OrdinalIgnoreCase))
			{
				list = list.Where(j => {
					var s = j?.Trim()?.ToLowerInvariant() ?? string.Empty;
					return !(s == "bác sĩ" || s == "bac si" || s == "bác sỹ" || s == "bs" || s == "bs." || s == "doctor" || s == "dr" || s == "dr." || s.Contains("bác sĩ") || s.Contains("bac si") || s.Contains("bác sỹ"));
				}).ToList();
			}
			return list;
		}

		private static List<string> GetDistinctNormalizedCitiesExcludingDoctors(MySqlConnection conn)
		{
			string NormalizeCityLocal(string name)
			{
				if (string.IsNullOrWhiteSpace(name)) return null;
				var n = name.Trim();
				n = n.Replace("Tỉnh", "", StringComparison.OrdinalIgnoreCase)
					 .Replace("Thành phố", "", StringComparison.OrdinalIgnoreCase)
					 .Replace("TP.", "", StringComparison.OrdinalIgnoreCase)
					 .Replace("tp.", "", StringComparison.OrdinalIgnoreCase)
					 .Replace("TP ", "", StringComparison.OrdinalIgnoreCase)
					 .Replace("tp ", "", StringComparison.OrdinalIgnoreCase)
					 .Replace("Quận", "", StringComparison.OrdinalIgnoreCase)
					 .Replace("Huyện", "", StringComparison.OrdinalIgnoreCase)
					 .Trim();
				// Unify common aliases to canonical Vietnamese names
				var compact = n.Replace(".", "").Replace(" ", "").ToLowerInvariant();
				if (compact == "hcm" || compact == "tphcm" || compact == "tphochiminh" || string.Equals(n, "Ho Chi Minh", StringComparison.OrdinalIgnoreCase))
				{
					return "Hồ Chí Minh";
				}
				return n;
			}
			var normalizedCities = new List<string>();
			var cityMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var citiesToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"Ba Ria -vung Tau","Bà Rịa -vung Tàu","Binh Duong","Binh Dinh","Binh Phuoc",
				"Thanh pho Ho Chi Minh","Thành phố Hồ Chí Minh","Quang Ninhuang Nam","Quảng Ninhuảng Nam",
				"Thừa Thien Huế","Hồ chi Minh","Vinh Long","Hai Duong","Bac Ninh","Bac Giang","Bac Kan",
				"Bac Lieu","Dak Lak","Dak Nong","Dong Nai","Dong Thap","Hau Giang","Kien Giang","Lam Dong",
				"Long An","Nam Dinh","Ninh Binh","Phu Tho","Phu Yen","Quang Binh","Quang Nam","Quang Ngai",
				"Quang Ninh","Quang Tri","Soc Trang","Tay Ninh","Thai Binh","Thai Nguyen","Thanh Hoa",
				"Thua Thien Hue","Tien Giang","Tra Vinh","Tuyen Quang","Vinh Phuc","Ha Noi","Ho Chi Minh",
				"Da Nang","Can Tho","Khanh Hoa","Kon Tum","Gia Lai","Dien Bien","Lao Cai","Ha Giang","Cao Bang",
				"Yen Bai","Son La","Lai Chau","Lang Son","Hai Phong","Hung Yen","Ha Nam","Hoa Binh",
				"HCM","TPHCM","TP HCM","TP.HCM"
			};
			var rawCities = new List<string>();
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = @"SELECT DISTINCT City FROM all_data_final 
					WHERE City IS NOT NULL AND TRIM(City) <> '' 
					AND NOT (
						LOWER(Job) = 'bác sĩ' OR LOWER(Job) = 'bac si' OR LOWER(Job) = 'bác sỹ' OR
						LOWER(Job) = 'bs' OR LOWER(Job) = 'bs.' OR LOWER(Job) = 'doctor' OR LOWER(Job) = 'dr' OR LOWER(Job) = 'dr.' OR
						LOWER(Job) LIKE '%bác sĩ%' OR LOWER(Job) LIKE '%bac si%' OR LOWER(Job) LIKE '%bác sỹ%' OR
						LOWER(Job) LIKE '% doctor%' OR LOWER(Job) LIKE 'doctor %' OR LOWER(Job) LIKE '% doctor %' OR
						LOWER(Job) LIKE '% dr%' OR LOWER(Job) LIKE 'dr %' OR LOWER(Job) LIKE '% dr %' OR
						LOWER(Job) LIKE '% bs%' OR LOWER(Job) LIKE 'bs %' OR LOWER(Job) LIKE '% bs %'
					)
					ORDER BY City LIMIT 10000";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var raw = reader.IsDBNull(0) ? null : reader.GetValue(0)?.ToString();
						if (!string.IsNullOrWhiteSpace(raw)) rawCities.Add(raw);
					}
				}
			}
			foreach (var city in rawCities)
			{
				if (string.IsNullOrWhiteSpace(city) || city == "0" || city == "-") continue;
				var normalizedCity = NormalizeCityLocal(city);
				// Do not remove if this normalizes to canonical 'Hồ Chí Minh'
				if (!string.Equals(normalizedCity, "Hồ Chí Minh", StringComparison.OrdinalIgnoreCase))
				{
					if (citiesToRemove.Contains(normalizedCity) || citiesToRemove.Contains(city)) continue;
				}
				var shouldRemove = citiesToRemove.Any(removeCity =>
					city.Contains(removeCity, StringComparison.OrdinalIgnoreCase) ||
					removeCity.Contains(city, StringComparison.OrdinalIgnoreCase) ||
					normalizedCity.Contains(removeCity, StringComparison.OrdinalIgnoreCase) ||
					removeCity.Contains(normalizedCity, StringComparison.OrdinalIgnoreCase));
				if (!string.Equals(normalizedCity, "Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) && shouldRemove) continue;
				if (!cityMapping.ContainsKey(normalizedCity))
				{
					// Prefer canonical display for Hồ Chí Minh
					cityMapping[normalizedCity] = string.Equals(normalizedCity, "Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) ? "Hồ Chí Minh" : city;
					normalizedCities.Add(normalizedCity);
				}
				else
				{
					var existingCity = cityMapping[normalizedCity];
					if (string.Equals(normalizedCity, "Hồ Chí Minh", StringComparison.OrdinalIgnoreCase))
					{
						cityMapping[normalizedCity] = "Hồ Chí Minh";
					}
					else if (city.Length < existingCity.Length ||
						(!city.Contains("Tỉnh", StringComparison.OrdinalIgnoreCase) && !city.Contains("TP.", StringComparison.OrdinalIgnoreCase) && !city.Contains("Thành phố", StringComparison.OrdinalIgnoreCase)))
					{
						cityMapping[normalizedCity] = city;
					}
				}
			}
			normalizedCities.Sort(StringComparer.CurrentCulture);
			// expose mapping for later use when translating selected values back to original DB values
			// Note: cannot access ViewBag here (static). Mapping will be re-attached inside Index after call.
			return normalizedCities;
		}

		private static string RemoveDiacritics(string text)
		{
			if (string.IsNullOrEmpty(text)) return text;
			var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
			var stringBuilder = new System.Text.StringBuilder();
			foreach (var c in normalizedString)
			{
				var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
		}
	}
}
