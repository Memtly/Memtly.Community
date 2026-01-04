import { displayMessage } from '@modules/message-box';
import { displayPopup } from '@modules/popups';
import { displayLoader } from '@modules/loader';
import { generatePasswordValidationContainer, initPasswordValidation } from '@validation/password-validation';

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindAddUserButton();
    bindEditUserButton();
    bindChangePasswordButton();
    bindMultiFactorWipeButton();
    bindActivateUserButton();
    bindFreezeUserButton();
    bindUnfreezeUserButton();
    bindDeleteUserButton();
}

function bindAddUserButton() {
    $(document).off('click', '.btnAddUser').on('click', '.btnAddUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        displayPopup({
            Title: localization.translate('User_Create'),
            Fields: [{
                Id: 'user-name',
                Name: localization.translate('User_Name'),
                Hint: localization.translate('User_Name_Hint')
            },
            {
                Id: 'user-email',
                Name: localization.translate('User_Email'),
                Hint: localization.translate('User_Email_Hint')
            },
            {
                Id: 'user-password',
                Name: localization.translate('User_Password'),
                Hint: localization.translate('User_Password_Hint'),
                Type: "password"
            },
            {
                Id: 'user-cpassword',
                Name: localization.translate('User_Confirm_Password'),
                Hint: localization.translate('User_Confirm_Password_Hint'),
                Type: "password",
                Class: 'confirm-password'
            },
            {
                Id: 'user-level',
                Name: localization.translate('User_Level'),
                Hint: localization.translate('User_Level_Hint'),
                Type: 'select',
                SelectOptions: [
                    {
                        key: '0',
                        selected: true,
                        value: 'Free'
                    },
                    {
                        key: '1',
                        selected: false,
                        value: 'Paid'
                    },
                    {
                        key: '2',
                        selected: false,
                        value: 'Reviewer'
                    },
                    {
                        key: '3',
                        selected: false,
                        value: 'Moderator'
                    },
                    {
                        key: '4',
                        selected: false,
                        value: 'Admin'
                    }
                ]
            }],
            FooterHtml: `${generatePasswordValidationContainer('input#popup-modal-field-user-password')}`,
            Buttons: [{
                Text: localization.translate('Add'),
                Class: 'btn-primary-2',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let username = $('#popup-modal-field-user-name').val();
                    if (username == undefined || username.length == 0) {
                        displayMessage(localization.translate('User_Create'), localization.translate('User_Missing_Name'));
                        return;
                    }

                    const usernameRegex = /^[a-zA-Z0-9\-\s-_~]+$/;
                    if (!usernameRegex.test(username)) {
                        displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Name'));
                        return;
                    }

                    let email = $('#popup-modal-field-user-email').val();
                    const emailRegex = /^((?!\.)[\w\-_.]*[^.])(@[\w\-_]+)(\.\w+(\.\w+)?[^.\W])$/;
                    if (email != undefined && email.length > 0 && !emailRegex.test(email)) {
                        displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Email'));
                        return;
                    }

                    let password = $('#popup-modal-field-user-password').val();
                    if (password == undefined || password.length < 8) {
                        displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Password'));
                        return;
                    }

                    let cpassword = $('#popup-modal-field-user-cpassword').val();
                    if (password !== cpassword) {
                        displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_CPassword'));
                        return;
                    }

                    let level = $('#popup-modal-field-user-level').val();
                    if (level == undefined || level.length == 0) {
                        displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Level'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/AddUser',
                        method: 'POST',
                        data: { Username: username, Email: email, Password: password, CPassword: cpassword, Level: level }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('User_Create'), localization.translate('User_Create_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('User_Create'), localization.translate('User_Create_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('User_Create'), localization.translate('User_Create_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('User_Create'), localization.translate('User_Create_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        }, () => {
            initPasswordValidation();
        });
    });
}

function bindEditUserButton() {
    $(document).off('click', '.btnEditUser').on('click', '.btnEditUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        let canModifyAccessLevel = row.data('modify-level');

        displayPopup({
            Title: localization.translate('User_Edit'),
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }, {
                Id: 'user-name',
                Name: localization.translate('User_Name'),
                Value: row.data('user-name'),
                Hint: localization.translate('User_Name_Hint'),
                Disabled: true
            }, {
                Id: 'user-email',
                Name: localization.translate('User_Email'),
                Value: row.data('user-email'),
                Hint: localization.translate('User_Email_Hint')
            }, {
                Id: 'user-level',
                Name: localization.translate('User_Level'),
                Hint: localization.translate('User_Level_Hint'),
                Type: 'select',
                SelectOptions: canModifyAccessLevel ? [
                    {
                        key: '0',
                        selected: row.data('user-level') == '0',
                        value: 'Free'
                    },
                    {
                        key: '1',
                        selected: row.data('user-level') == '1',
                        value: 'Paid'
                    },
                    {
                        key: '2',
                        selected: row.data('user-level') == '2',
                        value: 'Reviewer'
                    },
                    {
                        key: '3',
                        selected: row.data('user-level') == '3',
                        value: 'Moderator'
                    },
                    {
                        key: '4',
                        selected: row.data('user-level') == '4',
                        value: 'Admin'
                    }
                ] : []
            }],
            Buttons: [{
                Text: localization.translate('Update'),
                Class: 'btn-primary-2',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('User_Edit'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    let email = $('#popup-modal-field-user-email').val();
                    const emailRegex = /^((?!\.)[\w\-_.]*[^.])(@[\w\-_]+)(\.\w+(\.\w+)?[^.\W])$/;
                    if (email != undefined && email.length > 0 && !emailRegex.test(email)) {
                        displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_Email'));
                        return;
                    }

                    let level = $('#popup-modal-field-user-level').val();
                    if (canModifyAccessLevel && (level == undefined || level.length == 0)) {
                        displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_Level'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/EditUser',
                        method: 'PUT',
                        data: { Id: id, Email: email, Level: level }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindChangePasswordButton() {
    $(document).off('click', '.btnChangePassword').on('click', '.btnChangePassword', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('User_Edit'),
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }, {
                Id: 'user-password',
                Name: localization.translate('User_Password'),
                Value: row.data('user-password'),
                Hint: localization.translate('User_Password_Hint'),
                Type: 'password'
            }, {
                Id: 'user-cpassword',
                Name: localization.translate('User_Confirm_Password'),
                Value: row.data('user-cpassword'),
                Hint: localization.translate('User_Confirm_Password_Hint'),
                Type: 'password',
                Class: 'confirm-password'
            }],
            FooterHtml: `${generatePasswordValidationContainer('input#popup-modal-field-user-password')}`,
            Buttons: [{
                Text: localization.translate('Update'),
                Class: 'btn-primary-2',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('User_Edit'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    let password = $('#popup-modal-field-user-password').val();
                    if (password == undefined || password.length < 8) {
                        displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_Password'));
                        return;
                    }

                    let cpassword = $('#popup-modal-field-user-cpassword').val();
                    if (password == undefined || password !== cpassword) {
                        displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_CPassword'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/ChangeUserPassword',
                        method: 'PUT',
                        data: { Id: id, Password: password, CPassword: cpassword }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        }, () => {
            initPasswordValidation();
        });
    });
}

function bindMultiFactorWipeButton() {
    $(document).off('click', '.btnWipe2FA').on('click', '.btnWipe2FA', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('2FA_Setup'),
            Message: localization.translate('2FA_Wipe_Message'),
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Wipe'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('2FA_Setup'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/MultiFactor/ResetForUser',
                        method: 'DELETE',
                        data: { userId: id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Wipe'));
                            } else if (data.message) {
                                displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindActivateUserButton() {
    $(document).off('click', '.btnActivateUser').on('click', '.btnActivateUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('Activate_User'),
            Message: `${localization.translate('Activate_User_Message')} '${row.data('user-name')}'`,
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Activate'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Activate_User'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/ActivateUser',
                        method: 'PUT',
                        data: { Id: id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('Activate_User'), localization.translate('Activate_Successfully'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Activate_User'), localization.translate('Activate_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Activate_User'), localization.translate('Activate_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Activate_User'), localization.translate('Activate_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindFreezeUserButton() {
    $(document).off('click', '.btnFreezeUser').on('click', '.btnFreezeUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('Freeze_User'),
            Message: `${localization.translate('Freeze_User_Message')} '${row.data('user-name')}'`,
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Freeze'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Freeze_User'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/FreezeUser',
                        method: 'PUT',
                        data: { Id: id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('Freeze_User'), localization.translate('Freeze_Successfully'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Freeze_User'), localization.translate('Freeze_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Freeze_User'), localization.translate('Freeze_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Freeze_User'), localization.translate('Freeze_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindUnfreezeUserButton() {
    $(document).off('click', '.btnUnfreezeUser').on('click', '.btnUnfreezeUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('Unfreeze_User'),
            Message: `${localization.translate('Unfreeze_User_Message')} '${row.data('user-name')}'`,
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Unfreeze'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Unfreeze_User'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/UnfreezeUser',
                        method: 'PUT',
                        data: { Id: id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('Unfreeze_User'), localization.translate('Unfreeze_Successfully'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Unfreeze_User'), localization.translate('Unfreeze_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Unfreeze_User'), localization.translate('Unfreeze_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Unfreeze_User'), localization.translate('Unfreeze_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindDeleteUserButton() {
    $(document).off('click', '.btnDeleteUser').on('click', '.btnDeleteUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        let name = row.data('user-name');
        displayPopup({
            Title: localization.translate('User_Delete'),
            Message: localization.translate('Delete_Are_You_Sure'),
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Delete'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('User_Delete'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/DeleteUser',
                        method: 'DELETE',
                        data: { id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('User_Delete'), localization.translate('User_Delete_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('User_Delete'), localization.translate('User_Delete_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('User_Delete'), localization.translate('User_Delete_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('User_Delete'), localization.translate('User_Delete_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

export function updateUsersList() {
    $.ajax({
        type: 'GET',
        url: `/Account/UsersList`,
        success: function (data) {
            $('#users-list').html(data);
            bindEventHandlers();
        }
    });
}

export default init;