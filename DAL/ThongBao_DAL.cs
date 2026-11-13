using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class ThongBao_DAL
    {
        private const string SelectWithDetails = @"
            SELECT tb.MaThongBao, tb.MaDocGia, tb.TieuDe, tb.NoiDung, tb.NgayGui, tb.TrangThai,
                   nd.HoTen AS TenDocGia
            FROM ThongBao tb
            INNER JOIN DocGia dg ON tb.MaDocGia = dg.MaDocGia
            INNER JOIN NguoiDung nd ON dg.MaNguoiDung = nd.MaNguoiDung";

        public List<ThongBaoModel> GetAll()
        {
            var list = new List<ThongBaoModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + " ORDER BY tb.NgayGui DESC", conn);
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

        public ThongBaoModel? GetById(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + " WHERE tb.MaThongBao = @id", conn);
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

        public List<ThongBaoModel> GetByDocGia(int maDocGia)
        {
            var list = new List<ThongBaoModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithDetails + " WHERE tb.MaDocGia = @maDg ORDER BY tb.NgayGui DESC", conn);
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

        public int Insert(ThongBaoModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"INSERT INTO ThongBao (MaDocGia, TieuDe, NoiDung, NgayGui, TrangThai) 
                                           VALUES (@maDg, @tieuDe, @noiDung, @ngayGui, @trangThai); 
                                           SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@maDg", model.MaDocGia);
                cmd.Parameters.AddWithValue("@tieuDe", model.TieuDe);
                cmd.Parameters.AddWithValue("@noiDung", model.NoiDung);
                cmd.Parameters.AddWithValue("@ngayGui", model.NgayGui.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool MarkAsRead(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("UPDATE ThongBao SET TrangThai = 1 WHERE MaThongBao = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("DELETE FROM ThongBao WHERE MaThongBao = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteByDocGia(int maDocGia)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("DELETE FROM ThongBao WHERE MaDocGia = @maDg", conn);
                cmd.Parameters.AddWithValue("@maDg", maDocGia);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private static ThongBaoModel MapFromReader(SqlDataReader reader)
        {
            return new ThongBaoModel
            {
                MaThongBao = Convert.ToInt32(reader["MaThongBao"]),
                MaDocGia = Convert.ToInt32(reader["MaDocGia"]),
                TieuDe = reader["TieuDe"].ToString() ?? "",
                NoiDung = reader["NoiDung"].ToString() ?? "",
                NgayGui = DateOnly.FromDateTime(Convert.ToDateTime(reader["NgayGui"])),
                TrangThai = Convert.ToBoolean(reader["TrangThai"]),
                TenDocGia = reader["TenDocGia"] != DBNull.Value ? reader["TenDocGia"].ToString() : null
            };
        }
    }
}
