import { displayMessage } from '@modules/message-box';
import { displayLoader, hideLoader } from '@modules/loader';
import { displayPopup } from '@modules/popups';

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindLoginForm();
}

function bindLoginForm() {
    $(document).off('submit', '#frmLogin').on('submit', '#frmLogin', function (e) {
        preventDefaults(e);

        var token = $('#frmLogin input[name=\'__RequestVerificationToken\']').val();

        var username = $('#frmLogin input.input-username').val();
        if (username === undefined || username.length === 0) {
            displayMessage(localization.translate('Login'), localization.translate('Login_Invalid_Username'));
            return;
        }

        var password = $('#frmLogin input.input-password').val();
        if (password === undefined || password.length === 0) {
            displayMessage(localization.translate('Login'), localization.translate('Login_Invalid_Password'));
            return;
        }

        displayLoader(localization.translate('Loading'));

        $.ajax({
            url: '/Account/Login',
            method: 'POST',
            data: { __RequestVerificationToken: token, Username: username, Password: password }
        })
            .done(data => {
                hideLoader();

                if (data.success === true) {
                    if (data.pending_activation == true) {
                        displayMessage(localization.translate('Login'), localization.translate('Login_Verify_Email'));
                    }
                    else if (data.mfa === true) {
                        displayPopup({
                            Title: localization.translate('2FA'),
                            Fields: [{
                                Id: '2fa-code',
                                Name: localization.translate('Code'),
                                Value: '',
                                Hint: localization.translate('2FA_Code_Hint')
                            }],
                            Buttons: [{
                                Text: localization.translate('Validate'),
                                Class: 'btn-primary-2',
                                Callback: function () {
                                    let code = $('#popup-modal-field-2fa-code').val();

                                    $.ajax({
                                        type: "POST",
                                        url: '/Account/ValidateMultifactorAuth',
                                        data: { __RequestVerificationToken: token, Username: username, Password: password, Code: code },
                                        success: function (data) {
                                            if (data.success === true) {
                                                window.location = `/Account`;
                                            } else if (data.message) {
                                                displayMessage(localization.translate('Login'), localization.translate('Login_Failed'), [data.message]);
                                            } else {
                                                displayMessage(localization.translate('Login'), localization.translate('Unexpected_Error_Occurred'));
                                            }
                                        }
                                    });
                                }
                            }, {
                                Text: localization.translate('Close')
                            }]
                        });
                    } else {
                        window.location = `/Account`;
                    }
                } else if (data.message) {
                    displayMessage(localization.translate('Login'), localization.translate('Login_Failed'), [data.message]);
                } else {
                    displayMessage(localization.translate('Login'), localization.translate('Login_Invalid_Details'));
                }
            })
            .fail((xhr, error) => {
                hideLoader();
                displayMessage(localization.translate('Login'), localization.translate('Login_Failed'), [error]);
            });
    });
}

export default init;