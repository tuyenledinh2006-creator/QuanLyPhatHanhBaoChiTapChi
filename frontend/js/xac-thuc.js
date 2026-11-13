/**
 * xac-thuc.js — Tiện ích quản lý phiên đăng nhập và bảo vệ trang
 */

const XacThuc = {
  // Lấy thông tin người dùng từ sessionStorage
  layNguoiDung: function() {
    try {
      const data = sessionStorage.getItem(KHOA_PHIEN.nguoi_dung);
      return data ? JSON.parse(data) : null;
    } catch (e) {
      return null;
    }
  },

  // Lấy JWT Bearer token
  layToken: function() {
    return sessionStorage.getItem(KHOA_PHIEN.token) || '';
  },

  // Lấy Refresh Token
  layRefreshToken: function() {
    return sessionStorage.getItem(KHOA_PHIEN.refresh_token) || '';
  },

  // Chuẩn hóa tên vai trò chuẩn xác 100%
  chuanHoaVaiTro: function(role) {
    if (!role && role !== 0) return 'DocGia';
    const s = role.toString().trim().toLowerCase();
    if (s === '1' || s.includes('admin') || s.includes('quản trị') || s.includes('quantri')) return 'Admin';
    if (s === '2' || s.includes('bientap') || s.includes('biên tập') || s.includes('bien tap') || s.includes('editor')) return 'BienTapVien';
    if (s === '3' || s.includes('docgia') || s.includes('độc giả') || s.includes('độc gia') || s.includes('doc gia') || s.includes('reader')) return 'DocGia';
    return 'DocGia';
  },

  // Lưu phiên đăng nhập
  luuPhien: function(user, token, refreshToken) {
    if (user) {
      user.vaiTro = this.chuanHoaVaiTro(user.vaiTro || user.quyenHan || user.role || 'DocGia');
      sessionStorage.setItem(KHOA_PHIEN.nguoi_dung, JSON.stringify(user));
    }
    if (token) {
      sessionStorage.setItem(KHOA_PHIEN.token, token);
    }
    if (refreshToken) {
      sessionStorage.setItem(KHOA_PHIEN.refresh_token, refreshToken);
    }
    sessionStorage.setItem(KHOA_PHIEN.thoi_gian, Date.now().toString());
  },

  // Kiểm tra quyền truy cập của trang hiện tại
  kiemTraQuyen: function(cacVaiTroChoPhep) {
    const user = this.layNguoiDung();
    if (!user) {
      window.location.replace('dang-nhap.html');
      return null;
    }

    const currentUrl = (window.location.href || window.location.pathname || '').toLowerCase();

    // Đặc quyền: Trang quan-ly-an-pham và quan-ly-so-bao luôn cho phép TẤT CẢ người dùng đã đăng nhập (Độc giả, Biên tập viên, Quản trị viên)
    if (currentUrl.includes('quan-ly-an-pham') || currentUrl.includes('quan-ly-so-bao')) {
      const userRole = this.chuanHoaVaiTro(user.vaiTro || user.quyenHan || user.role || user.quyen || 'DocGia');
      user.vaiTro = userRole;
      return user;
    }

    // Chuẩn hóa tên vai trò người dùng hiện tại
    const userRole = this.chuanHoaVaiTro(user.vaiTro || user.quyenHan || user.role || user.quyen || 'DocGia');
    user.vaiTro = userRole;

    if (cacVaiTroChoPhep && cacVaiTroChoPhep.length > 0) {
      const dsChoPhepChuan = cacVaiTroChoPhep.map(r => this.chuanHoaVaiTro(r).toLowerCase());
      const hopLe = dsChoPhepChuan.includes(userRole.toLowerCase()) ||
                    cacVaiTroChoPhep.some(r => r.toString().toLowerCase() === userRole.toLowerCase()) ||
                    cacVaiTroChoPhep.some(r => this.chuanHoaVaiTro(r) === userRole);

      if (!hopLe) {
        alert('Bạn không có quyền truy cập vào chức năng này!');
        window.location.replace('tong-quan.html');
        return null;
      }
    }
    return user;
  },

  // Đăng xuất khỏi hệ thống
  dangXuat: function() {
    sessionStorage.removeItem(KHOA_PHIEN.nguoi_dung);
    sessionStorage.removeItem(KHOA_PHIEN.token);
    sessionStorage.removeItem(KHOA_PHIEN.refresh_token);
    sessionStorage.removeItem(KHOA_PHIEN.thoi_gian);
    window.location.replace('dang-nhap.html');
  }
};
