import { displayMessage } from '@modules/message-box';
import { displayPopup, hidePopup } from '@modules/popups';
import { displayLoader, hideLoader } from '@modules/loader';
import { displayIdentityCheck } from '@modules/identity-check';
import { refreshGalleryPage } from '@pages/gallery';

class UploadBox {
    constructor() {
        this.maxRetries = 5;
        this.retryDelay = 2000;
    }

    init() {
        this.initializeDropZones();
    }

    isIdentityRequired() {
        return $('form.file-uploader-form').attr('data-identity-required') === 'true';
    }

    triggerSelector(event) {
        if (this.isIdentityRequired()) {
            displayIdentityCheck(true, () => {
                this.triggerSelector(event);
            });
            return;
        }

        const zone = event.target.closest('fieldset.upload_drop');
        const input = $(zone.querySelector('input.upload-input'));

        if (input.data('post-allow-camera') === true) {
            this.showUploadMethodPopup(input);
        } else {
            this.setGalleryMode(input);
            input[0].click();
        }
    }

    showUploadMethodPopup(input) {
        displayPopup({
            Title: localization.translate('Upload'),
            Message: localization.translate('Upload_Method'),
            Buttons: [
                {
                    Text: localization.translate('Gallery'),
                    Class: "btn-primary-2",
                    Callback: () => {
                        this.setGalleryMode(input);
                        input[0].click();
                        hidePopup();
                    }
                },
                {
                    Text: localization.translate('Camera'),
                    Class: "btn-primary-2",
                    Callback: () => {
                        this.setCameraMode(input);
                        input[0].click();
                        hidePopup();
                    }
                },
                {
                    Text: localization.translate('Close')
                }
            ]
        });
    }

    setGalleryMode(input) {
        input.attr('accept', 'image/*,video/*');
        input.attr('multiple', '');
        input.removeAttr('capture');
    }

    setCameraMode(input) {
        input.attr('accept', 'image/*');
        input.attr('capture', 'environment');
        input.removeAttr('multiple');
    }

    highlight(e) {
        $(e.target).closest('.upload_drop').addClass('highlight');
    }

    unhighlight(e) {
        $(e.target).closest('.upload_drop').removeClass('highlight');
    }

    getInputAndGalleryRefs(element) {
        const zone = element.closest('fieldset.upload_drop') || false;
        const gallery = zone ? zone.querySelector('.upload_gallery') : false;
        const input = zone ? zone.querySelector('input[type="file"]') : false;
        return { input, gallery };
    }

    handleDrop(event) {
        const dataRefs = this.getInputAndGalleryRefs(event.target);
        dataRefs.files = event.dataTransfer.files;

        if (this.isIdentityRequired()) {
            displayIdentityCheck(true, () => {
                this.handleFiles(dataRefs);
            });
        } else {
            this.handleFiles(dataRefs);
        }
    }

    initializeDropZones() {
        const dropZones = document.querySelectorAll('fieldset.upload_drop');

        dropZones.forEach(zone => {
            this.setupEventHandlers(zone);
        });
    }

