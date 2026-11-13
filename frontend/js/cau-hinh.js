/**
 * cau-hinh.js — Cấu hình toàn cục của Hệ Thống Quản Lý Phát Hành Báo Chí
 */

// Base URL kết nối qua API Gateway (Ocelot)
// Tự động nhận diện origin hiện tại (http://localhost, http://localhost:8080 hoặc http://localhost:5103)
const BASE_URL = (typeof window !== 'undefined' && window.location.origin && window.location.protocol.startsWith('http'))
  ? window.location.origin
  : 'http://localhost:5103';

// Tiền tố định tuyến Ocelot Gateway
const TIEN_TO_API = {
  login:   BASE_URL + '/gateway/TaiKhoan',
  admin:   BASE_URL + '/gateway',
  bientap: BASE_URL + '/gateway',
  docgia:  BASE_URL + '/gateway'
};

// Khóa lưu phiên trong sessionStorage
const KHOA_PHIEN = {
  nguoi_dung: 'current_user',
  token: 'access_token',
  refresh_token: 'refresh_token',
  thoi_gian: 'token_received_at'
};

// Ánh xạ Quyền (int / string) -> Tên vai trò
const QUYEN_ROLE = {
  1: 'Admin',
  2: 'BienTapVien',
  3: 'DocGia'
};

// Tên hiển thị tiếng Việt của các vai trò
const TEN_VAI_TRO = {
  Admin: 'Quản Trị Viên',
  BienTapVien: 'Biên Tập Viên',
  DocGia: 'Độc Giả'
};

// Màu sắc badge định danh vai trò
const MAU_BADGE_VAI_TRO = {
  Admin: 'badge-do',
  BienTapVien: 'badge-lam',
  DocGia: 'badge-xanh'
};

