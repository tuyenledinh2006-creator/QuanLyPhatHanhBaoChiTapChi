-- ========================================================
-- DỰ ÁN BÁO CÁO BTL: HỆ THỐNG QUẢN LÝ PHÁT HÀNH BÁO CHÍ
-- CSDL: QuanLyPhatHanhBaoChi
--
-- QUẢN TRỊ VIÊN: Lê Đình Tuyên (admin@gmail.com, ID = 1)
-- BIÊN TẬP VIÊN: Đúng 2 người (Trần Văn Bình, Nguyễn Thị Mai)
-- MẬT KHẨU MẶC ĐỊNH: 123456
-- HAI BẢNG ĐỂ TRỐNG: AuditLog, LichSuPhienBan
-- ========================================================

-- TẠO CƠ SỞ DỮ LIỆU NẾU CHƯA CÓ
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyPhatHanhBaoChi')
BEGIN
    CREATE DATABASE QuanLyPhatHanhBaoChi;
END
GO

USE QuanLyPhatHanhBaoChi;
GO

-- XÓA BẢNG CŨ NẾU ĐÃ TỒN TẠI (ĐỂ CHẠY LẠI KHÔNG BỊ LỖI TRÙNG BẢNG)
DROP TABLE IF EXISTS ThanhToan;
DROP TABLE IF EXISTS HoaDon;
DROP TABLE IF EXISTS PhatHanh;
DROP TABLE IF EXISTS DangKy;
DROP TABLE IF EXISTS ThongBao;
DROP TABLE IF EXISTS DocGia;
DROP TABLE IF EXISTS NguoiDung;
DROP TABLE IF EXISTS SoBao;
DROP TABLE IF EXISTS AnPham;
DROP TABLE IF EXISTS DanhMuc;
DROP TABLE IF EXISTS AuditLog;
DROP TABLE IF EXISTS LichSuPhienBan;
GO

-- ========================================================
-- PHẦN 1: KHỞI TẠO CẤU TRÚC 12 BẢNG (SCHEMA)
-- ========================================================

-- 1. BẢNG NGƯỜI DÙNG
CREATE TABLE NguoiDung
(
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    SoDienThoai VARCHAR(15),
    QuyenHan NVARCHAR(30) NOT NULL,
    TrangThai BIT DEFAULT 1,
    RefreshToken VARCHAR(255) NULL,
    RefreshTokenExpiryTime DATETIME NULL
);
GO

-- 2. BẢNG ĐỘC GIẢ
CREATE TABLE DocGia
(
    MaDocGia INT IDENTITY(1,1) PRIMARY KEY,
    MaNguoiDung INT NOT NULL,
    NgaySinh DATE,
    DiaChi NVARCHAR(255),
    IsDeleted BIT DEFAULT 0,
    DeletedAt DATETIME NULL,
    DeletedBy INT NULL,
    Version INT DEFAULT 1,

    FOREIGN KEY (MaNguoiDung)
        REFERENCES NguoiDung(MaNguoiDung)
);
GO

-- 3. BẢNG DANH MỤC
CREATE TABLE DanhMuc
(
    MaDanhMuc INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255)
);
GO

-- 4. BẢNG ẤN PHẨM
CREATE TABLE AnPham
(
    MaAnPham INT IDENTITY(1,1) PRIMARY KEY,
    MaDanhMuc INT NOT NULL,
    TenAnPham NVARCHAR(200) NOT NULL,
    MoTa NVARCHAR(500),
    Gia DECIMAL(18,2) NOT NULL,
    ChuKyPhatHanh NVARCHAR(50),
    TrangThai BIT DEFAULT 1,

    FOREIGN KEY (MaDanhMuc)
        REFERENCES DanhMuc(MaDanhMuc)
);
GO

-- 5. BẢNG SỐ BÁO
CREATE TABLE SoBao
(
    MaSoBao INT IDENTITY(1,1) PRIMARY KEY,
    MaAnPham INT NOT NULL,
    SoThuTu INT NOT NULL,
    NgayPhatHanh DATE NOT NULL,
    SoLuongIn INT NOT NULL,
    SoLuongDaPhatHanh INT DEFAULT 0,
    TrangThai NVARCHAR(50) DEFAULT N'Chuẩn bị phát hành',

    FOREIGN KEY (MaAnPham)
        REFERENCES AnPham(MaAnPham)
);
GO

