import { default as initMultiFactorAuth } from '@modules/multifactor-auth';

let accountStateCheckInterval = null;

function init() {
    const tab = getActiveTab()?.toLowerCase();
    if (tab === 'reviews') {
        import('@pages/account/partials/review').then(({ default: init }) => { init(); });
    } else if (tab === 'galleries') {
        import('@pages/account/partials/gallery').then(({ default: init }) => { init(); });
    } else if (tab === 'users') {
        import('@pages/account/partials/user').then(({ default: init }) => { init(); });
    } else if (tab === 'resources') {
        import('@pages/account/partials/custom-resources').then(({ default: init }) => { init(); });
    } else if (tab === 'settings') {
        import('@pages/account/partials/settings').then(({ default: init }) => { init(); });
    } else if (tab === 'audit') {
        import('@pages/account/partials/audit').then(({ default: init }) => { init(); });
    } else if (tab === 'data') {
        import('@pages/account/partials/data').then(({ default: init }) => { init(); });
    }

    initMultiFactorAuth();

    bindEventHandlers();
}

function bindEventHandlers() {
    bindTabSelector();
    bindAccountLogoutCheck();
}

function bindTabSelector() {
    $(document).off('click', 'a.pnl-selector').on('click', 'a.pnl-selector', function (e) {
        preventDefaults(e);

        let tab = $(this).data('tab');
        selectActiveTab(tab);
    });
}

function bindAccountLogoutCheck() {
    clearInterval(accountStateCheckInterval);
    accountStateCheckInterval = setInterval(() => {
        checkAccountState();
    }, 60000);
}

function getDefaultTab() {
    return $('a.pnl-selector')[0].attributes['data-tab'].value;
}

function getActiveTab() {
    return $('a.pnl-selector.active')[0].attributes['data-tab'].value;
}

function selectActiveTab(tab) {
    if (tab === undefined || tab === null || tab.length === 0) {
        tab = getDefaultTab()();
    }

    window.location = `/Account?tab=${tab}`;
}

function checkAccountState() {
    $.ajax({
        url: '/Account/CheckAccountState',
        method: 'GET'
    })
        .done(data => {
            if (data.active !== true) {
                location.href = '/Account/Logout';
            }
        });
}

export default init;