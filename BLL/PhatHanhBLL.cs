using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class PhatHanhBLL
    {
        private readonly PhatHanhDAL _dal = new();
        private readonly SoBaoDAL _soBaoDal = new();
        private readonly AuditLogBLL _audit = new();
        private readonly LichSuPhienBanBLL _version = new();

        public List<PhatHanhModel> GetAll(bool baoGomDaXoa = false)
        {
            return _dal.GetAll(baoGomDaXoa);
        }

        public PhatHanhModel? GetById(int id, bool baoGomDaXoa = false)
        {
            if (id <= 0) return null;
            return _dal.GetById(id, baoGomDaXoa);
        }

        public List<PhatHanhModel> GetByDangKy(int maDangKy)
        {
            if (maDangKy <= 0) return new List<PhatHanhModel>();
            return _dal.GetByDangKy(maDangKy);
        }

        public List<PhatHanhModel> GetBySoBao(int maSoBao)
        {
            if (maSoBao <= 0) return new List<PhatHanhModel>();
            return _dal.GetBySoBao(maSoBao);
        }

        public List<PhatHanhModel> GetByDocGia(int maDocGia)
        {
            if (maDocGia <= 0) return new List<PhatHanhModel>();
            return _dal.GetByDocGia(maDocGia);
        }

        public int Insert(PhatHanhModel model, string? userThucHien = null)
        {
            if (model.MaDangKy <= 0)
                throw new ArgumentException("Mã đăng ký không hợp lệ.");
            if (model.MaSoBao <= 0)
                throw new ArgumentException("Mã số báo không hợp lệ.");
            if (model.SoLuong <= 0)
                model.SoLuong = 1;

            if (model.NgayPhatHanh == default)
                model.NgayPhatHanh = DateOnly.FromDateTime(DateTime.Today);

            if (string.IsNullOrWhiteSpace(model.TrangThai))
                model.TrangThai = "Chờ xử lý";

            model.Version = 1;
            int id = _dal.Insert(model);
            model.MaPhatHanh = id;

            // Cập nhật số lượng đã phát hành của số báo
            _soBaoDal.UpdateSoLuongPhatHanh(model.MaSoBao, model.SoLuong);

            _audit.GhiNhatKy(null, userThucHien, "ThemMoi", "PhatHanh", id.ToString(), null, model, "Tạo lệnh phát hành báo mới");
            _version.LuuPhienBan("PhatHanh", id.ToString(), 1, model, userThucHien, "Khởi tạo lệnh phát hành");

            return id;
        }

        public bool Update(PhatHanhModel model, string? userThucHien = null)
        {
            if (model.MaPhatHanh <= 0)
                throw new ArgumentException("Mã phát hành không hợp lệ.");

            var old = _dal.GetById(model.MaPhatHanh);
            if (old != null && old.TrangThai == "Đã giao" && model.TrangThai != "Đã giao")
            {
                throw new InvalidOperationException("Lệnh phát hành đã ở trạng thái 'Đã giao', không thể chuyển lại trạng thái khác.");
            }
            bool success = _dal.Update(model);
            if (success)
            {
                var updated = _dal.GetById(model.MaPhatHanh);
                int ver = updated?.Version ?? (old?.Version + 1 ?? 2);
                _audit.GhiNhatKy(null, userThucHien, "CapNhat", "PhatHanh", model.MaPhatHanh.ToString(), old, updated, "Cập nhật lệnh phát hành");
                if (updated != null)
                {
                    _version.LuuPhienBan("PhatHanh", model.MaPhatHanh.ToString(), ver, updated, userThucHien, "Cập nhật lệnh phát hành");
                }
            }
            return success;
        }

        public bool UpdateTrangThai(int id, string trangThai, string? userThucHien = null)
        {
            if (id <= 0) return false;
            var old = _dal.GetById(id);
            if (old != null && old.TrangThai == "Đã giao")
            {
                throw new InvalidOperationException("Lệnh phát hành đã ở trạng thái 'Đã giao', không thể chuyển lại trạng thái khác.");
            }
            bool success = _dal.UpdateTrangThai(id, trangThai);
            if (success)
            {
                var updated = _dal.GetById(id);
                int ver = updated?.Version ?? (old?.Version + 1 ?? 2);
                _audit.GhiNhatKy(null, userThucHien, "DoiTrangThai", "PhatHanh", id.ToString(), old, updated, $"Cập nhật trạng thái phát hành '{trangThai}'");
                if (updated != null)
                {
                    _version.LuuPhienBan("PhatHanh", id.ToString(), ver, updated, userThucHien, $"Cập nhật tiến độ: {trangThai}");
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
                _audit.GhiNhatKy(null, deletedBy, "SoftDelete", "PhatHanh", id.ToString(), old, updated, "Hủy/Xóa mềm lệnh phát hành");
                if (updated != null)
                {
                    _version.LuuPhienBan("PhatHanh", id.ToString(), ver, updated, deletedBy, "Xóa mềm lệnh phát hành (thùng rác)");
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
                _audit.GhiNhatKy(null, restoredBy, "Restore", "PhatHanh", id.ToString(), old, updated, "Khôi phục lệnh phát hành");
                if (updated != null)
                {
                    _version.LuuPhienBan("PhatHanh", id.ToString(), ver, updated, restoredBy, "Khôi phục lệnh phát hành từ thùng rác");
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
