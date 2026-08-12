;(function () {
    const compileTemplate = kendo.template;

    kendo.template = function (template, options) {
        if (typeof template === "function") {
            return template;
        }

        if (typeof template === "string" && !template.includes("#")) {
            return function () {
                return template;
            };
        }

        return compileTemplate.call(kendo, template, options);
    };
})();