-- 6. BẢNG ĐĂNG KÝ
CREATE TABLE DangKy
(
    MaDangKy INT IDENTITY(1,1) PRIMARY KEY,
    MaDocGia INT NOT NULL,
    MaAnPham INT NOT NULL,
    NgayDangKy DATE DEFAULT GETDATE(),
    NgayBatDau DATE NOT NULL,
    NgayKetThuc DATE NOT NULL,
    KyHan INT NOT NULL,
    SoLuong INT DEFAULT 1,
    TrangThai NVARCHAR(50) DEFAULT N'Chờ thanh toán',
    IsDeleted BIT DEFAULT 0,
    DeletedAt DATETIME NULL,
    DeletedBy INT NULL,
    Version INT DEFAULT 1,

    FOREIGN KEY (MaDocGia)
        REFERENCES DocGia(MaDocGia),

    FOREIGN KEY (MaAnPham)
        REFERENCES AnPham(MaAnPham)
);
GO

-- 7. BẢNG PHÁT HÀNH
CREATE TABLE PhatHanh
(
    MaPhatHanh INT IDENTITY(1,1) PRIMARY KEY,
    MaDangKy INT NOT NULL,
    MaSoBao INT NOT NULL,
    NgayPhatHanh DATE DEFAULT GETDATE(),
    SoLuong INT NOT NULL,
    HinhThuc NVARCHAR(50),
    DiaChiGiao NVARCHAR(255),
    TrangThai NVARCHAR(50) DEFAULT N'Chờ xử lý',
    IsDeleted BIT DEFAULT 0,
    DeletedAt DATETIME NULL,
    DeletedBy INT NULL,
    Version INT DEFAULT 1,

    FOREIGN KEY (MaDangKy)
        REFERENCES DangKy(MaDangKy),

    FOREIGN KEY (MaSoBao)
        REFERENCES SoBao(MaSoBao)
);
GO

-- 8. BẢNG HÓA ĐƠN
CREATE TABLE HoaDon
(
    MaHoaDon INT IDENTITY(1,1) PRIMARY KEY,
    MaDangKy INT NOT NULL UNIQUE,
    NgayLap DATE DEFAULT GETDATE(),
    TongTien DECIMAL(18,2) NOT NULL,
    GiamGia DECIMAL(18,2) DEFAULT 0,
    ThanhTien AS (TongTien - GiamGia),
    TrangThai NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    IsDeleted BIT DEFAULT 0,
    DeletedAt DATETIME NULL,
    DeletedBy INT NULL,
    Version INT DEFAULT 1,

    FOREIGN KEY (MaDangKy)
        REFERENCES DangKy(MaDangKy)
);
GO

-- 9. BẢNG THANH TOÁN
CREATE TABLE ThanhToan
(
    MaThanhToan INT IDENTITY(1,1) PRIMARY KEY,
    MaHoaDon INT NOT NULL,
    NgayThanhToan DATETIME DEFAULT GETDATE(),
    SoTien DECIMAL(18,2) NOT NULL,
    PhuongThuc NVARCHAR(50) NOT NULL,
    TrangThai NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    IsDeleted BIT DEFAULT 0,
    DeletedAt DATETIME NULL,
    DeletedBy INT NULL,
    Version INT DEFAULT 1,

    FOREIGN KEY (MaHoaDon)
        REFERENCES HoaDon(MaHoaDon)
);
GO

-- 10. BẢNG THÔNG BÁO
CREATE TABLE ThongBao
(
    MaThongBao INT IDENTITY(1,1) PRIMARY KEY,
    MaDocGia INT NOT NULL,
    TieuDe NVARCHAR(200) NOT NULL,
    NoiDung NVARCHAR(1000) NOT NULL,
    NgayGui DATETIME DEFAULT GETDATE(),
    TrangThai BIT DEFAULT 0,

    FOREIGN KEY (MaDocGia)
        REFERENCES DocGia(MaDocGia)
);
GO

-- 11. BẢNG AUDIT LOG (NHẬT KÝ HỆ THỐNG)
CREATE TABLE AuditLog
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    TenDangNhap VARCHAR(50) NULL,
    HanhDong NVARCHAR(50) NOT NULL,
    Bang NVARCHAR(50) NOT NULL,
    KhoaChinh NVARCHAR(50) NULL,
    DuLieuCu NVARCHAR(MAX) NULL,
    DuLieuMoi NVARCHAR(MAX) NULL,
    MoTa NVARCHAR(500) NULL,
    ThoiGian DATETIME DEFAULT GETDATE(),
    IpAddress VARCHAR(45) NULL
);
GO

