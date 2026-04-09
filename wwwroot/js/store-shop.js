
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {

        // ── Search with debounce ──
        var searchInput = document.getElementById('shopSearch');
        var searchTimer;

        if (searchInput) {
            searchInput.addEventListener('input', function () {
                clearTimeout(searchTimer);
                var query = this.value.trim();

                searchTimer = setTimeout(function () {
                    var url = new URL(window.location.href);
                    if (query) {
                        url.searchParams.set('search', query);
                    } else {
                        url.searchParams.delete('search');
                    }
                    window.location.href = url.toString();
                }, 600);
            });
        }

        // ── Sort change ──
        var sortSelect = document.getElementById('sortSelect');
        if (sortSelect) {
            sortSelect.addEventListener('change', function () {
                var url = new URL(window.location.href);
                if (this.value) {
                    url.searchParams.set('sort', this.value);
                } else {
                    url.searchParams.delete('sort');
                }
                window.location.href = url.toString();
            });
        }

        // ── Mobile filter toggle ──
        var filterBtn = document.getElementById('mobileFilterBtn');
        var filterSidebar = document.getElementById('filtersSidebar');
        var filterClose = document.getElementById('filterClose');

        if (filterBtn && filterSidebar) {
            filterBtn.addEventListener('click', function () {
                filterSidebar.classList.add('mobile-open');
                document.body.style.overflow = 'hidden';
            });
        }

        if (filterClose && filterSidebar) {
            filterClose.addEventListener('click', function () {
                filterSidebar.classList.remove('mobile-open');
                document.body.style.overflow = '';
            });
        }

    });

})();
