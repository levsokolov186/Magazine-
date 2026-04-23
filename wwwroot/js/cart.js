// Cart management
(function() {
    var CART_KEY = 'cart';

    function getCart() {
        return JSON.parse(localStorage.getItem(CART_KEY) || '[]');
    }

    function saveCart(cart) {
        localStorage.setItem(CART_KEY, JSON.stringify(cart));
    }

    window.addToCart = function(name, price, size, callback) {
        var cart = getCart();
        var existingItem = null;
        for (var i = 0; i < cart.length; i++) {
            if (cart[i].name === name && cart[i].size === size) {
                existingItem = cart[i];
                break;
            }
        }
        if (existingItem) {
            existingItem.quantity++;
        } else {
            cart.push({ name: name, price: price, size: size, quantity: 1 });
        }
        saveCart(cart);
        updateNavCounts();
        if (callback) callback(cart);
        return cart;
    };
})();
