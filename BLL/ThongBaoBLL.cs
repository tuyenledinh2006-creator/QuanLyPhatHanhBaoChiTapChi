using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class ThongBaoBLL
    {
        private readonly ThongBao_DAL _dal = new();

        public List<ThongBaoModel> GetAll()
        {
            return _dal.GetAll();
        }

        public ThongBaoModel? GetById(int id)
        {
            if (id <= 0) return null;
            return _dal.GetById(id);
        }

        public List<ThongBaoModel> GetByDocGia(int maDocGia)
        {
            if (maDocGia <= 0) return new List<ThongBaoModel>();
            return _dal.GetByDocGia(maDocGia);
        }

        public int Insert(ThongBaoModel model)
        {
            if (model.MaDocGia <= 0)
                throw new ArgumentException("Mã độc giả không hợp lệ.");
            if (string.IsNullOrWhiteSpace(model.TieuDe))
                throw new ArgumentException("Tiêu đề thông báo không được để trống.");
            if (string.IsNullOrWhiteSpace(model.NoiDung))
                throw new ArgumentException("Nội dung thông báo không được để trống.");

            if (model.NgayGui == default)
                model.NgayGui = DateOnly.FromDateTime(DateTime.Now);

            return _dal.Insert(model);
        }

        public bool MarkAsRead(int id)
        {
            if (id <= 0) return false;
            return _dal.MarkAsRead(id);
        }

        public bool Delete(int id)
        {
            if (id <= 0) return false;
            return _dal.Delete(id);
        }

        public bool DeleteByDocGia(int maDocGia)
        {
            if (maDocGia <= 0) return false;
            return _dal.DeleteByDocGia(maDocGia);
        }

        public List<int> GuiThongBaoTatCa(string tieuDe, string noiDung, DateOnly? ngayGui = null)
        {
            if (string.IsNullOrWhiteSpace(tieuDe))
                throw new ArgumentException("Tiêu đề thông báo không được để trống.");
            if (string.IsNullOrWhiteSpace(noiDung))
                throw new ArgumentException("Nội dung thông báo không được để trống.");

            var docGiaList = new DocGiaBLL().GetAll(false);
            var resultIds = new List<int>();
            var ngay = ngayGui ?? DateOnly.FromDateTime(DateTime.Now);

            foreach (var dg in docGiaList)
            {
                int id = _dal.Insert(new ThongBaoModel
                {
                    MaDocGia = dg.MaDocGia,
                    TieuDe = tieuDe,
                    NoiDung = noiDung,
                    NgayGui = ngay,
                    TrangThai = false
                });
                resultIds.Add(id);
            }
            return resultIds;
        }
    }
}
