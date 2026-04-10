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
