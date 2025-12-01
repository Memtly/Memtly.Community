let presentationTimeout = null;

export function initPresentation() {
    bindEventHandlers();
}

function bindEventHandlers() {
    if ($('div.navbar-options').length === 0) {
        presentationTimeout = setTimeout(hidePresentation, 1000);

        $(document).off('mousemove').on('mousemove', () => {
            showPresentation();
            resetTimeout();
        });
    }
}

function hidePresentation() {
    $('.presentation-hidden').fadeOut(500);
    $('body').css('cursor', 'none');
}

function showPresentation() {
    $('.presentation-hidden').fadeIn(200);
    $('body').css('cursor', 'default');
}

function resetTimeout() {
    clearTimeout(presentationTimeout);
    presentationTimeout = setTimeout(hidePresentation, 1000);
}