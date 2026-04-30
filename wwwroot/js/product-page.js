(function () {
    'use strict';

    var data = window.PRODUCT_DATA || { id: null, name: '', price: 0, emoji: '👠', category: '' };
    var selectedSize = null;

    function selectSize(btn) {
        var buttons = document.querySelectorAll('#sizeSelector .btn-size');
        buttons.forEach(function (b) {
            b.classList.remove('active');
            b.setAttribute('aria-checked', 'false');
        });
        btn.classList.add('active');
        btn.setAttribute('aria-checked', 'true');
        selectedSize = btn.getAttribute('data-size');
        var err = document.getElementById('sizeError');
        var actionErr = document.getElementById('actionError');
        if (err) err.classList.add('d-none');
        if (actionErr) actionErr.classList.add('d-none');
    }

    function validateSize() {
        if (!selectedSize) {
            var err = document.getElementById('sizeError');
            var actionErr = document.getElementById('actionError');
            var selector = document.getElementById('sizeSelector');
            if (err) err.classList.remove('d-none');
            if (actionErr) actionErr.classList.remove('d-none');
            if (selector) {
                selector.classList.add('shake');
                setTimeout(function () { selector.classList.remove('shake'); }, 500);
            }
            return false;
        }
        return true;
    }

    function flashButton(btn, iconClass, text, successClass, removeClass) {
        var originalHtml = btn.innerHTML;
        var originalDisabled = btn.disabled;
        // Build flash content with textContent so user-derived strings can never be HTML.
        btn.innerHTML = '';
        var icon = document.createElement('i');
        icon.className = iconClass + ' me-2';
        icon.setAttribute('aria-hidden', 'true');
        btn.appendChild(icon);
        btn.appendChild(document.createTextNode(text));
        if (successClass) btn.classList.add(successClass);
        if (removeClass) btn.classList.remove(removeClass);
        btn.disabled = true;
        setTimeout(function () {
            btn.innerHTML = originalHtml;
            if (successClass) btn.classList.remove(successClass);
            if (removeClass) btn.classList.add(removeClass);
            btn.disabled = originalDisabled;
        }, 2000);
    }

    function onAddToCart() {
        if (!validateSize()) return;
        if (typeof window.addToCart !== 'function') return;
        window.addToCart(data.id, data.name, data.price, selectedSize, data.emoji, function () {
            var btn = document.getElementById('addToCartBtn');
            if (btn) {
                flashButton(btn, 'bi bi-check-lg', 'Добавлено! (размер ' + selectedSize + ')', 'btn-success');
            }
        });
    }

    function onAddToFav() {
        if (!validateSize()) return;
        if (typeof window.addToFavorites !== 'function') return;
        window.addToFavorites(data.id, data.name, data.price, selectedSize, data.emoji, data.category);
        var btn = document.getElementById('addToFavBtn');
        if (btn) {
            flashButton(btn, 'bi bi-check-lg', 'В избранном!', 'btn-success', 'btn-outline-danger');
        }
    }

    function init() {
        var sizeButtons = document.querySelectorAll('#sizeSelector .btn-size');
        sizeButtons.forEach(function (btn) {
            btn.addEventListener('click', function () { selectSize(btn); });
        });
        var cartBtn = document.getElementById('addToCartBtn');
        if (cartBtn) cartBtn.addEventListener('click', onAddToCart);
        var favBtn = document.getElementById('addToFavBtn');
        if (favBtn) favBtn.addEventListener('click', onAddToFav);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
