(function () {
    'use strict';

    function sameItem(a, b) {
        // Prefer matching by id when both records carry one (new entries);
        // fall back to name for entries written before id was tracked.
        if (a.id != null && b.id != null) {
            return Number(a.id) === Number(b.id) && String(a.size) === String(b.size);
        }
        return a.name === b.name && String(a.size) === String(b.size);
    }

    window.addToCart = function (id, name, price, size, emoji, callback) {
        var cart = window.StepStyle.readCart();
        var candidate = {
            id: id != null ? Number(id) : null,
            name: name,
            size: size
        };
        var existing = null;
        for (var i = 0; i < cart.length; i++) {
            if (sameItem(cart[i], candidate)) {
                existing = cart[i];
                break;
            }
        }
        if (existing) {
            existing.quantity = (Number(existing.quantity) || 0) + 1;
            if (emoji && !existing.emoji) {
                existing.emoji = emoji;
            }
            if (existing.id == null && candidate.id != null) {
                existing.id = candidate.id;
            }
        } else {
            cart.push({
                id: candidate.id,
                name: name,
                price: Number(price) || 0,
                size: size,
                emoji: emoji || '👠',
                quantity: 1
            });
        }
        window.StepStyle.writeCart(cart);
        window.StepStyle.updateNavCounts();
        if (typeof callback === 'function') callback();
    };
})();
