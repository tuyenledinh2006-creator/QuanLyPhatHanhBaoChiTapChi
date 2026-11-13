using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;

namespace API_Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : ControllerBase
    {
        private readonly AuditLogBLL _auditBLL = new();
        private readonly LichSuPhienBanBLL _versionBLL = new();

        [HttpGet("LayDanhSach")]
        public IActionResult LayDanhSach([FromQuery] string? bang, [FromQuery] string? hanhDong, [FromQuery] int? limit)
        {
            var list = _auditBLL.LayDanhSach(bang, hanhDong, limit ?? 100);
            return Ok(new { success = true, data = list });
        }

        [HttpGet("LayLichSuPhienBan")]
        public IActionResult LayLichSuPhienBan([FromQuery] string bang, [FromQuery] string khoaChinh)
        {
            if (string.IsNullOrWhiteSpace(bang) || string.IsNullOrWhiteSpace(khoaChinh))
                return BadRequest(new { success = false, message = "Vui lòng cung cấp tên bảng (bang) và khóa chính (khoaChinh)." });

            var list = _versionBLL.LayTheoThucThe(bang, khoaChinh);
            return Ok(new { success = true, data = list });
        }
    }
}
