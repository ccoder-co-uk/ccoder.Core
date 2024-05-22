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
        var apiUrl = this.endpoint;

        if (this.odataAppend) {
            apiUrl += this.odataAppend;
            apiUrl += "&$filter=" + this.parentIdField + " eq " + parentIdValue;
        } else {
            apiUrl += "&$filter=" + this.parentIdField + " eq " + parentIdValue;
        }

        var url = model.odata.joinFilters(apiUrl);

        console.log(api.apiRoot + url);

        return url;
    }

    async drop(e) {
        var dragged = e.sender.dataItem(e.sourceNode);
        var droppedOver = e.sender.dataItem(e.dropTarget);
        if (dragged === droppedOver) { return; }

        dragged.data[this.parentIdField] = droppedOver.data[this.idField];
        dragged.data.save();
    }

    async expand(e) {
        e.preventDefault();
        this.expandNode(e.node);
    }

    async expandNode(element) {

        var treeItem = this.dataItem(element);

        if (!(!treeItem.expanded && treeItem.hasChildren && !treeItem.loaded())) { return; }

        treeItem.expanded = true;
        treeItem.loaded(true);

        var entity = treeItem.data;

        var data = (await api.get(this.buildParentQuery(entity[this.idField]))).value;

        var nodes = data.map(d => this.prepareData(d));

        for (let i = 0; i < nodes.length; i++) {
            treeItem.items.push(nodes[i]);
        }
    }

    async refresh() {
        var rootNodes = (await api.get(this.buildParentQuery(this.parentIdInitialValue))).value;

        this.kendoObject.setDataSource(new kendo.data.HierarchicalDataSource({
            data: rootNodes.map(data => this.prepareData(data))
        }));
    }

    async init() {
        var rootNodes = (await api.get(this.buildParentQuery(this.parentIdInitialValue))).value;

        this.dataSource = new kendo.data.HierarchicalDataSource({
            data: rootNodes.map(data => this.prepareData(data))
        });

        super.init();
    }

    prepareData(data) {
    
    }
}