class DetailWidget extends Widget {
    // Consumes https://demos.telerik.com/kendo-ui/templates/expressions to build a read only detail view of an object
    // or portion of an object
    constructor(element, args) {
        super(element);
        args = args || {};
        // default configuation for all grid widgets.
        this.detailName = $(element).attr("name");
        this.detailElement = element;
        this.fields = args.fields;
        this.labelTooltip = true;
        this.title = args.title || "Details";

        if (args.hasOwnProperty("header")) {//false || true -> true. Hence hasOwnProperty is required.
            this.header = args.header;
        } else {
            this.header = true;
        }

        this.editable = false;
        this.splits = args.splits;

        if (args.data) {
            this.data = new kendo.observable(args.data);
        }

        this.config = { endpoint: $(this.detailElement).data("endpoint"), odataAppend: "(" + getQueryParameter("Id") + ")" + $(element).data("odataappend") };

        // attach this object to the widget element and add the grid element to prepare for an init call.
        $(this.detailElement).data("widget", this);
    }

    async init() {
        if (!this.template) { this.buildTemplate(); }

        if (this.data) {
            await this.render();
        }
        else {
            let d = await api.get(this.config.endpoint + this.config.odataAppend);
            this.data = new kendo.observable(d);
            this.parseData(this.fields);
            await this.render();
        }
    }

    parseData(meta) {
        $.each(meta, (i, p) => {
            if (p.type === 'date') {
                this.data[p.field] = this.data[p.field] !== null
                    ? new Date(this.data[p.field])
                    : this.data[p.field];
            }
        });
    }

    async render() {
        $(this.detailElement).append(kendo.template(this.template)(this.data));
    }

    computeFields(exclude, callback) {
        let that = this;
        type.fieldsFor(this.config.endpoint, (fields) => {
            that.fields = fields.filter(f => exclude.IndexOf(f.field) < 0);
            if (callback) { callback(); }
        });
    }

    buildTemplate() {

        let build = (that) => {
            let fieldSet = "";

            that.fields.map((meta) => {
                if (that.splits && that.splits.indexOf(meta.field) > - 1) { fieldSet += "</ul><ul class='fieldList'>"; }
                fieldSet += "<li name='" + that.config.endpoint + "/" + meta.field + "'><label title='" + meta.description + "' for='" + meta.field + "'>" + meta.title + "</label><div class='value'>" + that.fieldValueExpression(meta.field) + "</div></li>";
            });

            that.template = "";

            if (that.header) {
                that.template = "<h3>" + that.title + "</h3>";
            }

            that.template += (that.toolbar
                    ? "<div class='k-header k-grid-toolbar'>" + that.toolbar + "</div><div name='details'><ul class='fieldList'>" + fieldSet + "</ul></div>"
                    : "<div name='details'><ul class='fieldList'>" + fieldSet + "</ul></div>");
        };

        if (!this.fields) {
            this.computeFields(null, () => { build(this); });
        }
        else { build(this); }
    }

    fieldValueExpression(fieldName) { /* Intentional */ }
}