-- 12. BẢNG LỊCH SỬ PHIÊN BẢN (VERSION CONTROL)
CREATE TABLE LichSuPhienBan
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Bang NVARCHAR(50) NOT NULL,
    KhoaChinh NVARCHAR(50) NOT NULL,
    SoPhienBan INT NOT NULL,
    DuLieuSnapshot NVARCHAR(MAX) NOT NULL,
    NguoiCapNhat NVARCHAR(100) NULL,
    ThoiGianCapNhat DATETIME DEFAULT GETDATE(),
    GhiChu NVARCHAR(500) NULL
);
GO

-- ========================================================
-- PHẦN 2: CHÈN DỮ LIỆU MẪU BAN ĐẦU
-- ========================================================

-- 1. BẢNG DANH MỤC (10 bản ghi)
INSERT INTO DanhMuc (TenDanhMuc, MoTa)
VALUES
(N'Báo chính trị - xã hội', N'Các ấn phẩm báo chí về chính trị, thời sự và đời sống xã hội'),
(N'Báo kinh tế - tài chính', N'Thông tin thị trường, chứng khoán, đầu tư và tài chính ngân hàng'),
(N'Tạp chí khoa học & công nghệ', N'Chuyên ngành khoa học, công nghệ thông tin và chuyển đổi số'),
(N'Tạp chí giáo dục & đào tạo', N'Nghiên cứu giáo dục, dạy học và thông tin tuyển sinh'),
(N'Tạp chí văn học nghệ thuật', N'Sáng tác văn học, bình luận nghệ thuật và điện ảnh'),
(N'Báo thể thao & giải trí', N'Tin tức thể thao trong nước, quốc tế và hoạt động giải trí'),
(N'Báo y tế & sức khỏe', N'Y học thường thức, tư vấn sức khỏe gia đình và phòng dịch'),
(N'Tạp chí du lịch & khám phá', N'Khám phá văn hóa các vùng miền và điểm đến du lịch'),
(N'Tạp chí pháp luật & đời sống', N'Tuyên truyền pháp luật, tư vấn pháp lý và phóng sự điều tra'),
(N'Tạp chí kiến trúc & xây dựng', N'Quy hoạch đô thị, kiến trúc nhà ở và vật liệu xây dựng mới');
GO

-- 2. BẢNG ẤN PHẨM (15 bản ghi)
INSERT INTO AnPham (MaDanhMuc, TenAnPham, MoTa, Gia, ChuKyPhatHanh, TrangThai)
VALUES
(1,  N'Báo Nhân Dân', N'Cơ quan ngôn luận của Đảng Cộng sản Việt Nam', 10000, N'Hàng ngày', 1),
(1,  N'Báo Tuổi Trẻ', N'Tin tức thời sự nóng hổi và đời sống thanh niên', 12000, N'Hàng ngày', 1),
(1,  N'Báo Thanh Niên', N'Diễn đàn của giới trẻ và các vấn đề thời sự', 12000, N'Hàng ngày', 1),
(2,  N'Thời Báo Kinh Tế Sài Gòn', N'Phân tích chuyên sâu về kinh tế và kinh doanh', 25000, N'Hàng tuần', 1),
(2,  N'Tạp chí Đầu Tư Tài Chính', N'Xu hướng thị trường chứng khoán và bất động sản', 30000, N'Hàng tháng', 1),
(3,  N'Tạp chí Tin Học & Nhà Trường', N'Ứng dụng công nghệ thông tin trong dạy và học', 28000, N'Hàng tháng', 1),
(3,  N'Tạp chí Công Nghệ Số', N'Trí tuệ nhân tạo, an toàn thông tin và chuyển đổi số', 35000, N'Hàng tháng', 1),
(4,  N'Tạp chí Giáo Dục Việt Nam', N'Chính sách giáo dục và phương pháp giảng dạy hiện đại', 22000, N'Hàng tháng', 1),
(5,  N'Tạp chí Văn Nghệ Quân Đội', N'Tác phẩm văn học, thơ ca và lý luận phê bình', 20000, N'Hàng tháng', 1),
(6,  N'Báo Thể Thao 24/7', N'Cập nhật tin tức bóng đá và thể thao thế giới', 15000, N'Hàng ngày', 1),
(7,  N'Báo Sức Khỏe & Đời Sống', N'Cơ quan ngôn luận của Bộ Y Tế Việt Nam', 14000, N'Hàng tuần', 1),
(8,  N'Tạp chí Du Lịch Việt', N'Cẩm nang du lịch và khám phá danh lam thắng cảnh', 32000, N'Hàng tháng', 1),
(9,  N'Tạp chí Pháp Luật & Thời Đại', N'Phổ biến pháp luật và tư vấn hôn nhân gia đình', 20000, N'Hàng tháng', 1),
(10, N'Tạp chí Kiến Trúc Đô Thị', N'Thiết kế không gian sống và công trình kiến trúc xanh', 45000, N'Hàng tháng', 1),
(2,  N'Báo Diễn Đàn Doanh Nghiệp', N'Tiếng nói của cộng đồng doanh nghiệp Việt Nam', 18000, N'Hàng tuần', 1);
GO

