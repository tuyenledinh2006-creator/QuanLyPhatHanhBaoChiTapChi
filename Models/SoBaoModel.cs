using System;

namespace Models
{
    public class SoBaoModel
    {
        public int MaSoBao { get; set; }
        public int MaAnPham { get; set; }
        public int SoThuTu { get; set; }
        public DateOnly NgayPhatHanh { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public int SoLuongIn { get; set; }
        public int SoLuongDaPhatHanh { get; set; } = 0;
        public string TrangThai { get; set; } = "Chuẩn bị phát hành";

        // Thông tin liên kết
        public string? TenAnPham { get; set; }
    }
}
