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
        this.drawIcon(ctx);

        draw.disableShadows(ctx);

        this.objects.forEach(o => o.draw(ctx));
        draw.text(ctx, this.x + 60, this.y + 62, this.text);
    }

    drawIcon(ctx) {
        const centreX = this.x + 30;
        const centreY = this.y + 62;
        const label = this.getIconLabel(this.meta.category);

        ctx.beginPath();
        ctx.arc(centreX, centreY, 17, 0, 2 * Math.PI);
        ctx.lineWidth = 2;
        ctx.strokeStyle = window.flowTheme.colours.primary;
        ctx.stroke();

        ctx.font = "bold 9px Arial";
        ctx.fillStyle = window.flowTheme.colours.primary;
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.fillText(label, centreX, centreY);
        ctx.textAlign = "start";
        ctx.textBaseline = "alphabetic";
    }

    getIconLabel(forName) {
        switch (forName) {
            case "ApiActivity":
                return "API";
            case "DMSActivity":
                return "DMS";
            case "SftpActivity":
                return "SFTP";
            case "TransactionActivity":
                return "TX";
            case "LogActivity":
                return "LOG";
            case "TransformationActivity":
                return "MAP";
            case "FlowControlActivity":
                return "FLOW";
            case "TemplatingActivity":
                return "TPL";
            default:
                return "STEP";
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
