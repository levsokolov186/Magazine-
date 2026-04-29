(function () {
    'use strict';

    function safeParse(key) {
        try {
            return JSON.parse(localStorage.getItem(key) || '[]');
        } catch (err) {
            console.error('Failed to parse ' + key + ' from localStorage', err);
            return [];
        }
    }

    function escapeHtml(value) {
        if (value === null || value === undefined) return '';
        var str = String(value);
        return str.replace(/[&<>"']/g, function (ch) {
            switch (ch) {
                case '&': return '&amp;';
                case '<': return '&lt;';
                case '>': return '&gt;';
                case '"': return '&quot;';
                case "'": return '&#39;';
            }
            return ch;
        });
    }

    function formatPrice(price) {
        var n = Number(price) || 0;
        // Always render with non-breaking space as thousand separator for consistency.
        var rounded = Math.round(n * 100) / 100;
        var parts = rounded.toString().split('.');
        parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, '\u00A0');
        return parts.join('.') + '\u00A0₽';
    }

    function readCart() { return safeParse('cart'); }
    function readFavorites() { return safeParse('favorites'); }

    function writeCart(cart) {
        try {
            localStorage.setItem('cart', JSON.stringify(cart));
        } catch (err) {
            console.error('Failed to save cart', err);
        }
    }

    function writeFavorites(favorites) {
        try {
            localStorage.setItem('favorites', JSON.stringify(favorites));
        } catch (err) {
            console.error('Failed to save favorites', err);
        }
    }

    function updateNavCounts() {
        var cart = readCart();
        var favorites = readFavorites();
        var cartCount = cart.reduce(function (sum, item) {
            return sum + (Number(item && item.quantity) || 0);
        }, 0);
        var cartEl = document.getElementById('cartCount');
        var favEl = document.getElementById('favoritesCount');
        if (cartEl) cartEl.textContent = String(cartCount);
        if (favEl) favEl.textContent = String(favorites.length);
    }

    window.StepStyle = {
        readCart: readCart,
        writeCart: writeCart,
        readFavorites: readFavorites,
        writeFavorites: writeFavorites,
        formatPrice: formatPrice,
        escapeHtml: escapeHtml,
        updateNavCounts: updateNavCounts
    };

    // Backwards-compatible global helpers used in pages.
    window.formatPrice = formatPrice;
    window.escapeHtml = escapeHtml;
    window.updateNavCounts = updateNavCounts;

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', updateNavCounts);
    } else {
        updateNavCounts();
    }
})();
