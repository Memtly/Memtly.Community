export function setCookie(cname, cvalue, hours) {
    let consent = getCookie('.AspNet.Consent');
    if (consent !== undefined && consent === 'yes') {
        const d = new Date();
        d.setTime(d.getTime() + (hours * 60 * 60 * 1000));
        document.cookie = `${cname}=${cvalue};expires=${d.toUTCString()};path=/`;
    } else {
        console.warn(`Cannot set cookie '${cname}' as the user has not accepted the cookie policy`);
    }
}

export function getCookie(cname) {
    let ca = document.cookie.split(';');
    let name = `${cname}=`;

    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];

        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }

        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }

    return "";
}