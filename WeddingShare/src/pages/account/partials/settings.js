import { displayMessage } from '@modules/message-box';
import { displayLoader } from '@modules/loader';

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindUpdateSettingActions();
    bindSaveSettingsButton();
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