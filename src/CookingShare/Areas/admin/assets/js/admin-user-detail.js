/* =========================================
   PAGE: CHI TIẾT NGƯỜI DÙNG (admin-user-detail.js)
   ========================================= */

$(document).ready(function() {
    
    // 1. KHỞI TẠO DATATABLES CHO CÁC BẢNG TRONG TAB
    var tables = $('.user-datatable').DataTable({
        "language": {
            "search": "Tìm kiếm:",
            "zeroRecords": "Không có dữ liệu",
            "info": "Trang _PAGE_ / _PAGES_",
            "paginate": { "next": "Sau", "previous": "Trước" }
        },
        "pageLength": 5, // Để ngắn vì màn hình này chủ yếu để xem nhanh
        "lengthChange": false // Tắt nút chọn số dòng hiển thị cho gọn
    });

    // 2. FIX LỖI DATATABLE TRONG BOOTSTRAP TABS (Rất quan trọng)
    // Khi chuyển Tab, bảng đang bị ẩn sẽ bị sai độ rộng cột. Đoạn code này tính toán lại cột.
    $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        $.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();
    });

    // 3. XỬ LÝ NÚT KHÓA TÀI KHOẢN (TRÊN HEADER)
    var isLocked = false;
    $('#btn-lock-user').click(function() {
        var btn = $(this);
        
        if(!isLocked) {
            // ĐANG HOẠT ĐỘNG -> CHUYỂN SANG KHÓA
            Swal.fire({
                title: 'Khóa tài khoản?',
                html: 'Tài khoản <b>Trần Thị B</b> sẽ bị đăng xuất lập tức và không thể truy cập lại.',
                icon: 'warning',
                input: 'text', // Bắt nhập lý do
                inputPlaceholder: 'Nhập lý do khóa (ví dụ: Spam nhiều lần)...',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Đồng ý Khóa',
                cancelButtonText: 'Hủy',
                inputValidator: (value) => {
                    if (!value) {
                        return 'Vui lòng nhập lý do khóa!'
                    }
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    // Update UI
                    isLocked = true;
                    btn.removeClass('btn-warning').addClass('btn-success');
                    btn.html('<i class="fas fa-unlock me-1"></i> Mở khóa tài khoản');
                    
                    $('#status-badge').removeClass('bg-success').addClass('bg-danger').html('<i class="fas fa-ban me-1"></i> Đã bị khóa');
                    
                    Swal.fire('Đã khóa!', `Lý do: ${result.value}`, 'success');
                }
            });
        } else {
            // ĐANG KHÓA -> CHUYỂN SANG MỞ KHÓA
            Swal.fire({
                title: 'Mở khóa tài khoản?',
                text: 'Người dùng sẽ có thể tiếp tục đăng nhập và đăng bài.',
                icon: 'info',
                showCancelButton: true,
                confirmButtonColor: '#198754',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Đồng ý',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Update UI
                    isLocked = false;
                    btn.removeClass('btn-success').addClass('btn-warning text-dark');
                    btn.html('<i class="fas fa-lock me-1"></i> Khóa tài khoản');
                    
                    $('#status-badge').removeClass('bg-danger').addClass('bg-success').html('Đang hoạt động');
                    
                    Swal.fire('Thành công!', 'Tài khoản đã được mở khóa.', 'success');
                }
            });
        }
    });

    // 4. NÚT XÓA NỘI DUNG (Dùng chung cho cả Công thức/Mẹo vặt trong tab)
    $('.btn-delete-content').click(function() {
        var row = $(this).closest('tr');
        Swal.fire({
            title: 'Xóa nội dung này?',
            text: "Dữ liệu sẽ bị xóa khỏi hệ thống!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                // Xóa dòng trong datatable hiện tại
                row.fadeOut(400, function() {
                    var tableApi = $(this).closest('table').DataTable();
                    tableApi.row($(this)).remove().draw(false);
                });
                Swal.fire('Đã xóa!', 'Nội dung đã được gỡ bỏ.', 'success');
            }
        })
    });
});