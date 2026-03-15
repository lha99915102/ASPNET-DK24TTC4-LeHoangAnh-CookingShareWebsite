/* =========================================
   ADMIN MAIN JS (admin-main.js)
   ========================================= */

document.addEventListener('DOMContentLoaded', function() {
    // 1. TOGGLE SIDEBAR
    const sidebarCollapse = document.getElementById('sidebarCollapse');
    const sidebar = document.getElementById('sidebar');
    const content = document.getElementById('content');

    if (sidebarCollapse && sidebar && content) {
        sidebarCollapse.addEventListener('click', function() {
            sidebar.classList.toggle('active');
            content.classList.toggle('active');
        });
    }

    // 2. VẼ BIỂU ĐỒ (Chỉ chạy ở Dashboard)
    const chartElement = document.getElementById('mainChart');
    if (chartElement) {
        const ctx = chartElement.getContext('2d');
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'CN'],
                datasets: [
                    {
                        label: 'Lượt xem trang',
                        data: [1500, 2300, 3200, 2800, 4100, 5600, 6200],
                        borderColor: '#FF6600',
                        backgroundColor: 'rgba(255, 102, 0, 0.1)',
                        tension: 0.4,
                        fill: true,
                        yAxisID: 'y'
                    },
                    {
                        label: 'Bài viết mới',
                        data: [12, 19, 8, 15, 22, 30, 25],
                        type: 'bar',
                        backgroundColor: '#2c3e50',
                        yAxisID: 'y1'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                scales: {
                    y: {
                        type: 'linear', display: true, position: 'left',
                        title: { display: true, text: 'Lượt xem' }
                    },
                    y1: {
                        type: 'linear', display: true, position: 'right',
                        grid: { drawOnChartArea: false },
                        title: { display: true, text: 'Bài đăng' }
                    }
                }
            }
        });
    }
});

// SỬ DỤNG JQUERY
$(document).ready(function() {
    
    var table; // Biến toàn cục

    // 3. KÍCH HOẠT DATATABLES
    if ($('.datatable').length > 0) {
        table = $('.datatable').DataTable({
            "language": {
                "lengthMenu": "Hiện _MENU_ dòng",
                "zeroRecords": "Không tìm thấy dữ liệu",
                "info": "Trang _PAGE_ / _PAGES_",
                "infoEmpty": "Không có dữ liệu",
                "search": "Tìm kiếm:",
                "paginate": { "next": "Sau", "previous": "Trước" }
            },
            "ordering": true,
            "responsive": true
        });

        // --- [QUAN TRỌNG] KÍCH HOẠT LỌC MẶC ĐỊNH KHI VÀO TRANG ---
        // Khi vừa vào trang, nếu đang ở trang Công thức (có bộ lọc), tự động lọc "Chờ duyệt"
        if ($('#statusFilters').length > 0) {
            table.column(4).search('Chờ duyệt').draw();
        }
    }

    // 4. XỬ LÝ CLICK TAB BỘ LỌC
    $('#statusFilters .nav-link').click(function(e) {
        e.preventDefault();

        // Đổi màu tab
        $('#statusFilters .nav-link').removeClass('active bg-warning text-dark').addClass('text-secondary');
        $(this).removeClass('text-secondary').addClass('active bg-warning text-dark');
        
        // Lấy từ khóa lọc
        var filterType = $(this).data('filter'); 
        var searchText = '';

        switch(filterType) {
            case 'pending': searchText = 'Chờ duyệt'; break;
            case 'approved': searchText = 'Đã duyệt'; break;
            case 'rejected': searchText = 'Đã từ chối'; break;
            default: searchText = ''; // Hiện tất cả
        }

        // Lọc bảng
        if (table) {
            // Reset lọc cũ trước khi lọc mới
            table.search('').columns().search('').draw();
            
            if (searchText !== '') {
                // Cột 4 là cột Trạng thái (đếm từ 0)
                table.column(4).search(searchText).draw();
            }
        }
    });


    // 5. SWEETALERT EVENTS
    $(document).on('click', '.btn-approve', function() {
        Swal.fire({ icon: 'success', title: 'Đã phê duyệt!', timer: 1500, showConfirmButton: false });
    });
    $(document).on('click', '.btn-reject', function() {
        Swal.fire({ icon: 'warning', title: 'Đã từ chối!', timer: 1500, showConfirmButton: false });
    });
    $(document).on('click', '.btn-delete', function() {
        Swal.fire({
            title: 'Xóa dữ liệu?', icon: 'error', showCancelButton: true, confirmButtonText: 'Xóa', cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $(this).closest('tr').fadeOut(300, function() { $(this).remove(); });
                Swal.fire('Đã xóa!', '', 'success');
            }
        });
    });

    // 6. XỬ LÝ NÚT "SỬA NỘI DUNG" (LOGIC ĐỒNG BỘ DỮ LIỆU)
    $('#btn-toggle-edit').click(function() {
        
        // Kiểm tra xem đang ở trạng thái nào (đang Sửa hay đang Xem?)
        // Nếu khung Edit KHÔNG có class d-none -> Tức là ĐANG SỬA
        var isEditing = !$('#admin-edit-area').hasClass('d-none');

        if (!isEditing) {
            // --- TRƯỜNG HỢP 1: ĐANG Ở CHẾ ĐỘ XEM -> BẤM ĐỂ SỬA ---
            
            // A. Lấy nội dung hiện tại từ màn hình Xem
            var currentContent = $('#recipe-steps-content').html();

            // B. Đẩy nội dung đó vào Summernote để sửa
            $('#summernote').summernote('code', currentContent);

            // C. Ẩn màn hình Xem, Hiện màn hình Sửa
            $('#recipe-steps-content').addClass('d-none');
            $('#admin-edit-area').removeClass('d-none');

            // D. Đổi giao diện nút bấm thành "Lưu thay đổi"
            $(this).html('<i class="fas fa-save me-1"></i> Lưu thay đổi');
            $(this).removeClass('btn-outline-primary').addClass('btn-primary');

        } else {
            // --- TRƯỜNG HỢP 2: ĐANG SỬA -> BẤM ĐỂ LƯU ---

            // A. Lấy nội dung mới từ Summernote
            var newContent = $('#summernote').summernote('code');

            // B. Cập nhật ngược lại vào màn hình Xem
            $('#recipe-steps-content').html(newContent);

            // C. Ẩn màn hình Sửa, Hiện màn hình Xem
            $('#admin-edit-area').addClass('d-none');
            $('#recipe-steps-content').removeClass('d-none');

            // D. Đổi giao diện nút bấm về lại "Sửa"
            $(this).html('<i class="fas fa-edit me-1"></i> Sửa nội dung (Admin Edit)');
            $(this).removeClass('btn-primary').addClass('btn-outline-primary');

            // E. Thông báo nhỏ là đã cập nhật (Toast)
            const Toast = Swal.mixin({
                toast: true, position: 'top-end', showConfirmButton: false, timer: 3000
            });
            Toast.fire({ icon: 'success', title: 'Đã cập nhật nội dung xem trước' });
        }
    });

});

// 7. HÀM LƯU BỘ SƯU TẬP (TRONG MODAL)
function saveCollection() {
            bootstrap.Modal.getInstance(document.getElementById('collectionModal')).hide();
            Swal.fire({
                icon: 'success',
                title: 'Đã lưu!',
                text: 'Bộ sưu tập đã được cập nhật thành công.',
                timer: 1500,
                showConfirmButton: false
            });
        }

