import { displayMessage } from '@modules/message-box';
import { displayPopup } from '@modules/popups';
import { displayLoader, hideLoader } from '@modules/loader';
import { getTimestamp } from '@utilities/datetime';
import { downloadBlob } from '@utilities/blobs';
import { default as galleryUpload } from '@modules/upload-box';
import MediaViewer from '@modules/media-viewer';
import Slideshow from '@modules/slideshow';
import { default as initSettings } from '@pages/account/partials/settings';
import { bindGallerySettingsButton } from '@pages/account/partials/gallery'

let resizeTimeout = null;
let idleTimeout = null;

let slideshow = null;

function init() {
    const slideshowSlideInterval = $('input#slideshowSlideInterval').val();
    const slideshowFadeInterval = $('input#slideshowFadeInterval').val();

    galleryUpload.init();

    slideshow = new Slideshow('#gallery-slideshow', slideshowSlideInterval, slideshowFadeInterval);
    slideshow.init();

    new MediaViewer().init();
    initSettings();
    bindEventHandlers();
}

function bindEventHandlers() {
    bindQRCodeSave();
    bindDownloadGroup();
    bindDownloadGallery();
    bindDeletePhoto();
    bindIdleRefresh();
    bindPageResizeEvent();
    bindGallerySettingsButton();
}

function bindPageResizeEvent() {
    $(window).on('resize', () => {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            slideshow.init();
        }, 200);
    });
}

function bindQRCodeSave() {
    $(document).off('click', 'button.btnSaveQRCode').on('click', 'button.btnSaveQRCode', (e) => {
        preventDefaults(e);

        if ($(e.currentTarget).attr('disabled') === 'disabled') {
            return;
        }

        const galleryName = $(e.currentTarget).data('gallery-name');
        const canvas = $('.qrcode-download canvas')[0];

        const link = document.createElement('a');
        link.download = `${galleryName}-qrcode.png`;
        link.href = canvas.toDataURL('image/png', 1.0).replace('image/png', 'image/octet-stream');
        link.click();
    });
}

function bindDownloadGroup() {
    $(document).off('click', '.btnDownloadGroup').on('click', '.btnDownloadGroup', (e) => {
        preventDefaults(e);

        if ($(e.currentTarget).attr('disabled') === 'disabled') {
            return;
        }

        displayLoader(localization.translate('Generating_Download'));

        const id = $(e.currentTarget).data('gallery-id');
        const name = $(e.currentTarget).data('gallery-name');
        const secretKey = $(e.currentTarget).data('gallery-key');
        const group = $(e.currentTarget).data('group-name');

        const items = $('div#main-gallery .btn-multi-select.fa-square-check');
        let ids = items.map(function () { return $(this).data('id'); }).get();

        $.ajax({
            url: '/Gallery/DownloadGallery',
            method: 'POST',
            data: { Id: id, SecretKey: secretKey, Group: group, FileFilter: ids },
            xhrFields: {
                responseType: 'blob'
            },
        })
            .done((data, status, xhr) => {
                hideLoader();
                try {
                    downloadBlob(`${name}_${getTimestamp()}.zip`, 'application/zip', data, xhr);
                } catch (error) {
                    displayMessage(
                        localization.translate('Download'),
                        localization.translate('Download_Failed')
                    );
                }
            })
            .fail((xhr, error) => {
                hideLoader();
                displayMessage(
                    localization.translate('Download'),
                    localization.translate('Download_Failed'),
                    [error]
                );
            });
    });
}

