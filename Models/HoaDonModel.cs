using System;

namespace Models
{
    public class HoaDonModel
    {
        public int MaHoaDon { get; set; }
        public int MaDangKy { get; set; }
        public DateOnly NgayLap { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public decimal TongTien { get; set; }
        public decimal GiamGia { get; set; } = 0;
        public decimal ThanhTien { get; set; }
        public string TrangThai { get; set; } = "Chưa thanh toán"; // Chưa thanh toán, Đã thanh toán

        // Thông tin liên kết
        public string? TenDocGia { get; set; }
        public string? TenAnPham { get; set; }

        // Soft-Delete & Versioning
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public int Version { get; set; } = 1;
    }
}
