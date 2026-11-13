using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class DangKyDAL
    {
        private const string SelectWithDetails = @"
            SELECT dk.MaDangKy, dk.MaDocGia, dk.MaAnPham, dk.NgayDangKy, 
                   dk.NgayBatDau, dk.NgayKetThuc, dk.KyHan, dk.SoLuong, dk.TrangThai,
                   dk.IsDeleted, dk.DeletedAt, dk.DeletedBy, dk.Version,
                   nd.HoTen AS TenDocGia,
                   ap.TenAnPham, ap.Gia AS GiaAnPham,
                   COALESCE(dk.MaSoBao, ph_sub.MaSoBao) AS MaSoBao,
                   sb.SoThuTu AS SoThuTuBao,
                   ph_sub.TrangThaiPhatHanh AS TrangThaiGiaoHang,
                   ph_sub.MaPhatHanh
            FROM DangKy dk
            INNER JOIN DocGia dg ON dk.MaDocGia = dg.MaDocGia
            INNER JOIN NguoiDung nd ON dg.MaNguoiDung = nd.MaNguoiDung
            INNER JOIN AnPham ap ON dk.MaAnPham = ap.MaAnPham
            LEFT JOIN (
                SELECT MaDangKy, MIN(MaSoBao) AS MaSoBao, MIN(MaPhatHanh) AS MaPhatHanh, MAX(TrangThai) AS TrangThaiPhatHanh 
                FROM PhatHanh 
                WHERE ISNULL(IsDeleted, 0) = 0 
                GROUP BY MaDangKy
            ) ph_sub ON dk.MaDangKy = ph_sub.MaDangKy
            LEFT JOIN SoBao sb ON COALESCE(dk.MaSoBao, ph_sub.MaSoBao) = sb.MaSoBao";

        public List<DangKyModel> GetAll(bool baoGomDaXoa = false)
        {
            var list = new List<DangKyModel>();
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithDetails;
                if (!baoGomDaXoa)
                {
                    sql += " WHERE ISNULL(dk.IsDeleted, 0) = 0";
                }
                sql += " ORDER BY dk.NgayDangKy DESC";

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

        public DangKyModel? GetById(int id, bool baoGomDaXoa = false)
        {
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithDetails + " WHERE dk.MaDangKy = @id";
                if (!baoGomDaXoa)
                {
                    sql += " AND ISNULL(dk.IsDeleted, 0) = 0";
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

        public List<DangKyModel> GetByDocGia(int maDocGia, bool baoGomDaXoa = false)
        {
            var list = new List<DangKyModel>();
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithDetails + " WHERE dk.MaDocGia = @maDg";
                if (!baoGomDaXoa)
                {
                    sql += " AND ISNULL(dk.IsDeleted, 0) = 0";
                }
                sql += " ORDER BY dk.NgayDangKy DESC";

                var cmd = new SqlCommand(sql, conn);
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

        public List<DangKyModel> GetByAnPham(int maAnPham)
        {
            var list = new List<DangKyModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + " WHERE dk.MaAnPham = @maAp AND ISNULL(dk.IsDeleted, 0) = 0 ORDER BY dk.NgayDangKy DESC", conn);
                cmd.Parameters.AddWithValue("@maAp", maAnPham);
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

        public List<DangKyModel> GetByAnPhamAndDocGia(int maAnPham, int maDocGia)
        {
            var list = new List<DangKyModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + " WHERE dk.MaAnPham = @maAp AND dk.MaDocGia = @maDg AND ISNULL(dk.IsDeleted, 0) = 0 ORDER BY dk.NgayDangKy DESC", conn);
                cmd.Parameters.AddWithValue("@maAp", maAnPham);
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

        public int Insert(DangKyModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"INSERT INTO DangKy (MaDocGia, MaAnPham, MaSoBao, NgayDangKy, NgayBatDau, NgayKetThuc, KyHan, SoLuong, TrangThai, IsDeleted, Version) 
                                           VALUES (@maDg, @maAp, @maSb, @ngayDk, @ngayBd, @ngayKt, @kyHan, @soLuong, @trangThai, 0, 1); 
                                           SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@maDg", model.MaDocGia);
                cmd.Parameters.AddWithValue("@maAp", model.MaAnPham);
                cmd.Parameters.AddWithValue("@maSb", (object?)model.MaSoBao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ngayDk", model.NgayDangKy.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@ngayBd", model.NgayBatDau.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@ngayKt", model.NgayKetThuc.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@kyHan", model.KyHan);
                cmd.Parameters.AddWithValue("@soLuong", model.SoLuong);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Update(DangKyModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE DangKy 
                                           SET MaDocGia = @maDg, MaAnPham = @maAp, MaSoBao = @maSb, NgayBatDau = @ngayBd, 
                                               NgayKetThuc = @ngayKt, KyHan = @kyHan, SoLuong = @soLuong, 
                                               TrangThai = @trangThai, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaDangKy = @id", conn);
                cmd.Parameters.AddWithValue("@id", model.MaDangKy);
                cmd.Parameters.AddWithValue("@maDg", model.MaDocGia);
                cmd.Parameters.AddWithValue("@maAp", model.MaAnPham);
                cmd.Parameters.AddWithValue("@maSb", (object?)model.MaSoBao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ngayBd", model.NgayBatDau.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@ngayKt", model.NgayKetThuc.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@kyHan", model.KyHan);
                cmd.Parameters.AddWithValue("@soLuong", model.SoLuong);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool UpdateTrangThai(int id, string trangThai)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("UPDATE DangKy SET TrangThai = @trangThai, Version = ISNULL(Version, 1) + 1 WHERE MaDangKy = @id", conn);
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
                var cmd = new SqlCommand(@"UPDATE DangKy 
                                           SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @deletedBy, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaDangKy = @id", conn);
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
                var cmd = new SqlCommand(@"UPDATE DangKy 
                                           SET IsDeleted = 0, DeletedAt = NULL, DeletedBy = NULL, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaDangKy = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            return SoftDelete(id);
        }

        private static DangKyModel MapFromReader(SqlDataReader reader)
        {
            return new DangKyModel
            {
                MaDangKy = Convert.ToInt32(reader["MaDangKy"]),
                MaDocGia = Convert.ToInt32(reader["MaDocGia"]),
                MaAnPham = Convert.ToInt32(reader["MaAnPham"]),
                NgayDangKy = DateOnly.FromDateTime(Convert.ToDateTime(reader["NgayDangKy"])),
                NgayBatDau = DateOnly.FromDateTime(Convert.ToDateTime(reader["NgayBatDau"])),
                NgayKetThuc = DateOnly.FromDateTime(Convert.ToDateTime(reader["NgayKetThuc"])),
                KyHan = Convert.ToInt32(reader["KyHan"]),
                SoLuong = Convert.ToInt32(reader["SoLuong"]),
                TrangThai = reader["TrangThai"].ToString() ?? "Chờ thanh toán",
                TenDocGia = reader["TenDocGia"] != DBNull.Value ? reader["TenDocGia"].ToString() : null,
                TenAnPham = reader["TenAnPham"] != DBNull.Value ? reader["TenAnPham"].ToString() : null,
                GiaAnPham = reader["GiaAnPham"] != DBNull.Value ? Convert.ToDecimal(reader["GiaAnPham"]) : null,
                MaSoBao = reader["MaSoBao"] != DBNull.Value ? Convert.ToInt32(reader["MaSoBao"]) : null,
                SoThuTuBao = reader["SoThuTuBao"] != DBNull.Value ? Convert.ToInt32(reader["SoThuTuBao"]) : null,
                TrangThaiGiaoHang = reader["TrangThaiGiaoHang"] != DBNull.Value ? reader["TrangThaiGiaoHang"].ToString() : null,
                MaPhatHanh = reader["MaPhatHanh"] != DBNull.Value ? Convert.ToInt32(reader["MaPhatHanh"]) : null,
                IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
                DeletedAt = reader["DeletedAt"] != DBNull.Value ? Convert.ToDateTime(reader["DeletedAt"]) : null,
                DeletedBy = reader["DeletedBy"] != DBNull.Value ? reader["DeletedBy"].ToString() : null,
                Version = reader["Version"] != DBNull.Value ? Convert.ToInt32(reader["Version"]) : 1
            };
        }
    }
}
