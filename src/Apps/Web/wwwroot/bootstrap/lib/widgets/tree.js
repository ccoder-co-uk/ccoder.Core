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
        this.treeElement = $(element).append("<div name='" + this.treeName + "Tree'></div>");
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
            let nodeModel = {
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

        let node = this.dataItem(e.node);
        let nodeElement = $(e.node);

        $(nodeElement).children('.k-treeview-group').remove();
        $(".k-i-collapse", nodeElement).removeClass("k-i-collapse").addClass("k-i-expand");

        node.hasChildren = true;
        node.expanded = false;
        node.loaded(false);
    }

    collapseNode(element) {
        let node = this.dataItem(element);
        let nodeElement = $(element);

        $(nodeElement).children('.k-treeview-group').remove();
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

        this.kendoObject.expand(".k-treeview-item:first");
    }

}