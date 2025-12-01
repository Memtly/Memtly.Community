import { displayMessage } from '../../../components/message-box';
import { displayLoader, hideLoader } from '../../../components/loader';

function bindEventHandlers() {
    $(document).off('submit', '#frmForgotPassword').on('submit', '#frmForgotPassword', function (e) {
        preventDefaults(e);

        var token = $('#frmForgotPassword input[name=\'__RequestVerificationToken\']').val();

        var email = $('#frmForgotPassword input.input-email').val();
        if (email === undefined || email.length === 0 || email.length > 100 || email.indexOf('@') === -1 || email.indexOf('.') === -1) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_Email'));
            return;
        }

        displayLoader(localization.translate('Loading'));

        $.ajax({
            url: '/Account/ForgotPassword',
            method: 'POST',
            data: { __RequestVerificationToken: token, emailAddress: email }
        })
            .done(data => {
                hideLoader();

                if (data.message) {
                    displayMessage(localization.translate('ForgotPassword'), localization.translate('ForgotPassword_Failed'), [data.message]);
                } else {
                    displayMessage(localization.translate('ForgotPassword'), localization.translate('ForgotPassword_Sent'), null, function () {
                        window.location = `/Account/Login`;
                    });
                }
            })
            .fail((xhr, error) => {
                hideLoader();
                displayMessage(localization.translate('ForgotPassword'), localization.translate('ForgotPassword_Failed'), [error]);
            });
    });
}

bindEventHandlers();