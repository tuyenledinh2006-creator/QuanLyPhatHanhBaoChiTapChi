using System;
using System.Collections.Generic;
using System.Text.Json;
using DAL;
using Models;

namespace BLL
{
    public class LichSuPhienBanBLL
    {
        private readonly LichSuPhienBanDAL _dal = new();

        public int LuuPhienBan(string bang, string khoaChinh, int soPhienBan, object duLieuSnapshot, string? nguoiCapNhat = null, string? ghiChu = null)
        {
            try
            {
                string jsonSnapshot = duLieuSnapshot is string s ? s : JsonSerializer.Serialize(duLieuSnapshot);
                var model = new LichSuPhienBanModel
                {
                    Bang = bang,
                    KhoaChinh = khoaChinh,
                    SoPhienBan = soPhienBan,
                    DuLieuSnapshot = jsonSnapshot,
                    NguoiCapNhat = nguoiCapNhat,
                    ThoiGianCapNhat = DateTime.Now,
                    GhiChu = ghiChu
                };
                return _dal.Insert(model);
            }
            catch
            {
                return 0;
            }
        }

        public List<LichSuPhienBanModel> LayTheoThucThe(string bang, string khoaChinh)
        {
            return _dal.GetByEntity(bang, khoaChinh);
        }
    }
}
