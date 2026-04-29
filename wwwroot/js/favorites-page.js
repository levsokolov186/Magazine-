(function () {
    'use strict';

    var ss = window.StepStyle;

    function buildFavoritesCard(name, group) {
        var col = document.createElement('div');
        col.className = 'col-md-6 col-lg-4';

        var card = document.createElement('div');
        card.className = 'product-card';
        col.appendChild(card);

        var imageBox = document.createElement('div');
        imageBox.className = 'product-image';
        card.appendChild(imageBox);

        var placeholder = document.createElement('div');
        placeholder.className = 'placeholder-img';
        placeholder.setAttribute('role', 'img');
        placeholder.setAttribute('aria-label', name);
        var emojiSpan = document.createElement('span');
        emojiSpan.textContent = group.emoji || '👠';
        placeholder.appendChild(emojiSpan);
        imageBox.appendChild(placeholder);

        var removeBtn = document.createElement('button');
        removeBtn.type = 'button';
        removeBtn.className = 'btn-favorite-remove';
        removeBtn.title = 'Удалить из избранного';
        removeBtn.setAttribute('aria-label', 'Удалить ' + name + ' из избранного');
        removeBtn.innerHTML = '<i class="bi bi-x-lg" aria-hidden="true"></i>';
        removeBtn.addEventListener('click', function (event) {
            event.stopPropagation();
            event.preventDefault();
            removeFavoriteByName(name);
        });
        imageBox.appendChild(removeBtn);

        var info = document.createElement('div');
        info.className = 'product-info';
        card.appendChild(info);

        var title = document.createElement('h2');
        title.className = 'product-name h5';
        title.textContent = name;
        info.appendChild(title);

        var sizesP = document.createElement('p');
        sizesP.className = 'product-category';
        sizesP.appendChild(document.createTextNode('Размеры: '));
        var strong = document.createElement('strong');
        var sortedSizes = group.items
            .map(function (i) { return Number(i.size); })
            .filter(function (s) { return !isNaN(s); })
            .sort(function (a, b) { return a - b; })
            .map(function (s) { return String(s); });
        if (!sortedSizes.length) {
            sortedSizes = group.items.map(function (i) { return String(i.size); });
        }
        strong.textContent = sortedSizes.join(', ');
        sizesP.appendChild(strong);
        info.appendChild(sizesP);

        var firstItem = group.items[0];
        var bottom = document.createElement('div');
        bottom.className = 'd-flex justify-content-between align-items-center mt-auto';
        info.appendChild(bottom);

        var priceSpan = document.createElement('span');
        priceSpan.className = 'product-price';
        priceSpan.textContent = ss.formatPrice(firstItem.price);
        bottom.appendChild(priceSpan);

        var cartBtn = document.createElement('button');
        cartBtn.type = 'button';
        cartBtn.className = 'btn btn-cart';
        cartBtn.innerHTML = '<i class="bi bi-cart-plus" aria-hidden="true"></i> В корзину';
        cartBtn.addEventListener('click', function (event) {
            event.stopPropagation();
            event.preventDefault();
            addToCartFromFavorites(name, firstItem.price, firstItem.size, group.emoji);
        });
        bottom.appendChild(cartBtn);

        // Make the whole card open the product (best-effort: we don't know the product id from
        // localStorage, so we link by name search via the catalog page).
        card.style.cursor = 'default';

        return col;
    }

    function removeFavoriteByName(name) {
        var favorites = ss.readFavorites().filter(function (item) {
            return item.name !== name;
        });
        ss.writeFavorites(favorites);
        renderFavorites();
    }

    function clearAllFavorites() {
        if (!confirm('Удалить всё из избранного?')) return;
        ss.writeFavorites([]);
        renderFavorites();
    }

    function addToCartFromFavorites(name, price, size, emoji) {
        if (typeof window.addToCart !== 'function') return;
        window.addToCart(name, price, size, emoji, function () {
            showToast(name + ' (размер ' + size + ') добавлен в корзину!');
        });
    }

    function showToast(message) {
        var container = document.getElementById('toastContainer');
        if (!container) return;
        var toast = document.createElement('div');
        toast.className = 'favorites-toast';
        toast.setAttribute('role', 'status');
        var icon = document.createElement('i');
        icon.className = 'bi bi-check-circle-fill text-success me-2';
        icon.setAttribute('aria-hidden', 'true');
        toast.appendChild(icon);
        toast.appendChild(document.createTextNode(message));
        container.appendChild(toast);
        setTimeout(function () {
            toast.classList.add('fading-out');
            setTimeout(function () {
                if (toast.parentNode) toast.parentNode.removeChild(toast);
            }, 300);
        }, 2200);
    }

    function renderFavorites() {
        var favorites = ss.readFavorites();
        var emptyEl = document.getElementById('emptyFavoritesMessage');
        var contentEl = document.getElementById('favoritesContent');
        var clearBtn = document.getElementById('clearFavoritesBtn');

        if (!favorites.length) {
            emptyEl.style.display = 'block';
            contentEl.style.display = 'none';
            if (clearBtn) clearBtn.classList.add('d-none');
            ss.updateNavCounts();
            return;
        }

        emptyEl.style.display = 'none';
        contentEl.style.display = 'flex';
        if (clearBtn) clearBtn.classList.remove('d-none');
        contentEl.textContent = '';

        var grouped = Object.create(null);
        var order = [];
        for (var i = 0; i < favorites.length; i++) {
            var item = favorites[i];
            if (!grouped[item.name]) {
                grouped[item.name] = { items: [], emoji: item.emoji || '👠' };
                order.push(item.name);
            }
            grouped[item.name].items.push({
                size: item.size,
                price: Number(item.price) || 0
            });
        }

        for (var k = 0; k < order.length; k++) {
            contentEl.appendChild(buildFavoritesCard(order[k], grouped[order[k]]));
        }

        ss.updateNavCounts();
    }

    function init() {
        var clearBtn = document.getElementById('clearFavoritesBtn');
        if (clearBtn) clearBtn.addEventListener('click', clearAllFavorites);
        renderFavorites();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
