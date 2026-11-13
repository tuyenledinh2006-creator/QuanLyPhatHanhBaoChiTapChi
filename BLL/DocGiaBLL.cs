using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class DocGiaBLL
    {
        private readonly DocGiaDAL _dal = new();
        private readonly AuditLogBLL _audit = new();
        private readonly LichSuPhienBanBLL _version = new();

        public List<DocGiaModel> GetAll(bool baoGomDaXoa = false)
        {
            return _dal.GetAll(baoGomDaXoa);
        }

        public DocGiaModel? GetById(int id, bool baoGomDaXoa = false)
        {
            if (id <= 0) return null;
            return _dal.GetById(id, baoGomDaXoa);
        }

        public DocGiaModel? GetByMaNguoiDung(int maNguoiDung)
        {
            if (maNguoiDung <= 0) return null;
            return _dal.GetByMaNguoiDung(maNguoiDung);
        }

        public int Insert(DocGiaModel model, string? userThucHien = null)
        {
            if (model.MaNguoiDung <= 0)
                throw new ArgumentException("Mã tài khoản người dùng không hợp lệ.");

            int id = _dal.Insert(model);
            model.MaDocGia = id;
            model.Version = 1;

            _audit.GhiNhatKy(null, userThucHien, "ThemMoi", "DocGia", id.ToString(), null, model, "Thêm mới hồ sơ độc giả");
            _version.LuuPhienBan("DocGia", id.ToString(), 1, model, userThucHien, "Khởi tạo hồ sơ độc giả phiên bản đầu tiên");
            return id;
        }

        public bool Update(DocGiaModel model, string? userThucHien = null)
        {
            if (model.MaDocGia <= 0)
                throw new ArgumentException("Mã độc giả không hợp lệ.");

            var old = _dal.GetById(model.MaDocGia);
            bool success = _dal.Update(model);
            if (success)
            {
                var updated = _dal.GetById(model.MaDocGia);
                int ver = updated?.Version ?? (old?.Version + 1 ?? 2);
                _audit.GhiNhatKy(null, userThucHien, "CapNhat", "DocGia", model.MaDocGia.ToString(), old, updated, "Cập nhật thông tin độc giả");
                if (updated != null)
                {
                    _version.LuuPhienBan("DocGia", model.MaDocGia.ToString(), ver, updated, userThucHien, "Cập nhật thông tin độc giả");
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
                _audit.GhiNhatKy(null, deletedBy, "SoftDelete", "DocGia", id.ToString(), old, updated, "Xóa mềm hồ sơ độc giả");
                if (updated != null)
                {
                    _version.LuuPhienBan("DocGia", id.ToString(), ver, updated, deletedBy, "Xóa mềm (chuyển vào thùng rác)");
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
                _audit.GhiNhatKy(null, restoredBy, "Restore", "DocGia", id.ToString(), old, updated, "Khôi phục hồ sơ độc giả đã xóa");
                if (updated != null)
                {
                    _version.LuuPhienBan("DocGia", id.ToString(), ver, updated, restoredBy, "Khôi phục hồ sơ độc giả từ thùng rác");
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
