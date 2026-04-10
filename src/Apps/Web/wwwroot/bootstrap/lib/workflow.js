class Close extends Collidable {
    constructor(parent) {
        super('x', 0, 0, parent.h - 2, parent.h - 2, 0);
        this.type = 'close';
        this.parent = parent;
        this.flow = this.parent.flow;
        this.text = 'x';
        this.col = window.flowTheme.colours.secondary;
        this.sel = '#f00';
        this.updatePosition();
    }

    updatePosition() {
        this.x = this.parent.x + (this.parent.w - this.parent.h) + 1;
        this.y = this.parent.y + 1;
    }

    draw(ctx) {
        draw.rect(ctx, this.x, this.y, this.w, this.h, this.col, this.sel);
        draw.text(ctx, this.x + 10, this.y + 15, this.text, '#fff');
    }

    mouseup(e) {
        this.flow.Activities = this.flow.Activities.filter(a => !Object.is(a, this.parent.activity));
        this.flow.Links = this.flow.Links.filter(l => l.model.Source !== this.parent.activity.model.Ref);
        this.flow.Links = this.flow.Links.filter(l => l.model.Destination !== this.parent.activity.model.Ref);
    }
}
class Handle extends Collidable {
    constructor(parent, col, textCol) {
        super(parent.model.Ref, parent.x, parent.y, parent.w - 1, handleHeight, 0);

        if (this.text.length > 25) { this.text = this.text.substring(0, 25) + "..."; }

        this.type = 'handle';
        this.col = col;
        this.textCol = textCol || $('h2').css('color');
        this.parent = parent;
        this.activity = parent;
        this.flow = this.parent.flow;
        this.objects = [new Close(this)]; //, new EditProps(this) ];
    }

    draw(ctx) {
        draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#CCC", 1);
        draw.rect(ctx, this.x, this.y, this.w, this.h, this.col);
        draw.disableShadows(ctx);

        draw.text(ctx, this.x + 8, this.y + 17, this.text, this.textCol);
        this.objects.forEach(o => o.draw(ctx));
    }

    mousedown(e) {
        this.moving = true;
        this.start = mouseposition;
    }

    mouseup(e) {
        this.moving = false;
    }

    updatePosition() {
        this.x = this.parent.x;
        this.y = this.parent.y;
        this.objects.forEach(o => o.updatePosition());
    }

    move(e) {
        if (this.moving) {
            this.parent.setPosition(
                this.x + (mouseposition.x - this.start.x),
                this.y + (mouseposition.y - this.start.y)
            );
            this.start = mouseposition;
        }
    }
}
class Link extends Collidable {
    constructor(model, flow) {
        super('', 0, 0, 0, 0, 5);
        this.model = model;
        this.flow = flow;
    }

    draw(ctx) {
        let source = this.flow.Activities.filter(a => a.model.Ref === this.model.Source)[0];
        let destination = this.flow.Activities.filter(a => a.model.Ref === this.model.Destination)[0];

        if (source && destination) {
            if (source.out && destination.in) {
                this.x = source.out.x + ((destination.in.x - source.out.x) / 2);
                this.y = source.out.y + ((destination.in.y - source.out.y) / 2);

                draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#111");
                draw.line(ctx, source.out.x, source.out.y, destination.in.x, destination.in.y, '#37e83a');
                draw.circle(ctx, this.x, this.y, this.r, '#37e83a', '#00ff04');
                draw.disableShadows(ctx);
            }
        }
    }

