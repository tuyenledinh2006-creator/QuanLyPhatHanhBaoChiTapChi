using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API_Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class NguoiDungController : ControllerBase
    {
        private readonly NguoiDungBLL _bll = new();

        [HttpGet("LayDanhSach")]
        public IActionResult LayDanhSach()
        {
            var list = _bll.GetAll();
            return Ok(new { success = true, data = list });
        }

        [HttpPost("ThemMoi")]
        public IActionResult ThemMoi([FromBody] NguoiDungModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.QuyenHan))
                {
                    model.QuyenHan = "Độc giả";
                }

                int id = _bll.Insert(model);
                model.MaNguoiDung = id;

                if (model.QuyenHan == "Độc giả")
                {
                    var dgBll = new DocGiaBLL();
                    var existingDg = dgBll.GetByMaNguoiDung(id);
                    if (existingDg == null)
                    {
                        try
                        {
                            dgBll.Insert(new DocGiaModel
                            {
                                MaNguoiDung = id,
                                DiaChi = "Chưa cập nhật"
                            });
                        }
                        catch { }
                    }
                }

                return Ok(new { success = true, message = "Thêm người dùng thành công.", data = model });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("CapNhat")]
        public IActionResult CapNhat([FromQuery] int id, [FromBody] NguoiDungModel model)
        {
            try
            {
                int targetId = id > 0 ? id : model.MaNguoiDung;
                if (targetId <= 0)
                    return BadRequest(new { success = false, message = "Vui lòng cung cấp mã người dùng (param ?id= hoặc trong body MaNguoiDung)." });

                model.MaNguoiDung = targetId;
                bool result = _bll.Update(model);
                if (result)
                    return Ok(new { success = true, message = "Cập nhật người dùng thành công." });
                return BadRequest(new { success = false, message = "Cập nhật người dùng thất bại." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("PhanQuyen")]
        public IActionResult PhanQuyen([FromQuery] int id, [FromBody] PhanQuyenBodyModel req)
        {
            int targetId = id > 0 ? id : (int.TryParse(Request.Query["maNguoiDung"], out int qId) ? qId : 0);
            string targetQuyen = !string.IsNullOrWhiteSpace(req?.QuyenHan) ? req.QuyenHan : Request.Query["quyenHan"].ToString();

            if (targetId <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã người dùng (param ?id=)." });

            if (string.IsNullOrWhiteSpace(targetQuyen))
                return BadRequest(new { success = false, message = "Vui lòng cung cấp quyền hạn mới trong body: {\"quyenHan\": \"Biên tập viên\"} [Admin, Biên tập viên, Độc giả]." });

            // 1. Kiểm tra Admin không được tự hạ quyền của chính mình
            var currentUserIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserIdStr, out int currentAdminId) && currentAdminId == targetId)
            {
                if (!targetQuyen.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Không thực hiện đc!"
                    });
                }
            }

            try
            {
                bool result = _bll.UpdateQuyenHan(targetId, targetQuyen);
                if (result)
                {
                    // 2. Thu hồi RefreshToken để phiên đăng nhập cũ bị hủy, bắt buộc tài khoản phải đăng xuất
                    try
                    {
                        _bll.RevokeRefreshToken(targetId);
                    }
                    catch { }

                    return Ok(new
                    {
                        success = true,
                        message = $"Phân quyền thành công cho người dùng {targetId} thành '{targetQuyen}'. Phiên đăng nhập cũ đã bị hủy, tài khoản này sẽ phải đăng nhập lại.",
                        yeuCauDangXuat = true,
                        maNguoiDungBiDoi = targetId
                    });
                }
                return BadRequest(new { success = false, message = "Phân quyền thất bại hoặc người dùng không tồn tại." });
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
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã người dùng (param ?id=)." });

            try
            {
                bool result = _bll.Delete(id);
                if (result)
                    return Ok(new { success = true, message = "Xóa người dùng thành công (hoặc đã chuyển sang trạng thái ngưng hoạt động nếu đã có đơn hàng)." });
                return BadRequest(new { success = false, message = "Xóa người dùng thất bại hoặc người dùng không tồn tại." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Không thể xóa người dùng: " + ex.Message });
            }
        }
    }

    public class PhanQuyenBodyModel
    {
        public string QuyenHan { get; set; } = string.Empty;
    }
}
