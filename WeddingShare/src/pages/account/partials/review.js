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
    bindReviewApprovalButton();
    bindReviewRejectButton();
    bindBulkReviewButton();
}

function bindReviewApprovalButton() {
    $(document).off('click', '.btnReviewApprove').on('click', '.btnReviewApprove', function (e) {
        preventDefaults(e);
        reviewPhoto($(this), 1);
    });
}

function bindReviewRejectButton() {
    $(document).off('click', '.btnReviewReject').on('click', '.btnReviewReject', function (e) {
        preventDefaults(e);
        reviewPhoto($(this), 2);
    });
}

function bindBulkReviewButton() {
    $(document).off('click', '.btnBulkReview').on('click', '.btnBulkReview', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        const items = $('div#pending-reviews .btn-multi-select.fa-square-check');
        let ids = items.map(function () { return $(this).data('id'); }).get();

        if (ids === undefined || ids.length === 0) {
            displayPopup({
                Title: localization.translate('Bulk_Review'),
                Message: localization.translate('Bulk_Review_Message'),
                Buttons: [{
                    Text: localization.translate('Approve'),
                    Class: 'btn-primary-2',
                    Callback: function () {
                        displayLoader(localization.translate('Loading'));

                        $.ajax({
                            url: '/Account/BulkReview',
                            method: 'POST',
                            data: { action: 1, ids: [] }
                        })
                            .done(data => {
                                if (data.success === true) {
                                    updatePendingReviews();
                                    hideLoader();
                                } else if (data.message) {
                                    displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Approve_Failed'), [data.message]);
                                } else {
                                    displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Approve_Failed'));
                                }
                            })
                            .fail((xhr, error) => {
                                displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Approve_Failed'), [error]);
                            });
                    }
                }, {
                    Text: localization.translate('Reject'),
                    Class: 'btn-danger',
                    Callback: function () {
                        displayLoader(localization.translate('Loading'));

                        $.ajax({
                            url: '/Account/BulkReview',
                            method: 'POST',
                            data: { action: 2, ids: [] }
                        })
                            .done(data => {
                                if (data.success === true) {
                                    updatePendingReviews();
                                    hideLoader();
                                } else if (data.message) {
                                    displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Reject_Failed'), [data.message]);
                                } else {
                                    displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Reject_Failed'));
                                }
                            })
                            .fail((xhr, error) => {
                                displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Reject_Failed'), [error]);
                            });
                    }
                }, {
                    Text: localization.translate('Close')
                }]
            });
        } else {
            displayPopup({
                Title: `${localization.translate('Bulk_Review')} (${ids.length})`,
                Message: localization.translate('Bulk_Review_Message_MultiSelect'),
                Buttons: [{
                    Text: localization.translate('Approve'),
                    Class: 'btn-primary-2',
                    Callback: function () {
                        displayLoader(localization.translate('Loading'));

                        $.ajax({
                            url: '/Account/BulkReview',
                            method: 'POST',
                            data: { action: 1, ids: ids }
                        })
                            .done(data => {
                                if (data.success === true) {
                                    updatePendingReviews();
                                    hideLoader();
                                } else if (data.message) {
                                    displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Approve_Failed_MultiSelect'), [data.message]);
                                } else {
                                    displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Approve_Failed_MultiSelect'));
                                }
                            })
                            .fail((xhr, error) => {
                                displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Approve_Failed_MultiSelect'), [error]);
                            });
                    }
                }, {
                    Text: localization.translate('Reject'),
                    Class: 'btn-danger',
                    Callback: function () {
                        displayLoader(localization.translate('Loading'));

                        $.ajax({
                            url: '/Account/BulkReview',
                            method: 'POST',
                            data: { action: 2, ids: ids }
                        })
                            .done(data => {
                                if (data.success === true) {
                                    updatePendingReviews();
                                    hideLoader();
                                } else if (data.message) {
                                    displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Reject_Failed_MultiSelect'), [data.message]);
                                } else {
                                    displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Reject_Failed_MultiSelect'));
                                }
                            })
                            .fail((xhr, error) => {
                                displayMessage(localization.translate('Bulk_Review'), localization.translate('Bulk_Review_Reject_Failed_MultiSelect'), [error]);
                            });
                    }
                }, {
                    Text: localization.translate('Close')
                }]
            });
        }
    });
}

export function updatePendingReviews() {
    const page = getQueryParam('page') ?? 1;
    const limit = getQueryParam('limit') ?? 50;

    $.ajax({
        type: 'GET',
        url: `/Account/PendingReviews?page=${page}&limit=${limit}`,
        success: function (data) {
            $('#pending-reviews').html(data);
            bindEventHandlers();
        }
    });
}

function reviewPhoto(element, action) {
    var id = element.data('id');
    if (!id) {
        displayMessage(localization.translate('Review'), localization.translate('Review_Id_Missing'));
        return;
    }

    displayLoader(localization.translate('Loading'));

    $.ajax({
        url: '/Account/ReviewPhoto',
        method: 'POST',
        data: { id, action }
    })
        .done(data => {
            hideLoader();

            if (data.success === true) {
                element.closest('.pending-approval').remove();
                //updateGalleryList();
                if ($('.pending-approval').length == 0) {
                    updatePendingReviews();
                }
            } else if (data.message) {
                displayMessage(localization.translate('Review'), localization.translate('Review_Failed'), [data.message]);
            }
        })
        .fail((xhr, error) => {
            hideLoader();
            displayMessage(localization.translate('Review'), localization.translate('Review_Failed'), [error]);
        });
}

export default init;