/* Api Management */
class Api
{
    constructor(args) {
        args = args || {};
        this.apiRoot = args.apiRoot || session.apiRoot;

        this.cache = {
            meta: [],
            resources: [],
            resourceLoads: []
        };
        this.app = args.app || session.app;
        this.file = {
            upload: async (to, file, callback) => {
                return new Promise((resolve, reject) => {
                    try {
                        let reader = new FileReader();
                        reader.onload = () => this.onFileReaderLoad(to, file, reader, resolve, callback);
                        reader.readAsArrayBuffer(file);
                    } catch(err) {
                        reject(err);
                    }
                });
            },

            destroy: async (path) => {
                await this.destroy("DMS/" + path)
                    .then(() => notification.success("File deleted."));
            }
        };
    }

    onFileReaderLoad(to, file, reader, resolve, callback) {
        let xhr = new XMLHttpRequest();
        xhr.open('POST', this.apiRoot + "DMS/" + to, true);
        xhr.setRequestHeader("Content-Type", file.type);

        xhr.onreadystatechange = () =>
            this.onReadyStateChange(xhr, file, resolve, callback);

        xhr.send(new Uint8Array(reader.result));
    }

    onReadyStateChange(xhr, file, resolve, callback) {
        switch (xhr.readyState) {
            case 2: //HEADERS_RECEIVED
                if (notification)
                    notification.info("Uploading file " + file.name + " ...");
                break;
            case 4: //DONE
                if (notification) {
                    notification.success(file.name + " has been uploaded.");
                } else {
                    notification.fail(file.name + " failed to upload.");
                    log(file.name + " upload fail.", "success");
                }
                resolve(file);
                if (callback)
                    callback(file);
                break;
            default:
                break;
        }
    }

    async login(user, pass, keepToken) {
        return this.send("POST", "Account/Login", {
            User: user,
            Pass: pass
        }).then((token) => {
            if (keepToken)
                this.token = token.id;
            return token;
        }).catch((e) => {
            error(e);
            return false;
        });
    }

    async logout() {
        if (this.token)
            delete this.token;

        return this.send("POST", "Account/Logout", '').catch((e) => error(e));
    }

    async register(details) {
        return api.post("Account/Register", details);
    }

    addToMetaCache(metaSet) {
        for (let i in metaSet) {
            let ctx = metaSet[i];
            let existing = this.cache.meta.filter(x => x.Name == ctx.Name);

            if (existing.length > 0) {
                for (let t in ctx.Types) {
                    existing[0].Types.push(ctx.Types[t]);
                }
            }
            else
                this.cache.meta.push(ctx);
        }
    }

    addToResourceCache(resourceSet) {
        for (let r in resourceSet)
            this.cache.resources.push(resourceSet[r]);
    }

    getType(endpointRef) {
        let context = endpointRef.split('/')[0];
        let typeName = endpointRef.split('/')[1];

        if (this.cache.meta.filter(x => x.Name === context).length === 0)
            throw new Error("Missing " + context + " type information in meta cache");

        let typeGroup = this.cache.meta.filter(x => x.Name === context)[0];

        if (typeGroup.Types.filter(x => x.ServerTypeName === typeName).length === 0)
            throw new Error("Missing " + endpointRef + " type information in meta cache");

        return typeGroup.Types.filter(x => x.ServerTypeName === typeName)[0];
    }

    getResource(key, name, culture) {
        culture = culture || "";
        if (!key || !name) {
            return {
                Key: key,
                Name: name,
                DisplayName: name,
                ShortDisplayName: name,
                Description: name
            };
        }

        const subSet = this.cache.resources.filter(r => r.Key.toLowerCase() === key.toLowerCase() && r.Name.toLowerCase() === name.toLowerCase());
        const resultSet = subSet
            .filter(r => culture === r.Culture || (culture.indexOf(r.Culture) > -1) || r.Culture === '')
            .sort((a, b) => b.Culture.length - a.Culture.length);

        return (resultSet.length > 0)
            ? resultSet[0]
            : {
                Key: key,
                Name: name,
                DisplayName: name,
                ShortDisplayName: name,
                Description: name
            };
    }

