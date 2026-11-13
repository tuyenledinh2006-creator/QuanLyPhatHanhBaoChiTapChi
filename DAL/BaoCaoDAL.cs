using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class BaoCaoDAL
    {
        public BaoCaoModel GetTongQuan()
        {
            var baoCao = new BaoCaoModel();
            using (var conn = KetNoi.GetConnection())
            {
                conn.Open();

                // Đếm người dùng
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM NguoiDung", conn))
                    baoCao.TongNguoiDung = Convert.ToInt32(cmd.ExecuteScalar());

                // Đếm độc giả
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM DocGia", conn))
                    baoCao.TongDocGia = Convert.ToInt32(cmd.ExecuteScalar());

                // Đếm ấn phẩm
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM AnPham", conn))
                    baoCao.TongAnPham = Convert.ToInt32(cmd.ExecuteScalar());

                // Đếm số báo
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM SoBao", conn))
                    baoCao.TongSoBao = Convert.ToInt32(cmd.ExecuteScalar());

                // Đếm đăng ký
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM DangKy", conn))
                    baoCao.TongDangKy = Convert.ToInt32(cmd.ExecuteScalar());

                // Đếm hóa đơn
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM HoaDon", conn))
                    baoCao.TongHoaDon = Convert.ToInt32(cmd.ExecuteScalar());

                // Tổng doanh thu từ thanh toán thành công
                using (var cmd = new SqlCommand("SELECT ISNULL(SUM(SoTien), 0) FROM ThanhToan WHERE TrangThai = N'Đã thanh toán'", conn))
                    baoCao.TongDoanhThu = Convert.ToDecimal(cmd.ExecuteScalar());

                // Doanh thu theo tháng
                baoCao.DoanhThuTheoThang = GetDoanhThuTheoThangInternal(conn, DateTime.Now.Year);

                // Ấn phẩm bán chạy
                baoCao.AnPhamPhoBien = GetAnPhamBanChayInternal(conn, 5);
            }
            return baoCao;
        }

        public List<BaoCaoDoanhThuThang> GetDoanhThuTheoThang(int nam)
        {
            using (var conn = KetNoi.GetConnection())
            {
                conn.Open();
                return GetDoanhThuTheoThangInternal(conn, nam);
            }
        }

        public List<BaoCaoAnPhamBanChay> GetAnPhamBanChay(int top = 10)
        {
            using (var conn = KetNoi.GetConnection())
            {
                conn.Open();
                return GetAnPhamBanChayInternal(conn, top);
            }
        }

        private static List<BaoCaoDoanhThuThang> GetDoanhThuTheoThangInternal(SqlConnection conn, int nam)
        {
            var list = new List<BaoCaoDoanhThuThang>();
            string sql;
            if (nam <= 0)
            {
                sql = @"
                    SELECT MONTH(NgayThanhToan) AS Thang, YEAR(NgayThanhToan) AS Nam, 
                           SUM(SoTien) AS DoanhThu, COUNT(*) AS SoHoaDon
                    FROM ThanhToan
                    WHERE TrangThai = N'Đã thanh toán'
                    GROUP BY YEAR(NgayThanhToan), MONTH(NgayThanhToan)
                    ORDER BY Nam DESC, Thang DESC";
            }
            else
            {
                sql = @"
                    SELECT MONTH(NgayThanhToan) AS Thang, YEAR(NgayThanhToan) AS Nam, 
                           SUM(SoTien) AS DoanhThu, COUNT(*) AS SoHoaDon
                    FROM ThanhToan
                    WHERE YEAR(NgayThanhToan) = @nam AND TrangThai = N'Đã thanh toán'
                    GROUP BY MONTH(NgayThanhToan), YEAR(NgayThanhToan)
                    ORDER BY Thang";
            }

            using (var cmd = new SqlCommand(sql, conn))
            {
                if (nam > 0)
                {
                    cmd.Parameters.AddWithValue("@nam", nam);
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new BaoCaoDoanhThuThang
                        {
                            Thang = Convert.ToInt32(reader["Thang"]),
                            Nam = Convert.ToInt32(reader["Nam"]),
                            DoanhThu = Convert.ToDecimal(reader["DoanhThu"]),
                            SoHoaDon = Convert.ToInt32(reader["SoHoaDon"])
                        });
                    }
                }
            }
            return list;
        }

        private static List<BaoCaoAnPhamBanChay> GetAnPhamBanChayInternal(SqlConnection conn, int top)
        {
            var list = new List<BaoCaoAnPhamBanChay>();
            string sql = $@"
                SELECT TOP {top} ap.MaAnPham, ap.TenAnPham, 
                       COUNT(dk.MaDangKy) AS SoLuongDangKy,
                       ISNULL(SUM(hd.ThanhTien), 0) AS TongTien
                FROM AnPham ap
                LEFT JOIN DangKy dk ON ap.MaAnPham = dk.MaAnPham
                LEFT JOIN HoaDon hd ON dk.MaDangKy = hd.MaDangKy AND hd.TrangThai = N'Đã thanh toán'
                GROUP BY ap.MaAnPham, ap.TenAnPham
                ORDER BY SoLuongDangKy DESC";

            using (var cmd = new SqlCommand(sql, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new BaoCaoAnPhamBanChay
                        {
                            MaAnPham = Convert.ToInt32(reader["MaAnPham"]),
                            TenAnPham = reader["TenAnPham"].ToString() ?? "",
                            SoLuongDangKy = Convert.ToInt32(reader["SoLuongDangKy"]),
                            TongTien = Convert.ToDecimal(reader["TongTien"])
                        });
                    }
                }
            }
            return list;
        }
    }
}
