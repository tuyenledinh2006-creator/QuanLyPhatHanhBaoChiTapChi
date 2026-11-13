using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using BLL;
using Models;

namespace API_Login.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : ControllerBase
    {
        private readonly NguoiDungBLL _nguoiDungBLL = new();
        private readonly DocGiaBLL _docGiaBLL = new();
        private readonly IConfiguration _configuration;

        public TaiKhoanController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("DangNhap")]
        [AllowAnonymous]
        public IActionResult DangNhap([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.MatKhau))
                return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu." });

            var user = _nguoiDungBLL.Login(request.TenDangNhap, request.MatKhau);
            if (user == null)
                return Unauthorized(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không chính xác, hoặc tài khoản đã bị khóa." });

            // Lấy thông tin độc giả nếu có
            DocGiaModel? docGia = null;
            if (user.QuyenHan == "Độc giả")
            {
                docGia = _docGiaBLL.GetByMaNguoiDung(user.MaNguoiDung);
                if (docGia == null)
                {
                    try
                    {
                        int newMaDg = _docGiaBLL.Insert(new DocGiaModel
                        {
                            MaNguoiDung = user.MaNguoiDung,
                            DiaChi = "Chưa cập nhật"
                        });
                        docGia = _docGiaBLL.GetById(newMaDg);
                    }
                    catch { }
                }
            }

            // Sinh JWT Token
            string accessToken = GenerateJwtToken(user, docGia?.MaDocGia, out DateTime accessExpiry);

            // Sinh Refresh Token và lưu vào SQL Server
            string refreshToken = GenerateRefreshToken();
            int refreshExpiryDays = int.TryParse(_configuration["Jwt:RefreshTokenExpirationDays"], out int days) ? days : 7;
            DateTime refreshExpiry = DateTime.Now.AddDays(refreshExpiryDays);

            _nguoiDungBLL.SaveRefreshToken(user.MaNguoiDung, refreshToken, refreshExpiry);

            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = accessExpiry,
                MaNguoiDung = user.MaNguoiDung,
                TenDangNhap = user.TenDangNhap,
                HoTen = user.HoTen,
                Email = user.Email,
                QuyenHan = user.QuyenHan,
                MaDocGia = docGia?.MaDocGia
            };

            return Ok(new
            {
                success = true,
                message = "Đăng nhập thành công.",
                data = response
            });
        }

        [HttpPost("LamMoiToken")]
        [AllowAnonymous]
        public IActionResult LamMoiToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { success = false, message = "Refresh token không được để trống." });

            // Tìm người dùng với refresh token hợp lệ và còn hạn trong SQL Server
            var user = _nguoiDungBLL.GetByRefreshToken(request.RefreshToken);
            if (user == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Refresh token không hợp lệ hoặc đã hết hạn/bị thu hồi. Vui lòng đăng nhập lại."
                });
            }

            DocGiaModel? docGia = null;
            if (user.QuyenHan == "Độc giả")
            {
                docGia = _docGiaBLL.GetByMaNguoiDung(user.MaNguoiDung);
                if (docGia == null)
                {
                    try
                    {
                        int newMaDg = _docGiaBLL.Insert(new DocGiaModel
                        {
                            MaNguoiDung = user.MaNguoiDung,
                            DiaChi = "Chưa cập nhật"
                        });
                        docGia = _docGiaBLL.GetById(newMaDg);
                    }
                    catch { }
                }
            }

            // Sinh cặp token mới (Token Rotation)
            string newAccessToken = GenerateJwtToken(user, docGia?.MaDocGia, out DateTime accessExpiry);
            string newRefreshToken = GenerateRefreshToken();
            int refreshExpiryDays = int.TryParse(_configuration["Jwt:RefreshTokenExpirationDays"], out int days) ? days : 7;
            DateTime refreshExpiry = DateTime.Now.AddDays(refreshExpiryDays);

            // Lưu Refresh Token mới được reset vào SQL Server
            _nguoiDungBLL.SaveRefreshToken(user.MaNguoiDung, newRefreshToken, refreshExpiry);

            var response = new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Expiration = accessExpiry,
                MaNguoiDung = user.MaNguoiDung,
                TenDangNhap = user.TenDangNhap,
                HoTen = user.HoTen,
                Email = user.Email,
                QuyenHan = user.QuyenHan,
                MaDocGia = docGia?.MaDocGia
            };

            return Ok(new
            {
                success = true,
                message = "Gia hạn token thành công.",
                data = response
            });
        }

        [HttpPost("DangXuat")]
        [AllowAnonymous]
        public IActionResult DangXuat([FromBody] RevokeTokenRequest? request)
        {
            // 1. Thu hồi theo RefreshToken nếu có
            if (!string.IsNullOrWhiteSpace(request?.RefreshToken))
            {
                bool byToken = _nguoiDungBLL.RevokeRefreshTokenByToken(request.RefreshToken);
                if (byToken)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Đăng xuất thành công, Refresh Token đã được thu hồi trong cơ sở dữ liệu."
                    });
                }
            }

            // 2. Thu hồi theo MaNguoiDung (từ body hoặc từ claims JWT)
            int maNguoiDung = request?.MaNguoiDung ?? 0;
            if (maNguoiDung <= 0 && User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(idClaim, out maNguoiDung);
            }

            if (maNguoiDung > 0)
            {
                bool result = _nguoiDungBLL.RevokeRefreshToken(maNguoiDung);
                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Đăng xuất thành công, Refresh Token đã được thu hồi trong cơ sở dữ liệu."
                    });
                }
            }

            return BadRequest(new { success = false, message = "Vui lòng cung cấp mã người dùng hoặc Refresh Token cần thu hồi." });
        }

        [HttpPost("DangKy")]
        [AllowAnonymous]
        public IActionResult DangKy([FromBody] RegisterRequest request)
        {
            try
            {
                var nguoiDung = new NguoiDungModel
                {
                    TenDangNhap = request.TenDangNhap,
                    MatKhau = request.MatKhau,
                    HoTen = request.HoTen,
                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    QuyenHan = "Độc giả",
                    TrangThai = true
                };

                int maNguoiDung = _nguoiDungBLL.Insert(nguoiDung);

                // Tự động tạo hồ sơ độc giả
                var docGia = new DocGiaModel
                {
                    MaNguoiDung = maNguoiDung,
                    NgaySinh = request.NgaySinh,
                    DiaChi = request.DiaChi
                };
                int maDocGia = _docGiaBLL.Insert(docGia);

                return Ok(new
                {
                    success = true,
                    message = "Đăng ký tài khoản độc giả thành công.",
                    data = new { MaNguoiDung = maNguoiDung, MaDocGia = maDocGia }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("DoiMatKhau")]
        [Authorize]
        public IActionResult DoiMatKhau([FromBody] ChangePasswordRequest request)
        {
            try
            {
                bool result = _nguoiDungBLL.ChangePassword(request.MaNguoiDung, request.MatKhauCu, request.MatKhauMoi);
                if (result)
                    return Ok(new { success = true, message = "Đổi mật khẩu thành công." });
                return BadRequest(new { success = false, message = "Đổi mật khẩu thất bại." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("LayThongTin/{id}")]
        [Authorize]
        public IActionResult LayThongTin(int id)
        {
            var user = _nguoiDungBLL.GetById(id);
            if (user == null)
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });

            return Ok(new { success = true, data = user });
        }

        #region Private Token Generator Helpers

        private string GenerateJwtToken(NguoiDungModel user, int? maDocGia, out DateTime expiry)
        {
            string secretKey = _configuration["Jwt:Key"] ?? "QuanLyPhatHanhBaoChi_SecretKey_2026_Secure_JWT_Key_Antigravity!";
            string issuer = _configuration["Jwt:Issuer"] ?? "QuanLyPhatHanhBaoChi_API";
            string audience = _configuration["Jwt:Audience"] ?? "QuanLyPhatHanhBaoChi_Client";
            int expiryMinutes = int.TryParse(_configuration["Jwt:AccessTokenExpirationMinutes"], out int mins) ? mins : 15;

            expiry = DateTime.Now.AddMinutes(expiryMinutes);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Role, user.QuyenHan),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("HoTen", user.HoTen)
            };

            if (maDocGia.HasValue && maDocGia.Value > 0)
            {
                claims.Add(new Claim("MaDocGia", maDocGia.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        #endregion
    }

    public class LoginRequest
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int MaNguoiDung { get; set; }
        public string MatKhauCu { get; set; } = string.Empty;
        public string MatKhauMoi { get; set; } = string.Empty;
    }
}
