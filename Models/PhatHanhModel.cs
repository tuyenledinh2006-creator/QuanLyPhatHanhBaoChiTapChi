using System;

namespace Models
{
    public class PhatHanhModel
    {
        public int MaPhatHanh { get; set; }
        public int MaDangKy { get; set; }
        public int MaSoBao { get; set; }
        public DateOnly NgayPhatHanh { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public int SoLuong { get; set; } = 1;
        public string? HinhThuc { get; set; } // Giao báo, Nhận trực tiếp
        public string? DiaChiGiao { get; set; }
        public string TrangThai { get; set; } = "Chờ xử lý"; // Chờ xử lý, Đang giao, Đã giao

        // Thông tin liên kết
        public string? TenDocGia { get; set; }
        public string? TenAnPham { get; set; }
        public int? SoThuTuBao { get; set; }
        public string? TrangThaiHoaDon { get; set; }
        public string? TrangThaiDangKy { get; set; }

        // Soft-Delete & Versioning
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public int Version { get; set; } = 1;
    }
}
