/**
 * dieu-huong.js — Xây dựng thanh bên Sidebar và Topbar theo vai trò
 */

const DieuHuong = {
  khoiTao: function() {
    const user = XacThuc.layNguoiDung();
    if (!user) return;

    let role = XacThuc.chuanHoaVaiTro ? XacThuc.chuanHoaVaiTro(user.vaiTro || user.quyenHan || user.role) : (user.vaiTro || 'DocGia');

    const danhSachNhom = CAU_HINH_MENU[role] || CAU_HINH_MENU.DocGia;
    const tenTrangHienTai = (window.location.pathname.split('/').pop() || 'tong-quan.html').split('?')[0];

    // 1. Render Menu Sidebar đúng chuẩn class của bo-cuc.css
    const navEl = document.querySelector('.sidebar-noi-dung');
    if (navEl) {
      let html = '';
      danhSachNhom.forEach(nhom => {
        html += `<div class="sidebar-nhom">`;
        html += `<div class="sidebar-nhom-ten">${nhom.nhom}</div>`;
        nhom.muc.forEach(m => {
          const tenFileMenu = (m.file || '').split('?')[0];
          const isActive = (tenTrangHienTai === tenFileMenu || tenTrangHienTai === m.file) ? 'active' : '';
          html += `
            <a href="${m.file}" class="menu-muc ${isActive}" data-tooltip="${m.tooltip}">
              <i class="bi ${m.icon} menu-muc-icon"></i>
              <span class="menu-muc-nhan">${m.nhan}</span>
            </a>
          `;
        });
        html += `</div>`;
      });
      navEl.innerHTML = html;
    }

    // 2. Render User Profile Chip ở Sidebar Footer
    const footerEl = document.querySelector('.sidebar-footer');
    if (footerEl) {
      const tenHienThi = user.hoTen || user.tenDangNhap || 'Người dùng';
      const tenVaiTro = TEN_VAI_TRO[role] || role;
      const mauBadge = MAU_BADGE_VAI_TRO[role] || 'badge-xanh';

      footerEl.innerHTML = `
        <div class="sidebar-user-card" style="display: flex; align-items: center; justify-content: space-between; padding: 0.75rem var(--kc-m); border-top: 1px solid rgba(255,255,255,0.08);">
          <a href="thong-tin-ca-nhan.html" style="display: flex; align-items: center; gap: 0.65rem; overflow: hidden; text-decoration: none; cursor: pointer;" title="Xem và sửa thông tin cá nhân">
            <div style="width: 36px; height: 36px; border-radius: 50%; background: linear-gradient(135deg, #4f46e5, #818cf8); color: #ffffff; display: flex; align-items: center; justify-content: center; font-weight: 700; flex-shrink: 0; box-shadow: 0 2px 8px rgba(79, 70, 229, 0.4);">
              ${tenHienThi.charAt(0).toUpperCase()}
            </div>
            <div style="overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
              <div style="font-weight: 600; font-size: 0.825rem; color: #ffffff; overflow: hidden; text-overflow: ellipsis; display: flex; align-items: center; gap: 4px;">
                ${tenHienThi} <i class="bi bi-pencil-square" style="font-size: 0.72rem; opacity: 0.7;"></i>
              </div>
              <span class="badge ${mauBadge}" style="font-size: 0.68rem; padding: 2px 6px;">${tenVaiTro}</span>
            </div>
          </a>
          <button onclick="XacThuc.dangXuat()" class="nut nut-phu nut-nho" title="Đăng xuất" style="padding: 5px 8px; border-radius: 6px; background: rgba(255,255,255,0.1); border: none; color: #fff;">
            <i class="bi bi-box-arrow-right"></i>
          </button>
        </div>
      `;
    }

    // 3. Sự kiện mở/đóng Sidebar & lưu trạng thái
    const nutToggle = document.getElementById('nut-thu-gon-sidebar');
    const sidebar = document.getElementById('sidebar');
    const vungChinh = document.getElementById('vung-chinh') || document.getElementById('noi-dung-chinh');
    const overlay = document.getElementById('overlay-sidebar');

    function capNhatIconToggle(isThuGon) {
      if (!nutToggle) return;
      const icon = nutToggle.querySelector('i');
      if (icon) {
        icon.className = isThuGon ? 'bi bi-chevron-right' : 'bi bi-chevron-left';
      }
      nutToggle.title = isThuGon ? 'Mở rộng thanh điều hướng' : 'Thu gọn thanh điều hướng';
    }

    // Khôi phục trạng thái thu gọn từ lần truy cập trước (nếu có)
    const daThuGon = localStorage.getItem('qlbc_sidebar_thugon') === 'true';
    if (sidebar) {
      if (daThuGon) {
        sidebar.classList.add('thu-gon');
        if (vungChinh) vungChinh.classList.add('mo-rong');
        capNhatIconToggle(true);
      } else {
        sidebar.classList.remove('thu-gon');
        if (vungChinh) vungChinh.classList.remove('mo-rong');
        capNhatIconToggle(false);
      }
    }

    if (nutToggle && sidebar) {
      nutToggle.onclick = function (e) {
        e.preventDefault();
        const isCollapsed = sidebar.classList.toggle('thu-gon');
        if (vungChinh) vungChinh.classList.toggle('mo-rong', isCollapsed);
        capNhatIconToggle(isCollapsed);
        try {
          localStorage.setItem('qlbc_sidebar_thugon', isCollapsed ? 'true' : 'false');
        } catch (err) {}
      };
    }

    if (overlay && sidebar) {
      overlay.onclick = function () {
        sidebar.classList.remove('mo-mobile');
        overlay.classList.remove('hien');
      };
    }
  }
};

// Tự động khởi tạo sau khi DOM sẵn sàng
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => {
    DieuHuong.khoiTao();
  });
} else {
  DieuHuong.khoiTao();
}
