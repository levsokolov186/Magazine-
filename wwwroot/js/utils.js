// Shared utilities for cart and favorites

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

function formatPrice(price) {
    return price.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ' ') + ' ₽';
}

function navigateToProduct(name, price, emoji, badge) {
    var url = '/Product?name=' + encodeURIComponent(name) +
              '&price=' + price +
              '&emoji=' + encodeURIComponent(emoji) +
              '&badge=' + encodeURIComponent(badge || 'Новинка');
    window.location.href = url;
}

function addToCart(name, price, size, callback) {
    var cart = JSON.parse(localStorage.getItem('cart') || '[]');
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
    localStorage.setItem('cart', JSON.stringify(cart));
    updateNavCounts();
    if (callback) callback(cart);
    return cart;
}

function addToFavorites(name, price, size, emoji, category) {
    var favorites = JSON.parse(localStorage.getItem('favorites') || '[]');
    var favItem = { name: name, size: size, price: price, emoji: emoji, category: category };
    var exists = false;
    for (var i = 0; i < favorites.length; i++) {
        if (favorites[i].name === name && favorites[i].size === size) {
            exists = true;
            break;
        }
    }
    if (!exists) {
        favorites.push(favItem);
        localStorage.setItem('favorites', JSON.stringify(favorites));
        updateNavCounts();
    }
    return favorites;
}

function updateNavCounts() {
    var cart = JSON.parse(localStorage.getItem('cart') || '[]');
    var favorites = JSON.parse(localStorage.getItem('favorites') || '[]');
    var cartCount = cart.reduce(function(sum, item) { return sum + item.quantity; }, 0);
    if ($('#cartCount').length) $('#cartCount').text(cartCount);
    if ($('#favoritesCount').length) $('#favoritesCount').text(favorites.length);
}
