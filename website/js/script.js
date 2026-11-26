
const menuToggle = document.getElementById("menu-toggle");
const navLinks = document.getElementById("nav-links");

if (menuToggle && navLinks) {
    const closeMenu = (restoreFocus = false) => {
        navLinks.classList.remove("active");
        menuToggle.setAttribute("aria-expanded", "false");
        menuToggle.setAttribute("aria-label", "Abrir menu");

        if (restoreFocus) {
            menuToggle.focus();
        }
    };

    menuToggle.addEventListener("click", () => {
        const isOpen = navLinks.classList.toggle("active");
        menuToggle.setAttribute("aria-expanded", String(isOpen));
        menuToggle.setAttribute("aria-label", isOpen ? "Fechar menu" : "Abrir menu");
    });

    navLinks.addEventListener("click", (event) => {
        if (!(event.target instanceof Element) || !event.target.closest("a")) {
            return;
        }

        closeMenu();
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && navLinks.classList.contains("active")) {
            closeMenu(true);
        }
    });
}

const currentYear = document.getElementById("current-year");
if (currentYear) {
    currentYear.textContent = new Date().getFullYear();
}
