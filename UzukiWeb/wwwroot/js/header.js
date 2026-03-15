document.addEventListener("DOMContentLoaded", function () {
    const header = document.getElementById('main-header');
    
    // Đổi màu Header khi cuộn
    window.addEventListener('scroll', function () {
        if (window.scrollY > 50) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    });

});