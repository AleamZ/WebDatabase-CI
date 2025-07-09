using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using CIResearch.Models;

namespace CIResearch.Controllers
{
    public class QuanlyKHController : Controller
    {
        private string _connectionString = "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;";

        // Xem danh sách dữ liệu
        public IActionResult Index()
        {
            var accountingData = GetAccountingData();
            return View(accountingData);
        }

        // Lấy dữ liệu từ cơ sở dữ liệu
        private List<QuanLyKH> GetAccountingData()
        {
            var dataList = new List<QuanLyKH>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Quanlykh"; // Thay 'AccountingTable' bằng tên bảng của bạn

                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dataList.Add(new QuanLyKH
                            {
                                Stt = reader.IsDBNull(reader.GetOrdinal("Stt")) ? (int?)null : reader.GetInt32("Stt"),
                                CodeQuocte = reader.IsDBNull(reader.GetOrdinal("CodeQuocte")) ? null : reader.GetString("CodeQuocte"),
                                CodeVietNam = reader.IsDBNull(reader.GetOrdinal("CodeVietNam")) ? null : reader.GetString("CodeVietNam"),
                                Team = reader.IsDBNull(reader.GetOrdinal("Team")) ? null : reader.GetString("Team"),
                                TenDuAn = reader.IsDBNull(reader.GetOrdinal("TenDuAn")) ? null : reader.GetString("TenDuAn"),
                                KhachHang = reader.IsDBNull(reader.GetOrdinal("KhachHang")) ? null : reader.GetString("KhachHang"),
                                Thang = reader.IsDBNull(reader.GetOrdinal("Thang")) ? null : reader.GetString("Thang"),
                                Sample = reader.IsDBNull(reader.GetOrdinal("Sample")) ? (int?)null : reader.GetInt32("Sample"),
                                HopDong = reader.IsDBNull(reader.GetOrdinal("HopDong")) ? (int?)null : reader.GetInt32("HopDong"),
                                HopDongVAT = reader.IsDBNull(reader.GetOrdinal("HopDongVAT")) ? (int?)null : reader.GetInt32("HopDongVAT"),
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
                                AmountAfter = reader.IsDBNull(reader.GetOrdinal("AmountAfter")) ? (int?)null : reader.GetInt32("AmountAfter"),
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

                            });
                        }
                    }
                }
            }
            return dataList;
        }

        // Thêm mới dữ liệu
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(QuanLyKH data)
        {
            if (ModelState.IsValid)
            {
                AddAccountingData(data);
                return RedirectToAction(nameof(Index));
            }
            return View(data);
        }

        // Thêm dữ liệu vào cơ sở dữ liệu
        private void AddAccountingData(QuanLyKH data)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO AccountingTable (CodeQuocte, CodeVietNam, Team, TenDuAn, KhachHang, Thang, Sample, HopDong, Status) 
                                 VALUES (@CodeQuocte, @CodeVietNam, @Team, @TenDuAn, @KhachHang, @Thang, @Sample, @HopDong, @Status)";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CodeQuocte", data.CodeQuocte);
                    command.Parameters.AddWithValue("@CodeVietNam", data.CodeVietNam);
                    command.Parameters.AddWithValue("@Team", data.Team);
                    command.Parameters.AddWithValue("@TenDuAn", data.TenDuAn);
                    command.Parameters.AddWithValue("@KhachHang", data.KhachHang);
                    command.Parameters.AddWithValue("@Thang", data.Thang);
                    command.Parameters.AddWithValue("@Sample", data.Sample ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@HopDong", data.HopDong ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", data.Status);
                    // Add other parameters...

                    command.ExecuteNonQuery();
                }
            }
        }

        // Sửa dữ liệu
        public IActionResult Edit(int id)
        {
            var data = GetAccountingDataById(id);
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(int id, QuanLyKH data)
        {
            if (id != data.Stt)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                UpdateAccountingData(data);
                return RedirectToAction(nameof(Index));
            }
            return View(data);
        }

        // Cập nhật dữ liệu
        private void UpdateAccountingData(QuanLyKH data)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"UPDATE QuanlyKH SET 
                                 CodeQuocte = @CodeQuocte,
                                 CodeVietNam = @CodeVietNam,
                                 Team = @Team,
                                 TenDuAn = @TenDuAn,
                                 KhachHang = @KhachHang,
                                 Thang = @Thang,
                                 Sample = @Sample,
                                 HopDong = @HopDong,
                                 Status = @Status
                                 WHERE Stt = @Stt";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Stt", data.Stt);
                    command.Parameters.AddWithValue("@CodeQuocte", data.CodeQuocte);
                    command.Parameters.AddWithValue("@CodeVietNam", data.CodeVietNam);
                    command.Parameters.AddWithValue("@Team", data.Team);
                    command.Parameters.AddWithValue("@TenDuAn", data.TenDuAn);
                    command.Parameters.AddWithValue("@KhachHang", data.KhachHang);
                    command.Parameters.AddWithValue("@Thang", data.Thang);
                    command.Parameters.AddWithValue("@Sample", data.Sample ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@HopDong", data.HopDong ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", data.Status);
                    // Add other parameters...

                    command.ExecuteNonQuery();
                }
            }
        }

        // Lấy dữ liệu theo Id
        private QuanLyKH GetAccountingDataById(int id)
        {
            QuanLyKH data = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Quanlykh WHERE Stt = @Stt";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Stt", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            data = new QuanLyKH
                            {

                                Stt = reader.IsDBNull(reader.GetOrdinal("Stt")) ? (int?)null : reader.GetInt32("Stt"),
                                CodeQuocte = reader.IsDBNull(reader.GetOrdinal("CodeQuocte")) ? null : reader.GetString("CodeQuocte"),
                                CodeVietNam = reader.IsDBNull(reader.GetOrdinal("CodeVietNam")) ? null : reader.GetString("CodeVietNam"),
                                Team = reader.IsDBNull(reader.GetOrdinal("Team")) ? null : reader.GetString("Team"),
                                TenDuAn = reader.IsDBNull(reader.GetOrdinal("TenDuAn")) ? null : reader.GetString("TenDuAn"),
                                KhachHang = reader.IsDBNull(reader.GetOrdinal("KhachHang")) ? null : reader.GetString("KhachHang"),
                                Thang = reader.IsDBNull(reader.GetOrdinal("Thang")) ? null : reader.GetString("Thang"),
                                Sample = reader.IsDBNull(reader.GetOrdinal("Sample")) ? (int?)null : reader.GetInt32("Sample"),
                                HopDong = reader.IsDBNull(reader.GetOrdinal("HopDong")) ? (int?)null : reader.GetInt32("HopDong"),
                                HopDongVAT = reader.IsDBNull(reader.GetOrdinal("HopDongVAT")) ? (int?)null : reader.GetInt32("HopDongVAT"),
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
                                AmountAfter = reader.IsDBNull(reader.GetOrdinal("AmountAfter")) ? (int?)null : reader.GetInt32("AmountAfter"),
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
            }
            return data;
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID không hợp lệ.");
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Quanlykh WHERE Stt = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["Message"] = "Xóa thành công!";
                    }
                    else
                    {
                        TempData["Message"] = "Không tìm thấy bản ghi để xóa.";
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
