class ODataTreeOptions {
    setElement(element) {
        this.element = element;
        return this;
    }

    setEndpoint(endpoint) {
        this.endpoint = endpoint;
        return this;
    }

    setODataAppend(odataAppend) {
        this.odataAppend = odataAppend;
        return this;
    }

    setParentIdField(parentIdField) {
        this.parentIdField = parentIdField;
        return this;
    }

    setParentIdInitialValue(parentIdInitialValue) {
        this.parentIdInitialValue = parentIdInitialValue;
        return this;
    }

    setIdField(idField) {
        this.idField = idField;
        return this;
    }
}

class ODataTree extends Tree {
    constructor(args) {
        super(args.element, null);
        this.endpoint = args.endpoint;
        this.odataAppend = args.odataAppend;
        this.parentIdField = args.parentIdField || "ParentId";
        this.parentIdInitialValue = args.parentIdInitialValue || null;
        this.idField = args.idField || "Id";
    }

    buildParentQuery(parentIdValue) {
        let apiUrl = this.endpoint;

        if (this.odataAppend) {
            apiUrl += this.odataAppend;
            apiUrl += "&$filter=" + this.parentIdField + " eq " + parentIdValue;
        } else {
            apiUrl += "&$filter=" + this.parentIdField + " eq " + parentIdValue;
        }

        let url = model.odata.joinFilters(apiUrl);

        return url;
    }

    async drop(e) {
        let dragged = e.sender.dataItem(e.sourceNode);
        let droppedOver = e.sender.dataItem(e.dropTarget);

        if (dragged === droppedOver) {
            return;
        }

        dragged.data[this.parentIdField] = droppedOver.data[this.idField];
        dragged.data.save();
    }

    async expand(e) {
        e.preventDefault();
        this.expandNode(e.node);
    }

    async expandNode(element) {

        let treeItem = this.dataItem(element);

        if (!(!treeItem.expanded && treeItem.hasChildren && !treeItem.loaded())) { return; }

        treeItem.expanded = true;
        treeItem.loaded(true);

        let entity = treeItem.data;

        let data = (await api.get(this.buildParentQuery(entity[this.idField]))).value;

        let nodes = data.map(d => this.prepareData(d));

        for (let i = 0; i < nodes.length; i++) {
            treeItem.items.push(nodes[i]);
        }
    }

    async refresh() {
        let rootNodes = (await api.get(this.buildParentQuery(this.parentIdInitialValue))).value;

        this.kendoObject.setDataSource(new kendo.data.HierarchicalDataSource({
            data: rootNodes.map(data => this.prepareData(data))
        }));
    }

    async init() {
        let rootNodes = (await api.get(this.buildParentQuery(this.parentIdInitialValue))).value;

        this.dataSource = new kendo.data.HierarchicalDataSource({
            data: rootNodes.map(data => this.prepareData(data))
        });

        super.init();
    }

    prepareData(data) {
    
    }
}