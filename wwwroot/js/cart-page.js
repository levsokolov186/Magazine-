(function () {
    'use strict';

    var ss = window.StepStyle;

    function buildCartItem(item, index, isLast) {
        var wrapper = document.createElement('div');
        wrapper.className = 'cart-item';

        var row = document.createElement('div');
        row.className = 'row align-items-center g-3';
        wrapper.appendChild(row);

        var iconCol = document.createElement('div');
        iconCol.className = 'col-auto';
        var iconBox = document.createElement('div');
        iconBox.className = 'cart-item-icon rounded-3 d-flex align-items-center justify-content-center';
        iconBox.setAttribute('role', 'img');
        iconBox.setAttribute('aria-label', item.name || '');
        iconBox.textContent = item.emoji || '👠';
        iconCol.appendChild(iconBox);
        row.appendChild(iconCol);

        var infoCol = document.createElement('div');
        infoCol.className = 'col';
        var title = document.createElement('h2');
        title.className = 'h5 mb-1 fw-bold';
        title.textContent = item.name || '';
        infoCol.appendChild(title);
        var sizeP = document.createElement('p');
        sizeP.className = 'text-muted mb-0';
        if (item.size) {
            sizeP.appendChild(document.createTextNode('Размер: '));
            var strong = document.createElement('strong');
            strong.textContent = String(item.size);
            sizeP.appendChild(strong);
        } else {
            sizeP.textContent = 'Размер не указан';
        }
        infoCol.appendChild(sizeP);
        row.appendChild(infoCol);

        var qtyCol = document.createElement('div');
        qtyCol.className = 'col-auto text-center';

        var qtyControl = document.createElement('div');
        qtyControl.className = 'quantity-control mb-2';

        var minusBtn = document.createElement('button');
        minusBtn.type = 'button';
        minusBtn.setAttribute('aria-label', 'Уменьшить количество');
        minusBtn.textContent = '−';
        minusBtn.addEventListener('click', function () { changeQuantity(index, -1); });
        qtyControl.appendChild(minusBtn);

        var qtySpan = document.createElement('span');
        qtySpan.setAttribute('aria-live', 'polite');
        qtySpan.textContent = String(Number(item.quantity) || 0);
        qtyControl.appendChild(qtySpan);

        var plusBtn = document.createElement('button');
        plusBtn.type = 'button';
        plusBtn.setAttribute('aria-label', 'Увеличить количество');
        plusBtn.textContent = '+';
        plusBtn.addEventListener('click', function () { changeQuantity(index, 1); });
        qtyControl.appendChild(plusBtn);

        qtyCol.appendChild(qtyControl);

        var totalP = document.createElement('p');
        totalP.className = 'fw-bold mb-1 fs-5';
        var itemTotal = (Number(item.price) || 0) * (Number(item.quantity) || 0);
        totalP.textContent = ss.formatPrice(itemTotal);
        qtyCol.appendChild(totalP);

        row.appendChild(qtyCol);

        var removeCol = document.createElement('div');
        removeCol.className = 'col-auto';
        var removeBtn = document.createElement('button');
        removeBtn.type = 'button';
        removeBtn.className = 'btn btn-sm btn-danger';
        removeBtn.title = 'Удалить';
        removeBtn.setAttribute('aria-label', 'Удалить ' + (item.name || 'товар'));
        removeBtn.innerHTML = '<i class="bi bi-trash" aria-hidden="true"></i>';
        removeBtn.addEventListener('click', function () { removeItem(index); });
        removeCol.appendChild(removeBtn);
        row.appendChild(removeCol);

        var frag = document.createDocumentFragment();
        frag.appendChild(wrapper);
        if (!isLast) {
            var hr = document.createElement('hr');
            hr.className = 'my-3';
            frag.appendChild(hr);
        }
        return frag;
    }

    function renderCart() {
        var cart = ss.readCart();
        var listEl = document.getElementById('cartList');
        var emptyEl = document.getElementById('emptyCartMessage');
        var contentEl = document.getElementById('cartContent');

        if (!cart.length) {
            emptyEl.style.display = 'block';
            contentEl.style.display = 'none';
            ss.updateNavCounts();
            return;
        }

        emptyEl.style.display = 'none';
        contentEl.style.display = 'flex';
        listEl.textContent = '';

        var totalItems = 0;
        var totalPrice = 0;
        for (var i = 0; i < cart.length; i++) {
            var item = cart[i];
            totalItems += Number(item.quantity) || 0;
            totalPrice += (Number(item.price) || 0) * (Number(item.quantity) || 0);
            listEl.appendChild(buildCartItem(item, i, i === cart.length - 1));
        }

        document.getElementById('totalItems').textContent = totalItems + ' шт.';
        document.getElementById('totalPrice').textContent = ss.formatPrice(totalPrice);
        ss.updateNavCounts();
    }

    function changeQuantity(index, delta) {
        var cart = ss.readCart();
        if (!cart[index]) return;
        cart[index].quantity = (Number(cart[index].quantity) || 0) + delta;
        if (cart[index].quantity <= 0) {
            cart.splice(index, 1);
        }
        ss.writeCart(cart);
        renderCart();
    }

    function removeItem(index) {
        var cart = ss.readCart();
        cart.splice(index, 1);
        ss.writeCart(cart);
        renderCart();
    }

    function clearCart() {
        if (!confirm('Очистить корзину?')) return;
        ss.writeCart([]);
        renderCart();
    }

    function checkout() {
        var cart = ss.readCart();
        if (!cart.length) {
            alert('Корзина пуста.');
            return;
        }
        // Real checkout/order processing is not implemented yet — make that
        // explicit instead of pretending the order was placed and clearing the cart.
        alert('Оформление заказа пока недоступно. Свяжитесь с нами, чтобы оформить покупку — корзина сохранена.');
    }

    function init() {
        var clearBtn = document.getElementById('clearCartBtn');
        if (clearBtn) clearBtn.addEventListener('click', clearCart);
        var checkoutBtn = document.getElementById('checkoutBtn');
        if (checkoutBtn) checkoutBtn.addEventListener('click', checkout);
        // Re-render when another tab edits the cart.
        window.addEventListener('storage', function (e) {
            if (e.key === 'cart') renderCart();
        });
        renderCart();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