function bindDownloadGallery() {
    $(document).off('click', 'button.btnDownloadGallery').on('click', 'button.btnDownloadGallery', (e) => {
        preventDefaults(e);

        if ($(e.currentTarget).attr('disabled') === 'disabled') {
            return;
        }

        displayLoader(localization.translate('Generating_Download'));

        const id = $(e.currentTarget).data('gallery-id');
        const name = $(e.currentTarget).data('gallery-name');
        const secretKey = $(e.currentTarget).data('gallery-key');

        const items = $('div#main-gallery .btn-multi-select.fa-square-check');
        let ids = items.map(function () { return $(this).data('id'); }).get();

        $.ajax({
            url: '/Gallery/DownloadGallery',
            method: 'POST',
            data: { Id: id, SecretKey: secretKey, FileFilter: ids },
            xhrFields: {
                responseType: 'blob'
            },
        })
            .done((data, status, xhr) => {
                hideLoader();
                try {
                    downloadBlob(`${name}_${getTimestamp()}.zip`, 'application/zip', data, xhr);
                } catch (error) {
                    displayMessage(
                        localization.translate('Download'),
                        localization.translate('Download_Failed')
                    );
                }
            })
            .fail((xhr, error) => {
                hideLoader();
                displayMessage(
                    localization.translate('Download'),
                    localization.translate('Download_Failed'),
                    [error]
                );
            });
    });
}

function bindDeletePhoto() {
    $(document).off('click', '.btnDeletePhoto').on('click', '.btnDeletePhoto', (e) => {
        preventDefaults(e);

        if ($(e.currentTarget).attr('disabled') === 'disabled') {
            return;
        }

        const id = $(e.currentTarget).data('photo-id');
        const name = $(e.currentTarget).data('photo-name');
        const tile = $(e.currentTarget).closest('.image-tile');

        displayPopup({
            Title: localization.translate('Delete_Item'),
            Message: localization.translate('Delete_Are_You_Sure'),
            Fields: [{
                Id: 'photo-id',
                Value: id,
                Type: 'hidden'
            }],
            Buttons: [
                {
                    Text: localization.translate('Delete'),
                    Class: 'btn-danger',
                    Callback: () => {
                        displayLoader(localization.translate('Loading'));

                        const photoId = $('#popup-modal-field-photo-id').val();
                        if (!photoId || photoId.length === 0) {
                            displayMessage(
                                localization.translate('Delete_Item'),
                                localization.translate('Delete_Item_Id_Missing')
                            );
                            return;
                        }

                        $.ajax({
                            url: '/Account/DeletePhoto',
                            method: 'DELETE',
                            data: { id: photoId }
                        })
                            .done((data) => {
                                if (data.success === true) {
                                    tile.remove();
                                    displayMessage(
                                        localization.translate('Delete_Item'),
                                        localization.translate('Delete_Item_Success'),
                                        null,
                                        () => refreshGalleryPage()
                                    );
                                } else if (data.message) {
                                    displayMessage(
                                        localization.translate('Delete_Item'),
                                        localization.translate('Delete_Item_Failed'),
                                        [data.message]
                                    );
                                } else {
                                    displayMessage(
                                        localization.translate('Delete_Item'),
                                        localization.translate('Delete_Item_Failed')
                                    );
                                }
                            })
                            .fail((xhr, error) => {
                                displayMessage(
                                    localization.translate('Delete_Item'),
                                    localization.translate('Delete_Item_Failed'),
                                    [error]
                                );
                            });
                    }
                },
                {
                    Text: localization.translate('Close')
                }
            ]
        });
    });
}

function bindIdleRefresh() {
    const duration = $('input#galleryIdleRefreshInterval').val();
    if (duration > 0) {
        $(document).on('mousemove keydown scroll click', () => {
            setIdleRefresh(duration);
        });
        setIdleRefresh(duration);
    }
}

function setIdleRefresh(duration) {
    clearTimeout(idleTimeout);
    idleTimeout = setTimeout(() => {
        refreshGalleryPage(bindIdleRefresh);
    }, duration);
}

export function refreshGalleryPage(callback) {
    $.ajax({
        type: 'GET',
        url: `${window.location.pathname}${window.location.search}&partial=true`,
        success: (data) => {
            $('#main-gallery').html(data);
            if (typeof callback === 'function') {
                callback();
            }
        }
    });
}

export default init;