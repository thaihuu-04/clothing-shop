// =========================================================
// FASHION STORE - CLIENT SCRIPTS
// =========================================================

function showToast(message, isError) {
    const area = document.getElementById("toast-area");
    if (!area) return;

    const toast = document.createElement("div");
    toast.className = "shop-toast" + (isError ? " error" : "");
    toast.textContent = message;
    area.appendChild(toast);

    setTimeout(() => toast.remove(), 3500);
}

function updateCartBadge(count) {
    const badge = document.getElementById("cart-count");
    if (!badge) return;

    badge.textContent = count;
    badge.classList.toggle("d-none", count <= 0);
}

// Thêm vào giỏ bằng AJAX
document.addEventListener("submit", async (e) => {
    const form = e.target.closest(".js-add-to-cart");
    if (!form) return;

    e.preventDefault();

    const button = form.querySelector("button[type=submit]");
    if (button) button.disabled = true;

    try {
        const response = await fetch(form.action, {
            method: "POST",
            body: new FormData(form),
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        // Chưa đăng nhập => chuyển tới trang đăng nhập rồi quay lại trang hiện tại
        if (response.status === 401) {
            const returnUrl = encodeURIComponent(location.pathname + location.search);
            location.href = "/Account/Login?returnUrl=" + returnUrl;
            return;
        }

        const data = await response.json();
        showToast(data.message, !data.success);

        if (data.success) updateCartBadge(data.count);
    } catch {
        showToast("Có lỗi xảy ra, vui lòng thử lại.", true);
    } finally {
        if (button) button.disabled = false;
    }
});

// Nút tăng / giảm số lượng
document.addEventListener("click", (e) => {
    const minus = e.target.closest(".js-qty-minus");
    const plus = e.target.closest(".js-qty-plus");
    if (!minus && !plus) return;

    const box = (minus || plus).closest(".qty-box");
    const input = box.querySelector("input");
    const min = parseInt(input.min || "1");
    const max = parseInt(input.max || "99");
    let value = parseInt(input.value || "1");

    value = minus ? Math.max(min, value - 1) : Math.min(max, value + 1);
    input.value = value;
    input.dispatchEvent(new Event("change", { bubbles: true }));
});

document.addEventListener("change", (e) => {
    const input = e.target;

    // Đồng bộ số lượng cho nút "Mua ngay"
    if (input.classList.contains("js-qty-input")) {
        document.querySelectorAll(".js-buy-now-qty").forEach(h => h.value = input.value);
    }

    // Trang giỏ hàng: tự gửi form khi đổi số lượng
    if (input.classList.contains("js-auto-submit")) {
        input.form.submit();
    }
});

// Xem trước ảnh khi chọn file upload
document.addEventListener("change", (e) => {
    const input = e.target;
    if (!input.matches("input[type=file][data-preview]")) return;

    const img = document.querySelector(input.dataset.preview);
    if (img && input.files && input.files[0]) {
        img.src = URL.createObjectURL(input.files[0]);
        img.classList.remove("d-none");
    }
});

// Xác nhận trước khi thực hiện thao tác quan trọng
document.addEventListener("submit", (e) => {
    const message = e.target.dataset.confirm;
    if (message && !confirm(message)) {
        e.preventDefault();
        e.stopImmediatePropagation();
    }
}, true);
