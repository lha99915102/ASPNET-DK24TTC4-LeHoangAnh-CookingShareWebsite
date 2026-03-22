/* =========================================
   PAGE: QUẢN LÝ BANNER (admin-banners.js)
   ========================================= */

$(document).ready(function() {
    
    // 1. Khởi tạo DataTable
    var table = $('.datatable').DataTable();

    // 2. Lọc theo trạng thái
    $('#filterStatus').change(function() {
        var val = $(this).val();
        table.column(3).search(val).draw();
    });

    // 3. Hiển thị Ảnh Xem Trước (Image Preview)
    $('#bannerImage').change(function() {
        const file = this.files[0];
        if (file) {
            // Check dung lượng 2MB
            if(file.size > 2 * 1024 * 1024) {
                Swal.fire('Lỗi', 'Dung lượng ảnh vượt quá 2MB.', 'error');
                this.value = '';
                return;
            }
            const reader = new FileReader();
            reader.onload = function(e) {
                $('#bannerPreview').attr('src', e.target.result);
            }
            reader.readAsDataURL(file);
        }
    });

    // 4. Xử lý nút Sửa Banner
    $('.datatable tbody').on('click', '.btn-edit', function() {
        var row = $(this).closest('tr');
        
        // Lấy dữ liệu demo
        var order = row.find('td:eq(0) span').text();
        var imageSrc = row.find('td:eq(1) img').attr('src');
        var title = row.find('td:eq(2) h6').text();
        var link = row.find('td:eq(2) a').text();
        
        // Fill vào Modal
        $('.modal-title').text('Cập nhật Banner');
        $('#bannerOrder').val(order);
        $('#bannerPreview').attr('src', imageSrc);
        $('#bannerTitle').val(title);
        $('#bannerLink').val(link.replace('/', '')); // Bỏ dấu gạch chéo hiển thị
        
        // Mở Modal
        var myModal = new bootstrap.Modal(document.getElementById('bannerModal'));
        myModal.show();
    });

    // 5. Xử lý nút Lưu Banner
    $('#btn-save-banner').click(function() {
        // Có thể validate thêm Title và Link ở đây
        
        var modalEl = document.getElementById('bannerModal');
        var modal = bootstrap.Modal.getInstance(modalEl);
        if (!modal) { modal = new bootstrap.Modal(modalEl); }
        modal.hide();

        Swal.fire({
            icon: 'success',
            title: 'Lưu thành công',
            text: 'Banner đã được cập nhật trên trang chủ!',
            timer: 1500,
            showConfirmButton: false
        });

        // Reset form khi lưu xong
        resetBannerModal();
    });

    // 6. Xóa Form khi tắt Modal để lần sau bấm Thêm mới nó trống trơn
    $('#bannerModal').on('hidden.bs.modal', function () {
        resetBannerModal();
    });

    function resetBannerModal() {
        $('#bannerForm')[0].reset();
        $('#bannerPreview').attr('src', 'https://placehold.co/800x300?text=Upload+Image+(1920x600)');
        $('.modal-title').text('Thêm Banner Mới');
    }
});