(function () {
    'use strict';

    // ── Variant Data from page ──
    var variants = window.productVariants || [];
    var selectedColor = null;
    var selectedSize = null;
    var selectedVariant = null;

    // ── Image Gallery ──
    var mainImage = document.getElementById('mainImage');
    var thumbs = document.querySelectorAll('.gallery-thumb');

    thumbs.forEach(function (thumb) {
        thumb.addEventListener('click', function () {
            var src = this.getAttribute('data-src');
            if (mainImage && src) {
                mainImage.src = src;
                thumbs.forEach(function (t) { t.classList.remove('active'); });
                this.classList.add('active');
            }
        });
    });

    // ── Color Selection ──
    var colorSwatches = document.querySelectorAll('.color-swatch');

    colorSwatches.forEach(function (swatch) {
        swatch.addEventListener('click', function () {
            selectedColor = this.getAttribute('data-color');
            colorSwatches.forEach(function (s) { s.classList.remove('active'); });
            this.classList.add('active');

            var labelEl = document.getElementById('selectedColorLabel');
            if (labelEl) labelEl.textContent = selectedColor;

            updateSizeOptions();
            updateSelectedVariant();
        });
    });

    // ── Size Selection ──
    function updateSizeOptions() {
        var sizeBtns = document.querySelectorAll('.size-btn');
        sizeBtns.forEach(function (btn) {
            var size = btn.getAttribute('data-size');
            var matchingVariant = variants.find(function (v) {
                return v.color === selectedColor && v.size === size;
            });

            if (selectedColor && matchingVariant) {
                if (matchingVariant.stock === 0) {
                    btn.classList.add('out-of-stock');
                    btn.disabled = true;
                } else {
                    btn.classList.remove('out-of-stock');
                    btn.disabled = false;
                }
            } else if (!selectedColor) {
                btn.classList.remove('out-of-stock');
                btn.disabled = false;
            }
        });
    }

    document.querySelectorAll('.size-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            if (this.classList.contains('out-of-stock')) return;
            selectedSize = this.getAttribute('data-size');
            document.querySelectorAll('.size-btn').forEach(function (b) {
                b.classList.remove('active');
            });
            this.classList.add('active');

            var labelEl = document.getElementById('selectedSizeLabel');
            if (labelEl) labelEl.textContent = selectedSize;

            updateSelectedVariant();
        });
    });

        // ── Update Selected Variant ──
        function updateSelectedVariant() {
            if (!selectedColor || !selectedSize) {
                selectedVariant = null;
                updatePriceDisplay(null);
                updateStockDisplay(null);
                updateAddToCartBtn(false);
                return;
            }

            selectedVariant = variants.find(function (v) {
                return v.color === selectedColor && v.size === selectedSize;
            });

            updatePriceDisplay(selectedVariant);
            updateStockDisplay(selectedVariant);
            updateAddToCartBtn(selectedVariant && selectedVariant.stock > 0);

            var variantIdInput = document.getElementById('selectedVariantId');
            if (variantIdInput && selectedVariant) {
                variantIdInput.value = selectedVariant.id;
            }
        }

       // ── Update Price ──
        function updatePriceDisplay(variant) {
            var priceEl = document.getElementById('productPrice');
            if (!priceEl) return;
            if (variant) {
                priceEl.textContent = '₦' + variant.price.toLocaleString('en-NG');
            } else {
                var minPrice = Math.min.apply(null, variants.map(function (v) { return v.price; }));
                priceEl.textContent = variants.length ? 'from ₦' + minPrice.toLocaleString('en-NG') : '—';
            }
        }

        // ── Update Stock ──
        function updateStockDisplay(variant) {
            var stockEl = document.getElementById('stockInfo');
            if (!stockEl) return;
            
            stockEl.className = 'stock-info';
        
            if (!variant) {
                stockEl.innerHTML = '';
                return;
            }
           
            if (variant.availableStock <= 0 && variant.reservedStock > 0) {
                stockEl.classList.add('reserved');
                stockEl.innerHTML = `
                    <svg viewBox="0 0 24 24" width="16" height="16">
                        <circle cx="12" cy="12" r="10"/>
                        <line x1="12" y1="8" x2="12" y2="12"/>
                        <line x1="12" y1="16" x2="12.01" y2="16"/>
                    </svg>
                    <span>Only ${variant.reservedStock} left — currently reserved for another customer. Will be available in 20 minutes if not purchased.</span>
                `;
            }
          
            else if (variant.stock === 0) {
                stockEl.classList.add('out-of-stock');
                stockEl.innerHTML = '<span class="stock-info-dot"></span> Out of Stock';
            }
          
            else if (variant.stock <= 5) {
                stockEl.classList.add('low-stock');
                stockEl.innerHTML = '<span class="stock-info-dot"></span> Only ' + variant.stock + ' left!';
            }
          
            else {
                stockEl.classList.add('in-stock');
                stockEl.innerHTML = '<span class="stock-info-dot"></span> In Stock';
            }
        }
     

    // ── Update Add to Cart Button ──
    function updateAddToCartBtn(enabled) {
        var btn = document.getElementById('addToCartBtn');
        if (!btn) return;
        btn.disabled = !enabled;
        btn.textContent = enabled ? 'Add to Cart' : 'Select Size & Color';
        if (enabled) {
            btn.innerHTML = '<svg viewBox="0 0 24 24" style="width:18px;height:18px;stroke:currentColor;fill:none;stroke-width:2;stroke-linecap:round;"><path d="M6 2L3 6v14a2 2 0 002 2h14a2 2 0 002-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 01-8 0"/></svg> Add to Cart';
        }
    }



        var qtyDisplay = document.getElementById("quantityDisplay");
        var qtyHidden = document.getElementById("quantityHidden");

        document.getElementById("qtyPlus").addEventListener("click", function () {
            var val = parseInt(qtyDisplay.value);
            if (val < 100) {
                val++;
                qtyDisplay.value = val;
                qtyHidden.value = val;
            }
        });

        document.getElementById("qtyMinus").addEventListener("click", function () {
            var val = parseInt(qtyDisplay.value);
            if (val > 1) {
                val--;
                qtyDisplay.value = val;
                qtyHidden.value = val;
            }
        });



    // ── Add to Cart ──2
