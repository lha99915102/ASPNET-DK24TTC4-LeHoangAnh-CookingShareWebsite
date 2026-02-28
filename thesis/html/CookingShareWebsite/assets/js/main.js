$(document).ready(function(){
    
    // Kích hoạt Hero Banner Slider
    $("#hero-slider").owlCarousel({
        items: 1,           // Hiện 1 ảnh
        loop: true,         // Lặp lại
        margin: 0,
        nav: false,         // Ẩn nút Next/Prev mặc định (cho đẹp)
        dots: true,         // Hiện dấu chấm tròn bên dưới
        autoplay: true,     // Tự chạy
        autoplayTimeout: 5000, // 5 giây đổi 1 lần
        smartSpeed: 1000,   // Tốc độ trượt mượt mà
        animateOut: 'fadeOut' // Hiệu ứng mờ dần (nếu Owl Carousel hỗ trợ animate.css)
    });

    // LOGIC SMART NAVBAR ẨN/HIỆN KHI CUỘN TRANG
    var lastScrollTop = 0;
    var navbarHeight = $('.navbar').outerHeight();

    $(window).scroll(function(){
        var st = $(this).scrollTop();
        
        // Fix lỗi trên Safari hoặc khi cuộn quá nhanh lên trên cùng
        if (st < 0) return; 

        // Nếu cuộn xuống VÀ đã cuộn qua khỏi chiều cao navbar
        if (st > lastScrollTop && st > navbarHeight){
            // Thêm class để ẩn menu
            $('.navbar').addClass('scrolled-down').removeClass('scrolled-up');
        } 
        // Nếu cuộn lên
        else {
            // Thêm class để hiện menu
            $('.navbar').removeClass('scrolled-down').addClass('scrolled-up');
        }
        
        lastScrollTop = st;
    });

    // --- XỬ LÝ SEARCH OVERLAY ---
    
    // 1. Khi bấm vào icon kính lúp trên Navbar
    $("#search-trigger").click(function(e){
        e.preventDefault(); // Chặn chuyển trang
        $("#search-overlay").addClass("active"); // Hiện overlay
        $(".search-input-transparent").focus(); // Tự động trỏ chuột vào ô nhập
    });

    // 2. Khi bấm nút Đóng (X)
    $("#search-close").click(function(){
        $("#search-overlay").removeClass("active"); // Ẩn overlay
    });

    // 3. Khi bấm phím ESC trên bàn phím thì cũng đóng luôn
    $(document).keyup(function(e) {
        if (e.key === "Escape") { 
            $("#search-overlay").removeClass("active");
        }
    });

    // --- BACK TO TOP BUTTON ---
    
    var btn = $('#back-to-top');

    // 1. Xử lý ẩn/hiện khi cuộn chuột
    $(window).scroll(function() {
        if ($(window).scrollTop() > 300) {
            btn.fadeIn(); // Hiện dần ra
        } else {
            btn.fadeOut(); // Mờ dần đi
        }
    });

    // 2. Xử lý sự kiện click
    btn.on('click', function(e) {
        e.preventDefault(); // Ngăn chặn hành động mặc định của thẻ a
        $('html, body').animate({scrollTop:0}, '300'); // Cuộn lên trong 300ms
    });

    // Kích hoạt Bootstrap Tooltips (Cho nút Tim/Share)
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })

});

// Thêm nguyên liệu
    $('#add-ingredient').click(function(){
        var html = `
            <div class="row g-2 mb-2 align-items-center ingredient-item">
                <div class="col-7 col-md-8"><input type="text" class="form-control" placeholder="Tên nguyên liệu"></div>
                <div class="col-3 col-md-3"><input type="text" class="form-control" placeholder="Số lượng"></div>
                <div class="col-2 col-md-1 text-center"><button type="button" class="btn btn-light text-danger btn-remove-row"><i class="fas fa-trash"></i></button></div>
            </div>`;
        $('#ingredient-list').append(html);
    });

    // Xóa nguyên liệu (Sử dụng Event Delegation)
    $(document).on('click', '.btn-remove-row', function(){
        $(this).closest('.ingredient-item').remove();
    });

    // Thêm bước làm
    $('#add-step').click(function(){
        var stepCount = $('.step-item').length + 1;
        var html = `
            <div class="step-item mb-4">
                <div class="d-flex justify-content-between mb-2">
                    <label class="fw-bold text-orange">Bước ${stepCount}</label>
                    <button type="button" class="btn btn-sm text-danger btn-remove-step"><i class="fas fa-times"></i> Xóa bước</button>
                </div>
                <textarea class="form-control mb-2" rows="3" placeholder="Mô tả chi tiết..."></textarea>
                <input type="file" class="form-control form-control-sm w-50" accept="image/*">
            </div>`;
        $('#step-list').append(html);
    });

    // Xóa bước làm
    $(document).on('click', '.btn-remove-step', function(){
        $(this).closest('.step-item').remove();
        // Cập nhật lại số thứ tự Bước 1, Bước 2... (Logic nâng cao, có thể làm sau)
    });

    // Chuyển đổi đơn vị nguyên liệu
    document.addEventListener('DOMContentLoaded', function() {
        const inputVal = document.getElementById('inputValue');
        const unitSelect = document.getElementById('inputUnit');
        const outputVal = document.getElementById('outputValue');

        function convert() {
            const val = parseFloat(inputVal.value) || 0;
            const unit = unitSelect.value;
            let result = 0;

            // Tỷ lệ quy đổi sang ml (ước lượng)
            switch(unit) {
                case 'cup': result = val * 240; break;
                case 'tbsp': result = val * 15; break;
                case 'tsp': result = val * 5; break;
                case 'oz': result = val * 29.57; break;
            }
            
            outputVal.value = result.toFixed(2);
        }

        inputVal.addEventListener('input', convert);
        unitSelect.addEventListener('change', convert);
        convert(); // Chạy lần đầu
    });