import { displayMessage } from '@modules/message-box';
import { displayPopup } from '@modules/popups';
import { displayLoader, hideLoader } from '@modules/loader';
import { downloadBlob } from '@utilities/blobs';

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindGallerySettingsButton();
    bindOpenGalleryButton();
    bindDownloadGalleryButton();
    bindAddGalleryButton();
    bindEditGalleryButton();
    bindWipeGalleryButton();
    bindWipeAllGalleriesButton();
    bindDeleteGalleryButton();
}

export function bindGallerySettingsButton() {
    $(document).off('click', '.btnGallerySettings').on('click', '.btnGallerySettings', function (e) {
        preventDefaults(e);

        let galleryId = $(this).data('gallery-id');

        $.ajax({
            type: 'GET',
            url: `/Account/Settings/${galleryId}`,
            success: function (data) {
                if (data !== undefined) {
                    displayPopup({
                        Title: localization.translate('Gallery_Settings'),
                        CustomHtml: data,
                        Buttons: [{
                            Text: localization.translate('Save'),
                            Class: 'btn-primary-2',
                            Callback: function () {
                                let updatedFields = $('.setting-field[data-updated="true"]');
                                if (updatedFields.length > 0) {
                                    var settingsList = $.map(updatedFields, function (item) {
                                        let element = $(item);
                                        return { key: element.data('setting-name'), value: element.val() };
                                    });

                                    displayLoader(localization.translate('Loading'));
                                    $.ajax({
                                        url: '/Account/UpdateGallerySettings',
                                        method: 'PUT',
                                        data: { model: settingsList, galleryId: galleryId }
                                    })
                                        .done(data => {
                                            if (data.success === true) {
                                                displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Success'), null, function () {
                                                    window.location.reload();
                                                });
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
                            }
                        }, {
                            Text: localization.translate('Close')
                        }]
                    });
                } else {
                    displayMessage(localization.translate('Gallery_Settings'), localization.translate('Gallery_Settings_None'));
                }
            }
        });
    });
}

function bindOpenGalleryButton() {
    $(document).off('click', '.btnOpenGallery').on('click', '.btnOpenGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        window.open($(this).data('url'), $(this).data('target'));
    });
}

function bindDownloadGalleryButton() {
    $(document).off('click', '.btnDownloadGallery').on('click', '.btnDownloadGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        displayLoader(localization.translate('Generating_Download'));

        let row = $(this).closest('tr');
        let id = row.data('gallery-id');
        let name = row.data('gallery-name');
        let secretKey = row.data('gallery-key');

        $.ajax({
            url: '/Gallery/DownloadGallery',
            method: 'POST',
            data: { Id: id, SecretKey: secretKey },
            xhrFields: {
                responseType: 'blob'
            },
        })
            .done((data, status, xhr) => {
                hideLoader();

                try {
                    downloadBlob(`${name}_${getTimestamp()}.zip`, 'application/zip', data, xhr);
                } catch {
                    displayMessage(localization.translate('Download'), localization.translate('Download_Failed'));
                }
            })
            .fail((xhr, error) => {
                hideLoader();
                displayMessage(localization.translate('Download'), localization.translate('Download_Failed'), [error]);
            });
    });
}

function bindAddGalleryButton() {
    $(document).off('click', '.btnAddGallery').on('click', '.btnAddGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        $.ajax({
            url: '/Gallery/GenerateSecretKey',
            method: 'GET'
        })
            .done(secretKey => {
                displayPopup({
                    Title: localization.translate('Gallery_Create'),
                    Fields: [{
                        Id: 'gallery-name',
                        Name: localization.translate('Gallery_Name'),
                        Hint: localization.translate('Gallery_Name_Hint')
                    }, {
                        Id: 'gallery-key',
                        Name: localization.translate('Gallery_Secret_Key'),
                        Hint: localization.translate('Gallery_Secret_Key_Hint'),
                        Value: secretKey
                    }],
                    Buttons: [{
                        Text: localization.translate('Create'),
                        Class: 'btn-primary-2',
                        Callback: function () {
                            displayLoader(localization.translate('Loading'));

                            let name = $('#popup-modal-field-gallery-name').val();
                            if (name == undefined || name.length == 0) {
                                displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Missing_Name'));
                                return;
                            }

                            const regex = /^[a-zA-Z0-9\-\s-_~]+$/;
                            if (!regex.test(name)) {
                                displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Invalid_Name'));
                                return;
                            }

                            let key = $('#popup-modal-field-gallery-key').val();

                            $.ajax({
                                url: '/Account/AddGallery',
                                method: 'POST',
                                data: { Id: 0, Name: name, SecretKey: key }
                            })
                                .done(data => {
                                    if (data.success === true) {
                                        updateGalleryList();
                                        displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Success'));
                                    } else if (data.message) {
                                        displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Failed'), [data.message]);
                                    } else {
                                        displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Failed'));
                                    }
                                })
                                .fail((xhr, error) => {
                                    displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Failed'), [error]);
                                });
                        }
                    }, {
                        Text: localization.translate('Close')
                    }]
                });
            });
    });
}

