using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class PhatHanhDAL
    {
        private const string SelectWithDetails = @"
            SELECT ph.MaPhatHanh, ph.MaDangKy, ph.MaSoBao, ph.NgayPhatHanh, 
                   ph.SoLuong, ph.HinhThuc, ph.DiaChiGiao, ph.TrangThai,
                   ph.IsDeleted, ph.DeletedAt, ph.DeletedBy, ph.Version,
                   nd.HoTen AS TenDocGia,
                   ap.TenAnPham,
                   sb.SoThuTu AS SoThuTuBao,
                   hd.TrangThai AS TrangThaiHoaDon,
                   dk.TrangThai AS TrangThaiDangKy
            FROM PhatHanh ph
            INNER JOIN DangKy dk ON ph.MaDangKy = dk.MaDangKy
            INNER JOIN DocGia dg ON dk.MaDocGia = dg.MaDocGia
            INNER JOIN NguoiDung nd ON dg.MaNguoiDung = nd.MaNguoiDung
            INNER JOIN SoBao sb ON ph.MaSoBao = sb.MaSoBao
            INNER JOIN AnPham ap ON sb.MaAnPham = ap.MaAnPham
            LEFT JOIN HoaDon hd ON dk.MaDangKy = hd.MaDangKy";

        public List<PhatHanhModel> GetAll(bool baoGomDaXoa = false)
        {
            var list = new List<PhatHanhModel>();
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithDetails + " WHERE (hd.TrangThai = N'Đã thanh toán' OR (hd.TrangThai IS NULL AND dk.TrangThai NOT IN (N'Chờ thanh toán', N'Chờ duyệt', N'Đã hủy')))";
                if (!baoGomDaXoa)
                {
                    sql += " AND ISNULL(ph.IsDeleted, 0) = 0";
                }
                sql += " ORDER BY ph.NgayPhatHanh DESC";

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

        public PhatHanhModel? GetById(int id, bool baoGomDaXoa = false)
        {
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithDetails + " WHERE ph.MaPhatHanh = @id";
                if (!baoGomDaXoa)
                {
                    sql += " AND ISNULL(ph.IsDeleted, 0) = 0";
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

        public List<PhatHanhModel> GetByDangKy(int maDangKy)
        {
            var list = new List<PhatHanhModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + " WHERE ph.MaDangKy = @maDk AND ISNULL(ph.IsDeleted, 0) = 0", conn);
                cmd.Parameters.AddWithValue("@maDk", maDangKy);
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

        public List<PhatHanhModel> GetBySoBao(int maSoBao)
        {
            var list = new List<PhatHanhModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + " WHERE ph.MaSoBao = @maSb AND ISNULL(ph.IsDeleted, 0) = 0", conn);
                cmd.Parameters.AddWithValue("@maSb", maSoBao);
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

        public List<PhatHanhModel> GetByDocGia(int maDocGia)
        {
            var list = new List<PhatHanhModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + @" WHERE dg.MaDocGia = @maDg 
                                            AND ISNULL(ph.IsDeleted, 0) = 0 
                                            AND (hd.TrangThai = N'Đã thanh toán' OR (hd.TrangThai IS NULL AND dk.TrangThai NOT IN (N'Chờ thanh toán', N'Chờ duyệt', N'Đã hủy')))
                                            ORDER BY ph.NgayPhatHanh DESC", conn);
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

        public int Insert(PhatHanhModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"INSERT INTO PhatHanh (MaDangKy, MaSoBao, NgayPhatHanh, SoLuong, HinhThuc, DiaChiGiao, TrangThai, IsDeleted, Version) 
                                           VALUES (@maDk, @maSb, @ngayPh, @soLuong, @hinhThuc, @diaChi, @trangThai, 0, 1); 
                                           SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@maDk", model.MaDangKy);
                cmd.Parameters.AddWithValue("@maSb", model.MaSoBao);
                cmd.Parameters.AddWithValue("@ngayPh", model.NgayPhatHanh.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@soLuong", model.SoLuong);
                cmd.Parameters.AddWithValue("@hinhThuc", (object?)model.HinhThuc ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@diaChi", (object?)model.DiaChiGiao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Update(PhatHanhModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE PhatHanh 
                                           SET MaDangKy = @maDk, MaSoBao = @maSb, NgayPhatHanh = @ngayPh, 
                                               SoLuong = @soLuong, HinhThuc = @hinhThuc, DiaChiGiao = @diaChi, 
                                               TrangThai = @trangThai, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaPhatHanh = @id", conn);
                cmd.Parameters.AddWithValue("@id", model.MaPhatHanh);
                cmd.Parameters.AddWithValue("@maDk", model.MaDangKy);
                cmd.Parameters.AddWithValue("@maSb", model.MaSoBao);
                cmd.Parameters.AddWithValue("@ngayPh", model.NgayPhatHanh.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@soLuong", model.SoLuong);
                cmd.Parameters.AddWithValue("@hinhThuc", (object?)model.HinhThuc ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@diaChi", (object?)model.DiaChiGiao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool UpdateTrangThai(int id, string trangThai)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("UPDATE PhatHanh SET TrangThai = @trangThai, Version = ISNULL(Version, 1) + 1 WHERE MaPhatHanh = @id", conn);
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
                var cmd = new SqlCommand(@"UPDATE PhatHanh 
                                           SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @deletedBy, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaPhatHanh = @id", conn);
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
                var cmd = new SqlCommand(@"UPDATE PhatHanh 
                                           SET IsDeleted = 0, DeletedAt = NULL, DeletedBy = NULL, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaPhatHanh = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            return SoftDelete(id);
        }

        private static PhatHanhModel MapFromReader(SqlDataReader reader)
        {
            return new PhatHanhModel
            {
                MaPhatHanh = Convert.ToInt32(reader["MaPhatHanh"]),
                MaDangKy = Convert.ToInt32(reader["MaDangKy"]),
                MaSoBao = Convert.ToInt32(reader["MaSoBao"]),
                NgayPhatHanh = DateOnly.FromDateTime(Convert.ToDateTime(reader["NgayPhatHanh"])),
                SoLuong = Convert.ToInt32(reader["SoLuong"]),
                HinhThuc = reader["HinhThuc"] != DBNull.Value ? reader["HinhThuc"].ToString() : null,
                DiaChiGiao = reader["DiaChiGiao"] != DBNull.Value ? reader["DiaChiGiao"].ToString() : null,
                TrangThai = reader["TrangThai"].ToString() ?? "Chờ xử lý",
                TenDocGia = reader["TenDocGia"] != DBNull.Value ? reader["TenDocGia"].ToString() : null,
                TenAnPham = reader["TenAnPham"] != DBNull.Value ? reader["TenAnPham"].ToString() : null,
                SoThuTuBao = reader["SoThuTuBao"] != DBNull.Value ? Convert.ToInt32(reader["SoThuTuBao"]) : null,
                TrangThaiHoaDon = reader["TrangThaiHoaDon"] != DBNull.Value ? reader["TrangThaiHoaDon"].ToString() : null,
                TrangThaiDangKy = reader["TrangThaiDangKy"] != DBNull.Value ? reader["TrangThaiDangKy"].ToString() : null,
                IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
                DeletedAt = reader["DeletedAt"] != DBNull.Value ? Convert.ToDateTime(reader["DeletedAt"]) : null,
                DeletedBy = reader["DeletedBy"] != DBNull.Value ? reader["DeletedBy"].ToString() : null,
                Version = reader["Version"] != DBNull.Value ? Convert.ToInt32(reader["Version"]) : 1
            };
        }
    }
}
