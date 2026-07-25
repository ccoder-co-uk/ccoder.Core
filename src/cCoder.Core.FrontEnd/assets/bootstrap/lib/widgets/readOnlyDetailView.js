class ReadOnlyDetailView extends DetailWidget {
    fieldValueExpression(fieldName) {
        let meta = this.fields.filter(i => i.field === fieldName)[0];

        if (!meta.type) {
            meta.type = "string";
        }

        if (meta.template) {
            return meta.template;
        }
        else {
            switch (meta.type) {
                case "number":
                case "date":
                    if (!meta.format) {
                        return "#:kendo.toString(" + fieldName + ", '" + type.dateFormat + "') || ''#";
                    } else {
                        return "#:kendo.toString(" + fieldName + ", '" + meta.format + "') || ''#";
                    }
                default:
                    return "#:" + fieldName + " || ''#";
            }
        }
    }
}