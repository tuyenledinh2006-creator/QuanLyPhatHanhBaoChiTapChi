/**
 * goi-api.js — Wrapper gọi toàn bộ API qua API Gateway (Ocelot)
 * Hỗ trợ tự động chuẩn hóa/unwrapping dữ liệu phản hồi { success: true, data: [...] }
 */

(function () {
  'use strict';

  angular.module('qlbcApp')
    .factory('GoiApiService', ['$http', function ($http) {

      // Hàm bọc promise để chuẩn hóa res.data:
      // res.data = dữ liệu thực tế (mảng hoặc đối tượng)
      // res.goc = JSON nguyên gốc trả về từ Microservice/Gateway
      function xuLy(promise) {
        return promise.then(function (res) {
          var goc = res.data;
          var duLieu = (goc && goc.data !== undefined) ? goc.data : goc;
          return {
            status: res.status,
            headers: res.headers,
            data: duLieu,
            goc: goc,
            success: (goc && goc.success !== undefined) ? goc.success : true,
            message: (goc && goc.message) ? goc.message : ''
          };
        });
      }

      return {
        // --- 1. XÁC THỰC (API_Login) ---
        dangNhap: function (data) {
          return xuLy($http.post(TIEN_TO_API.login + '/DangNhap', data));
        },
        lamMoiToken: function (refreshToken) {
          return xuLy($http.post(TIEN_TO_API.login + '/LamMoiToken', { refreshToken: refreshToken }));
        },
        dangKy: function (data) {
          return xuLy($http.post(TIEN_TO_API.login + '/DangKy', data));
        },
        dangXuat: function () {
          return xuLy($http.post(TIEN_TO_API.login + '/DangXuat', {}));
        },
        doiMatKhau: function (data) {
          return xuLy($http.post(TIEN_TO_API.login + '/DoiMatKhau', data));
        },
        layThongTinNguoiDung: function (id) {
          return xuLy($http.get(TIEN_TO_API.login + '/LayThongTin/' + id));
        },

        // --- 2. QUẢN TRỊ VIÊN (API_Admin) ---
        layDanhSachNguoiDung: function () {
          return xuLy($http.get(TIEN_TO_API.admin + '/NguoiDung/LayDanhSach'));
        },
        themNguoiDung: function (data) {
          return xuLy($http.post(TIEN_TO_API.admin + '/NguoiDung/ThemMoi', data));
        },
        capNhatNguoiDung: function (data) {
          return xuLy($http.put(TIEN_TO_API.admin + '/NguoiDung/CapNhat?id=' + (data.maNguoiDung || data.id), data));
        },
        phanQuyenNguoiDung: function (maNguoiDung, quyenHan) {
          return xuLy($http.put(TIEN_TO_API.admin + '/NguoiDung/PhanQuyen?id=' + maNguoiDung + '&quyenHan=' + encodeURIComponent(quyenHan), {
            quyenHan: quyenHan
          }));
        },
        xoaNguoiDung: function (id) {
          return xuLy($http.delete(TIEN_TO_API.admin + '/NguoiDung/Xoa?id=' + id));
        },
        layThongKeTongQuan: function () {
          return xuLy($http.get(TIEN_TO_API.admin + '/BaoCao/ThongKeTongQuan'));
        },
        layDoanhThuTheoThang: function (nam) {
          var query = (nam !== undefined && nam !== null && nam !== '') ? ('?nam=' + nam) : '';
          return xuLy($http.get(TIEN_TO_API.admin + '/BaoCao/ThongKeDoanhThuTheoThang' + query));
        },
        layAnPhamBanChay: function (top) {
          return xuLy($http.get(TIEN_TO_API.admin + '/BaoCao/ThongKeAnPhamBanChay' + (top ? '?top=' + top : '')));
        },
        layTatCaHoaDon: function () {
          return xuLy($http.get(TIEN_TO_API.admin + '/HoaDon/LayDanhSach'));
        },
        layDanhSachDocGia: function () {
          return xuLy($http.get(TIEN_TO_API.admin + '/DocGia/LayDanhSach'));
        },
        layDanhSachDanhMuc: function () {
          return xuLy($http.get(TIEN_TO_API.admin + '/DanhMuc/LayDanhSach'));
        },
        themDanhMuc: function (data) {
          return xuLy($http.post(TIEN_TO_API.admin + '/DanhMuc/ThemMoi', data));
        },
        capNhatDanhMuc: function (data) {
          return xuLy($http.put(TIEN_TO_API.admin + '/DanhMuc/CapNhat?id=' + (data.maDanhMuc || 0), data));
        },
        xoaDanhMuc: function (id) {
          return xuLy($http.delete(TIEN_TO_API.admin + '/DanhMuc/Xoa?id=' + id));
        },

        // --- 3. BIÊN TẬP VIÊN (API_BienTap) ---
        layDanhSachAnPham: function () {
          return xuLy($http.get(TIEN_TO_API.bientap + '/AnPham/LayDanhSach'));
        },
        themAnPham: function (data) {
          return xuLy($http.post(TIEN_TO_API.bientap + '/AnPham/ThemMoi', data));
        },
        capNhatAnPham: function (data) {
          return xuLy($http.put(TIEN_TO_API.bientap + '/AnPham/CapNhat?id=' + (data.maAnPham || 0), data));
        },
        xoaAnPham: function (id) {
          return xuLy($http.delete(TIEN_TO_API.bientap + '/AnPham/Xoa?id=' + id));
        },

        layDanhSachSoBao: function () {
          return xuLy($http.get(TIEN_TO_API.bientap + '/SoBao/LayDanhSach'));
        },
        laySoBaoTheoAnPham: function (maAnPham) {
          return xuLy($http.get(TIEN_TO_API.bientap + '/SoBao/LayTheoAnPham?maAnPham=' + maAnPham));
        },
        themSoBao: function (data) {
          return xuLy($http.post(TIEN_TO_API.bientap + '/SoBao/ThemMoi', data));
        },
        capNhatSoBao: function (data) {
          return xuLy($http.put(TIEN_TO_API.bientap + '/SoBao/CapNhat?id=' + (data.maSoBao || 0), data));
        },
        xoaSoBao: function (id) {
          return xuLy($http.delete(TIEN_TO_API.bientap + '/SoBao/Xoa?id=' + id));
        },

        layDanhSachPhatHanh: function () {
          return xuLy($http.get(TIEN_TO_API.bientap + '/PhatHanh/LayDanhSach'));
        },
        themPhatHanh: function (data) {
          return xuLy($http.post(TIEN_TO_API.bientap + '/PhatHanh/TaoLenhPhatHanh', data));
        },
        capNhatTrangThaiPhatHanh: function (id, trangThai) {
          return xuLy($http.put(TIEN_TO_API.bientap + '/PhatHanh/CapNhatTrangThai?id=' + id, { trangThai: trangThai }));
        },
        xoaPhatHanh: function (id) {
          return xuLy($http.delete(TIEN_TO_API.bientap + '/PhatHanh/Xoa?id=' + id));
        },
        guiThongBao: function (data) {
          return xuLy($http.post(TIEN_TO_API.bientap + '/ThongBao/GuiThongBao', data));
        },
        layDanhSachThongBao: function () {
          return xuLy($http.get(TIEN_TO_API.bientap + '/ThongBao/LayDanhSach'));
        },

        // --- 4. ĐỘC GIẢ (API_DocGia) ---
        layDanhSachDangKy: function (maDocGia) {
          var url = maDocGia ? (TIEN_TO_API.docgia + '/DangKy/LayDanhSach?maDocGia=' + maDocGia) : (TIEN_TO_API.docgia + '/DangKy/LayDanhSach');
          return xuLy($http.get(url));
        },
        dangKyDatBao: function (data) {
          return xuLy($http.post(TIEN_TO_API.docgia + '/DangKy/DangKyDatBao', data));
        },
        huyDangKy: function (id) {
          return xuLy($http.delete(TIEN_TO_API.docgia + '/DangKy/HuyDangKy?id=' + id));
        },
        layHoaDonTheoDocGia: function (maDocGia) {
          var url = maDocGia ? (TIEN_TO_API.docgia + '/ThanhToan/LayHoaDonTheoDocGia?maDocGia=' + maDocGia) : (TIEN_TO_API.docgia + '/ThanhToan/LayHoaDonTheoDocGia');
          return xuLy($http.get(url));
        },
        thanhToanOnline: function (data) {
          return xuLy($http.post(TIEN_TO_API.docgia + '/ThanhToan/ThucHienThanhToan', data));
        },
        layLichSuNhanBao: function (maDocGia) {
          var url = maDocGia ? (TIEN_TO_API.docgia + '/CaNhan/LayLichSuNhanBao?maDocGia=' + maDocGia) : (TIEN_TO_API.docgia + '/CaNhan/LayLichSuNhanBao');
          return xuLy($http.get(url));
        },
        layThongBaoCaNhan: function (maDocGia) {
          var url = maDocGia ? (TIEN_TO_API.docgia + '/CaNhan/LayThongBao?maDocGia=' + maDocGia) : (TIEN_TO_API.docgia + '/CaNhan/LayThongBao');
          return xuLy($http.get(url));
        },
        danhDauDaDoc: function (maThongBao) {
          var id = Number(maThongBao) || 0;
          return xuLy($http.put(TIEN_TO_API.docgia + '/CaNhan/DanhDauThongBaoDaDoc?maThongBao=' + id, {})
            .catch(function () {
              return $http.put(TIEN_TO_API.bientap + '/ThongBao/DanhDauDaDoc?id=' + id, {});
            }));
        },
        xoaThongBao: function (maThongBao) {
          var id = Number(maThongBao) || 0;
          return xuLy($http.delete(TIEN_TO_API.docgia + '/CaNhan/XoaThongBao?id=' + id)
            .catch(function () {
              return $http.delete(TIEN_TO_API.bientap + '/ThongBao/Xoa?id=' + id);
            }));
        },
        xoaTatCaThongBaoCaNhan: function () {
          return xuLy($http.delete(TIEN_TO_API.docgia + '/CaNhan/XoaTatCaThongBao'));
        },
        layDanhSachDocGiaChoThongBao: function () {
          return xuLy($http.get(TIEN_TO_API.bientap + '/ThongBao/LayDanhSachDocGia')
            .catch(function () {
              return $http.get(TIEN_TO_API.admin + '/DocGia/LayDanhSach');
            }));
        },
        layThongTinCaNhan: function (maDocGia) {
          var url = maDocGia ? (TIEN_TO_API.docgia + '/CaNhan/LayThongTin?maDocGia=' + maDocGia) : (TIEN_TO_API.docgia + '/CaNhan/LayThongTin');
          return xuLy($http.get(url));
        },
        capNhatThongTinCaNhan: function (data, maDocGia) {
          var url = maDocGia ? (TIEN_TO_API.docgia + '/CaNhan/CapNhatThongTin?maDocGia=' + maDocGia) : (TIEN_TO_API.docgia + '/CaNhan/CapNhatThongTin');
          return xuLy($http.put(url, data));
        },

        // --- 5. AUDIT LOG & VERSIONING (Lịch sử & Phiên bản) ---
        layNhatKyAudit: function (bang, hanhDong, limit) {
          var params = [];
          if (bang) params.push('bang=' + encodeURIComponent(bang));
          if (hanhDong) params.push('hanhDong=' + encodeURIComponent(hanhDong));
          if (limit) params.push('limit=' + limit);
          var q = params.length ? ('?' + params.join('&')) : '';
          return xuLy($http.get(TIEN_TO_API.admin + '/AuditLog/LayDanhSach' + q));
        },
        layLichSuPhienBan: function (bang, khoaChinh) {
          return xuLy($http.get(TIEN_TO_API.admin + '/AuditLog/LayLichSuPhienBan?bang=' + encodeURIComponent(bang) + '&khoaChinh=' + encodeURIComponent(khoaChinh)));
        },

        // --- 6. THÙNG RÁC & KHÔI PHỤC (Soft Delete & Restore) ---
        layThungRacDocGia: function () {
          return xuLy($http.get(TIEN_TO_API.admin + '/DocGia/LayDanhSachThungRac'));
        },
        khoiPhucDocGia: function (id) {
          return xuLy($http.put(TIEN_TO_API.admin + '/DocGia/KhoiPhuc?id=' + id, {}));
        },
        layThungRacHoaDon: function () {
          return xuLy($http.get(TIEN_TO_API.admin + '/HoaDon/LayDanhSachThungRac'));
        },
        khoiPhucHoaDon: function (id) {
          return xuLy($http.put(TIEN_TO_API.admin + '/HoaDon/KhoiPhuc?id=' + id, {}));
        },
        layThungRacDangKy: function () {
          return xuLy($http.get(TIEN_TO_API.docgia + '/DangKy/LayDanhSachThungRac'));
        },
        khoiPhucDangKy: function (id) {
          return xuLy($http.put(TIEN_TO_API.docgia + '/DangKy/KhoiPhuc?id=' + id, {}));
        },
        layThungRacPhatHanh: function () {
          return xuLy($http.get(TIEN_TO_API.bientap + '/PhatHanh/LayDanhSachThungRac'));
        },
        khoiPhucPhatHanh: function (id) {
          return xuLy($http.put(TIEN_TO_API.bientap + '/PhatHanh/KhoiPhuc?id=' + id, {}));
        },
        layThungRacThanhToan: function () {
          return xuLy($http.get(TIEN_TO_API.docgia + '/ThanhToan/LayDanhSachThungRac'));
        },
        khoiPhucThanhToan: function (id) {
          return xuLy($http.put(TIEN_TO_API.docgia + '/ThanhToan/KhoiPhuc?id=' + id, {}));
        }
      };
    }]);
})();