-- 3. BẢNG SỐ BÁO (25 bản ghi)
INSERT INTO SoBao (MaAnPham, SoThuTu, NgayPhatHanh, SoLuongIn, SoLuongDaPhatHanh, TrangThai)
VALUES
(1,  101,  '2026-10-01', 5000, 4800, N'Đã phát hành'),
(1,  102,  '2026-10-02', 5000, 4900, N'Đã phát hành'),
(1,  103,  '2026-10-03', 5000, 4750, N'Đã phát hành'),
(2,  201,  '2026-10-01', 6000, 5800, N'Đã phát hành'),
(2,  202,  '2026-10-02', 6000, 5950, N'Đã phát hành'),
(3,  301,  '2026-10-01', 4500, 4300, N'Đã phát hành'),
(3,  302,  '2026-10-02', 4500, 4400, N'Đã phát hành'),
(4,  401,  '2026-10-05', 3000, 2800, N'Đã phát hành'),
(4,  402,  '2026-10-12', 3000, 2900, N'Đã phát hành'),
(5,  501,  '2026-10-01', 2000, 1950, N'Đã phát hành'),
(6,  601,  '2026-10-01', 2500, 2300, N'Đã phát hành'),
(7,  701,  '2026-10-01', 3500, 3200, N'Đã phát hành'),
(7,  702,  '2026-11-01', 3500, 0,    N'Chuẩn bị phát hành'),
(8,  801,  '2026-10-01', 2200, 2100, N'Đã phát hành'),
(9,  901,  '2026-10-01', 1800, 1750, N'Đã phát hành'),
(10, 1001, '2026-10-01', 4000, 3900, N'Đã phát hành'),
(10, 1002, '2026-10-02', 4000, 3850, N'Đã phát hành'),
(11, 1101, '2026-10-03', 2800, 2600, N'Đã phát hành'),
(12, 1201, '2026-10-01', 2500, 2400, N'Đã phát hành'),
(13, 1301, '2026-10-01', 2000, 1900, N'Đã phát hành'),
(14, 1401, '2026-10-01', 1500, 1450, N'Đã phát hành'),
(15, 1501, '2026-10-04', 2200, 2100, N'Đã phát hành'),
(1,  104,  '2026-11-15', 5000, 0,    N'Chuẩn bị phát hành'),
(2,  203,  '2026-11-15', 6000, 0,    N'Chuẩn bị phát hành'),
(4,  403,  '2026-11-19', 3000, 0,    N'Chuẩn bị phát hành');
GO

-- 4. BẢNG NGƯỜI DÙNG (15 bản ghi)

INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, QuyenHan, TrangThai)
VALUES
('admin',     '123456', N'Lê Đình Tuyên',  'admin@gmail.com',     '0901234567', N'Admin',         1),
('bientap01', '123456', N'Trần Văn Bình',  'bientap01@gmail.com', '0901000002', N'Biên tập viên', 1),
('bientap02', '123456', N'Nguyễn Thị Mai', 'bientap02@gmail.com', '0901000003', N'Biên tập viên', 1),
('docgia01',  '123456', N'Phạm Văn Nam',   'docgia01@gmail.com',  '0902000001', N'Độc giả',       1),
('docgia02',  '123456', N'Hoàng Thị Lan',  'docgia02@gmail.com',  '0902000002', N'Độc giả',       1),
('docgia03',  '123456', N'Vũ Đình Khang',  'docgia03@gmail.com',  '0902000003', N'Độc giả',       1),
('docgia04',  '123456', N'Đỗ Thị Hương',   'docgia04@gmail.com',  '0902000004', N'Độc giả',       1),
('docgia05',  '123456', N'Bùi Quang Dũng', 'docgia05@gmail.com',  '0902000005', N'Độc giả',       1),
('docgia06',  '123456', N'Ngô Thanh Thảo', 'docgia06@gmail.com',  '0902000006', N'Độc giả',       1),
('docgia07',  '123456', N'Đặng Quốc Bảo',  'docgia07@gmail.com',  '0902000007', N'Độc giả',       1),
('docgia08',  '123456', N'Dương Thúy Nga', 'docgia08@gmail.com',  '0902000008', N'Độc giả',       1),
('docgia09',  '123456', N'Lý Văn Cường',   'docgia09@gmail.com',  '0902000009', N'Độc giả',       1),
('docgia10',  '123456', N'Trịnh Diệu Linh','docgia10@gmail.com',  '0902000010', N'Độc giả',       1),
('docgia11',  '123456', N'Hà Anh Tuấn',    'docgia11@gmail.com',  '0902000011', N'Độc giả',       1),
('docgia12',  '123456', N'Mai Phương Loan','docgia12@gmail.com',  '0902000012', N'Độc giả',       1);
GO

