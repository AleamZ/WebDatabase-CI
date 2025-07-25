using CIResearch.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace CIResearch.Controllers
{
    public class AllDataController : Controller
    {
        private readonly string _connectionString = "Server=localhost;Database=sakila;User=root;Password=123456;";



        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Please select a valid file.";
                return View();
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", file.FileName);
            try
            {
                // Lưu file lên server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    using (MySqlConnection conn = new MySqlConnection(_connectionString))
                    {
                        conn.Open();

                        // Lấy giá trị STT lớn nhất hiện có trong bảng
                        int currentMaxStt = GetMaxStt(conn);

                        for (int row = 2; row <= rowCount; row++) // Bỏ qua hàng tiêu đề
                        {
                            var projectData = new ALLDATA
                            {
                                Stt = currentMaxStt + (row - 1),
                                Code = worksheet.Cells[row, 2].Text,
                                ProjectName = worksheet.Cells[row, 3].Text,
                                Year = Convert.ToInt32(worksheet.Cells[row, 4].Text),
                                ContactObject = worksheet.Cells[row, 5].Text,
                                Sbjnum = Convert.ToInt32(worksheet.Cells[row, 6].Text),
                                Fullname = worksheet.Cells[row, 7].Text,
                                City = worksheet.Cells[row, 8].Text,
                                Address = worksheet.Cells[row, 9].Text,
                                Street = worksheet.Cells[row, 10].Text,
                                Ward = worksheet.Cells[row, 11].Text,
                                District = worksheet.Cells[row, 12].Text,
                                PhoneNumber = worksheet.Cells[row, 13].Text,
                                Email = worksheet.Cells[row, 14].Text,
                                DateOfBirth = Convert.ToInt32(worksheet.Cells[row, 15].Text),
                                Age = Convert.ToInt32(worksheet.Cells[row, 16].Text),
                                Sex = worksheet.Cells[row, 17].Text,
                                Job = worksheet.Cells[row, 18].Text,
                                HouseholdIncome = worksheet.Cells[row, 19].Text,
                                PersonalIncome = worksheet.Cells[row, 20].Text,
                                MaritalStatus = worksheet.Cells[row, 21].Text,
                                MostFrequentlyUsedBrand = worksheet.Cells[row, 22].Text,
                                Source = worksheet.Cells[row, 23].Text,
                                Class = worksheet.Cells[row, 24].Text,
                                Education = worksheet.Cells[row, 25].Text,
                                Provinces = worksheet.Cells[row, 26].Text,
                                Qc = worksheet.Cells[row, 27].Text,
                                Qa = worksheet.Cells[row, 28].Text,
                                Khuvuc = worksheet.Cells[row, 29].Text,
                                Nganhhang = worksheet.Cells[row, 30].Text
                            };

                            string query = @"INSERT INTO all_data_final (Stt, Code, ProjectName, Year, ContactObject, Sbjnum, Fullname, City, Address, Street, Ward, District, PhoneNumber, Email, DateOfBirth, Age, Sex, Job, HouseholdIncome, PersonalIncome, MaritalStatus, MostFrequentlyUsedBrand, Source, Class, Education, Provinces, Qc, Qa, Khuvuc, Nganhhang) 
                                            VALUES (@Stt, @Code, @ProjectName, @Year, @ContactObject, @Sbjnum, @Fullname, @City, @Address, @Street, @Ward, @District, @PhoneNumber, @Email, @DateOfBirth, @Age, @Sex, @Job, @HouseholdIncome, @PersonalIncome, @MaritalStatus, @MostFrequentlyUsedBrand, @Source, @Class, @Education, @Provinces, @Qc, @Qa, @Khuvuc, @Nganhhang)";

                            using (MySqlCommand cmd = new MySqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@Stt", projectData.Stt);
                                cmd.Parameters.AddWithValue("@Code", projectData.Code);
                                cmd.Parameters.AddWithValue("@ProjectName", projectData.ProjectName);
                                cmd.Parameters.AddWithValue("@Year", projectData.Year);
                                cmd.Parameters.AddWithValue("@ContactObject", projectData.ContactObject);
                                cmd.Parameters.AddWithValue("@Sbjnum", projectData.Sbjnum);
                                cmd.Parameters.AddWithValue("@Fullname", projectData.Fullname);
                                cmd.Parameters.AddWithValue("@City", projectData.City);
                                cmd.Parameters.AddWithValue("@Address", projectData.Address);
                                cmd.Parameters.AddWithValue("@Street", projectData.Street);
                                cmd.Parameters.AddWithValue("@Ward", projectData.Ward);
                                cmd.Parameters.AddWithValue("@District", projectData.District);
                                cmd.Parameters.AddWithValue("@PhoneNumber", projectData.PhoneNumber);
                                cmd.Parameters.AddWithValue("@Email", projectData.Email);
                                cmd.Parameters.AddWithValue("@DateOfBirth", projectData.DateOfBirth);
                                cmd.Parameters.AddWithValue("@Age", projectData.Age);
                                cmd.Parameters.AddWithValue("@Sex", projectData.Sex);
                                cmd.Parameters.AddWithValue("@Job", projectData.Job);
                                cmd.Parameters.AddWithValue("@HouseholdIncome", projectData.HouseholdIncome);
                                cmd.Parameters.AddWithValue("@PersonalIncome", projectData.PersonalIncome);
                                cmd.Parameters.AddWithValue("@MaritalStatus", projectData.MaritalStatus);
                                cmd.Parameters.AddWithValue("@MostFrequentlyUsedBrand", projectData.MostFrequentlyUsedBrand);
                                cmd.Parameters.AddWithValue("@Source", projectData.Source);
                                cmd.Parameters.AddWithValue("@Class", projectData.Class);
                                cmd.Parameters.AddWithValue("@Education", projectData.Education);
                                cmd.Parameters.AddWithValue("@Provinces", projectData.Provinces);
                                cmd.Parameters.AddWithValue("@Qc", projectData.Qc);
                                cmd.Parameters.AddWithValue("@Qa", projectData.Qa);
                                cmd.Parameters.AddWithValue("@Khuvuc", projectData.Khuvuc);
                                cmd.Parameters.AddWithValue("@Nganhhang", projectData.Nganhhang);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                ViewBag.Message = "File imported successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error: " + ex.Message;
            }

            return View();
        }

        private int GetMaxStt(MySqlConnection conn)
        {
            string maxSttQuery = "SELECT COALESCE(MAX(STT), 0) FROM all_data_final";
            using (MySqlCommand cmd = new MySqlCommand(maxSttQuery, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
