using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL;
using Models;

namespace API_DocGia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CaNhanController : ControllerBase
    {
        private readonly DocGiaBLL _docGiaBLL = new();
        private readonly NguoiDungBLL _nguoiDungBLL = new();
        private readonly ThongBaoBLL _thongBaoBLL = new();
        private readonly PhatHanhBLL _phatHanhBLL = new();

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

        private int LayMaNguoiDungHienTai()
        {
            var claimNd = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claimNd, out int maNd) && maNd > 0)
                return maNd;
            return 0;
        }

        [HttpGet("LayThongTin")]
        public IActionResult LayThongTin([FromQuery] int? maDocGia)
        {
            int currentDocGiaId = LayMaDocGiaHienTai();
            int currentUserId = LayMaNguoiDungHienTai();
            bool isAdmin = User.IsInRole("Admin");

            // Nếu người dùng có hồ sơ độc giả
            if (currentDocGiaId > 0 || (maDocGia.HasValue && maDocGia.Value > 0))
            {
                int targetId = (maDocGia.HasValue && maDocGia.Value > 0) ? maDocGia.Value : currentDocGiaId;

                if (!isAdmin && targetId != currentDocGiaId)
                    return StatusCode(403, new { success = false, message = "Bạn chỉ có quyền xem thông tin cá nhân của chính mình." });

                var docGia = _docGiaBLL.GetById(targetId);
                if (docGia == null)
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin độc giả." });

                return Ok(new { success = true, data = docGia });
            }

            // Nếu là Admin hoặc Biên tập viên không có hồ sơ độc giả
            if (currentUserId > 0)
            {
                var user = _nguoiDungBLL.GetById(currentUserId);
                if (user == null)
                    return NotFound(new { success = false, message = "Không tìm thấy người dùng." });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        maDocGia = 0,
                        maNguoiDung = user.MaNguoiDung,
                        tenDangNhap = user.TenDangNhap,
                        hoTen = user.HoTen,
                        email = user.Email,
                        soDienThoai = user.SoDienThoai,
                        quyenHan = user.QuyenHan,
                        ngaySinh = (string?)null,
                        diaChi = (string?)null
                    }
                });
            }

            return BadRequest(new { success = false, message = "Không xác định được danh tính người dùng đang đăng nhập." });
        }

        [HttpPut("CapNhatThongTin")]
        public IActionResult CapNhatThongTin([FromQuery] int? maDocGia, [FromBody] UpdateProfileRequest request)
        {
            try
            {
                int currentDocGiaId = LayMaDocGiaHienTai();
                int currentUserId = LayMaNguoiDungHienTai();
                bool isAdmin = User.IsInRole("Admin");

                if (currentDocGiaId > 0 || (maDocGia.HasValue && maDocGia.Value > 0))
                {
                    int targetId = (maDocGia.HasValue && maDocGia.Value > 0) ? maDocGia.Value : currentDocGiaId;

                    if (!isAdmin && targetId != currentDocGiaId)
                        return StatusCode(403, new { success = false, message = "Bạn chỉ có quyền cập nhật thông tin của chính mình." });

                    var docGia = _docGiaBLL.GetById(targetId);
                    if (docGia == null)
                        return NotFound(new { success = false, message = "Không tìm thấy độc giả." });

                    // Cập nhật thông tin độc giả
                    if (request.NgaySinh.HasValue)
                        docGia.NgaySinh = request.NgaySinh;
                    if (request.DiaChi != null)
                        docGia.DiaChi = request.DiaChi;
                    _docGiaBLL.Update(docGia);

                    // Cập nhật thông tin người dùng liên kết
                    var nguoiDung = _nguoiDungBLL.GetById(docGia.MaNguoiDung);
                    if (nguoiDung != null)
                    {
                        if (!string.IsNullOrWhiteSpace(request.HoTen))
                            nguoiDung.HoTen = request.HoTen;
                        if (!string.IsNullOrWhiteSpace(request.Email))
                            nguoiDung.Email = request.Email;
                        if (!string.IsNullOrWhiteSpace(request.SoDienThoai))
                            nguoiDung.SoDienThoai = request.SoDienThoai;

                        _nguoiDungBLL.Update(nguoiDung);
                    }

                    return Ok(new { success = true, message = "Cập nhật thông tin cá nhân thành công." });
                }

                // Cập nhật tài khoản Admin hoặc Biên tập viên
                if (currentUserId > 0)
                {
                    var nguoiDung = _nguoiDungBLL.GetById(currentUserId);
                    if (nguoiDung == null)
                        return NotFound(new { success = false, message = "Không tìm thấy tài khoản người dùng." });

                    if (!string.IsNullOrWhiteSpace(request.HoTen))
                        nguoiDung.HoTen = request.HoTen;
                    if (!string.IsNullOrWhiteSpace(request.Email))
                        nguoiDung.Email = request.Email;
                    if (!string.IsNullOrWhiteSpace(request.SoDienThoai))
                        nguoiDung.SoDienThoai = request.SoDienThoai;

                    _nguoiDungBLL.Update(nguoiDung);
                    return Ok(new { success = true, message = "Cập nhật thông tin cá nhân thành công." });
                }

                return BadRequest(new { success = false, message = "Không xác định được danh tính người dùng cần cập nhật." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("LayThongBao")]
        public IActionResult LayThongBao([FromQuery] int? maDocGia)
        {
            int currentDocGiaId = LayMaDocGiaHienTai();
            bool isAdmin = User.IsInRole("Admin");
            int targetId = (maDocGia.HasValue && maDocGia.Value > 0) ? maDocGia.Value : currentDocGiaId;

            if (!isAdmin && targetId != currentDocGiaId)
                return StatusCode(403, new { success = false, message = "Bạn chỉ có quyền xem thông báo của chính mình." });

            var list = _thongBaoBLL.GetByDocGia(targetId);
            return Ok(new { success = true, data = list });
        }

        [HttpPut("DanhDauThongBaoDaDoc")]
        public IActionResult DanhDauThongBaoDaDoc([FromQuery] int? maThongBao = null, [FromQuery] int? id = null)
        {
            int targetId = (maThongBao.HasValue && maThongBao.Value > 0) ? maThongBao.Value : (id.HasValue ? id.Value : 0);
            if (targetId <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã thông báo (param ?maThongBao= hoặc ?id=)." });

            var tb = _thongBaoBLL.GetById(targetId);
            if (tb == null)
                return NotFound(new { success = false, message = "Không tìm thấy thông báo." });

            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Biên tập viên") || User.IsInRole("BienTapVien");
            if (!isStaff && tb.MaDocGia != LayMaDocGiaHienTai())
                return StatusCode(403, new { success = false, message = "Bạn không có quyền thao tác trên thông báo của người khác." });

            bool result = _thongBaoBLL.MarkAsRead(targetId);
            if (result)
                return Ok(new { success = true, message = "Đã đánh dấu là đã đọc." });
            return BadRequest(new { success = false, message = "Thao tác thất bại." });
        }

        [HttpDelete("XoaThongBao")]
        public IActionResult XoaThongBao([FromQuery] int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp mã thông báo (param ?id=)." });

            var tb = _thongBaoBLL.GetById(id);
            if (tb == null)
                return NotFound(new { success = false, message = "Không tìm thấy thông báo." });

            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Biên tập viên") || User.IsInRole("BienTapVien");
            if (!isStaff && tb.MaDocGia != LayMaDocGiaHienTai())
                return StatusCode(403, new { success = false, message = "Bạn không có quyền xóa thông báo của người khác." });

            bool result = _thongBaoBLL.Delete(id);
            if (result)
                return Ok(new { success = true, message = "Xóa thông báo thành công." });
            return BadRequest(new { success = false, message = "Xóa thông báo thất bại." });
        }

        [HttpDelete("XoaTatCaThongBao")]
        public IActionResult XoaTatCaThongBao()
        {
            int currentDocGiaId = LayMaDocGiaHienTai();
            if (currentDocGiaId <= 0)
                return BadRequest(new { success = false, message = "Không xác định được độc giả đang đăng nhập." });

            bool result = _thongBaoBLL.DeleteByDocGia(currentDocGiaId);
            return Ok(new { success = true, message = "Đã xóa toàn bộ thông báo trong hộp thư của bạn." });
        }

        [HttpGet("LayLichSuNhanBao")]
        public IActionResult LayLichSuNhanBao([FromQuery] int? maDocGia)
        {
            int currentDocGiaId = LayMaDocGiaHienTai();
            bool isAdmin = User.IsInRole("Admin");
            int targetId = (maDocGia.HasValue && maDocGia.Value > 0) ? maDocGia.Value : currentDocGiaId;

            if (!isAdmin && targetId != currentDocGiaId)
                return StatusCode(403, new { success = false, message = "Bạn chỉ có quyền xem lịch sử nhận báo của chính mình." });

            var list = _phatHanhBLL.GetByDocGia(targetId);
            return Ok(new { success = true, data = list });
        }
    }

    public class UpdateProfileRequest
    {
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
    }
}