function bindEditGalleryButton() {
    $(document).off('click', '.btnEditGallery').on('click', '.btnEditGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('Gallery_Edit'),
            Fields: [{
                Id: 'gallery-id',
                Value: row.data('gallery-id'),
                Type: 'hidden'
            }, {
                Id: 'gallery-name',
                Name: localization.translate('Gallery_Name'),
                Value: row.data('gallery-name'),
                Hint: localization.translate('Gallery_Name_Hint')
            }, {
                Id: 'gallery-key',
                Name: localization.translate('Gallery_Secret_Key'),
                Value: row.data('gallery-key'),
                Hint: localization.translate('Gallery_Secret_Key_Hint')
            }],
            Buttons: [{
                Text: localization.translate('Update'),
                Class: 'btn-primary-2',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-gallery-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Missing_Id'));
                        return;
                    }

                    let name = $('#popup-modal-field-gallery-name').val();
                    if (name == undefined || name.length == 0) {
                        displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Missing_Name'));
                        return;
                    }

                    let key = $('#popup-modal-field-gallery-key').val();

                    $.ajax({
                        url: '/Account/EditGallery',
                        method: 'PUT',
                        data: { Id: id, Name: name, SecretKey: key }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateGalleryList();
                                displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindWipeGalleryButton() {
    $(document).off('click', '.btnWipeGallery').on('click', '.btnWipeGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        let name = row.data('gallery-name');
        displayPopup({
            Title: localization.translate('Gallery_Wipe'),
            Message: `${name} - ${localization.translate('Gallery_Wipe_Message')}`,
            Fields: [{
                Id: 'gallery-id',
                Value: row.data('gallery-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Wipe'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-gallery-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Gallery_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/WipeGallery',
                        method: 'DELETE',
                        data: { id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateGalleryList();
                                displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Gallery_Wipe_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Gallery_Wipe_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Gallery_Wipe_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Gallery_Wipe_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindWipeAllGalleriesButton() {
    $(document).off('click', '.btnWipeAllGalleries').on('click', '.btnWipeAllGalleries', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        displayPopup({
            Title: localization.translate('Wipe_Data'),
            Message: localization.translate('Wipe_Data_Message'),
            Buttons: [{
                Text: localization.translate('Wipe'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    $.ajax({
                        url: '/Account/WipeAllGalleries',
                        method: 'DELETE'
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateGalleryList();
                                displayMessage(localization.translate('Wipe_Data'), localization.translate('Wipe_Data_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Wipe_Data'), localization.translate('Wipe_Data_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Wipe_Data'), localization.translate('Wipe_Data_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Wipe_Data'), localization.translate('Wipe_Data_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindDeleteGalleryButton() {
    $(document).off('click', '.btnDeleteGallery').on('click', '.btnDeleteGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        let name = row.data('gallery-name');
        displayPopup({
            Title: localization.translate('Gallery_Delete'),
            Message: localization.translate('Delete_Are_You_Sure'),
            Fields: [{
                Id: 'gallery-id',
                Value: row.data('gallery-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Delete'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-gallery-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Gallery_Delete'), localization.translate('Gallery_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/DeleteGallery',
                        method: 'DELETE',
                        data: { id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateGalleryList();
                                displayMessage(localization.translate('Gallery_Delete'), localization.translate('Gallery_Delete_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Gallery_Delete'), localization.translate('Gallery_Delete_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Gallery_Delete'), localization.translate('Gallery_Delete_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Gallery_Delete'), localization.translate('Gallery_Delete_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

export function updateGalleryList() {
    $.ajax({
        type: 'GET',
        url: `/Account/GalleriesList`,
        success: function (data) {
            $('#galleries-list').html(data);
        }
    });
}

export default init;