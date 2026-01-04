let auditSearchTimeout = null;

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindAuditSearchBox();
}

function bindAuditSearchBox() {
    $(document).off('keyup', 'input#audit-log-search-term, input#audit-log-limit').on('keyup', 'input#audit-log-search-term, input#audit-log-limit', function (e) {
        let term = $('input#audit-log-search-term').val();
        let limit = $('input#audit-log-limit').val();

        searchAudit(term, limit);
    });
}

export function searchAudit(term = '', limit = 10) {
    clearTimeout(auditSearchTimeout);
    auditSearchTimeout = setTimeout(() => {
        updateAuditList(term, limit);
    }, 500);
}

export function resetAudit() {
    updateAuditList('');
}

export function updateAuditList(term = '', limit = 10) {
    $.ajax({
        type: 'POST',
        url: `/Account/AuditList`,
        data: { term: term?.trim(), limit: limit },
        success: function (data) {
            $('#audit-list').html(data);
            bindEventHandlers();
        }
    });
}

export default init;