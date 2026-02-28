/* =========================================
   admin-tags.js
   ========================================= */

$(document).ready(function() {
        
        var table = $('.datatable').DataTable();

        // --- 1. XỬ LÝ BỘ LỌC ---
        $('input[name="tagFilter"]').change(function() {
            var selectedId = $(this).attr('id');
            var searchText = '';

            // Map ID của nút radio sang từ khóa cần tìm trong bảng
            switch(selectedId) {
                case 'filterAllergy':
                    searchText = 'Dị ứng'; // Tìm chữ "Dị ứng" trong bảng
                    break;
                case 'filterDiet':
                    searchText = 'Chế độ ăn'; // Tìm chữ "Chế độ ăn"
                    break;
                case 'filterAll':
                    searchText = ''; // Rỗng = Hiện tất cả
                    break;
            }

            // Lọc cột thứ 2 (Index 1: Loại Tag)
            table.column(1).search(searchText).draw();
        });

        // 3. [MỚI] XỬ LÝ NÚT SỬA (EDIT)
        // Dùng .on('click') để bắt sự kiện cho cả các trang sau (nếu có phân trang)
        $('.datatable tbody').on('click', '.btn-edit', function() {
            // A. Lấy dữ liệu từ dòng hiện tại để điền vào Modal
            var row = $(this).closest('tr');
            var tagName = row.find('td:eq(0)').text(); // Cột 0: Tên Tag
            
            // B. Đổi tiêu đề Modal và điền dữ liệu
            $('.modal-title').text('Cập nhật Tag: ' + tagName);
            $('#tagName').val(tagName);
            
            // C. Mở Modal
            var myModal = new bootstrap.Modal(document.getElementById('tagModal'));
            myModal.show();
        });

        // 4. XỬ LÝ NÚT LƯU TAG (TỰ ĐỘNG CHỌN MÀU)
        $('#btn-save-tag').click(function() {
            var name = $('#tagName').val();
            var type = $('#tagTypeSelect').val();
            
            if(name.trim() === '') {
                Swal.fire('Lỗi', 'Vui lòng nhập tên Tag!', 'error');
                return;
            }

            var badgeClass = '';
            var typeName = '';

            if (type === 'allergy') {
                badgeClass = 'bg-danger'; typeName = 'Cảnh báo Dị ứng';
            } else if (type === 'diet') {
                badgeClass = 'bg-success'; typeName = 'Chế độ ăn';
            } else {
                badgeClass = 'bg-warning text-dark'; typeName = 'Dịp lễ / Khác';
            }

            // Đóng Modal
            // Tìm instance hoặc tạo mới nếu chưa có
            var modalEl = document.getElementById('tagModal');
            var modal = bootstrap.Modal.getInstance(modalEl);
            if (!modal) {
                modal = new bootstrap.Modal(modalEl);
            }
            modal.hide();

            Swal.fire({
                icon: 'success',
                title: 'Thành công!',
                html: `Đã lưu Tag: <b>${name}</b><br>
                       Loại: ${typeName}<br>`,
                timer: 2000,
                showConfirmButton: false
            });

            // Reset form và tiêu đề Modal về mặc định
            $('#tagForm')[0].reset();
            $('.modal-title').text('Thêm Tag Mới');
        });
        
        // 5. Reset Modal khi đóng (Để lần sau bấm "Thêm mới" nó sạch sẽ)
        $('#tagModal').on('hidden.bs.modal', function () {
            $('#tagForm')[0].reset();
            $('.modal-title').text('Thêm Tag Mới');
        });
    });