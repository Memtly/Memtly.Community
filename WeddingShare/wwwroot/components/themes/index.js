import { setCookie } from '../cookies';

export function initThemes() {
    bindEventHandlers();
}

function bindEventHandlers() {
    $(document).off('click', '.change-theme').on('click', '.change-theme', function (e) {
        if ($('.change-theme').hasClass('fa-sun')) {
            setCookie('Theme', 'default', 24);
        } else {
            setCookie('Theme', 'dark', 24);
        }

        window.location.reload();
    });
}