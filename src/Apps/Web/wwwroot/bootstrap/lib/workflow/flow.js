const activityWidth = 200;
const handleHeight = 25;
const nodeSize = 16;

class Flow {
    constructor(flow) {
        if (!flow.DefinitionJson) {
            flow.DefinitionJson = JSON.stringify({
                RequiredRoles: null,
                Activities: [{
                    "$type": "cCoder.Core.Objects.Workflow.Activities.Start, cCoder.Core.Objects",
                    "AuthToken": null,
                    "Data": null,
                    "Ref": "Start",
                    "State": 0
                }],
                Links: [],
                Name: flow.Name
            });
        }

        this.model = flow;
        this.Config = flow.ConfigJson ? JSON.parse(flow.ConfigJson) : {};
        this.Definition = JSON.parse(flow.DefinitionJson);
        this.stepTypes = window.knownTypes.filter(ctx => ctx.Name === "Workflow")[0];
        this.Activities = this.Definition.Activities.map(a => {
            try {
                var activityType = a['$type'].split('[');
                var meta = this.stepTypes.Types.filter(f => f.ServerType.indexOf(activityType[0]) === 0);
                if (meta.length === 0) {
                    var activityNamespaces = activityType[0].split(",")[0].split("`")[0].split(".");
                    var activityName = activityNamespaces[activityNamespaces.length - 1];
                    var matchedMeta = this.stepTypes.Types.filter(r => {
                        let namespaces = r.ServerType.split(",")[0].split("`")[0].split(".");
                        let workflowActivityName = namespaces[namespaces.length - 1].toLowerCase();
                        return workflowActivityName === activityName.toLowerCase();
                    })[0];
                    meta = matchedMeta;
                    var hasTypeArguments = a['$type'].indexOf("[") !== -1;

                    a['$type'] = (hasTypeArguments) ? matchedMeta.ServerType + "[" + activityType[activityType.length - 1] : matchedMeta.ServerType;
                } else {
                    meta = meta[0];
                }
                return new Activity(meta, this, a);
            }
            catch (ex) {
                notification.error("An activity could not be constructed, removed.");
                error(ex);
                return null;
            }
        }).filter(a => a);
        this.Links = this.Definition.Links.map(l => new Link(l, this));
        return this;
    }

    addActivity(meta, pos) {
        var nameParts = meta.Name.split("`");
        var typesRequired = nameParts.length > 1 ? parseInt(nameParts[1]) : 0;
        var that = this;

        var next = function (m, type) {
            if (m && type) {
                let serverType = m.ServerType;
                let start = serverType.indexOf('[');
                let end = serverType.lastIndexOf(']');

                serverType = `${serverType.substring(0, start)}[${type}]${serverType.substring(end + 1)}`;
                m.ServerType = serverType;
            }

            var activity = new Activity(m, that, { "$type": m.ServerType, Ref: Guid() });
            activity.setPosition(pos.x, pos.y);
            that.Activities.push(activity);
        };

        if (typesRequired > 0) {
            this.getActivityTypes(meta, typesRequired, next);
        } else {
            next(meta);
        }
    }

    getActivityTypes(type, count, callback) {
        var dialog = $('<div></div>').appendTo(document.body);
        var d = dialog.kendoWindow({
            title: 'Pick Activity Type',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: true,
            width: '50%'
        }).data("kendoWindow");

        d.content(
            `<ul class="fieldList">
                <li><label>Types</label><div name="typeLists"></div></li>
                <li><label></label><div><button class="btn" name="submit">Submit</button></div></li>
            </ul>`
        );

        var ds = {
            data: [].concat.apply([], knownTypes
                .filter(t => t.Name !== "Workflow")
                .map(t => t.Types)
            ),
            group: { field: "Category" }
        };


        for (var i = 0; i < count; i++) {
            $(`<input class="type-isArray" name="isArray${i}" type="checkbox">Array<input class="type-picker" name="type${i}" >`)
                .appendTo($('[name=typeLists]', d.element));

            $(`input[name=type${i}]`, d.element)
                .kendoComboBox({
                    dataTextField: "ServerTypeName",
                    dataValueField: "ServerType",
                    height: 400,
                    dataSource: ds
                });
        }

        $('[name=submit]', d.element).on('click', {
            root: $('[name=typeLists]', d.element),
            dialog: d,
            count: count,
            callback: function(typestring) {
                d.close();
                callback(type, typestring);
            }
        }, this.confirmTypes.bind(this));

        $('.k-combobox', d.element).css({ 'width': 'auto', 'display': 'block', 'margin': '4px 0' });
        d.wrapper.css({ top: 140, left: '20%' });
        d.open();
    }