    mouseup(e) {
        let that = this;

        let dialog = $('<div><div name="content"></div></div>')
            .appendTo(document.body);

        let d = dialog.kendoWindow({
            title: 'Expression Editor',
            modal: true,
            visible: false,
            resizable: false,
            width: '1000px',
            close: function() { this.destroy(); }
        }).data("kendoWindow");

        d.wrapper.css({ top: 140, left: '20%' });
        d.open();

        let container = $("[name=content]", $(d.element));

        loadComponent(container, "ExpressionBuilder", function(c) {
            let builderParams = {
                expression: that.model.Expression,
                args: {
                    destination: that.flow.Activities
                        .filter(a => a.model.Ref === that.model.Destination)[0]
                        .meta.ServerTypeName,
                    source: that.flow.Activities
                        .filter(a => a.model.Ref === that.model.Source)[0]
                        .meta.ServerTypeName,
                    variables: "IDictionary<String,Object>",
                    flow: "Flow"
                }
            };

            ExpressionBuilder.init(null, container, builderParams, function(expression) {
                that.model.Expression = expression;
                d.close();
            });
        });
    }
}
class Action extends Collidable {
    constructor(parent, text, offsetX, offsetY, height, width, col, onClick) {
        super(text, parent.x + offsetX, parent.y + offsetY, height, width, 0);
        this.type = 'Action';
        this.parent = parent;
        this.flow = this.parent.flow;
        this.text = text;
        this.col = col;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.updatePosition();
        this.click = onClick;
    }

    updatePosition() {
        this.x = this.parent.x + this.offsetX;
        this.y = this.parent.y + this.offsetY;
    }

    mouseup(e) {
        this.click(e);
    }

    draw(ctx) {
        draw.text(ctx, this.x + 10, this.y + 15, this.text, this.col);
    }
}
class Activity extends Collidable {
    constructor(meta, flow, model) {
        super(meta.DisplayName.replace("Activity", ""), 0, 0, activityWidth, 100, 0);

        this.text = this.text.replace(/([A-Z])/g, " $1").replace("D M S", "DMS");

        if (this.text.length > 20) {
            this.text = this.text.substring(0, 18) + "...";
        }

        if (meta && flow) {
            this.flow = flow;
            this.meta = meta;
            this.model = model;
            this.handle = new Handle(this, window.flowTheme.colours.primary, '#fff');
            meta.Properties.map(p => this.model[p.Name] = this.model[p.Name] || null);
            this.objects = [];
            this.objects.push(this.handle);

            if (this.meta.name !== "Start") {
                this.in = new Connector(this, 'input');
                this.objects.push(this.in);
            }

            this.out = new Connector(this, 'output');
            this.objects.push(this.out);
            this.objects.push(new Action(this, 'Edit', 50, 65, 35, 25, window.flowTheme.colours.primary, this.edit));
            return this;
        }
    }

    addInstanceData(state, log) {
        this.InstanceState = state;
        this.InstanceLog = log;
        this.objects.push(new Action(this, 'State', 80, 65, 35, 25, window.flowTheme.colours.secondary, this.showExecutionState));
        this.objects.push(new Action(this, 'Log', 110, 65, 35, 25, window.flowTheme.colours.secondary, this.showExecutionLog));
    }

    removeInstanceData() {
        delete this.InstanceState;
        delete this.InstanceLog;
        this.objects = this.objects.filter(o => o.type !== 'State' && o.type !== 'Log');
    }

    setPosition(x, y) {
        this.x = x;
        this.y = y;
        this.objects.forEach(o => o.updatePosition());
    }

    // overide for drawing activities
    draw(ctx) {
        draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#555");

        if (!this.InstanceState) {
            draw.rect(ctx, this.x, this.y + handleHeight, this.w, this.h - handleHeight + 1, '#fff');
        }
        else {
            switch (this.InstanceState.State) {
                case 0:
                case 1:
                case 4:
                    draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#ccc");
                    draw.rect(ctx, this.x, this.y + handleHeight, this.w, this.h - handleHeight + 1, "#eee");
                    break;
                case 2:
                    if (this.InstanceLog.filter(l => l.Level === "Warning").length === 0) {
                        draw.rect(ctx, this.x, this.y + handleHeight, this.w, this.h - handleHeight + 1, "#ded");
                    } else {
                        draw.rect(ctx, this.x, this.y + handleHeight, this.w, this.h - handleHeight + 1, window.flowTheme.colours.primary);
                    }
                    break;
                case 3:
                    draw.rect(ctx, this.x, this.y + handleHeight, this.w, this.h - handleHeight + 1, "#edd");
                    break;
            }
        }

        draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#111");
        ctx.font = "30px WebComponentsIcons";
        ctx.fillStyle = window.flowTheme.colours.primary;
        ctx.fillText(this.getIcon(this.meta.category), this.x + 20, this.y + 73);

        draw.disableShadows(ctx);

        this.objects.forEach(o => o.draw(ctx));
        draw.text(ctx, this.x + 60, this.y + 62, this.text);
    }

