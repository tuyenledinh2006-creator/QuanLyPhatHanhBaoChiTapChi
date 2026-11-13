using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class AnPhamBLL
    {
        private readonly AnPhamDAL _dal = new();

        public List<AnPhamModel> GetAll()
        {
            return _dal.GetAll();
        }

        public AnPhamModel? GetById(int id)
        {
            if (id <= 0) return null;
            return _dal.GetById(id);
        }

        public List<AnPhamModel> GetByDanhMuc(int maDanhMuc)
        {
            if (maDanhMuc <= 0) return new List<AnPhamModel>();
            return _dal.GetByDanhMuc(maDanhMuc);
        }

        public int Insert(AnPhamModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TenAnPham))
                throw new ArgumentException("Tên ấn phẩm không được để trống.");
            if (model.TenAnPham.Trim().Length < 2)
                throw new ArgumentException("Tên ấn phẩm phải có ít nhất 2 ký tự.");
            if (System.Text.RegularExpressions.Regex.IsMatch(model.TenAnPham, @"^[0-9]+$"))
                throw new ArgumentException("Tên ấn phẩm không được chỉ chứa chữ số, phải có tên chữ cái.");
            if (model.Gia < 1000)
                throw new ArgumentException("Đơn giá ấn phẩm tối thiểu là 1.000 VNĐ.");
            if (model.Gia > 50000000)
                throw new ArgumentException("Đơn giá ấn phẩm không được vượt quá 50.000.000 VNĐ.");

            if (model.MaDanhMuc <= 0)
                model.MaDanhMuc = 1;

            if (string.IsNullOrWhiteSpace(model.ChuKyPhatHanh))
                model.ChuKyPhatHanh = "Hàng ngày";

            int id = _dal.Insert(model);
            if (id > 0)
            {
                try
                {
                    string tieuDe = "Ấn phẩm mới ra mắt";
                    string noiDung = $"Tòa soạn trân trọng giới thiệu ấn phẩm mới: \"{model.TenAnPham}\" ({model.ChuKyPhatHanh}) với giá {model.Gia:N0} đ. Quý độc giả có thể đăng ký đặt báo ngay từ hôm nay!";
                    new ThongBaoBLL().GuiThongBaoTatCa(tieuDe, noiDung, DateOnly.FromDateTime(DateTime.Now));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi tự động gửi thông báo ấn phẩm mới: " + ex.Message);
                }
            }

            return id;
        }

        public bool Update(AnPhamModel model)
        {
            if (model.MaAnPham <= 0)
                throw new ArgumentException("Mã ấn phẩm không hợp lệ.");
            if (string.IsNullOrWhiteSpace(model.TenAnPham))
                throw new ArgumentException("Tên ấn phẩm không được để trống.");
            if (model.TenAnPham.Trim().Length < 2)
                throw new ArgumentException("Tên ấn phẩm phải có ít nhất 2 ký tự.");
            if (System.Text.RegularExpressions.Regex.IsMatch(model.TenAnPham, @"^[0-9]+$"))
                throw new ArgumentException("Tên ấn phẩm không được chỉ chứa chữ số, phải có tên chữ cái.");
            if (model.Gia < 1000)
                throw new ArgumentException("Đơn giá ấn phẩm tối thiểu là 1.000 VNĐ.");
            if (model.Gia > 50000000)
                throw new ArgumentException("Đơn giá ấn phẩm không được vượt quá 50.000.000 VNĐ.");

            if (model.MaDanhMuc <= 0)
                model.MaDanhMuc = 1;

            return _dal.Update(model);
        }

        public bool Delete(int id)
        {
            if (id <= 0) return false;
            return _dal.Delete(id);
        }
    }
}
