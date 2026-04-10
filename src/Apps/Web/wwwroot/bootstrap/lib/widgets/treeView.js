class TreeView extends Widget {
    constructor(element, args) {
        super(element, args);

        this.dragStart = args.dragStart;
        this.collapse = args.collapse;   
    }

    init() {
        this.kendoObject = treeRoot.kendoTreeView({
            dragAndDrop: true,

            dataSource: new kendo.data.HierarchicalDataSource({
                data: [{
                    text: app.Name,
                    spriteCssClass: "root",
                    type: "Root",
                    data: null,
                    expanded: false,
                    hasChildren: true
                }]
            }),

            // events
            select: this.select,
            expand: this.expand,
            collapse: this.collapse,
            dragStart: this.dragStart,
            drop: this.drop
        }).data("kendoTreeView");

        //Provide default implementations of the functions if they are not already there.
        if (!this.dragStart) {
            this.dragStart = (e) => {
                var nodeData = this.kendoObject.dataItem(e.sourceNode);
                if (nodeData.data == null) {
                    e.preventDefault();
                    $(e.element).addClass("k-denied");
                }
            };
        }
        if (!this.collapse) {
            this.collapse = (e) => {
                var nodeData = this.kendoObject.dataItem(e.node);
                $(nodeData).children('.k-group').remove();
                if (nodeData.data != null) {
                    nodeData.loaded(false);
                    e.node.loaded = false;
                }
            }
        }
        this.kendoObject.expand(".k-item:first");
    }

    select(e) {
        // placeholder
    }

    expand(e) {
        // placeholder
    }

    collapse(e) {
        // placeholder
    }

    drop(e) {
        // placeholder
    }
}