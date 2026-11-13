using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API_BienTap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Biên tập viên,BienTapVien")]
    public class ThongBaoController : ControllerBase
    {
        private readonly ThongBaoBLL _bll = new();

        [HttpGet("LayDanhSach")]
        public IActionResult LayDanhSach()
        {
            var list = _bll.GetAll();
            return Ok(new { success = true, data = list });
        }

        [HttpPost("GuiThongBao")]
        public IActionResult GuiThongBao([FromBody] ThongBaoModel model)
        {
            try
            {
                if (model.MaDocGia <= 0)
                {
                    var ids = _bll.GuiThongBaoTatCa(model.TieuDe, model.NoiDung, model.NgayGui);
                    return Ok(new { success = true, message = $"Đã phát thông báo thành công đến tất cả {ids.Count} độc giả.", soLuong = ids.Count });
                }

                int id = _bll.Insert(model);
                model.MaThongBao = id;
                return Ok(new { success = true, message = "Gửi thông báo thành công.", data = model });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("LayDanhSachDocGia")]
        public IActionResult LayDanhSachDocGia()
        {
            var list = new DocGiaBLL().GetAll(false);
            return Ok(new { success = true, data = list });
        }

        [HttpPut("DanhDauDaDoc")]
        public IActionResult DanhDauDaDoc([FromQuery] int? id = null, [FromQuery] int? maThongBao = null)
        {
            int targetId = (id.HasValue && id.Value > 0) ? id.Value : (maThongBao.HasValue ? maThongBao.Value : 0);
            if (targetId <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã thông báo (param ?id= hoặc ?maThongBao=)." });

            bool result = _bll.MarkAsRead(targetId);
            if (result)
                return Ok(new { success = true, message = "Đã đánh dấu thông báo đã đọc." });
            return BadRequest(new { success = false, message = "Thao tác thất bại." });
        }

        [HttpDelete("Xoa")]
        public IActionResult Xoa([FromQuery] int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã thông báo (param ?id=)." });

            bool result = _bll.Delete(id);
            if (result)
                return Ok(new { success = true, message = "Xóa thông báo thành công." });
            return BadRequest(new { success = false, message = "Xóa thông báo thất bại hoặc không tồn tại." });
        }
    }
}
