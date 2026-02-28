/* =========================================
   PAGE: XỬ LÝ ĐĂNG NHẬP & QUÊN MẬT KHẨU
   ========================================= */

$(document).ready(function() {

    // 1. TÍNH NĂNG ẨN / HIỆN MẬT KHẨU (TRANG LOGIN)
    $('#togglePassword').click(function() {
        var passInput = $('#loginPassword');
        var icon = $(this).find('i');

        if (passInput.attr('type') === 'password') {
            passInput.attr('type', 'text');
            icon.removeClass('fa-eye-slash').addClass('fa-eye text-success');
        } else {
            passInput.attr('type', 'password');
            icon.removeClass('fa-eye text-success').addClass('fa-eye-slash text-muted');
        }
    });

    // 2. XỬ LÝ FORM ĐĂNG NHẬP (TRANG LOGIN)
    $('#loginForm').submit(function(e) {
        e.preventDefault(); 
        
        // Lấy dữ liệu từ ô input
        var account = $('#loginAccount').val().trim();
        var password = $('#loginPassword').val();
        var btn = $('#btnLogin');
        var originalText = btn.html();

        if(account === '' || password === '') {
            return; 
        }

        btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i> Đang xác thực...');

        setTimeout(function() {
            // Cập nhật logic kiểm tra: Cho phép cả email, tài khoản hoặc số điện thoại của bạn
            if (account === 'admin@cookingshare.vn' || 
                account === 'anhlh240199@tvu-onschool.edu.vn' || 
                account === '0338684934' || 
                account === 'admin') {
                
                Swal.fire({
                    icon: 'success',
                    title: 'Đăng nhập thành công!',
                    text: 'Đang chuyển hướng vào hệ thống...',
                    timer: 1500,
                    showConfirmButton: false
                }).then(() => {
                    window.location.href = 'admin-dashboard.html';
                });

            } else {
                btn.prop('disabled', false).html(originalText);
                Swal.fire({
                    icon: 'error',
                    title: 'Đăng nhập thất bại',
                    text: 'Tài khoản hoặc mật khẩu không chính xác. Vui lòng thử lại!'
                });
            }
        }, 1500);
    });

    // 3. XỬ LÝ FORM QUÊN MẬT KHẨU
    $('#forgotPasswordForm').submit(function(e) {
        e.preventDefault();
        
        var email = $('#resetEmail').val();
        var btn = $('#btnReset');
        var originalText = btn.html();

        btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i> Đang gửi...');

        // Giả lập gửi email mất 1.5 giây
        setTimeout(function() {
            btn.prop('disabled', false).html(originalText);
            
            Swal.fire({
                icon: 'success',
                title: 'Đã gửi liên kết!',
                text: `Vui lòng kiểm tra hộp thư ${email} để đặt lại mật khẩu.`,
                confirmButtonColor: '#28a745'
            }).then(() => {
                // Gửi xong thì tự quay về trang đăng nhập
                window.location.href = 'admin-login.html';
            });
            
        }, 1500);
    });

});