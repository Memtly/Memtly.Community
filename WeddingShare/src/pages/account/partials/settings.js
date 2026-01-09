import { displayMessage } from '@modules/message-box';
import { displayLoader } from '@modules/loader';

let settingsSearchTimeout = null;

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindUpdateSettingActions();
    bindSaveSettingsButton();
    bindAdvancedSettingsButtons();
    bindSettingsSearchBox();
}

function bindUpdateSettingActions() {
    $(document).off('change', 'input.setting-field,select.setting-field,textarea.setting-field').on('change', 'input.setting-field,select.setting-field,textarea.setting-field', function (e) {
        $(this).attr('data-updated', 'true');
    });
}

function bindSaveSettingsButton() {
    $(document).off('click', 'button#btnSaveSettings').on('click', 'button#btnSaveSettings', function (e) {
        let updatedFields = $('.setting-field[data-updated="true"]');
        if (updatedFields.length > 0) {
            var settingsList = $.map(updatedFields, function (item) {
                let element = $(item);
                return { key: element.data('setting-name'), value: element.val() };
            });

            displayLoader(localization.translate('Loading'));
            $.ajax({
                url: '/Account/UpdateSettings',
                method: 'PUT',
                data: { model: settingsList }
            })
                .done(data => {
                    if (data.success === true) {
                        displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Success'));
                    } else if (data.message) {
                        displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Failed'), [data.message]);
                    } else {
                        displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Failed'));
                    }
                })
                .fail((xhr, error) => {
                    displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Failed'), [error]);
                });
        } else {
            displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_No_Change'));
        }
    });
}

function bindAdvancedSettingsButtons() {
    $(document).off('click', '.btnShowAdvancedSettings').on('click', '.btnShowAdvancedSettings', function (e) {
        showAdvancedSettings();
        searchSettings();
    });

    $(document).off('click', '.btnHideAdvancedSettings').on('click', '.btnHideAdvancedSettings', function (e) {
        hideAdvancedSettings();
        searchSettings();
    });
}

function bindSettingsSearchBox() {
    $(document).off('keyup', 'input#settings-search-term').on('keyup', 'input#settings-search-term', function (e) {
        searchSettings();
    });
}

function showAdvancedSettings() {
    $('.setting-advanced').removeClass('d-none');
    $('.btnShowAdvancedSettings').addClass('d-none');
    $('.btnHideAdvancedSettings').removeClass('d-none');
}

function hideAdvancedSettings() {
    $('.setting-advanced').addClass('d-none');
    $('.btnShowAdvancedSettings').removeClass('d-none');
    $('.btnHideAdvancedSettings').addClass('d-none');
}

export function searchSettings() {
    clearTimeout(settingsSearchTimeout);
    settingsSearchTimeout = setTimeout(() => {
        let term = $('input#settings-search-term').val();

        $('#settings-accordion .accordion-item, #settings-accordion .accordion-item .setting-container').removeClass('d-none');

        if (term !== undefined && term.length > 0) {
            $('.setting-container').each(function () {
                const label = $(this).find('.setting-label').text();
                const hint = $(this).find('.setting-hint').text();

                if ((label === undefined && hint === undefined) || (label.toLowerCase().indexOf(term.toLowerCase()) === -1 && hint.toLowerCase().indexOf(term.toLowerCase()) === -1)) {
                    $(this).addClass('d-none');
                } else {
                    $(this).removeClass('d-none');
                }
            });
        }

        if ($('.btnShowAdvancedSettings:not(.d-none)').length > 0) {
            hideAdvancedSettings();
        }

        $('#settings-accordion .accordion-item').each(function () {
            const count = $(this).find('.setting-container:not(.d-none)').length;
            if (count === 0) {
                $(this).addClass('d-none');
            }
        });
    }, 500);
}

export function updateSettings() {
    $.ajax({
        type: 'GET',
        url: `/Account/SettingsPartial`,
        success: function (data) {
            $('#settings-list').html(data);
            bindEventHandlers();
        }
    });
}

export default init;