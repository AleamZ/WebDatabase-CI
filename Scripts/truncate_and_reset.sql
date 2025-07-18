-- Script sử dụng TRUNCATE để xóa dữ liệu và reset AUTO_INCREMENT
USE sakila;

-- TRUNCATE sẽ xóa tất cả dữ liệu và reset AUTO_INCREMENT về 1
TRUNCATE TABLE dn_all;

-- Kiểm tra AUTO_INCREMENT hiện tại
SELECT AUTO_INCREMENT 
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'sakila' 
AND TABLE_NAME = 'dn_all';

-- Kiểm tra số lượng bản ghi (phải là 0)
SELECT COUNT(*) as TotalRecords FROM dn_all; 