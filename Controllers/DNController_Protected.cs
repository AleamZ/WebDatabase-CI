using CIResearch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MySql.Data.MySqlClient;
using System.Collections.Concurrent;

namespace CIResearch.Controllers
[object Object]    /// <summary>
                   /// Protected DN Controller with authentication
                   /// </summary>
public class DNController_Protected : Controller
   [object Object]  private readonly IMemoryCache _cache;
private string _connectionString = Server = localhost; Database=sakila;User=root;Password=1234;
        public DNController_Protected(IMemoryCache cache)
     [object Object] _cache = cache;
        }

/// <summary>
/// Kiểm tra authentication trước khi thực hiện action
/// </summary>
private IActionResult CheckAuthentication()
       [object Object] var username = HttpContext.Session.GetString("Username);
            if (string.IsNullOrEmpty(username))
           [object Object]            return RedirectToAction("Login", LoginRegister);     }
            return null; // Authentication OK
        }

        public async Task<IActionResult> Index(string stt = "",
            List<string>? Nam = null,
            List<string>? MaTinh_Dieutra = null,
            List<string>? Masothue = null,
            List<string>? Loaihinhkte = null,
            List<string>? Vungkinhte = null)
      [object Object]
// Kiểm tra authentication
var authResult = CheckAuthentication();
if (authResult != null) return authResult;

try
[object Object]                // Lấy dữ liệu từ database
                var data = await GetDataFromDatabaseAsync();

// Áp dụng filter
var filteredData = ApplyFilters(data, stt, Nam, MaTinh_Dieutra, Masothue, Loaihinhkte, Vungkinhte);

ViewBag.Data = filteredData;
ViewBag.TotalRecords = data.Count;
ViewBag.FilteredRecords = filteredData.Count;

return View(filteredData);
}
catch (Exception ex)
[object Object]              ViewBag.Error = $Lỗi: { ex.Message}
; return View(new List<QLKH>());
}
        }

        public async Task<IActionResult> ViewRawData(string stt = "",
            List<string>? Nam = null,
            List<string>? MaTinh_Dieutra = null,
            List<string>? Masothue = null,
            List<string>? Loaihinhkte = null,
            List<string>? Vungkinhte = null)
      [object Object]
// Kiểm tra authentication
var authResult = CheckAuthentication();
if (authResult != null) return authResult;

try
[object Object]                // Lấy dữ liệu từ database
                var data = await GetDataFromDatabaseAsync();

// Áp dụng filter
var filteredData = ApplyFilters(data, stt, Nam, MaTinh_Dieutra, Masothue, Loaihinhkte, Vungkinhte);

ViewBag.Data = filteredData;
ViewBag.TotalRecords = data.Count;
ViewBag.FilteredRecords = filteredData.Count;

return View(filteredData);
}
catch (Exception ex)
[object Object]              ViewBag.Error = $Lỗi: { ex.Message}
; return View(new List<QLKH>());
}
        }

        private async Task<List<QLKH>> GetDataFromDatabaseAsync()
       [object Object] var result = new List<QLKH>();

try
[object Object]             using var connection = new MySqlConnection(_connectionString);
await connection.OpenAsync();

var command = new MySqlCommand("SELECT * FROM qlkh LIMIT100);
                using var reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
    [object Object]                   result.Add(new QLKH
        [object Object]                   STT = reader.GetInt32("STT"),
                            Nam = reader.GetInt32("Nam"),
                            Masothue = reader.IsDBNull(Masothue) ? null : reader.GetString("Masothue"),
                            TenDN = reader.IsDBNull("TenDN) ? null : reader.GetString("TenDN"),


                            Loaihinhkte = reader.IsDBNull("Loaihinhkte) ? null : reader.GetString("Loaihinhkte"),


                            MaTinh_Dieutra = reader.IsDBNull("MaTinh_Dieutra) ? null : reader.GetString("MaTinh_Dieutra"),


                            MaHuyen_Dieutra = reader.IsDBNull(MaHuyen_Dieutra) ? null : reader.GetString("MaHuyen_Dieutra"),
                            MaXa_Dieutra = reader.IsDBNull("MaXa_Dieutra) ? null : reader.GetString("MaXa_Dieutra"),


                            Diachi = reader.IsDBNull(Diachi) ? null : reader.GetString("Diachi"),
                            Dienthoai = reader.IsDBNull("Dienthoai) ? null : reader.GetString("Dienthoai"),


                            Email = reader.IsDBNull("Email) ? null : reader.GetString("Email"),


                            Region = reader.IsDBNull(Region) ? null : reader.GetString("Region")
                    });
                }
            }
            catch (Exception ex)
           [object Object] Console.WriteLine($Database error: { ex.Message});   }
            
            return result;
        }

        private List<QLKH> ApplyFilters(List<QLKH> data, string stt, List<string>? nam, List<string>? maTinh, List<string>? masothue, List<string>? loaihinhkte, List<string>? vungkinhte)
       [object Object] var filtered = data.AsEnumerable();

if (!string.IsNullOrEmpty(stt))
    [object Object]                if (int.TryParse(stt, out int sttValue))
    [object Object]                   filtered = filtered.Where(x => x.STT == sttValue);
                }
            }

            if (nam != null && nam.Any())
    [object Object]               var years = nam.Select(n => int.TryParse(n, out int y) ? y : 0).Where(y => y > 0);
if (years.Any())
    [object Object]                   filtered = filtered.Where(x => years.Contains(x.Nam));
                }
            }

            if (maTinh != null && maTinh.Any())
    [object Object]          filtered = filtered.Where(x => !string.IsNullOrEmpty(x.MaTinh_Dieutra) && maTinh.Contains(x.MaTinh_Dieutra));
            }

            if (masothue != null && masothue.Any())
    [object Object]          filtered = filtered.Where(x => !string.IsNullOrEmpty(x.Masothue) && masothue.Contains(x.Masothue));
            }

            if (loaihinhkte != null && loaihinhkte.Any())
    [object Object]          filtered = filtered.Where(x => !string.IsNullOrEmpty(x.Loaihinhkte) && loaihinhkte.Contains(x.Loaihinhkte));
            }

            if (vungkinhte != null && vungkinhte.Any())
    [object Object]          filtered = filtered.Where(x => !string.IsNullOrEmpty(x.Region) && vungkinhte.Contains(x.Region));
            }

            return filtered.ToList();
        }
    }
} 