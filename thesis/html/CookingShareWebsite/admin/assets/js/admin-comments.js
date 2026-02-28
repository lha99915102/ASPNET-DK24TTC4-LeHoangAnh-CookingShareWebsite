/* =========================================
   PAGE: QUẢN LÝ BÌNH LUẬN (admin-comments.js)
   ========================================= */

$(document).ready(function() {
    
    // 1. Khởi tạo Datatable (Lấy instance từ admin-main.js)
    var table = $('.datatable').DataTable();

    // 2. LỌC THEO NGUỒN (Cột 3 - index 2)
    $('#filterSource').change(function() {
        var val = $(this).val();
        table.column(2).search(val).draw();
    });

    // 3. LỌC THEO TRẠNG THÁI (Cột 5 - index 4)
    $('#filterStatus').change(function() {
        var val = $(this).val();
        table.column(4).search(val).draw();
    });

    // 4. XỬ LÝ NÚT XEM CHI TIẾT (VIEW MODAL)
    $('.datatable tbody').on('click', '.btn-view-comment', function() {
        var row = $(this).closest('tr');
        
        // Lấy dữ liệu từ dòng được bấm (DOM Traversal)
        var authorName = row.find('td:eq(0) a').text().trim();
        var authorAvatar = row.find('td:eq(0) img').attr('src');
        var fullComment = row.find('td:eq(1) .comment-text').attr('title'); // Lấy text đầy đủ từ attribute title
        var sourceBadge = row.find('td:eq(2) .badge').text().trim();
        var sourceClass = row.find('td:eq(2) .badge').attr('class');
        var targetLink = row.find('td:eq(2) a').text().trim();
        var dateStr = row.find('td:eq(3)').html().replace('<br>', ' - ').replace(/<[^>]*>?/gm, ''); // Lấy ngày giờ, bỏ thẻ html
        
        // Đổ dữ liệu vào Modal
        $('#modal-author').text(authorName);
        $('#modal-avatar').attr('src', authorAvatar);
        $('#modal-content').text(fullComment);
        $('#modal-source-badge').text(sourceBadge).attr('class', sourceClass); // Gán lại đúng màu badge (VD: bg-primary)
        $('#modal-target-link').text(targetLink);
        $('#modal-date').text(dateStr);
        
        // Mở Modal
        var myModal = new bootstrap.Modal(document.getElementById('commentModal'));
        myModal.show();
    });

    // 5. XỬ LÝ NÚT ẨN / HIỆN BÌNH LUẬN (Soft Delete)
    $('.datatable tbody').on('click', '.btn-hide-comment', function() {
        var btn = $(this);
        var row = btn.closest('tr');
        var statusCell = row.find('td:eq(4)');
        
        // Kiểm tra xem bình luận đang Ẩn hay Hiện dựa vào Icon của nút bấm
        var isHidden = btn.find('i').hasClass('fa-eye'); // Nút mang icon "Mắt mở" nghĩa là đang ẩn, cần bấm để hiện

        if (isHidden) {
            // ĐANG ẨN -> ĐỔI THÀNH HIỆN
            Swal.fire({
                title: 'Khôi phục bình luận?',
                text: "Bình luận này sẽ hiển thị lại cho mọi người cùng xem.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#198754',
                confirmButtonText: 'Khôi phục',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Update UI: Row
                    row.removeClass('opacity-75 bg-light');
                    // Update UI: Nút bấm
                    btn.removeClass('btn-outline-success').addClass('btn-outline-warning');
                    btn.html('<i class="fas fa-eye-slash"></i>');
                    btn.attr('title', 'Ẩn bình luận');
                    // Update UI: Trạng thái
                    statusCell.html('<span class="badge bg-success">Bình thường</span>');
                    
                    Swal.fire('Thành công', 'Bình luận đã được khôi phục.', 'success');
                }
            });
        } else {
            // ĐANG HIỆN -> ĐỔI THÀNH ẨN
            Swal.fire({
                title: 'Ẩn bình luận này?',
                text: "Bình luận sẽ bị làm mờ và ẩn khỏi người dùng bình thường.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ffc107',
                confirmButtonText: 'Vâng, Ẩn nó!',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Update UI: Row
                    row.removeClass('table-danger').addClass('opacity-75 bg-light');
                    // Update UI: Nút bấm
                    btn.removeClass('btn-warning btn-outline-warning text-dark').addClass('btn-outline-success');
                    btn.html('<i class="fas fa-eye"></i>');
                    btn.attr('title', 'Đang ẩn -> Ấn để Hiện lại');
                    // Update UI: Trạng thái
                    statusCell.html('<span class="badge bg-secondary"><i class="fas fa-eye-slash me-1"></i> Đã ẩn</span>');
                    
                    Swal.fire('Đã ẩn', 'Bình luận không còn hiển thị công khai.', 'success');
                }
            });
        }
    });

    // (Ghi chú: Nút Xóa vĩnh viễn .btn-delete đã được xử lý chung ở file admin-main.js)
});