using System;

namespace Models
{
    public class DocGiaModel
    {
        public int MaDocGia { get; set; }
        public int MaNguoiDung { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string? DiaChi { get; set; }

        // Thông tin liên kết từ bảng NguoiDung
        public string? TenDangNhap { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }

        // Soft-Delete & Versioning
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public int Version { get; set; } = 1;
    }
}
