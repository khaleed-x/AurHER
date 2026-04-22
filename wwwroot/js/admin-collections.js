

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {

        // ── Delete Collection Confirmation ──
        var deleteForms = document.querySelectorAll('.delete-form');
        deleteForms.forEach(function (form) {
            form.addEventListener('submit', function (e) {
                e.preventDefault();
                if (confirm('Are you sure you want to delete this collection? This cannot be undone.')) {
                    form.submit();
                }
            });
        });

        // ── Remove Product Confirmation ──
        var removeForms = document.querySelectorAll('.remove-form');
        removeForms.forEach(function (form) {
            form.addEventListener('submit', function (e) {
                e.preventDefault();
                if (confirm('Remove this product from the collection?')) {
                    form.submit();
                }
            });
        });

    });

})();

// ── Disable Add button until product selected ──
var productSelect = document.getElementById('productSelect');
var addProductBtn = document.getElementById('addProductBtn');

if (productSelect && addProductBtn) {
    productSelect.addEventListener('change', function () {
        addProductBtn.disabled = this.value === '0';
    });
}