    getIcon(forName) {
        switch (forName) {
            case "ApiActivity":
                return "\ue672";
            case "DMSActivity":
                return "\ue900";
            case "SftpActivity":
                return "\ue904";
            case "TransactionActivity":
                return "\ue694";
            case "LogActivity":
                return "\ue64a";
            case "TransformationActivity":
                return "\ue518"
            case "FlowControlActivity":
                return "\ue144";
            case "TemplatingActivity":
                return "\ue647";
            default:
                return "\ue13a";
        }
    }

    activeConnector() {
        if (this.in && this.in.active) { return this.in; }
        if (this.out && this.out.active) { return this.out; }
        return null;
    }

    edit(e) {
        var that = this.parent;
        var dialog = $('<div><div name="content"></div></div>').appendTo(document.body);
        var d = dialog.kendoWindow({
            title: 'Edit Acitvity',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: false,
            width: '820px'
        }).data("kendoWindow");
        d.wrapper.css({ top: 140, left: '20%' });
        d.open();

        loadComponent($("[name=content]", dialog), "ActivityEditor", function(c) {
            ActivityEditor.init(null, dialog, that, function(ac) { that.handle.text = ac.model.Ref; d.close(); });
        });
    }

    showExecutionState(e) {
        var dialog = $('<div class="stateDialog"><pre class="state" style="padding: 10px; width: 625px; height: 500px; overflow:auto;">' + JSON.stringify(this.parent.InstanceState, null, "\t") + '</pre></div>').appendTo(document.body);
        var d = dialog.kendoWindow({
            title: `Activity ${this.parent.model.Ref} :: Execution State`,
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: false,
            width: '650px'
        }).data("kendoWindow");
        d.wrapper.css({ top: 140, left: '20%' });
        d.open();
    }

    showExecutionLog(e) {
        var dialog = $($("script[name=executionLog]").html()).appendTo(document.body);
        var d = dialog.kendoWindow({
            title: 'Activity ' + this.parent.model.Ref + ' :: Execution Log',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: false,
            width: '810px'
        }).data("kendoWindow");
        d.wrapper.css({ top: 140, left: '20%' });
        d.open();

        var messages = this.parent.InstanceLog.map(function(msg) {
            var msgStamp = new Date(msg.Timestamp);
            var time = `${msgStamp.getHours()}:${msgStamp.getMinutes()}:${msgStamp.getSeconds()}`;
            return $(`<div class='message ${msg.Level}'><span class='time'>${time}</span><pre class='message'>${html.encode(msg.Message)}</pre></div>`);
        });

        $(".flowConsole", dialog).append(messages);
    }
}
/// <reference path="workflowdesigner.js" />
class Connector extends Collidable {
    constructor(parent, type) {
        super('', 0, 0, 0, 0, nodeSize);
        this.parent = parent;
        this.type = type;
        this.col = window.flowTheme.colours.primary;
        this.sel = window.flowTheme.colours.secondary;
        this.updatePosition();
    }

    mousedown() {
        this.active = true;

        window.addEventListener('keydown', this.listenForEsc);
    }

    removeLink(otherConnector) {
        let from = this.type === 'output' ? this : otherConnector;
        let to = this.type === 'output' ? otherConnector : this;

        if (otherConnector) {
            var fromRef = from.parent.model.Ref;
            var toRef = to.parent.model.Ref;

            var removal = this.parent.flow.Links
                .filter(l => l.model.Source === fromRef && l.model.Destination === toRef)[0];

            this.parent.flow.Links = this.parent.flow.Links.filter(i => i !== removal);
        } else if(this.type == 'input') {
            this.parent.flow.Links = this.parent.flow.Links
                .filter(l => l.model.Destination !== to.parent.model.Ref);
        }
    }

