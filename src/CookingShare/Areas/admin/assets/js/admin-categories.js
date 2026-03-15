$(document).ready(function() {
            
    // 1. Khởi tạo Datatable (Lấy instance từ admin-main.js)
    var table = $('.datatable').DataTable();

    // 2. Tự động tạo Slug khi nhập Tên (Auto-Slug)
    $('#catName').on('input', function() {
        var name = $(this).val();
        var slug = name.toLowerCase()
                       .normalize("NFD").replace(/[\u0300-\u036f]/g, "") // Bỏ dấu tiếng Việt
                       .replace(/đ/g, "d").replace(/Đ/g, "D") // Xử lý chữ Đ
                       .replace(/[^a-z0-9\s]/g, "") // Bỏ ký tự đặc biệt
                       .trim()
                       .replace(/\s+/g, "-"); // Thay khoảng trắng bằng -
        $('#catSlug').val(slug);
    });

    // 3. Xem trước ảnh khi chọn file (Image Preview)
    $('#catImageInput').change(function() {
        const file = this.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function(e) {
                $('#imgPreview').attr('src', e.target.result);
            }
            reader.readAsDataURL(file);
        }
    });

    // 4. Xử lý nút Sửa
    $('.datatable tbody').on('click', '.btn-edit', function() {
        var row = $(this).closest('tr');
        var name = row.find('td:eq(2) h6').text(); 
        var slug = row.find('td:eq(3)').text();
        
        $('.modal-title').text('Cập nhật Danh mục');
        $('#catName').val(name);
        $('#catSlug').val(slug);
        
        var myModal = new bootstrap.Modal(document.getElementById('categoryModal'));
        myModal.show();
    });

    // 5. Xử lý nút Lưu
    $('#btn-save-category').click(function() {
        var name = $('#catName').val();

        if(name === '') {
            Swal.fire('Lỗi', 'Vui lòng nhập tên danh mục!', 'error');
            return;
        }

        // CÁCH ĐÓNG MODAL AN TOÀN TRÁNH LỖI JS
        var modalEl = document.getElementById('categoryModal');
        var modal = bootstrap.Modal.getInstance(modalEl);
        if (!modal) {
            modal = new bootstrap.Modal(modalEl);
        }
        modal.hide();

        Swal.fire({
            icon: 'success',
            title: 'Thành công',
            text: 'Danh mục đã được cập nhật!',
            timer: 1500,
            showConfirmButton: false
        });

        // Reset form
        $('#categoryForm')[0].reset();
        $('#imgPreview').attr('src', 'https://placehold.co/300x150?text=Preview');
        $('.modal-title').text('Thêm Danh Mục Mới');
    });

    // Reset modal khi đóng
    $('#categoryModal').on('hidden.bs.modal', function () {
        $('#categoryForm')[0].reset();
        $('#imgPreview').attr('src', 'https://placehold.co/300x150?text=Preview');
        $('.modal-title').text('Thêm Danh Mục Mới');
    });
});