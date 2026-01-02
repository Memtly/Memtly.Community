import { displayPopup } from '@modules/popups';
import { displayMessage } from '@modules/message-box';

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindMultiFactorChangeButton();
}

function bindMultiFactorChangeButton() {
    $(document).off('click', '.change-2fa').on('click', '.change-2fa', function (e) {
        preventDefaults(e);

        const isSet = $(this).data('mfa-set');
        if (isSet) {
            showResetPopup();
        } else {
            generateToken().then(data => {
                if (data !== undefined && data.secret !== undefined && data.qr_code !== undefined) {
                    showSetupPopup(data.secret, data.qr_code);
                } else {
                    displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Failed'), [ localization.translate('2FA_Generate_Secret_Failed') ]);
                }
            });
        }
    });
}

function showSetupPopup(secret, qrCode) {
    displayPopup({
        Title: localization.translate('2FA_Setup'),
        CustomHtml: `<div class="text-center">
                <p class="mb-1">${localization.translate('2FA_Scan_With_App')}</p>
                <p class="mb-2"><img src="${qrCode}"/></p>
                <p class="mb-2">${localization.translate('Or')}</p>
                <p class="mb-0">${localization.translate('2FA_Manually_Enter_Code')}</p>
                <p class="mb-4 fw-bold">${secret}</p>
            </div>`,
        Buttons: [{
            Text: localization.translate('Next'),
            Class: 'btn-primary-2',
            Callback: function () {
                multiFactorAuthValidation(secret);
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

function showResetPopup() {
    displayPopup({
        Title: localization.translate('2FA_Setup'),
        Buttons: [{
            Text: localization.translate('Reset'),
            Class: 'btn-danger',
            Callback: function () {
                $.ajax({
                    type: "DELETE",
                    url: '/MultiFactor/Reset',
                    success: function (data) {
                        if (data.success) {
                            displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Reset_Successfully'));
                            $('.change-2fa').attr('data-mfa-set', 'true');
                        } else {
                            displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Reset_Failed'));
                        }
                        $('.change-2fa').attr('data-mfa-set', data.success);
                    }
                });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

function multiFactorAuthValidation(secret) {
    displayPopup({
        Title: localization.translate('2FA_Setup'),
        Fields: [{
            Id: '2fa-secret',
            Value: secret,
            Type: 'hidden'
        }, {
            Id: '2fa-code',
            Name: localization.translate('Code'),
            Value: '',
            Hint: localization.translate('2FA_Code_Hint')
        }],
        Buttons: [{
            Text: localization.translate('Validate'),
            Class: 'btn-primary-2',
            Callback: function () {
                let secret = $('#popup-modal-field-2fa-secret').val();
                let code = $('#popup-modal-field-2fa-code').val();

                $.ajax({
                    type: "POST",
                    url: '/MultiFactor/Register',
                    data: { secret, code },
                    success: function (data) {
                        if (data.success) {
                            displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Successfully'));
                        } else {
                            displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Failed'), [localization.translate('2FA_Invalid_Code')]);
                        }
                        $('.change-2fa').attr('data-mfa-set', data.success);
                    }
                });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

async function generateToken() {
    const response = await fetch(`/MultiFactor/GenerateToken`);
    const data = await response.json();

    return data;
}

export default init;