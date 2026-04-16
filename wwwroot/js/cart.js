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

    window.CartManager = {
        add: function(name, price, size) {
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
            return cart;
        },

        remove: function(name, size) {
            var cart = getCart();
            cart = cart.filter(function(item) {
                return !(item.name === name && item.size === size);
            });
            saveCart(cart);
            updateNavCounts();
            return cart;
        },

        clear: function() {
            saveCart([]);
            updateNavCounts();
        },

        getItems: function() {
            return getCart();
        },

        getTotal: function() {
            var cart = getCart();
            return cart.reduce(function(sum, item) {
                return sum + (item.price * item.quantity);
            }, 0);
        },

        getCount: function() {
            var cart = getCart();
            return cart.reduce(function(sum, item) {
                return sum + item.quantity;
            }, 0);
        }
    };
})();
