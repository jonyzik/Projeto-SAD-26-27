
// Menu de navegação para dispositivos móveis

const menuToggle = document.getElementById('menu-toggle');
const navLinks = document.getElementById('nav-links');

if (menuToggle && navLinks) {

    menuToggle.addEventListener('click', () => {

        const isOpen = navLinks.classList.toggle('active');

        menuToggle.setAttribute(
            'aria-expanded',
            String(isOpen)
        );

    });

    navLinks.querySelectorAll('a').forEach(link => {

        link.addEventListener('click', () => {

            navLinks.classList.remove('active');

            menuToggle.setAttribute(
                'aria-expanded',
                'false'
            );

        });

    });

}


// Atualizar automaticamente o ano do rodapé

const currentYear = document.getElementById('current-year');

if (currentYear) {
    currentYear.textContent = new Date().getFullYear();
}


// Formulário de login (apenas interface visual)

const loginForm = document.getElementById('login-form');
const loginMessage = document.getElementById('login-message');

if (loginForm && loginMessage) {

    loginForm.addEventListener('submit', (event) => {

        event.preventDefault();

        loginMessage.textContent =
            'A autenticação ainda não está ligada ao servidor .NET.';

    });

}