// Cấu hình menu sidebar theo từng vai trò
const CAU_HINH_MENU = {
  Admin: [
    {
      nhom: 'Tổng quan & Báo cáo',
      muc: [
        { id: 'tong-quan', nhan: 'Trang Chủ', file: 'tong-quan.html', icon: 'bi-house-door', tooltip: 'Trang chủ' },
      ]
    },
    {
      nhom: 'Quản trị hệ thống',
      muc: [
        { id: 'quan-ly-tai-khoan', nhan: 'Quản Lý Người Dùng & Phân Quyền', file: 'quan-ly-tai-khoan.html', icon: 'bi-person-gear', tooltip: 'Quản lý người dùng' },
        { id: 'quan-ly-an-pham', nhan: 'Danh Mục Ấn Phẩm Báo Chí', file: 'quan-ly-an-pham.html?v=20261113_fix', icon: 'bi-newspaper', tooltip: 'Ấn phẩm' },
        { id: 'quan-ly-so-bao', nhan: 'Quản Lý Các Số Báo Theo Kỳ', file: 'quan-ly-so-bao.html?v=20261113_fix', icon: 'bi-file-earmark-richtext', tooltip: 'Số báo' },
      ]
    },
    {
      nhom: 'Kinh doanh & Phát hành',
      muc: [
        { id: 'quan-ly-dang-ky', nhan: 'Đặt Báo & Thuê Bao Định Kỳ', file: 'quan-ly-dang-ky.html', icon: 'bi-cart-plus', tooltip: 'Đặt mua báo & quản lý thuê bao' },
        { id: 'quan-ly-phat-hanh', nhan: 'Kế Hoạch & Lịch Phát Hành', file: 'quan-ly-phat-hanh.html', icon: 'bi-truck', tooltip: 'Phát hành' },
        { id: 'quan-ly-thanh-toan', nhan: 'Quản Lý Hóa Đơn & Thu Tiền', file: 'quan-ly-thanh-toan.html', icon: 'bi-cash-coin', tooltip: 'Hóa đơn & Thanh toán' },
      ]
    },
    {
      nhom: 'Giám sát hệ thống',
      muc: [
        { id: 'thong-bao', nhan: 'Gửi Thông Báo Tòa Soạn', file: 'thong-bao.html', icon: 'bi-bell', tooltip: 'Thông báo' },
        { id: 'nhat-ky-he-thong', nhan: 'Nhật Ký & Lịch Sử', file: 'nhat-ky-he-thong.html', icon: 'bi-clock-history', tooltip: 'Audit Log & Snapshot' },
      ]
    },
    {
      nhom: 'Tài khoản & Cá nhân',
      muc: [
        { id: 'thong-tin-ca-nhan', nhan: 'Thông Tin Cá Nhân', file: 'thong-tin-ca-nhan.html', icon: 'bi-person-badge', tooltip: 'Thông tin cá nhân & Đổi mật khẩu' }
      ]
    }
  ],
  BienTapVien: [
    {
      nhom: 'Tổng quan',
      muc: [
        { id: 'tong-quan', nhan: 'Trang Chủ', file: 'tong-quan.html', icon: 'bi-house-door', tooltip: 'Trang chủ' },
      ]
    },
    {
      nhom: 'Nghiệp vụ xuất bản & Biên tập',
      muc: [
        { id: 'quan-ly-an-pham', nhan: 'Danh Mục Ấn Phẩm', file: 'quan-ly-an-pham.html?v=20261113_fix', icon: 'bi-journal-text', tooltip: 'Ấn phẩm' },
        { id: 'quan-ly-so-bao', nhan: 'Quản Lý Các Số Báo Theo Kỳ', file: 'quan-ly-so-bao.html?v=20261113_fix', icon: 'bi-file-earmark-richtext', tooltip: 'Số báo' },
        { id: 'quan-ly-dang-ky', nhan: 'Đặt Báo & Thuê Bao Định Kỳ', file: 'quan-ly-dang-ky.html', icon: 'bi-cart-plus', tooltip: 'Đặt mua báo & quản lý thuê bao' },
        { id: 'quan-ly-phat-hanh', nhan: 'Kế Hoạch & Lịch Phát Hành', file: 'quan-ly-phat-hanh.html', icon: 'bi-truck', tooltip: 'Phát hành' },
        { id: 'quan-ly-thanh-toan', nhan: 'Quản Lý Hóa Đơn & Thu Tiền', file: 'quan-ly-thanh-toan.html', icon: 'bi-cash-coin', tooltip: 'Xem hóa đơn & Doanh thu' },
        { id: 'thong-bao', nhan: 'Gửi Thông Báo Tòa Soạn', file: 'thong-bao.html', icon: 'bi-bell', tooltip: 'Thông báo' },
      ]
    },
    {
      nhom: 'Tài khoản & Cá nhân',
      muc: [
        { id: 'thong-tin-ca-nhan', nhan: 'Thông Tin Cá Nhân', file: 'thong-tin-ca-nhan.html', icon: 'bi-person-badge', tooltip: 'Thông tin cá nhân & Đổi mật khẩu' }
      ]
    }
  ],
  DocGia: [
    {
      nhom: 'Không gian Độc giả',
      muc: [
        { id: 'tong-quan', nhan: 'Trang Chủ', file: 'tong-quan.html', icon: 'bi-house-door', tooltip: 'Trang chủ' },
        { id: 'quan-ly-an-pham', nhan: 'Tra Cứu Ấn Phẩm Báo Chí', file: 'quan-ly-an-pham.html?v=20261113_fix', icon: 'bi-newspaper', tooltip: 'Tra cứu & Xem ấn phẩm báo chí' },
        { id: 'quan-ly-so-bao', nhan: 'Tra Cứu Số Báo Đã Phát Hành', file: 'quan-ly-so-bao.html?v=20261113_fix', icon: 'bi-file-earmark-richtext', tooltip: 'Tra cứu các kỳ báo & số báo đã phát hành' },
        { id: 'quan-ly-dang-ky', nhan: 'Đăng Ký Đặt Báo & Thuê Bao', file: 'quan-ly-dang-ky.html', icon: 'bi-cart-plus', tooltip: 'Đặt báo' },
        { id: 'quan-ly-thanh-toan', nhan: 'Hóa Đơn & Thanh Toán Online', file: 'quan-ly-thanh-toan.html', icon: 'bi-credit-card-2-front', tooltip: 'Thanh toán' },
        { id: 'lich-su-giao-bao', nhan: 'Lịch Sử Giao Nhận Báo Định Kỳ', file: 'lich-su-giao-bao.html', icon: 'bi-box-seam', tooltip: 'Giao nhận' },
        { id: 'thong-bao', nhan: 'Thông Báo Từ Tòa Soạn', file: 'thong-bao.html', icon: 'bi-bell-fill', tooltip: 'Thông báo' },
      ]
    },
    {
      nhom: 'Cá nhân & Cài đặt',
      muc: [
        { id: 'thong-tin-ca-nhan', nhan: 'Thông Tin Cá Nhân', file: 'thong-tin-ca-nhan.html', icon: 'bi-person-badge', tooltip: 'Thông tin cá nhân & Địa chỉ giao báo' }
      ]
    }
  ]
};