    addLink(otherConnector) {
        if (otherConnector && this.type !== otherConnector) {
            let from = this.type === 'output' ? this : otherConnector;
            let to = this.type === 'output' ? otherConnector : this;

            if (from && from.type === 'output') {
                let newLink = {
                    "Source": from.parent.model.Ref,
                    "Destination": to.parent.model.Ref,
                    "Expression": ""
                };

                this.parent.flow.Links.push(new Link(newLink, this.parent.flow));
            }
        }
    }

    mouseup(e, editor, otherConnector) {
        var node1 = this.parent.model.Ref;
        var node2 = otherConnector ? otherConnector.parent.model.Ref : null;

        if (node2 == null) {
            this.parent.flow.Links = this.type == 'output'
                ? this.parent.flow.Links.filter(l => l.model.Source != node1)
                : this.parent.flow.Links.filter(l => l.model.Destination != node1);
        } else {
            var existingLinks = this.parent.flow.Links
                .filter(l => {
                    return (l.model.Source === node1 && ((l.model.Destination === node2) || !node2)) ||
                        (((l.model.Source === node2) || !node2) && l.model.Destination === node1)
                });

            if (existingLinks.length > 0)
                this.removeLink(otherConnector);
            else
                this.addLink(otherConnector);
        }

        window.removeEventListener('keydown', this.listenForEsc);
        this.active = false;
    }

    updatePosition() {
        if (this.type === "input")
            this.x = this.parent.x + 2;

        if (this.type === "output")
            this.x = this.parent.x + this.parent.w - 2;

        this.y = this.parent.y + (this.parent.h / 2) + (handleHeight / 2);
    }

    draw(ctx) {
        if (this.type == "output")
            draw.semiCircle(ctx, this.x, this.y, this.r, Math.PI * 0.5, Math.PI * 1.5, this.sel, this.col);
        else
            draw.semiCircle(ctx, this.x, this.y, this.r, Math.PI * 1.5, Math.PI * 2.5, this.sel, this.col);

        if (this.active)
            draw.line(ctx, this.x, this.y, mouseposition.x, mouseposition.y, window.flowTheme.colours.secondary);
    }

    listenForEsc(e) {
        if (e.keyCode == 27) { // 27 = esc key.
            this.active = false;
            window.removeEventListener('keydown', this.listenForEsc);
        }
    }
}
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
class WorkflowDesigner {
    constructor(container, flow) {

        //TODO: handle this not being available for some reason
        window.flowTheme = session.app.Config.Themes[session.theme];

        this.stepTypes = window.knownTypes.filter(ctx => ctx.Name === "Workflow")[0].Types;
        this.workspace = $(".workspace", container);
        this.canvas = $("canvas", this.workspace);
        this.flow = new Flow(flow);
        this.autoArrange();
        this.init(container);
    }

    init(container) {
        $("[name=splitter]", container).kendoSplitter({
            orientation: "horizontal",
            panes: [
                { collapsable: false, size: '250px' },
                { collapsable: false, scrollable: false }
            ]
        });

        this.initMenu(container);
        this.canvas.off('mousedown').on('mousedown', this.mousedown.bind(this));
        this.canvas.off('mouseup').on('mouseup', this.mouseup.bind(this));
        this.canvas.off('mousemove').on('mousemove', this.mousemove.bind(this));

        let canvas = this.canvas[0];

        window.addEventListener('mousemove', function(e) {
            let bounds = canvas.getBoundingClientRect();

            window.mouseposition = {
                x: parseInt(e.clientX - bounds.left),
                y: parseInt(e.clientY - bounds.top)
            };
        });

        window.addEventListener('keydown', function(e) {
            if (e.ctrlKey && (e.keyCode || e.which) === 83) {
                e.preventDefault();
                editor.flow.save();
            }
        });

        window.mouseposition = { x: 0, y: 0 };

        this.canvas.height = this.workspace.height();
        this.canvas.width = this.workspace.width();

        $(window).on("resize", this.resize.bind(this));
        this.resize();
        this.draw();
    }

