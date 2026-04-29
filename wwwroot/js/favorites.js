(function () {
    'use strict';

    window.addToFavorites = function (name, price, size, emoji, category) {
        var favorites = window.StepStyle.readFavorites();
        for (var i = 0; i < favorites.length; i++) {
            if (favorites[i].name === name && String(favorites[i].size) === String(size)) {
                return; // already favorited
            }
        }
        favorites.push({
            name: name,
            size: size,
            price: Number(price) || 0,
            emoji: emoji || '👠',
            category: category || ''
        });
        window.StepStyle.writeFavorites(favorites);
        window.StepStyle.updateNavCounts();
    };

    window.isFavorited = function (name, size) {
        var favorites = window.StepStyle.readFavorites();
        for (var i = 0; i < favorites.length; i++) {
            if (favorites[i].name === name && String(favorites[i].size) === String(size)) {
                return true;
            }
        }
        return false;
    };
})();
