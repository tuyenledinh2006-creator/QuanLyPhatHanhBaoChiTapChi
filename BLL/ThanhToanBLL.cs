using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class ThanhToanBLL
    {
        private readonly ThanhToanDAL _dal = new();
        private readonly HoaDonDAL _hoaDonDal = new();
        private readonly DangKyDAL _dangKyDal = new();
        private readonly ThongBao_DAL _thongBaoDal = new();
        private readonly PhatHanhDAL _phatHanhDal = new();
        private readonly SoBaoDAL _soBaoDal = new();
        private readonly DocGiaDAL _docGiaDal = new();
        private readonly AuditLogBLL _audit = new();
        private readonly LichSuPhienBanBLL _version = new();

        public List<ThanhToanModel> GetAll(bool baoGomDaXoa = false)
        {
            return _dal.GetAll(baoGomDaXoa);
        }

        public ThanhToanModel? GetById(int id, bool baoGomDaXoa = false)
        {
            if (id <= 0) return null;
            return _dal.GetById(id, baoGomDaXoa);
        }

        public List<ThanhToanModel> GetByHoaDon(int maHoaDon)
        {
            if (maHoaDon <= 0) return new List<ThanhToanModel>();
            return _dal.GetByHoaDon(maHoaDon);
        }

        public List<ThanhToanModel> GetByDocGia(int maDocGia, bool baoGomDaXoa = false)
        {
            if (maDocGia <= 0) return new List<ThanhToanModel>();
            return _dal.GetByDocGia(maDocGia);
        }

        public int Insert(ThanhToanModel model, string? userThucHien = null)
        {
            if (model.MaHoaDon <= 0)
                throw new ArgumentException("Mã hóa đơn không hợp lệ.");
            if (model.SoTien <= 0)
                throw new ArgumentException("Số tiền thanh toán phải lớn hơn 0.");

            if (model.NgayThanhToan == default)
                model.NgayThanhToan = DateOnly.FromDateTime(DateTime.Now);

            if (string.IsNullOrWhiteSpace(model.PhuongThuc))
                model.PhuongThuc = "Chuyển khoản";

            model.TrangThai = "Đã thanh toán";
            model.Version = 1;

            int id = _dal.Insert(model);
            model.MaThanhToan = id;

            // Cập nhật trạng thái hóa đơn thành Đã thanh toán
            _hoaDonDal.UpdateTrangThai(model.MaHoaDon, "Đã thanh toán");

            // Cập nhật trạng thái đăng ký thành Đang hoạt động
            var hoaDon = _hoaDonDal.GetById(model.MaHoaDon);
            if (hoaDon != null)
            {
                var dangKy = _dangKyDal.GetById(hoaDon.MaDangKy, true);
                if (dangKy != null)
                {
                    if (dangKy.IsDeleted)
                    {
                        _dangKyDal.Restore(hoaDon.MaDangKy);
                        dangKy.IsDeleted = false;
                    }
                    _dangKyDal.UpdateTrangThai(hoaDon.MaDangKy, "Đang hoạt động");

                    // Nếu là mua báo lẻ theo số báo cụ thể, tự động tạo phân bổ phát hành ngay khi thanh toán thành công
                    if (dangKy.KyHan == 0)
                    {
                        try
                        {
                            int maSoBao = dangKy.MaSoBao ?? 0;
                            if (maSoBao <= 0)
                            {
                                var danhSachSoBao = _soBaoDal.GetByAnPham(dangKy.MaAnPham);
                                if (danhSachSoBao.Count > 0)
                                {
                                    var sbMatch = danhSachSoBao.Find(x => x.NgayPhatHanh == dangKy.NgayBatDau) ?? danhSachSoBao[0];
                                    maSoBao = sbMatch.MaSoBao;
                                    dangKy.MaSoBao = maSoBao;
                                    _dangKyDal.Update(dangKy);
                                }
                            }

                            if (maSoBao > 0)
                            {
                                // Cập nhật số lượng đã phát hành của kỳ báo sau khi thanh toán thành công
                                try
                                {
                                    _soBaoDal.UpdateSoLuongPhatHanh(maSoBao, dangKy.SoLuong);
                                }
                                catch { }

                                var existingPhatHanh = _phatHanhDal.GetByDangKy(dangKy.MaDangKy);
                                if (existingPhatHanh == null || existingPhatHanh.Count == 0)
                                {
                                    var soBao = _soBaoDal.GetById(maSoBao);
                                    var docGia = _docGiaDal.GetById(dangKy.MaDocGia);
                                    var phatHanh = new PhatHanhModel
                                    {
                                        MaDangKy = dangKy.MaDangKy,
                                        MaSoBao = maSoBao,
                                        NgayPhatHanh = soBao?.NgayPhatHanh ?? dangKy.NgayBatDau,
                                        SoLuong = dangKy.SoLuong,
                                        HinhThuc = "Giao tận nơi",
                                        DiaChiGiao = docGia?.DiaChi ?? "Địa chỉ giao báo",
                                        TrangThai = "Chờ xử lý"
                                    };
                                    _phatHanhDal.Insert(phatHanh);
                                }
                            }
                        }
                        catch
                        {
                            // Tránh gián đoạn giao dịch thanh toán
                        }
                    }

                    _thongBaoDal.Insert(new ThongBaoModel
                    {
                        MaDocGia = dangKy.MaDocGia,
                        TieuDe = "Xác nhận thanh toán thành công",
                        NoiDung = $"Hóa đơn #{hoaDon.MaHoaDon} đã thanh toán thành công {model.SoTien:N0} đ qua {model.PhuongThuc}.",
                        NgayGui = DateOnly.FromDateTime(DateTime.Now),
                        TrangThai = false
                    });
                }
            }

            _audit.GhiNhatKy(null, userThucHien, "ThanhToan", "ThanhToan", id.ToString(), null, model, $"Thanh toán thành công {model.SoTien:N0} đ cho hóa đơn #{model.MaHoaDon}");
            _version.LuuPhienBan("ThanhToan", id.ToString(), 1, model, userThucHien, "Ghi nhận giao dịch thanh toán");

            return id;
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
                _audit.GhiNhatKy(null, userThucHien, "DoiTrangThai", "ThanhToan", id.ToString(), old, updated, $"Cập nhật trạng thái thanh toán '{trangThai}'");
                if (updated != null)
                {
                    _version.LuuPhienBan("ThanhToan", id.ToString(), ver, updated, userThucHien, $"Cập nhật trạng thái: {trangThai}");
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
                _audit.GhiNhatKy(null, deletedBy, "SoftDelete", "ThanhToan", id.ToString(), old, updated, "Hủy/Xóa mềm giao dịch thanh toán");
                if (updated != null)
                {
                    _version.LuuPhienBan("ThanhToan", id.ToString(), ver, updated, deletedBy, "Xóa mềm giao dịch thanh toán (thùng rác)");
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
                _audit.GhiNhatKy(null, restoredBy, "Restore", "ThanhToan", id.ToString(), old, updated, "Khôi phục giao dịch thanh toán");
                if (updated != null)
                {
                    _version.LuuPhienBan("ThanhToan", id.ToString(), ver, updated, restoredBy, "Khôi phục giao dịch thanh toán từ thùng rác");
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
