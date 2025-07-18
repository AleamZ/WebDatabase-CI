using System;

namespace CIResearch.Models
{
    public class ExportRequest
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime RequestTime { get; set; }
        public string Status { get; set; } // pending, approved, rejected
        public string FilterParams { get; set; }
        public byte[] FileData { get; set; } // nullable
        public string RejectReason { get; set; } // nullable
        public DateTime? ApprovedTime { get; set; }
        public string AdminApprovedBy { get; set; } // nullable
    }
}