(function () {
    'use strict';
    var THEME_KEY = 'theme';
    var DARK = 'dark';

    function applyIcon(theme) {
        var icon = document.getElementById('themeIcon');
        if (!icon) return;
        if (theme === DARK) {
            icon.classList.remove('bi-moon-fill');
            icon.classList.add('bi-sun-fill');
        } else {
            icon.classList.remove('bi-sun-fill');
            icon.classList.add('bi-moon-fill');
        }
    }

    function init() {
        var current = (document.documentElement.getAttribute('data-theme') === DARK) ? DARK : 'light';
        applyIcon(current);

        var toggle = document.getElementById('themeToggle');
        if (toggle) {
            toggle.addEventListener('click', toggleTheme);
        }
    }

    function toggleTheme() {
        var html = document.documentElement;
        var next = (html.getAttribute('data-theme') === DARK) ? 'light' : DARK;
        if (next === DARK) {
            html.setAttribute('data-theme', DARK);
        } else {
            html.removeAttribute('data-theme');
        }
        try {
            localStorage.setItem(THEME_KEY, next);
        } catch (err) {
            console.warn('Theme persistence failed', err);
        }
        applyIcon(next);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
