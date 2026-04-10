const html = {
    encode: function (value) { return $('<div />').text(value).html(); },
    decode: function (value) { return $('<div/>').html(value).text(); }
};

const notification = {
    popup: null,
    success: function (message) {
        if (notification.popup != null && notification.popup.show != null)
            notification.popup.show(message, "success");
        else
            console.log('Success', message);
    },
    info: function (message) {
        if (notification.popup != null && notification.popup.show != null)
            notification.popup.show(message, "info");
        else
            console.log('Info', message);
    },
    warning: function (message) {
        if (notification.popup != null && notification.popup.show != null)
            notification.popup.show(message, "warning");
        else
            console.log('Warning', message);
    },
    error: function (message) {
        if (notification.popup != null && notification.popup.show != null)
            notification.popup.show(message, "error");
        else
            console.log('Error', message);
    }
};

const type = {
    dateFormat: 'yyyy-MM-dd',
    moneyFormat: 'n',
    aggregateMoneyFormat: 'n0'
};

String.prototype.replaceAll = function (search, replacement) { return this.split(search).join(replacement); };

async function date() {
    let dateElement = document.getElementById('date');

    if (dateElement) {
        let today = new Date();
        dateElement.innerHTML = today.getDate() + ' ' + await api.getResource("Default", "month-" + (today.getMonth() + 1), session.culture).ShortDisplayName + ' ' + today.getFullYear();
    }
}

function time() {
    let timeElement = document.getElementById('time');

    let appendZero = function (i) {
        if (i < 10) { i = "0" + i; }  // add zero in front of numbers < 10
        return i;
    };

    if (timeElement) {
        let today = new Date();
        let h = today.getHours();
        let m = today.getMinutes();
        let s = today.getSeconds();
        m = appendZero(m);
        s = appendZero(s);
        timeElement.innerHTML = h + ":" + m + ":" + s;
        setTimeout(function () { time(); }, 500);
    }
}

function getQueryParameter(name) {
    let query = window.location.search.replace("?", "").split('&').map(p => { return { k: p.split('=')[0], v: p.split('=')[1] }; });
    let result = null;

    for (let i in query) {
        if (query[i].k === name) { result = query[i].v; }
    }

    return result !== null ? unescape(result) : result;
}

function setQueryParameter(name, value) {
    let query = window.location;

    if (query.search === '') {
        let result = window.location.href + '?' + name + '=' + value;
        window.location.href = result;
    } else {
        let queryStrings = query.search
            .replace('?', '')
            .split('&');

        let validStrings = queryStrings.filter(function (s) {
            return s.indexOf(name + '=') === -1;
        });

        let newSearch = '?' + validStrings.join('&') + (validStrings.length > 0 ? '&' : '') + [name, value].join('=');
        let currentUrl = window.location.href;
        let baseUrl = currentUrl.split('?')[0];
        window.location.href = baseUrl + newSearch;
    }
}


function removeQueryParameter(key, sourceURL) {
    let rtn = sourceURL.split("?")[0],
        param,
        params_arr = [],
        queryString = (sourceURL.indexOf("?") !== -1) ? sourceURL.split("?")[1] : "";

    if (queryString !== "") {
        params_arr = queryString.split("&");

        for (let i = params_arr.length - 1; i >= 0; i -= 1) {
            param = params_arr[i].split("=")[0];
            if (param === key) {
                params_arr.splice(i, 1);
            }
        }

        rtn = rtn + "?" + params_arr.join("&");
    }
    return rtn;
};


    async function loadComponent(container, componentName, callback) {
        let result = await api
            .get("Core/Component/Render()?AppId=" + session.app.Id + "&Name=" + componentName + "&culture=" + session.culture + "&theme=" + session.theme);

        try {
            $(container).append(result.value);

            if (callback) {
                callback(window[componentName]);
            }

            return window[componentName];
        } 
        catch (ex) {
            $(container).empty();
            $(container).append($("<h3>Component Loading Failed</h3><p>" + componentName + " could not be found or loaded because of the following exception:<br>" + ex.message + "</p>"));
            $(container).append($("<pre>" + ex.stack + "</pre>"));
        }
    }

function clone(object) {
    return JSON.parse(JSON.stringify(object));
}

function log(data, level) {

    if (data.responseText) {
        if (data.responseText.startsWith("{")) {
            var item = JSON.parse(data.responseText) || data.responseJson;

            if (item.Message) {
                log(item.Message, level);
            }

            if (item.message) {
                log(item.message, level);
            }

            if (item.error) {
                log(item.error, level);
            }
            else if (item.ErrorMessage) {
                log(item.ErrorMessage, "error");
            }
            else {
                log(data.responseText, level);
            }
        }
        else {
            log(data.responseText, level);
        }

    } else if(notification) {
        if (level === "log") {
            notification.info(data);
        }
        else if (notification[level]) {
            notification[level](data);
        }
    }

    if (console[level]) {
        console[level](data);
    }
}

async function error(e) {
    log(e, "error");
    if (typeof e == 'string') {
        if (e.indexOf("Access Denied!") !== -1) {
            notification.error(await api.getResource("Core", "AccessDenied", "").Description);
        }
        else {
            notification.error(await api.getResource("Core", "ServerRejectedAttemptedAction", "").Description);
        }
    } else if (typeof e == 'object') {
        if (e.hasOwnProperty("message")) {
            if (e.message.indexOf("Access Denied!") !== -1) {
                notification.error(await api.getResource("Core", "AccessDenied", "").Description);
            }
            else {
                notification.error(await api.getResource("Core", "ServerRejectedAttemptedAction", "").Description);
            }
        }
    }
}

function Guid() {
    return crypto.randomUUID();
};

function initContent() {
    // auto init components dropped on the page
    $.each($(".component"), function (i, c) {
        try {
            let name = $(c).attr("name");

            if (window[name]) {
                window[name].init(session.app, $(c));
            }
        }
        catch (ex) {
            log("Component loading error", "error");
            log(ex, "error");
        }
    });

    date();
    time();
}

$(async function() {
    if (session.culture && session.culture.split("-").length == 2) {
        kendo.culture(session.culture);
    }

    //setup notifcations 
    $(document.body).append("<div id='notif'></div>");
    notification.popup = $("#notif").kendoNotification({
        autoHideAfter: 5000,
        stacking: 'up',
        position: { bottom: 35, right: 10 },
        // these are not really needed as they basically put out the default
        // they are purely in place as a hook to say to me later "if you want to change notification rendering here's how".
        templates: [
            { type: "success", template: $("<div><div class='notification'><p>#= data.content #</p></div></div>").html() },
            { type: "info", template: $("<div><div class='notification'><p>#= data.content #</p></div></div>").html() },
            { type: "warning", template: $("<div><div class='notification'><p>#= data.content #</p></div></div>").html() },
            { type: "error", template: $("<div><div class='notification'><p>#= data.content #</p></div></div>").html() }
        ]
    }).data("kendoNotification");
});

function getMonthlyDateRange(start, end, format) {
    let months = [];
    let current = new Date(start);
    current.setUTCDate(1);

    while (current < end) {
        let thisMonth = new Date(current);
        let thisStart = new Date(thisMonth);
        let thisEnd = new Date(thisMonth);
        thisEnd.setUTCMonth(thisEnd.getUTCMonth() + 1);
        thisEnd.setUTCDate(-0);

        months.push({
            from: thisStart.toISOString().split('T')[0],
            to: thisEnd.toISOString().split('T')[0],
            formatted: kendo.toString(thisMonth, format)
        });

        current.setUTCMonth(current.getUTCMonth() + 1);
    }

    return months;
}