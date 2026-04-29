(function () {
    'use strict';

    var data = window.PRODUCT_DATA || { name: '', price: 0, emoji: '👠', category: '' };
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

    function flashButton(btn, html, successClass, removeClass) {
        var original = btn.innerHTML;
        btn.innerHTML = html;
        if (successClass) btn.classList.add(successClass);
        if (removeClass) btn.classList.remove(removeClass);
        setTimeout(function () {
            btn.innerHTML = original;
            if (successClass) btn.classList.remove(successClass);
            if (removeClass) btn.classList.add(removeClass);
        }, 2000);
    }

    function onAddToCart() {
        if (!validateSize()) return;
        if (typeof window.addToCart !== 'function') return;
        window.addToCart(data.name, data.price, selectedSize, data.emoji, function () {
            var btn = document.getElementById('addToCartBtn');
            if (btn) {
                flashButton(
                    btn,
                    '<i class="bi bi-check-lg me-2" aria-hidden="true"></i>Добавлено! (размер ' + selectedSize + ')',
                    'btn-success'
                );
            }
        });
    }

    function onAddToFav() {
        if (!validateSize()) return;
        if (typeof window.addToFavorites !== 'function') return;
        window.addToFavorites(data.name, data.price, selectedSize, data.emoji, data.category);
        var btn = document.getElementById('addToFavBtn');
        if (btn) {
            flashButton(
                btn,
                '<i class="bi bi-check-lg me-2" aria-hidden="true"></i>В избранном!',
                'btn-success',
                'btn-outline-danger'
            );
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
