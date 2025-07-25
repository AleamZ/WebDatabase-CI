using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using MySql.Data.MySqlClient;
using System.IO;

namespace CIResearch.Controllers
{
    public class testController : Controller
    {
        private string _connectionString = "Server=localhost;Database=sakila;User=root;Password=123456;";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Lưu file đã tải lên
                string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                Directory.CreateDirectory(uploadDir);
                string filePath = Path.Combine(uploadDir, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Xử lý file Excel
                using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    using (MySqlConnection conn = new MySqlConnection(_connectionString))
                    {
                        conn.Open();

                        // Lấy giá trị STT lớn nhất hiện tại
                        int currentMaxStt = GetMaxStt(conn);

                        for (int row = 2; row <= rowCount; row++) // Bỏ qua tiêu đề
                        {
                            var projectData = new
                            {
                                Stt = currentMaxStt + (row - 1),
                                NamSL = worksheet.Cells[row, 2].Text.Trim(),
                                Maxa_dieutra = worksheet.Cells[row, 3].Text.Trim(),
                                Mahuyen_dieutra = worksheet.Cells[row, 4].Text.Trim(),
                                Matinh_dieutra = worksheet.Cells[row, 5].Text.Trim(),
                                MST = worksheet.Cells[row, 6].Text.Trim(),
                                MST2 = worksheet.Cells[row, 7].Text.Trim(),
                                TenDN = worksheet.Cells[row, 8].Text.Trim(),
                                TinhtrangHD = worksheet.Cells[row, 9].Text.Trim(),
                                LoaihinhKT = worksheet.Cells[row, 10].Text.Trim(),
                                Diachi = worksheet.Cells[row, 11].Text.Trim(),
                                SDT = worksheet.Cells[row, 12].Text.Trim(),
                                Email = worksheet.Cells[row, 13].Text.Trim(),
                                Von_NNTW = worksheet.Cells[row, 14].Text.Trim(),
                                TS_daunam = worksheet.Cells[row, 15].Text.Trim(),
                                TS_Cuoinam = worksheet.Cells[row, 16].Text.Trim(),
                                SoLaodong_DauNam = worksheet.Cells[row, 17].Text.Trim(),
                                SoLaodong_CuoiNam = worksheet.Cells[row, 18].Text.Trim(),
                                SoLaodong_T1_TS = worksheet.Cells[row, 19].Text.Trim(),
                                SoLaodong_T1_Nu = worksheet.Cells[row, 20].Text.Trim(),
                                SoLaodong_T2_TS = worksheet.Cells[row, 21].Text.Trim(),
                                SoLaodong_T2_Nu = worksheet.Cells[row, 22].Text.Trim(),
                                SoLaodong_T3_TS = worksheet.Cells[row, 23].Text.Trim(),
                                SoLaodong_T3_Nu = worksheet.Cells[row, 24].Text.Trim(),
                                SoLaodong_T4_TS = worksheet.Cells[row, 25].Text.Trim(),
                                SoLaodong_T4_Nu = worksheet.Cells[row, 26].Text.Trim(),
                                SoLaodong_T5_TS = worksheet.Cells[row, 27].Text.Trim(),
                                SoLaodong_T5_Nu = worksheet.Cells[row, 28].Text.Trim(),
                                SoLaodong_T6_TS = worksheet.Cells[row, 29].Text.Trim(),
                                SoLaodong_T6_Nu = worksheet.Cells[row, 30].Text.Trim(),
                                SoLaodong_T7_TS = worksheet.Cells[row, 31].Text.Trim(),
                                SoLaodong_T7_Nu = worksheet.Cells[row, 32].Text.Trim(),
                                SoLaodong_T8_TS = worksheet.Cells[row, 33].Text.Trim(),
                                SoLaodong_T8_Nu = worksheet.Cells[row, 34].Text.Trim(),
                                SoLaodong_T9_TS = worksheet.Cells[row, 35].Text.Trim(),
                                SoLaodong_T9_Nu = worksheet.Cells[row, 36].Text.Trim(),
                                SoLaodong_T10_TS = worksheet.Cells[row, 37].Text.Trim(),
                                SoLaodong_T10_Nu = worksheet.Cells[row, 38].Text.Trim(),
                                SoLaodong_T11_TS = worksheet.Cells[row, 39].Text.Trim(),
                                SoLaodong_T11_Nu = worksheet.Cells[row, 40].Text.Trim(),
                                SoLaodong_T12_TS = worksheet.Cells[row, 41].Text.Trim(),
                                SoLaodong_T12_Nu = worksheet.Cells[row, 42].Text.Trim(),
                                Doanhthudauki = worksheet.Cells[row, 43].Text.Trim(),
                                Doanhthucuoiki = worksheet.Cells[row, 44].Text.Trim(),
                                Chiphikhac = worksheet.Cells[row, 45].Text.Trim()
                            };

                            string query = @"INSERT INTO dn_all (
                                STT, NamSL, Maxa_dieutra, Mahuyen_dieutra, Matinh_dieutra, MST, MST2, 
                                TenDN, TinhtrangHD, LoaihinhKT, Diachi, SDT, Email, Von_NNTW, TS_daunam, TS_Cuoinam,
                                SoLaodong_DauNam, SoLaodong_CuoiNam, SoLaodong_T1_TS, SoLaodong_T1_Nu, SoLaodong_T2_TS, SoLaodong_T2_Nu,
                                SoLaodong_T3_TS, SoLaodong_T3_Nu, SoLaodong_T4_TS, SoLaodong_T4_Nu, SoLaodong_T5_TS, SoLaodong_T5_Nu,
                                SoLaodong_T6_TS, SoLaodong_T6_Nu, SoLaodong_T7_TS, SoLaodong_T7_Nu, SoLaodong_T8_TS, SoLaodong_T8_Nu,
                                SoLaodong_T9_TS, SoLaodong_T9_Nu, SoLaodong_T10_TS, SoLaodong_T10_Nu, SoLaodong_T11_TS, SoLaodong_T11_Nu,
                                SoLaodong_T12_TS, SoLaodong_T12_Nu, Doanhthudauki, Doanhthucuoiki, Chiphikhac
                            ) VALUES (
                                @STT, @NamSL, @Maxa_dieutra, @Mahuyen_dieutra, @Matinh_dieutra, @MST, @MST2, 
                                @TenDN, @TinhtrangHD, @LoaihinhKT, @Diachi, @SDT, @Email, @Von_NNTW, @TS_daunam, @TS_Cuoinam,
                                @SoLaodong_DauNam, @SoLaodong_CuoiNam, @SoLaodong_T1_TS, @SoLaodong_T1_Nu, @SoLaodong_T2_TS, @SoLaodong_T2_Nu,
                                @SoLaodong_T3_TS, @SoLaodong_T3_Nu, @SoLaodong_T4_TS, @SoLaodong_T4_Nu, @SoLaodong_T5_TS, @SoLaodong_T5_Nu,
                                @SoLaodong_T6_TS, @SoLaodong_T6_Nu, @SoLaodong_T7_TS, @SoLaodong_T7_Nu, @SoLaodong_T8_TS, @SoLaodong_T8_Nu,
                                @SoLaodong_T9_TS, @SoLaodong_T9_Nu, @SoLaodong_T10_TS, @SoLaodong_T10_Nu, @SoLaodong_T11_TS, @SoLaodong_T11_Nu,
                                @SoLaodong_T12_TS, @SoLaodong_T12_Nu, @Doanhthudauki, @Doanhthucuoiki, @Chiphikhac
                            )";

                            // Thêm các tham số vào câu lệnh SQL
                            using (MySqlCommand cmd = new MySqlCommand(query, conn))
                            {
                                foreach (var prop in projectData.GetType().GetProperties())
                                {
                                    cmd.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(projectData) ?? DBNull.Value);
                                }
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                ViewBag.Message = "File imported successfully!";
            }
            return View();
        }

        private int GetMaxStt(MySqlConnection conn)
        {
            string maxSttQuery = "SELECT COALESCE(MAX(STT), 0) FROM dn_all";
            using (MySqlCommand cmd = new MySqlCommand(maxSttQuery, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
