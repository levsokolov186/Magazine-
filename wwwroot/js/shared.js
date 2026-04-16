// Shared utilities
(function() {
    var productEmojis = {
        'Туфли Elegance': '👠',
        'Сапоги Winter Comfort': '👢',
        'Балетки Soft Step': '🥿',
        'Сандалии Summer Breeze': '👡',
        'Ботильоны Chelsea': '👢',
        'Кроссовки Lady Sport': '👟',
        'Лоферы Classic': '👠',
        'Сапоги-трубы Luxe': '👢'
    };

    window.formatPrice = function(price) {
        return price.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ' ') + ' ₽';
    };

    window.Utils = {
        formatPrice: function(price) {
            return price.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ' ') + ' ₽';
        },

        navigateToProduct: function(name, price, emoji, badge) {
            var url = '/Product?name=' + encodeURIComponent(name) +
                      '&price=' + price +
                      '&emoji=' + encodeURIComponent(emoji) +
                      '&badge=' + encodeURIComponent(badge || 'Новинка');
            window.location.href = url;
        },

        getProductEmoji: function(name) {
            return productEmojis[name] || '👠';
        }
    };

    window.updateNavCounts = function() {
        var cart = JSON.parse(localStorage.getItem('cart') || '[]');
        var favorites = JSON.parse(localStorage.getItem('favorites') || '[]');
        var cartCount = cart.reduce(function(sum, item) { return sum + item.quantity; }, 0);
        if ($('#cartCount').length) $('#cartCount').text(cartCount);
        if ($('#favoritesCount').length) $('#favoritesCount').text(favorites.length);
    };
})();