var addToCartForm = document.getElementById('addToCartForm');
var addToCartBtn = document.getElementById('addToCartBtn');

if (addToCartForm) {
    addToCartForm.addEventListener('submit', function (e) {
        e.preventDefault();

        if (!selectedVariant) {
            showAlert('Please select a color and size first!', 'error');
            return;
        }

        var quantity = parseInt(document.getElementById('quantityDisplay').value) || 1;
        var variantId = selectedVariant.id;

        var formData = new FormData();
        formData.append('variantId', variantId);
        formData.append('quantity', quantity);

        var token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch('/Cart/Add', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        })
        .then(function (res) { return res.json(); })
        .then(function (data) {
            if (data.success) {
                showAlert('Item added to cart!', 'success');
                if (window.StoreUI) {
                    window.StoreUI.updateCartBadge(data.cartCount);
                }
                // Reset quantity to 1 after successful add
                document.getElementById('quantityDisplay').value = 1;
                document.getElementById('quantityHidden').value = 1;
            } else {
                showAlert(data.message || 'Something went wrong', 'error');
            }
        })
        .catch(function () {
            showAlert('Something went wrong. Please try again.', 'error');
        });
    });
}

    function showAlert(message, type) {
        var alertEl = document.getElementById('cartAlert');
        if (!alertEl) return;

        alertEl.className = 'cart-alert show ' + type;
        alertEl.querySelector('span').textContent = message;

        setTimeout(function () {
            alertEl.classList.remove('show');
        }, 3000);
    }

    // Initialize price display
    updatePriceDisplay(null);

})();
