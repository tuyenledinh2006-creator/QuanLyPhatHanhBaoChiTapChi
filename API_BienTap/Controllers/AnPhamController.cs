using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API_BienTap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Biên tập viên,BienTapVien")]
    public class AnPhamController : ControllerBase
    {
        private readonly AnPhamBLL _bll = new();

        [HttpGet("LayDanhSach")]
        [AllowAnonymous]
        public IActionResult LayDanhSach()
        {
            var list = _bll.GetAll();
            return Ok(new { success = true, data = list });
        }

        [HttpPost("ThemMoi")]
        public IActionResult ThemMoi([FromBody] AnPhamModel model)
        {
            try
            {
                int id = _bll.Insert(model);
                model.MaAnPham = id;
                return Ok(new { success = true, message = "Thêm ấn phẩm thành công.", data = model });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("CapNhat")]
        public IActionResult CapNhat([FromQuery] int id, [FromBody] AnPhamModel model)
        {
            try
            {
                int targetId = id > 0 ? id : model.MaAnPham;
                if (targetId <= 0)
                    return BadRequest(new { success = false, message = "Vui lòng cung cấp mã ấn phẩm (param ?id= hoặc trong body MaAnPham)." });

                model.MaAnPham = targetId;
                bool result = _bll.Update(model);
                if (result)
                    return Ok(new { success = true, message = "Cập nhật ấn phẩm thành công." });
                return BadRequest(new { success = false, message = "Cập nhật ấn phẩm thất bại." });
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
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã ấn phẩm (param ?id=)." });

            bool result = _bll.Delete(id);
            if (result)
                return Ok(new { success = true, message = "Xóa ấn phẩm thành công." });
            return BadRequest(new { success = false, message = "Xóa ấn phẩm thất bại hoặc không tồn tại." });
        }
    }
}
