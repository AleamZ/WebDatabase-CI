using Microsoft.AspNetCore.Mvc;
using CIResearch.Models;
using CIResearch.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace CIResearch.Controllers
{
    public class AdminExportRequestController : Controller
    {
        private readonly string _connectionString = "Server=localhost;Database=sakila;User=root;Password=1234;";

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        public async Task<IActionResult> Index(string status = "pending")
        {
            if (!IsAdmin()) return Unauthorized();
            var repo = new ExportRequestRepository(_connectionString);
            var requests = await repo.GetRequestsByStatusAsync(status);
            ViewBag.Status = status;
            return View(requests);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var repo = new ExportRequestRepository(_connectionString);
            var req = await repo.GetByIdAsync(id);
            if (req == null) return NotFound();
            return View(req);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var repo = new ExportRequestRepository(_connectionString);
            var req = await repo.GetByIdAsync(id);
            if (req == null || req.Status != "pending") return NotFound();

            try
            {
                SendEmailWithAttachment_CIResearch(req.Email, "[CIResearch] Dữ liệu doanh nghiệp bạn đã export",
                    $"File dữ liệu doanh nghiệp bạn vừa export đã được duyệt và gửi kèm trong email này.\n\nTrân trọng,\nCIResearch", req.FileData);
                await repo.UpdateStatusAsync(id, "approved", HttpContext.Session.GetString("Username"), DateTime.Now);
                TempData["Success"] = "Đã duyệt và gửi mail thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi gửi mail: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            if (!IsAdmin()) return Unauthorized();
            var repo = new ExportRequestRepository(_connectionString);
            var req = await repo.GetByIdAsync(id);
            if (req == null || req.Status != "pending") return NotFound();
            await repo.UpdateStatusAsync(id, "rejected", HttpContext.Session.GetString("Username"), DateTime.Now, reason);
            TempData["Success"] = "Đã từ chối yêu cầu export.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DownloadFile(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var repo = new ExportRequestRepository(_connectionString);
            var req = await repo.GetByIdAsync(id);
            if (req == null || req.FileData == null)
                return NotFound();
            var fileName = $"Export_{req.Id}_{req.Username}_{req.RequestTime:yyyyMMdd_HHmmss}.xlsx";
            return File(req.FileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // Hàm gửi mail (dùng lại logic cũ)
        private static void SendEmailWithAttachment_CIResearch(string toEmail, string subject, string body, byte[] attachmentData)
        {
            const string fromEmail = "ciresearch.dn@gmail.com";
            const string fromPassword = "mhip zhvj dhpd zrgo";
            using var message = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(fromEmail),
                Subject = subject,
                Body = body
            };
            message.To.Add(new System.Net.Mail.MailAddress(toEmail));
            message.Attachments.Add(new System.Net.Mail.Attachment(new System.IO.MemoryStream(attachmentData),
                "Data_Ciresearch.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            using var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new System.Net.NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };
            client.Send(message);
        }
    }
}