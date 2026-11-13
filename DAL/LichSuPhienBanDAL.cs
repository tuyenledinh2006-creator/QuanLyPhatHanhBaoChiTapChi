using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class LichSuPhienBanDAL
    {
        public int Insert(LichSuPhienBanModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"
                    INSERT INTO LichSuPhienBan (Bang, KhoaChinh, SoPhienBan, DuLieuSnapshot, NguoiCapNhat, ThoiGianCapNhat, GhiChu)
                    VALUES (@bang, @khoaChinh, @soPhienBan, @duLieuSnapshot, @nguoiCapNhat, @thoiGianCapNhat, @ghiChu);
                    SELECT SCOPE_IDENTITY();", conn);

                cmd.Parameters.AddWithValue("@bang", model.Bang);
                cmd.Parameters.AddWithValue("@khoaChinh", model.KhoaChinh);
                cmd.Parameters.AddWithValue("@soPhienBan", model.SoPhienBan);
                cmd.Parameters.AddWithValue("@duLieuSnapshot", model.DuLieuSnapshot);
                cmd.Parameters.AddWithValue("@nguoiCapNhat", (object?)model.NguoiCapNhat ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@thoiGianCapNhat", model.ThoiGianCapNhat);
                cmd.Parameters.AddWithValue("@ghiChu", (object?)model.GhiChu ?? DBNull.Value);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public List<LichSuPhienBanModel> GetByEntity(string bang, string khoaChinh)
        {
            var list = new List<LichSuPhienBanModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"
                    SELECT * FROM LichSuPhienBan 
                    WHERE Bang = @bang AND KhoaChinh = @khoaChinh 
                    ORDER BY SoPhienBan DESC", conn);

                cmd.Parameters.AddWithValue("@bang", bang);
                cmd.Parameters.AddWithValue("@khoaChinh", khoaChinh);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new LichSuPhienBanModel
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Bang = reader["Bang"].ToString() ?? string.Empty,
                            KhoaChinh = reader["KhoaChinh"].ToString() ?? string.Empty,
                            SoPhienBan = Convert.ToInt32(reader["SoPhienBan"]),
                            DuLieuSnapshot = reader["DuLieuSnapshot"].ToString() ?? string.Empty,
                            NguoiCapNhat = reader["NguoiCapNhat"] != DBNull.Value ? reader["NguoiCapNhat"].ToString() : null,
                            ThoiGianCapNhat = Convert.ToDateTime(reader["ThoiGianCapNhat"]),
                            GhiChu = reader["GhiChu"] != DBNull.Value ? reader["GhiChu"].ToString() : null
                        });
                    }
                }
            }
            return list;
        }
    }
}
