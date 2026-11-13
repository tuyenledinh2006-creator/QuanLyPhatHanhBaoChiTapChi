using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class HoaDonDAL
    {
        private const string SelectWithDetails = @"
            SELECT hd.MaHoaDon, hd.MaDangKy, hd.NgayLap, hd.TongTien, hd.GiamGia, hd.ThanhTien, hd.TrangThai,
                   hd.IsDeleted, hd.DeletedAt, hd.DeletedBy, hd.Version,
                   nd.HoTen AS TenDocGia,
                   ap.TenAnPham
            FROM HoaDon hd
            INNER JOIN DangKy dk ON hd.MaDangKy = dk.MaDangKy
            INNER JOIN DocGia dg ON dk.MaDocGia = dg.MaDocGia
            INNER JOIN NguoiDung nd ON dg.MaNguoiDung = nd.MaNguoiDung
            INNER JOIN AnPham ap ON dk.MaAnPham = ap.MaAnPham";

        public List<HoaDonModel> GetAll(bool baoGomDaXoa = false)
        {
            var list = new List<HoaDonModel>();
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithDetails;
                if (!baoGomDaXoa)
                {
                    sql += " WHERE ISNULL(hd.IsDeleted, 0) = 0";
                }
                sql += " ORDER BY hd.NgayLap DESC";

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

        public HoaDonModel? GetById(int id, bool baoGomDaXoa = false)
        {
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithDetails + " WHERE hd.MaHoaDon = @id";
                if (!baoGomDaXoa)
                {
                    sql += " AND ISNULL(hd.IsDeleted, 0) = 0";
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

        public HoaDonModel? GetByDangKy(int maDangKy, bool baoGomDaXoa = false)
        {
            using (var conn = KetNoi.GetConnection())
            {
                string sql = SelectWithDetails + " WHERE hd.MaDangKy = @maDk";
                if (!baoGomDaXoa)
                {
                    sql += " AND ISNULL(hd.IsDeleted, 0) = 0";
                }
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@maDk", maDangKy);
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

        public List<HoaDonModel> GetByDocGia(int maDocGia)
        {
            var list = new List<HoaDonModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + " WHERE dk.MaDocGia = @maDg AND ISNULL(hd.IsDeleted, 0) = 0 AND ISNULL(dk.IsDeleted, 0) = 0 ORDER BY hd.NgayLap DESC", conn);
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

        public int Insert(HoaDonModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"INSERT INTO HoaDon (MaDangKy, NgayLap, TongTien, GiamGia, TrangThai, IsDeleted, Version) 
                                           VALUES (@maDk, @ngayLap, @tongTien, @giamGia, @trangThai, 0, 1); 
                                           SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@maDk", model.MaDangKy);
                cmd.Parameters.AddWithValue("@ngayLap", model.NgayLap.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@tongTien", model.TongTien);
                cmd.Parameters.AddWithValue("@giamGia", model.GiamGia);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Update(HoaDonModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE HoaDon 
                                           SET TongTien = @tongTien, GiamGia = @giamGia, TrangThai = @trangThai, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaHoaDon = @id", conn);
                cmd.Parameters.AddWithValue("@id", model.MaHoaDon);
                cmd.Parameters.AddWithValue("@tongTien", model.TongTien);
                cmd.Parameters.AddWithValue("@giamGia", model.GiamGia);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool UpdateTrangThai(int id, string trangThai)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("UPDATE HoaDon SET TrangThai = @trangThai, Version = ISNULL(Version, 1) + 1 WHERE MaHoaDon = @id", conn);
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
                var cmd = new SqlCommand(@"UPDATE HoaDon 
                                           SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @deletedBy, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaHoaDon = @id", conn);
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
                var cmd = new SqlCommand(@"UPDATE HoaDon 
                                           SET IsDeleted = 0, DeletedAt = NULL, DeletedBy = NULL, Version = ISNULL(Version, 1) + 1 
                                           WHERE MaHoaDon = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            return SoftDelete(id);
        }

        private static HoaDonModel MapFromReader(SqlDataReader reader)
        {
            return new HoaDonModel
            {
                MaHoaDon = Convert.ToInt32(reader["MaHoaDon"]),
                MaDangKy = Convert.ToInt32(reader["MaDangKy"]),
                NgayLap = DateOnly.FromDateTime(Convert.ToDateTime(reader["NgayLap"])),
                TongTien = Convert.ToDecimal(reader["TongTien"]),
                GiamGia = reader["GiamGia"] != DBNull.Value ? Convert.ToDecimal(reader["GiamGia"]) : 0,
                ThanhTien = reader["ThanhTien"] != DBNull.Value ? Convert.ToDecimal(reader["ThanhTien"]) : Convert.ToDecimal(reader["TongTien"]),
                TrangThai = reader["TrangThai"].ToString() ?? "Chưa thanh toán",
                TenDocGia = reader["TenDocGia"] != DBNull.Value ? reader["TenDocGia"].ToString() : null,
                TenAnPham = reader["TenAnPham"] != DBNull.Value ? reader["TenAnPham"].ToString() : null,
                IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
                DeletedAt = reader["DeletedAt"] != DBNull.Value ? Convert.ToDateTime(reader["DeletedAt"]) : null,
                DeletedBy = reader["DeletedBy"] != DBNull.Value ? reader["DeletedBy"].ToString() : null,
                Version = reader["Version"] != DBNull.Value ? Convert.ToInt32(reader["Version"]) : 1
            };
        }
    }
}
