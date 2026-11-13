using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class DangKyBLL
    {
        private readonly DangKyDAL _dal = new();
        private readonly AnPhamDAL _anPhamDal = new();
        private readonly HoaDonDAL _hoaDonDal = new();
        private readonly PhatHanhDAL _phatHanhDal = new();
        private readonly DocGiaDAL _docGiaDal = new();
        private readonly SoBaoDAL _soBaoDal = new();
        private readonly AuditLogBLL _audit = new();
        private readonly LichSuPhienBanBLL _version = new();

        public List<DangKyModel> GetAll(bool baoGomDaXoa = false)
        {
            return _dal.GetAll(baoGomDaXoa);
        }

        public DangKyModel? GetById(int id, bool baoGomDaXoa = false)
        {
            if (id <= 0) return null;
            return _dal.GetById(id, baoGomDaXoa);
        }

        public List<DangKyModel> GetByDocGia(int maDocGia, bool baoGomDaXoa = false)
        {
            if (maDocGia <= 0) return new List<DangKyModel>();
            return _dal.GetByDocGia(maDocGia, baoGomDaXoa);
        }

        public List<DangKyModel> GetByAnPham(int maAnPham)
        {
            if (maAnPham <= 0) return new List<DangKyModel>();
            return _dal.GetByAnPham(maAnPham);
        }

        public List<DangKyModel> GetByAnPhamAndDocGia(int maAnPham, int maDocGia)
        {
            if (maAnPham <= 0 || maDocGia <= 0) return new List<DangKyModel>();
            return _dal.GetByAnPhamAndDocGia(maAnPham, maDocGia);
        }

        public int Insert(DangKyModel model, string? userThucHien = null)
        {
            if (model.MaDocGia <= 0)
                throw new ArgumentException("Mã độc giả không hợp lệ.");
            if (model.MaAnPham <= 0)
                throw new ArgumentException("Mã ấn phẩm không hợp lệ.");
            if (model.KyHan < 0)
                throw new ArgumentException("Kỳ hạn đăng ký không hợp lệ.");
            if (model.SoLuong <= 0)
                model.SoLuong = 1;

            // Kiểm tra số lượng mua không được vượt quá số lượng in
            if (model.KyHan == 0 && model.MaSoBao.HasValue && model.MaSoBao.Value > 0)
            {
                var soBao = _soBaoDal.GetById(model.MaSoBao.Value);
                if (soBao != null)
                {
                    if (model.SoLuong > soBao.SoLuongIn)
                    {
                        throw new ArgumentException($"Số lượng mua ({model.SoLuong} bản) không được vượt quá số lượng in ({soBao.SoLuongIn} bản).");
                    }

                    int conLai = Math.Max(0, soBao.SoLuongIn - soBao.SoLuongDaPhatHanh);
                    if (model.SoLuong > conLai)
                    {
                        throw new ArgumentException($"Số lượng mua ({model.SoLuong} bản) vượt quá số lượng bản in còn lại khả dụng của kỳ báo (còn {conLai} bản).");
                    }
                }
            }

            if (model.NgayBatDau == default)
                model.NgayBatDau = DateOnly.FromDateTime(DateTime.Today);

            if (model.NgayKetThuc == default)
            {
                if (model.KyHan == 0)
                    model.NgayKetThuc = model.NgayBatDau; // Mua báo lẻ: trong ngày
                else
                    model.NgayKetThuc = model.NgayBatDau.AddMonths(model.KyHan);
            }

            model.NgayDangKy = DateOnly.FromDateTime(DateTime.Now);
            model.TrangThai = "Chờ thanh toán";
            model.Version = 1;

            int maDangKy = _dal.Insert(model);
            model.MaDangKy = maDangKy;

            // Quy tắc nghiệp vụ: Chưa thanh toán thì CHƯA lập hóa đơn.
            // Hóa đơn chỉ được tạo khi độc giả hoặc quản trị viên thực hiện thanh toán thành công.

            // Audit log & Versioning
            string hanhDongLog = (model.KyHan == 0) ? "Đặt mua báo lẻ mới" : "Đăng ký thuê bao báo mới";
            _audit.GhiNhatKy(null, userThucHien, "ThemMoi", "DangKy", maDangKy.ToString(), null, model, hanhDongLog);
            _version.LuuPhienBan("DangKy", maDangKy.ToString(), 1, model, userThucHien, hanhDongLog);

            return maDangKy;
        }

        public bool Update(DangKyModel model, string? userThucHien = null)
        {
            if (model.MaDangKy <= 0)
                throw new ArgumentException("Mã đăng ký không hợp lệ.");

            var old = _dal.GetById(model.MaDangKy);
            bool success = _dal.Update(model);
            if (success)
            {
                var updated = _dal.GetById(model.MaDangKy);
                int ver = updated?.Version ?? (old?.Version + 1 ?? 2);
                _audit.GhiNhatKy(null, userThucHien, "CapNhat", "DangKy", model.MaDangKy.ToString(), old, updated, "Cập nhật đơn đăng ký");
                if (updated != null)
                {
                    _version.LuuPhienBan("DangKy", model.MaDangKy.ToString(), ver, updated, userThucHien, "Cập nhật đơn đăng ký");
                }
            }
            return success;
        }

        public bool UpdateTrangThai(int id, string trangThai, string? userThucHien = null)
        {
            if (id <= 0) return false;
            var old = _dal.GetById(id);
            bool success = _dal.UpdateTrangThai(id, trangThai);
            if (success)
            {
                var updated = _dal.GetById(id);
                int ver = updated?.Version ?? (old?.Version + 1 ?? 2);
                _audit.GhiNhatKy(null, userThucHien, "DoiTrangThai", "DangKy", id.ToString(), old, updated, $"Đổi trạng thái thành '{trangThai}'");
                if (updated != null)
                {
                    _version.LuuPhienBan("DangKy", id.ToString(), ver, updated, userThucHien, $"Đổi trạng thái: {trangThai}");
                }
            }
            return success;
        }

        public bool SoftDelete(int id, string? deletedBy = null)
        {
            if (id <= 0) return false;
            var old = _dal.GetById(id);
            bool success = _dal.SoftDelete(id, deletedBy);
            if (success)
            {
                // Khi hủy đăng ký: nếu hóa đơn chưa thanh toán, tự động hủy (xóa mềm) hóa đơn tương ứng
                try
                {
                    var hoaDon = _hoaDonDal.GetByDangKy(id);
                    if (hoaDon != null && hoaDon.TrangThai != "Đã thanh toán")
                    {
                        _hoaDonDal.SoftDelete(hoaDon.MaHoaDon, deletedBy);
                        _hoaDonDal.UpdateTrangThai(hoaDon.MaHoaDon, "Đã hủy");
                    }
                }
                catch { }

                if (old != null && old.KyHan == 0 && old.MaSoBao.HasValue && old.MaSoBao.Value > 0)
                {
                    // Chỉ hoàn lại số lượng phát hành nếu đơn đã thanh toán/hoạt động
                    if (old.TrangThai == "Đang hoạt động" || old.TrangThai == "Đã thanh toán")
                    {
                        try
                        {
                            _soBaoDal.UpdateSoLuongPhatHanh(old.MaSoBao.Value, -old.SoLuong);
                        }
                        catch { }
                    }
                }

                var updated = _dal.GetById(id, true);
                int ver = updated?.Version ?? (old?.Version + 1 ?? 2);
                _audit.GhiNhatKy(null, deletedBy, "SoftDelete", "DangKy", id.ToString(), old, updated, "Hủy/Xóa mềm đăng ký thuê bao");
                if (updated != null)
                {
                    _version.LuuPhienBan("DangKy", id.ToString(), ver, updated, deletedBy, "Xóa mềm (chuyển vào thùng rác)");
                }
            }
            return success;
        }

        public bool Restore(int id, string? restoredBy = null)
        {
            if (id <= 0) return false;
            var old = _dal.GetById(id, true);
            bool success = _dal.Restore(id);
            if (success)
            {
                // Khi khôi phục gói đăng ký: khôi phục lại hóa đơn nếu trước đó bị hủy theo gói đăng ký
                try
                {
                    var hoaDon = _hoaDonDal.GetByDangKy(id, true);
                    if (hoaDon != null && (hoaDon.TrangThai == "Đã hủy" || hoaDon.IsDeleted))
                    {
                        _hoaDonDal.Restore(hoaDon.MaHoaDon);
                        _hoaDonDal.UpdateTrangThai(hoaDon.MaHoaDon, "Chưa thanh toán");
                    }
                }
                catch { }

                if (old != null && old.KyHan == 0 && old.MaSoBao.HasValue && old.MaSoBao.Value > 0)
                {
                    if (old.TrangThai == "Đang hoạt động" || old.TrangThai == "Đã thanh toán")
                    {
                        try
                        {
                            _soBaoDal.UpdateSoLuongPhatHanh(old.MaSoBao.Value, old.SoLuong);
                        }
                        catch { }
                    }
                }

                var updated = _dal.GetById(id, false);
                int ver = updated?.Version ?? (old?.Version + 1 ?? 2);
                _audit.GhiNhatKy(null, restoredBy, "Restore", "DangKy", id.ToString(), old, updated, "Khôi phục đăng ký thuê bao");
                if (updated != null)
                {
                    _version.LuuPhienBan("DangKy", id.ToString(), ver, updated, restoredBy, "Khôi phục đơn đăng ký từ thùng rác");
                }
            }
            return success;
        }

        public bool Delete(int id)
        {
            return SoftDelete(id);
        }
    }
}
