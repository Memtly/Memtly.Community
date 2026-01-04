import '@styles/main.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap';
import '@fortawesome/fontawesome-free/css/all.min.css';
import '@fortawesome/fontawesome-free/js/all.js';
import 'jquery-loading';
import 'jquery-qrcode';
import 'jquery-validation';
import 'jquery-validation-unobtrusive';

import { Localization } from '@modules/localization';
import initGdpr from '@modules/gdpr';
import { default as initThemes, getSelectedTheme } from '@themes';
import initIdentityCheck from '@modules/identity-check';
import initSponsors from '@modules/sponsors';
import initQrCodes from '@modules/qr-codes';
import { displayMessage } from '@modules/message-box';

const app = {
    initialized: false,
    config: {
        theme: 'default',
        debug: true
    }
};

async function init() {
    if (app.initialized) return;

    resizeLayout();
    bindEventHandlers();

    const localization = new Localization();
    await localization.init();

    window.localization = localization;

    initPage();
    initGdpr();
    initThemes();
    initIdentityCheck();
    initSponsors();
    initQrCodes();

    app.config.theme = getSelectedTheme();
    app.initialized = true;
}

function initPage() {
    const path = window.location.pathname.toLowerCase();
    if (path === '/') {
        import('@pages/homepage').then(({ default: init }) => { init(); });
    } else if (path.startsWith('/gallery')) {
        import('@pages/gallery').then(({ default: init }) => { init(); });
    } else if (path.startsWith('/account')) {
        import('@pages/account').then(({ default: init }) => { init(); });
    }
}

function bindEventHandlers() {
    bindPressentationMode();
    bindNavigationScrollers();
    bindPageResizeEvent();
    bindUpgradeToUnlock();
    bindLoginPrompt();
}

function bindPressentationMode() {
    if ($('div.navbar-options').length == 0) {
        var presentationTimeout = setTimeout(() => {
            $('.presentation-hidden').fadeOut(500);
            $('body').css('cursor', 'none');
        }, 1000);

        $(document).off('mousemove').on('mousemove', () => {
            $('.presentation-hidden').fadeIn(200);
            $('body').css('cursor', 'default');

            clearTimeout(presentationTimeout);
            presentationTimeout = setTimeout(() => {
                $('.presentation-hidden').fadeOut(500);
                $('body').css('cursor', 'none');
            }, 1000);
        });
    }
}

function bindNavigationScrollers() {
    if ($('.nav-horizontal-scroller').length > 0) {
        $('.nav-horizontal-scroller').each(function () {
            let pos = $(this).find('.active').position().left - 30;
            $('.nav-horizontal-scroller').scrollLeft(pos);
        });
    }
}

function bindPageResizeEvent() {
    $(window).on('resize', function (e) {
        resizeLayout();
    });
}

function bindUpgradeToUnlock() {
    $(document).off('click', '.upgradeToUnlock').on('click', '.upgradeToUnlock', function (e) {
        displayMessage(localization.translate('Unavailable'), localization.translate('Paywall_Feature'));
    });
}

function bindLoginPrompt() {
    $(document).off('click', '.login-prompt').on('click', '.login-prompt', function (e) {
        displayMessage(localization.translate('Login'), localization.translate('Login_To_Complete_Action'));
    });
}

function resizeLayout() {
    if ($('div#main-wrapper').length > 0) {
        let windowWidth = $(window).width();
        let windowHeight = $(window).height();
        let navHeight = $('nav.navbar').outerHeight();
        let alertHeight = $('.header-alert').length > 0 ? $('.header-alert').outerHeight() : 0;
        let footerHeight = $('footer').outerHeight();
        let bodyHeight = windowHeight - (navHeight + footerHeight + alertHeight);

        $('div#main-wrapper').css({
            'height': `${bodyHeight + alertHeight}px`,
            'max-height': `${bodyHeight + alertHeight}px`,
            'top': `${navHeight}px`
        });

        if ($('div#main-content').length > 0) {
            let contentHeight = $('div#main-content').outerHeight();
            let padding = (bodyHeight - contentHeight) / 2;

            if (windowWidth >= 700 && padding < 30) {
                padding = 30;
            } else if (windowWidth < 700 && padding < 20) {
                padding = 20;
            }

            $('div#main-content').css({
                'padding-top': `${padding}px`,
                'padding-bottom': `50px`
            });
        }
    }
}

document.addEventListener('DOMContentLoaded', function () {
    window.preventDefaults = event => {
        event.preventDefault();
        event.stopPropagation();
    };

    init();
});

export { app };