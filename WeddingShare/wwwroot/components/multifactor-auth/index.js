import { displayPopup } from '../popups';
import { displayMessage } from '../message-box';

export function initMultiFactorAuth() {
    bindEventHandlers();
}

function bindEventHandlers() {
    $(document).off('click', '.change-2fa').on('click', '.change-2fa', function (e) {
        preventDefaults(e);
        multiFactorAuthSetup($('.change-2fa').data('mfa-set'));
    });
}

function multiFactorAuthSetup(showResetOption) {
    let customHtml = '';
    let buttons = [];

    if (showResetOption) {
        buttons = [{
            Text: localization.translate('Reset'),
            Class: 'btn-danger',
            Callback: function () {
                $.ajax({
                    type: "DELETE",
                    url: '/Account/ResetMultifactorAuth',
                    success: function (data) {
                        if (data.success) {
                            displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Reset_Successfully'));
                        } else {
                            displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Reset_Failed'));
                        }
                        $('i.change-2fa').attr('data-mfa-set', data.success);
                    }
                });
            }
        }, {
            Text: localization.translate('Close')
        }];
    } else {
        customHtml = `<div class="text-center">
            <p class="mb-1">${localization.translate('2FA_Scan_With_App')}</p>
            <p class="mb-2"><img src="@qrCode"/></p>
            <p class="mb-2">${localization.translate('Or')}</p>
            <p class="mb-0">${localization.translate('2FA_Manually_Enter_Code')}</p>
            <p class="mb-4 fw-bold">@secret</p>
        </div>`;

        buttons = [{
            Text: localization.translate('Next'),
            Class: 'btn-success',
            Callback: function () {
                multiFactorAuthValidation();
            }
        }, {
            Text: localization.translate('Close')
        }];
    }

    displayPopup({
        Title: localization.translate('2FA_Setup'),
        CustomHtml: customHtml,
        Buttons: buttons
    });
}

export function multiFactorAuthValidation() {
    displayPopup({
        Title: localization.translate('2FA_Setup'),
        Fields: [{
            Id: '2fa-secret',
            Value: '@secret',
            Type: 'hidden'
        }, {
            Id: '2fa-code',
            Name: localization.translate('Code'),
            Value: '',
            Hint: localization.translate('2FA_Code_Hint')
        }],
        Buttons: [{
            Text: localization.translate('Validate'),
            Class: 'btn-success',
            Callback: function () {
                let secret = $('#popup-modal-field-2fa-secret').val();
                let code = $('#popup-modal-field-2fa-code').val();

                $.ajax({
                    type: "POST",
                    url: '/Account/RegisterMultifactorAuth',
                    data: { secret, code },
                    success: function (data) {
                        if (data.success) {
                            displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Successfully'));
                        } else {
                            displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Failed'));
                        }
                        $('i.change-2fa').attr('data-mfa-set', data.success);
                    }
                });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}