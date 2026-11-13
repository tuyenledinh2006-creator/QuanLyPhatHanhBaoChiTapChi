using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class DanhMucDAL
    {
        public List<DanhMucModel> GetAll()
        {
            var list = new List<DanhMucModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("SELECT MaDanhMuc, TenDanhMuc, MoTa FROM DanhMuc", conn);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DanhMucModel
                        {
                            MaDanhMuc = Convert.ToInt32(reader["MaDanhMuc"]),
                            TenDanhMuc = reader["TenDanhMuc"].ToString() ?? "",
                            MoTa = reader["MoTa"] != DBNull.Value ? reader["MoTa"].ToString() : null
                        });
                    }
                }
            }
            return list;
        }

        public DanhMucModel? GetById(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("SELECT MaDanhMuc, TenDanhMuc, MoTa FROM DanhMuc WHERE MaDanhMuc = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new DanhMucModel
                        {
                            MaDanhMuc = Convert.ToInt32(reader["MaDanhMuc"]),
                            TenDanhMuc = reader["TenDanhMuc"].ToString() ?? "",
                            MoTa = reader["MoTa"] != DBNull.Value ? reader["MoTa"].ToString() : null
                        };
                    }
                }
            }
            return null;
        }

        public int Insert(DanhMucModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("INSERT INTO DanhMuc (TenDanhMuc, MoTa) VALUES (@ten, @mota); SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@ten", model.TenDanhMuc);
                cmd.Parameters.AddWithValue("@mota", (object?)model.MoTa ?? DBNull.Value);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Update(DanhMucModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("UPDATE DanhMuc SET TenDanhMuc = @ten, MoTa = @mota WHERE MaDanhMuc = @id", conn);
                cmd.Parameters.AddWithValue("@id", model.MaDanhMuc);
                cmd.Parameters.AddWithValue("@ten", model.TenDanhMuc);
                cmd.Parameters.AddWithValue("@mota", (object?)model.MoTa ?? DBNull.Value);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("DELETE FROM DanhMuc WHERE MaDanhMuc = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
