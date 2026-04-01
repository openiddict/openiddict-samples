
function getAntiForgeryToken() {
    var elements = document.getElementsByName('__RequestVerificationToken');
    if (elements.length > 0) {
        return elements[0].value
    }

    console.warn('No anti forgery token was found in the document.');
    return null;
}