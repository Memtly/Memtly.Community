import { getCookie, setCookie } from '@modules/cookies';

function init() {
    const theme = getSelectedTheme();
    const themeCookie = getCookie('Theme');
    if (themeCookie === undefined || themeCookie.length === 0 || themeCookie !== theme) {
        changeSelectedTheme(theme);
    }

    bindEventHandlers();
}

export function getSelectedTheme() {
    return document.body.dataset.theme !== undefined ? document.body.dataset.theme.toLowerCase() : 'default';
}

function changeSelectedTheme(theme) {
    setCookie('Theme', theme, 'Functional', 24);
    window.location.reload();
}

function bindEventHandlers() {
    $(document).off('click', '.change-theme').on('click', '.change-theme', function (e) {
        if ($('.change-theme').hasClass('fa-sun')) {
            changeSelectedTheme('default');
        } else {
            changeSelectedTheme('dark');
        }
    });
}

export default init;