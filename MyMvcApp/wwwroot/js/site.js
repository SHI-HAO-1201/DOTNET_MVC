(() => {
    const CART_KEY = "motospeed-cart";
    const SAVED_KEY = "motospeed-saved";
    const readCart = () => {
        try {
            const value = JSON.parse(localStorage.getItem(CART_KEY) || "[]");
            return Array.isArray(value) ? value : [];
        } catch {
            return [];
        }
    };
    const saveCart = (cart) => {
        localStorage.setItem(CART_KEY, JSON.stringify(cart));
        updateCartCount(cart);
    };
    const formatPrice = (price) => `${Number(price).toLocaleString("vi-VN")} ₫`;
    const updateCartCount = (cart = readCart()) => {
        const count = cart.reduce((total, item) => total + item.quantity, 0);
        document.querySelectorAll("#cart-count").forEach((badge) => badge.textContent = String(count));
    };
    const readSaved = () => {
        try {
            const value = JSON.parse(localStorage.getItem(SAVED_KEY) || "[]");
            return Array.isArray(value) ? value : [];
        } catch {
            return [];
        }
    };
    const saveSaved = (saved) => localStorage.setItem(SAVED_KEY, JSON.stringify(saved));
    const renderSaved = () => {
        const container = document.querySelector("#saved-items");
        if (!container) return;
        const saved = readSaved();
        container.replaceChildren();
        document.querySelector("#empty-saved")?.classList.toggle("d-none", saved.length > 0);
        saved.forEach((item) => {
            const column = document.createElement("div");
            column.className = "col-md-6 col-xl-4";
            const card = document.createElement("article");
            card.className = "product-card";
            const link = document.createElement("a");
            link.className = "product-image";
            link.href = `/Motor/Details/${encodeURIComponent(item.id)}`;
            const image = document.createElement("img");
            image.src = item.image;
            image.alt = item.name;
            link.append(image);
            const info = document.createElement("div");
            info.className = "product-info";
            const name = document.createElement("h3");
            name.textContent = item.name;
            const bottom = document.createElement("div");
            bottom.className = "product-bottom";
            const price = document.createElement("strong");
            price.textContent = formatPrice(item.price);
            const remove = document.createElement("button");
            remove.className = "add-cart";
            remove.type = "button";
            remove.dataset.unsave = item.id;
            remove.setAttribute("aria-label", `Xóa ${item.name} khỏi danh sách yêu thích`);
            remove.innerHTML = '<i class="bi bi-heartbreak"></i>';
            bottom.append(price, remove);
            info.append(name, bottom);
            card.append(link, info);
            column.append(card);
            container.append(column);
        });
    };
    const addFavoriteButtons = () => {
        document.querySelectorAll(".product-card").forEach((card) => {
            if (card.querySelector(".favorite-toggle")) return;
            const detailsLink = card.querySelector('a[href*="Details"]');
            const image = card.querySelector(".product-image img");
            const name = card.querySelector("h2, h3")?.textContent.trim();
            const rawPrice = card.querySelector(".product-bottom strong")?.textContent || "";
            const id = detailsLink?.getAttribute("href")?.match(/(?:Details\/|id=)(\d+)/i)?.[1];
            if (!id || !image || !name) return;
            const button = document.createElement("button");
            button.type = "button";
            button.className = "favorite-toggle";
            button.dataset.favoriteId = id;
            button.dataset.favoriteName = name;
            button.dataset.favoritePrice = rawPrice.replace(/[^0-9]/g, "");
            button.dataset.favoriteImage = image.src;
            button.setAttribute("aria-label", `Lưu ${name} vào xe yêu thích`);
            button.innerHTML = '<i class="bi bi-heart"></i>';
            card.append(button);
        });
        const detailImage = document.querySelector(".detail-image");
        const detailName = document.querySelector(".detail-title")?.textContent.trim();
        if (detailImage && detailName && !detailImage.querySelector(".favorite-toggle")) {
            const id = window.location.pathname.match(/(\d+)\/?$/)?.[1];
            const image = detailImage.querySelector("img");
            const price = document.querySelector(".detail-price")?.textContent || "";
            if (id && image) {
                const button = document.createElement("button");
                button.type = "button";
                button.className = "favorite-toggle";
                button.dataset.favoriteId = id;
                button.dataset.favoriteName = detailName;
                button.dataset.favoritePrice = price.replace(/[^0-9]/g, "");
                button.dataset.favoriteImage = image.src;
                button.setAttribute("aria-label", `Lưu ${detailName} vào xe yêu thích`);
                button.innerHTML = '<i class="bi bi-heart"></i>';
                detailImage.append(button);
            }
        }
        syncFavoriteButtons();
    };
    const syncFavoriteButtons = () => {
        const savedIds = new Set(readSaved().map((item) => String(item.id)));
        document.querySelectorAll(".favorite-toggle").forEach((button) => {
            const isSaved = savedIds.has(button.dataset.favoriteId);
            button.classList.toggle("is-saved", isSaved);
            button.setAttribute("aria-pressed", String(isSaved));
            button.setAttribute("aria-label", `${isSaved ? "Xóa khỏi" : "Lưu vào"} xe yêu thích: ${button.dataset.favoriteName}`);
            button.innerHTML = `<i class="bi ${isSaved ? "bi-heart-fill" : "bi-heart"}"></i>`;
        });
    };
    const renderCart = () => {
        const cart = readCart();
        const itemsContainer = document.querySelector("#cart-items");
        const cartContent = document.querySelector("#cart-content");
        const emptyCart = document.querySelector("#empty-cart");
        const checkoutItems = document.querySelector("#checkout-items");
        const total = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);

        if (itemsContainer) {
            itemsContainer.replaceChildren();
            cart.forEach((item) => {
                const row = document.createElement("div");
                row.className = "cart-row";
                const image = document.createElement("img");
                image.src = item.image;
                image.alt = item.name;
                const details = document.createElement("div");
                const name = document.createElement("h3");
                name.textContent = item.name;
                const quantity = document.createElement("span");
                quantity.className = "text-secondary small";
                quantity.textContent = `Số lượng: ${item.quantity}`;
                const quantityControls = document.createElement("span");
                quantityControls.className = "cart-quantity-control";
                const decrease = document.createElement("button");
                decrease.type = "button";
                decrease.dataset.quantityId = item.id;
                decrease.dataset.quantityDelta = "-1";
                decrease.setAttribute("aria-label", `Giảm số lượng ${item.name}`);
                decrease.textContent = "−";
                const increase = document.createElement("button");
                increase.type = "button";
                increase.dataset.quantityId = item.id;
                increase.dataset.quantityDelta = "1";
                increase.setAttribute("aria-label", `Tăng số lượng ${item.name}`);
                increase.textContent = "+";
                quantityControls.append(decrease, increase);
                const price = document.createElement("div");
                price.className = "cart-price";
                price.textContent = formatPrice(item.price * item.quantity);
                details.append(name, quantity, quantityControls, price);
                const remove = document.createElement("button");
                remove.type = "button";
                remove.dataset.remove = item.id;
                remove.setAttribute("aria-label", `Xóa ${item.name}`);
                remove.innerHTML = '<i class="bi bi-trash"></i>';
                row.append(image, details, remove);
                itemsContainer.append(row);
            });
            document.querySelector("#cart-subtotal").textContent = formatPrice(total);
            document.querySelector("#cart-total").textContent = formatPrice(total);
            cartContent?.classList.toggle("d-none", cart.length === 0);
            emptyCart?.classList.toggle("d-none", cart.length > 0);
        }

        if (checkoutItems) {
            checkoutItems.replaceChildren();
            cart.forEach((item) => {
                const row = document.createElement("div");
                row.className = "checkout-item";
                const title = document.createElement("span");
                title.textContent = `${item.name} × ${item.quantity}`;
                const price = document.createElement("strong");
                price.textContent = formatPrice(item.price * item.quantity);
                row.append(title, price);
                checkoutItems.append(row);
            });
            document.querySelector("#checkout-total").textContent = formatPrice(total);
        }
    };

    document.querySelector("[data-checkout-form]")?.addEventListener("submit", (event) => {
        const cart = readCart();
        if (cart.length === 0) {
            event.preventDefault();
            window.alert("Giỏ hàng đang trống. Hãy thêm xe trước khi đặt hàng.");
            return;
        }

        document.querySelector("#checkout-cart-json").value = JSON.stringify(
            cart.map((item) => ({ id: String(item.id), quantity: Number(item.quantity) }))
        );
    });

    document.addEventListener("click", (event) => {
        const addButton = event.target.closest(".add-cart[data-id]");
        if (addButton) {
            event.preventDefault();
            const cart = readCart();
            const id = addButton.dataset.id;
            const existingItem = cart.find((item) => item.id === id);
            if (existingItem) {
                existingItem.quantity += 1;
            } else {
                cart.push({
                    id,
                    name: addButton.dataset.name,
                    price: Number(addButton.dataset.price),
                    image: addButton.dataset.image,
                    quantity: 1
                });
            }
            saveCart(cart);
            addButton.innerHTML = '<i class="bi bi-check2"></i>';
            window.setTimeout(() => addButton.innerHTML = '<i class="bi bi-bag-plus"></i>', 1200);
        }

        const removeButton = event.target.closest("[data-remove]");
        if (removeButton) {
            saveCart(readCart().filter((item) => item.id !== removeButton.dataset.remove));
            renderCart();
        }

        const quantityButton = event.target.closest("[data-quantity-id]");
        if (quantityButton) {
            const cart = readCart();
            const item = cart.find((cartItem) => String(cartItem.id) === quantityButton.dataset.quantityId);
            const delta = Number.parseInt(quantityButton.dataset.quantityDelta, 10);
            if (item && Number.isFinite(delta)) {
                item.quantity = Math.max(1, Math.min(99, Number(item.quantity) + delta));
                saveCart(cart);
                renderCart();
            }
        }

        const favoriteButton = event.target.closest(".favorite-toggle");
        if (favoriteButton) {
            const saved = readSaved();
            const id = favoriteButton.dataset.favoriteId;
            const existing = saved.some((item) => String(item.id) === id);
            saveSaved(existing ? saved.filter((item) => String(item.id) !== id) : [...saved, {
                id,
                name: favoriteButton.dataset.favoriteName,
                price: Number(favoriteButton.dataset.favoritePrice),
                image: favoriteButton.dataset.favoriteImage
            }]);
            syncFavoriteButtons();
            renderSaved();
        }

        const unsaveButton = event.target.closest("[data-unsave]");
        if (unsaveButton) {
            saveSaved(readSaved().filter((item) => String(item.id) !== unsaveButton.dataset.unsave));
            renderSaved();
        }

        if (event.target.closest("#clear-cart")) {
            saveCart([]);
            renderCart();
        }
    });

    updateCartCount();
    renderCart();
    addFavoriteButtons();
    renderSaved();
})();
