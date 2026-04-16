// Favorites management
(function() {
    var FAVORITES_KEY = 'favorites';

    function getFavorites() {
        return JSON.parse(localStorage.getItem(FAVORITES_KEY) || '[]');
    }

    function saveFavorites(favorites) {
        localStorage.setItem(FAVORITES_KEY, JSON.stringify(favorites));
    }

    window.addToFavorites = function(name, price, size, emoji, category) {
        var favorites = getFavorites();
        var exists = false;
        for (var i = 0; i < favorites.length; i++) {
            if (favorites[i].name === name && favorites[i].size === size) {
                exists = true;
                break;
            }
        }
        if (!exists) {
            favorites.push({
                name: name,
                size: size,
                price: price,
                emoji: emoji,
                category: category
            });
            saveFavorites(favorites);
            updateNavCounts();
        }
        return favorites;
    };

    window.FavoritesManager = {
        add: function(name, price, size, emoji, category) {
            var favorites = getFavorites();
            var exists = false;
            for (var i = 0; i < favorites.length; i++) {
                if (favorites[i].name === name && favorites[i].size === size) {
                    exists = true;
                    break;
                }
            }
            if (!exists) {
                favorites.push({
                    name: name,
                    size: size,
                    price: price,
                    emoji: emoji,
                    category: category
                });
                saveFavorites(favorites);
                updateNavCounts();
            }
            return favorites;
        },

        remove: function(name, size) {
            var favorites = getFavorites();
            favorites = favorites.filter(function(item) {
                return !(item.name === name && item.size === size);
            });
            saveFavorites(favorites);
            updateNavCounts();
            return favorites;
        },

        clear: function() {
            saveFavorites([]);
            updateNavCounts();
        },

        getItems: function() {
            return getFavorites();
        },

        isFavorite: function(name, size) {
            var favorites = getFavorites();
            for (var i = 0; i < favorites.length; i++) {
                if (favorites[i].name === name && favorites[i].size === size) {
                    return true;
                }
            }
            return false;
        }
    };
})();
