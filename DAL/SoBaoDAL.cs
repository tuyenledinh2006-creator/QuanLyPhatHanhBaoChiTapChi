using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class SoBaoDAL
    {
        private const string SelectWithAnPham = @"
            SELECT sb.MaSoBao, sb.MaAnPham, sb.SoThuTu, sb.NgayPhatHanh, sb.SoLuongIn, 
                   sb.SoLuongDaPhatHanh, sb.TrangThai,
                   ap.TenAnPham
            FROM SoBao sb
            LEFT JOIN AnPham ap ON sb.MaAnPham = ap.MaAnPham";

        public List<SoBaoModel> GetAll()
        {
            var list = new List<SoBaoModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithAnPham, conn);
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

        public SoBaoModel? GetById(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithAnPham + " WHERE sb.MaSoBao = @id", conn);
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

        public List<SoBaoModel> GetByAnPham(int maAnPham)
        {
            var list = new List<SoBaoModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithAnPham + " WHERE sb.MaAnPham = @maAp ORDER BY sb.SoThuTu DESC", conn);
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

        public int Insert(SoBaoModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"INSERT INTO SoBao (MaAnPham, SoThuTu, NgayPhatHanh, SoLuongIn, SoLuongDaPhatHanh, TrangThai) 
                                           VALUES (@maAp, @soThuTu, @ngayPh, @soLuongIn, @soLuongDaPh, @trangThai); 
                                           SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@maAp", model.MaAnPham);
                cmd.Parameters.AddWithValue("@soThuTu", model.SoThuTu);
                cmd.Parameters.AddWithValue("@ngayPh", model.NgayPhatHanh.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@soLuongIn", model.SoLuongIn);
                cmd.Parameters.AddWithValue("@soLuongDaPh", model.SoLuongDaPhatHanh);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Update(SoBaoModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE SoBao 
                                           SET MaAnPham = @maAp, SoThuTu = @soThuTu, NgayPhatHanh = @ngayPh, 
                                               SoLuongIn = @soLuongIn, SoLuongDaPhatHanh = @soLuongDaPh, TrangThai = @trangThai 
                                           WHERE MaSoBao = @id", conn);
                cmd.Parameters.AddWithValue("@id", model.MaSoBao);
                cmd.Parameters.AddWithValue("@maAp", model.MaAnPham);
                cmd.Parameters.AddWithValue("@soThuTu", model.SoThuTu);
                cmd.Parameters.AddWithValue("@ngayPh", model.NgayPhatHanh.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@soLuongIn", model.SoLuongIn);
                cmd.Parameters.AddWithValue("@soLuongDaPh", model.SoLuongDaPhatHanh);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool UpdateSoLuongPhatHanh(int maSoBao, int soLuongThem)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE SoBao 
                                           SET SoLuongDaPhatHanh = SoLuongDaPhatHanh + @soLuong 
                                           WHERE MaSoBao = @id", conn);
                cmd.Parameters.AddWithValue("@id", maSoBao);
                cmd.Parameters.AddWithValue("@soLuong", soLuongThem);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("DELETE FROM SoBao WHERE MaSoBao = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private static SoBaoModel MapFromReader(SqlDataReader reader)
        {
            return new SoBaoModel
            {
                MaSoBao = Convert.ToInt32(reader["MaSoBao"]),
                MaAnPham = Convert.ToInt32(reader["MaAnPham"]),
                SoThuTu = Convert.ToInt32(reader["SoThuTu"]),
                NgayPhatHanh = DateOnly.FromDateTime(Convert.ToDateTime(reader["NgayPhatHanh"])),
                SoLuongIn = Convert.ToInt32(reader["SoLuongIn"]),
                SoLuongDaPhatHanh = Convert.ToInt32(reader["SoLuongDaPhatHanh"]),
                TrangThai = reader["TrangThai"].ToString() ?? "Chuẩn bị phát hành",
                TenAnPham = reader["TenAnPham"] != DBNull.Value ? reader["TenAnPham"].ToString() : null
            };
        }
    }
}
