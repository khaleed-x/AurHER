
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {

        // ── Quantity Update ──
        var qtyBtns = document.querySelectorAll('.cart-qty-btn');

        qtyBtns.forEach(function (btn) {
            btn.addEventListener('click', function () {
                var itemId = this.getAttribute('data-item-id');
                var action = this.getAttribute('data-action');
                var numEl = document.getElementById('qty-' + itemId);

                if (!numEl) return;

                var current = parseInt(numEl.textContent) || 1;
                var newQty = action === 'plus' ? current + 1 : current - 1;

                if (newQty < 1) return;

                // Submit update form
                var form = document.getElementById('update-form-' + itemId);
                if (form) {
                    form.querySelector('[name="quantity"]').value = newQty;
                    form.submit();
                }
            });
        });

        // ── Remove Confirmation ──
        var removeForms = document.querySelectorAll('.remove-form');
        removeForms.forEach(function (form) {
            form.addEventListener('submit', function (e) {
                e.preventDefault();
                if (confirm('Remove this item from your cart?')) {
                    form.submit();
                }
            });
        });

        // ── Clear Cart Confirmation ──
        var clearForm = document.getElementById('clearCartForm');
        if (clearForm) {
            clearForm.addEventListener('submit', function (e) {
                e.preventDefault();
                if (confirm('Clear all items from your cart?')) {
                    clearForm.submit();
                }
            });
        }

    });

})();
