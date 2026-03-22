/* =========================================
   admin-units.js
   ========================================= */

$(document).ready(function() {
            
            // 1. Khởi tạo Datatable
            var table = $('.datatable').DataTable();

            // 2. Logic Modal: Tự động gợi ý khi chọn Loại
            $('#unitType').change(function() {
                var type = $(this).val();
                var helpText = $('#conversionHelp');
                var rateInput = $('#conversionRate');

                if (type === 'quantity') {
                    // Nếu là Số lượng (Cái/Con) -> Mặc định 0
                    rateInput.val(0);
                    rateInput.prop('disabled', true); // Khóa lại
                    helpText.html('<i class="fas fa-exclamation-circle text-danger me-1"></i> Đơn vị này sẽ được định nghĩa riêng trong từng Nguyên liệu.');
                } else {
                    rateInput.prop('disabled', false); // Mở khóa
                    if (type === 'weight') {
                        helpText.html('<i class="fas fa-check text-success me-1"></i> Với khối lượng, quy đổi là cố định (VD: 1kg = 1000g).');
                    } else {
                        helpText.html('<i class="fas fa-info-circle text-info me-1"></i> Với thể tích, hãy nhập ước lượng trung bình (VD: 1 thìa = 15g).');
                    }
                }
            });

            // 3. Xử lý nút Sửa
            $('.datatable tbody').on('click', '.btn-edit', function() {
                var row = $(this).closest('tr');
                var name = row.find('td:eq(0)').text().trim(); // Lấy tên
                var abbr = row.find('td:eq(1)').text().trim(); // Lấy viết tắt
                
                // Demo điền dữ liệu
                $('.modal-title').text('Cập nhật: ' + name);
                $('#unitName').val(name);
                $('#unitAbbr').val(abbr);

                var myModal = new bootstrap.Modal(document.getElementById('unitModal'));
                myModal.show();
            });

            // 4. Xử lý nút Lưu
            $('#btn-save-unit').click(function() {
                var name = $('#unitName').val();
                var rate = $('#conversionRate').val();

                // Validate cơ bản
                if(name === '') {
                    Swal.fire('Lỗi', 'Vui lòng nhập tên đơn vị!', 'error');
                    return;
                }

                var modalEl = document.getElementById('unitModal');
                var modal = bootstrap.Modal.getInstance(modalEl);
                modal.hide();

                Swal.fire({
                    icon: 'success',
                    title: 'Đã lưu!',
                    html: `Đã cập nhật đơn vị: <b>${name}</b><br>Hệ số quy đổi: ${rate}g`,
                    timer: 2000,
                    showConfirmButton: false
                });

                // Reset modal về trạng thái thêm mới
                $('.modal-title').text('Thêm Đơn vị đo lường');
                $('#unitForm')[0].reset();
                $('#conversionRate').prop('disabled', false);
            });

            // Reset modal khi đóng
            $('#unitModal').on('hidden.bs.modal', function () {
                $('.modal-title').text('Thêm Đơn vị đo lường');
                $('#unitForm')[0].reset();
                $('#conversionRate').prop('disabled', false);
                $('#conversionHelp').html('<i class="fas fa-lightbulb text-warning me-1"></i> Nhập <b>0</b> nếu đơn vị này phụ thuộc vào từng nguyên liệu.');
            });
        });