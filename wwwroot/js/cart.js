(function () {
    'use strict';

    window.addToCart = function (name, price, size, emoji, callback) {
        var cart = window.StepStyle.readCart();
        var existing = null;
        for (var i = 0; i < cart.length; i++) {
            if (cart[i].name === name && String(cart[i].size) === String(size)) {
                existing = cart[i];
                break;
            }
        }
        if (existing) {
            existing.quantity = (Number(existing.quantity) || 0) + 1;
            if (emoji && !existing.emoji) {
                existing.emoji = emoji;
            }
        } else {
            cart.push({
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
