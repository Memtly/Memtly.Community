import { displayMessage } from '@modules/message-box';
import { displayPopup } from '@modules/popups';
import { displayLoader, hideLoader } from '@modules/loader';
import MediaViewer from '@modules/media-viewer';
import { getQueryParam } from '@utilities/urls';

function init() {
    new MediaViewer().init();
    bindEventHandlers();
}

function bindEventHandlers() {
    bindSearchBox();
    bindUploadCustomResourceInput();
    bindDeleteCustomResourceButton();
    bindRelinkCustomResourceButton();
    bindBulkDeleteCustomResourceButton();
}

function bindSearchBox() {
    $(document).off('keyup', 'input#custom-resources-search-term').on('keyup', 'input#custom-resources-search-term', function (e) {
        const term = $('input#custom-resources-search-term').val();

        const url = new URL(window.location.href);
        url.searchParams.set('term', term);
        url.searchParams.set('page', '1');

        history.pushState({}, '', url);

        updateCustomResources();
    });
}

function bindUploadCustomResourceInput() {
    $(document).off('change', 'input#custom-resource-upload').on('change', 'input#custom-resource-upload', function (e) {
        const files = $(this)[0].files;
        let retries = 0;

        function uploadCustomResource(i) {
            if (files !== undefined && files.length > 0) {
                const formData = new FormData();
                formData.append(files[i].name, files[i]);

                displayLoader(`${localization.translate('Upload_Progress', { index: i + 1, count: 1 })} <br/><br/><span id="file-upload-progress">0%</span>`);

                $.ajax({
                    url: '/Account/UploadCustomResource',
                    type: 'POST',
                    data: formData,
                    async: true,
                    cache: false,
                    contentType: false,
                    dataType: 'json',
                    processData: false,
                    success: function (response) {
                        hideLoader();
                        if (response !== undefined && response.success === true) {
                            displayMessage(localization.translate('Upload'), localization.translate('Upload_Success', { count: 1 }));

                            updateCustomResources();
                            //updateSettings();

                            $('input#custom-resource-upload').val('');
                        } else if (response.errors !== undefined && response.errors.length > 0) {
                            displayMessage(localization.translate('Upload'), localization.translate('Upload_Failed'), [response.errors]);
                        }
                    },
                    xhr: function () {
                        var xhr = new window.XMLHttpRequest();

                        xhr.upload.addEventListener("progress", function (evt) {
                            if (evt.lengthComputable) {
                                var percentComplete = evt.loaded / evt.total;
                                percentComplete = parseInt(percentComplete * 100);

                                if ($('span#file-upload-progress').length > 0) {
                                    $('span#file-upload-progress').text(`(${percentComplete}%)`);
                                }
                            }
                        }, false);

                        xhr.upload.addEventListener("error", function (evt) {
                            console.log(evt);
                            if (retries < 5) {
                                setTimeout(() => {
                                    retries++;
                                    uploadCustomResource(i);
                                }, 2000);
                            } else {
                                displayMessage(localization.translate('Upload'), localization.translate('Upload_Failed'));
                            }
                        }, false);

                        return xhr;
                    },
                });
            }
        }

        uploadCustomResource(0);
    });
}

function bindRelinkCustomResourceButton() {
    $(document).off('click', '.custom-resource-relink').on('click', '.custom-resource-relink', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let id = $(this).data('id');
        let element = $(this).closest('.custom-resource');

        displayPopup({
            Title: localization.translate('Custom_Resource_Relink'),
            Fields: [{
                Id: 'custom-resource-id',
                Value: id,
                Type: 'hidden'
            }, {
                Id: 'custom-resource-username',
                Name: localization.translate('Username'),
                Value: element.data('custom-resource-username'),
                Hint: localization.translate('Relink_Username_Hint')
            }],
            Buttons: [{
                Text: localization.translate('Update'),
                Class: 'btn-primary-2',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-custom-resource-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Custom_Resource_Relink'), localization.translate('Custom_Resource_Missing_Id'));
                        return;
                    }

                    let username = $('#popup-modal-field-custom-resource-username').val();
                    if (username == undefined || username.length == 0) {
                        displayMessage(localization.translate('Custom_Resource_Relink'), localization.translate('Missing_Username'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/RelinkCustomResource',
                        method: 'PUT',
                        data: { Id: id, UploadedBy: username }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateCustomResources();
                                displayMessage(localization.translate('Custom_Resource_Relink'), localization.translate('Custom_Resource_Relink_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Custom_Resource_Relink'), localization.translate('Custom_Resource_Relink_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Custom_Resource_Relink'), localization.translate('Custom_Resource_Relink_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Custom_Resource_Relink'), localization.translate('Custom_Resource_Relink_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindDeleteCustomResourceButton() {
    $(document).off('click', '.custom-resource-delete').on('click', '.custom-resource-delete', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let id = $(this).data('id');
        let name = $(this).data('name');
        let element = $(this).closest('.custom-resource');

        displayPopup({
            Title: localization.translate('Delete_Item'),
            Message: localization.translate('Delete_Are_You_Sure'),
            Fields: [{
                Id: 'custom-resource-id',
                Value: id,
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Delete'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-custom-resource-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Id_Missing'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/RemoveCustomResource',
                        method: 'DELETE',
                        data: { id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Success'));

                                updateCustomResources();
                                //updateSettings();

                                element.remove();
                            } else if (data.message) {
                                displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindBulkDeleteCustomResourceButton() {
    $(document).off('click', '.btn-bulk-delete-resources').on('click', '.btn-bulk-delete-resources', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        const items = $('div#custom-resources .btn-multi-select.fa-square-check');
        let ids = items.map(function () { return $(this).data('id'); }).get();

        displayPopup({
            Title: `${localization.translate('Bulk_Delete_Items')} (${items.length})`,
            Message: localization.translate('Delete_Are_You_Sure'),
            Fields: [{
                Id: 'custom-resource-ids',
                Value: ids.join(),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Delete'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let ids = $('#popup-modal-field-custom-resource-ids').val().split(',');
                    if (ids == undefined || ids.length == 0) {
                        displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Id_Missing'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/BulkRemoveCustomResource',
                        method: 'DELETE',
                        data: { ids }
                    })
                        .done(data => {
                            if (data.success === true) {
                                displayMessage(localization.translate('Bulk_Delete_Items'), localization.translate('Bulk_Delete_Items_Success'));

                                updateCustomResources();
                                //updateSettings();

                                $('div#custom-resources .btn-multi-select.fa-square-check').remove();
                                $('.btn-bulk-delete-resources').addClass('d-none');
                            } else if (data.message) {
                                displayMessage(localization.translate('Bulk_Delete_Items'), localization.translate('Bulk_Delete_Items_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Bulk_Delete_Items'), localization.translate('Bulk_Delete_Items_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Bulk_Delete_Items'), localization.translate('Bulk_Delete_Items_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

export function updateCustomResources() {
    const term = getQueryParam('term') ?? '';
    const page = getQueryParam('page') ?? 1;
    const limit = getQueryParam('limit') ?? 50;

    $.ajax({
        type: 'GET',
        url: `/Account/CustomResources?term=${term}&page=${page}&limit=${limit}`,
        success: function (data) {
            $('#custom-resources').html(data);
            bindEventHandlers();

            const url = new URL(window.location.href);
            url.searchParams.set('term', term);

            history.pushState({}, '', url);
        }
    });
}

export default init;