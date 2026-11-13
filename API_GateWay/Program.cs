using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;

// Đảm bảo thư mục wwwroot luôn tồn tại để tránh DirectoryNotFoundException trong Visual Studio
var defaultWwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(defaultWwwroot))
{
    try { Directory.CreateDirectory(defaultWwwroot); } catch { }
}

var builder = WebApplication.CreateBuilder(args);

// Thêm file cấu hình Ocelot (Tự động nạp ocelot.Docker.json khi chạy trong Docker/Production)
var envOcelot = $"ocelot.{builder.Environment.EnvironmentName}.json";
var ocelotConfigFile = File.Exists(Path.Combine(builder.Environment.ContentRootPath, envOcelot))
    ? envOcelot
    : "ocelot.json";
builder.Configuration.AddJsonFile(ocelotConfigFile, optional: false, reloadOnChange: true);

// Cấu hình JWT Authentication cho Gateway
string jwtKey = builder.Configuration["Jwt:Key"] ?? "QuanLyPhatHanhBaoChi_SecretKey_2026_Secure_JWT_Key_Antigravity!";
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "QuanLyPhatHanhBaoChi_API";
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? "QuanLyPhatHanhBaoChi_Client";

var authenticationProviderKey = "Bearer";

builder.Services.AddAuthentication()
    .AddJwtBearer(authenticationProviderKey, options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ClockSkew = TimeSpan.Zero
        };
    });

// Thêm Swagger cho Gateway
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gateway - Hệ Thống Quản Lý Báo Chí",
        Version = "v1",
        Description = "Cổng kết nối tập trung (Ocelot). Mọi API được gọi qua tiền tố /gateway/{Controller}/{Action}"
    });
});

// Thêm dịch vụ Ocelot
builder.Services.AddOcelot(builder.Configuration);

// Thêm CORS toàn quyền
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Cho phép Private Network Access đối với các trình duyệt mở file:///
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Access-Control-Request-Private-Network"))
    {
        context.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
    }
    if (HttpMethods.IsOptions(context.Request.Method))
    {
        context.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
    }
    await next();
});

// Kích hoạt phục vụ giao diện Web Frontend trực tiếp từ thư mục frontend/
var frontendPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "frontend"));
if (Directory.Exists(frontendPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        RequestPath = ""
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        RequestPath = ""
    });
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

// Chuyển hướng trang chủ về /index.html tránh lỗi Ocelot không tìm thấy downstream route cho GET /
app.Use(async (context, next) =>
{
    var p = context.Request.Path.Value;
    if (string.IsNullOrEmpty(p) || p == "/" || p == "/index")
    {
        context.Response.Redirect("/index.html");
        return;
    }
    await next();
});

// Kích hoạt Swagger UI cho Gateway trước khi Ocelot chặn pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cổng API Gateway");
    c.SwaggerEndpoint("http://localhost:5171/swagger/v1/swagger.json", "1. API Login (Port 5171)");
    c.SwaggerEndpoint("http://localhost:5107/swagger/v1/swagger.json", "2. API Admin (Port 5107)");
    c.SwaggerEndpoint("http://localhost:5131/swagger/v1/swagger.json", "3. API Biên Tập (Port 5131)");
    c.SwaggerEndpoint("http://localhost:5142/swagger/v1/swagger.json", "4. API Độc Giả (Port 5142)");
    c.RoutePrefix = "swagger";
});

await app.UseOcelot();

app.Run();
