document.addEventListener("DOMContentLoaded", function () {

    const registerForm = document.getElementById("register-form");

    if (!registerForm) {
        console.error("Não foi encontrado o formulário de registo.");
        return;
    }

    registerForm.addEventListener("submit", function (event) {
        event.preventDefault();

        const nome = document.getElementById("nome").value.trim();
        const email = document.getElementById("email").value.trim();
        const password = document.getElementById("password").value;
        const confirmPassword = document.getElementById("confirm-password").value;

        const message = document.getElementById("login-message");

        if (password.length < 8) {
            message.textContent = "A palavra-passe deve ter pelo menos 8 caracteres.";
            return;
        }

        if (password !== confirmPassword) {
            message.textContent = "As palavras-passe não coincidem.";
            return;
        }

        message.textContent = "Conta criada com sucesso!";

        window.location.href = "download.html";
    });

});