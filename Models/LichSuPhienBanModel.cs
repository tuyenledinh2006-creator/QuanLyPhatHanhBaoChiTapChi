using System;

namespace Models
{
    public class LichSuPhienBanModel
    {
        public int Id { get; set; }
        public string Bang { get; set; } = string.Empty;
        public string KhoaChinh { get; set; } = string.Empty;
        public int SoPhienBan { get; set; }
        public string DuLieuSnapshot { get; set; } = string.Empty;
        public string? NguoiCapNhat { get; set; }
        public DateTime ThoiGianCapNhat { get; set; } = DateTime.Now;
        public string? GhiChu { get; set; }
    }
}
