using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API_DocGia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Biên tập viên,Độc giả")]
    public class DangKyController : ControllerBase
    {
        private readonly DangKyBLL _bll = new();
        private readonly DocGiaBLL _docGiaBLL = new();

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

        [HttpGet("LayDanhSach")]
        public IActionResult LayDanhSach([FromQuery] int? maDocGia)
        {
            int currentDocGiaId = LayMaDocGiaHienTai();
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Biên tập viên");

            if (!isStaff)
            {
                if (currentDocGiaId <= 0)
                    return BadRequest(new { success = false, message = "Không xác định được độc giả đang đăng nhập." });

                if (maDocGia.HasValue && maDocGia.Value > 0 && maDocGia.Value != currentDocGiaId)
                    return StatusCode(403, new { success = false, message = "Bạn chỉ có quyền xem thông tin đăng ký của chính mình." });

                var listByDocGia = _bll.GetByDocGia(currentDocGiaId);
                return Ok(new { success = true, data = listByDocGia });
            }

            if (maDocGia.HasValue && maDocGia.Value > 0)
            {
                var listByDocGia = _bll.GetByDocGia(maDocGia.Value);
                return Ok(new { success = true, data = listByDocGia });
            }

            var list = _bll.GetAll();
            return Ok(new { success = true, data = list });
        }

        [HttpPost("DangKyDatBao")]
        public IActionResult DangKyDatBao([FromBody] DangKyModel model)
        {
            try
            {
                int currentDocGiaId = LayMaDocGiaHienTai();
                bool isStaff = User.IsInRole("Admin") || User.IsInRole("Biên tập viên");

                if (!isStaff)
                {
                    if (currentDocGiaId <= 0)
                        return BadRequest(new { success = false, message = "Không xác định được độc giả đang đăng nhập." });

                    model.MaDocGia = currentDocGiaId;
                }
                else
                {
                    if (model.MaDocGia <= 0)
                        model.MaDocGia = currentDocGiaId > 0 ? currentDocGiaId : 1;
                }

                int id = _bll.Insert(model);
                model.MaDangKy = id;
                return Ok(new
                {
                    success = true,
                    message = "Đăng ký đặt báo thành công. Vui lòng thanh toán hóa đơn để hoàn tất.",
                    data = model
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("HuyDangKy")]
        public IActionResult HuyDangKy([FromQuery] int? id, [FromQuery] int? maDangKy)
        {
            int targetId = (id.HasValue && id.Value > 0) ? id.Value : (maDangKy.HasValue ? maDangKy.Value : 0);
            if (targetId <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã đăng ký (param ?id= hoặc ?maDangKy=)." });

            var item = _bll.GetById(targetId);
            if (item == null)
                return NotFound(new { success = false, message = "Không tìm thấy thông tin đăng ký cần hủy." });

            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Biên tập viên");
            int currentDocGiaId = LayMaDocGiaHienTai();

            if (!isStaff && item.MaDocGia != currentDocGiaId)
                return StatusCode(403, new { success = false, message = "Bạn chỉ có quyền hủy đơn đăng ký của chính mình." });

            string nguoiThucHien = User.Identity?.Name ?? (isStaff ? "Staff" : "DocGia");
            bool result = _bll.SoftDelete(targetId, nguoiThucHien);
            if (result)
                return Ok(new { success = true, message = "Hủy đăng ký thành công (đã chuyển vào thùng rác)." });
            return BadRequest(new { success = false, message = "Hủy đăng ký thất bại." });
        }

        [HttpGet("LayDanhSachThungRac")]
        public IActionResult LayDanhSachThungRac()
        {
            var all = _bll.GetAll(true);
            var thungRac = all.FindAll(x => x.IsDeleted);
            return Ok(new { success = true, data = thungRac });
        }

        [HttpPut("KhoiPhuc")]
        public IActionResult KhoiPhuc([FromQuery] int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã đăng ký (param ?id=)." });

            string nguoiThucHien = User.Identity?.Name ?? "DocGia";
            bool result = _bll.Restore(id, nguoiThucHien);
            if (result)
                return Ok(new { success = true, message = "Khôi phục đăng ký thuê bao thành công." });
            return BadRequest(new { success = false, message = "Khôi phục thất bại." });
        }
    }
}
