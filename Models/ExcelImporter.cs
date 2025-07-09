
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml; // Cần cài đặt thư viện EPPlus
using MySql.Data.MySqlClient;
namespace CIResearch.Models

{
    public class ExcelImporter
    {
        private readonly string _connectionString;

        public ExcelImporter(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=127.0.0.1;Database=admin_ciresearch;User=admin_dbciresearch;Password=9t52$7sBx;";
        }
        // Hàm để lấy STT lớn nhất từ Excel
        public int GetMaxSttFromExcel(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            int maxStt = 0;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Lấy worksheet đầu tiên
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Bỏ qua hàng tiêu đề
                {
                    int sttValue = worksheet.Cells[row, 1].GetValue<int>(); // Giả sử STT ở cột đầu tiên
                    if (sttValue > maxStt)
                    {
                        maxStt = sttValue;
                    }
                }
            }
            return maxStt;
        }

        public void ImportExcel(string filePath)
        {
            var allDataList = new List<ALLDATA>();

            // Lấy STT lớn nhất từ file Excel
            int currentMaxStt = GetMaxSttFromExcel(filePath);

            // Đọc file Excel và thêm dữ liệu vào danh sách
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Lấy worksheet đầu tiên
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Bỏ qua hàng tiêu đề
                {
                    var allData = new ALLDATA
                    {
                        // Bỏ qua cột STT trong file Excel
                        Code = worksheet.Cells[row, 2].GetValue<string>(),
                        ProjectName = worksheet.Cells[row, 3].GetValue<string>(),
                        Year = worksheet.Cells[row, 4].GetValue<int?>(),
                        ContactObject = worksheet.Cells[row, 5].GetValue<string>(),
                        Sbjnum = worksheet.Cells[row, 6].GetValue<int>(), // Khóa chính
                        Fullname = worksheet.Cells[row, 7].GetValue<string>(),
                        City = worksheet.Cells[row, 8].GetValue<string>(),
                        Address = worksheet.Cells[row, 9].GetValue<string>(),
                        Street = worksheet.Cells[row, 10].GetValue<string>(),
                        Ward = worksheet.Cells[row, 11].GetValue<string>(),
                        District = worksheet.Cells[row, 12].GetValue<string>(),
                        PhoneNumber = worksheet.Cells[row, 13].GetValue<string>(),
                        Email = worksheet.Cells[row, 14].GetValue<string>(),
                        DateOfBirth = worksheet.Cells[row, 15].GetValue<int?>(),
                        Age = worksheet.Cells[row, 16].GetValue<int?>(),
                        Sex = worksheet.Cells[row, 17].GetValue<string>(),
                        Job = worksheet.Cells[row, 18].GetValue<string>(),
                        HouseholdIncome = worksheet.Cells[row, 19].GetValue<string>(),
                        PersonalIncome = worksheet.Cells[row, 20].GetValue<string>(),
                        MaritalStatus = worksheet.Cells[row, 21].GetValue<string>(),
                        MostFrequentlyUsedBrand = worksheet.Cells[row, 22].GetValue<string>(),
                        Source = worksheet.Cells[row, 23].GetValue<string>(),
                        Class = worksheet.Cells[row, 24].GetValue<string>(),
                        Education = worksheet.Cells[row, 25].GetValue<string>(),
                        Provinces = worksheet.Cells[row, 26].GetValue<string>(),
                        Qc = worksheet.Cells[row, 27].GetValue<string>(),
                        Qa = worksheet.Cells[row, 28].GetValue<string>()
                    };

                    allDataList.Add(allData);
                }
            }

            // Nhập dữ liệu vào MySQL
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var data in allDataList)
                {
                    currentMaxStt++; // Tăng giá trị STT mới

                    var cmd = new MySqlCommand("INSERT INTO `01_bigbang` (STT, CODE, PROJECT_NAME, YEAR, CONTACT_OBJECT, SBJNUM, FULLNAME, CITY, ADDRESS, STREET, WARD, DISTRICT, PHONE_NUMBER, EMAIL, DATE_OF_BIRTH, AGE, SEX, JOB, HOUSEHOLD_INCOME, PERSONAL_INCOME, MARITAL_STATUS, MOST_FREQUENTLY_USED_BRAND, SOURCE, Class, EDUCATION, PROVINCES, QC, QA) VALUES (@Stt, @Code, @ProjectName, @Year, @ContactObject, @Sbjnum, @Fullname, @City, @Address, @Street, @Ward, @District, @PhoneNumber, @Email, @DateOfBirth, @Age, @Sex, @Job, @HouseholdIncome, @PersonalIncome, @MaritalStatus, @MostFrequentlyUsedBrand, @Source, @Class, @Education, @Provinces, @Qc, @Qa)", connection);

                    cmd.Parameters.AddWithValue("@Stt", currentMaxStt);
                    cmd.Parameters.AddWithValue("@Code", data.Code);
                    cmd.Parameters.AddWithValue("@ProjectName", data.ProjectName);
                    cmd.Parameters.AddWithValue("@Year", data.Year);
                    cmd.Parameters.AddWithValue("@ContactObject", data.ContactObject);
                    cmd.Parameters.AddWithValue("@Sbjnum", data.Sbjnum);
                    cmd.Parameters.AddWithValue("@Fullname", data.Fullname);
                    cmd.Parameters.AddWithValue("@City", data.City);
                    cmd.Parameters.AddWithValue("@Address", data.Address);
                    cmd.Parameters.AddWithValue("@Street", data.Street);
                    cmd.Parameters.AddWithValue("@Ward", data.Ward);
                    cmd.Parameters.AddWithValue("@District", data.District);
                    cmd.Parameters.AddWithValue("@PhoneNumber", data.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Email", data.Email);
                    cmd.Parameters.AddWithValue("@DateOfBirth", data.DateOfBirth);
                    cmd.Parameters.AddWithValue("@Age", data.Age);
                    cmd.Parameters.AddWithValue("@Sex", data.Sex);
                    cmd.Parameters.AddWithValue("@Job", data.Job);
                    cmd.Parameters.AddWithValue("@HouseholdIncome", data.HouseholdIncome);
                    cmd.Parameters.AddWithValue("@PersonalIncome", data.PersonalIncome);
                    cmd.Parameters.AddWithValue("@MaritalStatus", data.MaritalStatus);
                    cmd.Parameters.AddWithValue("@MostFrequentlyUsedBrand", data.MostFrequentlyUsedBrand);
                    cmd.Parameters.AddWithValue("@Source", data.Source);
                    cmd.Parameters.AddWithValue("@Class", data.Class);
                    cmd.Parameters.AddWithValue("@Education", data.Education);
                    cmd.Parameters.AddWithValue("@Provinces", data.Provinces);
                    cmd.Parameters.AddWithValue("@Qc", data.Qc);
                    cmd.Parameters.AddWithValue("@Qa", data.Qa);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}


