class Tree extends Widget {
    constructor(element, dataSource) {
        super(element, null);
        this.dataSource = dataSource;
        this.animation = {
            collapse: { duration: 400, effects: "fadeOut collapseVertical" },
            expand: { duration: 400, effects: "fadeIn" }
        };
        this.dragAndDrop = true;
        this.treeName = $(element).attr("name");
        this.treeElement = $(element).append("<div name='" + this.treeName + "Tree' style='width:100%; height:100%; display: block;'></div>");
    }

    dataItem(element) {
        return this.kendoObject.dataItem(element);
    }

    selectNode(element) {
        return this.kendoObject.select(element);
    }

    removeNode(element) {
        return this.kendoObject.remove(element);
    }

    parentNode(element) {
        return this.kendoObject.parent(element);
    }

    setText(node, text) {
        return this.kendoObject.text(node, text);
    }
    
    addNodes(items, parentNodeData, spriteClass, type) {
        for(var i in items) {
            var nodeModel = {
                spriteCssClass: spriteClass,
                text: i.Name,
                type: type,
                data: i,
                expanded: false,
                hasChildren: true,
                items: []
            };
            parentNodeData.items.push(nodeModel);
        }
    }
    
    clearChildNodes(node) {
        let nodeItem = this.kendoObject.dataItem(node);
        let items = nodeItem.children.data();
        for (let i = 0, max = items.length; i < max; i++) {
            let item = this.kendoObject.findByUid(items[0].uid);
            this.remove(item);
        }
    }

    remove(item) {
        return this.kendoObject.remove(item);
    }
    
    collapse(e) {
        e.preventDefault();

        var node = this.dataItem(e.node);
        var nodeElement = $(e.node);

        $(nodeElement).children('.k-group').remove();
        $(".k-i-collapse", nodeElement).removeClass("k-i-collapse").addClass("k-i-expand");

        node.hasChildren = true;
        node.expanded = false;
        node.loaded(false);
    }

    collapseNode(element) {
        var node = this.dataItem(element);
        var nodeElement = $(element);

        $(nodeElement).children('.k-group').remove();
        $(".k-i-collapse", nodeElement).removeClass("k-i-collapse").addClass("k-i-expand");

        node.hasChildren = true;
        node.expanded = false;
        node.loaded(false);

    }

    init() {
        this.kendoObject = this.treeElement.kendoTreeView({
            dragAndDrop: this.dragAndDrop,
            dataSource: this.dataSource,
            // events
            animation: this.animation,
            select: this.select,
            expand: (e) => this.expand(e),
            collapse: (e) => this.collapse(e),
            dragStart: this.dragStart,
            drop: (e) => this.drop(e)
        }).data("kendoTreeView");

        $(this.treeElement).data("tree", this);

        this.kendoObject.expand(".k-item:first");
    }

}