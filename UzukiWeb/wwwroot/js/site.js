document.addEventListener("DOMContentLoaded", function () {
    // 1. CHỨC NĂNG NỀN LƠ LỬNG (PARALLAX)
    const bgElement = document.getElementById('parallax-bg');
    let ticking = false;

    if (bgElement) {
        window.addEventListener('scroll', function () {
            let scrollY = window.scrollY;
            if (!ticking) {
                window.requestAnimationFrame(function () {
                    // Di chuyển nền chậm hơn so với cuộn trang
                    let moveDistance = scrollY * 0.1;
                    bgElement.style.transform = `translateY(${moveDistance}px)`;
                    ticking = false;
                });
                ticking = true;
            }
        });
    }

    // 2. CHỨC NĂNG ĐÓNG/MỞ BANNER BÊN TRÁI
    const bannerContainer = document.getElementById('floatingBanner');
    const toggleBannerBtn = document.getElementById('toggleBannerBtn');

    if (toggleBannerBtn && bannerContainer) {
        toggleBannerBtn.addEventListener('click', function () {
            // Khi click, thêm hoặc xóa class 'closed' để kích hoạt transition CSS
            bannerContainer.classList.toggle('closed');
            console.log("Banner state toggled!"); // Log để debug trên console
        });
    }

    // 3. LOGIC CHO HEADER (Thay đổi màu khi cuộn nếu cần)
    const mainHeader = document.getElementById('main-header');
    window.addEventListener('scroll', function () {
        if (window.scrollY > 50) {
            mainHeader.classList.add('shadow-sm');
        } else {
            mainHeader.classList.remove('shadow-sm');
        }
    });
});
// 4. KÍCH HOẠT BANNER TRƯỢT (SWIPER)
const swiper = new Swiper('.myUzukiSwiper', {
    slidesPerView: 'auto', // Cho phép hiển thị nhiều slide dựa trên chiều rộng CSS
    centeredSlides: true,  // Bắt buộc slide active nằm chính giữa
    spaceBetween: 20,      // Khoảng cách giữa các slide (px)
    loop: true,            // Lặp lại vô hạn
    autoplay: {
        delay: 4000,       // Tự động chuyển sau 4 giây
        disableOnInteraction: false, // Vẫn tự động cuộn sau khi người dùng click
    },
    pagination: {
        el: '.swiper-pagination',
        clickable: true,
    },
    navigation: {
        nextEl: '.swiper-button-next',
        prevEl: '.swiper-button-prev',
    },
});
// 5. KÍCH HOẠT KỆ TRUYỆN TRƯỢT
document.querySelectorAll('.shelf-container').forEach(container => {
    const swiperEl = container.querySelector('.shelfSwiper');
    const nextBtn = container.querySelector('.shelf-next');
    const prevBtn = container.querySelector('.shelf-prev');

    new Swiper(swiperEl, {
        slidesPerView: 2.2, // Trên điện thoại hiện 2 truyện và lấp ló một chút truyện thứ 3
        spaceBetween: 15,
        navigation: {
            nextEl: nextBtn,
            prevEl: prevBtn,
        },
        breakpoints: {
            576: { slidesPerView: 3, spaceBetween: 15 },
            768: { slidesPerView: 4, spaceBetween: 20 },
            1024: { slidesPerView: 5, spaceBetween: 20 } // Trên PC hiện đủ 5 truyện
        }
    });
});
