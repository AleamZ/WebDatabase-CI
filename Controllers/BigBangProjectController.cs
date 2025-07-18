using CIResearch.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace CIResearch.Controllers
{
    public class BigBangProjectController : Controller
    {
        private readonly string _connectionString = "Server=localhost;Database=sakila;User=root;Password=1234";

        public IActionResult Index(
            string stt = "", string code = "", string projectName = "", string year = "",
            string contactObject = "", string sbjnum = "", string fullname = "",
            string city = "", string address = "", string street = "", string ward = "",
            string district = "", string phoneNumber = "", string email = "",
            string dateOfBirth = "", string age = "", string sex = "",
            string job = "", string householdIncome = "", string personalIncome = "",
            string maritalStatus = "", string mostFrequentlyUsedBrand = "",
            string source = "", string className = "", string education = "",
            string provinces = "", string qc = "", string qa = "")
        {
            List<_01Bigbang> project = new List<_01Bigbang>();

            // Ki?m tra xem có b?t k? tham s? nào du?c nh?p không
            if (string.IsNullOrEmpty(stt) && string.IsNullOrEmpty(code) &&
                string.IsNullOrEmpty(projectName) && string.IsNullOrEmpty(year) &&
                string.IsNullOrEmpty(contactObject) && string.IsNullOrEmpty(sbjnum) &&
                string.IsNullOrEmpty(fullname) && string.IsNullOrEmpty(city) &&
                string.IsNullOrEmpty(address) && string.IsNullOrEmpty(street) &&
                string.IsNullOrEmpty(ward) && string.IsNullOrEmpty(district) &&
                string.IsNullOrEmpty(phoneNumber) && string.IsNullOrEmpty(email) &&
                string.IsNullOrEmpty(dateOfBirth) && string.IsNullOrEmpty(age) &&
                string.IsNullOrEmpty(sex) && string.IsNullOrEmpty(job) &&
                string.IsNullOrEmpty(householdIncome) && string.IsNullOrEmpty(personalIncome) &&
                string.IsNullOrEmpty(maritalStatus) && string.IsNullOrEmpty(mostFrequentlyUsedBrand) &&
                string.IsNullOrEmpty(source) && string.IsNullOrEmpty(className) &&
                string.IsNullOrEmpty(education) && string.IsNullOrEmpty(provinces) &&
                string.IsNullOrEmpty(qc) && string.IsNullOrEmpty(qa))
            {
                return View(project); // Ho?c có th? tr? v? m?t thông báo
            }

            project = GetProjects(stt, code, projectName, year, contactObject, sbjnum, fullname, city, address, street, ward, district, phoneNumber, email, dateOfBirth, age, sex, job, householdIncome, personalIncome, maritalStatus, mostFrequentlyUsedBrand, source, className, education, provinces, qc, qa);
            return View(project);
        }

        private List<_01Bigbang> GetProjects(
            string stt, string code, string projectName, string year,
            string contactObject, string sbjnum, string fullname,
            string city, string address, string street, string ward,
            string district, string phoneNumber, string email,
            string dateOfBirth, string age, string sex,
            string job, string householdIncome, string personalIncome,
            string maritalStatus, string mostFrequentlyUsedBrand,
            string source, string className, string education,
            string provinces, string qc, string qa)
        {
            List<_01Bigbang> project = new List<_01Bigbang>();

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM 01_BIGBANG WHERE 1=1";

                // Thêm di?u ki?n l?c cho t?ng tham s?
                if (!string.IsNullOrEmpty(stt)) query += " AND STT = @stt";
                if (!string.IsNullOrEmpty(code)) query += " AND CODE LIKE @code";
                if (!string.IsNullOrEmpty(projectName)) query += " AND PROJECT_NAME LIKE @projectName";
                if (!string.IsNullOrEmpty(year)) query += " AND YEAR = @year";
                if (!string.IsNullOrEmpty(contactObject)) query += " AND CONTACT_OBJECT LIKE @contactObject";
                if (!string.IsNullOrEmpty(sbjnum)) query += " AND SBJNUM = @sbjnum";
                if (!string.IsNullOrEmpty(fullname)) query += " AND FULLNAME LIKE @fullname";
                if (!string.IsNullOrEmpty(city)) query += " AND CITY LIKE @city";
                if (!string.IsNullOrEmpty(address)) query += " AND ADDRESS LIKE @address";
                if (!string.IsNullOrEmpty(street)) query += " AND STREET LIKE @street";
                if (!string.IsNullOrEmpty(ward)) query += " AND WARD LIKE @ward";
                if (!string.IsNullOrEmpty(district)) query += " AND DISTRICT LIKE @district";
                if (!string.IsNullOrEmpty(phoneNumber)) query += " AND PHONE_NUMBER LIKE @phoneNumber";
                if (!string.IsNullOrEmpty(email)) query += " AND EMAIL LIKE @email";
                if (!string.IsNullOrEmpty(dateOfBirth)) query += " AND DATE_OF_BIRTH = @dateOfBirth";
                if (!string.IsNullOrEmpty(age)) query += " AND AGE = @age";
                if (!string.IsNullOrEmpty(sex)) query += " AND SEX LIKE @sex";
                if (!string.IsNullOrEmpty(job)) query += " AND JOB LIKE @job";
                if (!string.IsNullOrEmpty(householdIncome)) query += " AND HOUSEHOLD_INCOME LIKE @householdIncome";
                if (!string.IsNullOrEmpty(personalIncome)) query += " AND PERSONAL_INCOME LIKE @personalIncome";
                if (!string.IsNullOrEmpty(maritalStatus)) query += " AND MARITAL_STATUS LIKE @maritalStatus";
                if (!string.IsNullOrEmpty(mostFrequentlyUsedBrand)) query += " AND MOST_FREQUENTLY_USED_BRAND LIKE @mostFrequentlyUsedBrand";
                if (!string.IsNullOrEmpty(source)) query += " AND SOURCE LIKE @source";
                if (!string.IsNullOrEmpty(className)) query += " AND Class LIKE @className";
                if (!string.IsNullOrEmpty(education)) query += " AND EDUCATION LIKE @education";
                if (!string.IsNullOrEmpty(provinces)) query += " AND PROVINCES LIKE @provinces";
                if (!string.IsNullOrEmpty(qc)) query += " AND QC LIKE @qc";
                if (!string.IsNullOrEmpty(qa)) query += " AND QA LIKE @qa";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Thêm các tham s? vào command
                    if (!string.IsNullOrEmpty(stt)) command.Parameters.AddWithValue("@stt", stt);
                    if (!string.IsNullOrEmpty(code)) command.Parameters.AddWithValue("@code", "%" + code + "%");
                    if (!string.IsNullOrEmpty(projectName)) command.Parameters.AddWithValue("@projectName", "%" + projectName + "%");
                    if (!string.IsNullOrEmpty(year)) command.Parameters.AddWithValue("@year", year);
                    if (!string.IsNullOrEmpty(contactObject)) command.Parameters.AddWithValue("@contactObject", "%" + contactObject + "%");
                    if (!string.IsNullOrEmpty(sbjnum)) command.Parameters.AddWithValue("@sbjnum", sbjnum);
                    if (!string.IsNullOrEmpty(fullname)) command.Parameters.AddWithValue("@fullname", "%" + fullname + "%");
                    if (!string.IsNullOrEmpty(city)) command.Parameters.AddWithValue("@city", "%" + city + "%");
                    if (!string.IsNullOrEmpty(address)) command.Parameters.AddWithValue("@address", "%" + address + "%");
                    if (!string.IsNullOrEmpty(street)) command.Parameters.AddWithValue("@street", "%" + street + "%");
                    if (!string.IsNullOrEmpty(ward)) command.Parameters.AddWithValue("@ward", "%" + ward + "%");
                    if (!string.IsNullOrEmpty(district)) command.Parameters.AddWithValue("@district", "%" + district + "%");
                    if (!string.IsNullOrEmpty(phoneNumber)) command.Parameters.AddWithValue("@phoneNumber", "%" + phoneNumber + "%");
                    if (!string.IsNullOrEmpty(email)) command.Parameters.AddWithValue("@email", "%" + email + "%");
                    if (!string.IsNullOrEmpty(dateOfBirth)) command.Parameters.AddWithValue("@dateOfBirth", dateOfBirth);
                    if (!string.IsNullOrEmpty(age)) command.Parameters.AddWithValue("@age", age);
                    if (!string.IsNullOrEmpty(sex)) command.Parameters.AddWithValue("@sex", "%" + sex + "%");
                    if (!string.IsNullOrEmpty(job)) command.Parameters.AddWithValue("@job", "%" + job + "%");
                    if (!string.IsNullOrEmpty(householdIncome)) command.Parameters.AddWithValue("@householdIncome", "%" + householdIncome + "%");
                    if (!string.IsNullOrEmpty(personalIncome)) command.Parameters.AddWithValue("@personalIncome", "%" + personalIncome + "%");
                    if (!string.IsNullOrEmpty(maritalStatus)) command.Parameters.AddWithValue("@maritalStatus", "%" + maritalStatus + "%");
                    if (!string.IsNullOrEmpty(mostFrequentlyUsedBrand)) command.Parameters.AddWithValue("@mostFrequentlyUsedBrand", "%" + mostFrequentlyUsedBrand + "%");
                    if (!string.IsNullOrEmpty(source)) command.Parameters.AddWithValue("@source", "%" + source + "%");
                    if (!string.IsNullOrEmpty(className)) command.Parameters.AddWithValue("@className", "%" + className + "%");
                    if (!string.IsNullOrEmpty(education)) command.Parameters.AddWithValue("@education", "%" + education + "%");
                    if (!string.IsNullOrEmpty(provinces)) command.Parameters.AddWithValue("@provinces", "%" + provinces + "%");
                    if (!string.IsNullOrEmpty(qc)) command.Parameters.AddWithValue("@qc", "%" + qc + "%");
                    if (!string.IsNullOrEmpty(qa)) command.Parameters.AddWithValue("@qa", "%" + qa + "%");

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _01Bigbang bigbang = new _01Bigbang
                            {
                                Stt = reader.GetInt32("STT"),
                                Code = reader.GetString("CODE"),
                                ProjectName = reader.GetString("PROJECT_NAME"),
                                Year = reader.GetInt32("YEAR"),
                                ContactObject = reader.GetString("CONTACT_OBJECT"),
                                Sbjnum = reader.GetInt32("SBJNUM"),
                                Fullname = reader.GetString("FULLNAME"),
                                City = reader.GetString("CITY"),
                                Address = reader.GetString("ADDRESS"),
                                Street = reader.GetString("STREET"),
                                Ward = reader.GetString("WARD"),
                                District = reader.GetString("DISTRICT"),
                                PhoneNumber = reader.GetString("PHONE_NUMBER"),
                                Email = reader.GetString("EMAIL"),
                                DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DATE_OF_BIRTH")) ? (int?)null : reader.GetInt32("DATE_OF_BIRTH"),
                                Age = reader.GetInt32("AGE"),
                                Sex = reader.GetString("SEX"),
                                Job = reader.GetString("JOB"),
                                HouseholdIncome = reader.GetString("HOUSEHOLD_INCOME"),
                                PersonalIncome = reader.GetString("PERSONAL_INCOME"),
                                MaritalStatus = reader.GetString("MARITAL_STATUS"),
                                MostFrequentlyUsedBrand = reader.GetString("MOST_FREQUENTLY_USED_BRAND"),
                                Source = reader.GetString("SOURCE"),
                                Class = reader.GetString("Class"),
                                Education = reader.GetString("EDUCATION"),
                                Provinces = reader.GetString("PROVINCES"),
                                Qc = reader.GetString("QC"),
                                Qa = reader.GetString("QA")
                            };
                            project.Add(bigbang);
                        }
                    }
                }
            }
            return project;
        }




        public IActionResult ExportToExcel(
    string stt = "", string code = "", string projectName = "", string year = "",
    string contactObject = "", string sbjnum = "", string fullname = "",
    string city = "", string address = "", string street = "", string ward = "",
    string district = "", string phoneNumber = "", string email = "",
    string dateOfBirth = "", string age = "", string sex = "",
    string job = "", string householdIncome = "", string personalIncome = "",
    string maritalStatus = "", string mostFrequentlyUsedBrand = "",
    string source = "", string className = "", string education = "",
    string provinces = "", string qc = "", string qa = "")
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // L?y d? li?u dã l?c t? co s? d? li?u
            var projectList = GetProjects(stt, code, projectName, year, contactObject, sbjnum, fullname, city, address, street, ward, district, phoneNumber, email, dateOfBirth, age, sex, job, householdIncome, personalIncome, maritalStatus, mostFrequentlyUsedBrand, source, className, education, provinces, qc, qa);

            // T?o file Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Projects");

                // T?o header cho các c?t
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Code";
                worksheet.Cells[1, 3].Value = "Project Name";
                worksheet.Cells[1, 4].Value = "Year";
                worksheet.Cells[1, 5].Value = "Contact Object";
                worksheet.Cells[1, 6].Value = "SBJNUM";
                worksheet.Cells[1, 7].Value = "Fullname";
                worksheet.Cells[1, 8].Value = "City";
                worksheet.Cells[1, 9].Value = "Address";
                worksheet.Cells[1, 10].Value = "Street";
                worksheet.Cells[1, 11].Value = "Ward";
                worksheet.Cells[1, 12].Value = "District";
                worksheet.Cells[1, 13].Value = "Phone Number";
                worksheet.Cells[1, 14].Value = "Email";
                worksheet.Cells[1, 15].Value = "Date of Birth";
                worksheet.Cells[1, 16].Value = "Age";
                worksheet.Cells[1, 17].Value = "Sex";
                worksheet.Cells[1, 18].Value = "Job";
                worksheet.Cells[1, 19].Value = "Household Income";
                worksheet.Cells[1, 20].Value = "Personal Income";
                worksheet.Cells[1, 21].Value = "Marital Status";
                worksheet.Cells[1, 22].Value = "Most Frequently Used Brand";
                worksheet.Cells[1, 23].Value = "Source";
                worksheet.Cells[1, 24].Value = "Class";
                worksheet.Cells[1, 25].Value = "Education";
                worksheet.Cells[1, 26].Value = "Provinces";
                worksheet.Cells[1, 27].Value = "QC";
                worksheet.Cells[1, 28].Value = "QA";

                // Ði?n d? li?u vào các hàng
                for (int i = 0; i < projectList.Count; i++)
                {
                    var project = projectList[i];
                    worksheet.Cells[i + 2, 1].Value = project.Stt;
                    worksheet.Cells[i + 2, 2].Value = project.Code;
                    worksheet.Cells[i + 2, 3].Value = project.ProjectName;
                    worksheet.Cells[i + 2, 4].Value = project.Year;
                    worksheet.Cells[i + 2, 5].Value = project.ContactObject;
                    worksheet.Cells[i + 2, 6].Value = project.Sbjnum;
                    worksheet.Cells[i + 2, 7].Value = project.Fullname;
                    worksheet.Cells[i + 2, 8].Value = project.City;
                    worksheet.Cells[i + 2, 9].Value = project.Address;
                    worksheet.Cells[i + 2, 10].Value = project.Street;
                    worksheet.Cells[i + 2, 11].Value = project.Ward;
                    worksheet.Cells[i + 2, 12].Value = project.District;
                    worksheet.Cells[i + 2, 13].Value = project.PhoneNumber;
                    worksheet.Cells[i + 2, 14].Value = project.Email;
                    worksheet.Cells[i + 2, 15].Value = project.DateOfBirth;
                    worksheet.Cells[i + 2, 16].Value = project.Age;
                    worksheet.Cells[i + 2, 17].Value = project.Sex;
                    worksheet.Cells[i + 2, 18].Value = project.Job;
                    worksheet.Cells[i + 2, 19].Value = project.HouseholdIncome;
                    worksheet.Cells[i + 2, 20].Value = project.PersonalIncome;
                    worksheet.Cells[i + 2, 21].Value = project.MaritalStatus;
                    worksheet.Cells[i + 2, 22].Value = project.MostFrequentlyUsedBrand;
                    worksheet.Cells[i + 2, 23].Value = project.Source;
                    worksheet.Cells[i + 2, 24].Value = project.Class;
                    worksheet.Cells[i + 2, 25].Value = project.Education;
                    worksheet.Cells[i + 2, 26].Value = project.Provinces;
                    worksheet.Cells[i + 2, 27].Value = project.Qc;
                    worksheet.Cells[i + 2, 28].Value = project.Qa;
                }

                // Ð?nh d?ng auto-fit cho các c?t
                worksheet.Cells.AutoFitColumns();

                // Tr? file Excel v? cho ngu?i dùng
                var excelData = package.GetAsByteArray();
                var fileName = "Projects.xlsx";
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }



    }
}