    getCollision() {
        let collision = this.flow.Activities.map(a => a.collides()).filter(r => r)[0];
        collision = collision || this.flow.Links.map(l => l.collides()).filter(r => r)[0];
        return collision;
    }

    mousedown(e) {
        if (e.target.localName.toLowerCase() === 'canvas') {
            this.active = this.getCollision();
            this.dragging = this.active ? false : true;
            this.dragstart = mouseposition;

            if (!this.dragging && this.active && this.active.mousedown) {
                this.active.mousedown(e, this);
            }

            window.addEventListener('keydown', this.listenForEsc);
        }
    }

    mouseup(e) {
        if (e.target.localName.toLowerCase() === 'canvas') {
            var collision = this.getCollision();
            this.dragging = false;

            if (this.active && this.active.mouseup && !Object.is(this.active, collision)) {
                this.active.mouseup(e, this, collision);
            }

            if (this.active && this.active.mouseup && Object.is(this.active, collision)) {
                this.active.mouseup(e, this);
            }

            this.active = null;
            window.removeEventListener('keydown', this.listenForEsc);
        }
    }

    mousemove(e) {
        if (e.target.localName.toLowerCase() === 'canvas') {
            if (!this.dragging && this.active && this.active.move) {
                this.active.move(e, this);
            }
            else if (this.dragging) {
                let x = mouseposition.x - this.dragstart.x;
                let y = mouseposition.y - this.dragstart.y;
                this.flow.Activities.map(a => a.setPosition(a.x + x, a.y + y));
                this.dragstart = mouseposition;
            }
        }
    }

    initMenu(container) {
        let menu = $(".flowmenu", container);

        $('[name=settings]', menu).off('click')
            .on('click', this.settings.bind(this));

        $('[name=run]', menu).off('click')
            .on('click', this.flow.run.bind(this.flow));

        $('[name=save]', menu).off('click')
            .on('click', this.flow.save.bind(this.flow));

        this.buildMenu(menu);
    }

    categoryIconMap(category) {
        switch (category) {
            case "Api":
                return "k-i-hyperlink-globe";
            case "DMS":
                return "k-i-folder";
            case "Sftp":
                return "k-i-folder-more";
            case "Transaction":
                return "k-i-dollar";
            case "Log":
                return "k-i-track-changes-enable";
            case "Transformation":
                return "k-i-shape";
            case "FlowControl":
                return "k-i-connector";
            case "Templating":
                return "k-i-template-manager";
            default:
                return "k-i-gear";
        }
    }

    buildMenu(container) {
        let groups = Array.from(new Set(this.stepTypes.map((t) =>  t.Category )));
        groups.map(g => container.append(this.buildMenuCategory.bind(this)({ Name: g, Types: this.stepTypes.filter((st) => st.Category === g) })));
    }

    buildMenuCategory(type) {
        let that = this;

        if (type.Types.length > 0) {
            let categoryName = type.Name.split("`")[0];

            categoryName = categoryName === 'Activity'
                ? 'Activity'
                : categoryName.replace('Activity', '');

            let category = $('<ul class="categorytypes" data-category="' + type.Name + '"></ul>');
            category.append('<li class="header"><span class="k-icon ' + this.categoryIconMap(categoryName) + '"></span>' + categoryName + '</li>');
            let sublist = $('<ul class="subtypes"></ul>');
            category.append(sublist);

            type.Types.map(t => {
                let typeName = t.DisplayName.split("`")[0];

                typeName = typeName === 'Activity'
                    ? 'Activity'
                    : typeName.replace('Activity', '');

                let newType = $('<li><span name="add" class="k-icon k-i-add"></span>' + typeName + '</li>');
                newType.data('model', t);
                newType.css('cursor', 'pointer');
                let helper = $('<div class="flow-helper"></div>');
                helper.css({ 'padding': '8px 16px', 'border': '1px solid #000' });

                newType.draggable({
                    helper: function(e) {
                        helper.html($(e.target).html());
                        return helper.appendTo(document.body);
                    },
                    stop: function(e) { that.menuItemClicked.bind(that)(e, mouseposition); }
                });

                sublist.append(newType);
            });

            sublist.css({ 'max-height': 0, transition: 'all 0.3s', overflow: 'hidden' });

            $('li', category).css('padding', '8px');
            $('ul', category).css('padding', '0 0 0 8px');

            $('.header', category).on('click', function () {
                let h = sublist.css('max-height');
                sublist.css({ 'max-height': h !== '500px' ? '500px' : 0 });
            });

            return category;
        }
        else { return null; }
    }

