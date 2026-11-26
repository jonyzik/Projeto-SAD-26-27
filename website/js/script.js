
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


// Formulário de registo (apenas interface visual)

const registerForm = document.getElementById('register-form');
const registerMessage = document.getElementById('register-message');

if (registerForm && registerMessage) {

    registerForm.addEventListener('submit', (event) => {

        event.preventDefault();

        registerMessage.textContent =
            'O registo ainda não está ligado ao servidor .NET.';

    });

}