    setupEventHandlers(zone) {
        const dataRefs = this.getInputAndGalleryRefs(zone);

        if (!dataRefs.input) return;

        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            zone.addEventListener(eventName, preventDefaults, false);
            document.body.addEventListener(eventName, preventDefaults, false);
        });

        // Open file browser on drop area click
        ['click', 'touch'].forEach(eventName => {
            zone.addEventListener(eventName, (e) => this.triggerSelector(e), false);
        });

        // Highlighting drop area when item is dragged over it
        ['dragenter', 'dragover'].forEach(eventName => {
            zone.addEventListener(eventName, (e) => this.highlight(e), false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            zone.addEventListener(eventName, (e) => this.unhighlight(e), false);
        });

        // Handle dropped files
        zone.addEventListener('drop', (e) => this.handleDrop(e), false);

        // Handle browse selected files
        dataRefs.input.addEventListener('change', (event) => {
            dataRefs.files = event.target.files;
            this.handleFiles(dataRefs);
        }, false);
    }

    isImageFile(file) {
        return file.type.toLowerCase().startsWith('image/');
    }

    isVideoFile(file) {
        return file.type.toLowerCase().startsWith('video/');
    }

    async handleFiles(dataRefs) {
        let files = [...dataRefs.files];

        // Remove unaccepted file types
        files = files.filter(item => {
            const isAllowed = this.isImageFile(item) || this.isVideoFile(item);
            if (!isAllowed) {
                console.log(`File type '${item.type}' is not allowed. Filename: '${item.name}'`);
            }
            return isAllowed;
        });

        if (!files.length) return;

        dataRefs.files = files;
        await this.imageUpload(dataRefs);
    }

    async imageUpload(dataRefs) {
        if (this.isIdentityRequired()) {
            displayIdentityCheck(true, () => {
                dataRefs.input.click();
            });
            return;
        }

        // Multiple source routes, so double check validity
        if (!dataRefs.files || !dataRefs.input) {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_No_Files_Detected')
            );
            return;
        }

        const token = $('form.file-uploader-form input[name=\'__RequestVerificationToken\']').val();
        const galleryId = dataRefs.input.getAttribute('data-post-gallery-id');
        const url = dataRefs.input.getAttribute('data-post-url');
        const secretKey = dataRefs.input.getAttribute('data-post-key');

        if (!galleryId) {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_Invalid_Gallery_Detected')
            );
            return;
        }

        if (!url) {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_Invalid_Upload_Url')
            );
            return;
        }

        let uploadedCount = 0;
        let requiresReview = true;
        let errors = [];

        const processFileUpload = (i, retries = 0) => {
            if (i < dataRefs.files.length) {
                const formData = new FormData();
                formData.append('__RequestVerificationToken', token);
                formData.append('Id', galleryId);
                formData.append('SecretKey', secretKey);
                formData.append(dataRefs.files[i].name, dataRefs.files[i]);

                displayLoader(
                    `${localization.translate('Upload_Progress')} ${i + 1}/${dataRefs.files.length}...<br/><br/><span id="file-upload-progress">0%</span>`
                );

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    async: true,
                    cache: false,
                    contentType: false,
                    dataType: 'json',
                    processData: false,
                    success: (response) => {
                        if (response?.success === true) {
                            requiresReview = response.requiresReview;
                            uploadedCount++;
                        } else if (response?.errors?.length > 0) {
                            errors.push(response.errors);
                        }
                        processFileUpload(i + 1);
                    },
                    xhr: () => {
                        const xhr = new window.XMLHttpRequest();

                        xhr.upload.addEventListener("progress", (evt) => {
                            if (evt.lengthComputable) {
                                const percentComplete = Math.floor((evt.loaded / evt.total) * 100);
                                const progressElement = $('span#file-upload-progress');
                                if (progressElement.length > 0) {
                                    progressElement.text(`(${percentComplete}%)`);
                                }
                            }
                        }, false);

                        xhr.upload.addEventListener("error", (evt) => {
                            console.error(evt);
                            if (retries < this.maxRetries) {
                                setTimeout(() => {
                                    processFileUpload(i, retries + 1);
                                }, this.retryDelay);
                            } else {
                                displayMessage(
                                    localization.translate('Upload'),
                                    localization.translate('Upload_Failed'),
                                    errors
                                );
                            }
                        }, false);

                        return xhr;
                    },
                });
            } else {
                this.handleUploadComplete(uploadedCount, requiresReview, errors, galleryId, secretKey, dataRefs);
            }
        };

        processFileUpload(0);
    }

    handleUploadComplete(uploadedCount, requiresReview, errors, galleryId, secretKey, dataRefs) {
        hideLoader();

        if (uploadedCount <= 0) {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_Failed'),
                errors
            );
        } else if (requiresReview) {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_Success_Pending_Review'),
                errors
            );

            this.notifyUploadCompleted(galleryId, secretKey, uploadedCount, dataRefs);
        } else {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_Success'),
                errors,
                () => refreshGalleryPage()
            );
        }
    }

    notifyUploadCompleted(galleryId, secretKey, uploadedCount, dataRefs) {
        const formData = new FormData();
        formData.append('Id', galleryId);
        formData.append('SecretKey', secretKey);
        formData.append('Count', uploadedCount);

        setTimeout(() => {
            $.ajax({
                url: '/Gallery/UploadCompleted',
                type: 'POST',
                data: formData,
                async: true,
                cache: false,
                contentType: false,
                dataType: 'json',
                processData: false,
                success: (response) => {
                    dataRefs.input.value = '';

                    const counter = $('.review-counter');
                    if (counter.length > 0) {
                        counter.find('.review-counter-total').text(response.counters.total);
                        counter.find('.review-counter-approved').text(response.counters.approved);
                        counter.find('.review-counter-pending').text(response.counters.pending);
                    }
                },
                error: (response) => {
                    console.error(response);
                    displayMessage(
                        localization.translate('Upload'),
                        localization.translate('Upload_Failed'),
                        [response]
                    );
                }
            });
        }, 500);
    }
}

const galleryUpload = new UploadBox();

export default galleryUpload;