import './message-box.css';
import { hideLoader } from '@modules/loader';
import { generateChecklistItem } from '@modules/html-elements';

let displayMessageCallbackTimeout = null;
let displayMessageTimeout = null;

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindEscapeKey();
    bindCancelButton();
}

function bindEscapeKey() {
    $(document).on('keyup', (e) => {
        if (e.key === 'Escape') {
            hideMessage();
        }
    });
}

function bindCancelButton() {
    $(document).on('click', '.btn-cancel', () => {
        hideMessage();
    });
}

export function displayMessage(title, message, errors, callbackFn) {
    hideLoader();

    $('#alert-message-modal').remove();
    $('body').append(`
        <div id="alert-message-modal" class="modal pt-lg-4" tabindex="-1" role="dialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${title}</h5>
                    </div>
                    <div class="modal-body modal-message">${message}</div>
                    <div class="modal-body modal-error" display="none"></div>
                    <div class="modal-footer">
                        <div class="row">
                            <div class="col-12">
                                <button type="button" class="btn btn-cancel col-6" data-dismiss="modal">${localization.translate('Close')}</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `);
    $('#alert-message-modal .modal-error').hide();

    if (errors && errors.length > 0) {
        let errorMessage = `<b>${localization.translate('Details')}:</b>`;
        errorMessage += `<div class="checklist">`;
        errors.forEach((error) => {
            errorMessage += generateChecklistItem('', 'none', error, false, false);
        });
        errorMessage += `</div>`;
        $('#alert-message-modal .modal-error').html(errorMessage);
        $('#alert-message-modal .modal-error').show();
    } else {
        $('#alert-message-modal .modal-error').text('');
    }

    $('#alert-message-modal .btn').off('click').on('click', (e) => {
        clearTimeout(displayMessageCallbackTimeout);
        if (callbackFn !== undefined && callbackFn !== null) {
            displayMessageCallbackTimeout = setTimeout(() => { callbackFn(); }, 200);
        }
    });

    $('#alert-message-modal').modal('show');

    clearTimeout(displayMessageTimeout);
    displayMessageTimeout = setTimeout(() => {
        if ($('#alert-message-modal').is(':visible')) {
            hideMessage();
            if (callbackFn !== undefined && callbackFn !== null) {
                callbackFn();
            }
        }
    }, 10000);
}

export function hideMessage() {
    $('#alert-message-modal').modal('hide');
    hideLoader();
}

init();