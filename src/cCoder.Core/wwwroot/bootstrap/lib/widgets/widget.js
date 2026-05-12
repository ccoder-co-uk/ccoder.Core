class Widget
{
    constructor(element, args) {
        if (!args) {
            args = {};
        }

        let e = element;

        if (!$(element).hasClass("component") || !$(element).attr("name")) {
            e = $(element).closest(".component");
        }

        this.element = args.e || e;
        this.name = args.name || $(e).attr("name");
        this.app = args.app || session.app;
    }

    initilizeChildrenOf(parentElement, args, callback) {
        $("[data-child-component]", parentElement)
            .each(function (i, el) {
                let name = $(el).attr("data-child-component");
                loadComponent(el, name, function (c) { c.init(args.widget.app, el, args); });
            });

        setTimeout(callback, 1000);
    }
}

jQuery.fn.getPath = function () {
    let path, node = this;

    while (node.length) {
        let realNode = node[0], name = realNode.localName;

        if (!name)
            break;

        name = name.toLowerCase();

        let parent = node.parent();
        let siblings = parent.children(name);

        if (siblings.length > 1) {
            name += ':eq(' + siblings.index(realNode) + ')';
        }

        path = name + (path ? '>' + path : '');
        node = parent;
    }

    return path;
};