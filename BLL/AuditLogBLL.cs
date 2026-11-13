using System;
using System.Collections.Generic;
using System.Text.Json;
using DAL;
using Models;

namespace BLL
{
    public class AuditLogBLL
    {
        private readonly AuditLogDAL _dal = new();

        public int GhiNhatKy(int? userId, string? tenDangNhap, string hanhDong, string bang, string? khoaChinh, object? duLieuCu, object? duLieuMoi, string? moTa = null, string? ip = null)
        {
            try
            {
                string? strCu = duLieuCu != null ? (duLieuCu is string s ? s : JsonSerializer.Serialize(duLieuCu)) : null;
                string? strMoi = duLieuMoi != null ? (duLieuMoi is string s2 ? s2 : JsonSerializer.Serialize(duLieuMoi)) : null;

                var log = new AuditLogModel
                {
                    UserId = userId,
                    TenDangNhap = tenDangNhap,
                    HanhDong = hanhDong,
                    Bang = bang,
                    KhoaChinh = khoaChinh,
                    DuLieuCu = strCu,
                    DuLieuMoi = strMoi,
                    MoTa = moTa,
                    ThoiGian = DateTime.Now,
                    IpAddress = ip
                };
                return _dal.Insert(log);
            }
            catch
            {
                return 0;
            }
        }

        public List<AuditLogModel> LayDanhSach(string? bang = null, string? hanhDong = null, int limit = 100)
        {
            return _dal.GetAll(bang, hanhDong, limit);
        }
    }
}
