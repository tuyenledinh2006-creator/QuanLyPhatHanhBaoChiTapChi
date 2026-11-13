using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class NguoiDungBLL
    {
        private readonly NguoiDungDAL _dal = new();

        public List<NguoiDungModel> GetAll()
        {
            return _dal.GetAll();
        }

        public NguoiDungModel? GetById(int id)
        {
            if (id <= 0) return null;
            return _dal.GetById(id);
        }

        public NguoiDungModel? GetByTenDangNhap(string tenDangNhap)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap)) return null;
            return _dal.GetByTenDangNhap(tenDangNhap.Trim());
        }

        public NguoiDungModel? GetByRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;
            return _dal.GetByRefreshToken(refreshToken.Trim());
        }

        public bool SaveRefreshToken(int maNguoiDung, string refreshToken, DateTime expiryTime)
        {
            if (maNguoiDung <= 0 || string.IsNullOrWhiteSpace(refreshToken)) return false;
            return _dal.UpdateRefreshToken(maNguoiDung, refreshToken, expiryTime);
        }

        public bool RevokeRefreshToken(int maNguoiDung)
        {
            if (maNguoiDung <= 0) return false;
            return _dal.RevokeRefreshToken(maNguoiDung);
        }

        public bool RevokeRefreshTokenByToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return false;
            return _dal.RevokeRefreshTokenByToken(refreshToken.Trim());
        }

        public NguoiDungModel? Login(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
                return null;

            return _dal.Login(tenDangNhap.Trim(), matKhau.Trim());
        }

        public int Insert(NguoiDungModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TenDangNhap))
                throw new ArgumentException("Tên đăng nhập không được để trống.");
            if (System.Text.RegularExpressions.Regex.IsMatch(model.TenDangNhap, @"\s"))
                throw new ArgumentException("Tên đăng nhập không được chứa khoảng trắng.");
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.TenDangNhap, @"^[a-zA-Z0-9_]{3,50}$"))
                throw new ArgumentException("Tên đăng nhập phải từ 3 đến 50 ký tự, viết liền không dấu và không chứa ký tự đặc biệt.");

            if (string.IsNullOrWhiteSpace(model.MatKhau))
                throw new ArgumentException("Mật khẩu không được để trống.");
            if (model.MatKhau.Length < 6)
                throw new ArgumentException("Mật khẩu phải có tối thiểu 6 ký tự.");

            if (string.IsNullOrWhiteSpace(model.HoTen))
                throw new ArgumentException("Họ tên không được để trống.");
            if (System.Text.RegularExpressions.Regex.IsMatch(model.HoTen, @"\d"))
                throw new ArgumentException("Họ tên không được chứa chữ số.");

            if (string.IsNullOrWhiteSpace(model.Email))
                throw new ArgumentException("Email không được để trống.");
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Định dạng email không hợp lệ (ví dụ: docgia@gmail.com).");

            if (!string.IsNullOrWhiteSpace(model.SoDienThoai))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(model.SoDienThoai, @"[a-zA-Z]"))
                    throw new ArgumentException("Số điện thoại không được chứa chữ cái.");
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.SoDienThoai, @"^0\d{9,10}$"))
                    throw new ArgumentException("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0.");
            }

            if (string.IsNullOrWhiteSpace(model.QuyenHan))
            {
                model.QuyenHan = "Độc giả";
            }

            var existing = _dal.GetByTenDangNhap(model.TenDangNhap);
            if (existing != null)
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");

            return _dal.Insert(model);
        }

        public bool Update(NguoiDungModel model)
        {
            if (model.MaNguoiDung <= 0)
                throw new ArgumentException("Mã người dùng không hợp lệ.");
            if (string.IsNullOrWhiteSpace(model.HoTen))
                throw new ArgumentException("Họ tên không được để trống.");
            if (System.Text.RegularExpressions.Regex.IsMatch(model.HoTen, @"\d"))
                throw new ArgumentException("Họ tên không được chứa chữ số.");

            if (string.IsNullOrWhiteSpace(model.Email))
                throw new ArgumentException("Email không được để trống.");
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Định dạng email không hợp lệ (ví dụ: docgia@gmail.com).");

            if (!string.IsNullOrWhiteSpace(model.SoDienThoai))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(model.SoDienThoai, @"[a-zA-Z]"))
                    throw new ArgumentException("Số điện thoại không được chứa chữ cái.");
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.SoDienThoai, @"^0\d{9,10}$"))
                    throw new ArgumentException("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0.");
            }

            if (string.IsNullOrWhiteSpace(model.QuyenHan))
            {
                var existing = _dal.GetById(model.MaNguoiDung);
                model.QuyenHan = existing?.QuyenHan ?? "Độc giả";
            }

            if (!string.IsNullOrWhiteSpace(model.MatKhau) && model.MatKhau.Length < 6)
                throw new ArgumentException("Mật khẩu mới phải có tối thiểu 6 ký tự.");

            bool ok = _dal.Update(model);
            if (ok && model.QuyenHan == "Độc giả")
            {
                var dgBll = new DocGiaBLL();
                var dg = dgBll.GetByMaNguoiDung(model.MaNguoiDung);
                if (dg == null)
                {
                    try
                    {
                        dgBll.Insert(new DocGiaModel
                        {
                            MaNguoiDung = model.MaNguoiDung,
                            DiaChi = "Chưa cập nhật"
                        });
                    }
                    catch { }
                }
            }
            return ok;
        }

        public bool UpdateQuyenHan(int id, string quyenHan)
        {
            if (id <= 0) return false;
            if (string.IsNullOrWhiteSpace(quyenHan))
                throw new ArgumentException("Quyền hạn không được để trống.");

            return _dal.UpdateQuyenHan(id, quyenHan.Trim());
        }

        public bool ChangePassword(int id, string oldPassword, string newPassword)
        {
            if (id <= 0) return false;
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Mật khẩu mới không được để trống.");

            var user = _dal.GetById(id);
            if (user == null || user.MatKhau != oldPassword)
                throw new InvalidOperationException("Mật khẩu cũ không chính xác.");

            return _dal.ChangePassword(id, newPassword);
        }

        public bool Delete(int id)
        {
            if (id <= 0) return false;
            return _dal.Delete(id);
        }
    }
}
