// Theme management
(function() {
    var THEME_KEY = 'theme';
    var DARK_THEME = 'dark';

    function init() {
        var currentTheme = localStorage.getItem(THEME_KEY) || 'light';
        var html = document.documentElement;
        var icon = document.getElementById('themeIcon');

        if (currentTheme === DARK_THEME) {
            html.setAttribute('data-theme', DARK_THEME);
            if (icon) {
                icon.classList.remove('bi-moon-fill');
                icon.classList.add('bi-sun-fill');
            }
        }

        var toggle = document.getElementById('themeToggle');
        if (toggle) {
            toggle.addEventListener('click', toggleTheme);
        }

        updateNavCounts();
    }

    function toggleTheme() {
        var html = document.documentElement;
        var icon = document.getElementById('themeIcon');
        var theme = html.getAttribute('data-theme');

        if (theme === DARK_THEME) {
            html.removeAttribute('data-theme');
            localStorage.setItem(THEME_KEY, 'light');
            if (icon) {
                icon.classList.remove('bi-sun-fill');
                icon.classList.add('bi-moon-fill');
            }
        } else {
            html.setAttribute('data-theme', DARK_THEME);
            localStorage.setItem(THEME_KEY, DARK_THEME);
            if (icon) {
                icon.classList.remove('bi-moon-fill');
                icon.classList.add('bi-sun-fill');
            }
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
