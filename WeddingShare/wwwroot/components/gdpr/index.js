export function initGdpr() {
    bindEventHandlers();
}

function bindEventHandlers() {
    if ($('div.cookie-consent-alert').length === 0) {
        acceptCookieConcent();
    }

    $(document).off('click', '.cookie-consent-alert button.accept-policy').on('click', '.cookie-consent-alert button.accept-policy', function (e) {
        preventDefaults(e);
        acceptCookieConcent();
    });
}

function acceptCookieConcent() {
    document.cookie = $('.cookie-consent').data('cookie-string');

    $('.cookie-consent-wrapper').remove();
    $('.cookie-consent-alert').remove();

    $.ajax({
        url: '/Home/LogCookieApproval',
        method: 'POST'
    });
}