using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class NguoiDungDAL
    {
        private const string SelectBase = @"
            SELECT MaNguoiDung, TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, 
                   QuyenHan, TrangThai, RefreshToken, RefreshTokenExpiryTime 
            FROM NguoiDung";

        public List<NguoiDungModel> GetAll()
        {
            var list = new List<NguoiDungModel>();
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectBase, conn);
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

        public NguoiDungModel? GetById(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectBase + " WHERE MaNguoiDung = @id", conn);
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

        public NguoiDungModel? GetByTenDangNhap(string tenDangNhap)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectBase + " WHERE TenDangNhap = @username", conn);
                cmd.Parameters.AddWithValue("@username", tenDangNhap);
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

        public NguoiDungModel? GetByRefreshToken(string refreshToken)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectBase + " WHERE RefreshToken = @token AND RefreshTokenExpiryTime > GETDATE() AND TrangThai = 1", conn);
                cmd.Parameters.AddWithValue("@token", refreshToken);
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

        public NguoiDungModel? Login(string tenDangNhap, string matKhau)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(SelectBase + " WHERE TenDangNhap = @username AND MatKhau = @password AND TrangThai = 1", conn);
                cmd.Parameters.AddWithValue("@username", tenDangNhap);
                cmd.Parameters.AddWithValue("@password", matKhau);
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

        public bool UpdateRefreshToken(int maNguoiDung, string? refreshToken, DateTime? expiryTime)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE NguoiDung 
                                           SET RefreshToken = @token, RefreshTokenExpiryTime = @expiry 
                                           WHERE MaNguoiDung = @id", conn);
                cmd.Parameters.AddWithValue("@id", maNguoiDung);
                cmd.Parameters.AddWithValue("@token", (object?)refreshToken ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@expiry", (object?)expiryTime ?? DBNull.Value);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool RevokeRefreshToken(int maNguoiDung)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE NguoiDung 
                                           SET RefreshToken = NULL, RefreshTokenExpiryTime = NULL 
                                           WHERE MaNguoiDung = @id", conn);
                cmd.Parameters.AddWithValue("@id", maNguoiDung);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool RevokeRefreshTokenByToken(string refreshToken)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"UPDATE NguoiDung 
                                           SET RefreshToken = NULL, RefreshTokenExpiryTime = NULL 
                                           WHERE RefreshToken = @token", conn);
                cmd.Parameters.AddWithValue("@token", refreshToken);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public int Insert(NguoiDungModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand(@"INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, QuyenHan, TrangThai, RefreshToken, RefreshTokenExpiryTime) 
                                           VALUES (@username, @password, @hoten, @email, @sdt, @quyen, @trangthai, @token, @expiry); 
                                           SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@username", model.TenDangNhap);
                cmd.Parameters.AddWithValue("@password", model.MatKhau);
                cmd.Parameters.AddWithValue("@hoten", model.HoTen);
                cmd.Parameters.AddWithValue("@email", model.Email);
                cmd.Parameters.AddWithValue("@sdt", (object?)model.SoDienThoai ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@quyen", model.QuyenHan);
                cmd.Parameters.AddWithValue("@trangthai", model.TrangThai);
                cmd.Parameters.AddWithValue("@token", (object?)model.RefreshToken ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@expiry", (object?)model.RefreshTokenExpiryTime ?? DBNull.Value);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Update(NguoiDungModel model)
        {
            using (var conn = KetNoi.GetConnection())
            {
                string sql = string.IsNullOrWhiteSpace(model.MatKhau)
                    ? @"UPDATE NguoiDung 
                       SET HoTen = @hoten, Email = @email, SoDienThoai = @sdt, QuyenHan = @quyen, TrangThai = @trangthai 
                       WHERE MaNguoiDung = @id"
                    : @"UPDATE NguoiDung 
                       SET HoTen = @hoten, Email = @email, SoDienThoai = @sdt, QuyenHan = @quyen, TrangThai = @trangthai, MatKhau = @password 
                       WHERE MaNguoiDung = @id";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", model.MaNguoiDung);
                cmd.Parameters.AddWithValue("@hoten", model.HoTen);
                cmd.Parameters.AddWithValue("@email", model.Email);
                cmd.Parameters.AddWithValue("@sdt", (object?)model.SoDienThoai ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@quyen", model.QuyenHan);
                cmd.Parameters.AddWithValue("@trangthai", model.TrangThai);
                if (!string.IsNullOrWhiteSpace(model.MatKhau))
                {
                    cmd.Parameters.AddWithValue("@password", model.MatKhau);
                }
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool ChangePassword(int id, string newPassword)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("UPDATE NguoiDung SET MatKhau = @password WHERE MaNguoiDung = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@password", newPassword);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool UpdateQuyenHan(int id, string quyenHan)
        {
            using (var conn = KetNoi.GetConnection())
            {
                var cmd = new SqlCommand("UPDATE NguoiDung SET QuyenHan = @quyen WHERE MaNguoiDung = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@quyen", quyenHan);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = KetNoi.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Kiểm tra độc giả có giao dịch đăng ký báo nào chưa
                        var checkCmd = new SqlCommand(@"
                            SELECT COUNT(*) FROM DangKy 
                            WHERE MaDocGia IN (SELECT MaDocGia FROM DocGia WHERE MaNguoiDung = @id)", conn, trans);
                        checkCmd.Parameters.AddWithValue("@id", id);
                        int countDangKy = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (countDangKy > 0)
                        {
                            // Nếu đã phát sinh đăng ký/hóa đơn: Vô hiệu hóa tài khoản (Soft Delete) để bảo toàn chứng từ
                            var deactCmd = new SqlCommand("UPDATE NguoiDung SET TrangThai = 0 WHERE MaNguoiDung = @id", conn, trans);
                            deactCmd.Parameters.AddWithValue("@id", id);
                            bool deactOk = deactCmd.ExecuteNonQuery() > 0;
                            trans.Commit();
                            return deactOk;
                        }

                        // 2. Nếu chưa có giao dịch ràng buộc, xóa bản ghi độc giả liên quan trước
                        var delDgCmd = new SqlCommand("DELETE FROM DocGia WHERE MaNguoiDung = @id", conn, trans);
                        delDgCmd.Parameters.AddWithValue("@id", id);
                        delDgCmd.ExecuteNonQuery();

                        // 3. Xóa tài khoản người dùng
                        var cmd = new SqlCommand("DELETE FROM NguoiDung WHERE MaNguoiDung = @id", conn, trans);
                        cmd.Parameters.AddWithValue("@id", id);
                        bool ok = cmd.ExecuteNonQuery() > 0;
                        trans.Commit();
                        return ok;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        private static NguoiDungModel MapFromReader(SqlDataReader reader)
        {
            return new NguoiDungModel
            {
                MaNguoiDung = Convert.ToInt32(reader["MaNguoiDung"]),
                TenDangNhap = reader["TenDangNhap"].ToString() ?? "",
                MatKhau = reader["MatKhau"].ToString() ?? "",
                HoTen = reader["HoTen"].ToString() ?? "",
                Email = reader["Email"].ToString() ?? "",
                SoDienThoai = reader["SoDienThoai"] != DBNull.Value ? reader["SoDienThoai"].ToString() : null,
                QuyenHan = reader["QuyenHan"].ToString() ?? "",
                TrangThai = Convert.ToBoolean(reader["TrangThai"]),
                RefreshToken = reader["RefreshToken"] != DBNull.Value ? reader["RefreshToken"].ToString() : null,
                RefreshTokenExpiryTime = reader["RefreshTokenExpiryTime"] != DBNull.Value ? Convert.ToDateTime(reader["RefreshTokenExpiryTime"]) : null
            };
        }
    }
}
