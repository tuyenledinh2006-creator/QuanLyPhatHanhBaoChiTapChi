using System;

namespace Models
{
    public class DangKyModel
    {
        public int MaDangKy { get; set; }
        public int MaDocGia { get; set; }
        public int MaAnPham { get; set; }
        public DateOnly NgayDangKy { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly NgayBatDau { get; set; }
        public DateOnly NgayKetThuc { get; set; }
        public int KyHan { get; set; } // Số tháng
        public int SoLuong { get; set; } = 1;
        public string TrangThai { get; set; } = "Chờ thanh toán";

        // Thông tin liên kết
        public string? TenDocGia { get; set; }
        public string? TenAnPham { get; set; }
        public decimal? GiaAnPham { get; set; }
        public int? MaSoBao { get; set; }
        public int? SoThuTuBao { get; set; }
        public string? TrangThaiGiaoHang { get; set; }
        public int? MaPhatHanh { get; set; }

        // Soft-Delete & Versioning
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public int Version { get; set; } = 1;
    }
}
