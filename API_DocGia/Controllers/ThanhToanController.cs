using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API_DocGia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Biên tập viên,Độc giả")]
    public class ThanhToanController : ControllerBase
    {
        private readonly ThanhToanBLL _thanhToanBLL = new();
        private readonly HoaDonBLL _hoaDonBLL = new();
        private readonly DangKyBLL _dangKyBLL = new();
        private readonly DocGiaBLL _docGiaBLL = new();
        private readonly AnPhamBLL _anPhamBLL = new();

        private int LayMaDocGiaHienTai()
        {
            var claimDg = User.FindFirst("MaDocGia")?.Value;
            if (int.TryParse(claimDg, out int maDg) && maDg > 0)
                return maDg;

            var claimNd = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claimNd, out int maNd) && maNd > 0)
            {
                var dg = _docGiaBLL.GetByMaNguoiDung(maNd);
                if (dg != null) return dg.MaDocGia;
            }
            return 0;
        }

        [HttpGet("LayHoaDonTheoDocGia")]
        public IActionResult LayHoaDonTheoDocGia([FromQuery] int? maDocGia)
        {
            int currentDocGiaId = LayMaDocGiaHienTai();
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Biên tập viên");
            int targetId = (maDocGia.HasValue && maDocGia.Value > 0) ? maDocGia.Value : currentDocGiaId;

            if (!isStaff && targetId != currentDocGiaId)
                return StatusCode(403, new { success = false, message = "Bạn chỉ có quyền xem hóa đơn của chính mình." });

            if (targetId <= 0)
                return BadRequest(new { success = false, message = "Không xác định được độc giả đang đăng nhập." });

            var list = _hoaDonBLL.GetByDocGia(targetId);
            return Ok(new { success = true, data = list });
        }

        [HttpPost("ThucHienThanhToan")]
        public IActionResult ThucHienThanhToan([FromBody] ThanhToanModel model)
        {
            try
            {
                bool isStaff = User.IsInRole("Admin") || User.IsInRole("Biên tập viên");
                int currentDocGiaId = LayMaDocGiaHienTai();
                string nguoiThucHien = isStaff ? (User.Identity?.Name ?? "Staff") : (User.Identity?.Name ?? "DocGia");

                if (!isStaff && currentDocGiaId <= 0)
                    return BadRequest(new { success = false, message = "Không xác định được độc giả đang đăng nhập." });

                HoaDonModel? hd = null;

                // 1. Nếu có MaHoaDon, tìm theo MaHoaDon
                if (model.MaHoaDon > 0)
                {
                    hd = _hoaDonBLL.GetById(model.MaHoaDon, true);
                }

                // 2. Nếu chưa có MaHoaDon hoặc chưa tìm thấy, nhưng có MaDangKy:
                if (hd == null && model.MaDangKy.HasValue && model.MaDangKy.Value > 0)
                {
                    hd = _hoaDonBLL.GetByDangKy(model.MaDangKy.Value, true);
                }

                // 3. Nếu chưa có hóa đơn (vì trước đó chưa thanh toán nên chưa lập): Tiến hành LẬP HÓA ĐƠN ngay lúc này
                if (hd == null)
                {
                    int maDangKy = model.MaDangKy ?? 0;
                    if (maDangKy <= 0)
                        return BadRequest(new { success = false, message = "Vui lòng cung cấp mã đăng ký hoặc mã hóa đơn để thanh toán." });

                    var dk = _dangKyBLL.GetById(maDangKy, true);
                    if (dk == null)
                        return NotFound(new { success = false, message = "Không tìm thấy thông tin đăng ký để lập hóa đơn." });

                    if (!isStaff && dk.MaDocGia != currentDocGiaId)
                        return StatusCode(403, new { success = false, message = "Bạn chỉ có quyền thanh toán cho đơn đăng ký của chính mình." });

                    decimal tongTien = model.SoTien;
                    if (tongTien <= 0)
                    {
                        var ap = _anPhamBLL.GetById(dk.MaAnPham);
                        decimal donGia = ap?.Gia ?? 0;
                        tongTien = (dk.KyHan == 0)
                            ? (donGia * dk.SoLuong)
                            : (donGia * dk.SoLuong * (dk.KyHan * 30));
                        model.SoTien = tongTien;
                    }

                    hd = new HoaDonModel
                    {
                        MaDangKy = dk.MaDangKy,
                        NgayLap = DateOnly.FromDateTime(DateTime.Now),
                        TongTien = tongTien,
                        GiamGia = 0,
                        ThanhTien = tongTien,
                        TrangThai = "Đã thanh toán"
                    };

                    int newHdId = _hoaDonBLL.Insert(hd, nguoiThucHien);
                    hd.MaHoaDon = newHdId;
                    model.MaHoaDon = newHdId;
                }
                else
                {
                    model.MaHoaDon = hd.MaHoaDon;
                    var dk = _dangKyBLL.GetById(hd.MaDangKy, true);
                    if (dk != null && !isStaff && dk.MaDocGia != currentDocGiaId)
                        return StatusCode(403, new { success = false, message = "Bạn chỉ có quyền thanh toán cho hóa đơn của chính mình." });

                    if (dk != null && dk.IsDeleted)
                    {
                        _dangKyBLL.Restore(dk.MaDangKy, nguoiThucHien);
                    }
                }

                int id = _thanhToanBLL.Insert(model, nguoiThucHien);
                model.MaThanhToan = id;

                // Tự động kích hoạt trạng thái gói Thuê Bao sang "Đang hoạt động"
                int targetDangKyId = (hd != null && hd.MaDangKy > 0) ? hd.MaDangKy : (model.MaDangKy ?? 0);
                if (targetDangKyId > 0)
                {
                    _dangKyBLL.UpdateTrangThai(targetDangKyId, "Đang hoạt động", nguoiThucHien);
                }

                // Cập nhật trạng thái Hóa Đơn sang "Đã thanh toán"
                if (hd != null && hd.MaHoaDon > 0)
                {
                    _hoaDonBLL.UpdateTrangThai(hd.MaHoaDon, "Đã thanh toán", nguoiThucHien);
                }

                return Ok(new
                {
                    success = true,
                    message = "Thanh toán và lập hóa đơn thành công.",
                    data = model
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("LayDanhSachThungRac")]
        public IActionResult LayDanhSachThungRac()
        {
            var all = _thanhToanBLL.GetAll(true);
            var thungRac = all.FindAll(x => x.IsDeleted);
            return Ok(new { success = true, data = thungRac });
        }

        [HttpPut("KhoiPhuc")]
        public IActionResult KhoiPhuc([FromQuery] int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã thanh toán (param ?id=)." });

            string nguoiThucHien = User.Identity?.Name ?? "DocGia";
            bool result = _thanhToanBLL.Restore(id, nguoiThucHien);
            if (result)
                return Ok(new { success = true, message = "Khôi phục giao dịch thanh toán thành công." });
            return BadRequest(new { success = false, message = "Khôi phục giao dịch thanh toán thất bại." });
        }
    }
}
