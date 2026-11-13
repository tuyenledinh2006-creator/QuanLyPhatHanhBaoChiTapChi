using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class AnPhamDAL
    {
        private const string SelectWithCategory = @"
            SELECT ap.MaAnPham, ap.MaDanhMuc, ap.TenAnPham, ap.MoTa, ap.Gia, ap.ChuKyPhatHanh, ap.TrangThai,
                   dm.TenDanhMuc
            FROM AnPham ap
            LEFT JOIN DanhMuc dm ON ap.MaDanhMuc = dm.MaDanhMuc";

        public List<AnPhamModel> GetAll()
        {
            var list = new List<AnPhamModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithCategory, conn);
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

        public AnPhamModel? GetById(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithCategory + " WHERE ap.MaAnPham = @id", conn);
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

        public List<AnPhamModel> GetByDanhMuc(int maDanhMuc)
        {
            var list = new List<AnPhamModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectWithCategory + " WHERE ap.MaDanhMuc = @maDm", conn);
                cmd.Parameters.AddWithValue("@maDm", maDanhMuc);
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

        public int Insert(AnPhamModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"INSERT INTO AnPham (MaDanhMuc, TenAnPham, MoTa, Gia, ChuKyPhatHanh, TrangThai) 
                                           VALUES (@maDm, @ten, @mota, @gia, @chuKy, @trangThai); 
                                           SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@maDm", model.MaDanhMuc);
                cmd.Parameters.AddWithValue("@ten", model.TenAnPham);
                cmd.Parameters.AddWithValue("@mota", (object?)model.MoTa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@gia", model.Gia);
                cmd.Parameters.AddWithValue("@chuKy", (object?)model.ChuKyPhatHanh ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Update(AnPhamModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE AnPham 
                                           SET MaDanhMuc = @maDm, TenAnPham = @ten, MoTa = @mota, 
                                               Gia = @gia, ChuKyPhatHanh = @chuKy, TrangThai = @trangThai 
                                           WHERE MaAnPham = @id", conn);
                cmd.Parameters.AddWithValue("@id", model.MaAnPham);
                cmd.Parameters.AddWithValue("@maDm", model.MaDanhMuc);
                cmd.Parameters.AddWithValue("@ten", model.TenAnPham);
                cmd.Parameters.AddWithValue("@mota", (object?)model.MoTa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@gia", model.Gia);
                cmd.Parameters.AddWithValue("@chuKy", (object?)model.ChuKyPhatHanh ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("DELETE FROM AnPham WHERE MaAnPham = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private static AnPhamModel MapFromReader(SqlDataReader reader)
        {
            return new AnPhamModel
            {
                MaAnPham = Convert.ToInt32(reader["MaAnPham"]),
                MaDanhMuc = Convert.ToInt32(reader["MaDanhMuc"]),
                TenAnPham = reader["TenAnPham"].ToString() ?? "",
                MoTa = reader["MoTa"] != DBNull.Value ? reader["MoTa"].ToString() : null,
                Gia = Convert.ToDecimal(reader["Gia"]),
                ChuKyPhatHanh = reader["ChuKyPhatHanh"] != DBNull.Value ? reader["ChuKyPhatHanh"].ToString() : null,
                TrangThai = Convert.ToBoolean(reader["TrangThai"]),
                TenDanhMuc = reader["TenDanhMuc"] != DBNull.Value ? reader["TenDanhMuc"].ToString() : null
            };
        }
    }
}
