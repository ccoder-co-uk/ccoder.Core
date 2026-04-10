class EditorDialog extends Dialog {
    constructor(args) {
        super(args);

        args = args || {};
        this.args = args;
        this.data = new kendo.observable(args.data);
        this.confirm = this.args.confirm;
        this.config = args.config;
        this.fields = args.fields;
        this.header = false;

        this.events.confirm = async function (e) {
            e.preventDefault();
            this.data.save();
            this.events.close();
        };
    }

    async init() {
        this.template += "<div name='editor' class='editorDialog'></div><hr><div class='value'><button class='btn btn-sm btn-primary float-end' name='confirm'>" + this.confirm + "</button></div>";
        await super.init();
        this.writableEditor = new WritableDetailView($("[name='editor']", this.element), this);
        await this.writableEditor.init();
        this.data = this.writableEditor.data;
    }
}