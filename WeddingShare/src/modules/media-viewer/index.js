import './media-viewer.css';
import { displayLoader, hideLoader } from '@modules/loader';

class MediaViewer {
    constructor() {
        this.playButtonTimeout = null;
        this.resizePopupTimeout = null;
        this.touchStartPosX = null;
        this.touchStartPosY = null;
    }

    init() {
        clearTimeout(this.playButtonTimeout);
        this.playButtonTimeout = setTimeout(() => {
            $('.media-viewer-item .media-viewer-play').each(function () {
                const element = $(this);
                const preview = element.parent();
                let thumbnail = $(preview.find('img')[0]);

                let adjustSizeFn = function () {
                    let size = element.height();
                    preview.css('height', `${thumbnail.outerHeight()}px`);

                    element.css({
                        'top': `-${(thumbnail.outerHeight() / 2)}px`,
                        'left': `${(thumbnail.outerWidth() / 2)}px`,
                        'margin-top': `-${size / 2}px`,
                        'margin-left': `-${size / 2}px`
                    });

                    element.fadeTo(200, 1.0);
                }

                thumbnail.on('load', adjustSizeFn);
                element.on('load', adjustSizeFn);

                adjustSizeFn();
            });
        }, 200);

        this.bindEventHandlers();
    }

    bindEventHandlers() {
        this.bindOpenPopup();
        this.bindClosePopup();
        this.bindRightClick();
        this.bindMultiSelectButton();
        this.bindScroll();
        this.bindArrowKeys();
        this.bindLikeButton();
        this.bindDownloadButton();
    }

    bindOpenPopup() {
        $(document).off('click', '.media-viewer-item').on('click', '.media-viewer-item', (e) => {
            e.preventDefault();
            e.stopPropagation();

            const element = $(e.currentTarget);

            this.openMediaViewer(element);
        });
    }

    bindLoadEvent() {
        $('.media-viewer-image').on('load', (e) => {
            const element = $(e.currentTarget).closest('.media-viewer');
            const type = element.data('type');
            const source = element.data('source');
            this.initMediaViewImage(type, source);
        });
    }

    bindRightClick() {
        $(document).off('contextmenu', '.image-tile').on('contextmenu', '.image-tile', (e) => {
            e.preventDefault();
            e.stopPropagation();
        });
    }

    bindMultiSelectButton() {
        $(document).off('click', '.btn-multi-select').on('click', '.btn-multi-select', function (e) {
            preventDefaults(e);
            $(this).toggleClass('fa-square').toggleClass('fa-square-check');

            if ($('.btn-multi-select.fa-square-check').length > 0) {
                $('.btn-bulk-delete-resources').removeClass('d-none');
            } else {
                $('.btn-bulk-delete-resources').addClass('d-none');
            }
        });
    }

    bindScroll() {
        $(document).off('click touchstart touchend', '.media-viewer .media-viewer-content').on('click touchstart touchend', '.media-viewer .media-viewer-content', (e) => {
            e.preventDefault();
            e.stopPropagation();

            const element = $(e.currentTarget);

            if (e.originalEvent.type === 'click') {
                let position = e.pageX - element.offset().left;
                if (position <= (element.width() / 2)) {
                    this.moveSlide(-1);
                } else {
                    this.moveSlide(1);
                }
            } else if (e.originalEvent.type === 'touchstart') {
                touchStartPosX = e.touches[0].screenX;
                touchStartPosY = e.touches[0].screenY;
            } else if (e.originalEvent.type === 'touchend') {
                let touchEndPosX = e.changedTouches[0].screenX;
                let touchEndPosY = e.changedTouches[0].screenY;

                let touchDiffX = Math.abs(touchStartPosX - touchEndPosX);
                let touchDiffY = Math.abs(touchStartPosY - touchEndPosY);

                if (touchDiffX > 100) {
                    if (touchEndPosX < touchStartPosX) {
                        this.moveSlide(1);
                    } else if (touchEndPosX > touchStartPosX) {
                        this.moveSlide(-1);
                    }
                } else if (touchDiffY > 100) {
                    if (touchEndPosY < touchStartPosY) {
                        this.moveSlide(1);
                    } else if (touchEndPosY > touchStartPosY) {
                        this.moveSlide(-1);
                    }
                } else {
                    let position = e.changedTouches[0].pageX - element.offset().left;
                    if (position <= (element.width() / 2)) {
                        this.moveSlide(-1);
                    } else {
                        this.moveSlide(1);
                    }
                }
            }
        });
    }

    bindArrowKeys() {
        $(document).on('keyup', (e) => {
            if ($('.media-viewer .media-viewer-content').is(':visible')) {
                if (e.key === 'Escape') {
                    this.hideMediaViewer();
                } else if (e.key === 'ArrowLeft') {
                    this.moveSlide(-1);
                } else if (e.key === 'ArrowRight') {
                    this.moveSlide(1);
                } else if (e.key === 'd') {
                    this.download();
                }
            }
        });
    }

    bindClosePopup() {
        $(document).off('click', 'div#media-viewer-popup').on('click', 'div#media-viewer-popup', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.hideMediaViewer();
        });

