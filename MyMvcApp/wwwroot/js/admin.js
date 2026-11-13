(() => {
    const sidebar = document.querySelector("#admin-sidebar");
    const backdrop = document.querySelector("#admin-sidebar-backdrop");
    const menuToggle = document.querySelector("#admin-menu-toggle");
    const toast = document.querySelector("#admin-toast");
    let toastTimeout;

    const closeSidebar = () => {
        sidebar?.classList.remove("open");
        backdrop?.classList.remove("show");
        menuToggle?.setAttribute("aria-expanded", "false");
    };

    menuToggle?.addEventListener("click", () => {
        const isOpen = sidebar?.classList.toggle("open") ?? false;
        backdrop?.classList.toggle("show", isOpen);
        menuToggle.setAttribute("aria-expanded", String(isOpen));
    });
    backdrop?.addEventListener("click", closeSidebar);
    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") closeSidebar();
    });

    const showToast = (message) => {
        if (!toast) return;
        toast.textContent = message;
        toast.classList.add("show");
        window.clearTimeout(toastTimeout);
        toastTimeout = window.setTimeout(() => toast.classList.remove("show"), 3500);
    };

    document.addEventListener("click", (event) => {
        if (event.target.closest("[data-reload-page]")) {
            window.location.reload();
        }

        const action = event.target.closest("[data-demo-action]");
        if (action) {
            event.preventDefault();
            showToast(action.dataset.demoAction);
        }

        const selectAll = event.target.closest("[data-select-all]");
        if (selectAll) {
            const table = document.querySelector(selectAll.dataset.selectAll);
            table?.querySelectorAll('tbody input[type="checkbox"]').forEach((checkbox) => {
                checkbox.checked = selectAll.checked;
            });
        }

        const statusTab = event.target.closest("[data-status-filter]");
        if (statusTab) {
            document.querySelectorAll("[data-status-filter]").forEach((tab) => tab.classList.toggle("active", tab === statusTab));
            const status = statusTab.dataset.statusFilter;
            document.querySelectorAll("#orders-table tbody tr[data-order-status]").forEach((row) => {
                row.classList.toggle("d-none", status !== "all" && row.dataset.orderStatus !== status);
            });
        }

        const filterTab = event.target.closest(".admin-filter-tabs button:not([data-status-filter])");
        if (filterTab) {
            filterTab.parentElement.querySelectorAll("button").forEach((tab) => tab.classList.toggle("active", tab === filterTab));
            showToast("Bộ lọc đang dùng dữ liệu minh họa.");
        }

        const pageButton = event.target.closest(".admin-pagination button:not(:disabled)");
        if (pageButton) {
            pageButton.parentElement.querySelectorAll("button").forEach((button) => button.classList.remove("selected"));
            pageButton.classList.add("selected");
            showToast("Phân trang cần kết nối dữ liệu thật.");
        }
    });

    document.querySelectorAll("[data-table-search]").forEach((input) => {
        input.addEventListener("input", () => {
            const table = document.querySelector(input.dataset.tableSearch);
            const query = input.value.trim().toLocaleLowerCase("vi");
            table?.querySelectorAll("tbody tr").forEach((row) => {
                row.classList.toggle("d-none", !row.textContent.toLocaleLowerCase("vi").includes(query));
            });
        });
    });

    document.querySelectorAll(".admin-toolbar select").forEach((select) => {
        select.addEventListener("change", () => {
            const table = select.closest(".admin-panel")?.querySelector(".admin-table");
            if (!table) return;
            const value = select.value.toLocaleLowerCase("vi");
            table.querySelectorAll("tbody tr").forEach((row) => {
                const matches = !value || value.startsWith("tất cả") || row.textContent.toLocaleLowerCase("vi").includes(value);
                row.classList.toggle("d-none", !matches);
            });
        });
    });

    const revenueChart = document.querySelector(".chart-bars");
    if (revenueChart) {
        const revenueValue = document.querySelector("#monthly-revenue-value");
        const revenueLabel = document.querySelector("#monthly-revenue-label");
        const defaultRevenue = revenueValue?.textContent ?? "0 ₫";
        let selectedMonth = null;
        const currency = new Intl.NumberFormat("vi-VN", { maximumFractionDigits: 0 });

        const showMonth = (monthButton) => {
            const amount = Number(monthButton.dataset.revenue ?? 0);
            if (revenueValue) revenueValue.textContent = `${currency.format(amount)} ₫`;
            if (revenueLabel) {
                revenueLabel.textContent = amount === 0
                    ? `${monthButton.dataset.month} · Chưa phát sinh doanh thu`
                    : `Doanh thu ${monthButton.dataset.month}`;
            }
        };

        const resetMonth = () => {
            if (selectedMonth) {
                showMonth(selectedMonth);
                return;
            }

            if (revenueValue) revenueValue.textContent = defaultRevenue;
            if (revenueLabel) revenueLabel.textContent = "Tổng doanh thu năm nay";
        };

        revenueChart.addEventListener("pointerover", (event) => {
            const monthButton = event.target.closest(".chart-month");
            if (monthButton) showMonth(monthButton);
        });
        revenueChart.addEventListener("pointerout", (event) => {
            const monthButton = event.target.closest(".chart-month");
            if (monthButton && !monthButton.contains(event.relatedTarget)) resetMonth();
        });
        revenueChart.addEventListener("focusin", (event) => {
            const monthButton = event.target.closest(".chart-month");
            if (monthButton) showMonth(monthButton);
        });
        revenueChart.addEventListener("focusout", (event) => {
            if (event.target.closest(".chart-month")) resetMonth();
        });
        revenueChart.addEventListener("click", (event) => {
            const monthButton = event.target.closest(".chart-month");
            if (!monthButton) return;

            selectedMonth = monthButton;
            revenueChart.querySelectorAll(".chart-month").forEach((button) => {
                button.classList.toggle("selected", button === selectedMonth);
                button.setAttribute("aria-pressed", String(button === selectedMonth));
            });
            showMonth(selectedMonth);
        });
    }
})();