    confirmTypes(e) {
        var types = [];
        var index = 0;

        function asArrayType(meta, asArray) {
            if (asArray) {
                meta.Name = meta.DisplayName + " Array";
                meta.Description = meta.Description + " Array";
                meta.ServerType = meta.ServerType.replace(",", "[],");
            }
            return meta;
        }

        function next(type) {
            types.push('[' + type.ServerType + ']');

            if (types.length === e.data.count)
                if (e.data.callback)
                    e.data.callback(types.join(','));

            if (index < e.data.count)
                getType();
        }

        function getType() {
            var type = $('[name=type' + index + ']').data('kendoComboBox').dataItem();
            var asArray = $('[name=isArray' + index + ']:checked').length;

            if (type) {
                index++;
                next(asArrayType(type, asArray));
            }
        }

        getType();
    }

    value() {
        this.Definition.Activities = this.Activities.map(a => a.model);
        this.Definition.Links = this.Links.map(l => l.model);
        this.model.DefinitionJson = JSON.stringify(this.Definition);
        return this.model;
    }

    save() {
        var flow = this.value();

        flow.DefinitionJson = JSON.parse(flow.DefinitionJson);
        flow.DefinitionJson.Name = "";
        flow.DefinitionJson = JSON.stringify(flow.DefinitionJson);

        api.update('Core/FlowDefinition(' + flow.Id + ')', flow)
            .then(() => notification.success('Flow Saved Succesfully'))
            .catch(error);
    }

    run(save=true) {
        var that = this;
        if (save) {
            that.save();
        }

        $(document.body).append($('script[name=flowExecution]').html());
        var dialog = $(".execution");
        var d = dialog.kendoWindow({
            title: 'Flow Execution',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: true,
            width: '50%'
        }).data("kendoWindow");

        loadComponent($(".executionConsole", dialog), "FlowRunner", function(c) {
            var autoClose = true;
            var load = true;
            FlowRunner.init(
                session.app,
                $(".executionConsole", dialog),
                that.value(),
                () => {
                    autoClose = $("input[name=autoClose]:checked", dialog).length;
                    load = $("input[name=loadInDesigner]:checked", dialog).length;
                    d.close();
                },
                function(exId, exD) { that.runComplete(exId, exD, autoClose, load); }
            );
        });

        d.wrapper.css({ top: 140, left: '20%' });
        d.open();
    }

    runComplete(id, dialog, autoClose, load) {
        var d = dialog.data("kendoWindow");
        if (load) { this.loadInstance(id); }
        if (autoClose) { d.close(); }
    }

    async loadInstance(id) {
        var that = this;
        var instance = await api.get("Core/FlowInstanceData(" + id + ")");
        var ctx = JSON.parse(instance.ContextString);
        that.Activities.map(function (a) {
            a.addInstanceData(
                ctx.Flow.Activities.filter(ia => a.model.Ref === ia.Ref)[0],
                ctx.ExecutionLog.filter(l => l.Message.startsWith(a.model.Ref + "::"))
            );
        });
    }

    draw(ctx) {
        this.Activities.map(a => a.draw(ctx));
        this.Links.map(l => l.draw(ctx));
    }
}