-- 5. BẢNG ĐỘC GIẢ (12 bản ghi, tương ứng MaNguoiDung 4..15)
INSERT INTO DocGia (MaNguoiDung, NgaySinh, DiaChi, IsDeleted, Version)
VALUES
(4,  '1995-03-15', N'123 Cầu Giấy, Hà Nội',                           0, 1),
(5,  '1998-07-22', N'45 Lê Lợi, Quận 1, TP. Hồ Chí Minh',             0, 1),
(6,  '2000-11-05', N'78 Trần Phú, Hải Phòng',                         0, 1),
(7,  '1992-04-18', N'89 Nguyễn Huệ, Đà Nẵng',                         0, 1),
(8,  '1989-09-30', N'12 Phan Bội Châu, Cần Thơ',                      0, 1),
(9,  '2001-01-12', N'56 Hùng Vương, Bắc Ninh',                        0, 1),
(10, '1997-06-25', N'34 Hoàng Hoa Thám, Nam Định',                    0, 1),
(11, '1994-12-08', N'67 Quang Trung, Hà Đông, Hà Nội',                0, 1),
(12, '1999-08-14', N'90 Điện Biên Phủ, Thanh Khê, Đà Nẵng',           0, 1),
(13, '1996-02-28', N'23 Võ Thị Sáu, Biên Hòa, Đồng Nai',              0, 1),
(14, '1993-10-19', N'11 Hai Bà Trưng, TP. Huế',                       0, 1),
(15, '2002-05-03', N'88 Lý Thường Kiệt, TP. Thái Nguyên',             0, 1);
GO

-- 6. BẢNG ĐĂNG KÝ (20 bản ghi)
INSERT INTO DangKy (MaDocGia, MaAnPham, NgayDangKy, NgayBatDau, NgayKetThuc, KyHan, SoLuong, TrangThai, IsDeleted, Version)
VALUES
(1,  1,  '2026-09-01', '2026-09-01', '2026-09-30', 1,  1, N'Đã thanh toán',  0, 1),
(1,  4,  '2026-09-01', '2026-09-01', '2027-08-31', 12, 1, N'Đang hoạt động', 0, 1),
(2,  2,  '2026-09-02', '2026-09-02', '2026-12-01', 3,  1, N'Đang hoạt động', 0, 1),
(2,  5,  '2026-09-05', '2026-09-05', '2027-09-04', 12, 1, N'Đang hoạt động', 0, 1),
(3,  3,  '2026-09-06', '2026-09-06', '2026-10-05', 1,  2, N'Đã thanh toán',  0, 1),
(3,  7,  '2026-09-10', '2026-09-10', '2027-09-09', 12, 1, N'Đang hoạt động', 0, 1),
(4,  6,  '2026-09-11', '2026-09-11', '2026-12-10', 3,  1, N'Đang hoạt động', 0, 1),
(4,  10, '2026-09-15', '2026-09-15', '2026-10-14', 1,  1, N'Đã thanh toán',  0, 1),
(5,  8,  '2026-09-15', '2026-09-15', '2027-09-14', 12, 1, N'Đang hoạt động', 0, 1),
(6,  9,  '2026-09-18', '2026-09-18', '2026-12-17', 3,  1, N'Đang hoạt động', 0, 1),
(7,  11, '2026-09-20', '2026-09-20', '2026-10-19', 1,  1, N'Đã thanh toán',  0, 1),
(8,  12, '2026-09-22', '2026-09-22', '2027-09-21', 12, 1, N'Đang hoạt động', 0, 1),
(9,  13, '2026-09-25', '2026-09-25', '2026-12-24', 3,  1, N'Đang hoạt động', 0, 1),
(10, 14, '2026-09-26', '2026-09-26', '2027-09-25', 12, 1, N'Đang hoạt động', 0, 1),
(11, 15, '2026-09-28', '2026-09-28', '2026-10-27', 1,  1, N'Đã thanh toán',  0, 1),
(12, 1,  '2026-09-30', '2026-09-30', '2027-09-29', 12, 1, N'Đang hoạt động', 0, 1),
(5,  2,  '2026-10-01', '2026-10-01', '2026-10-31', 1,  1, N'Chờ thanh toán', 0, 1),
(6,  4,  '2026-10-02', '2026-10-02', '2027-10-01', 12, 1, N'Chờ thanh toán', 0, 1),
(7,  7,  '2026-10-03', '2026-10-03', '2026-11-02', 1,  1, N'Chờ thanh toán', 0, 1),
(8,  10, '2026-10-05', '2026-10-05', '2026-12-04', 3,  1, N'Chờ thanh toán', 0, 1);
GO

