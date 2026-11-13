using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Models
{
    public class NguoiDungModel
    {
        public int MaNguoiDung { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string QuyenHan { get; set; } = "Độc giả"; // Admin, Biên tập viên, Độc giả
        public bool TrangThai { get; set; } = true;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public int MaNguoiDung { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string QuyenHan { get; set; } = string.Empty;
        public int? MaDocGia { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RevokeTokenRequest
    {
        public int MaNguoiDung { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private const string OutputFormat = "yyyy-MM-dd";
        private static readonly string[] Formats = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "O", "s" };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str)) return default;

            if (DateTime.TryParseExact(str, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            return DateTime.Parse(str, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(OutputFormat));
        }
    }

    public class CustomNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private const string OutputFormat = "yyyy-MM-dd";
        private static readonly string[] Formats = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "O", "s" };

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str)) return null;

            if (DateTime.TryParseExact(str, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(OutputFormat));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    public class CustomDateOnlyConverter : JsonConverter<DateOnly>
    {
        private const string OutputFormat = "yyyy-MM-dd";
        private static readonly string[] Formats = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "O", "s" };

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str)) return default;

            if (DateOnly.TryParseExact(str, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;

            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return DateOnly.FromDateTime(dt);

            return DateOnly.Parse(str, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(OutputFormat));
        }
    }

    public class CustomNullableDateOnlyConverter : JsonConverter<DateOnly?>
    {
        private const string OutputFormat = "yyyy-MM-dd";
        private static readonly string[] Formats = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "O", "s" };

        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str)) return null;

            if (DateOnly.TryParseExact(str, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;

            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return DateOnly.FromDateTime(dt);

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(OutputFormat));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
