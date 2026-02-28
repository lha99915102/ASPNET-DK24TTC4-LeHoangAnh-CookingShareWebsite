/* =========================================
   PAGE: HỒ SƠ CÁ NHÂN (admin-profile.js)
   ========================================= */

$(document).ready(function() {
    
    // 1. XỬ LÝ UPLOAD VÀ XEM TRƯỚC ẢNH ĐẠI DIỆN (AVATAR)
    $('#avatarInput').change(function() {
        const file = this.files[0];
        if (file) {
            // Kiểm tra dung lượng (Tối đa 1MB cho avatar)
            if(file.size > 1 * 1024 * 1024) {
                Swal.fire('Lỗi', 'Dung lượng ảnh không được vượt quá 1MB.', 'error');
                this.value = ''; // Reset input
                return;
            }

            const reader = new FileReader();
            reader.onload = function(e) {
                $('#profileAvatarPreview').attr('src', e.target.result);
                
                // Show thông báo nhỏ ở góc màn hình (Toast)
                Swal.fire({
                    toast: true,
                    position: 'bottom-end',
                    icon: 'success',
                    title: 'Đã cập nhật ảnh tạm thời. Nhớ lưu thông tin!',
                    showConfirmButton: false,
                    timer: 3000
                });
            }
            reader.readAsDataURL(file);
        }
    });

    // 2. XỬ LÝ LƯU THÔNG TIN CƠ BẢN
    $('#btn-save-info').click(function() {
        var name = $('#profileName').val().trim();
        var phone = $('#profilePhone').val().trim();

        if (name === '') {
            Swal.fire('Thiếu thông tin', 'Vui lòng nhập Họ và Tên của bạn.', 'warning');
            return;
        }

        var btn = $(this);
        var originalText = btn.html();

        // Hiệu ứng Loading
        btn.html('<i class="fas fa-spinner fa-spin me-2"></i>Đang lưu...');
        btn.prop('disabled', true);

        // Giả lập API lưu
        setTimeout(function() {
            btn.html(originalText);
            btn.prop('disabled', false);

            Swal.fire({
                icon: 'success',
                title: 'Thành công',
                text: 'Thông tin hồ sơ của bạn đã được cập nhật!',
                timer: 2000,
                showConfirmButton: false
            });
            
            // Cập nhật tên trên thanh Topbar (Nếu có)
            $('.dropdown-toggle span.fw-bold').text(name);

        }, 1000);
    });

    // 3. XỬ LÝ ĐỔI MẬT KHẨU
    $('#btn-change-password').click(function() {
        var currentPass = $('#currentPass').val();
        var newPass = $('#newPass').val();
        var confirmPass = $('#confirmPass').val();

        // Validate cơ bản
        if (currentPass === '' || newPass === '' || confirmPass === '') {
            Swal.fire('Lỗi', 'Vui lòng điền đầy đủ các trường mật khẩu.', 'error');
            return;
        }

        if (newPass.length < 8) {
            Swal.fire('Lỗi', 'Mật khẩu mới phải có ít nhất 8 ký tự.', 'warning');
            return;
        }

        if (newPass !== confirmPass) {
            Swal.fire('Lỗi', 'Mật khẩu xác nhận không khớp với mật khẩu mới.', 'error');
            return;
        }

        if (currentPass === newPass) {
            Swal.fire('Lỗi', 'Mật khẩu mới không được giống mật khẩu hiện tại.', 'warning');
            return;
        }

        var btn = $(this);
        var originalText = btn.html();

        // Hiệu ứng Loading
        btn.html('<i class="fas fa-spinner fa-spin me-2"></i>Đang xử lý...');
        btn.prop('disabled', true);

        // Giả lập gọi API đổi pass
        setTimeout(function() {
            btn.html(originalText);
            btn.prop('disabled', false);

            // Báo thành công và xóa trắng form
            Swal.fire({
                icon: 'success',
                title: 'Đã đổi mật khẩu!',
                text: 'Vui lòng sử dụng mật khẩu mới cho lần đăng nhập sau.',
                confirmButtonColor: '#28a745'
            }).then(() => {
                $('#form-change-password')[0].reset();
            });

        }, 1500);
    });
});