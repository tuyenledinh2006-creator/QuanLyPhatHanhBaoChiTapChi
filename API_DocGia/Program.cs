using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình ConnectionString cho DAL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    KetNoi.ConnectionString = connectionString;
}

// Add services to the container, cấu hình chỉ lấy ngày tháng năm (yyyy-MM-dd)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new CustomNullableDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new CustomDateOnlyConverter());
        options.JsonSerializerOptions.Converters.Add(new CustomNullableDateOnlyConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger kèm nút Authorize (JWT)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API_DocGia - Quản Lý Báo Chí", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token JWT theo định dạng: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Cấu hình JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "QuanLyPhatHanhBaoChi_SecretKey_2026_Secure_JWT_Key_Antigravity!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "QuanLyPhatHanhBaoChi_API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "QuanLyPhatHanhBaoChi_Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Thêm CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-New-Access-Token", "X-New-Refresh-Token");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Middleware tự động làm mới token khi người dùng vẫn còn đăng nhập (hỗ trợ cả Postman lẫn Client)
app.Use(async (context, next) =>
{
    string? authHeader = context.Request.Headers["Authorization"].ToString();
    string? rawToken = null;
    if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        rawToken = authHeader.Substring("Bearer ".Length).Trim();
    }

    // 1. Tự động kiểm tra và gia hạn trực tiếp qua Bearer Token
    if (!string.IsNullOrWhiteSpace(rawToken))
    {
        var handler = new JwtSecurityTokenHandler();
        if (handler.CanReadToken(rawToken))
        {
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = false, // Cho phép đọc claim kể cả khi đã hết hạn 15 phút
                ClockSkew = TimeSpan.Zero
            };

            ClaimsPrincipal? principal = null;
            try
            {
                principal = handler.ValidateToken(rawToken, tokenValidationParams, out _);
            }
            catch
            {
                principal = null;
            }

            if (principal != null)
            {
                var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out int userId))
                {
                    var nguoiDungBLL = new BLL.NguoiDungBLL();
                    var user = nguoiDungBLL.GetById(userId);

                    // Kiểm tra trong SQL: Người dùng CÒN ĐĂNG NHẬP (chưa bấm Đăng xuất)
                    if (user != null && user.TrangThai && !string.IsNullOrWhiteSpace(user.RefreshToken) && user.RefreshTokenExpiryTime > DateTime.Now)
                    {
                        var expClaim = principal.FindFirst("exp")?.Value;
                        bool needRenew = true;
                        if (long.TryParse(expClaim, out long expSeconds))
                        {
                            var expTime = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                            if (expTime > DateTime.UtcNow.AddMinutes(5))
                            {
                                needRenew = false;
                            }
                        }

                        if (needRenew)
                        {
                            // Tự động gia hạn thêm 15 phút mới
                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                                new Claim(ClaimTypes.Name, user.TenDangNhap),
                                new Claim(ClaimTypes.Role, user.QuyenHan),
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim("HoTen", user.HoTen)
                            };
                            var docGia = new BLL.DocGiaBLL().GetByMaNguoiDung(user.MaNguoiDung);
                            if (docGia != null) claims.Add(new Claim("MaDocGia", docGia.MaDocGia.ToString()));

                            var newToken = new JwtSecurityToken(
                                issuer: jwtIssuer,
                                audience: jwtAudience,
                                claims: claims,
                                expires: DateTime.Now.AddMinutes(15),
                                signingCredentials: creds
                            );
                            string newAccessTokenStr = handler.WriteToken(newToken);

                            // Ghi đè Authorization header thành token mới cho request hiện tại thành công ngay
                            context.Request.Headers["Authorization"] = "Bearer " + newAccessTokenStr;
                            context.Response.Headers["X-New-Access-Token"] = newAccessTokenStr;
                        }
                    }
                }
            }
        }
    }
    // 2. Kiểm tra nếu client gửi kèm X-Refresh-Token
    else if (context.Request.Headers.TryGetValue("X-Refresh-Token", out var refreshHeader) && !string.IsNullOrWhiteSpace(refreshHeader))
    {
        string rfToken = refreshHeader.ToString().Trim();
        var nguoiDungBLL = new BLL.NguoiDungBLL();
        var user = nguoiDungBLL.GetByRefreshToken(rfToken);
        if (user != null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Role, user.QuyenHan),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("HoTen", user.HoTen)
            };
            var docGia = new BLL.DocGiaBLL().GetByMaNguoiDung(user.MaNguoiDung);
            if (docGia != null) claims.Add(new Claim("MaDocGia", docGia.MaDocGia.ToString()));

            var newToken = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: creds
            );
            string newAccessTokenStr = new JwtSecurityTokenHandler().WriteToken(newToken);

            context.Request.Headers["Authorization"] = "Bearer " + newAccessTokenStr;
            context.Response.Headers["X-New-Access-Token"] = newAccessTokenStr;
        }
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
