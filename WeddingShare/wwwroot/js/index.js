import '../css/index.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap';
import '@fortawesome/fontawesome-free/css/all.min.css';
import '@fortawesome/fontawesome-free/js/all.js';
import 'jquery-loading';
import 'jquery-qrcode';
import 'jquery-validation';
import 'jquery-validation-unobtrusive';

import { Localization } from '../components/localization';
import { initGdpr } from '../components/gdpr';
import { initThemes } from '../components/themes';
import { initIdentityCheck } from '../components/identity-check';
import { initSponsors } from '../components/sponsors';

const app = {
    initialized: false,
    config: {
        theme: 'default',
        debug: true
    }
};

async function init() {
    if (app.initialized) return;

    app.config.theme = document.body.dataset.theme.toLowerCase();
    app.initialized = true;

    resizeLayout();
    bindEventHandlers();

    const localization = new Localization();
    await localization.init();

    window.localization = localization;

    initGdpr();
    initThemes();
    initIdentityCheck();
    initSponsors();
}

function bindEventHandlers() {
    if ($('div.navbar-options').length == 0) {
        var presentationTimeout = setTimeout(function () {
            $('.presentation-hidden').fadeOut(500);
            $('body').css('cursor', 'none');
        }, 1000);

        $(document).off('mousemove').on('mousemove', function () {
            $('.presentation-hidden').fadeIn(200);
            $('body').css('cursor', 'default');

            clearTimeout(presentationTimeout);
            presentationTimeout = setTimeout(function () {
                $('.presentation-hidden').fadeOut(500);
                $('body').css('cursor', 'none');
            }, 1000);
        });
    }

    if ($('.nav-horizontal-scroller').length > 0) {
        $('.nav-horizontal-scroller').each(function () {
            let pos = $(this).find('.active').position().left - 30;
            $('.nav-horizontal-scroller').scrollLeft(pos);
        });
    }
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