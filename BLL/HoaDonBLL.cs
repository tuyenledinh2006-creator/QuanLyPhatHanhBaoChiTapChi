using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class HoaDonBLL
    {
        private readonly HoaDonDAL _dal = new();
        private readonly DangKyDAL _dangKyDal = new();
        private readonly AuditLogBLL _audit = new();
        private readonly LichSuPhienBanBLL _version = new();

        public List<HoaDonModel> GetAll(bool baoGomDaXoa = false)
        {
            return _dal.GetAll(baoGomDaXoa);
        }

        public HoaDonModel? GetById(int id, bool baoGomDaXoa = false)
        {
            if (id <= 0) return null;
            return _dal.GetById(id, baoGomDaXoa);
        }

        public HoaDonModel? GetByDangKy(int maDangKy, bool baoGomDaXoa = false)
        {
            if (maDangKy <= 0) return null;
            return _dal.GetByDangKy(maDangKy, baoGomDaXoa);
        }

        public List<HoaDonModel> GetByDocGia(int maDocGia, bool baoGomDaXoa = false)
        {
            if (maDocGia <= 0) return new List<HoaDonModel>();
            return _dal.GetByDocGia(maDocGia);
        }

        public int Insert(HoaDonModel model, string? userThucHien = null)
        {
            if (model.MaDangKy <= 0)
                throw new ArgumentException("Mã đăng ký không hợp lệ.");

            var dangKy = _dangKyDal.GetById(model.MaDangKy);
            if (dangKy == null)
                throw new ArgumentException($"Không tìm thấy thông tin đăng ký có mã {model.MaDangKy}.");

            var existing = _dal.GetByDangKy(model.MaDangKy);
            if (existing != null)
                throw new ArgumentException($"Đăng ký mã {model.MaDangKy} đã có hóa đơn (Mã hóa đơn: {existing.MaHoaDon}). Mỗi đăng ký chỉ có duy nhất 1 hóa đơn.");

            if (model.TongTien < 0)
                throw new ArgumentException("Tổng tiền không được âm.");

            if (model.NgayLap == default)
                model.NgayLap = DateOnly.FromDateTime(DateTime.Today);

            if (string.IsNullOrWhiteSpace(model.TrangThai))
                model.TrangThai = "Chưa thanh toán";

            model.Version = 1;
            int id = _dal.Insert(model);
            model.MaHoaDon = id;

            _audit.GhiNhatKy(null, userThucHien, "ThemMoi", "HoaDon", id.ToString(), null, model, "Lập hóa đơn thanh toán tiền báo");
            _version.LuuPhienBan("HoaDon", id.ToString(), 1, model, userThucHien, "Khởi tạo hóa đơn");

            return id;
        }

        public bool Update(HoaDonModel model, string? userThucHien = null)
        {
            if (model.MaHoaDon <= 0)
                throw new ArgumentException("Mã hóa đơn không hợp lệ.");

            var old = _dal.GetById(model.MaHoaDon);
            bool success = _dal.Update(model);
            if (success)
            {
                var updated = _dal.GetById(model.MaHoaDon);
                int ver = updated?.Version ?? (old?.Version + 1 ?? 2);
                _audit.GhiNhatKy(null, userThucHien, "CapNhat", "HoaDon", model.MaHoaDon.ToString(), old, updated, "Cập nhật thông tin hóa đơn");
                if (updated != null)
                {
                    _version.LuuPhienBan("HoaDon", model.MaHoaDon.ToString(), ver, updated, userThucHien, "Cập nhật hóa đơn");
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
                _audit.GhiNhatKy(null, userThucHien, "DoiTrangThai", "HoaDon", id.ToString(), old, updated, $"Cập nhật trạng thái hóa đơn '{trangThai}'");
                if (updated != null)
                {
                    _version.LuuPhienBan("HoaDon", id.ToString(), ver, updated, userThucHien, $"Cập nhật trạng thái hóa đơn: {trangThai}");
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
                var updated = _dal.GetById(id, true);
                int ver = updated?.Version ?? (old?.Version + 1 ?? 2);
                _audit.GhiNhatKy(null, deletedBy, "SoftDelete", "HoaDon", id.ToString(), old, updated, "Hủy/Xóa mềm hóa đơn");
                if (updated != null)
                {
                    _version.LuuPhienBan("HoaDon", id.ToString(), ver, updated, deletedBy, "Xóa mềm hóa đơn (thùng rác)");
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
                var updated = _dal.GetById(id, false);
                int ver = updated?.Version ?? (old?.Version + 1 ?? 2);
                _audit.GhiNhatKy(null, restoredBy, "Restore", "HoaDon", id.ToString(), old, updated, "Khôi phục hóa đơn");
                if (updated != null)
                {
                    _version.LuuPhienBan("HoaDon", id.ToString(), ver, updated, restoredBy, "Khôi phục hóa đơn từ thùng rác");
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