    menuItemClicked(e, position) {
        let activity = $(e.target).data('model');
        if (activity) { this.flow.addActivity(activity, position); }
    }

    resize() {
        this.canvas[0].height = this.workspace.height();
        this.canvas[0].width = this.workspace.width();
    }

    settings() {
        let that = this;
        let dialog = $('<div></div>').appendTo(document.body);

        let d = dialog.kendoWindow({
            title: 'Settings',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: true,
            width: '50%'
        }).data("kendoWindow");

        d.content('<div name="content"></div>');

        let container = $("[name=content]", d.element);

        loadComponent(container, "FlowSettings", function(c) {
            FlowSettings.init(session.app, container, that.flow, function () {
                d.close();
            });
        });

        d.wrapper.css({
            top: 140,
            left: '20%'
        });

        d.open();
    }

    draw() {
        let ctx = this.canvas[0].getContext('2d');
        ctx.clearRect(0, 0, this.workspace.width(), this.workspace.height());
        this.drawGrid(ctx, this.workspace.width(), this.workspace.height());
        this.flow.draw(ctx);
        setTimeout(this.draw.bind(this), 1000 / 60);
    }

    drawGrid(ctx, width, height) {
        let yLines = Math.ceil(width / 10);
        let xLines = Math.ceil(height / 10);

        for (let y = 0; y < yLines; y++) {
            let start = { x: (10 * y), y: 0 };
            let end = { x: (10 * y), y: height };
            draw.line(ctx, start.x, start.y, end.x, end.y, y % 5 === 0 ? '#99b' : '#dde', 0.5);
        }

        for (let x = 0; x < xLines; x++) {
            let start = { x: 0, y: (x * 10) };
            let end = { x: width, y: (x * 10) };
            draw.line(ctx, start.x, start.y, end.x, end.y, x % 5 === 0 ? '#99b' : '#dde', 0.5);
        }
    }

    autoArrange() {
        let that = this;

        function getActivitiesAt(x, y) {
            return that.flow.Activities.filter(a => a.x === x && a.y === y);
        }

        function placeActivity(prev, a, i) {
            let x = (a.x == 0) ? prev.x + a.w + 100 : a.x;
            let y = prev.y + (i * 1.5);

            a.setPosition(x, y);
            getChildActivities(a).map(function (b, j) {
                placeActivity(a, b, j * a.h);
            });
        }

        function getChildActivities(a) {
            let links = that.flow.Links.filter(l => l.model.Source === a.model.Ref);
            return that.flow.Activities.filter(b => links.filter(l => l.model.Destination === b.model.Ref).length > 0);
        }

        // find the first set of activities
        that.flow.Activities
            .filter(a => that.flow.Links.filter(l => l.model.Destination === a.model.Ref).length === 0)
            .map(function (c, i) { placeActivity({ x: -200, y: 100 }, c, i); });

        for (let i in that.flow.Activities) {
            let a = that.flow.Activities[i];
            let clash = getActivitiesAt(a.x, a.y);

            if (clash.length > 1) {
                clash[1].setPosition(clash[1].x, clash[1].y += 150);
            }
        }
    }

    listenForEsc(e) {
        if (e.keyCode == 27) { // 27 = esc key.
            this.dragging = false;
            this.active = null;
            window.removeEventListener('keydown', this.listenForEsc);
        }
    }
}