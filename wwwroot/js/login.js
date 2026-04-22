
(function () {
    'use strict';

    // ── Password Toggle ──
    var passwordInput = document.getElementById('passwordInput');
    var passwordToggle = document.getElementById('passwordToggle');
    var eyeOpen = document.getElementById('eyeOpen');
    var eyeClosed = document.getElementById('eyeClosed');

    if (passwordToggle && passwordInput) {
        passwordToggle.addEventListener('click', function () {
            var isPassword = passwordInput.type === 'password';
            passwordInput.type = isPassword ? 'text' : 'password';

            if (eyeOpen) eyeOpen.style.display = isPassword ? 'none' : 'block';
            if (eyeClosed) eyeClosed.style.display = isPassword ? 'block' : 'none';
        });
    }

    // ── Form Loading State ──
    var loginForm = document.getElementById('loginForm');
    var loginBtn = document.getElementById('loginBtn');
    var loginBtnText = document.getElementById('loginBtnText');

    if (loginForm && loginBtn) {
        loginForm.addEventListener('submit', function () {
            loginBtn.disabled = true;
            if (loginBtnText) loginBtnText.textContent = 'Signing in...';
            loginBtn.style.opacity = '0.8';
        });
    }

})();
