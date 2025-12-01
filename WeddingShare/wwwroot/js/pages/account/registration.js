import { displayMessage } from '../../../components/message-box';
import { displayLoader, hideLoader } from '../../../components/loader';

function bindEventHandlers() {
    $(document).off('submit', '#frmRegisterAccount').on('submit', '#frmRegisterAccount', function (e) {
        preventDefaults(e);

        var token = $('#frmRegisterAccount input[name=\'__RequestVerificationToken\']').val();

        var username = $('#frmRegisterAccount input.input-username').val();
        if (username === undefined || username.length < 5 || username.length > 50) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_Username'));
            return;
        }

        var email = $('#frmRegisterAccount input.input-email').val();
        if (email === undefined || email.length === 0 || email.length > 100 || email.indexOf('@') === -1 || email.indexOf('.') === -1) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_Email'));
            return;
        }

        var password = $('#frmRegisterAccount input.input-password').val();
        if (password === undefined || password.length < 8 || password.length > 100) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_Password'));
            return;
        }

        var cpassword = $('#frmRegisterAccount input.input-cpassword').val();
        if (cpassword === undefined || cpassword.length === 0 || cpassword !== password) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_CPassword'));
            return;
        }

        displayLoader(localization.translate('Loading'));

        $.ajax({
            url: '/Account/Register',
            method: 'POST',
            data: { __RequestVerificationToken: token, Username: username, EmailAddress: email, Password: password, ConfirmPassword: cpassword }
        })
            .done(data => {
                hideLoader();

                if (data.success === true) {
                    if (data.validation === true) {
                        displayMessage(localization.translate('Registration'), localization.translate('Registration_Success_Validation'), null, function () {
                            window.location = `/Account`;
                        });
                    } else {
                        displayMessage(localization.translate('Registration'), localization.translate('Registration_Success'), null, function () {
                            displayLoader(localization.translate('Loading'));
                            $.ajax({
                                url: '/Account/Login',
                                method: 'POST',
                                data: { __RequestVerificationToken: token, Username: username, Password: password }
                            })
                                .done(data => {
                                    hideLoader();

                                    if (data.success === true) {
                                        window.location = `/Account`;
                                    } else if (data.message) {
                                        displayMessage(localization.translate('Registration'), localization.translate('Login_Failed'), [data.message]);
                                    } else {
                                        displayMessage(localization.translate('Registration'), localization.translate('Login_Invalid_Details'));
                                    }
                                })
                                .fail((xhr, error) => {
                                    hideLoader();
                                    displayMessage(localization.translate('Registration'), localization.translate('Login_Failed'), [error]);
                                });
                        });
                    }
                } else if (data.message) {
                    displayMessage(localization.translate('Registration'), localization.translate('Registration_Failed'), [data.message]);
                } else {
                    displayMessage(localization.translate('Registration'), localization.translate('Unexpected_Error_Occurred'));
                }
            })
            .fail((xhr, error) => {
                hideLoader();
                displayMessage(localization.translate('Registration'), localization.translate('Registration_Failed'), [error]);
            });
    });
}

bindEventHandlers();