-- 7. BẢNG HÓA ĐƠN (16 bản ghi, tương ứng MaDangKy 1..16)
INSERT INTO HoaDon (MaDangKy, NgayLap, TongTien, GiamGia, TrangThai, IsDeleted, Version)
VALUES
(1,  '2026-09-01', 300000, 0,     N'Đã thanh toán', 0, 1),
(2,  '2026-09-01', 300000, 15000, N'Đã thanh toán', 0, 1),
(3,  '2026-09-02', 360000, 0,     N'Đã thanh toán', 0, 1),
(4,  '2026-09-05', 360000, 20000, N'Đã thanh toán', 0, 1),
(5,  '2026-09-06', 240000, 0,     N'Đã thanh toán', 0, 1),
(6,  '2026-09-10', 420000, 25000, N'Đã thanh toán', 0, 1),
(7,  '2026-09-11', 252000, 0,     N'Đã thanh toán', 0, 1),
(8,  '2026-09-15', 180000, 0,     N'Đã thanh toán', 0, 1),
(9,  '2026-09-15', 264000, 14000, N'Đã thanh toán', 0, 1),
(10, '2026-09-18', 240000, 0,     N'Đã thanh toán', 0, 1),
(11, '2026-09-20', 168000, 0,     N'Đã thanh toán', 0, 1),
(12, '2026-09-22', 384000, 30000, N'Đã thanh toán', 0, 1),
(13, '2026-09-25', 240000, 0,     N'Đã thanh toán', 0, 1),
(14, '2026-09-26', 540000, 40000, N'Đã thanh toán', 0, 1),
(15, '2026-09-28', 216000, 0,     N'Đã thanh toán', 0, 1),
(16, '2026-09-30', 360000, 20000, N'Đã thanh toán', 0, 1);
GO

-- 8. BẢNG THANH TOÁN (16 bản ghi, tương ứng MaHoaDon 1..16)
INSERT INTO ThanhToan (MaHoaDon, NgayThanhToan, SoTien, PhuongThuc, TrangThai, IsDeleted, Version)
VALUES
(1,  '2026-09-01 08:30:00', 300000, N'Chuyển khoản QR', N'Đã thanh toán', 0, 1),
(2,  '2026-09-01 09:15:00', 285000, N'Ví điện tử',      N'Đã thanh toán', 0, 1),
(3,  '2026-09-02 10:20:00', 360000, N'Tiền mặt',        N'Đã thanh toán', 0, 1),
(4,  '2026-09-05 14:00:00', 340000, N'Chuyển khoản QR', N'Đã thanh toán', 0, 1),
(5,  '2026-09-06 11:30:00', 240000, N'Chuyển khoản QR', N'Đã thanh toán', 0, 1),
(6,  '2026-09-10 15:45:00', 395000, N'Ví điện tử',      N'Đã thanh toán', 0, 1),
(7,  '2026-09-11 08:10:00', 252000, N'Tiền mặt',        N'Đã thanh toán', 0, 1),
(8,  '2026-09-15 16:30:00', 180000, N'Chuyển khoản QR', N'Đã thanh toán', 0, 1),
(9,  '2026-09-15 17:00:00', 250000, N'Ví điện tử',      N'Đã thanh toán', 0, 1),
(10, '2026-09-18 09:20:00', 240000, N'Tiền mặt',        N'Đã thanh toán', 0, 1),
(11, '2026-09-20 10:05:00', 168000, N'Chuyển khoản QR', N'Đã thanh toán', 0, 1),
(12, '2026-09-22 14:40:00', 354000, N'Ví điện tử',      N'Đã thanh toán', 0, 1),
(13, '2026-09-25 11:15:00', 240000, N'Tiền mặt',        N'Đã thanh toán', 0, 1),
(14, '2026-09-26 15:50:00', 500000, N'Chuyển khoản QR', N'Đã thanh toán', 0, 1),
(15, '2026-09-28 08:45:00', 216000, N'Ví điện tử',      N'Đã thanh toán', 0, 1),
(16, '2026-09-30 13:25:00', 340000, N'Chuyển khoản QR', N'Đã thanh toán', 0, 1);
GO

