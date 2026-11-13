using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class SoBaoBLL
    {
        private readonly SoBaoDAL _dal = new();

        public List<SoBaoModel> GetAll()
        {
            return _dal.GetAll();
        }

        public SoBaoModel? GetById(int id)
        {
            if (id <= 0) return null;
            return _dal.GetById(id);
        }

        public List<SoBaoModel> GetByAnPham(int maAnPham)
        {
            if (maAnPham <= 0) return new List<SoBaoModel>();
            return _dal.GetByAnPham(maAnPham);
        }

        public int Insert(SoBaoModel model)
        {
            if (model.MaAnPham <= 0)
                throw new ArgumentException("Mã ấn phẩm không hợp lệ.");
            if (model.SoThuTu <= 0)
                throw new ArgumentException("Số thứ tự phải lớn hơn 0.");
            if (model.SoLuongIn <= 0)
                throw new ArgumentException("Số lượng in phải lớn hơn 0.");

            var today = DateOnly.FromDateTime(DateTime.Today);
            bool laDaPhatHanh = model.TrangThai == "Đã phát hành" || model.TrangThai == "Đang phát hành" || model.TrangThai == "Phát hành";
            if (laDaPhatHanh && model.NgayPhatHanh > today)
            {
                throw new ArgumentException("Ngày phát hành không hợp lệ.");
            }

            if (model.SoLuongDaPhatHanh > model.SoLuongIn)
            {
                throw new ArgumentException($"Số lượng đã phát hành ({model.SoLuongDaPhatHanh}) không được vượt quá số lượng in ({model.SoLuongIn}).");
            }

            int id = _dal.Insert(model);
            if (id > 0 && laDaPhatHanh)
            {
                try
                {
                    var anPham = new AnPhamDAL().GetById(model.MaAnPham);
                    string tenAnPham = anPham?.TenAnPham ?? model.TenAnPham ?? "Báo/Tạp chí";
                    string tieuDe = "Số báo mới";
                    string noiDung = $"{tenAnPham} số #{model.SoThuTu} đã được phát hành ngày {model.NgayPhatHanh:dd/MM/yyyy}.";
                    new ThongBaoBLL().GuiThongBaoTatCa(tieuDe, noiDung, model.NgayPhatHanh);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi tự động gửi thông báo phát hành: " + ex.Message);
                }
            }

            return id;
        }

        public bool Update(SoBaoModel model)
        {
            if (model.MaSoBao <= 0)
                throw new ArgumentException("Mã số báo không hợp lệ.");
            if (model.MaAnPham <= 0)
                throw new ArgumentException("Mã ấn phẩm không hợp lệ.");
            if (model.SoLuongIn <= 0)
                throw new ArgumentException("Số lượng in phải lớn hơn 0.");

            var today = DateOnly.FromDateTime(DateTime.Today);
            bool laDaPhatHanh = model.TrangThai == "Đã phát hành" || model.TrangThai == "Đang phát hành" || model.TrangThai == "Phát hành";
            if (laDaPhatHanh && model.NgayPhatHanh > today)
            {
                throw new ArgumentException("Ngày phát hành không hợp lệ.");
            }

            var old = _dal.GetById(model.MaSoBao);
            if (old != null && model.SoLuongIn < old.SoLuongDaPhatHanh)
            {
                throw new ArgumentException($"Số lượng in ({model.SoLuongIn}) không được nhỏ hơn số lượng đã phát hành ({old.SoLuongDaPhatHanh} bản).");
            }

            bool result = _dal.Update(model);
            if (result)
            {
                bool oldChuaPhatHanh = old == null || (old.TrangThai != "Đã phát hành" && old.TrangThai != "Đang phát hành" && old.TrangThai != "Phát hành");
                if (oldChuaPhatHanh && laDaPhatHanh)
                {
                    try
                    {
                        var anPham = new AnPhamDAL().GetById(model.MaAnPham);
                        string tenAnPham = anPham?.TenAnPham ?? model.TenAnPham ?? "Báo/Tạp chí";
                        string tieuDe = "Số báo mới";
                        string noiDung = $"{tenAnPham} số #{model.SoThuTu} đã được phát hành ngày {model.NgayPhatHanh:dd/MM/yyyy}.";
                        new ThongBaoBLL().GuiThongBaoTatCa(tieuDe, noiDung, model.NgayPhatHanh);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Lỗi tự động gửi thông báo phát hành: " + ex.Message);
                    }
                }
            }

            return result;
        }

        public bool UpdateSoLuongPhatHanh(int maSoBao, int soLuongThem)
        {
            if (maSoBao <= 0 || soLuongThem <= 0) return false;
            return _dal.UpdateSoLuongPhatHanh(maSoBao, soLuongThem);
        }

        public bool Delete(int id)
        {
            if (id <= 0) return false;
            return _dal.Delete(id);
        }
    }
}
