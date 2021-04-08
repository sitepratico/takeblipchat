const { end } = require("@popperjs/core");

function init() {
    console.log("init()");

    //updater();
    window.canUpdate = true;
    //setInterval(function () { updater() }, 7000);
    setTimeout(function () { updater() }, 15000);
}

function updater() {
    console.log("updater()");

    if (window.canUpdate) {

        isLogged = !UtilIsObjectNullOrUndefinedOrEmpty($('#sessionid').val());

        if (isLogged) {
            window.document.forms[0].submit();
        }
    }
}

function UtilIsUndefined(Data) {
    return (typeof Data === "undefined");
}

function UtilIsNull(Data) {
    return (Data === null)
}

function UtilIsEmpty(Data) {
    return (Data.length == 0);
}

function UtilIsNullOrUndefined(Data) {
    return UtilIsNull(Data) || UtilIsUndefined(Data);
}

function UtilIsNullOrUndefinedOrEmpty(Data) {
    return UtilIsNull(Data) || UtilIsUndefined(Data) || UtilIsEmpty(Data);
}

function UtilIsObjectNullOrUndefinedOrEmpty(Data) {
    var Return = UtilIsNull(Data) || UtilIsUndefined(Data) || UtilIsEmpty(Data);

    // CONFERE SE É UM OBJETO VAZIO
    if (!Return) {
        IsObject = typeof Data === 'object' && Data !== null;

        if (IsObject) {
            Return = Object.keys(Data).length == 0;
        }
    }

    return Return;
}

function UtilIsNumeric(Data) {
    return !isNaN(Data);
}