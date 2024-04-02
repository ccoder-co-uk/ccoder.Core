class Widget
{
    constructor(element, args) {
        if (!args) { args = {}; }
        var e = element;
        if (!$(element).hasClass("component") || !$(element).attr("name")) {
            e = $(element).closest(".component");
        }

        this.element = args.e || e;
        this.name = args.name || $(e).attr("name");
        this.app = args.app || session.app;
    }

    initilizeChildrenOf(parentElement, args, callback) {
        $("[data-child-component]", parentElement).each(function (i, el) {
            var name = $(el).attr("data-child-component");
            loadComponent(el, name, function (c) { c.init(args.widget.app, el, args); });
        });

        $("[data-auto-init=true]", parentElement).each(function (i, el) {
            var type = $(el).attr("role");
            eval("new " + type + "($('" + $(el).getPath() + "')[0]).init()");
        });

        setTimeout(callback, 1000);
    }
}

jQuery.fn.getPath = function () {
    var path, node = this;
    while (node.length) {
        var realNode = node[0], name = realNode.localName;
        if (!name) break;
        name = name.toLowerCase();
        var parent = node.parent();
        var siblings = parent.children(name);
        if (siblings.length > 1) {
            name += ':eq(' + siblings.index(realNode) + ')';
        }

        path = name + (path ? '>' + path : '');
        node = parent;
    }

    return path;
};