-- 9. BẢNG PHÁT HÀNH (22 bản ghi)
INSERT INTO PhatHanh (MaDangKy, MaSoBao, NgayPhatHanh, SoLuong, HinhThuc, DiaChiGiao, TrangThai, IsDeleted, Version)
VALUES
(1,  1,  '2026-10-01', 1, N'Giao báo tận nơi', N'123 Cầu Giấy, Hà Nội',                    N'Đã giao',   0, 1),
(1,  2,  '2026-10-02', 1, N'Giao báo tận nơi', N'123 Cầu Giấy, Hà Nội',                    N'Đã giao',   0, 1),
(1,  3,  '2026-10-03', 1, N'Giao báo tận nơi', N'123 Cầu Giấy, Hà Nội',                    N'Đang giao', 0, 1),
(2,  8,  '2026-10-05', 1, N'Giao báo tận nơi', N'123 Cầu Giấy, Hà Nội',                    N'Đã giao',   0, 1),
(3,  4,  '2026-10-01', 1, N'Giao báo tận nơi', N'45 Lê Lợi, Quận 1, TP. Hồ Chí Minh',     N'Đã giao',   0, 1),
(3,  5,  '2026-10-02', 1, N'Giao báo tận nơi', N'45 Lê Lợi, Quận 1, TP. Hồ Chí Minh',     N'Đã giao',   0, 1),
(4,  10, '2026-10-01', 1, N'Giao báo tận nơi', N'45 Lê Lợi, Quận 1, TP. Hồ Chí Minh',     N'Đã giao',   0, 1),
(5,  6,  '2026-10-01', 2, N'Giao báo tận nơi', N'78 Trần Phú, Hải Phòng',                 N'Đã giao',   0, 1),
(5,  7,  '2026-10-02', 2, N'Giao báo tận nơi', N'78 Trần Phú, Hải Phòng',                 N'Đang giao', 0, 1),
(6,  12, '2026-10-01', 1, N'Nhận tại bưu cục', N'78 Trần Phú, Hải Phòng',                 N'Đã giao',   0, 1),
(7,  11, '2026-10-01', 1, N'Giao báo tận nơi', N'89 Nguyễn Huệ, Đà Nẵng',                 N'Đã giao',   0, 1),
(8,  16, '2026-10-01', 1, N'Giao báo tận nơi', N'89 Nguyễn Huệ, Đà Nẵng',                 N'Đã giao',   0, 1),
(8,  17, '2026-10-02', 1, N'Giao báo tận nơi', N'89 Nguyễn Huệ, Đà Nẵng',                 N'Đang giao', 0, 1),
(9,  14, '2026-10-01', 1, N'Giao báo tận nơi', N'12 Phan Bội Châu, Cần Thơ',              N'Đã giao',   0, 1),
(10, 15, '2026-10-01', 1, N'Giao báo tận nơi', N'56 Hùng Vương, Bắc Ninh',                N'Đã giao',   0, 1),
(11, 18, '2026-10-03', 1, N'Giao báo tận nơi', N'34 Hoàng Hoa Thám, Nam Định',            N'Đã giao',   0, 1),
(12, 19, '2026-10-01', 1, N'Nhận tại bưu cục', N'67 Quang Trung, Hà Đông, Hà Nội',        N'Đã giao',   0, 1),
(13, 20, '2026-10-01', 1, N'Giao báo tận nơi', N'90 Điện Biên Phủ, Thanh Khê, Đà Nẵng',   N'Đã giao',   0, 1),
(14, 21, '2026-10-01', 1, N'Giao báo tận nơi', N'23 Võ Thị Sáu, Biên Hòa, Đồng Nai',      N'Đã giao',   0, 1),
(15, 22, '2026-10-04', 1, N'Giao báo tận nơi', N'11 Hai Bà Trưng, TP. Huế',               N'Đã giao',   0, 1),
(16, 1,  '2026-10-01', 1, N'Giao báo tận nơi', N'88 Lý Thường Kiệt, TP. Thái Nguyên',     N'Đã giao',   0, 1),
(16, 2,  '2026-10-02', 1, N'Giao báo tận nơi', N'88 Lý Thường Kiệt, TP. Thái Nguyên',     N'Chờ xử lý', 0, 1);
GO

