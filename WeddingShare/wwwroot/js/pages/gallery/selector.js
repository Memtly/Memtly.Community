import { displayMessage } from '../../../components/message-box';
import { uuidv4 } from '../../../components/utilities';

function bindEventHandlers() {
    $(document).off('click', '#btnGenerateGalleryName').on('click', '#btnGenerateGalleryName', function (e) {
        preventDefaults(e);
        $('input#gallery-id').val(uuidv4());
    });

    $(document).off('submit', '#frmSelectGallery').on('submit', '#frmSelectGallery', function (e) {
        preventDefaults(e);

        var galleryId = $('input#gallery-id,select#gallery-id').val().trim();
        var secretKey = $('input#gallery-key').val().trim();

        const regex = /^[a-zA-Z0-9\-\s\-_~]+$/;
        if (galleryId && galleryId.length > 0 && regex.test(galleryId)) {
            $.ajax({
                type: "POST",
                url: '/Gallery/Login',
                data: { identifier: galleryId, key: secretKey },
                success: function (data) {
                    if (data.success && data.redirectUrl) {
                        window.location = data.redirectUrl;
                    } else {
                        displayMessage(localization.translate('Gallery'), localization.translate('Gallery_Invalid_Gallery_Or_Secret_Key'));
                    }
                }
            });
        } else {
            displayMessage(localization.translate('Gallery'), localization.translate('Gallery_Invalid_Name'));
        }
    });
}

bindEventHandlers();