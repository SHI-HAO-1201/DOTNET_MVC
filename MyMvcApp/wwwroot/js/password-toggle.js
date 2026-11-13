document.addEventListener("click", event => {
    const button = event.target.closest("[data-password-toggle]");
    if (!button) return;

    const input = document.querySelector(button.dataset.passwordToggle);
    if (!(input instanceof HTMLInputElement)) return;

    const isVisible = input.type === "text";
    input.type = isVisible ? "password" : "text";
    button.textContent = isVisible ? "Hiện" : "Ẩn";
    button.setAttribute("aria-label", isVisible ? "Hiện mật khẩu" : "Ẩn mật khẩu");
    button.setAttribute("aria-pressed", String(!isVisible));
});
