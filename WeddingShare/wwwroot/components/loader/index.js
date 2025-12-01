import './jquery.loading.min.js';
import './loader.css';

function bindEventHandlers() {
    $(document).on('keyup', (e) => {
        if (e.key === 'Escape') {
            hideLoader();
        }
    });

    $(document).on('click', '.btn-reload', () => {
        hideLoader();
    });
}

export function displayLoader(message) {
    $('body').loading({
        theme: 'dark',
        message,
        stoppable: false,
        start: true
    });
}

export function hideLoader() {
    $('body').loading('stop');
}

bindEventHandlers();