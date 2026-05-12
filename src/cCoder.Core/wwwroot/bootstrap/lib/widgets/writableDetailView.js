class WritableDetailView extends DetailWidget {
    constructor(element, args) {
        super(element, args);
        this.editable = true;
    }

    async init() {
        await super.init();
        kendo.bind($(this.detailElement), this.data);
    }

    fieldValueExpression(fieldName) {
        var meta = this.fields.filter(i => i.field === fieldName)[0];
        var fieldType = meta.fieldType || "text";

        if (!meta.type) {
            meta.type = "string";
        }

        if (meta.template) {
            return meta.template;
        } else {
            return `<input class="form-control input-sm" type="${fieldType}" data-bind="value: ${fieldName}" name="${fieldName}" />`;
        }
    }
}