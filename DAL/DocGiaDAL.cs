using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class DocGiaDAL
    {
        private const string SelectWithUser = @"
            SELECT dg.MaDocGia, dg.MaNguoiDung, dg.NgaySinh, dg.DiaChi, dg.IsDeleted, dg.DeletedAt, dg.DeletedBy, dg.Version,
                   nd.TenDangNhap, nd.HoTen, nd.Email, nd.SoDienThoai
            FROM DocGia dg
            INNER JOIN NguoiDung nd ON dg.MaNguoiDung = nd.MaNguoiDung";

        public List<DocGiaModel> GetAll(bool baoGomDaXoa = false)
        {
            var list = new List<DocGiaModel>();
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithUser;
                if (!baoGomDaXoa)
                {
                    sql += " WHERE ISNULL(dg.IsDeleted, 0) = 0";
                }
                var cmd = new SqlCommand(sql, conn);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapFromReader(reader));
                    }
                }
            }
            return list;
        }

        public DocGiaModel? GetById(int id, bool baoGomDaXoa = false)
        {
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithUser + " WHERE dg.MaDocGia = @id";
                if (!baoGomDaXoa)
                {
                    sql += " AND ISNULL(dg.IsDeleted, 0) = 0";
                }
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapFromReader(reader);
                    }
                }
            }
            return null;
        }

        public DocGiaModel? GetByMaNguoiDung(int maNguoiDung)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithUser + " WHERE dg.MaNguoiDung = @maNguoiDung AND ISNULL(dg.IsDeleted, 0) = 0", conn);
                cmd.Parameters.AddWithValue("@maNguoiDung", maNguoiDung);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapFromReader(reader);
                    }
                }
            }
            return null;
        }

        public int Insert(DocGiaModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"INSERT INTO DocGia (MaNguoiDung, NgaySinh, DiaChi, IsDeleted, Version) 
                                           VALUES (@maNd, @ngaySinh, @diaChi, 0, 1); 
                                           SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@maNd", model.MaNguoiDung);
                cmd.Parameters.AddWithValue("@ngaySinh", (object?)model.NgaySinh?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@diaChi", (object?)model.DiaChi ?? DBNull.Value);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Update(DocGiaModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE DocGia 
                                           SET NgaySinh = @ngaySinh, DiaChi = @diaChi, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaDocGia = @id", conn);
                cmd.Parameters.AddWithValue("@id", model.MaDocGia);
                cmd.Parameters.AddWithValue("@ngaySinh", (object?)model.NgaySinh?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@diaChi", (object?)model.DiaChi ?? DBNull.Value);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Soft-delete
        public bool SoftDelete(int id, string? deletedBy = null)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE DocGia 
                                           SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @deletedBy, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaDocGia = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@deletedBy", (object?)deletedBy ?? DBNull.Value);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Restore
        public bool Restore(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE DocGia 
                                           SET IsDeleted = 0, DeletedAt = NULL, DeletedBy = NULL, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaDocGia = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Hard-delete
        public bool Delete(int id)
        {
            return SoftDelete(id);
        }

        private static DocGiaModel MapFromReader(SqlDataReader reader)
        {
            return new DocGiaModel
            {
                MaDocGia = Convert.ToInt32(reader["MaDocGia"]),
                MaNguoiDung = Convert.ToInt32(reader["MaNguoiDung"]),
                NgaySinh = reader["NgaySinh"] != DBNull.Value ? DateOnly.FromDateTime(Convert.ToDateTime(reader["NgaySinh"])) : null,
                DiaChi = reader["DiaChi"] != DBNull.Value ? reader["DiaChi"].ToString() : null,
                TenDangNhap = reader["TenDangNhap"] != DBNull.Value ? reader["TenDangNhap"].ToString() : null,
                HoTen = reader["HoTen"] != DBNull.Value ? reader["HoTen"].ToString() : null,
                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                SoDienThoai = reader["SoDienThoai"] != DBNull.Value ? reader["SoDienThoai"].ToString() : null,
                IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
                DeletedAt = reader["DeletedAt"] != DBNull.Value ? Convert.ToDateTime(reader["DeletedAt"]) : null,
                DeletedBy = reader["DeletedBy"] != DBNull.Value ? reader["DeletedBy"].ToString() : null,
                Version = reader["Version"] != DBNull.Value ? Convert.ToInt32(reader["Version"]) : 1
            };
        }
    }
}
