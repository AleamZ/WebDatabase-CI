
using CIResearch.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace CIResearch.Controllers
{
    public class ThemSuaXoaDuAn : Controller
    {
        private string _connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;";
        public IActionResult Index(string searchUser = "")
        {
            // Lấy danh sách các dự án từ CSDL
            List<QuanLyKH> projects = new List<QuanLyKH>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT * FROM quanlykh WHERE TenDuAn LIKE @SearchUser ORDER BY STT DESC", connection);

                command.Parameters.AddWithValue("@SearchUser", "%" + searchUser + "%");
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var project = new QuanLyKH
                        {
                            Stt = reader.IsDBNull(reader.GetOrdinal("STT")) ? (int?)null : reader.GetInt32("STT"),
                            CodeQuocte = reader.IsDBNull(reader.GetOrdinal("CodeQuocTe")) ? null : reader.GetString("CodeQuocTe"),
                            CodeVietNam = reader.IsDBNull(reader.GetOrdinal("CodeVietnam")) ? null : reader.GetString("CodeVietnam"),
                            Team = reader.IsDBNull(reader.GetOrdinal("Team")) ? null : reader.GetString("Team"),
                            TenDuAn = reader.IsDBNull(reader.GetOrdinal("TenDuAn")) ? null : reader.GetString("TenDuAn"),
                            KhachHang = reader.IsDBNull(reader.GetOrdinal("KhachHang")) ? null : reader.GetString("KhachHang"),
                            Thang = reader.IsDBNull(reader.GetOrdinal("Thang")) ? null : reader.GetString("Thang"),
                            Sample = reader.IsDBNull(reader.GetOrdinal("Sample")) ? (int?)null : reader.GetInt32("Sample"),
                            HopDong = reader.IsDBNull(reader.GetOrdinal("HopDong")) ? (int?)null : reader.GetInt32("HopDong"),
                            HopDongVAT = reader.IsDBNull(reader.GetOrdinal("HopDongVat")) ? (int?)null : reader.GetInt32("HopDongVat"),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString("Status"),
                            TamUng = reader.IsDBNull(reader.GetOrdinal("TamUng")) ? (int?)null : reader.GetInt32("TamUng"),
                            TinhTrangHopDong = reader.IsDBNull(reader.GetOrdinal("TinhTrangHD")) ? null : reader.GetString("TinhTrangHD"),
                            SoTienConLai = reader.IsDBNull(reader.GetOrdinal("SoTienConLai")) ? (int?)null : reader.GetInt32("SoTienConLai"),
                            QuaTangDapVien = reader.IsDBNull(reader.GetOrdinal("QuaTangDapVien")) ? (int?)null : reader.GetInt32("QuaTangDapVien"),
                            LuongPVV = reader.IsDBNull(reader.GetOrdinal("LuongPVV")) ? (int?)null : reader.GetInt32("LuongPVV"),
                            LuongGSVvaManage = reader.IsDBNull(reader.GetOrdinal("LuongGSVvaManage")) ? (int?)null : reader.GetInt32("LuongGSVvaManage"),
                            ChiPhiQC = reader.IsDBNull(reader.GetOrdinal("ChiPhiQC")) ? (int?)null : reader.GetInt32("ChiPhiQC"),
                            ChiPhiTravel = reader.IsDBNull(reader.GetOrdinal("ChiPhiTravel")) ? (int?)null : reader.GetInt32("ChiPhiTravel"),
                            ChiPhiKhacOP = reader.IsDBNull(reader.GetOrdinal("ChiPhiKhacOP")) ? (int?)null : reader.GetInt32("ChiPhiKhacOP"),
                            ChiPhiBCH = reader.IsDBNull(reader.GetOrdinal("ChiPhiBCH")) ? (int?)null : reader.GetInt32("ChiPhiBCH"),
                            Moderator = reader.IsDBNull(reader.GetOrdinal("Moderator")) ? (int?)null : reader.GetInt32("Moderator"),
                            Report = reader.IsDBNull(reader.GetOrdinal("Report")) ? (int?)null : reader.GetInt32("Report"),
                            ChiPhiKhacRS = reader.IsDBNull(reader.GetOrdinal("ChiPhiKhacRS")) ? (int?)null : reader.GetInt32("ChiPhiKhacRS"),
                            Translator = reader.IsDBNull(reader.GetOrdinal("Translator")) ? (int?)null : reader.GetInt32("Translator"),
                            DPScripting = reader.IsDBNull(reader.GetOrdinal("DPScripting")) ? (int?)null : reader.GetInt32("DPScripting"),
                            Coding = reader.IsDBNull(reader.GetOrdinal("Coding")) ? (int?)null : reader.GetInt32("Coding"),
                            DPTabulation = reader.IsDBNull(reader.GetOrdinal("DPTabulation")) ? (int?)null : reader.GetInt32("DPTabulation"),
                            ChiPhiKhacDP = reader.IsDBNull(reader.GetOrdinal("ChiPhiKhacDP")) ? (int?)null : reader.GetInt32("ChiPhiKhacDP"),
                            adccording = reader.IsDBNull(reader.GetOrdinal("adccording")) ? (int?)null : reader.GetInt32("adccording"),
                            Amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? (int?)null : reader.GetInt32("Amount"),
                            CommissionClients = reader.IsDBNull(reader.GetOrdinal("CommissionClients")) ? (int?)null : reader.GetInt32("CommissionClients"),
                            TongChiPhi = reader.IsDBNull(reader.GetOrdinal("TongChiPhi")) ? (int?)null : reader.GetInt32("TongChiPhi"),
                            VAT = reader.IsDBNull(reader.GetOrdinal("VAT")) ? (int?)null : reader.GetInt32("VAT"),
                            AmountAfter = reader.IsDBNull(reader.GetOrdinal("Amountafter")) ? (int?)null : reader.GetInt32("Amountafter"),
                            AmountPercent = reader.IsDBNull(reader.GetOrdinal("AmountPercent")) ? (int?)null : reader.GetInt32("AmountPercent"),
                            Net = reader.IsDBNull(reader.GetOrdinal("Net")) ? (int?)null : reader.GetInt32("Net"),
                            DoneSalary = reader.IsDBNull(reader.GetOrdinal("DoneSalary")) ? (int?)null : reader.GetInt32("DoneSalary"),
                            GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu")) ? null : reader.GetString("GhiChu"),
                            NgayBatDau = reader.IsDBNull(reader.GetOrdinal("NgayBatDau")) ? (DateTime?)null : reader.GetDateTime("NgayBatDau"),
                            NgayKetThuc = reader.IsDBNull(reader.GetOrdinal("NgayKetThuc")) ? (DateTime?)null : reader.GetDateTime("NgayKetThuc"),
                            HopDongYesNo = reader.IsDBNull(reader.GetOrdinal("HopDongYesNo")) ? null : reader.GetString("HopDongYesNo"),
                            NghiemThu = reader.IsDBNull(reader.GetOrdinal("NghiemThu")) ? null : reader.GetString("NghiemThu"),
                            SoLanThanhToan = reader.IsDBNull(reader.GetOrdinal("SoLanThanhToan")) ? (int?)null : reader.GetInt32("SoLanThanhToan"),
                            NgayThanhToan = reader.IsDBNull(reader.GetOrdinal("NgayThanhToan")) ? (DateTime?)null : reader.GetDateTime("NgayThanhToan"),
                            GhiChuThanhToan = reader.IsDBNull(reader.GetOrdinal("GhiChuThanhToan")) ? null : reader.GetString("GhiChuThanhToan"),
                            SoNgayThanhToan = reader.IsDBNull(reader.GetOrdinal("SoNgayThanhToan")) ? (int?)null : reader.GetInt32("SoNgayThanhToan"),
                            NgayHoaDonChoThanhToan = reader.IsDBNull(reader.GetOrdinal("NgayHoaDonChoThanhToan")) ? (DateTime?)null : reader.GetDateTime("NgayHoaDonChoThanhToan"),
                            SoTienChoThanhToan = reader.IsDBNull(reader.GetOrdinal("SoTienChoThanhToan")) ? (int?)null : reader.GetInt32("SoTienChoThanhToan"),
                            KiemToanNote = reader.IsDBNull(reader.GetOrdinal("KiemToanNote")) ? null : reader.GetString("KiemToanNote")

                        };
                        projects.Add(project);
                    }
                }
            }

            ViewBag.SearchUser = searchUser;
            return View(projects);
        }




        [HttpPost]
        public IActionResult Create(QuanLyKH newProject)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new MySqlCommand(
                        "INSERT INTO quanlyKH (CodeQuocte, CodeVietnam, Team, TenDuAn, KhachHang, Thang, Sample, HopDong, HopDongVAT, Status, TamUng, TinhTrangHD, SoTienConLai, QuaTangDapVien, LuongPVV, LuongGSVvaManage, ChiPhiQC, ChiPhiTravel, ChiPhiKhacOP, ChiPhiBCH, Moderator, Report, ChiPhiKhacRS, Translator, DPScripting, Coding, DPTabulation, ChiPhiKhacDP, adccording, Amount, CommissionClients, TongChiPhi, VAT, Amountafter, AmountPercent, Net, DoneSalary, GhiChu, NgayBatDau, NgayKetThuc, HopDongYesNo, NghiemThu, SoLanThanhToan, NgayThanhToan, GhiChuThanhToan, SoNgayThanhToan, NgayHoaDonChoThanhToan, SoTienChoThanhToan, KiemToanNote) " +
                        "VALUES (@CodeQuocte, @CodeVietnam, @Team, @TenDuAn, @KhachHang, @Thang, @Sample, @HopDong, @HopDongVAT, @Status, @TamUng, @TinhTrangHD, @SoTienConLai, @QuaTangDapVien, @LuongPVV, @LuongGSVvaManage, @ChiPhiQC, @ChiPhiTravel, @ChiPhiKhacOP, @ChiPhiBCH, @Moderator, @Report, @ChiPhiKhacRS, @Translator, @DPScripting, @Coding, @DPTabulation, @ChiPhiKhacDP, @adccording, @Amount, @CommissionClients, @TongChiPhi, @VAT, @Amountafter, @AmountPercent, @Net, @DoneSalary, @GhiChu, @NgayBatDau, @NgayKetThuc, @HopDongYesNo, @NghiemThu, @SoLanThanhToan, @NgayThanhToan, @GhiChuThanhToan, @SoNgayThanhToan, @NgayHoaDonChoThanhToan, @SoTienChoThanhToan, @KiemToanNote)",
                        connection);

                    // Thêm các tham số vào câu lệnh SQL
                    command.Parameters.AddWithValue("@CodeQuocte", newProject.CodeQuocte);
                    command.Parameters.AddWithValue("@CodeVietnam", newProject.CodeVietNam); // Đảm bảo tên tham số đúng
                    command.Parameters.AddWithValue("@Team", newProject.Team);
                    command.Parameters.AddWithValue("@TenDuAn", newProject.TenDuAn);
                    command.Parameters.AddWithValue("@KhachHang", newProject.KhachHang);
                    command.Parameters.AddWithValue("@Thang", newProject.Thang);
                    command.Parameters.AddWithValue("@Sample", newProject.Sample);
                    command.Parameters.AddWithValue("@HopDong", newProject.HopDong);
                    command.Parameters.AddWithValue("@HopDongVAT", newProject.HopDongVAT);
                    command.Parameters.AddWithValue("@Status", newProject.Status);
                    command.Parameters.AddWithValue("@TamUng", newProject.TamUng);
                    command.Parameters.AddWithValue("@TinhTrangHD", newProject.TinhTrangHopDong);
                    command.Parameters.AddWithValue("@SoTienConLai", newProject.SoTienConLai); // Thêm tham số SoTienConLai
                    command.Parameters.AddWithValue("@QuaTangDapVien", newProject.QuaTangDapVien);
                    command.Parameters.AddWithValue("@LuongPVV", newProject.LuongPVV);
                    command.Parameters.AddWithValue("@LuongGSVvaManage", newProject.LuongGSVvaManage);
                    command.Parameters.AddWithValue("@ChiPhiQC", newProject.ChiPhiQC);
                    command.Parameters.AddWithValue("@ChiPhiTravel", newProject.ChiPhiTravel);
                    command.Parameters.AddWithValue("@ChiPhiKhacOP", newProject.ChiPhiKhacOP);
                    command.Parameters.AddWithValue("@ChiPhiBCH", newProject.ChiPhiBCH);
                    command.Parameters.AddWithValue("@Moderator", newProject.Moderator);
                    command.Parameters.AddWithValue("@Report", newProject.Report);
                    command.Parameters.AddWithValue("@ChiPhiKhacRS", newProject.ChiPhiKhacRS);
                    command.Parameters.AddWithValue("@Translator", newProject.Translator);
                    command.Parameters.AddWithValue("@DPScripting", newProject.DPScripting);
                    command.Parameters.AddWithValue("@Coding", newProject.Coding);
                    command.Parameters.AddWithValue("@DPTabulation", newProject.DPTabulation);
                    command.Parameters.AddWithValue("@ChiPhiKhacDP", newProject.ChiPhiKhacDP); // Thêm tham số ChiPhiKhacDP
                    command.Parameters.AddWithValue("@adccording", newProject.adccording);
                    command.Parameters.AddWithValue("@Amount", newProject.Amount); // Thêm tham số Amount
                    command.Parameters.AddWithValue("@CommissionClients", newProject.CommissionClients); // Thêm tham số CommissionClients
                    command.Parameters.AddWithValue("@TongChiPhi", newProject.TongChiPhi); // Thêm tham số TongChiPhi
                    command.Parameters.AddWithValue("@VAT", newProject.VAT); // Thêm tham số VAT
                    command.Parameters.AddWithValue("@Amountafter", newProject.AmountAfter); // Thêm tham số Amountafter
                    command.Parameters.AddWithValue("@AmountPercent", newProject.AmountPercent); // Thêm tham số AmountPercent
                    command.Parameters.AddWithValue("@Net", newProject.Net); // Thêm tham số Net
                    command.Parameters.AddWithValue("@DoneSalary", newProject.DoneSalary);
                    command.Parameters.AddWithValue("@GhiChu", newProject.GhiChu);
                    command.Parameters.AddWithValue("@NgayBatDau", newProject.NgayBatDau);
                    command.Parameters.AddWithValue("@NgayKetThuc", newProject.NgayKetThuc);
                    command.Parameters.AddWithValue("@HopDongYesNo", newProject.HopDongYesNo);
                    command.Parameters.AddWithValue("@NghiemThu", newProject.NghiemThu);
                    command.Parameters.AddWithValue("@SoLanThanhToan", newProject.SoLanThanhToan);
                    command.Parameters.AddWithValue("@NgayThanhToan", newProject.NgayThanhToan);
                    command.Parameters.AddWithValue("@GhiChuThanhToan", newProject.GhiChuThanhToan);
                    command.Parameters.AddWithValue("@SoNgayThanhToan", newProject.SoNgayThanhToan);
                    command.Parameters.AddWithValue("@NgayHoaDonChoThanhToan", newProject.NgayHoaDonChoThanhToan);
                    command.Parameters.AddWithValue("@SoTienChoThanhToan", newProject.SoTienChoThanhToan);
                    command.Parameters.AddWithValue("@KiemToanNote", newProject.KiemToanNote);

                    // Thực thi câu lệnh
                    command.ExecuteNonQuery();
                }

                return RedirectToAction("Index"); // Trở lại trang danh sách sau khi thêm
            }

            return View(newProject); // Trả về nếu có lỗi trong form
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            QuanLyKH project = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT * FROM quanlykh WHERE STT = @STT", connection);
                command.Parameters.AddWithValue("@STT", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        project = new QuanLyKH
                        {
                            Stt = reader.IsDBNull(reader.GetOrdinal("STT")) ? (int?)null : reader.GetInt32("STT"),
                            CodeQuocte = reader.IsDBNull(reader.GetOrdinal("CodeQuocTe")) ? null : reader.GetString("CodeQuocTe"),
                            CodeVietNam = reader.IsDBNull(reader.GetOrdinal("CodeVietnam")) ? null : reader.GetString("CodeVietnam"),
                            Team = reader.IsDBNull(reader.GetOrdinal("Team")) ? null : reader.GetString("Team"),
                            TenDuAn = reader.IsDBNull(reader.GetOrdinal("TenDuAn")) ? null : reader.GetString("TenDuAn"),
                            KhachHang = reader.IsDBNull(reader.GetOrdinal("KhachHang")) ? null : reader.GetString("KhachHang"),
                            Thang = reader.IsDBNull(reader.GetOrdinal("Thang")) ? null : reader.GetString("Thang"),
                            Sample = reader.IsDBNull(reader.GetOrdinal("Sample")) ? (int?)null : reader.GetInt32("Sample"),
                            HopDong = reader.IsDBNull(reader.GetOrdinal("HopDong")) ? (int?)null : reader.GetInt32("HopDong"),
                            HopDongVAT = reader.IsDBNull(reader.GetOrdinal("HopDongVat")) ? (int?)null : reader.GetInt32("HopDongVat"),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString("Status"),
                            TamUng = reader.IsDBNull(reader.GetOrdinal("TamUng")) ? (int?)null : reader.GetInt32("TamUng"),
                            TinhTrangHopDong = reader.IsDBNull(reader.GetOrdinal("TinhTrangHD")) ? null : reader.GetString("TinhTrangHD"),
                            SoTienConLai = reader.IsDBNull(reader.GetOrdinal("SoTienConLai")) ? (int?)null : reader.GetInt32("SoTienConLai"),
                            QuaTangDapVien = reader.IsDBNull(reader.GetOrdinal("QuaTangDapVien")) ? (int?)null : reader.GetInt32("QuaTangDapVien"),
                            LuongPVV = reader.IsDBNull(reader.GetOrdinal("LuongPVV")) ? (int?)null : reader.GetInt32("LuongPVV"),
                            LuongGSVvaManage = reader.IsDBNull(reader.GetOrdinal("LuongGSVvaManage")) ? (int?)null : reader.GetInt32("LuongGSVvaManage"),
                            ChiPhiQC = reader.IsDBNull(reader.GetOrdinal("ChiPhiQC")) ? (int?)null : reader.GetInt32("ChiPhiQC"),
                            ChiPhiTravel = reader.IsDBNull(reader.GetOrdinal("ChiPhiTravel")) ? (int?)null : reader.GetInt32("ChiPhiTravel"),
                            ChiPhiKhacOP = reader.IsDBNull(reader.GetOrdinal("ChiPhiKhacOP")) ? (int?)null : reader.GetInt32("ChiPhiKhacOP"),
                            ChiPhiBCH = reader.IsDBNull(reader.GetOrdinal("ChiPhiBCH")) ? (int?)null : reader.GetInt32("ChiPhiBCH"),
                            Moderator = reader.IsDBNull(reader.GetOrdinal("Moderator")) ? (int?)null : reader.GetInt32("Moderator"),
                            Report = reader.IsDBNull(reader.GetOrdinal("Report")) ? (int?)null : reader.GetInt32("Report"),
                            ChiPhiKhacRS = reader.IsDBNull(reader.GetOrdinal("ChiPhiKhacRS")) ? (int?)null : reader.GetInt32("ChiPhiKhacRS"),
                            Translator = reader.IsDBNull(reader.GetOrdinal("Translator")) ? (int?)null : reader.GetInt32("Translator"),
                            DPScripting = reader.IsDBNull(reader.GetOrdinal("DPScripting")) ? (int?)null : reader.GetInt32("DPScripting"),
                            Coding = reader.IsDBNull(reader.GetOrdinal("Coding")) ? (int?)null : reader.GetInt32("Coding"),
                            DPTabulation = reader.IsDBNull(reader.GetOrdinal("DPTabulation")) ? (int?)null : reader.GetInt32("DPTabulation"),
                            ChiPhiKhacDP = reader.IsDBNull(reader.GetOrdinal("ChiPhiKhacDP")) ? (int?)null : reader.GetInt32("ChiPhiKhacDP"),
                            adccording = reader.IsDBNull(reader.GetOrdinal("adccording")) ? (int?)null : reader.GetInt32("adccording"),
                            Amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? (int?)null : reader.GetInt32("Amount"),
                            CommissionClients = reader.IsDBNull(reader.GetOrdinal("CommissionClients")) ? (int?)null : reader.GetInt32("CommissionClients"),
                            TongChiPhi = reader.IsDBNull(reader.GetOrdinal("TongChiPhi")) ? (int?)null : reader.GetInt32("TongChiPhi"),
                            VAT = reader.IsDBNull(reader.GetOrdinal("VAT")) ? (int?)null : reader.GetInt32("VAT"),
                            AmountAfter = reader.IsDBNull(reader.GetOrdinal("Amountafter")) ? (int?)null : reader.GetInt32("Amountafter"),
                            AmountPercent = reader.IsDBNull(reader.GetOrdinal("AmountPercent")) ? (int?)null : reader.GetInt32("AmountPercent"),
                            Net = reader.IsDBNull(reader.GetOrdinal("Net")) ? (int?)null : reader.GetInt32("Net"),
                            DoneSalary = reader.IsDBNull(reader.GetOrdinal("DoneSalary")) ? (int?)null : reader.GetInt32("DoneSalary"),
                            GhiChu = reader.IsDBNull(reader.GetOrdinal("GhiChu")) ? null : reader.GetString("GhiChu"),
                            NgayBatDau = reader.IsDBNull(reader.GetOrdinal("NgayBatDau")) ? (DateTime?)null : reader.GetDateTime("NgayBatDau"),
                            NgayKetThuc = reader.IsDBNull(reader.GetOrdinal("NgayKetThuc")) ? (DateTime?)null : reader.GetDateTime("NgayKetThuc"),
                            HopDongYesNo = reader.IsDBNull(reader.GetOrdinal("HopDongYesNo")) ? null : reader.GetString("HopDongYesNo"),
                            NghiemThu = reader.IsDBNull(reader.GetOrdinal("NghiemThu")) ? null : reader.GetString("NghiemThu"),
                            SoLanThanhToan = reader.IsDBNull(reader.GetOrdinal("SoLanThanhToan")) ? (int?)null : reader.GetInt32("SoLanThanhToan"),
                            NgayThanhToan = reader.IsDBNull(reader.GetOrdinal("NgayThanhToan")) ? (DateTime?)null : reader.GetDateTime("NgayThanhToan"),
                            GhiChuThanhToan = reader.IsDBNull(reader.GetOrdinal("GhiChuThanhToan")) ? null : reader.GetString("GhiChuThanhToan"),
                            SoNgayThanhToan = reader.IsDBNull(reader.GetOrdinal("SoNgayThanhToan")) ? (int?)null : reader.GetInt32("SoNgayThanhToan"),
                            NgayHoaDonChoThanhToan = reader.IsDBNull(reader.GetOrdinal("NgayHoaDonChoThanhToan")) ? (DateTime?)null : reader.GetDateTime("NgayHoaDonChoThanhToan"),
                            SoTienChoThanhToan = reader.IsDBNull(reader.GetOrdinal("SoTienChoThanhToan")) ? (int?)null : reader.GetInt32("SoTienChoThanhToan"),
                            KiemToanNote = reader.IsDBNull(reader.GetOrdinal("KiemToanNote")) ? null : reader.GetString("KiemToanNote")
                        };
                    }
                }
            }

            if (project == null)
            {
                return NotFound(); // Nếu không tìm thấy dự án với ID này
            }

            return View(project); // Trả về form chỉnh sửa với dữ liệu của dự án
        }

        // POST: Cập nhật thông tin dự án
        [HttpPost]
        public IActionResult Edit(QuanLyKH updatedProject)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new MySqlCommand("UPDATE quanlykh SET CodeQuocTe = @CodeQuocTe, CodeVietnam = @CodeVietnam, Team = @Team, TenDuAn = @TenDuAn, KhachHang = @KhachHang, Thang = @Thang, Sample = @Sample, HopDong = @HopDong, Status = @Status, TamUng = @TamUng, TinhTrangHD = @TinhTrangHD, QuaTangDapVien = @QuaTangDapVien, LuongPVV = @LuongPVV, LuongGSVvaManage = @LuongGSVvaManage, ChiPhiQC = @ChiPhiQC, ChiPhiTravel = @ChiPhiTravel, ChiPhiKhacOP = @ChiPhiKhacOP, ChiPhiBCH = @ChiPhiBCH, Moderator = @Moderator, Report = @Report, ChiPhiKhacRS = @ChiPhiKhacRS, Translator = @Translator, DPScripting = @DPScripting, Coding = @Coding, DPTabulation = @DPTabulation, adccording = @adccording, Amount = @Amount, CommissionClients = @CommissionClients, TongChiPhi = @TongChiPhi, VAT = @VAT, Amountafter = AmountAfter, AmountPercent = @AmountPercent, Net = @Net, DoneSalary = @DoneSalary, GhiChu = @GhiChu, NgayBatDau = @NgayBatDau, NgayKetThuc = @NgayKetThuc, HopDongYesNo = @HopDongYesNo, NghiemThu = @NghiemThu, SoLanThanhToan = @SoLanThanhToan, NgayThanhToan = @NgayThanhToan, GhiChuThanhToan = @GhiChuThanhToan, SoNgayThanhToan = @SoNgayThanhToan, NgayHoaDonChoThanhToan = @NgayHoaDonChoThanhToan, SoTienChoThanhToan = @SoTienChoThanhToan, KiemToanNote = @KiemToanNote WHERE STT = @STT", connection);

                    // Thêm các tham số vào câu lệnh SQL
                    command.Parameters.AddWithValue("@STT", updatedProject.Stt);
                    command.Parameters.AddWithValue("@CodeQuocTe", updatedProject.CodeQuocte);
                    command.Parameters.AddWithValue("@CodeVietnam", updatedProject.CodeVietNam);
                    command.Parameters.AddWithValue("@Team", updatedProject.Team);
                    command.Parameters.AddWithValue("@TenDuAn", updatedProject.TenDuAn);
                    command.Parameters.AddWithValue("@KhachHang", updatedProject.KhachHang);
                    command.Parameters.AddWithValue("@Thang", updatedProject.Thang);
                    command.Parameters.AddWithValue("@Sample", updatedProject.Sample);
                    command.Parameters.AddWithValue("@HopDong", updatedProject.HopDong);
                    command.Parameters.AddWithValue("@Status", updatedProject.Status);
                    command.Parameters.AddWithValue("@TamUng", updatedProject.TamUng);
                    command.Parameters.AddWithValue("@TinhTrangHD", updatedProject.TinhTrangHopDong);
                    command.Parameters.AddWithValue("@QuaTangDapVien", updatedProject.QuaTangDapVien);
                    command.Parameters.AddWithValue("@LuongPVV", updatedProject.LuongPVV);
                    command.Parameters.AddWithValue("@LuongGSVvaManage", updatedProject.LuongGSVvaManage);
                    command.Parameters.AddWithValue("@ChiPhiQC", updatedProject.ChiPhiQC);
                    command.Parameters.AddWithValue("@ChiPhiTravel", updatedProject.ChiPhiTravel);
                    command.Parameters.AddWithValue("@ChiPhiKhacOP", updatedProject.ChiPhiKhacOP);
                    command.Parameters.AddWithValue("@ChiPhiBCH", updatedProject.ChiPhiBCH);
                    command.Parameters.AddWithValue("@Moderator", updatedProject.Moderator);
                    command.Parameters.AddWithValue("@Report", updatedProject.Report);
                    command.Parameters.AddWithValue("@ChiPhiKhacRS", updatedProject.ChiPhiKhacRS);
                    command.Parameters.AddWithValue("@Translator", updatedProject.Translator);
                    command.Parameters.AddWithValue("@DPScripting", updatedProject.DPScripting);
                    command.Parameters.AddWithValue("@Coding", updatedProject.Coding);
                    command.Parameters.AddWithValue("@DPTabulation", updatedProject.DPTabulation);
                    command.Parameters.AddWithValue("@adccording", updatedProject.adccording);
                    command.Parameters.AddWithValue("@Amount", updatedProject.Amount);
                    command.Parameters.AddWithValue("@CommissionClients", updatedProject.CommissionClients);
                    command.Parameters.AddWithValue("@TongChiPhi", updatedProject.TongChiPhi);
                    command.Parameters.AddWithValue("@VAT", updatedProject.VAT);
                    command.Parameters.AddWithValue("@AmountAfter", updatedProject.AmountAfter);
                    command.Parameters.AddWithValue("@AmountPercent", updatedProject.AmountPercent);
                    command.Parameters.AddWithValue("@Net", updatedProject.Net);
                    command.Parameters.AddWithValue("@DoneSalary", updatedProject.DoneSalary);
                    command.Parameters.AddWithValue("@GhiChu", updatedProject.GhiChu);
                    command.Parameters.AddWithValue("@NgayBatDau", updatedProject.NgayBatDau);
                    command.Parameters.AddWithValue("@NgayKetThuc", updatedProject.NgayKetThuc);
                    command.Parameters.AddWithValue("@HopDongYesNo", updatedProject.HopDongYesNo);
                    command.Parameters.AddWithValue("@NghiemThu", updatedProject.NghiemThu);
                    command.Parameters.AddWithValue("@SoLanThanhToan", updatedProject.SoLanThanhToan);
                    command.Parameters.AddWithValue("@NgayThanhToan", updatedProject.NgayThanhToan);
                    command.Parameters.AddWithValue("@GhiChuThanhToan", updatedProject.GhiChuThanhToan);
                    command.Parameters.AddWithValue("@SoNgayThanhToan", updatedProject.SoNgayThanhToan);
                    command.Parameters.AddWithValue("@NgayHoaDonChoThanhToan", updatedProject.NgayHoaDonChoThanhToan);
                    command.Parameters.AddWithValue("@SoTienChoThanhToan", updatedProject.SoTienChoThanhToan);
                    command.Parameters.AddWithValue("@KiemToanNote", updatedProject.KiemToanNote);

                    command.ExecuteNonQuery(); // Thực thi câu lệnh
                }

                return RedirectToAction("Index"); // Quay lại trang danh sách dự án
            }

            return View(updatedProject); // Nếu dữ liệu không hợp lệ, quay lại form sửa
        }

        // GET: Xóa dự án
        [HttpGet]
        public IActionResult Delete(int id)
        {
            QuanLyKH project = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT * FROM quanlykh WHERE STT = @STT", connection);
                command.Parameters.AddWithValue("@STT", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        project = new QuanLyKH
                        {
                            Stt = reader.GetInt32("STT"),
                            TenDuAn = reader.GetString("TenDuAn")
                            // Các trường khác nếu cần
                        };
                    }
                }
            }

            if (project == null)
            {
                return NotFound(); // Nếu không tìm thấy dự án với ID này
            }

            return View(project); // Trả về trang xác nhận xóa
        }

        // POST: Xóa dự án
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = new MySqlCommand("DELETE FROM quanlykh WHERE STT = @STT", connection);
                command.Parameters.AddWithValue("@STT", id);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    // Dự án đã được xóa thành công
                    return RedirectToAction("Index"); // Hoặc trang khác sau khi xóa
                }
                else
                {
                    // Nếu không xóa được dự án
                    return NotFound();
                }
            }




        }
    }
}