        $(document).off('click', '.media-viewer-close').on('click', '.media-viewer-close', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.hideMediaViewer();
        });

        $(document).off('click', 'div.media-viewer').on('click', 'div.media-viewer', (e) => {
            e.preventDefault();
            e.stopPropagation();
        });
    }

    bindLikeButton() {
        $(document).off('click', '.like-button').on('click', '.like-button', () => {
            const id = $('.media-viewer-like-button button').attr('data-like-id');
            const action = $('.media-viewer-like-button button').attr('data-action');
            this.like(id, action);
        });
    }

    bindDownloadButton() {
        $(document).off('click', '.media-viewer-download').on('click', '.media-viewer-download', (e) => {
            e.preventDefault();
            e.stopPropagation();

            const element = $(e.currentTarget).closest('.media-viewer');
            const source = element.data('source');
            this.download(source);
        });
    }

    openMediaViewer(e) {
        let id = $(e).data('media-viewer-id');
        let index = $(e).data('media-viewer-index');
        let type = $(e).data('media-viewer-type');
        let collection = $(e).data('media-viewer-collection');

        this.displayMediaViewer(id, index, type, collection);
    }

    displayMediaViewer(id, index, type, collection) {
        this.hideMediaViewer();

        displayLoader(localization.translate('Loading'));

        let url;
        if (type !== undefined && type.length > 0) {
            if (type.toLowerCase() === 'pending_review') {
                url = '/MediaViewer/ReviewItem';
            } else if (type.toLowerCase() === 'custom_resource') {
                url = '/MediaViewer/CustomResource';
            } else if (type.toLowerCase() === 'gallery_item') {
                url = '/MediaViewer/GalleryItem';
            }
        }

        if (url !== undefined && url.length > 0) {
            $.ajax({
                url: url,
                type: 'GET',
                data: { id },
                success: (response) => {
                    hideLoader();
                    $('body').append(response);
                    $('#media-viewer-popup .media-viewer').attr('data-media-viewer-index', `${index}`);
                    $('#media-viewer-popup .media-viewer').attr('data-media-viewer-collection', `${collection}`);

                    this.bindLoadEvent();
                },
                error: (response) => {
                    hideLoader();
                    console.log(response);
                }
            });
        }
    }

    hideMediaViewer() {
        $('div#media-viewer-popup').hide();
        $('div#media-viewer-popup').remove();
    }

    initMediaViewImage(type, source) {
        this.resizeMediaViewer(1, $('#media-viewer-popup'), type, source);
    }

    resizeMediaViewer(iteration, popup, type, source) {
        let container = popup.find('.media-viewer');
        let mediaContainer = container.find('.media-viewer-content');
        let media = mediaContainer.find('img');

        let margin = window.innerWidth > 900 ? 50 : 20;
        let targetWidth = popup.innerWidth() - (margin * 2);
        let targetHeight = popup.innerHeight() - (margin * 2);

        if (iteration == 1) {
            media.width(10);
        }

        if (container.outerWidth() < targetWidth && container.outerHeight() < targetHeight) {
            media.width(media.width() + 10);

            clearTimeout(this.resizePopupTimeout);
            this.resizePopupTimeout = setTimeout(() => {
                this.resizeMediaViewer(iteration + 1, popup, type, source);
            }, 5);
        } else {
            container.css({
                'top': `${(popup.innerHeight() - container.outerHeight()) / 2}px`,
                'left': `${(popup.innerWidth() - container.outerWidth()) / 2}px`
            });

            if (type === 'video') {
                let width = $('.media-viewer-content img').innerWidth();
                let height = $('.media-viewer-content img').innerHeight();
                $('.media-viewer-content').html(`
                <video width="${width}" height="${height}" controls autoplay>
                    <source src="${source}" type="video/mp4">
                    ${localization.translate('Browser_Does_Not_Support')}
                </video>
            `);
            }

            popup.fadeTo(500, 1.0);
        }
    }

    like(id, action) {
        $.ajax({
            url: '/MediaViewer/Like',
            type: 'POST',
            data: { id, action },
            success: function (response) {
                if (response !== undefined && response.success) {
                    $('.media-viewer-like-button .lbl-like-count').text(response.value);
                    if (action.toLowerCase() === 'like') {
                        $('.media-viewer-like-button button').addClass('like-button-active');
                        $('.media-viewer-like-button button').attr('data-action', 'unlike')
                    } else {
                        $('.media-viewer-like-button button').removeClass('like-button-active');
                        $('.media-viewer-like-button button').attr('data-action', 'like')
                    }
                }
            }
        });
    }

    download(source) {
        let parts = source.split('/');

        let a = document.createElement('a');
        a.href = source;
        a.download = parts[parts.length - 1];
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }

    getOrientation(item) {
        let width = item.width();
        let height = item.height();

        let orientation = 'unkown';
        if (width > height) {
            orientation = 'horizontal';
        } else if (width < height) {
            orientation = 'vertical';
        } else {
            orientation = 'square';
        }

        return orientation;
    }

    moveSlide(direction) {
        let viewer = $('.media-viewer .media-viewer-content').closest('.media-viewer');
        let index = viewer.data('media-viewer-index') + direction;
        let collection = viewer.data('media-viewer-collection');
        let items = $(`a[data-media-viewer-collection='${collection}']`);

        if (index < 0) {
            index = items.length - 1;
        } else if (index >= items.length) {
            index = 0;
        }

        let slide = $(`a[data-media-viewer-index='${index}']`);

        this.openMediaViewer(slide);
    }
}

export default MediaViewer;