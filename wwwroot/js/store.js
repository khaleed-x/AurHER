
(function () {
    'use strict';

    // ── Mobile Nav ──
    var mobileNav = document.getElementById('mobileNav');
    var mobileMenuBtn = document.getElementById('mobileMenuBtn');
    var mobileNavClose = document.getElementById('mobileNavClose');
    var mobileNavOverlay = document.getElementById('mobileNavOverlay');

    function openMobileNav() {
        if (mobileNav) mobileNav.classList.add('open');
        document.body.style.overflow = 'hidden';
    }

    function closeMobileNav() {
        if (mobileNav) mobileNav.classList.remove('open');
        document.body.style.overflow = '';
    }

    if (mobileMenuBtn) mobileMenuBtn.addEventListener('click', openMobileNav);
    if (mobileNavClose) mobileNavClose.addEventListener('click', closeMobileNav);
    if (mobileNavOverlay) mobileNavOverlay.addEventListener('click', closeMobileNav);

    // ── Cart Badge ──
    function updateCartBadge(count) {
        var badge = document.getElementById('cartBadge');
        if (!badge) return;

        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.classList.remove('hidden');
        } else {
            badge.classList.add('hidden');
        }
    }

    // Load cart count from session on page load
    window.addEventListener('load', function () {
        fetch('/Cart/Count')
            .then(function (res) { return res.json(); })
            .then(function (data) { updateCartBadge(data.count || 0); })
            .catch(function () { }); // silent fail
    });

    // Sticky Header shadow on scroll 
    var header = document.querySelector('.site-header');
    window.addEventListener('scroll', function () {
        if (!header) return;
        if (window.scrollY > 10) {
            header.style.boxShadow = '0 4px 20px rgba(0,0,0,0.08)';
        } else {
            header.style.boxShadow = 'none';
        }
    });

    //  Active nav link 
    var currentPath = window.location.pathname.toLowerCase();
    var navLinks = document.querySelectorAll('.nav-link, .mobile-nav-link');
    navLinks.forEach(function (link) {
        var href = link.getAttribute('href');
        if (href && href !== '/' && currentPath.includes(href.toLowerCase())) {
            link.classList.add('active');
        } else if (href === '/' && currentPath === '/') {
            link.classList.add('active');
        }
    });

    //  Expose globally 
    window.StoreUI = {
        updateCartBadge: updateCartBadge,
        openMobileNav: openMobileNav,
        closeMobileNav: closeMobileNav
    };

    // ── Dark Mode ──
        var darkModeToggle = document.getElementById('darkModeToggle');
        var moonIcon = document.getElementById('moonIcon');
        var sunIcon = document.getElementById('sunIcon');

        function applyDarkMode(isDark) {
            if (isDark) {
                document.body.classList.add('dark');
                if (moonIcon) moonIcon.style.display = 'none';
                if (sunIcon) sunIcon.style.display = 'block';
            } else {
                document.body.classList.remove('dark');
                if (moonIcon) moonIcon.style.display = 'block';
                if (sunIcon) sunIcon.style.display = 'none';
            }
        }


})();
