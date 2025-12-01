import { displayMessage } from '../../../components/message-box';
import { displayLoader, hideLoader } from '../../../components/loader';

function bindEventHandlers() {
    $(document).off('submit', '#frmResetPassword').on('submit', '#frmResetPassword', function (e) {
        preventDefaults(e);

        var token = $('#frmResetPassword input[name=\'__RequestVerificationToken\']').val();

        var data = $('#frmResetPassword input.input-data').val();
        if (data === undefined || data.length === 0) {
            displayMessage(localization.translate('PasswordReset'), localization.translate('Unexpected_Error_Occurred'));
            return;
        }

        var password = $('#frmResetPassword input.input-password').val();
        if (password === undefined || password.length < 8 || password.length > 100) {
            displayMessage(localization.translate('PasswordReset'), localization.translate('Registration_Invalid_Password'));
            return;
        }

        var cpassword = $('#frmResetPassword input.input-cpassword').val();
        if (cpassword === undefined || cpassword.length === 0 || cpassword !== password) {
            displayMessage(localization.translate('PasswordReset'), localization.translate('Registration_Invalid_CPassword'));
            return;
        }

        displayLoader(localization.translate('Loading'));

        $.ajax({
            url: '/Account/ResetPassword',
            method: 'POST',
            data: { __RequestVerificationToken: token, Data: data, Password: password, ConfirmPassword: cpassword }
        })
            .done(data => {
                hideLoader();

                if (data.success === true && data.username) {
                    displayMessage(localization.translate('PasswordReset'), localization.translate('PasswordReset_Success'), null, function () {
                        if (data.mfa) {
                            window.location = `/Account/Login`;
                        } else {
                            displayLoader(localization.translate('Loading'));
                            $.ajax({
                                url: '/Account/Login',
                                method: 'POST',
                                data: { __RequestVerificationToken: token, Username: data.username, Password: password }
                            })
                                .done(data => {
                                    hideLoader();

                                    if (data.success === true) {
                                        window.location = `/Account`;
                                    } else if (data.message) {
                                        displayMessage(localization.translate('PasswordReset'), localization.translate('Login_Failed'), [data.message]);
                                    } else {
                                        displayMessage(localization.translate('PasswordReset'), localization.translate('Login_Invalid_Details'));
                                    }
                                })
                                .fail((xhr, error) => {
                                    hideLoader();
                                    displayMessage(localization.translate('PasswordReset'), localization.translate('Login_Failed'), [error]);
                                });
                        }
                    });
                } else if (data.message) {
                    displayMessage(localization.translate('PasswordReset'), localization.translate('PasswordReset_Failed'), [data.message]);
                } else {
                    displayMessage(localization.translate('PasswordReset'), localization.translate('Unexpected_Error_Occurred'));
                }
            })
            .fail((xhr, error) => {
                hideLoader();
                displayMessage(localization.translate('PasswordReset'), localization.translate('PasswordReset_Failed'), [error]);
            });
    });
}

bindEventHandlers();