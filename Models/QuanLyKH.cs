using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CIResearch.Models
{
    public partial class QuanLyKH
    {

        [Key]
        public int? Stt { get; set; }

        public string? CodeQuocte { get; set; }

        public string? CodeVietNam { get; set; }

        public string? Team { get; set; }

        public string? TenDuAn { get; set; }

        public string? KhachHang { get; set; }

        public String? Thang { get; set; }

        public int? Sample { get; set; }

        public int? HopDong { get; set; }

        public int? HopDongVAT { get; set; }

        public string? Status { get; set; }

        public int? TamUng { get; set; }

        public string? TinhTrangHopDong { get; set; }

        public int? SoTienConLai { get; set; }

        public int? QuaTangDapVien { get; set; }

        public int? LuongPVV { get; set; }

        public int? LuongGSVvaManage { get; set; }
        public int? ChiPhiQC { get; set; }

        public int? ChiPhiTravel { get; set; }

        public int? ChiPhiKhacOP { get; set; }

        public int? ChiPhiBCH { get; set; }

        public int? Moderator { get; set; }

        public int? Report { get; set; }

        public int? ChiPhiKhacRS { get; set; }


        public int? Translator { get; set; }

        public int? DPScripting { get; set; }

        public int? Coding { get; set; }

        public int? DPTabulation { get; set; }

        public int? ChiPhiKhacDP { get; set; }


        public int? adccording { get; set; }

        public int? Amount { get; set; }

        public int? CommissionClients { get; set; }

        public int? TongChiPhi { get; set; }

        public int? VAT { get; set; }

        public int? AmountAfter { get; set; }

        public int? AmountPercent { get; set; }

        public int? Net { get; set; }

        public int? DoneSalary { get; set; }


        public string? GhiChu { get; set; }


        public DateTime? NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        public string? HopDongYesNo { get; set; }

        public string? NghiemThu { get; set; }

        public int? SoLanThanhToan { get; set; }

        public DateTime? NgayThanhToan { get; set; }

        public String? GhiChuThanhToan { get; set; }

        public int? SoNgayThanhToan { get; set; }

        public DateTime? NgayHoaDonChoThanhToan { get; set; }

        public int? SoTienChoThanhToan { get; set; }

        public string? KiemToanNote { get; set; }







    }
}
