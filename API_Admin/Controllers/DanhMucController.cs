using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API_Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Biên tập viên,BienTapVien,Quản trị viên,QuanTriVien")]
    public class DanhMucController : ControllerBase
    {
        private readonly DanhMucBLL _bll = new();

        [HttpGet("LayDanhSach")]
        [AllowAnonymous]
        public IActionResult LayDanhSach()
        {
            var list = _bll.GetAll();
            return Ok(new { success = true, data = list });
        }

        [HttpPost("ThemMoi")]
        public IActionResult ThemMoi([FromBody] DanhMucModel model)
        {
            try
            {
                int id = _bll.Insert(model);
                model.MaDanhMuc = id;
                return Ok(new { success = true, message = "Thêm danh mục thành công.", data = model });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("CapNhat")]
        public IActionResult CapNhat([FromQuery] int id, [FromBody] DanhMucModel model)
        {
            try
            {
                int targetId = id > 0 ? id : model.MaDanhMuc;
                if (targetId <= 0)
                    return BadRequest(new { success = false, message = "Vui lòng cung cấp mã danh mục (param ?id= hoặc trong body MaDanhMuc)." });

                model.MaDanhMuc = targetId;
                bool result = _bll.Update(model);
                if (result)
                    return Ok(new { success = true, message = "Cập nhật danh mục thành công." });
                return BadRequest(new { success = false, message = "Cập nhật danh mục thất bại." });
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
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã danh mục (param ?id=)." });

            try
            {
                var apList = new AnPhamBLL().GetByDanhMuc(id);
                if (apList != null && apList.Count > 0)
                {
                    return BadRequest(new { success = false, message = "Không thể thực hiện" });
                }

                bool result = _bll.Delete(id);
                if (result)
                    return Ok(new { success = true, message = "Xóa danh mục thành công." });
                return BadRequest(new { success = false, message = "Xóa danh mục thất bại hoặc không tồn tại." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Không thể xóa danh mục: " + ex.Message });
            }
        }
    }
}
