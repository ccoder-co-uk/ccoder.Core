class DataTreeViewWidget extends Widget {
    constructor(element, treeViewConfig) {
        super(element, null);

        this.treeElement = element;

        this.animation = {
            collapse: { duration: 400, effects: "fadeOut collapseVertical" },
            expand: { duration: 400, effects: "fadeIn" }
        };

        this.treeName = $(this.treeElement).attr("name");

        this.treeViewConfig = treeViewConfig;

        if (!this.treeViewConfig.animation)
            this.treeViewConfig.animation = this.animation;
    }

    init() {
        this.wireUpEvents();

        this.kendoObject = this.treeElement
            .kendoTreeView(this.treeViewConfig)
            .data("kendoTreeView");

        $(this.treeElement).data("tree", this);
    }

    wireUpEvents() {
        if(!this.treeViewConfig.select)
            this.treeViewConfig.select = this.onSelect;

        if (!this.treeViewConfig.drop)
            this.treeViewConfig.drop = this.onDrop;

        if (!this.treeViewConfig.check)
            this.treeViewConfig.check = this.onCheck;

        if (!this.treeViewConfig.collapse)
            this.treeViewConfig.collapse = this.onCollapse;

        if (!this.treeViewConfig.expand)
             this.treeViewConfig.expand = this.onExpand;

        this.wireUp();
    }

    getDatasource() {
        return this.treeViewConfig.dataSource;
    }

    refreshData() {
        this.kendoObject.dataSource.refresh();
    }

    async onSelect(e) { }

    async onDrop(e) { }

    async onCheck(e) { }

    async onCollapse(e) { }

    async onExpand(e) { }

    async wireUp() { }
}