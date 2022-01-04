
function getAntiForgeryToken() {
    var elements = document.getElementsByName('__RequestVerificationToken');
    if (elements.length > 0) {
        return elements[0].value
    }

    console.warn('no anti forgery token found!');
    return null;
}