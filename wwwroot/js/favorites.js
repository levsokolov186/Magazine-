(function () {
    'use strict';

    function sameItem(a, b) {
        if (a.id != null && b.id != null) {
            return Number(a.id) === Number(b.id) && String(a.size) === String(b.size);
        }
        return a.name === b.name && String(a.size) === String(b.size);
    }

    window.addToFavorites = function (id, name, price, size, emoji, category) {
        var favorites = window.StepStyle.readFavorites();
        var candidate = {
            id: id != null ? Number(id) : null,
            name: name,
            size: size
        };
        for (var i = 0; i < favorites.length; i++) {
            if (sameItem(favorites[i], candidate)) {
                return; // already favorited
            }
        }
        favorites.push({
            id: candidate.id,
            name: name,
            size: size,
            price: Number(price) || 0,
            emoji: emoji || '👠',
            category: category || ''
        });
        window.StepStyle.writeFavorites(favorites);
        window.StepStyle.updateNavCounts();
    };

    window.isFavorited = function (id, name, size) {
        var favorites = window.StepStyle.readFavorites();
        var candidate = {
            id: id != null ? Number(id) : null,
            name: name,
            size: size
        };
        for (var i = 0; i < favorites.length; i++) {
            if (sameItem(favorites[i], candidate)) {
                return true;
            }
        }
        return false;
    };
})();
