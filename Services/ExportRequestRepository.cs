using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using CIResearch.Models;

namespace CIResearch.Services
{
    public class ExportRequestRepository
    {
        private readonly string _connectionString;
        public ExportRequestRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddRequestAsync(ExportRequest request)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new MySqlCommand(@"INSERT INTO ExportRequests (username, email, request_time, status, filter_params, file_data, reject_reason, approved_time, admin_approved_by)
                VALUES (@username, @email, @request_time, @status, @filter_params, @file_data, @reject_reason, @approved_time, @admin_approved_by);
                SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@username", request.Username);
            cmd.Parameters.AddWithValue("@email", request.Email);
            cmd.Parameters.AddWithValue("@request_time", request.RequestTime);
            cmd.Parameters.AddWithValue("@status", request.Status);
            cmd.Parameters.AddWithValue("@filter_params", request.FilterParams);
            cmd.Parameters.AddWithValue("@file_data", (object)request.FileData ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@reject_reason", (object)request.RejectReason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@approved_time", (object)request.ApprovedTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@admin_approved_by", (object)request.AdminApprovedBy ?? DBNull.Value);
            var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return id;
        }

        public async Task<List<ExportRequest>> GetRequestsByStatusAsync(string status)
        {
            var list = new List<ExportRequest>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new MySqlCommand("SELECT * FROM ExportRequests WHERE status = @status ORDER BY request_time DESC", conn);
            cmd.Parameters.AddWithValue("@status", status);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(Map(reader));
            }
            return list;
        }

        public async Task<ExportRequest> GetByIdAsync(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new MySqlCommand("SELECT * FROM ExportRequests WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return Map(reader);
            return null;
        }

        public async Task UpdateStatusAsync(int id, string status, string admin, DateTime? approvedTime = null, string rejectReason = null)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new MySqlCommand(@"UPDATE ExportRequests SET status = @status, admin_approved_by = @admin, approved_time = @approved_time, reject_reason = @reject_reason WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@admin", (object)admin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@approved_time", (object)approvedTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@reject_reason", (object)rejectReason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        private ExportRequest Map(System.Data.Common.DbDataReader reader)
        {
            return new ExportRequest
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Username = reader.GetString(reader.GetOrdinal("username")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                RequestTime = reader.GetDateTime(reader.GetOrdinal("request_time")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                FilterParams = reader.GetString(reader.GetOrdinal("filter_params")),
                FileData = reader.IsDBNull(reader.GetOrdinal("file_data")) ? null : (byte[])reader[reader.GetOrdinal("file_data")],
                RejectReason = reader.IsDBNull(reader.GetOrdinal("reject_reason")) ? null : reader.GetString(reader.GetOrdinal("reject_reason")),
                ApprovedTime = reader.IsDBNull(reader.GetOrdinal("approved_time")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("approved_time")),
                AdminApprovedBy = reader.IsDBNull(reader.GetOrdinal("admin_approved_by")) ? null : reader.GetString(reader.GetOrdinal("admin_approved_by"))
            };
        }
    }
}