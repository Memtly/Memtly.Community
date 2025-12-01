import { displayLoader, hideLoader } from '../loader';
import { displayPopup } from '../popups';
import { getCookie } from '../cookies';

export class Localization {
    constructor() {
        this.data = {
            current: {
                full: 'English (en-GB)',
                code: 'en-GB',
                name: 'English'
            },
            translations: []
        };
    }

    async init() {
        const cultureCookie = getCookie('.AspNetCore.Culture');
        if (cultureCookie === undefined || cultureCookie.length === 0) {
            const urlParams = new URLSearchParams(window.location.search);
            const culture = urlParams.get('culture') || window.navigator.language;

            changeSelectedLanguage(culture);
        }

        await this.getTranslations();
    }

    async getTranslations() {
        const response = await fetch(`/Language/GetTranslations`);
        this.data = await response.json();
    }

    translate(key) {
        return this.data.translations[key] || key;
    }
}

function bindEventHandlers() {
    $(document).off('click', '.change-language').on('click', '.change-language', function (e) {
        preventDefaults(e);

        displayLoader(localization.translate('Loading'));

        $.ajax({
            type: "GET",
            url: '/Language',
            success: function (data) {
                hideLoader();

                if (data.supported && data.supported.length > 0) {
                    displayPopup({
                        Title: localization.translate('Language_Change'),
                        Fields: [{
                            Id: 'language-id',
                            Name: localization.translate('Language'),
                            Hint: localization.translate('Language_Name_Hint'),
                            Placeholder: 'English (en-GB)',
                            Type: 'select',
                            SelectOptions: data.supported
                        }],
                        Buttons: [{
                            Text: localization.translate('Switch'),
                            Class: 'btn-success',
                            Callback: function () {
                                let culture = $('#popup-modal-field-language-id').val().trim();
                                changeSelectedLanguage(culture);
                            }
                        }, {
                            Text: localization.translate('Cancel')
                        }]
                    });
                }
            }
        });
    });
}

export function changeSelectedLanguage(culture) {
    $.ajax({
        type: "POST",
        url: '/Language/ChangeDisplayLanguage',
        data: { culture: culture || 'en-GB' },
        success: function (data) {
            if (data.success) {
                try {
                    window.location = window.location.toString().replace(/([&]*culture\=.+?)(\&|$)/g, '');
                } catch {
                    window.location.reload();
                }
            }
        }
    });
}

bindEventHandlers();