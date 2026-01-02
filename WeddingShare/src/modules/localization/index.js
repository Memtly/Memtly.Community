import { displayLoader, hideLoader } from '@modules/loader';
import { displayPopup } from '@modules/popups';
import { getCookie } from '@modules/cookies';

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

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindChangeLanguageButton();
}

function bindChangeLanguageButton() {
    $(document).off('click', '.change-language').on('click', '.change-language', function (e) {
        preventDefaults(e);

        displayLoader(localization.translate('Loading'));

        $.ajax({
            type: "GET",
            url: '/Language',
            success: function (data) {
                hideLoader();

                if (data.languages && data.languages.length > 0) {
                    displayPopup({
                        Title: localization.translate('Language_Change'),
                        Fields: [{
                            Id: 'language-id',
                            Name: localization.translate('Language'),
                            Hint: localization.translate('Language_Name_Hint'),
                            Placeholder: 'English',
                            Type: 'select',
                            SelectOptions: data.languages.map(lang => ({
                                key: lang.name,
                                value: lang.name,
                                selected: lang.selected
                            }))
                        }, {
                            Id: 'culture-id',
                            Name: localization.translate('Culture'),
                            Hint: localization.translate('Culture_Name_Hint'),
                            Placeholder: 'en-GB',
                            Type: 'select',
                            SelectOptions: data.languages.find(lang => lang.selected).cultures.map(culture => ({
                                key: culture.name,
                                value: culture.name,
                                selected: culture.selected
                            }))
                        }],
                        Buttons: [{
                            Text: localization.translate('Switch'),
                            Class: 'btn-primary-2',
                            Callback: function () {
                                let culture = $('#popup-modal-field-culture-id').val().trim();
                                changeSelectedLanguage(culture);
                            }
                        }, {
                            Text: localization.translate('Cancel')
                        }]
                    }, () => {
                        $('#popup-modal-field-language-id').off('change').on('change', function () {
                            const selectedLang = $('#popup-modal-field-language-id').val().trim();
                            let options = data.languages.find(lang => lang.name === selectedLang).cultures.map(culture => `<option value="${culture.name}">${culture.name}</option>`);
                            $('#popup-modal-field-culture-id').html(options.join(''));
                        });
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

init();