// wwwroot/js/site.js

document.addEventListener("DOMContentLoaded", function () {
    // 1. Xử lý Mobile Menu Toggle
    const menuBtn = document.getElementById("mobile-menu-btn");
    const mobileMenu = document.getElementById("mobile-menu");

    if (menuBtn && mobileMenu) {
        menuBtn.addEventListener("click", function () {
            // Toggle class 'hidden' để hiện/ẩn menu
            mobileMenu.classList.toggle("hidden");
        });
    }

    // (Optional) Tự động đóng menu khi click ra ngoài
});