    async send(type, query, data, contentType) {
        // if no query provided set to blank //
        query = query || '';
        // construct the promise //
        // if not return promise for client to handle //
        return new Promise((resolve, reject) => {
            // create the config that will be used by the ajax call //
            const ajaxConfig = {
                type: type,
                contentType: contentType || 'application/json',
                crossDomain: window.location.href.indexOf(this.apiRoot) > -1,
                url: this.apiRoot + query,
                success: resolve,
                error: reject,
                beforeSend: (xhr) => this.beforeSend(xhr, contentType),
                //timeout: this.timeout
            };

            // if we have been provided data, then add it to the call //
            if (data) {
                ajaxConfig.data = JSON.stringify(data);
            }

            $.ajax(ajaxConfig);
        });
    }

    async sendRaw(type, query, data) {
        // if no query provided set to blank //
        query = query || '';
        // construct the promise //
        // if not return promise for client to handle //
        return new Promise((resolve, reject) => {
            // create the config that will be used by the ajax call //
            const ajaxConfig = {
                type: type,
                contentType: "text/plain",
                crossDomain: window.location.href.indexOf(this.apiRoot) > -1,
                url: this.apiRoot + query,
                success: resolve,
                error: reject,
                beforeSend: (xhr) => this.beforeSend(xhr, "text/plain"),
                data: data
            };

            $.ajax(ajaxConfig);
        });
    }

    beforeSend(xhr, contentType) {
        xhr.setRequestHeader("Content-Type", contentType || "application/json;odata=minimalmetadata");
        xhr.setRequestHeader("Accept", contentType || "application/json;odata=minimalmetadata");

        if (this.token) {
            xhr.setRequestHeader("Authorization", "bearer " + this.token);
        }
    }

    async get(query) {
        return this.send("GET", query, null);
    }

    async add(query, model) {
        return this.send("POST", query, model);
    }

    async update(query, model) {
        return this.send("PUT", query, model);
    }

    async post(query, model) {
        return this.send("POST", query, model);
    }

    async put(query, model) {
        return this.send("PUT", query, model);
    }

    async destroy(query) {
        return this.send("DELETE", query, null);
    }

    success() {
        notification.success("Request Complete!");
    }
}

window.api = new Api({
    baseUrl: session.apiRoot,
    token: session.token
});
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
var form = {
    get: async function(id) {
        let formData = await api.get("Core/Form(" + id + ")");
        formData.render = form.render;
        return formData;
    },

    new: async function(appId) {
        let newForm = await api.get("Core/Form/NewForm()");
        newForm.render = form.render;
        newForm.AppId = appId;
        return newForm;
    },

    render: async function(element, id, callback) {
        $(element).empty();
        let result = await api.call("Core/Form(" + id + ")/Render()", this);
        $(element).append(result.value);
    },

    modelFor: function(theForm) {
        var rootMeta = null;

        for (var i in theForm.Meta) {
            if (theForm.Meta[i].Name === theForm.RootMetaItem) {
                rootMeta = theForm.Meta[i];
                break;
            }
        }

        return kendo.observable(form.buildObject(rootMeta, theForm.Meta));
    },

    buildObject: function(meta, metaCollection) {

        if (meta !== null) {
            var res = {};
            for (var i in meta.Properties) {
                var p = meta.Properties[i];
                switch (p.Type) {
                    case "text": res[p.Name] = ""; break;
                    case "string": res[p.Name] = ""; break;
                    case "email": res[p.Name] = ""; break;
                    case "password": res[p.Name] = ""; break;
                    case "date": res[p.Name] = new Date(); break;
                    case "number": res[p.Name] = 0; break;
                    case "checkbox": res[p.Name] = false; break;
                    case "file": res[p.Name] = null; break;
                    case "array": res[p.Name] = []; break;
                    case "bool": res[p.Name] = false; break;
                    case "int": res[p.Name] = 0; break;
                    default:
                        var subMeta = null;
                        if (metaCollection) {
                            for (var j in metaCollection) {
                                if (metaCollection[j].Name === p.Type) {
                                    subMeta = metaCollection[j];
                                    break;
                                }
                            }

                            res[p.Name] = form.buildObject(metaCollection, subMeta);
                        }
                        break;
                }
            }
            return res;
        }

        return null;
    },

    getMetaForType: function(typeName, formDef) {
        var result = null;
        for (var i in formDef.Meta) {
            if (formDef.Meta[i].Name === typeName) {
                result = formDef.Meta[i];
                break;
            }
        }

        return result;
    }
};

