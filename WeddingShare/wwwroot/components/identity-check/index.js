import { displayMessage } from '../message-box';
import { displayPopup } from '../popups';

export function initIdentityCheck() {
    bindEventHandlers();
}

function bindEventHandlers() {
    $(document).off('click', '.change-identity').on('click', '.change-identity', function (e) {
        function displayIdentityCheckChangeIdentity(elem) {
            let emailRequired = elem.attr('data-identity-email') !== undefined;

            let fields = [{
                Id: 'identity-name',
                Name: localization.translate('Identity_Check_Name'),
                Value: elem.data('identity-name'),
                Hint: localization.translate('Identity_Check_Name_Hint'),
                Placeholder: localization.translate('Identity_Check_Name_Placeholder')
            }];

            if (emailRequired) {
                fields.push({
                    Id: 'identity-email',
                    Name: localization.translate('Identity_Check_Email'),
                    Value: elem.data('identity-email'),
                    Hint: localization.translate('Identity_Check_Email_Hint'),
                    Placeholder: localization.translate('Identity_Check_Email_Placeholder')
                });
            }

            preventDefaults(e);
            displayPopup({
                Title: localization.translate('Identity_Check_Change_Identity'),
                Fields: fields,
                Buttons: [{
                    Text: localization.translate('Identity_Check_Change'),
                    Class: 'btn-success',
                    Callback: function () {
                        let name = $('#popup-modal-field-identity-name').val().trim();
                        let emailAddress = $('#popup-modal-field-identity-email').length > 0 ? $('#popup-modal-field-identity-email').val().trim() : '';
                        if (name !== undefined && name.length > 0) {
                            $.ajax({
                                url: '/Home/SetIdentity',
                                method: 'POST',
                                data: { name, emailAddress }
                            })
                                .done(data => {
                                    if (data == undefined || data.success == undefined) {
                                        displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error]);
                                    } else if (data.success) {
                                        window.location.reload();
                                    } else if (data.reason == 1) {
                                        displayMessage(localization.translate('Identity_Check_Invalid_Name'), localization.translate('Identity_Check_Invalid_Name_Msg'), null, () => {
                                            displayIdentityCheckChangeIdentity(elem);
                                        });
                                    } else if (data.reason == 2) {
                                        displayMessage(localization.translate('Identity_Check_Invalid_Email'), localization.translate('Identity_Check_Invalid_Email_Msg'), null, () => {
                                            displayIdentityCheckChangeIdentity(elem);
                                        });
                                    } else {
                                        displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error]);
                                    }
                                })
                                .fail((xhr, error) => {
                                    displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error]);
                                });
                        } else {
                            displayMessage(localization.translate('Identity_Check_Invalid_Name'), localization.translate('Identity_Check_Invalid_Name_Msg'), null, () => {
                                displayIdentityCheckChangeIdentity(elem);
                            });
                        }
                    }
                }, {
                    Text: localization.translate('Cancel')
                }]
            });
        }
        displayIdentityCheckChangeIdentity($(this));
    });
}

export function displayIdentityCheck(required, callbackFn) {
    let buttons = [{
        Text: localization.translate('Identity_Check_Tell_Us'),
        Class: 'btn-success',
        Callback: function () {
            let name = $('#popup-modal-field-identity-name').val().trim();
            let emailAddress = $('#popup-modal-field-identity-email').length > 0 ? $('#popup-modal-field-identity-email').val().trim() : '';
            if (name !== undefined && name.length > 0) {
                $.ajax({
                    url: '/Home/SetIdentity',
                    method: 'POST',
                    data: { name, emailAddress }
                })
                    .done(data => {
                        if (data == undefined || data.success == undefined) {
                            displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error]);
                        } else if (data.success) {
                            $('.file-uploader-form').attr('data-identity-required', 'false');

                            if (callbackFn !== undefined && callbackFn !== null) {
                                callbackFn();
                            } else {
                                window.location.reload();
                            }
                        } else if (data.reason == 1) {
                            displayMessage(localization.translate('Identity_Check_Invalid_Name'), localization.translate('Identity_Check_Invalid_Name_Msg'), null, () => {
                                displayIdentityCheck(required, callbackFn);
                            });
                        } else if (data.reason == 2) {
                            displayMessage(localization.translate('Identity_Check_Invalid_Email'), localization.translate('Identity_Check_Invalid_Email_Msg'), null, () => {
                                displayIdentityCheck(required, callbackFn);
                            });
                        } else {
                            displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error]);
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error]);
                    });
            } else {
                displayMessage(localization.translate('Identity_Check_Invalid_Name'), localization.translate('Identity_Check_Invalid_Name_Msg'), null, () => {
                    displayIdentityCheck(required, callbackFn);
                });
            }
        }
    }];

    if (!required) {
        buttons.push({
            Text: localization.translate('Identity_Check_Stay_Anonymous'),
            Callback: function () {
                $.ajax({
                    url: '/Home/SetIdentity',
                    method: 'POST',
                    data: { name: 'Anonymous', emailAddress: '' }
                })
                    .done(data => {
                        if (data == undefined || data.success == undefined) {
                            displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error]);
                        } else if (data.success) {
                            window.location.reload();
                        } else if (data.reason == 1) {
                            displayMessage(localization.translate('Identity_Check_Invalid_Name'), localization.translate('Identity_Check_Invalid_Name_Msg'), null, () => {
                                displayIdentityCheck(required, callbackFn);
                            });
                        } else if (data.reason == 2) {
                            displayMessage(localization.translate('Identity_Check_Invalid_Email'), localization.translate('Identity_Check_Invalid_Email_Msg'), null, () => {
                                displayIdentityCheck(required, callbackFn);
                            });
                        } else {
                            displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error]);
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error]);
                    });
            }
        });
    }

    let emailRequired = $('.change-identity').attr('data-identity-email') !== undefined;
    let identityCheckFields = [{
        Id: 'identity-name',
        Name: localization.translate('Identity_Check_Name'),
        Value: '',
        Hint: localization.translate('Identity_Check_Name_Hint'),
        Placeholder: localization.translate('Identity_Check_Name_Placeholder')
    }];

    if (emailRequired) {
        identityCheckFields.push({
            Id: 'identity-email',
            Name: localization.translate('Identity_Check_Email'),
            Value: '',
            Hint: localization.translate('Identity_Check_Email_Hint'),
            Placeholder: localization.translate('Identity_Check_Email_Placeholder')
        });
    }

    displayPopup({
        Title: localization.translate('Identity_Check'),
        Fields: identityCheckFields,
        Buttons: buttons
    });
}