/* =========================================
   PAGE: CẤU HÌNH WEBSITE (admin-settings.js)
   ========================================= */

$(document).ready(function() {
    
    // 1. XỬ LÝ XEM TRƯỚC ẢNH LOGO KHI UPLOAD
    $('#logoInput').change(function() {
        const file = this.files[0];
        if (file) {
            // Kiểm tra dung lượng (giả sử tối đa 2MB)
            if(file.size > 2 * 1024 * 1024) {
                Swal.fire('Lỗi', 'Dung lượng ảnh không được vượt quá 2MB.', 'error');
                this.value = ''; // Xóa file đã chọn
                return;
            }

            const reader = new FileReader();
            reader.onload = function(e) {
                $('#logoPreview').attr('src', e.target.result);
            }
            reader.readAsDataURL(file);
        }
    });

    // 2. KHỞI TẠO SUMMERNOTE (CHO TAB TRANG TĨNH)
    $('#summernote-editor').summernote({
        placeholder: 'Nhập nội dung HTML/Văn bản tại đây...',
        tabsize: 2,
        height: 350, // Chiều cao mặc định lớn hơn chút
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'clear']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['insert', ['link', 'picture']],
            ['view', ['fullscreen', 'codeview', 'help']]
        ]
    });

    // Giả lập load nội dung khi đổi Trang trong select box
    $('#pageSelect').change(function() {
        var page = $(this).val();
        var fakeContent = '';

        if(page === 'about') {
            fakeContent = '<h2>Về CookingShare</h2><p>Cộng đồng chia sẻ công thức nấu ăn lớn nhất Việt Nam...</p>';
        } else if (page === 'terms') {
            fakeContent = '<h2>Điều khoản dịch vụ</h2><p>Bằng việc đăng ký tài khoản, bạn đồng ý với các điều khoản sau...</p>';
        } else {
            fakeContent = '<h2>Chính sách bảo mật</h2><p>Chúng tôi cam kết bảo vệ dữ liệu cá nhân của bạn...</p>';
        }

        // Ghi đè nội dung vào khung soạn thảo
        $('#summernote-editor').summernote('code', fakeContent);
    });

    // Load mặc định nội dung trang 'about' khi vừa vào tab
    $('#pageSelect').trigger('change');

    // 3. XỬ LÝ NÚT LƯU CHUNG CHO CÁC TAB
    $('.btn-save-settings').click(function() {
        var btn = $(this);
        var originalText = btn.html();

        // Hiệu ứng Loading trên nút
        btn.html('<i class="fas fa-spinner fa-spin me-2"></i>Đang lưu...');
        btn.prop('disabled', true);

        // Giả lập gọi API lưu dữ liệu mất 1.5 giây
        setTimeout(function() {
            btn.html(originalText);
            btn.prop('disabled', false);

            Swal.fire({
                icon: 'success',
                title: 'Lưu thành công!',
                text: 'Các cài đặt đã được áp dụng lên hệ thống.',
                timer: 2000,
                showConfirmButton: false
            });
        }, 1500);
    });

});