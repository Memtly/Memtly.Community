import { displayMessage } from '@modules/message-box';
import { displayPopup } from '@modules/popups';
import { displayLoader, hideLoader } from '@modules/loader';
import { getTimestamp } from '@utilities/datetime';
import { downloadBlob } from '@utilities/blobs';

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindImportButton();
    bindExportButton();
}

function bindImportButton() {
    $(document).off('click', '.btnImport').on('click', '.btnImport', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        displayPopup({
            Title: localization.translate('Import_Data'),
            Fields: [{
                Id: 'import-file',
                Name: localization.translate('Import_Data_Backup_File'),
                Type: 'File',
                Hint: localization.translate('Import_Data_Backup_Hint'),
                Accept: '.zip'
            }],
            Buttons: [{
                Text: localization.translate('Import'),
                Class: 'btn-primary-2',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    var files = $('#popup-modal-field-import-file')[0].files;
                    if (files == undefined || files.length == 0) {
                        displayMessage(localization.translate('Import_Data'), localization.translate('Import_Data_Select_File'));
                        return;
                    }

                    var data = new FormData();
                    data.append('file-0', files[0]);

                    $.ajax({
                        url: '/Account/ImportBackup',
                        method: 'POST',
                        data: data,
                        contentType: false,
                        processData: false
                    })
                        .done(data => {
                            if (data.success === true) {
                                displayMessage(localization.translate('Import_Data'), localization.translate('Import_Data_Success'));
                                window.location.reload();
                            } else if (data.message) {
                                displayMessage(localization.translate('Import_Data'), localization.translate('Import_Data_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Import_Data'), localization.translate('Import_Data_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Import_Data'), localization.translate('Import_Data_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindExportButton() {
    $(document).off('click', '.btnExport').on('click', '.btnExport', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let exportOptions = [];

        let includeDatabaseOption = false;
        if (includeDatabaseOption) {
            exportOptions.push({
                Id: 'database',
                Type: 'checkbox',
                Checked: true,
                Class: 'form-check-input',
                Label: 'Database'
            });
        }

        exportOptions.push({
            Id: 'uploads',
            Type: 'checkbox',
            Checked: true,
            Class: 'form-check-input',
            Label: 'Uploads'
        });

        exportOptions.push({
            Id: 'thumbnails',
            Type: 'checkbox',
            Checked: true,
            Class: 'form-check-input',
            Label: 'Thumbnails'
        });

        exportOptions.push({
            Id: 'custom-resources',
            Type: 'checkbox',
            Checked: true,
            Class: 'form-check-input',
            Label: 'Custom Resources'
        });

        displayPopup({
            Title: localization.translate('Export_Data'),
            Fields: exportOptions,
            Buttons: [{
                Text: localization.translate('Export'),
                Class: 'btn-primary-2',
                Callback: function () {
                    displayLoader(localization.translate('Generating_Download'));

                    $.ajax({
                        url: '/Account/ExportBackup',
                        method: 'POST',
                        data: {
                            Database: includeDatabaseOption ? $('#popup-modal-field-database').is(':checked') : false,
                            Uploads: $('#popup-modal-field-uploads').is(':checked'),
                            Thumbnails: $('#popup-modal-field-thumbnails').is(':checked'),
                            CustomResources: $('#popup-modal-field-custom-resources').is(':checked')
                        },
                        xhrFields: {
                            responseType: 'blob'
                        }
                    })
                        .done((data, status, xhr) => {
                            hideLoader();

                            try {
                                downloadBlob(`WeddingShare_${getTimestamp()}.zip`, 'application/zip', data, xhr);
                            } catch (ex) {
                                displayMessage(localization.translate('Export_Data'), localization.translate('Export_Data_Failed'), [ex]);
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Export_Data'), localization.translate('Export_Data_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

export default init;