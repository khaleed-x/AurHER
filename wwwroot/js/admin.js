// ── Logout Confirmation ──
document.addEventListener('DOMContentLoaded', function () {
    var logoutForm = document.getElementById('logoutForm');

    if (logoutForm) {
        logoutForm.addEventListener('submit', function (e) {
            e.preventDefault();
            var confirmed = confirm('Are you sure you want to logout?');
            if (confirmed) {
                logoutForm.submit();
            }
        });
    }
});

(function () {
    'use strict';

    // ── Sidebar Toggle ──
    const sidebar = document.getElementById('adminSidebar');
    const mainContent = document.getElementById('mainContent');
    const toggleBtn = document.getElementById('sidebarToggle');

    if (toggleBtn && sidebar && mainContent) {
    toggleBtn.addEventListener('click', function () {
        if (window.innerWidth <= 768) {
            openMobileSidebar();
        } else {
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        }
    });

  

        // Restore sidebar state
        const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (isCollapsed) {
            sidebar.classList.add('collapsed');
            mainContent.classList.add('expanded');
        }
    }

    // ── Mobile Sidebar ──
    const overlay = document.getElementById('sidebarOverlay');

    function openMobileSidebar() {
    if (sidebar) sidebar.classList.add('mobile-open');
    if (overlay) {
        overlay.style.display = 'block';
    }
    document.body.style.overflow = 'hidden';
}

function closeMobileSidebar() {
    if (sidebar) sidebar.classList.remove('mobile-open');
    if (overlay) {
        overlay.style.display = 'none';
    }
    document.body.style.overflow = '';
}
    // ── Active Nav Item ──
    const currentPath = window.location.pathname.toLowerCase();
    const navItems = document.querySelectorAll('.nav-item');

    navItems.forEach(function (item) {
        const href = item.getAttribute('href');
        if (href && currentPath.includes(href.toLowerCase())) {
            item.classList.add('active');
        }
    });

    // ── Update Header Date ──
    const dateEl = document.getElementById('headerDate');
    if (dateEl) {
        const now = new Date();
        const options = { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' };
        dateEl.textContent = now.toLocaleDateString('en-NG', options);
    }

    // ── Number Counter Animation ──
    function animateCounter(el, target, duration, prefix, suffix) {
        prefix = prefix || '';
        suffix = suffix || '';
        duration = duration || 1000;

        const start = 0;
        const startTime = performance.now();
        const isDecimal = target % 1 !== 0;

        function update(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3); // ease out cubic
            const current = start + (target - start) * eased;

            if (isDecimal) {
                el.textContent = prefix + formatNumber(current.toFixed(2)) + suffix;
            } else {
                el.textContent = prefix + formatNumber(Math.floor(current)) + suffix;
            }

            if (progress < 1) {
                requestAnimationFrame(update);
            } else {
                if (isDecimal) {
                    el.textContent = prefix + formatNumber(parseFloat(target).toFixed(2)) + suffix;
                } else {
                    el.textContent = prefix + formatNumber(target) + suffix;
                }
            }
        }

        requestAnimationFrame(update);
    }

    function formatNumber(num) {
        return Number(num).toLocaleString('en-NG');
    }

    // Run counters on page load
    window.addEventListener('load', function () {
        const counters = document.querySelectorAll('[data-counter]');
        counters.forEach(function (el) {
            const target = parseFloat(el.getAttribute('data-counter'));
            const prefix = el.getAttribute('data-prefix') || '';
            const suffix = el.getAttribute('data-suffix') || '';
            animateCounter(el, target, 1200, prefix, suffix);
        });
    });

    // ── Expose helpers globally ──
    window.AdminPanel = {
        openMobileSidebar: openMobileSidebar,
        closeMobileSidebar: closeMobileSidebar,
        formatNumber: formatNumber
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

// Restore preference on load
var savedDarkMode = localStorage.getItem('darkMode') === 'true';
applyDarkMode(savedDarkMode);

        // Toggle on click
        if (darkModeToggle) {
            darkModeToggle.addEventListener('click', function () {
                var isDark = document.body.classList.contains('dark');
                applyDarkMode(!isDark);
                localStorage.setItem('darkMode', !isDark);
            });
        }


})();