-- 10. BẢNG THÔNG BÁO (15 bản ghi, MaDocGia 1..12)
INSERT INTO ThongBao (MaDocGia, TieuDe, NoiDung, NgayGui, TrangThai)
VALUES
(1,  N'Đăng ký thành công',       N'Chào mừng bạn đến với tòa soạn. Đăng ký thuê bao #ĐK-1 đã kích hoạt thành công.', '2026-09-01 08:35:00', 1),
(1,  N'Số báo mới phát hành',     N'Báo Nhân Dân số 101 đã xuất bản và đang trên đường giao đến bạn.',                 '2026-10-01 07:00:00', 1),
(2,  N'Thanh toán hoàn tất',      N'Hóa đơn #HD-3 đã được xác nhận thanh toán thành công qua Tiền mặt.',               '2026-09-02 10:25:00', 1),
(2,  N'Số báo mới phát hành',     N'Báo Tuổi Trẻ số 201 đã được gửi tới địa chỉ của bạn.',                             '2026-10-01 07:15:00', 1),
(3,  N'Xác nhận đơn đặt báo',     N'Thuê bao đặt Báo Thanh Niên của bạn đã sẵn sàng phục vụ.',                         '2026-09-06 11:35:00', 1),
(4,  N'Phát hành kỳ mới',         N'Tạp chí Tin Học & Nhà Trường số 601 phát hành định kỳ tháng 10.',                 '2026-10-01 08:00:00', 0),
(5,  N'Ưu đãi bạn đọc thân thiết',N'Tòa soạn gửi tặng bạn mã giảm giá 10% cho kỳ gia hạn tiếp theo.',                  '2026-09-15 09:00:00', 1),
(6,  N'Thông báo xuất bản',       N'Tạp chí Văn Nghệ Quân Đội số 901 đã được phát hành.',                              '2026-10-01 08:30:00', 1),
(7,  N'Giao nhận thành công',     N'Lệnh phát hành #PH-11 Báo Sức Khỏe & Đời Sống đã giao thành công.',               '2026-10-03 16:00:00', 1),
(8,  N'Khám phá số báo mới',      N'Tạp chí Du Lịch Việt số 1201 với chuyên đề Mùa Thu Hà Nội.',                      '2026-10-01 09:00:00', 0),
(9,  N'Tư vấn pháp lý',           N'Tạp chí Pháp Luật & Thời Đại số 1301 đã được chuyển đến bưu tá.',                 '2026-10-01 09:30:00', 1),
(10, N'Kiến trúc xanh 2026',      N'Tạp chí Kiến Trúc Đô Thị số 1401 đã phát hành trên toàn quốc.',                    '2026-10-01 10:00:00', 1),
(11, N'Diễn đàn doanh nghiệp',    N'Báo Diễn Đàn Doanh Nghiệp số 1501 cập nhật kinh tế tuần mới.',                     '2026-10-04 07:30:00', 0),
(12, N'Kích hoạt thuê bao năm',   N'Gói thuê bao 12 tháng Báo Nhân Dân của bạn đã có hiệu lực.',                       '2026-09-30 13:30:00', 1),
(1,  N'Lịch phát hành kỳ lễ',     N'Thông báo lịch phát hành và giao báo các ngày nghỉ lễ sắp tới.',                  '2026-10-10 14:00:00', 0);
GO

-- 11. BẢNG AUDIT LOG: ĐỂ TRỐNG BAN ĐẦU (Hệ thống sẽ tự động ghi log khi vận hành)

-- 12. BẢNG LỊCH SỬ PHIÊN BẢN: ĐỂ TRỐNG BAN ĐẦU (Hệ thống sẽ tự động ghi snapshot khi cập nhật)

-- --------------------------------------------------------
-- ĐỒNG BỘ TRẠNG THÁI KHỞI TẠO SOFT DELETE
-- --------------------------------------------------------
UPDATE DocGia    SET IsDeleted = 0, Version = 1 WHERE IsDeleted IS NULL;
UPDATE DangKy    SET IsDeleted = 0, Version = 1 WHERE IsDeleted IS NULL;
UPDATE PhatHanh  SET IsDeleted = 0, Version = 1 WHERE IsDeleted IS NULL;
UPDATE HoaDon    SET IsDeleted = 0, Version = 1 WHERE IsDeleted IS NULL;
UPDATE ThanhToan SET IsDeleted = 0, Version = 1 WHERE IsDeleted IS NULL;
GO

SELECT 'CSDL QuanLyPhatHanhBaoChi đã khởi tạo và nạp dữ liệu hoàn tất thành công!' AS KetQua;
GO
