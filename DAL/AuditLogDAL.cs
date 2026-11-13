using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class AuditLogDAL
    {
        public int Insert(AuditLogModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"
                    INSERT INTO AuditLog (UserId, TenDangNhap, HanhDong, Bang, KhoaChinh, DuLieuCu, DuLieuMoi, MoTa, ThoiGian, IpAddress)
                    VALUES (@userId, @tenDangNhap, @hanhDong, @bang, @khoaChinh, @duLieuCu, @duLieuMoi, @moTa, @thoiGian, @ipAddress);
                    SELECT SCOPE_IDENTITY();", conn);

                cmd.Parameters.AddWithValue("@userId", (object?)model.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tenDangNhap", (object?)model.TenDangNhap ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@hanhDong", model.HanhDong ?? string.Empty);
                cmd.Parameters.AddWithValue("@bang", model.Bang ?? string.Empty);
                cmd.Parameters.AddWithValue("@khoaChinh", (object?)model.KhoaChinh ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@duLieuCu", (object?)model.DuLieuCu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@duLieuMoi", (object?)model.DuLieuMoi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@moTa", (object?)model.MoTa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@thoiGian", model.ThoiGian);
                cmd.Parameters.AddWithValue("@ipAddress", (object?)model.IpAddress ?? DBNull.Value);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public List<AuditLogModel> GetAll(string? bang = null, string? hanhDong = null, int limit = 100)
        {
            var list = new List<AuditLogModel>();
            using (var conn = KetNoi.GetConnection())
            {
                string sql = "SELECT TOP (@limit) * FROM AuditLog WHERE 1=1";
                if (!string.IsNullOrWhiteSpace(bang))
                    sql += " AND Bang = @bang";
                if (!string.IsNullOrWhiteSpace(hanhDong))
                    sql += " AND HanhDong = @hanhDong";
                sql += " ORDER BY ThoiGian DESC";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@limit", limit > 0 ? limit : 100);
                if (!string.IsNullOrWhiteSpace(bang))
                    cmd.Parameters.AddWithValue("@bang", bang);
                if (!string.IsNullOrWhiteSpace(hanhDong))
                    cmd.Parameters.AddWithValue("@hanhDong", hanhDong);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new AuditLogModel
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            UserId = reader["UserId"] != DBNull.Value ? Convert.ToInt32(reader["UserId"]) : null,
                            TenDangNhap = reader["TenDangNhap"] != DBNull.Value ? reader["TenDangNhap"].ToString() : null,
                            HanhDong = reader["HanhDong"].ToString() ?? string.Empty,
                            Bang = reader["Bang"].ToString() ?? string.Empty,
                            KhoaChinh = reader["KhoaChinh"] != DBNull.Value ? reader["KhoaChinh"].ToString() : null,
                            DuLieuCu = reader["DuLieuCu"] != DBNull.Value ? reader["DuLieuCu"].ToString() : null,
                            DuLieuMoi = reader["DuLieuMoi"] != DBNull.Value ? reader["DuLieuMoi"].ToString() : null,
                            MoTa = reader["MoTa"] != DBNull.Value ? reader["MoTa"].ToString() : null,
                            ThoiGian = Convert.ToDateTime(reader["ThoiGian"]),
                            IpAddress = reader["IpAddress"] != DBNull.Value ? reader["IpAddress"].ToString() : null
                        });
                    }
                }
            }
            return list;
        }
    }
}
