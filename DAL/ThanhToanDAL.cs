using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class ThanhToanDAL
    {
        private const string SelectWithDetails = @"
            SELECT tt.MaThanhToan, tt.MaHoaDon, tt.NgayThanhToan, tt.SoTien, tt.PhuongThuc, tt.TrangThai,
                   tt.IsDeleted, tt.DeletedAt, tt.DeletedBy, tt.Version,
                   hd.MaDangKy,
                   nd.HoTen AS TenDocGia
            FROM ThanhToan tt
            INNER JOIN HoaDon hd ON tt.MaHoaDon = hd.MaHoaDon
            INNER JOIN DangKy dk ON hd.MaDangKy = dk.MaDangKy
            INNER JOIN DocGia dg ON dk.MaDocGia = dg.MaDocGia
            INNER JOIN NguoiDung nd ON dg.MaNguoiDung = nd.MaNguoiDung";

        public List<ThanhToanModel> GetAll(bool baoGomDaXoa = false)
        {
            var list = new List<ThanhToanModel>();
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithDetails;
                if (!baoGomDaXoa)
                {
                    sql += " WHERE ISNULL(tt.IsDeleted, 0) = 0";
                }
                sql += " ORDER BY tt.NgayThanhToan DESC";

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

        public ThanhToanModel? GetById(int id, bool baoGomDaXoa = false)
        {
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithDetails + " WHERE tt.MaThanhToan = @id";
                if (!baoGomDaXoa)
                {
                    sql += " AND ISNULL(tt.IsDeleted, 0) = 0";
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

        public List<ThanhToanModel> GetByHoaDon(int maHoaDon)
        {
            var list = new List<ThanhToanModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + " WHERE tt.MaHoaDon = @maHd AND ISNULL(tt.IsDeleted, 0) = 0", conn);
                cmd.Parameters.AddWithValue("@maHd", maHoaDon);
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

        public List<ThanhToanModel> GetByDocGia(int maDocGia)
        {
            var list = new List<ThanhToanModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + " WHERE dg.MaDocGia = @maDg AND ISNULL(tt.IsDeleted, 0) = 0 ORDER BY tt.NgayThanhToan DESC", conn);
                cmd.Parameters.AddWithValue("@maDg", maDocGia);
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

        public int Insert(ThanhToanModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"INSERT INTO ThanhToan (MaHoaDon, NgayThanhToan, SoTien, PhuongThuc, TrangThai, IsDeleted, Version) 
                                           VALUES (@maHd, @ngayTt, @soTien, @phuongThuc, @trangThai, 0, 1); 
                                           SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@maHd", model.MaHoaDon);
                cmd.Parameters.AddWithValue("@ngayTt", model.NgayThanhToan.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@soTien", model.SoTien);
                cmd.Parameters.AddWithValue("@phuongThuc", model.PhuongThuc);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool UpdateTrangThai(int id, string trangThai)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("UPDATE ThanhToan SET TrangThai = @trangThai, Version = ISNULL(Version, 1) + 1 WHERE MaThanhToan = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@trangThai", trangThai);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Soft-delete
        public bool SoftDelete(int id, string? deletedBy = null)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE ThanhToan 
                                           SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @deletedBy, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaThanhToan = @id", conn);
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
                var cmd = new SqlCommand(@"UPDATE ThanhToan 
                                           SET IsDeleted = 0, DeletedAt = NULL, DeletedBy = NULL, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaThanhToan = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            return SoftDelete(id);
        }

        private static ThanhToanModel MapFromReader(SqlDataReader reader)
        {
            return new ThanhToanModel
            {
                MaThanhToan = Convert.ToInt32(reader["MaThanhToan"]),
                MaHoaDon = Convert.ToInt32(reader["MaHoaDon"]),
                NgayThanhToan = DateOnly.FromDateTime(Convert.ToDateTime(reader["NgayThanhToan"])),
                SoTien = Convert.ToDecimal(reader["SoTien"]),
                PhuongThuc = reader["PhuongThuc"].ToString() ?? "Chuyển khoản",
                TrangThai = reader["TrangThai"].ToString() ?? "Đã thanh toán",
                MaDangKy = reader["MaDangKy"] != DBNull.Value ? Convert.ToInt32(reader["MaDangKy"]) : null,
                TenDocGia = reader["TenDocGia"] != DBNull.Value ? reader["TenDocGia"].ToString() : null,
                IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
                DeletedAt = reader["DeletedAt"] != DBNull.Value ? Convert.ToDateTime(reader["DeletedAt"]) : null,
                DeletedBy = reader["DeletedBy"] != DBNull.Value ? reader["DeletedBy"].ToString() : null,
                Version = reader["Version"] != DBNull.Value ? Convert.ToInt32(reader["Version"]) : 1
            };
        }
    }
}
