
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {

        // ── Live Search ──
        var searchInput = document.getElementById('inventorySearch');
        var tableRows = document.querySelectorAll('.inventory-row');

        if (searchInput) {
            searchInput.addEventListener('input', function () {
                var query = this.value.toLowerCase().trim();

                tableRows.forEach(function (row) {
                    var text = row.getAttribute('data-search') || '';
                    if (text.toLowerCase().includes(query)) {
                        row.style.display = '';
                    } else {
                        row.style.display = 'none';
                    }
                });

                // Update visible count
                var visibleCount = document.querySelectorAll('.inventory-row:not([style*="display: none"])').length;
                var countEl = document.getElementById('visibleCount');
                if (countEl) countEl.textContent = visibleCount;
            });
        }

    });

})();
