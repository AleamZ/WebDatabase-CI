using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
namespace CIResearch.Models
{
    public class QLKH
    {
        [Key]
        public int STT { get; set; }
        public string? TenDN { get; set; }
        public string? Diachi { get; set; }
        public string? MaTinh_Dieutra { get; set; }
        public string? MaHuyen_Dieutra { get; set; }
        public string? MaXa_Dieutra { get; set; }
        public string? DNTB_MaTinh { get; set; }
        public string? DNTB_MaHuyen { get; set; }
        public string? DNTB_MaXa { get; set; }
        public string? Region { get; set; }
        public string? Loaihinhkte { get; set; }
        public string? Email { get; set; }
        public string? Dienthoai { get; set; }
        public int? Nam { get; set; }
        public string? Masothue { get; set; }
        public string? Vungkinhte { get; set; }
        public string? QUY_MO { get; set; }
        public string? MaNganhC5_Chinh { get; set; }
        public string? TEN_NGANH { get; set; }
        public decimal? SR_Doanhthu_Thuan_BH_CCDV { get; set; }
        public decimal? SR_Loinhuan_TruocThue { get; set; }
        public int? SoLaodong_DauNam { get; set; }
        public int? SoLaodong_CuoiNam { get; set; }
        public decimal? Taisan_Tong_CK { get; set; }
        public decimal? Taisan_Tong_DK { get; set; }

        // Timestamp fields for tracking
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
