/**
 * angular-app.js — Khởi tạo ứng dụng AngularJS, Interceptor bảo mật và bộ lọc
 */

(function () {
  'use strict';

  var app = angular.module('qlbcApp', []);

  // Cấu hình HTTP Interceptor tự động chèn JWT Bearer Token và Silent Refresh Token
  app.factory('JwtInterceptor', ['$q', '$injector', function ($q, $injector) {
    var isRefreshing = false;
    var refreshDeferredList = [];

    function logoutAndRedirect() {
      sessionStorage.removeItem(KHOA_PHIEN.nguoi_dung);
      sessionStorage.removeItem(KHOA_PHIEN.token);
      sessionStorage.removeItem(KHOA_PHIEN.refresh_token);
      sessionStorage.removeItem(KHOA_PHIEN.thoi_gian);
      if (!window.location.pathname.endsWith('dang-nhap.html')) {
        window.location.replace('dang-nhap.html');
      }
    }

    return {
      request: function (config) {
        var token = sessionStorage.getItem(KHOA_PHIEN.token);
        if (token && !config.headers.Authorization) {
          config.headers.Authorization = 'Bearer ' + token;
        }
        return config;
      },
      response: function (response) {
        // Tự động gia hạn Sliding Token nếu Gateway hoặc Microservice trả về token mới
        var newToken = response.headers('X-New-Access-Token') || response.headers('x-new-access-token');
        if (newToken) {
          sessionStorage.setItem(KHOA_PHIEN.token, newToken);
          sessionStorage.setItem(KHOA_PHIEN.thoi_gian, Date.now().toString());
        }
        return response;
      },
      responseError: function (rejection) {
        var url = (rejection.config && rejection.config.url) ? rejection.config.url : '';
        var isAuthEndpoint = url.indexOf('/DangNhap') !== -1 || url.indexOf('/LamMoiToken') !== -1;

        // Tự động văng ra màn hình đăng nhập khi bị từ chối quyền (403 Forbidden - ví dụ bị hạ quyền)
        if (rejection.status === 403 && !isAuthEndpoint) {
          alert('Quyền hạn của bạn đã bị thay đổi hoặc không có quyền truy cập. Vui lòng đăng nhập lại!');
          logoutAndRedirect();
          return $q.reject(rejection);
        }

        // Tự động gia hạn token khi gặp mã lỗi 401 Unauthorized
        if (rejection.status === 401 && !isAuthEndpoint) {
          var refreshToken = sessionStorage.getItem(KHOA_PHIEN.refresh_token);

          // Nếu không có Refresh Token, đăng xuất về màn hình đăng nhập
          if (!refreshToken) {
            logoutAndRedirect();
            return $q.reject(rejection);
          }

          var deferred = $q.defer();

          if (!isRefreshing) {
            isRefreshing = true;
            console.log('Access token hết hạn (401). Đang tự động gia hạn token qua Refresh Token...');

            var $http = $injector.get('$http');
            var refreshUrl = TIEN_TO_API.login + '/LamMoiToken';

            $http.post(refreshUrl, { refreshToken: refreshToken })
              .then(function (res) {
                var data = res.data;
                var authData = (data && data.data) ? data.data : data;

                if (authData && (authData.accessToken || authData.token)) {
                  var newAccessToken = authData.accessToken || authData.token;
                  var newRefreshToken = authData.refreshToken || refreshToken;

                  sessionStorage.setItem(KHOA_PHIEN.token, newAccessToken);
                  sessionStorage.setItem(KHOA_PHIEN.refresh_token, newRefreshToken);
                  sessionStorage.setItem(KHOA_PHIEN.thoi_gian, Date.now().toString());
                  console.log('Gia hạn token thành công! Tự động gửi lại request...');

                  // Gửi lại request bị lỗi trước đó với token mới
                  rejection.config.headers.Authorization = 'Bearer ' + newAccessToken;
                  $http(rejection.config).then(
                    function (retryRes) { deferred.resolve(retryRes); },
                    function (retryErr) { deferred.reject(retryErr); }
                  );

                  // Giải phóng các request khác trong hàng đợi
                  for (var i = 0; i < refreshDeferredList.length; i++) {
                    refreshDeferredList[i].resolve(newAccessToken);
                  }
                  refreshDeferredList = [];
                } else {
                  logoutAndRedirect();
                  deferred.reject(rejection);
                }
              })
              .catch(function (err) {
                console.warn('Gia hạn token thất bại. Refresh token hết hạn hoặc không hợp lệ:', err);
                logoutAndRedirect();
                for (var j = 0; j < refreshDeferredList.length; j++) {
                  refreshDeferredList[j].reject(rejection);
                }
                refreshDeferredList = [];
                deferred.reject(rejection);
              })
              .finally(function () {
                isRefreshing = false;
              });

          } else {
            // Đã có một tiến trình gia hạn đang chạy, đưa request này vào hàng đợi
            refreshDeferredList.push({
              resolve: function (newAccessToken) {
                var $http = $injector.get('$http');
                rejection.config.headers.Authorization = 'Bearer ' + newAccessToken;
                $http(rejection.config).then(
                  function (retryRes) { deferred.resolve(retryRes); },
                  function (retryErr) { deferred.reject(retryErr); }
                );
              },
              reject: function () {
                deferred.reject(rejection);
              }
            });
          }

          return deferred.promise;
        }

        return $q.reject(rejection);
      }
    };
  }]);

  app.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push('JwtInterceptor');
  }]);

  app.run(['$rootScope', function ($rootScope) {
    // Khởi tạo điều hướng thanh bên nếu DieuHuong đã sẵn sàng
    if (typeof DieuHuong !== 'undefined' && typeof DieuHuong.khoiTao === 'function') {
      DieuHuong.khoiTao();
    }
  }]);

  // Bộ lọc định dạng tiền tệ VND
  app.filter('tienVND', function () {
    return function (giaTri) {
      if (giaTri === null || giaTri === undefined || isNaN(giaTri)) return '0 ₫';
      return Number(giaTri).toLocaleString('vi-VN') + ' ₫';
    };
  });

  // Bộ lọc định dạng ngày tháng yyyy-MM-dd -> dd/MM/yyyy
  app.filter('dinhDangNgay', function () {
    return function (chuoiNgay) {
      if (!chuoiNgay) return '';
      try {
        var d = new Date(chuoiNgay);
        if (isNaN(d.getTime())) return chuoiNgay;
        var ngay = ('0' + d.getDate()).slice(-2);
        var thang = ('0' + (d.getMonth() + 1)).slice(-2);
        var nam = d.getFullYear();
        return ngay + '/' + thang + '/' + nam;
      } catch (e) {
        return chuoiNgay;
      }
    };
  });

  /**
   * Directive enterXuongDong:
   * Cho phép nhấn phím Enter để tự động chuyển xuống ô input tiếp theo ("xuống dòng").
   * Chỉ khi đã điền đầy đủ thông tin bắt buộc và tới ô cuối cùng thì phím Enter mới xác nhận Đăng nhập hoặc Đăng ký.
   */
  app.directive('enterXuongDong', function () {
    return {
      restrict: 'A',
      link: function (scope, element) {
        element.on('keydown', function (e) {
          if (e.key === 'Enter' || e.keyCode === 13) {
            var target = e.target;
            if (!target || target.tagName !== 'INPUT') return;
            if (target.type === 'submit' || target.type === 'button') return;

            var form = element[0];
            var inputs = Array.prototype.slice.call(
              form.querySelectorAll('input:not([type="hidden"]):not([disabled]):not([type="submit"]):not([type="button"])')
            );

            var currentIndex = inputs.indexOf(target);
            if (currentIndex === -1) return;

            // Kiểm tra các trường required (hoặc tất cả inputs)
            var requiredInputs = inputs.filter(function (inp) {
              return inp.hasAttribute('required');
            });
            var checkList = requiredInputs.length ? requiredInputs : inputs;

            var duThongTin = checkList.every(function (inp) {
              return inp.value && inp.value.trim().length > 0;
            });

            // Trường hợp 1: Chưa ở ô cuối cùng -> Ngăn submit và chuyển focus xuống ô kế tiếp
            if (currentIndex < inputs.length - 1) {
              e.preventDefault();
              e.stopPropagation();
              var nextInput = inputs[currentIndex + 1];
              nextInput.focus();
              if (typeof nextInput.select === 'function') {
                nextInput.select();
              }
              scope.$applyAsync();
              return false;
            }

            // Trường hợp 2: Đang ở ô cuối cùng nhưng chưa điền đủ các ô bắt buộc phía trước
            if (!duThongTin) {
              e.preventDefault();
              e.stopPropagation();
              // Nhảy về ô trống đầu tiên để người dùng tiếp tục điền
              for (var i = 0; i < checkList.length; i++) {
                if (!checkList[i].value || !checkList[i].value.trim()) {
                  checkList[i].focus();
                  if (typeof checkList[i].select === 'function') {
                    checkList[i].select();
                  }
                  break;
                }
              }
              scope.$applyAsync();
              return false;
            }

            // Trường hợp 3: Đang ở ô cuối cùng VÀ đã điền đầy đủ thông tin -> Cho phép Enter submit form!
          }
        });
      }
    };
  });

})();
