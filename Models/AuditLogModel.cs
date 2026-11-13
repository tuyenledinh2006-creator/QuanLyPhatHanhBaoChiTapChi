using System;

namespace Models
{
    public class AuditLogModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? TenDangNhap { get; set; }
        public string HanhDong { get; set; } = string.Empty; // Insert, Update, SoftDelete, Restore, Export, ThanhToan
        public string Bang { get; set; } = string.Empty;     // DocGia, DangKy, PhatHanh, HoaDon, ThanhToan, AnPham...
        public string? KhoaChinh { get; set; }
        public string? DuLieuCu { get; set; }
        public string? DuLieuMoi { get; set; }
        public string? MoTa { get; set; }
        public DateTime ThoiGian { get; set; } = DateTime.Now;
        public string? IpAddress { get; set; }
    }
}
