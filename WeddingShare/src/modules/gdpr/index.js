import { getCookie } from '@modules/cookies';

function init() {
    if ($('div.cookie-consent-alert').length === 0) {
        acceptCookieConcent();
    }

    bindEventHandlers();
}

function bindEventHandlers() {
    bindAcceptCookiePolicyButton();
}

function bindAcceptCookiePolicyButton() {
    $(document).off('click', '.cookie-consent-alert button.accept-policy').on('click', '.cookie-consent-alert button.accept-policy', function (e) {
        preventDefaults(e);
        acceptCookieConcent();
    });
}

function acceptCookieConcent() {
    let consent = getCookie('.AspNet.Consent');
    if (consent === undefined || consent.toLowerCase() === 'no') {
        document.cookie = $('.cookie-consent').data('cookie-string');

        $('.cookie-consent-wrapper').remove();
        $('.cookie-consent-alert').remove();

        $.ajax({
        url: '/Home/LogCookieApproval',
        method: 'POST'
    });
    }
}

export default init;