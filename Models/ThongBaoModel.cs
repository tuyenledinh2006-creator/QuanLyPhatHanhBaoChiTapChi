using System;

namespace Models
{
    public class ThongBaoModel
    {
        public int MaThongBao { get; set; }
        public int MaDocGia { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public DateOnly NgayGui { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public bool TrangThai { get; set; } = false; // 0: Chưa đọc, 1: Đã đọc

        // Thông tin liên kết
        public string? TenDocGia { get; set; }
    }
}
