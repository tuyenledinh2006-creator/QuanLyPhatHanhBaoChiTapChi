using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API_Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Biên tập viên")]
    public class DocGiaController : ControllerBase
    {
        private readonly DocGiaBLL _bll = new();

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

        [HttpPut("KhoiPhuc")]
        public IActionResult KhoiPhuc([FromQuery] int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã độc giả (param ?id=)." });

            string nguoiThucHien = User.Identity?.Name ?? "Admin";
            bool result = _bll.Restore(id, nguoiThucHien);
            if (result)
                return Ok(new { success = true, message = "Khôi phục hồ sơ độc giả thành công." });
            return BadRequest(new { success = false, message = "Khôi phục thất bại hoặc không tìm thấy." });
        }
    }
}
