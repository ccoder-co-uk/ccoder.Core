let model = {
    clone: (obj) => JSON.parse(JSON.stringify(obj)),

    prepareItem: function (item, meta) {
        item.type = meta.Category + "/" + meta.ServerTypeName;

        meta.Properties.forEach(p => {
            if (p.Type === 'date' || p.Type === 'time') {
                item[p.Name] = item[p.Name] !== null ? new Date(item[p.Name]) : item[p.Name];
            }
        });

        // add functions for working with this object
        for (var f in model.item) {
            if (f !== "createInstance") { item[f] = model.item[f]; }
        }

        return item;
    },

    prepareItemFromModelInfo: function (item, model) {
        item.type = 'dynamic';
        item.context = '';
        $.each(model.fields, function (i, f) {
            if (f.type === 'date' || f.type === 'time') {
                item[f.Name] = item[f.Name] !== null ? new Date(item[f.Name]) : item[f.Name];
            }
        });
    },

    prepConfigForGrid: function (config, callback) {
        var ds = model.getDatasource(config.sourceInitParams);
        if (callback) {
            config.dataSource = ds;
            callback(config);
        }
        return config;
    },

    createforListOf: function (type) {
        return kendo.observable({ type: type, context: context, items: model.getDatasource(type) });
    },

    createFor: async function (type, id) {
        var meta = api.getType(type);
        var result = new kendo.observable(await model.item.createInstance(type));
        result.fetch = async function (callback) {
            if (id !== 'new') {
                var res = await api.get(type + "(" + id + ")");
                model.prepareItem(res, meta);
                result.item = res;
                if (typeof callback !== 'undefined') {
                    callback();
                }
            }
            else {
                if (typeof callback !== 'undefined') {
                    callback();
                }
            }
        };
        return result;
    },

    getDatasource: function (context) {
        var result = null;
        var modelInfo = context.model ? context.model : null;

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
            var query = url.split("?");
            var params = query[1].split("&");
            if (params.filter(p => p.indexOf("$filter=") > -1).length > 1) {
                var filter = "$filter=" + params.filter(p => p.indexOf("$filter=") > -1).map(f => f.replace("$filter=", "")).join(" AND ");
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

        if (!context.pageSize) { context.pageSize = session.app.Config.DefaultPageSize || 50; }

        // construct the root url to be used by the datasource
        var baseUrl = context.base + context.endpoint;
        var metaType = context.dataType !== undefined ? context.dataType : context.endpoint;
        var schemaModel = m;
        let typeInfo = api.getType(metaType);
        var dateFieldNames = typeInfo.Properties.filter(p => p.Type === "date").map(c => c.Name);

        var result = null;

        var cfg = {
            serverFiltering: context.serverOptions.filtering,
            serverSorting: context.serverOptions.sorting,
            serverGrouping: context.serverOptions.grouping,
            serverPaging: context.serverOptions.paging,
            pageSize: context.pageSize,
            group: context.group ? context.group : [],
            filter: context.filter ? context.filter : { logic: 'and', filters: [] },
            sort: context.sort ? context.sort : [],
            type: 'odata-v4',
            change: context.onChange ? context.onChange : function (e) { /* to ensure don't try caling undefined */ },
            transport: context.transport ? context.transport : {
                read: {
                    url: function (data) {
                        var result = baseUrl;
                        if (typeof context.odataAppend !== 'undefined') { result += context.odataAppend; }
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
                update: { url: function (data) { return baseUrl + "(" + result.getIdForUrl(data) + ")"; }, dataType: 'json', beforeSend: api.beforeSend },
                destroy: { url: function (data) { return baseUrl + "(" + result.getIdForUrl(data) + ")"; }, dataType: 'json', beforeSend: api.beforeSend },
                create: { url: baseUrl, dataType: 'json', beforeSend: api.beforeSend },
                parameterMap: function (data, operation) {
                    var dataCopy = JSON.parse(JSON.stringify(data));

                    if (dataCopy.filter && dataCopy.filter.filters && dataCopy.filter.filters.length > 0) {
                        for (let i = 0; i < dataCopy.filter.filters.length; i++) {
                            var entry = dataCopy.filter.filters[i];
                            if (dateFieldNames.indexOf(entry.field) !== -1) {
                                entry.value = new Date(entry.value);
                            }
                        }
                    }

                    if (context.customFields && dataCopy.filter && dataCopy.filter.filters) {
                        var fieldsToIgnore = context.customFields.map(c => c.templateField);
                        dataCopy.filter.filters = data.filter.filters.filter(c => fieldsToIgnore.indexOf(c.field) === -1);
                    }

                    var filterApplied = kendo.data.transports['odata-v4'].parameterMap.call(result, dataCopy, operation);

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
                                        var filterEntry = data.filter.filters.filter(c => c.field == cf.templateField)[0];
                                        var operatorString = "";
                                        if (filterEntry.operator === "contains") {
                                            operatorString = "contains(" + cf.field + ", '" + filterEntry.value + "')";
                                        } else {
                                            operatorString = " " + cf.field + " " + filterEntry.operator + " '" + filterEntry.value + "'";
                                        }
                                        var filterString = cf.arrayField + "/any(c: " + operatorString + (cf.fieldAppend ? cf.fieldAppend + ")" : ")")
                                        if (filterApplied.hasOwnProperty("$filter")) {
                                            filterApplied["$filter"] = "(" + filterApplied["$filter"] + ") and " + filterString;
                                        } else {
                                            filterApplied["$filter"] = filterString;
                                        }
                                    } else {
                                        var operatorString = "";
                                        if (filterEntry.operator === "contains") {
                                            operatorString = "contains(" + cf.field + ", '" + filterEntry.value + "')";
                                        } else {
                                            operatorString = " " + cf.field + " " + filterEntry.operator + " '" + filterEntry.value + "'";
                                        }
                                        var filterString = operatorString + (cf.fieldAppend ? cf.fieldAppend + ")" : ")")
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
                data: function (data) { return data.value; },
                total: function (data) { return data['@odata.count']; },
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
            error: function (e) { log({ event: e, source: this }, "error"); },
            save: function () { this.sync(); setTimeout(this.read, 1000); }
        };

        result = new kendo.data.DataSource(cfg);
        result.baseUrl = baseUrl;
        result.odataAppend = context.odataAppend;
        result.meta = typeInfo;
        result.getIdForUrl = function (data) {
            var meta = result.meta.Properties.filter(function (p) { return p.Template === "key"; })[0];
            return meta.Type === "string" ? "'" + data[meta.Name] + "'" : data[meta.Name];
        };

        return result;
    },

    item: {
        createFor: function (metaArray, rootObjectType, maxDepth, currentDepth) {
            maxDepth = maxDepth || 1;
            currentDepth = currentDepth || 0;

            var meta = metaArray.filter(function (m) { return m.Name === rootObjectType; })[0];
            if (meta) {
                var result = {};
                for (var i in meta.Properties) {
                    var p = meta.Properties[i];
                    var propMeta = metaArray.filter(function (m) { return m.ServerType === p.ServerType; })[0];

                    if (propMeta && currentDepth + 1 <= maxDepth) {
                        result[p.Name] = buildModel(metaArray, p.Name, maxDepth, currentDepth + 1);
                    }
                    else {
                        if (p.Type === 'string' || p.Type === 'password' || p.Type === 'email') { result[p.Name] = ''; }

                        if (p.Type !== 'object' && p.Type !== 'array') {
                            if (p.Type === 'date') { result[p.Name] = new Date(); }
                            if (p.Type === 'number' || p.Type === 'key') { result[p.Name] = 0; }
                            if (p.Type === 'guid') { result[p.Name] = '00000000-0000-0000-0000-000000000000'; }
                            if (p.Type === 'bool') { result[p.Name] = false; }
                        }
                    }
                }

                return result;
            }

            return {};
        },

        createInstance: async function (endpoint, callback) {
            var meta = await api.getType(endpoint);
            var result = {type: endpoint};
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
            if (callback) { callback(result); }
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
            var obj = this;
            var valid = true;

            if (e) {
                e.preventDefault();
                var form = $(e.currentTarget).closest("form");
                if (form.length > 0) {
                    try {
                        valid = form.valid();
                    }
                    catch (ex) { error(ex); }
                }
            }

            var url = this.type;
            var method = "update";

            log({ Operation: "Save", item: this }, "debug");
            if (valid) {
                var meta = await api.getType(this.type);
                var idProp = meta.Properties.filter(function (p) {
                    return p.Template === "key";
                })[0];
                var key = idProp.Type === "string" ? "('" + obj[idProp.Name] + "')" : "(" + obj[idProp.Name] + ")";

                // save existing entry to system
                if (obj.Id !== 'new') {
                    url += key;
                }
                // push new entry to system
                else if (idProp.type !== "string") {
                    delete obj.Id;
                    method = "add";
                }

                var payload = obj.prepForPush();
                return await api[method](url, payload).catch((res) => error(res));
            }
        },

        destroy: async function (e) {
            e.preventDefault();
            var retiredObject = this;
            var meta = await api.getType(this.type);
            var idProp = meta.Properties.filter(function (p) {
                return p.Template === "key";
            })[0];
            var key = idProp.Type === "string" ? "('" + retiredObject[idProp.Name] + "')" : "(" + retiredObject[idProp.Name] + ")";
            return await api.destroy(retiredObject.type + key).catch((err) => error(err));
        }
    },

    fieldsFor: function (endpoint) {
        var typeInfo = api.getType(endpoint);
        var result = {};
        typeInfo.Properties.forEach(p => result[p.Name] = { field: p.Name, type: p.Type });
        return result;
    },

    columnsFor: function (endpoint, extraCols, removeCols) {
        var typeInfo = api.getType(endpoint);
        var result = new Array();
        var scalarProperties = typeInfo.Properties.filter(p => p.IsValueType);
        for (var p in scalarProperties) {
            var prop = scalarProperties[p];
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
            for (var i in extraCols)
                result.push(extraCols[i]);
        }

        if (typeof removeCols !== 'undefined') {
            for (var j in removeCols) {
                for (var c in result) {
                    if (result[c].field === removeCols[j] || result[c].name === removeCols[j]) {
                        result.splice(c, 1);
                    }
                }
            }
        }

        return result;
    },

    propertiesFor: function (endpoint, fields, extraFields) {
        var typeInfo = api.getType(endpoint);
        var scalarProperties = typeInfo.Properties.filter(p => p.IsValueType);
        return scalarProperties.filter(prop => (fields) ? (fields.indexOf(prop.Name) !== -1) : true).map((prop) => {
            var p = {
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