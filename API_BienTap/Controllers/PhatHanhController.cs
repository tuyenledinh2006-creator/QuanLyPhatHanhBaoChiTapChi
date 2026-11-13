using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API_BienTap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Biên tập viên,BienTapVien")]
    public class PhatHanhController : ControllerBase
    {
        private readonly PhatHanhBLL _bll = new();

        [HttpGet("LayDanhSach")]
        public IActionResult LayDanhSach([FromQuery] bool? baoGomDaXoa)
        {
            var list = _bll.GetAll(baoGomDaXoa ?? false);
            return Ok(new { success = true, data = list });
        }

        [HttpGet("LayDanhSachThungRac")]
        public IActionResult LayDanhSachThungRac()
        {
            var all = _bll.GetAll(true);
            var thungRac = all.FindAll(x => x.IsDeleted);
            return Ok(new { success = true, data = thungRac });
        }

        [HttpPost("TaoLenhPhatHanh")]
        public IActionResult TaoLenhPhatHanh([FromBody] PhatHanhModel model)
        {
            try
            {
                string nguoiThucHien = User.Identity?.Name ?? "BienTapVien";
                int id = _bll.Insert(model, nguoiThucHien);
                model.MaPhatHanh = id;
                return Ok(new { success = true, message = "Tạo lệnh phát hành thành công.", data = model });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("CapNhatTrangThai")]
        public IActionResult CapNhatTrangThai([FromQuery] int id, [FromBody] UpdatePhatHanhTrangThaiRequest request)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã phát hành (param ?id=)." });

            try
            {
                string nguoiThucHien = User.Identity?.Name ?? "BienTapVien";
                bool result = _bll.UpdateTrangThai(id, request.TrangThai, nguoiThucHien);
                if (result)
                    return Ok(new { success = true, message = "Cập nhật trạng thái phát hành thành công." });
                return BadRequest(new { success = false, message = "Cập nhật trạng thái thất bại." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("Xoa")]
        public IActionResult Xoa([FromQuery] int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã phát hành (param ?id=)." });

            string nguoiThucHien = User.Identity?.Name ?? "BienTapVien";
            bool result = _bll.SoftDelete(id, nguoiThucHien);
            if (result)
                return Ok(new { success = true, message = "Đã chuyển lệnh phát hành vào thùng rác (xóa mềm thành công)." });
            return BadRequest(new { success = false, message = "Xóa phát hành thất bại hoặc không tồn tại." });
        }

        [HttpPut("KhoiPhuc")]
        public IActionResult KhoiPhuc([FromQuery] int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã phát hành (param ?id=)." });

            string nguoiThucHien = User.Identity?.Name ?? "BienTapVien";
            bool result = _bll.Restore(id, nguoiThucHien);
            if (result)
                return Ok(new { success = true, message = "Khôi phục lệnh phát hành thành công." });
            return BadRequest(new { success = false, message = "Khôi phục thất bại." });
        }
    }

    public class UpdatePhatHanhTrangThaiRequest
    {
        public string TrangThai { get; set; } = string.Empty;
    }
}
