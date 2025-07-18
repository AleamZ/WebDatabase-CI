-- Script để drop và tạo lại bảng dn_all
USE sakila;

-- Drop bảng cũ
DROP TABLE IF EXISTS dn_all;

-- Tạo lại bảng với AUTO_INCREMENT = 1
CREATE TABLE `dn_all` (
  `STT` int NOT NULL AUTO_INCREMENT,
  `TenDN` varchar(255) NOT NULL,
  `Diachi` varchar(500) DEFAULT NULL,
  `MaTinh_Dieutra` varchar(100) DEFAULT NULL,
  `MaHuyen_Dieutra` varchar(100) DEFAULT NULL,
  `MaXa_Dieutra` varchar(100) DEFAULT NULL,
  `DNTB_MaTinh` varchar(100) DEFAULT NULL,
  `DNTB_MaHuyen` varchar(100) DEFAULT NULL,
  `DNTB_MaXa` varchar(100) DEFAULT NULL,
  `Region` varchar(100) DEFAULT NULL,
  `Loaihinhkte` varchar(100) DEFAULT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `Dienthoai` varchar(100) DEFAULT NULL,
  `Nam` int DEFAULT NULL,
  `Masothue` varchar(100) DEFAULT NULL,
  `Vungkinhte` varchar(100) DEFAULT NULL,
  `QUY_MO` varchar(100) DEFAULT NULL,
  `MaNganhC5_Chinh` varchar(100) DEFAULT NULL,
  `TEN_NGANH` varchar(255) DEFAULT NULL,
  `SR_Doanhthu_Thuan_BH_CCDV` decimal(25,2) DEFAULT NULL,
  `SR_Loinhuan_TruocThue` decimal(25,2) DEFAULT NULL,
  `SoLaodong_DauNam` int DEFAULT NULL,
  `SoLaodong_CuoiNam` int DEFAULT NULL,
  `Taisan_Tong_CK` decimal(25,2) DEFAULT NULL,
  `Taisan_Tong_DK` decimal(25,2) DEFAULT NULL,
  PRIMARY KEY (`STT`),
  KEY `idx_dn_masothue` (`Masothue`),
  KEY `idx_dn_nam` (`Nam`),
  KEY `idx_dn_manh_dieutra` (`MaTinh_Dieutra`),
  KEY `idx_dn_loaihinhkte` (`Loaihinhkte`),
  KEY `idx_dn_vungkinhte` (`Vungkinhte`),
  KEY `idx_dn_quy_mo` (`QUY_MO`),
  KEY `idx_dn_ten_nganh` (`TEN_NGANH`),
  KEY `idx_dn_composite_main` (`Nam`,`Masothue`,`MaTinh_Dieutra`),
  KEY `idx_dn_composite_filter` (`Nam`,`Loaihinhkte`,`Vungkinhte`),
  KEY `idx_dn_composite_financial` (`Nam`,`SR_Doanhthu_Thuan_BH_CCDV`,`SR_Loinhuan_TruocThue`),
  KEY `idx_dn_covering_stats` (`Nam`,`Masothue`,`SoLaodong_CuoiNam`,`SR_Doanhthu_Thuan_BH_CCDV`,`Taisan_Tong_CK`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4;

-- Kiểm tra AUTO_INCREMENT hiện tại
SELECT AUTO_INCREMENT 
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'sakila' 
AND TABLE_NAME = 'dn_all';

-- Kiểm tra số lượng bản ghi (phải là 0)
SELECT COUNT(*) as TotalRecords FROM dn_all; 