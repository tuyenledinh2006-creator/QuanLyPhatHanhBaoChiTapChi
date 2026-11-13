using System;

namespace Models
{
    public class AnPhamModel
    {
        public int MaAnPham { get; set; }
        public int MaDanhMuc { get; set; }
        public string TenAnPham { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public decimal Gia { get; set; }
        public string? ChuKyPhatHanh { get; set; }
        public bool TrangThai { get; set; } = true;

        // Thông tin liên kết
        public string? TenDanhMuc { get; set; }
    }
}
