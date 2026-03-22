/* =========================================
   admin-ingredients.js
   ========================================= */

$(document).ready(function() {
    
    // Khởi tạo DataTable
    var table = $('.datatable').DataTable();

    // 1. XỬ LÝ LỌC NHANH (Category Filter)
    $('#categoryFilter').change(function() {
        var selectedCategory = $(this).val();
        table.column(1).search(selectedCategory).draw();
    });

    // 2. XỬ LÝ NÚT IMPORT EXCEL (Giả lập)
    $('#btn-import-excel').click(function() {
        Swal.fire({
            title: 'Nhập dữ liệu từ Excel',
            html: `
                <input type="file" class="form-control mb-3">
                <div class="text-start small text-muted">
                    <i class="fas fa-info-circle"></i> Vui lòng sử dụng <a href="#">file mẫu chuẩn</a>.
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: 'Tải lên',
            cancelButtonText: 'Hủy',
            preConfirm: () => {
                return new Promise((resolve) => {
                    setTimeout(() => { resolve() }, 1000)
                })
            }
        }).then((result) => {
            if (result.isConfirmed) {
                Swal.fire('Thành công!', 'Đã thêm 50 nguyên liệu mới từ file.', 'success');
            }
        });
    });

    // 3. XỬ LÝ NÚT SỬA (Edit)
    // Dùng .on('click') để vẫn bắt được sự kiện khi chuyển trang (pagination)
    $('.datatable').on('click', '.btn-edit', function() {
        $('#modalTitle').text('Cập nhật Nguyên liệu: Thịt Bò'); // Demo đổi tên
        var myModal = new bootstrap.Modal(document.getElementById('ingredientModal'));
        myModal.show();
    });

    // 4. XỬ LÝ NÚT LƯU (SAVE)
    $('#btn-save-ingredient').click(function() {
        
        // A. Lấy danh sách dị ứng từ các nút Toggle Badge
        var selectedAllergies = [];
        
        // Tìm các checkbox có class .allergy-item đang được CHECKED
        $('.allergy-item:checked').each(function() {
            selectedAllergies.push($(this).val());
        });

        // Debug xem đã lấy được chưa
        console.log("Dị ứng đã chọn:", selectedAllergies);

        // B. Đóng Modal
        var modalEl = document.getElementById('ingredientModal');
        var modal = bootstrap.Modal.getInstance(modalEl);
        modal.hide();
        
        // C. Hiện thông báo thành công
        Swal.fire({
            icon: 'success',
            title: 'Lưu thành công!',
            html: 'Dữ liệu đã được cập nhật.<br><b>Cảnh báo dị ứng:</b> ' + 
                  (selectedAllergies.length > 0 ? `<span class="text-danger fw-bold">${selectedAllergies.join(', ')}</span>` : '<span class="text-success">Không có</span>'),
            timer: 3000,
            showConfirmButton: false
        });
    });

});