const model = {
    clone: (obj) => JSON.parse(JSON.stringify(obj)),

    prepareItem: function (item, meta) {
        item.type = meta.Category + "/" + meta.ServerTypeName;

        meta.Properties.forEach(p => {
            if (p.Type === 'date' || p.Type === 'time') {
                item[p.Name] = item[p.Name] !== null ? new Date(item[p.Name]) : item[p.Name];
            }
        });

        // add functions for working with this object
        for (let f in model.item) {
            if (f !== "createInstance") { item[f] = model.item[f]; }
        }

        return item;
    },

    prepareItemFromModelInfo: function (item, model) {
        item.type = 'dynamic';
        item.context = '';
        $.each(model.fields, function (i, f) {
            if (f.type === 'date' || f.type === 'time') {
                item[f.Name] = item[f.Name] !== null
                    ? new Date(item[f.Name])
                    : item[f.Name];
            }
        });
    },

    prepConfigForGrid: function (config, callback) {
        let ds = model.getDatasource(config.sourceInitParams);

        if (callback) {
            config.dataSource = ds;
            callback(config);
        }

        return config;
    },

    createforListOf: function (type) {
        return kendo.observable({
            type: type,
            context: context,
            items: model.getDatasource(type)
        });
    },

    createFor: async function (type, id) {
        let meta = api.getType(type);
        let result = new kendo.observable(await model.item.createInstance(type));

        result.fetch = async function (callback) {
            if (id !== 'new') {
                let res = await api.get(type + "(" + id + ")");
                model.prepareItem(res, meta);
                result.item = res;

                if (typeof callback !== 'undefined') {
                    callback();
                }
            }
            else if (typeof callback !== 'undefined') {
                callback();
            }
        };

        return result;
    },

    getDatasource: function (context) {
        let result = null;
        let modelInfo = context.model
            ? context.model
            : null;

        if (modelInfo === null) {
            var m = model.fieldsFor(context.endpoint);
            result = model.build(context, {fields: m});
        }
        else {
            result = model.build(context, modelInfo);
        }
        return result;
    },

    odata: {
        joinFilters: function(url) {
            let query = url.split("?");
            let params = query[1].split("&");

            if (params.filter(p => p.indexOf("$filter=") > -1).length > 1) {

                let filter = "$filter=" + params
                    .filter(p => p.indexOf("$filter=") > -1).map(f => f.replace("$filter=", ""))
                    .join(" AND ");

                params = params.filter(p => p.indexOf("$filter=") === -1);
                params.push(filter);
                query[1] = params.join("&");
            }

            return query[0] + "?" + query[1];
        }
    },

    build: function (context, m) {
        if (!context.base) { context.base = session.apiRoot; }
        if (!context.serverOptions) {
            context.serverOptions = {
                filtering: true,
                sorting: true,
                paging: true,
                grouping: false
            };
        }

        if (!context.pageSize) {
            context.pageSize = session.app.Config.DefaultPageSize || 50;
        }

        // construct the root url to be used by the datasource
        let baseUrl = context.base + context.endpoint;

        let metaType = context.dataType !== undefined
            ? context.dataType
            : context.endpoint;

        let schemaModel = m;
        let typeInfo = api.getType(metaType);

        let dateFieldNames = typeInfo.Properties
            .filter(p => p.Type === "date")
            .map(c => c.Name);

        let result = null;

        let cfg = {
            serverFiltering: context.serverOptions.filtering,
            serverSorting: context.serverOptions.sorting,
            serverGrouping: context.serverOptions.grouping,
            serverPaging: context.serverOptions.paging,
            pageSize: context.pageSize,
            group: context.group ? context.group : [],
            filter: context.filter ? context.filter : { logic: 'and', filters: [] },
            sort: context.sort ? context.sort : [],
            type: 'odata-v4',

            change: context.onChange
                ? context.onChange
                : function (e) { /* to ensure don't try calling undefined */ },

            transport: context.transport
                ? context.transport
                : {
                    read: {
                        url: function (data) {
                            let result = baseUrl;

                            if (typeof context.odataAppend !== 'undefined') {
                                result += context.odataAppend;
                            }

                            return result;
                        },
                        dataType: 'json',
                        beforeSend: function (xhr, request) {
                            api.beforeSend(xhr);
                            request.url = request.url.replaceAll("%24", "$");
                            if (request.url.indexOf("?") > 0) {
                                request.url = model.odata.joinFilters(request.url);
                            }
                            result.lastGet = request;
                            if (context.enableExport) {
                                result.setExportUrls();
                            }
                        }
                    },

                    update: {
                        url: function (data) {
                            return baseUrl + "(" + result.getIdForUrl(data) + ")";
                        },

                        dataType: 'json',
                        beforeSend: api.beforeSend
                    },
                    destroy: {
                        url: function (data) {
                            return baseUrl + "(" + result.getIdForUrl(data) + ")";
                        },

                        dataType: 'json',
                        beforeSend: api.beforeSend
                    },
                    create: {
                        url: baseUrl,
                        dataType: 'json',
                        beforeSend: api.beforeSend
                    },
                    parameterMap: function (data, operation) {
                        let dataCopy = JSON.parse(JSON.stringify(data));

                        if (dataCopy.filter && dataCopy.filter.filters && dataCopy.filter.filters.length > 0) {
                            for (const element of dataCopy.filter.filters) {
                                let entry = element

                                if (dateFieldNames.indexOf(entry.field) !== -1) {
                                    entry.value = new Date(entry.value);
                                }
                            }
                        }

                        if (context.customFields && dataCopy.filter && dataCopy.filter.filters) {
                            let fieldsToIgnore = context.customFields
                                .map(c => c.templateField);

                            dataCopy.filter.filters = data.filter
                                .filters
                                .filter(c => fieldsToIgnore.indexOf(c.field) === -1);
                        }

                        let filterApplied = kendo.data.transports['odata-v4']
                            .parameterMap
                            .call(result, dataCopy, operation);

                        if (operation === "update") {
                            delete dataCopy.type;  // this will annoy the server, so lets remove it before we send.
                            delete dataCopy.source;
                            delete dataCopy.context;
                            return JSON.stringify(dataCopy);
                        }

                        if (operation === "read" && context.customFields) {
                            if (context.customFields) {
                                context.customFields
                                    .filter(cf => data.filter && data.filter.filters && data.filter.filters.filter(c => c.field == cf.templateField).length > 0)//where a filter has been applied
                                    .forEach(cf => {
                                        if (cf.type === "array") {

                                            let filterEntry = data.filter.filters
                                                .filter(c => c.field == cf.templateField)[0];

                                            let operatorString = "";

                                            if (filterEntry.operator === "contains") {
                                                operatorString = "contains(" + cf.field + ", '" + filterEntry.value + "')";
                                            } else {
                                                operatorString = " " + cf.field + " " + filterEntry.operator + " '" + filterEntry.value + "'";
                                            }

                                            let filterString = cf.arrayField + "/any(c: " + operatorString + (cf.fieldAppend ? cf.fieldAppend + ")" : ")");

                                            if (filterApplied.hasOwnProperty("$filter")) {
                                                filterApplied["$filter"] = "(" + filterApplied["$filter"] + ") and " + filterString;
                                            } else {
                                                filterApplied["$filter"] = filterString;
                                            }
                                        } else {
                                            let operatorString = "";

                                            if (filterEntry.operator === "contains") {
                                                operatorString = "contains(" + cf.field + ", '" + filterEntry.value + "')";
                                            } else {
                                                operatorString = " " + cf.field + " " + filterEntry.operator + " '" + filterEntry.value + "'";
                                            }

                                            let filterString = operatorString + (cf.fieldAppend ? cf.fieldAppend + ")" : ")");

                                            if (filterApplied.hasOwnProperty("$filter")) {
                                                filterApplied["$filter"] = "(" + filterApplied["$filter"] + ") and " + filterString;
                                            } else {
                                                filterApplied["$filter"] = filterString;
                                            }
                                        }
                                    });
                            }
                            if (context.postCustomServerFiltering) {
                                context.postCustomServerFiltering(data, filterApplied);
                            }
                        }

                        return filterApplied;
                    }
                },

            schema: {
                model: schemaModel,
                data: function (data) {
                    return data.value;
                },

                total: function (data) {
                    return data['@odata.count'];
                },

                parse: function (data) {
                    if ($.isArray(data.value)) {  // if we got an array of items prep each item
                        $.each(data.value, function (idx, item) {
                            model.prepareItem(item, typeInfo);

                            if (context.computeFields) {
                                context.computeFields(item);
                            }
                        });
                    }
                    else {
                        model.prepareItem(data, typeInfo);
                        if (context.computeFields) {
                            context.computeFields(item);
                        }
                    }

                    return data;
                }
            },

            error: function (e) {
                log({ event: e, source: this }, "error");
            },

            save: function () {
                this.sync();
                setTimeout(this.read, 1000);
            }
        };

        result = new kendo.data.DataSource(cfg);
        result.baseUrl = baseUrl;
        result.odataAppend = context.odataAppend;
        result.meta = typeInfo;

        result.getIdForUrl = function (data) {
            let meta = result.meta.Properties.filter(function (p) {
                return p.Template === "key";
            })[0];

            return meta.Type === "string"
                ? "'" + data[meta.Name] + "'"
                : data[meta.Name];
        };

        return result;
    },

    item: {
        createFor: function (metaArray, rootObjectType, maxDepth, currentDepth) {
            maxDepth = maxDepth || 1;
            currentDepth = currentDepth || 0;

            let meta = metaArray
                .filter(function (m) { return m.Name === rootObjectType; })[0];

            if (meta) {
                let result = {};
                for (let i in meta.Properties) {
                    let p = meta.Properties[i];

                    let propMeta = metaArray
                        .filter(function (m) { return m.ServerType === p.ServerType; })[0];

                    if (propMeta && currentDepth + 1 <= maxDepth) {
                        result[p.Name] = buildModel(metaArray, p.Name, maxDepth, currentDepth + 1);
                    }
                    else {
                        if (p.Type === 'string' || p.Type === 'password' || p.Type === 'email') { result[p.Name] = ''; }

                        if (p.Type !== 'object' && p.Type !== 'array') {
                            if (p.Type === 'date') {
                                result[p.Name] = new Date();
                            }

                            if (p.Type === 'number' || p.Type === 'key') {
                                result[p.Name] = 0;
                            }

                            if (p.Type === 'guid') {
                                result[p.Name] = '00000000-0000-0000-0000-000000000000';
                            }

                            if (p.Type === 'bool') {
                                result[p.Name] = false;
                            }
                        }
                    }
                }

                return result;
            }

            return {};
        },

        createInstance: async function (endpoint, callback) {
            let meta = await api.getType(endpoint);
            let result = { type: endpoint };

            $.each(meta.Properties, function (i, p) {
                if (p.Type !== 'object' && p.Type !== 'array') {
                    if (p.Name === 'Id') {
                        result[p.Name] = 'new';
                    } else {
                        result[p.Name] = '';
                        if (p.Type === 'date') {
                            result[p.Name] = new Date();
                        }
                        if (p.Type === 'number' || p.Type === 'key') {
                            result[p.Name] = 0;
                        }
                        if (p.Type === 'guid') {
                            result[p.Name] = model.newGuid;
                        }
                        if (p.Type === 'bool') {
                            result[p.Name] = false;
                        }
                    }
                }
            });

            model.prepareItem(result, meta);

            if (callback) {
                callback(result);
            }

            return result;
        },

        prepForPush: function () {
            // kendo will remove its gubbins, I then simply have to parse the 
            //  resulting string back in to an object to get a clean version
            var result = JSON.parse(kendo.stringify(this));

            // added by kendo / knockout during binding
            delete result.guid;

            // added by me to track the object type
            delete result.type;

            //add by me to track the apiContext that this object came from
            delete result.context;

            // returns a dto that matches the server definition of the object

            // delete auditing information
            delete result.CreatedOn;
            delete result.LastUpdated;

            return result;
        },

        save: async function (e) {
            let obj = this;
            let valid = true;

            if (e) {
                e.preventDefault();
                let form = $(e.currentTarget).closest("form");

                if (form.length > 0) {
                    try {
                        valid = form.valid();
                    }
                    catch (ex) {
                        error(ex);
                    }
                }
            }

            let url = this.type;
            let method = "update";

            log({ Operation: "Save", item: this }, "debug");
            if (valid) {
                let meta = await api.getType(this.type);
                let idProp = meta.Properties.filter(function (p) {
                    return p.Name === "Id" || p.Template === "key";
                })[0];

                let key = idProp.Type === "string"
                    ? "('" + obj[idProp.Name] + "')"
                    : "(" + obj[idProp.Name] + ")";

                // save existing entry to system
                if (obj.Id !== 'new') {
                    url += key;
                }
                // push new entry to system
                else if (idProp.type !== "string") {
                    delete obj.Id;
                    method = "add";
                }

                let payload = obj.prepForPush();
                return await api[method](url, payload);
            }
        },

        destroy: async function (e) {
            e.preventDefault();
            let retiredObject = this;
            let meta = await api.getType(this.type);

            let idProp = meta.Properties.filter(function (p) {
                return p.Template === "key";
            })[0];

            let key = idProp.Type === "string"
                ? "('" + retiredObject[idProp.Name] + "')"
                : "(" + retiredObject[idProp.Name] + ")";

            return await api.destroy(retiredObject.type + key);
        }
    },

    fieldsFor: function (endpoint) {
        let typeInfo = api.getType(endpoint);
        let result = {};

        typeInfo.Properties
            .forEach(p => result[p.Name] = { field: p.Name, type: p.Type });

        return result;
    },

    columnsFor: function (endpoint, extraCols, removeCols) {
        let typeInfo = api.getType(endpoint);
        let result = new Array();
        let scalarProperties = typeInfo.Properties.filter(p => p.IsValueType);

        for (let p in scalarProperties) {
            let prop = scalarProperties[p];
            prop.type = prop.Type;
            prop.field = prop.Name;
            prop.title = prop.ShortDisplayName;
            prop.editable = !prop.IsReadOnly;

            if (prop.Type === "date") {
                prop.format = "{0:" + type.dateFormat + "}";
                prop.filterable = {
                    ui: function (element) {
                        element.kendoDatePicker({ format: type.dateFormat })
                    }
                };

                prop.attributes = { style: "text-align: right;" };
            }

            if (prop.ServerTypeName === "Decimal") {
                prop.format = "{0:" + type.moneyFormat + "}";
            }

            if (prop.type === "number") {
                prop.attributes = { style: "text-align: right;" };
            }

            result.push(prop);
        }

        if (typeof extraCols !== 'undefined') {
            for (let i in extraCols)
                result.push(extraCols[i]);
        }

        if (typeof removeCols !== 'undefined') {
            for (let j in removeCols) {
                for (let c in result) {
                    if (result[c].field === removeCols[j] || result[c].name === removeCols[j]) {
                        result.splice(c, 1);
                    }
                }
            }
        }

        return result;
    },

    propertiesFor: function (endpoint, fields, extraFields) {
        let typeInfo = api.getType(endpoint);

        let scalarProperties = typeInfo.Properties
            .filter(p => p.IsValueType);

        return scalarProperties.filter(prop => (fields)
            ? (fields.indexOf(prop.Name) !== -1)
            : true).map((prop) => {
                let p = {
                    type: prop.Type,
                    field: prop.Name,
                    title: prop.ShortDisplayName,
                    description: prop.Description,
                    name: prop.DisplayName,
                    editable: !prop.IsReadOnly
                };

                if (prop.Type === "date") {
                    p.format = "{0:" + type.dateFormat + "}";
                    p.filterable = {
                        ui: (element) => element.kendoDatePicker({ format: type.dateFormat })
                    };

                    p.attributes = { style: "text-align: right;" };
                }

                if (prop.ServerTypeName === "Decimal") {
                    p.format = "{0:" + type.moneyFormat + "}";
                }

                if (prop.type === "number") {
                    p.attributes = { style: "text-align: right;" };
                }

                return p;
            }).concat(extraFields ? extraFields : []);
    }
};