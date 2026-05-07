(function () {
    'use strict';

    function bindText(inputId, targetId, fallback) {
        var input = document.getElementById(inputId);
        var target = document.getElementById(targetId);
        if (!input || !target) return;
        input.addEventListener('input', function () {
            target.textContent = input.value || fallback;
        });
    }

    function init() {
        bindText('Product_Name', 'previewName', 'Название товара');
        bindText('Product_Category', 'previewCategory', 'Категория');
        bindText('Product_Emoji', 'previewEmoji', '👠');

        var priceInput = document.getElementById('Product_Price');
        var priceTarget = document.getElementById('previewPrice');
        if (priceInput && priceTarget) {
            priceInput.addEventListener('input', function () {
                var price = parseFloat(priceInput.value);
                if (isNaN(price)) price = 0;
                priceTarget.textContent = window.formatPrice ? window.formatPrice(price) : (price + ' ₽');
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
