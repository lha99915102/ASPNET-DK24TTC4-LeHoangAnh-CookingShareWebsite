/* =========================================
   PAGE: QUẢN LÝ NGƯỜI DÙNG (admin-users.js)
   ========================================= */

$(document).ready(function() {
    
    // 1. Lấy instance DataTable (tránh lỗi khởi tạo 2 lần)
    var table = $('.datatable').DataTable();

    // 2. LỌC THEO VAI TRÒ (Cột 2 - index 2)
    $('#filterRole').change(function() {
        var val = $(this).val();
        // Regex = false, Smart = true
        table.column(2).search(val).draw();
    });

    // 3. LỌC THEO TRẠNG THÁI (Cột 3 - index 3)
    $('#filterStatus').change(function() {
        var val = $(this).val();
        table.column(3).search(val).draw();
    });

    // 4. XỬ LÝ NÚT KHÓA / MỞ KHÓA TÀI KHOẢN (BAN/UNBAN)
    $('.datatable tbody').on('click', '.btn-lock', function() {
        var btn = $(this);
        var userName = btn.data('name');
        var currentStatus = btn.data('status'); // 'active' hoặc 'locked'
        
        var isLocking = (currentStatus === 'active');
        
        var title = isLocking ? 'Khóa tài khoản?' : 'Mở khóa tài khoản?';
        var text = isLocking ? `Bạn có chắc muốn KHÓA tài khoản của <b>${userName}</b> không? Người này sẽ không thể đăng nhập.` 
                             : `Bạn muốn MỞ KHÓA cho <b>${userName}</b>? Người này có thể đăng nhập lại.`;
        var icon = isLocking ? 'warning' : 'info';
        var confirmBtnText = isLocking ? 'Đồng ý Khóa' : 'Đồng ý Mở khóa';
        var confirmBtnColor = isLocking ? '#dc3545' : '#198754';

        Swal.fire({
            title: title,
            html: text,
            icon: icon,
            showCancelButton: true,
            confirmButtonColor: confirmBtnColor,
            cancelButtonColor: '#6c757d',
            confirmButtonText: confirmBtnText,
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                // Xử lý logic đổi màu nút / đổi trạng thái (Giả lập Frontend)
                if(isLocking) {
                    btn.data('status', 'locked');
                    btn.removeClass('btn-outline-warning').addClass('btn-outline-success');
                    btn.attr('title', 'Mở khóa tài khoản');
                    btn.html('<i class="fas fa-unlock"></i>');
                    
                    Swal.fire('Đã Khóa!', `Tài khoản ${userName} đã bị khóa.`, 'success');
                } else {
                    btn.data('status', 'active');
                    btn.removeClass('btn-outline-success').addClass('btn-outline-warning');
                    btn.attr('title', 'Khóa tài khoản');
                    btn.html('<i class="fas fa-lock"></i>');
                    
                    Swal.fire('Đã Mở Khóa!', `Tài khoản ${userName} đã hoạt động trở lại.`, 'success');
                }
            }
        });
    });

    // 5. XỬ LÝ NÚT SỬA (EDIT)
    $('.datatable tbody').on('click', '.btn-edit', function() {
        var row = $(this).closest('tr');
        var name = row.find('td:eq(0) span.text-dark').text(); 
        var email = row.find('td:eq(1)').text();
        
        // Cập nhật Modal
        $('.modal-title').text('Cập nhật: ' + name);
        $('#userName').val(name.trim());
        $('#userEmail').val(email.trim());
        
        var myModal = new bootstrap.Modal(document.getElementById('userModal'));
        myModal.show();
    });

    // 6. XỬ LÝ NÚT LƯU THAY ĐỔI
    $('#btn-save-user').click(function() {
        var name = $('#userName').val();
        
        if(name.trim() === '') {
            Swal.fire('Lỗi', 'Tên không được để trống!', 'error');
            return;
        }

        // Đóng Modal an toàn
        var modalEl = document.getElementById('userModal');
        var modal = bootstrap.Modal.getInstance(modalEl);
        if (!modal) {
            modal = new bootstrap.Modal(modalEl);
        }
        modal.hide();

        Swal.fire({
            icon: 'success',
            title: 'Thành công',
            text: 'Dữ liệu người dùng đã được cập nhật!',
            timer: 2000,
            showConfirmButton: false
        });

        // Reset form
        $('#userForm')[0].reset();
        $('.modal-title').text('Cập nhật Người dùng');
    });

    // Reset Modal khi ẩn
    $('#userModal').on('hidden.bs.modal', function () {
        $('#userForm')[0].reset();
        $('.modal-title').text('Cập nhật Người dùng');
    });
});