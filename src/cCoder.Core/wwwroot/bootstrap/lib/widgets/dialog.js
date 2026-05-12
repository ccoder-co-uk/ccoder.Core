class Dialog extends Widget {
    /* Consumes https://demos.telerik.com/kendo-ui/templates/expressions to build a read only detail view of an object or portion of an object */
    constructor(args) {
        super(null, args);

        if (!args) { args = {}; }

        this.args = args;

        if (args.modal === false) {
            this.modal = false;
        } else {
            this.modal = true;
        }

        this.name = args.name || "Dialog";
        this.title = args.title || "Dialog";
        this.height = args.height || "auto";
        this.width = args.width || 600;
        this.template = args.template || "";
        this.component = args.component || false;

        this.events = args.events || {
            close: (e) => { 
                $(".dialog[name=" + this.name + "]").remove();
                this.kendoObject.destroy(); 
            }
        }
    }

    async init(callback) {
        $("body").append("<div class = 'dialog' name='" + this.name + "'>" + this.template + "</div>");
        var dialog = $("[name=" + this.name + "]");

        this.element = dialog.kendoWindow({
            visible: false,
            modal: this.modal,
            resizable: true,
            height: this.height,
            width: this.width,
            title: this.title,
            deactivate: this.events.close
        });

        this.kendoObject = dialog.data("kendoWindow");

        var wireUp = () => {
            this.kendoObject.center();
            this.kendoObject.open();
            for (var i in this.events) {
                $("button[name=" + i + "]", this.element).on("click", this.events[i]);
            }
        }

        if (this.component) {
            loadComponent(dialog, this.component, (c) => {
                wireUp();
                if (callback) { callback(); }
                return this;
            });
        }
        else {
            wireUp();

            if (callback) {
                callback();
            }

            return this;
        }
    }
}

