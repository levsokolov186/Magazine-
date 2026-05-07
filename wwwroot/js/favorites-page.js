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
            removeFavoriteGroup(group);
        });
        imageBox.appendChild(removeBtn);

        var info = document.createElement('div');
        info.className = 'product-info';
        card.appendChild(info);

        var title = document.createElement('h2');
        title.className = 'product-name h5';
        title.textContent = name;
        info.appendChild(title);

        var sortedItems = group.items.slice().sort(function (a, b) {
            var na = Number(a.size), nb = Number(b.size);
            if (!isNaN(na) && !isNaN(nb)) return na - nb;
            return String(a.size).localeCompare(String(b.size));
        });

        var sizesP = document.createElement('p');
        sizesP.className = 'product-category mb-2';
        sizesP.textContent = 'Выберите размер для добавления в корзину:';
        info.appendChild(sizesP);

        var sizeGroup = document.createElement('div');
        sizeGroup.className = 'd-flex flex-wrap gap-2 mb-3';
        sizeGroup.setAttribute('role', 'radiogroup');
        sizeGroup.setAttribute('aria-label', 'Размер для ' + name);
        var selectedSize = sortedItems.length ? String(sortedItems[0].size) : null;

        sortedItems.forEach(function (it, idx) {
            var sBtn = document.createElement('button');
            sBtn.type = 'button';
            sBtn.className = 'btn btn-sm btn-outline-secondary';
            if (idx === 0) sBtn.classList.add('active');
            sBtn.setAttribute('role', 'radio');
            sBtn.setAttribute('aria-checked', idx === 0 ? 'true' : 'false');
            sBtn.textContent = String(it.size);
            sBtn.addEventListener('click', function () {
                selectedSize = String(it.size);
                Array.prototype.forEach.call(sizeGroup.children, function (child) {
                    child.classList.remove('active');
                    child.setAttribute('aria-checked', 'false');
                });
                sBtn.classList.add('active');
                sBtn.setAttribute('aria-checked', 'true');
            });
            sizeGroup.appendChild(sBtn);
        });
        info.appendChild(sizeGroup);

        var firstItem = sortedItems[0];
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
            var match = null;
            for (var i = 0; i < sortedItems.length; i++) {
                if (String(sortedItems[i].size) === String(selectedSize)) {
                    match = sortedItems[i];
                    break;
                }
            }
            if (!match) match = firstItem;
            addToCartFromFavorites(match.id, name, match.price, match.size, group.emoji);
        });
        bottom.appendChild(cartBtn);

        card.style.cursor = 'default';

        return col;
    }

    function removeFavoriteGroup(group) {
        var ids = {};
        var names = {};
        group.items.forEach(function (it) {
            if (it.id != null) ids[Number(it.id)] = true;
            else names[String(group.name)] = true;
        });
        var favorites = ss.readFavorites().filter(function (item) {
            if (item.id != null && ids[Number(item.id)]) return false;
            if (item.id == null && names[String(item.name)]) return false;
            return true;
        });
        ss.writeFavorites(favorites);
        ss.updateNavCounts();
        renderFavorites();
    }

    function clearAllFavorites() {
        if (!confirm('Удалить всё из избранного?')) return;
        ss.writeFavorites([]);
        ss.updateNavCounts();
        renderFavorites();
    }

    function addToCartFromFavorites(id, name, price, size, emoji) {
        if (typeof window.addToCart !== 'function') return;
        window.addToCart(id, name, price, size, emoji, function () {
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
                grouped[item.name] = { name: item.name, items: [], emoji: item.emoji || '👠' };
                order.push(item.name);
            }
            grouped[item.name].items.push({
                id: item.id != null ? Number(item.id) : null,
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
        // Cross-tab sync: re-render when another tab updates favorites or cart.
        window.addEventListener('storage', function (e) {
            if (e.key === 'favorites' || e.key === 'cart') renderFavorites();
        });
        renderFavorites();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
