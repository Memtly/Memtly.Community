export function uuidv4() {
    return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, c =>
        (+c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> +c / 4).toString(16)
    );
}

export function getTimestamp() {
    var datetime = new Date();
    return `${pad(datetime.getFullYear(), 4)}-${pad(datetime.getMonth(), 2)}-${pad(datetime.getDate(), 2)}_${pad(datetime.getHours(), 2)}-${pad(datetime.getMinutes(), 2)}-${pad(datetime.getSeconds(), 2)}`;
}

export function pad(num, size) {
    num = num.toString();
    while (num.length < size) num = "0" + num;
    return num;
}

export function downloadBlob(filename, contentType, data, xhr) {
    const downloadUrl = URL.createObjectURL(new Blob([data], { type: contentType }));

    const a = document.createElement('a');
    a.href = downloadUrl;

    let fileName = filename;
    if (xhr) {
        const disposition = xhr.getResponseHeader('Content-Disposition');
        if (disposition && disposition.indexOf('attachment') !== -1) {
            const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
            const matches = filenameRegex.exec(disposition);
            if (matches != null && matches[1]) {
                fileName = matches[1].replace(/['"]/g, '');
            }
        }
    }

    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(downloadUrl);
}