/**
 * tien-ich.js — Bộ công cụ tiện ích nâng cao:
 * 1. Nhập / Xuất Excel & CSV chuẩn UTF-8 tiếng Việt
 * 2. Xuất bản in & Lưu PDF (Hóa đơn, Phiếu phát hành, Báo cáo)
 * 3. In-App Toast Notifications (Thông báo nổi thời gian thực)
 */

const TienIch = {
  // Hàm bổ trợ chuẩn hóa mapping cột (hỗ trợ cả dạng Mảng [ {field, title} ] và dạng Đối tượng { 'Tiêu đề': 'field' })
  _chuanHoaMapping: function (mapping) {
    const headers = [];
    const keys = [];

    if (Array.isArray(mapping)) {
      mapping.forEach(col => {
        if (typeof col === 'string') {
          headers.push(col);
          keys.push(col);
        } else if (col && typeof col === 'object') {
          headers.push(col.title || col.label || col.header || (typeof col.field === 'string' ? col.field : '') || '');
          keys.push(col.field !== undefined ? col.field : (col.key || col.prop || col.value || ''));
        }
      });
    } else if (mapping && typeof mapping === 'object') {
      Object.keys(mapping).forEach(k => {
        headers.push(k);
        keys.push(mapping[k]);
      });
    }

    return { headers, keys };
  },

  // Hàm bổ trợ lấy giá trị an toàn theo key hoặc hàm
  _layGiaTriCot: function (item, key) {
    if (typeof key === 'function') {
      return key(item);
    }
    if (!key || typeof key !== 'string') {
      return '';
    }
    let val = '';
    if (key.indexOf('.') !== -1) {
      val = key.split('.').reduce((acc, part) => (acc !== undefined && acc !== null ? acc[part] : ''), item);
    } else {
      val = item[key];
      // Tự động fallback các trường tương đương phổ biến
      if ((val === undefined || val === null || val === '') && (key === 'vaiTro' || key === 'quyenHan')) {
        val = item['quyenHan'] || item['vaiTro'] || item['role'] || '';
      }
    }
    if (val === undefined || val === null) {
      return '';
    }
    if (typeof val === 'boolean') {
      return val ? 'Có' : 'Không';
    }
    // Nếu là đối tượng Date
    if (val instanceof Date) {
      const nam = val.getFullYear();
      const thang = ('0' + (val.getMonth() + 1)).slice(-2);
      const ngay = ('0' + val.getDate()).slice(-2);
      return `${nam}-${thang}-${ngay}`;
    }
    // Nếu là chuỗi ngày tháng ISO (e.g. 2026-11-07T00:00:00 hoặc 2026-11-07 00:00:00)
    if (typeof val === 'string' && /^\d{4}-\d{2}-\d{2}[T\s]/.test(val)) {
      return val.substring(0, 10);
    }
    return val;
  },

  // Hàm bổ trợ chuẩn hóa tên file tránh bị trùng đuôi .csv.csv hoặc .xls.xls
  _chuanHoaTenFile: function (filename, extension) {
    let clean = (filename || 'xuat-du-lieu').trim();
    clean = clean.replace(/\.(csv|xls|xlsx)$/i, '');
    return clean + '.' + extension;
  },

  /**
   * Xuất danh sách đối tượng ra file CSV (chuẩn UTF-8 có BOM)
   * Sử dụng dấu chấm phẩy (;) làm phân cách mặc định để Microsoft Excel trên Windows (khu vực Việt Nam)
   * tự động chia tách từng cột riêng biệt khi mở trực tiếp, không bị dồn tất cả dữ liệu vào 1 cột A.
   * @param {Array} data Danh sách đối tượng
   * @param {Array|Object} mapping Ánh xạ [ { field, title } ] hoặc { 'Tiêu đề': 'field' }
   * @param {String} filename Tên file xuất
   * @param {String} delimiter Dấu phân cách cột (mặc định ';')
   */
  xuatCSV: function (data, mapping, filename, delimiter = ';') {
    if (!data || !data.length) {
      TienIch.thongBao('Không có dữ liệu để xuất file!', 'canh-bao');
      return;
    }

    const { headers, keys } = TienIch._chuanHoaMapping(mapping);

    let csvContent = '\uFEFF'; // UTF-8 BOM hiển thị chuẩn tiếng Việt có dấu trong Excel
    csvContent += headers.map(h => `"${String(h).replace(/"/g, '""')}"`).join(delimiter) + '\r\n';

    data.forEach(item => {
      const row = keys.map(key => {
        let val = TienIch._layGiaTriCot(item, key);
        // Nếu là ngày tháng YYYY-MM-DD, xuất dạng ="YYYY-MM-DD" để Excel hiển thị rõ chữ, không bị lỗi '########'
        if (typeof val === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(val)) {
          return `="${val}"`;
        }
        // Nếu là số điện thoại (bắt đầu bằng 0), xuất dạng ="0..." để Excel không bị mất số 0 ở đầu
        if (typeof val === 'string' && /^0\d{8,11}$/.test(val)) {
          return `="${val}"`;
        }
        return `"${String(val).replace(/"/g, '""')}"`;
      });
      csvContent += row.join(delimiter) + '\r\n';
    });

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = TienIch._chuanHoaTenFile(filename, 'csv');
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);

    TienIch.thongBao(`Đã xuất ${data.length} dòng dữ liệu ra file CSV!`, 'thanh-cong');
  },

  /**
   * Xuất danh sách ra định dạng bảng Excel (.xls) chuẩn Unicode tiếng Việt
   * Kẻ bảng viền đen toàn bộ (All Borders) chuẩn Microsoft Excel 100%
   */
  xuatExcel: function (data, mapping, filename) {
    if (!data || !data.length) {
      TienIch.thongBao('Không có dữ liệu để xuất file!', 'canh-bao');
      return;
    }

    const { headers, keys } = TienIch._chuanHoaMapping(mapping);
    const tenHienThi = (filename || 'DANH SÁCH DỮ LIỆU').replace(/\.(csv|xls|xlsx)$/i, '');

    const escapeHtml = function (val) {
      if (val === null || val === undefined) return '';
      return String(val)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
    };

    let html = `
      <html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40">
      <head>
        <meta http-equiv="content-type" content="application/vnd.ms-excel; charset=UTF-8">
        <!--[if gte mso 9]>
        <xml>
          <x:ExcelWorkbook>
            <x:ExcelWorksheets>
              <x:ExcelWorksheet>
                <x:Name>Sheet1</x:Name>
                <x:WorksheetOptions>
                  <x:DisplayGridlines/>
                  <x:Print>
                    <x:ValidPrinterInfo/>
                    <x:PaperSizeIndex>9</x:PaperSizeIndex>
                    <x:HorizontalResolution>600</x:HorizontalResolution>
                    <x:VerticalResolution>600</x:VerticalResolution>
                  </x:Print>
                </x:WorksheetOptions>
              </x:ExcelWorksheet>
            </x:ExcelWorksheets>
          </x:ExcelWorkbook>
        </xml>
        <![endif]-->
        <style>
          table {
            border-collapse: collapse;
            border-spacing: 0;
            width: 100%;
            font-family: Arial, 'Segoe UI', Tahoma, sans-serif;
            font-size: 11pt;
            border: .5pt solid #000000;
          }
          th {
            border: .5pt solid #000000;
            background-color: #ffffff;
            color: #000000;
            font-weight: bold;
            font-size: 11pt;
            text-align: center;
            vertical-align: middle;
            padding: 6px 10px;
            white-space: nowrap;
            mso-number-format: "\\@";
          }
          td {
            border: .5pt solid #000000;
            padding: 5px 8px;
            font-size: 11pt;
            color: #000000;
            vertical-align: middle;
          }
          .so {
            mso-number-format: "\\#\\,\\#\\#0";
            text-align: right;
          }
          .ma {
            mso-number-format: "\\@";
            text-align: center;
          }
          .chuoi {
            mso-number-format: "\\@";
            text-align: left;
          }
          .ngay {
            mso-number-format: "yyyy\\-mm\\-dd";
            text-align: center;
          }
          .sdt {
            mso-number-format: "\\@";
            text-align: center;
          }
        </style>
      </head>
      <body>
        <h3 style="font-family: Arial, 'Segoe UI', sans-serif; font-size: 13pt; font-weight: bold; color: #000000; margin: 0 0 4px 0;">${escapeHtml(tenHienThi)}</h3>
        <p style="font-family: Arial, 'Segoe UI', sans-serif; font-size: 10pt; color: #555555; margin: 0 0 12px 0;">Thời gian xuất: ${new Date().toLocaleString('vi-VN')}</p>
        <table border="1" cellpadding="6" cellspacing="0" style="border-collapse: collapse; border: .5pt solid #000000; font-family: Arial, sans-serif; font-size: 11pt; width: 100%;">
          <thead>
            <tr>
              ${headers.map(h => `<th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000; font-weight: bold; text-align: center; padding: 6px 10px; font-size: 11pt; vertical-align: middle;">${escapeHtml(h)}</th>`).join('')}
            </tr>
          </thead>
          <tbody>
    `;

    data.forEach(item => {
      html += '<tr>';
      keys.forEach((key, idx) => {
        const val = TienIch._layGiaTriCot(item, key);
        const header = String(headers[idx] || '').toLowerCase();
        const keyStr = String(typeof key === 'string' ? key : '').toLowerCase();

        // 1. Ô rỗng hoặc null
        if (val === null || val === undefined || val === '') {
          html += `<td style="border: .5pt solid #000000; text-align: left; mso-number-format: '\\@'; padding: 5px 8px;"></td>`;
        }
        // 2. Ngày tháng (YYYY-MM-DD)
        else if (typeof val === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(val)) {
          html += `<td style="border: .5pt solid #000000; text-align: center; mso-number-format: 'yyyy\\-mm\\-dd'; padding: 5px 8px;">${escapeHtml(val)}</td>`;
        }
        // 3. Số điện thoại (giữ số 0 đầu)
        else if (keyStr.includes('dienthoai') || keyStr.includes('sdt') || keyStr.includes('phone') || header.includes('điện thoại') || header.includes('sđt') || (typeof val === 'string' && /^0\d{8,11}$/.test(val))) {
          html += `<td style="border: .5pt solid #000000; text-align: center; mso-number-format: '\\@'; padding: 5px 8px;">${escapeHtml(val)}</td>`;
        }
        // 4. Mã / ID / STT (Không bắt nhầm các từ có đuôi ma)
        else if (keyStr === 'id' || keyStr === 'stt' || (keyStr.startsWith('ma') && !keyStr.includes('matkhau')) || header === 'stt' || header === 'id' || header.startsWith('mã')) {
          html += `<td style="border: .5pt solid #000000; text-align: center; mso-number-format: '\\@'; padding: 5px 8px;">${escapeHtml(val)}</td>`;
        }
        // 5. Nếu là số thực sự (number)
        else if (typeof val === 'number') {
          html += `<td style="border: .5pt solid #000000; text-align: right; mso-number-format: '\\#\\,\\#\\#0'; padding: 5px 8px;">${val}</td>`;
        }
        // 6. Chuỗi số tiền / đơn giá (chỉ khi giá trị là chuỗi số và tên trường thực sự là giá/tiền, TUYỆT ĐỐI KHÔNG bắt nhầm tên độc giả, ấn phẩm)
        else if (
          typeof val === 'string' &&
          /^-?\d+(\.\d+)?$/.test(val.trim()) &&
          (keyStr === 'gia' || keyStr === 'dongia' || keyStr === 'thanhtien' || keyStr === 'tongtien' || keyStr === 'giamgia' || keyStr.endsWith('tien') || keyStr.endsWith('dongia') || header.includes('tiền') || header.includes('đơn giá') || header.includes('thành tiền')) &&
          !keyStr.includes('docgia') &&
          !keyStr.includes('ten') &&
          !header.includes('độc giả')
        ) {
          const num = parseFloat(val) || 0;
          html += `<td style="border: .5pt solid #000000; text-align: right; mso-number-format: '\\#\\,\\#\\#0'; padding: 5px 8px;">${num}</td>`;
        }
        // 7. Văn bản chuỗi thông thường (Tên độc giả, tên ấn phẩm, email, vai trò, trạng thái, địa chỉ...)
        else {
          html += `<td style="border: .5pt solid #000000; text-align: left; mso-number-format: '\\@'; padding: 5px 8px;">${escapeHtml(val)}</td>`;
        }
      });
      html += '</tr>';
    });

    html += `
          </tbody>
        </table>
      </body>
      </html>
    `;

    const blob = new Blob(['\uFEFF' + html], { type: 'application/vnd.ms-excel;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = TienIch._chuanHoaTenFile(filename, 'xls');
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);

    TienIch.thongBao(`Đã xuất ${data.length} bản ghi ra file Excel (.xls) có kẻ bảng đầy đủ!`, 'thanh-cong');
  },

  /**
   * Đọc và phân tích file Excel (.xlsx, .xls) hoặc CSV tải lên từ người dùng
   * Hỗ trợ file XLSX chuẩn, file XLS nhị phân và file XLS dạng HTML table
   * @param {File} file Đối tượng File từ input
   * @param {Function} callback function(err, items)
   */
  docFileExcel: function (file, callback) {
    if (!file) return;

    // 1. Nếu có thư viện SheetJS XLSX
    if (typeof XLSX !== 'undefined') {
      const reader = new FileReader();
      reader.onload = function (e) {
        try {
          const data = new Uint8Array(e.target.result);
          const workbook = XLSX.read(data, { type: 'array' });
          if (!workbook.SheetNames || !workbook.SheetNames.length) {
            if (callback) callback('File Excel không có sheet nào!', []);
            return;
          }
          const firstSheet = workbook.Sheets[workbook.SheetNames[0]];
          const json = XLSX.utils.sheet_to_json(firstSheet, { defval: '' });
          if (!json || !json.length) {
            // Thử đọc theo định dạng HTML table (phòng trường hợp file .xls dạng HTML)
            TienIch._docFileExcelHTML(file, callback);
            return;
          }
          if (callback) callback(null, json);
        } catch (err) {
          console.warn('SheetJS không đọc được file này, thử đọc dạng HTML table...', err);
          TienIch._docFileExcelHTML(file, callback);
        }
      };
      reader.onerror = function (err) {
        if (callback) callback('Lỗi khi đọc file: ' + (err.message || 'Không thể mở file'), []);
      };
      reader.readAsArrayBuffer(file);
    } else {
      // 2. Fallback nếu chưa nạp SheetJS: đọc dạng HTML table hoặc CSV
      TienIch._docFileExcelHTML(file, callback);
    }
  },

  /**
   * Hàm hỗ trợ đọc file .xls xuất từ dạng HTML table
   */
  _docFileExcelHTML: function (file, callback) {
    const reader = new FileReader();
    reader.onload = function (e) {
      try {
        const text = e.target.result || '';
        if (text.indexOf('<table') !== -1 && text.indexOf('</table>') !== -1) {
          const parser = new DOMParser();
          const doc = parser.parseFromString(text, 'text/html');
          const table = doc.querySelector('table');
          if (!table) {
            if (callback) callback('Không tìm thấy bảng dữ liệu trong file Excel!', []);
            return;
          }
          const rows = Array.from(table.querySelectorAll('tr'));
          if (rows.length < 2) {
            if (callback) callback('Bảng Excel không có đủ dữ liệu!', []);
            return;
          }
          const headerCells = Array.from(rows[0].querySelectorAll('th, td'));
          const headers = headerCells.map(c => (c.textContent || '').trim());
          const result = [];
          for (let i = 1; i < rows.length; i++) {
            const cells = Array.from(rows[i].querySelectorAll('td'));
            if (!cells.length) continue;
            const obj = {};
            cells.forEach((cell, idx) => {
              const h = headers[idx] || ('cot_' + idx);
              obj[h] = (cell.textContent || '').trim();
            });
            result.push(obj);
          }
          if (callback) callback(null, result);
        } else {
          // Fallback sang CSV
          TienIch.docFileCSV(file, callback);
        }
      } catch (err) {
        if (callback) callback('Lỗi phân tích file Excel: ' + (err.message || err), []);
      }
    };
    reader.onerror = function (err) {
      if (callback) callback('Lỗi khi đọc file: ' + (err.message || 'Không thể mở file'), []);
    };
    reader.readAsText(file, 'UTF-8');
  },

  /**
   * Đọc và phân tích file CSV tải lên từ người dùng (hỗ trợ cả dấu ',' và ';')
   */
  docFileCSV: function (file, callback) {
    if (!file) return;
    const reader = new FileReader();
    reader.onload = function (e) {
      try {
        const text = e.target.result || '';
        const lines = text.split(/\r\n|\n/).filter(line => line.trim().length > 0);
        if (lines.length < 2) {
          TienIch.thongBao('File rỗng hoặc chỉ có dòng tiêu đề!', 'canh-bao');
          if (callback) {
            if (callback.length >= 2) callback('File rỗng hoặc chỉ có dòng tiêu đề!', []);
            else callback([]);
          }
          return;
        }

        // Tự động nhận diện dấu phân cách ',' hoặc ';'
        const delimiter = lines[0].indexOf(';') !== -1 ? ';' : ',';

        // Tách tiêu đề
        const headers = lines[0].split(delimiter).map(h => h.trim().replace(/^"|"$/g, ''));
        const result = [];

        for (let i = 1; i < lines.length; i++) {
          const cells = lines[i].split(delimiter).map(c => c.trim().replace(/^"|"$/g, ''));
          const rowObj = {};
          headers.forEach((h, idx) => {
            rowObj[h] = cells[idx] || '';
          });
          result.push(rowObj);
        }

        if (callback) {
          if (callback.length >= 2) callback(null, result);
          else callback(result);
        }
      } catch (err) {
        console.error(err);
        TienIch.thongBao('Lỗi đọc cấu trúc file!', 'nguy-hiem');
        if (callback) {
          if (callback.length >= 2) callback(err.message || 'Lỗi đọc file', []);
          else callback([]);
        }
      }
    };
    reader.readAsText(file, 'UTF-8');
  },

  /**
   * In ấn và Xuất Hóa Đơn PDF chuẩn quốc gia
   */
  inHoaDonPDF: function (hd) {
    if (!hd) return;
    const printWindow = window.open('', '_blank', 'width=850,height=950');
    if (!printWindow) {
      alert('Vui lòng cho phép mở cửa sổ popup để in hóa đơn PDF!');
      return;
    }

    const tienVND = (num) => Number(num || 0).toLocaleString('vi-VN') + ' ₫';

    printWindow.document.write(`
      <!DOCTYPE html>
      <html lang="vi">
      <head>
        <meta charset="UTF-8">
        <title>Hóa Đơn Thu Tiền Báo - #HD-${hd.maHoaDon}</title>
        <style>
          body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 30px; color: #1e293b; }
          .header { display: flex; justify-content: space-between; border-bottom: 2px solid #4f46e5; padding-bottom: 20px; }
          .logo { font-size: 22px; font-weight: 800; color: #4f46e5; }
          .title { text-align: center; margin: 25px 0 15px 0; font-size: 24px; font-weight: 800; text-transform: uppercase; color: #0f172a; }
          .sub { text-align: center; color: #64748b; font-size: 14px; margin-bottom: 25px; }
          .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 25px; background: #f8fafc; padding: 15px; border-radius: 8px; border: 1px solid #e2e8f0; }
          table { width: 100%; border-collapse: collapse; margin-bottom: 30px; }
          th { background: #4f46e5; color: white; padding: 12px; font-size: 14px; border: 1px solid #4338ca; text-align: left; }
          td { padding: 12px; border: 1px solid #e2e8f0; font-size: 14px; }
          .total-row { font-weight: 700; font-size: 16px; background: #f1f5f9; }
          .signatures { display: flex; justify-content: space-between; margin-top: 40px; text-align: center; }
          .btn-in { position: fixed; top: 15px; right: 15px; background: #4f46e5; color: #fff; padding: 10px 20px; border-radius: 6px; border: none; cursor: pointer; font-weight: 600; font-size: 14px; }
          @media print {
            .btn-in { display: none; }
            body { margin: 0; }
          }
        </style>
      </head>
      <body>
        <button class="btn-in" onclick="window.print()">🖨️ In / Lưu file PDF</button>

        <div class="header">
          <div>
            <div class="logo">TÒA SOẠN & CÔNG TY PHÁT HÀNH BÁO CHÍ</div>
            <div style="font-size: 13px; color: #64748b; margin-top: 4px;">Địa chỉ: Thôn Phương Mạn Đường, xã Châu Ninh, tỉnh Hưng Yên</div>
            <div style="font-size: 13px; color: #64748b;">Hotline: (024) 3825 6789 - Email: toasoan@baochi.vn</div>
          </div>
          <div style="text-align: right;">
            <div style="font-weight: 700; font-size: 16px;">MÃ HÓA ĐƠN: #HD-${hd.maHoaDon}</div>
            <div style="font-size: 13px; color: #64748b; margin-top: 4px;">Ngày lập: ${hd.ngayLap || new Date().toISOString().split('T')[0]}</div>
            <div style="font-size: 13px; color: ${hd.trangThai === 'Đã thanh toán' ? '#059669' : '#d97706'}; font-weight: 700; margin-top: 4px;">Trạng thái: ${hd.trangThai || 'Chưa thanh toán'}</div>
          </div>
        </div>

        <div class="title">HÓA ĐƠN GIÁ TRỊ GIA TĂNG (ĐẶT BÁO ĐỊNH KỲ)</div>
        <div class="sub">Hợp đồng thuê bao phát hành báo & tạp chí</div>

        <div class="info-grid">
          <div>
            <strong>Độc giả thanh toán:</strong> ${hd.tenDocGia || 'Độc giả'}<br>
            <strong>Mã đăng ký thuê bao:</strong> #ĐK-${hd.maDangKy}<br>
          </div>
          <div>
            <strong>Phương thức:</strong> ${hd.phuongThuc || 'Chuyển khoản trực tuyến / QR'}<br>
            <strong>Hình thức nhận:</strong> Phát hành và giao báo tận nơi
          </div>
        </div>

        <table>
          <thead>
            <tr>
              <th>STT</th>
              <th>Tên Ấn Phẩm Báo Chí</th>
              <th>Kỳ Hạn Thuê Bao</th>
              <th>Đơn Giá / Kỳ</th>
              <th>Tổng Thành Tiền</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td style="text-align: center;">1</td>
              <td><strong>${hd.tenAnPham || 'Ấn phẩm Báo chí & Tạp chí định kỳ'}</strong></td>
              <td>Theo hợp đồng đăng ký</td>
              <td style="text-align: right;">${tienVND(hd.tongTien)}</td>
              <td style="text-align: right; font-weight: 700; color: #4f46e5;">${tienVND(hd.thanhTien || hd.tongTien)}</td>
            </tr>
            <tr class="total-row">
              <td colspan="4" style="text-align: right;">TỔNG TIỀN THANH TOÁN (ĐÃ GỒM VAT):</td>
              <td style="text-align: right; color: #059669;">${tienVND(hd.thanhTien || hd.tongTien)}</td>
            </tr>
          </tbody>
        </table>

        <div class="signatures">
          <div>
            <strong>NGƯỜI ĐẶT BÁO</strong><br>
            <span style="font-size: 12px; color: #64748b;">(Ký, ghi rõ họ tên)</span><br><br><br><br>
            <strong>${hd.tenDocGia || ''}</strong>
          </div>
          <div>
            <strong>THỦ QUỸ / ĐẠI DIỆN TÒA SOẠN</strong><br>
            <span style="font-size: 12px; color: #64748b;">(Ký tên và đóng dấu)</span><br><br><br><br>
            <strong>PHÒNG PHÁT HÀNH BÁO CHÍ</strong>
          </div>
        </div>
      </body>
      </html>
    `);
    printWindow.document.close();
  },

  /**
   * Xuất Phiếu phát hành báo PDF
   */
  inPhieuPhatHanhPDF: function (ph) {
    if (!ph) return;
    const printWindow = window.open('', '_blank', 'width=850,height=900');
    if (!printWindow) {
      alert('Vui lòng cho phép popup để in phiếu!');
      return;
    }

    printWindow.document.write(`
      <!DOCTYPE html>
      <html lang="vi">
      <head>
        <meta charset="UTF-8">
        <title>Phiếu Phát Hành Báo - #PH-${ph.maPhatHanh}</title>
        <style>
          body { font-family: Arial, sans-serif; margin: 30px; color: #1e293b; }
          .header { border-bottom: 2px solid #059669; padding-bottom: 15px; margin-bottom: 20px; }
          .title { text-align: center; font-size: 22px; font-weight: 800; color: #059669; margin: 20px 0; }
          .box { border: 1px solid #cbd5e1; padding: 20px; border-radius: 8px; margin-bottom: 25px; background: #f8fafc; }
          .row { display: flex; justify-content: space-between; margin-bottom: 10px; font-size: 15px; }
          .btn-in { position: fixed; top: 15px; right: 15px; background: #059669; color: #fff; padding: 10px 20px; border-radius: 6px; border: none; cursor: pointer; font-weight: 600; }
          @media print { .btn-in { display: none; } body { margin: 0; } }
        </style>
      </head>
      <body>
        <button class="btn-in" onclick="window.print()">🖨️ In / Lưu PDF</button>
        <div class="header">
          <h2>BAN BIÊN TẬP & PHÒNG PHÁT HÀNH BÁO CHÍ</h2>
          <p>Hệ thống Quản lý Vận chuyển & Giao nhận Báo tận nơi</p>
        </div>
        <div class="title">PHIẾU ĐIỀU PHỐI PHÁT HÀNH BÁO</div>
        <div class="box">
          <div class="row"><span>Mã Lệnh Phát Hành:</span><strong>#PH-${ph.maPhatHanh}</strong></div>
          <div class="row"><span>Mã Thuê Bao Đăng Ký:</span><strong>#ĐK-${ph.maDangKy}</strong></div>
          <div class="row"><span>Người Nhận Báo:</span><strong>${ph.tenDocGia || 'Độc giả'}</strong></div>
          <div class="row"><span>Ấn Phẩm & Số Kỳ:</span><strong style="color: #4f46e5;">${ph.tenAnPham || 'Báo ngày'} - Số #${ph.soThuTuBao || ph.maSoBao}</strong></div>
          <div class="row"><span>Ngày Phát Hành:</span><strong>${ph.ngayPhatHanh}</strong></div>
          <div class="row"><span>Số Lượng Giao:</span><strong>${ph.soLuong || 1} bản</strong></div>
          <div class="row"><span>Hình Thức Giao Nhận:</span><strong>${ph.hinhThuc || 'Giao báo tận nơi'}</strong></div>
          <div class="row"><span>Địa Chỉ Nhận:</span><strong style="color: #dc2626;">${ph.diaChiGiao || 'Địa chỉ đăng ký'}</strong></div>
          <div class="row"><span>Trạng Thái Hiện Tại:</span><strong>${ph.trangThai}</strong></div>
        </div>
        <div style="display: flex; justify-content: space-between; margin-top: 50px; text-align: center;">
          <div><strong>NHÂN VIÊN GIAO BÁO</strong><br><br><br><br>..................................................</div>
          <div><strong>NGƯỜI KÝ NHẬN BÁO</strong><br><br><br><br>..................................................</div>
        </div>
      </body>
      </html>
    `);
    printWindow.document.close();
  },

  /**
   * In & Xuất Báo Cáo Doanh Thu ra PDF chuẩn tài chính kế toán
   * @param {Array} dsThang Danh sách thống kê doanh thu theo tháng [{ thang, nam, doanhThu, soHoaDon }]
   * @param {Number|String} nam Năm báo cáo (hoặc 0/'0' cho toàn thời gian)
   * @param {Array} topAnPham Top ấn phẩm đóng góp doanh thu
   */
  inBaoCaoDoanhThuPDF: function (dsThang, nam, topAnPham) {
    dsThang = Array.isArray(dsThang) ? dsThang : [];
    topAnPham = Array.isArray(topAnPham) ? topAnPham : [];
    const laToanThoiGian = (!nam || nam === 0 || nam === '0' || nam === 'all');
    const tieuDeKy = laToanThoiGian ? 'Toàn Thời Gian' : ('Năm ' + nam);
    const maSoBC = laToanThoiGian ? 'BC-DT/ALL' : ('BC-DT/' + nam);

    const tienVND = (num) => Number(num || 0).toLocaleString('vi-VN') + ' ₫';
    let tongDoanhThu = 0;
    let tongHoaDon = 0;

    for (let i = 0; i < dsThang.length; i++) {
      tongDoanhThu += Number(dsThang[i].doanhThu || 0);
      tongHoaDon += Number(dsThang[i].soHoaDon || 0);
    }

    const printWindow = window.open('', '_blank', 'width=950,height=950');
    if (!printWindow) {
      alert('Vui lòng cho phép mở popup trình duyệt để xem và in Báo Cáo Doanh Thu PDF!');
      return;
    }

    let rowsThangHtml = '';
    if (!dsThang.length) {
      rowsThangHtml = '<tr><td colspan="6" style="text-align: center; padding: 15px; color: #64748b;">Chưa có dữ liệu doanh thu ' + (laToanThoiGian ? 'toàn thời gian' : ('trong năm ' + nam)) + '</td></tr>';
    } else {
      for (let i = 0; i < dsThang.length; i++) {
        const item = dsThang[i];
        const dt = Number(item.doanhThu || 0);
        const tyTrong = tongDoanhThu > 0 ? ((dt / tongDoanhThu) * 100).toFixed(1) + '%' : '0%';
        const kyThangHienThi = laToanThoiGian ? (`Tháng ${item.thang}/${item.nam || ''}`) : (`Tháng ${item.thang}/${nam}`);
        rowsThangHtml += `
          <tr>
            <td style="text-align: center;">${i + 1}</td>
            <td style="font-weight: 600;">${kyThangHienThi}</td>
            <td style="text-align: center;">${Number(item.soHoaDon || 0).toLocaleString('vi-VN')}</td>
            <td style="text-align: right; font-weight: 600; color: #0284c7;">${tienVND(dt)}</td>
            <td style="text-align: center;">${tyTrong}</td>
            <td style="font-size: 13px; color: #64748b; text-align: center;">Đã thu đủ</td>
          </tr>
        `;
      }
    }

    let rowsTopApHtml = '';
    if (topAnPham.length) {
      for (let i = 0; i < topAnPham.length; i++) {
        const ap = topAnPham[i];
        rowsTopApHtml += `
          <tr>
            <td style="text-align: center;">${i + 1}</td>
            <td style="text-align: center;">#${ap.maAnPham}</td>
            <td><strong>${ap.tenAnPham || 'Ấn phẩm'}</strong></td>
            <td style="text-align: center;">${Number(ap.soLuongDangKy || 0).toLocaleString('vi-VN')} lượt</td>
            <td style="text-align: right; font-weight: 700; color: #059669;">${tienVND(ap.tongTien)}</td>
          </tr>
        `;
      }
    }

    printWindow.document.write(`
      <!DOCTYPE html>
      <html lang="vi">
      <head>
        <meta charset="UTF-8">
        <title>Báo Cáo Doanh Thu ${tieuDeKy} - Quản Lý Báo Chí</title>
        <style>
          body { font-family: 'Times New Roman', Times, serif; margin: 35px; color: #1e293b; line-height: 1.5; }
          .header-grid { display: flex; justify-content: space-between; margin-bottom: 25px; border-bottom: 2px solid #334155; padding-bottom: 15px; }
          .title { text-align: center; margin: 25px 0 10px 0; font-size: 24px; font-weight: bold; text-transform: uppercase; color: #0f172a; }
          .sub { text-align: center; font-style: italic; font-size: 14px; margin-bottom: 25px; color: #475569; }
          .kpi-container { display: flex; gap: 15px; margin-bottom: 25px; justify-content: space-between; }
          .kpi-box { flex: 1; border: 1px solid #cbd5e1; background: #f8fafc; padding: 12px; border-radius: 6px; text-align: center; }
          .kpi-label { font-size: 12px; color: #64748b; font-weight: bold; text-transform: uppercase; }
          .kpi-value { font-size: 18px; font-weight: bold; color: #0284c7; margin-top: 4px; }
          table { width: 100%; border-collapse: collapse; margin-bottom: 25px; }
          th { background: #f1f5f9; color: #0f172a; padding: 10px 8px; font-size: 14px; border: 1px solid #94a3b8; text-align: center; }
          td { padding: 9px 8px; border: 1px solid #cbd5e1; font-size: 14px; }
          .total-row { font-weight: bold; background: #f8fafc; }
          .signatures { display: flex; justify-content: space-between; margin-top: 45px; text-align: center; page-break-inside: avoid; }
          .btn-in { position: fixed; top: 15px; right: 15px; background: #4f46e5; color: #fff; padding: 10px 22px; border-radius: 6px; border: none; cursor: pointer; font-weight: 600; font-family: Arial, sans-serif; font-size: 14px; box-shadow: 0 4px 10px rgba(0,0,0,0.15); }
          .btn-in:hover { background: #4338ca; }
          @media print {
            .btn-in { display: none; }
            body { margin: 15mm; }
          }
        </style>
      </head>
      <body>
        <button class="btn-in" onclick="window.print()">🖨️ In / Lưu Báo Cáo PDF</button>

        <div class="header-grid">
          <div>
            <div style="font-weight: bold; font-size: 14px;">TÒA SOẠN & CÔNG TY PHÁT HÀNH BÁO CHÍ</div>
            <div style="font-size: 13px; color: #475569;">Ban Quản Trị Hệ Thống & Phát Hành</div>
            <div style="font-size: 12px; color: #64748b;">Số: ${maSoBC}</div>
          </div>
          <div style="text-align: center;">
            <div style="font-weight: bold; font-size: 14px;">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div>
            <div style="font-size: 13px; font-weight: bold;">Độc lập - Tự do - Hạnh phúc</div>
            <div style="font-size: 12px; font-style: italic; margin-top: 4px;">Hà Nội, ngày ${new Date().getDate()} tháng ${new Date().getMonth() + 1} năm ${new Date().getFullYear()}</div>
          </div>
        </div>

        <div class="title">BÁO CÁO TỔNG HỢP DOANH THU PHÁT HÀNH BÁO CHÍ ${tieuDeKy.toUpperCase()}</div>
        <div class="sub">Kỳ hạch toán tài chính: ${tieuDeKy}</div>

        <div class="kpi-container">
          <div class="kpi-box">
            <div class="kpi-label">Tổng Doanh Thu Thực Thu</div>
            <div class="kpi-value" style="color: #059669;">${tienVND(tongDoanhThu)}</div>
          </div>
          <div class="kpi-box">
            <div class="kpi-label">Tổng Số Hóa Đơn Thu Tiền</div>
            <div class="kpi-value" style="color: #4f46e5;">${tongHoaDon.toLocaleString('vi-VN')} HĐ</div>
          </div>
          <div class="kpi-box">
            <div class="kpi-label">Thời Điểm Lập Báo Cáo</div>
            <div class="kpi-value" style="font-size: 15px; color: #334155; margin-top: 6px;">${new Date().toLocaleDateString('vi-VN')}</div>
          </div>
        </div>

        <h3 style="font-size: 15px; text-transform: uppercase; margin-bottom: 8px;">I. Bảng Tổng Hợp Doanh Thu Theo Tháng</h3>
        <table>
          <thead>
            <tr>
              <th style="width: 50px;">STT</th>
              <th>Kỳ Hạch Toán</th>
              <th style="width: 140px;">Số Lượng Hóa Đơn</th>
              <th style="width: 180px;">Doanh Thu (VNĐ)</th>
              <th style="width: 100px;">Tỷ Trọng</th>
              <th>Ghi Chú</th>
            </tr>
          </thead>
          <tbody>
            ${rowsThangHtml}
            <tr class="total-row">
              <td colspan="2" style="text-align: center; font-weight: bold;">TỔNG CỘNG ${laToanThoiGian ? 'TOÀN BỘ' : 'CẢ NĂM'}:</td>
              <td style="text-align: center; font-weight: bold;">${tongHoaDon.toLocaleString('vi-VN')}</td>
              <td style="text-align: right; font-weight: bold; color: #059669; font-size: 15px;">${tienVND(tongDoanhThu)}</td>
              <td style="text-align: center; font-weight: bold;">100%</td>
              <td style="text-align: center; color: #059669; font-weight: bold;">Hoàn thành</td>
            </tr>
          </tbody>
        </table>

        ${topAnPham.length ? `
          <h3 style="font-size: 15px; text-transform: uppercase; margin-top: 25px; margin-bottom: 8px;">II. Top Ấn Phẩm Báo Chí Đóng Góp Doanh Thu Cao Nhất</h3>
          <table>
            <thead>
              <tr>
                <th style="width: 50px;">STT</th>
                <th style="width: 90px;">Mã AP</th>
                <th>Tên Ấn Phẩm Báo Chí</th>
                <th style="width: 150px;">Lượt Đăng Ký</th>
                <th style="width: 180px;">Tổng Doanh Thu (VNĐ)</th>
              </tr>
            </thead>
            <tbody>
              ${rowsTopApHtml}
            </tbody>
          </table>
        ` : ''}

        <div class="signatures" style="display: flex; justify-content: space-around; margin-top: 50px; text-align: center; page-break-inside: avoid;">
          <div>
            <strong>NGƯỜI LẬP BÁO CÁO</strong><br>
            <span style="font-size: 12px; font-style: italic;">(Ký, ghi rõ họ tên)</span><br><br><br><br><br>
            <strong>Quản Trị Viên Hệ Thống</strong>
          </div>
          <div>
            <strong>TỔNG BIÊN TẬP / BAN GIÁM ĐỐC</strong><br>
            <span style="font-size: 12px; font-style: italic;">(Ký tên và đóng dấu)</span><br><br><br><br><br>
            <strong>BAN LÃNH ĐẠO</strong>
          </div>
        </div>
      </body>
      </html>
    `);
    printWindow.document.close();
  },

  /**
   * Xuất Báo Cáo Doanh Thu tổng hợp ra định dạng bảng Excel (.xls)
   * @param {Array} dsThang Danh sách thống kê doanh thu theo tháng [{ thang, nam, doanhThu, soHoaDon }]
   * @param {Number|String} nam Năm báo cáo (hoặc 0/'0' cho toàn thời gian)
   * @param {Array} topAnPham Top ấn phẩm đóng góp doanh thu
   */
  xuatBaoCaoDoanhThuExcel: function (dsThang, nam, topAnPham) {
    dsThang = Array.isArray(dsThang) ? dsThang : [];
    topAnPham = Array.isArray(topAnPham) ? topAnPham : [];
    const laToanThoiGian = (!nam || nam === 0 || nam === '0' || nam === 'all');
    const tieuDeKy = laToanThoiGian ? 'TOÀN THỜI GIAN' : ('NĂM ' + nam);
    const tenTep = laToanThoiGian ? 'BaoCaoDoanhThu_ToanThoiGian.xls' : (`BaoCaoDoanhThu_Nam_${nam}.xls`);
    const thongBaoMsg = laToanThoiGian 
      ? 'Đã xuất báo cáo doanh thu toàn thời gian ra file Excel (.xls)!' 
      : `Đã xuất báo cáo doanh thu năm ${nam} ra file Excel (.xls)!`;

    let tongDoanhThu = 0;
    let tongHoaDon = 0;
    for (let i = 0; i < dsThang.length; i++) {
      tongDoanhThu += Number(dsThang[i].doanhThu || 0);
      tongHoaDon += Number(dsThang[i].soHoaDon || 0);
    }

    let rowsHtml = '';
    for (let i = 0; i < dsThang.length; i++) {
      const item = dsThang[i];
      const dt = Number(item.doanhThu || 0);
      const tyTrong = tongDoanhThu > 0 ? ((dt / tongDoanhThu) * 100).toFixed(1) + '%' : '0%';
      const kyThangHienThi = laToanThoiGian ? (`Tháng ${item.thang}/${item.nam || ''}`) : (`Tháng ${item.thang}/${nam}`);
      rowsHtml += `
        <tr>
          <td style="border: .5pt solid #000000; text-align: center;">${i + 1}</td>
          <td style="border: .5pt solid #000000;">${kyThangHienThi}</td>
          <td style="border: .5pt solid #000000; text-align: center;">${item.soHoaDon || 0}</td>
          <td class="so" style="border: .5pt solid #000000; text-align: right; font-weight: bold;">${dt}</td>
          <td style="border: .5pt solid #000000; text-align: center;">${tyTrong}</td>
        </tr>
      `;
    }

    let topApRows = '';
    for (let i = 0; i < topAnPham.length; i++) {
      const ap = topAnPham[i];
      topApRows += `
        <tr>
          <td style="border: .5pt solid #000000; text-align: center;">${i + 1}</td>
          <td style="border: .5pt solid #000000; text-align: center;">#${ap.maAnPham}</td>
          <td style="border: .5pt solid #000000;"><strong>${ap.tenAnPham || ''}</strong></td>
          <td style="border: .5pt solid #000000; text-align: center;">${ap.soLuongDangKy || 0}</td>
          <td class="so" style="border: .5pt solid #000000; text-align: right; font-weight: bold;">${Number(ap.tongTien || 0)}</td>
        </tr>
      `;
    }

    let html = `
      <html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40">
      <head>
        <meta http-equiv="content-type" content="application/vnd.ms-excel; charset=UTF-8">
        <style>
          table { border-collapse: collapse; border-spacing: 0; width: 100%; font-family: Arial, 'Segoe UI', sans-serif; font-size: 11pt; margin-bottom: 20px; border: .5pt solid #000000; }
          th { background-color: #ffffff; color: #000000; border: .5pt solid #000000; padding: 6px 10px; font-size: 11pt; font-weight: bold; text-align: center; }
          td { border: .5pt solid #000000; padding: 5px 8px; font-size: 11pt; color: #000000; }
          .so { mso-number-format:"\\#\\,\\#\\#0"; text-align: right; }
          .tong-cong { background-color: #ffffff; font-weight: bold; }
        </style>
      </head>
      <body>
        <h2 style="font-family: Arial, sans-serif; font-size: 14pt; font-weight: bold; color: #000000; margin: 0 0 4px 0;">BÁO CÁO TỔNG HỢP DOANH THU PHÁT HÀNH BÁO CHÍ ${tieuDeKy}</h2>
        <p style="font-family: Arial, sans-serif; font-size: 10pt; color: #555555; margin: 0 0 14px 0;">Thời gian xuất: ${new Date().toLocaleString('vi-VN')} | Đơn vị tiền tệ: VNĐ</p>

        <h3 style="font-family: Arial, sans-serif; font-size: 12pt; font-weight: bold; color: #000000; margin: 12px 0 6px 0;">I. DOANH THU THEO TỪNG THÁNG</h3>
        <table border="1" cellpadding="6" cellspacing="0" style="border-collapse: collapse; border: .5pt solid #000000; font-family: Arial, sans-serif; font-size: 11pt; width: 100%;">
          <thead>
            <tr>
              <th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000;">STT</th>
              <th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000;">Kỳ Tháng</th>
              <th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000;">Số Hóa Đơn</th>
              <th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000;">Doanh Thu (VNĐ)</th>
              <th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000;">Tỷ Trọng</th>
            </tr>
          </thead>
          <tbody>
            ${rowsHtml}
            <tr class="tong-cong" style="font-weight: bold;">
              <td colspan="2" style="border: .5pt solid #000000; text-align: center;">TỔNG CỘNG ${laToanThoiGian ? 'TOÀN BỘ' : 'CẢ NĂM'}:</td>
              <td style="border: .5pt solid #000000; text-align: center;">${tongHoaDon}</td>
              <td class="so" style="border: .5pt solid #000000; text-align: right; color: #000000;">${tongDoanhThu}</td>
              <td style="border: .5pt solid #000000; text-align: center;">100%</td>
            </tr>
          </tbody>
        </table>

        ${topAnPham.length ? `
          <h3 style="font-family: Arial, sans-serif; font-size: 12pt; font-weight: bold; color: #000000; margin: 16px 0 6px 0;">II. TOP ẤN PHẨM BÁO CHÍ BÁN CHẠY NHẤT</h3>
          <table border="1" cellpadding="6" cellspacing="0" style="border-collapse: collapse; border: .5pt solid #000000; font-family: Arial, sans-serif; font-size: 11pt; width: 100%;">
            <thead>
              <tr>
                <th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000;">STT</th>
                <th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000;">Mã AP</th>
                <th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000;">Tên Ấn Phẩm Báo Chí</th>
                <th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000;">Số Lượt Thuê Bao</th>
                <th style="border: .5pt solid #000000; background-color: #ffffff; color: #000000;">Tổng Doanh Thu (VNĐ)</th>
              </tr>
            </thead>
            <tbody>
              ${topApRows}
            </tbody>
          </table>
        ` : ''}
      </body>
      </html>
    `;

    const blob = new Blob(['\uFEFF' + html], { type: 'application/vnd.ms-excel;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = tenTep;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);

    TienIch.thongBao(thongBaoMsg, 'thanh-cong');
  },

  /**
   * In-App Toast Notification thời gian thực
   */
  thongBao: function (thongDiep, loai = 'thong-tin') {
    let container = document.getElementById('toast-container');
    if (!container) {
      container = document.createElement('div');
      container.id = 'toast-container';
      container.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        display: flex;
        flex-direction: column;
        gap: 10px;
        pointer-events: none;
      `;
      document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.style.cssText = `
      min-width: 280px;
      max-width: 420px;
      padding: 12px 18px;
      border-radius: 10px;
      font-size: 14px;
      font-weight: 500;
      color: #ffffff;
      box-shadow: 0 10px 25px -5px rgba(0,0,0,0.25);
      display: flex;
      align-items: center;
      gap: 12px;
      pointer-events: auto;
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      transform: translateX(100%);
      opacity: 0;
    `;

    let icon = 'bi-info-circle-fill';
    let bgColor = '#4f46e5';

    if (loai === 'thanh-cong') {
      bgColor = '#10b981';
      icon = 'bi-check-circle-fill';
    } else if (loai === 'canh-bao') {
      bgColor = '#f59e0b';
      icon = 'bi-exclamation-triangle-fill';
    } else if (loai === 'nguy-hiem') {
      bgColor = '#ef4444';
      icon = 'bi-x-circle-fill';
    }

    toast.style.backgroundColor = bgColor;
    toast.innerHTML = `<i class="bi ${icon}" style="font-size: 18px;"></i> <span>${thongDiep}</span>`;
    container.appendChild(toast);

    requestAnimationFrame(() => {
      toast.style.transform = 'translateX(0)';
      toast.style.opacity = '1';
    });

    setTimeout(() => {
      toast.style.transform = 'translateX(100%)';
      toast.style.opacity = '0';
      setTimeout(() => {
        if (toast.parentNode) {
          toast.parentNode.removeChild(toast);
        }
      }, 300);
    }, 3500);
  }
};
