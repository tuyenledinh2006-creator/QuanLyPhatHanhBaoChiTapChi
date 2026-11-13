using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API_BienTap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Biên tập viên,BienTapVien")]
    public class SoBaoController : ControllerBase
    {
        private readonly SoBaoBLL _bll = new();

        [HttpGet("LayDanhSach")]
        [AllowAnonymous]
        public IActionResult LayDanhSach()
        {
            var list = _bll.GetAll();
            return Ok(new { success = true, data = list });
        }

        [HttpGet("LayTheoAnPham")]
        [AllowAnonymous]
        public IActionResult LayTheoAnPham([FromQuery] int maAnPham)
        {
            if (maAnPham <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã ấn phẩm (param ?maAnPham=)." });

            var list = _bll.GetByAnPham(maAnPham);
            return Ok(new { success = true, data = list });
        }

        [HttpPost("ThemMoi")]
        public IActionResult ThemMoi([FromBody] SoBaoModel model)
        {
            try
            {
                int id = _bll.Insert(model);
                model.MaSoBao = id;
                return Ok(new { success = true, message = "Thêm số báo thành công.", data = model });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("CapNhat")]
        public IActionResult CapNhat([FromQuery] int id, [FromBody] SoBaoModel model)
        {
            try
            {
                int targetId = id > 0 ? id : model.MaSoBao;
                if (targetId <= 0)
                    return BadRequest(new { success = false, message = "Vui lòng cung cấp mã số báo (param ?id= hoặc trong body MaSoBao)." });

                model.MaSoBao = targetId;
                bool result = _bll.Update(model);
                if (result)
                    return Ok(new { success = true, message = "Cập nhật số báo thành công." });
                return BadRequest(new { success = false, message = "Cập nhật số báo thất bại." });
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
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã số báo (param ?id=)." });

            bool result = _bll.Delete(id);
            if (result)
                return Ok(new { success = true, message = "Xóa số báo thành công." });
            return BadRequest(new { success = false, message = "Xóa số báo thất bại hoặc không tồn tại." });
        }
    }
}
