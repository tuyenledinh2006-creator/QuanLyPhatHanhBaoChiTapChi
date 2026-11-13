using System;
using System.Collections.Generic;

namespace Models
{
    public class BaoCaoModel
    {
        public int TongNguoiDung { get; set; }
        public int TongDocGia { get; set; }
        public int TongAnPham { get; set; }
        public int TongSoBao { get; set; }
        public int TongDangKy { get; set; }
        public int TongHoaDon { get; set; }
        public decimal TongDoanhThu { get; set; }
        public List<BaoCaoDoanhThuThang> DoanhThuTheoThang { get; set; } = new();
        public List<BaoCaoAnPhamBanChay> AnPhamPhoBien { get; set; } = new();
    }

    public class BaoCaoDoanhThuThang
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoHoaDon { get; set; }
    }

    public class BaoCaoAnPhamBanChay
    {
        public int MaAnPham { get; set; }
        public string TenAnPham { get; set; } = string.Empty;
        public int SoLuongDangKy { get; set; }
        public decimal TongTien { get; set; }
    }
}
