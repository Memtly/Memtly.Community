function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindEscapeKey();
    bindCancelButton();
}

function bindEscapeKey() {
    $(document).on('keyup', function (e) {
        if (e.key === 'Escape') {
            hidePopup();
        }
    });
}

function bindCancelButton() {
    $(document).on('click', '.btn-cancel', function () {
        hidePopup();
    });
}

export function displayPopup(options, callbackFn) {
    let fields = '';
    (options?.Fields ?? [])?.forEach((field, index) => {
        let input = '';

        if (field?.Type?.toLowerCase() == 'checkbox') {
            fields += `<div class="row pb-0 ${field?.Type?.toLowerCase() == 'hidden' ? "d-none" : ""}">
                <div class="col-12">
                    <input type="checkbox" id="popup-modal-field-${field?.Id}" class="${field?.Class ?? ""}" aria-describedby="${field?.DescribedBy ?? ""}" ${field?.Checked ? "checked=\"checked\"" : ""} aria-label="${field?.Name}" ${field?.Disabled ? "disabled=\"disabled\"" : ""} /> ${field?.Label}
                </div>
            </div>`;
        } else {
            if (field?.Type?.toLowerCase() == 'select') {
                let options = ``;
                let selectOptions = field?.SelectOptions ?? [];
                if (selectOptions.length > 0) {
                    selectOptions.forEach((option, index) => {
                        options += `<option value="${option?.key}" ${option?.selected ? "selected=\"selected\"" : ""}>${option?.value}</option>`;
                    });
                    input = `<select id="popup-modal-field-${field?.Id}" class="form-control ${field?.Class ?? ""}" aria-describedby="${field?.DescribedBy ?? ""}" placeholder="${field?.Placeholder ?? ""}" aria-label="${field?.Name}" ${field?.Disabled ? "disabled=\"disabled\"" : ""}>${options}</select>`;
                }
            } else {
                input = `<input type="${field?.Type?.toLowerCase() ?? "text"}" id="popup-modal-field-${field?.Id}" class="form-control ${field?.Class ?? ""}" aria-describedby="${field?.DescribedBy ?? ""}" placeholder="${field?.Placeholder ?? ""}" value="${field?.Value ?? ""}" aria-label="${field?.Name}" ${field?.Disabled ? "disabled=\"disabled\"" : ""} ${field?.Accept ? "accept=\"" + field?.Accept + "\"" : ""} />`;
            }

            if (input !== undefined && input.length > 0) {
                fields += `<div class="row pb-3 ${field?.Type?.toLowerCase() == 'hidden' ? "d-none" : ""}">
                    <div class="col-12">
                        <label>${field?.Name ?? ""}</label>
                        ${input}
                        <div class="form-text">${field?.Hint ?? ""}</div>
                    </div>
                </div>`;
            }
        }
    });

    let message = '';
    if (options?.Message != undefined && options?.Message.length > 0) {
        message = `<div class="row pb-3">
            <div class="col-12 modal-message"></div>
        </div>`;
    }

    let buttons = '';
    (options?.Buttons ?? [{ Text: localization.translate('Close') }])?.forEach((button, index) => {
        buttons += `<div class="col-${12 / options?.Buttons?.length ?? 1}">
            <button type="button" id="popup-modal-button-${index}" class="btn btm-sm ${button?.Class ?? "btn-cancel"} popup-modal-button col-12">${button?.Text ?? localization.translate('Close')}</button>
        </div>`;
    });

    $('body').loading({
        theme: 'dark',
        message: '',
        stoppable: false,
        start: true
    });

    $('.popup-modal').remove();
    $('.popup-modal-button').off('click');
    $('body').append(`<div class="modal popup-modal pt-lg-4" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"></h5>
                </div>
                <div class="modal-body">
                    ${options?.CustomHtml ?? ''}
                    ${fields}
                    ${message}
                    ${options?.FooterHtml ?? ''}
                    <div class="row ${(options?.Fields?.length ?? 0) > 0 ? "pt-3" : ""}">
                        ${buttons}
                    </div>
                </div>
            </div>
        </div>
    </div>`);
    $('.popup-modal .modal-title').text(options.Title);
    $('.popup-modal .modal-message').text(options?.Message ?? "");

    (options?.Buttons ?? [{ Text: localization.translate('Close') }])?.forEach((button, index) => {
        $(`#popup-modal-button-${index}`).off('click').on('click', function () {
            hidePopup();

            if (button?.Callback) {
                button?.Callback();
            }
        });
    });

    $('.popup-modal').show();

    if (callbackFn !== undefined && callbackFn !== null) {
        callbackFn();
    }
}

export function hidePopup() {
    $('body').loading('stop');
    $('.popup-modal').hide();
}

init();