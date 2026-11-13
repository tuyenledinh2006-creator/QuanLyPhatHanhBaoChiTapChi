using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;

namespace API_Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Biên tập viên")]
    public class BaoCaoController : ControllerBase
    {
        private readonly BaoCaoBLL _bll = new();

        [HttpGet("ThongKeTongQuan")]
        public IActionResult ThongKeTongQuan()
        {
            var data = _bll.GetTongQuan();
            return Ok(new { success = true, data });
        }

        [HttpGet("ThongKeDoanhThuTheoThang")]
        public IActionResult ThongKeDoanhThuTheoThang([FromQuery] int? nam)
        {
            var data = _bll.GetDoanhThuTheoThang(nam ?? DateTime.Now.Year);
            return Ok(new { success = true, data });
        }

        [HttpGet("ThongKeAnPhamBanChay")]
        public IActionResult ThongKeAnPhamBanChay([FromQuery] int? top)
        {
            var data = _bll.GetAnPhamBanChay(top ?? 10);
            return Ok(new { success = true, data });
        }
    }
}
