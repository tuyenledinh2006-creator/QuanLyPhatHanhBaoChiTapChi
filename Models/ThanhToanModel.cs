using System;

namespace Models
{
    public class ThanhToanModel
    {
        public int MaThanhToan { get; set; }
        public int MaHoaDon { get; set; }
        public DateOnly NgayThanhToan { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public decimal SoTien { get; set; }
        public string PhuongThuc { get; set; } = "Chuyển khoản"; // Chuyển khoản, Tiền mặt, Ví điện tử
        public string TrangThai { get; set; } = "Đã thanh toán";

        // Thông tin liên kết
        public int? MaDangKy { get; set; }
        public string? TenDocGia { get; set; }

        // Soft-Delete & Versioning
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public int Version { get; set; } = 1;
    }
}
