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
class Dialog extends Widget {
    /* Consumes https://demos.telerik.com/kendo-ui/templates/expressions to build a read only detail view of an object or portion of an object */
    constructor(args) {
        super(null, args);

        if (!args) { args = {}; }

        this.args = args;

        if (args.modal === false) {
            this.modal = false;
        } else {
            this.modal = true;
        }

        this.name = args.name || "Dialog";
        this.title = args.title || "Dialog";
        this.height = args.height || "auto";
        this.width = args.width || 600;
        this.template = args.template || "";
        this.component = args.component || false;

        this.events = args.events || {
            close: (e) => { 
                $(".dialog[name=" + this.name + "]").remove();
                this.kendoObject.destroy(); 
            }
        }
    }

    async init(callback) {
        $("body").append("<div class = 'dialog' name='" + this.name + "'>" + this.template + "</div>");
        var dialog = $("[name=" + this.name + "]");

        this.element = dialog.kendoWindow({
            visible: false,
            modal: this.modal,
            resizable: true,
            height: this.height,
            width: this.width,
            title: this.title,
            deactivate: this.events.close
        });

        this.kendoObject = dialog.data("kendoWindow");

        var wireUp = () => {
            this.kendoObject.center();
            this.kendoObject.open();
            for (var i in this.events) {
                $("button[name=" + i + "]", this.element).on("click", this.events[i]);
            }
        }

        if (this.component) {
            loadComponent(dialog, this.component, (c) => {
                wireUp();
                if (callback) { callback(); }
                return this;
            });
        }
        else {
            wireUp();

            if (callback) {
                callback();
            }

            return this;
        }
    }
}


class BootstrapDialog extends Widget {
    constructor(args) {
        super(null, args);

        if (!args)
            args = {};

        this.args = args;

        this.name = args.name || "Dialog";
        this.title = args.title || "Dialog";
        this.height = args.height || "auto";
        this.width = args.width || 'md';
        this.template = args.template || "";
        this.component = args.component || false;
        this.elementId = Guid();
        this.container = null;
        this.footer = args.footer;
        this.modal = null;

        this.events = args.events || {}
    }

    async init(callback) {
        $("body").append(`<div class="modal fade" name="${this.name}" id="${this.elementId}" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-${this.width}">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            ${this.title}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body" name="${this.name}-content">
                        ${this.template}
                    </div>
                    <div class="modal-footer" name="footer">
                        ${this.footer}
                    </div>
                </div>
            </div>
        </div>`);

        this.element = $(`#${this.elementId}`);

        var dialog = $(`[name=${this.name}-content]`, this.element);
        this.modal = new bootstrap.Modal(this.element, {});

        var wireUp =  () => {
            for (var i in this.events) {
                $("button[name=" + i + "]", this.element).on("click", this.events[i]);
            }
        }

        if (this.component) {
            loadComponent(dialog, this.component, (c) => {
                wireUp();

                if (callback)
                    callback();
            
                return this;
            });
        } else {
            wireUp();

            if (callback)
                callback();

            return this;
        }
    }

    show() {
        this.modal.show();
    }

    hide() {
        this.modal.hide();
    }
}


class BootstrapTabs extends Widget {
    constructor(args) {
        super(null, args);

        if (!args)
            args = {};

        this.args = args;

        this.name = args.name.replace(" ", "-") || "Tabs";
        this.id = Guid();
        this.title = args.title || (args.name + " Tabs");
        this.defaultTab = args.defaultTab || null;
        this.tabs = args.tabs;
        this.idPrefix = null;

        this.container = args.container;
    }

    async init(app, callback) {
        this.idPrefix = this.name + '-' + this.id;
        var tabButtons = this.generateTabButtons();
        var tabContents = this.generateTabContents();

        $(this.container).append(`
            <div class="tab-control" name="${this.idPrefix}-tabs">
                <nav>
                    <div class="nav nav-tabs" id="${this.idPrefix}-nav-tab" role="tablist">
                        ${tabButtons}
                    </div>
                </nav>
                <div class="tab-content" id="${this.idPrefix}-tabContent">
                    ${tabContents}
                </div>
            </div>
        `);

        await this.initEventsAndCallbacks(app);

        if (this.defaultTab != null) {
            $(`#${this.idPrefix}-${this.defaultTab}-tab`, this.container).click();
        }

        if (callback)
            await callback();
    }

    generateTabButtons() {
        var tabs = ``;

        for (var i in this.tabs) {
            var tab = this.tabs[i];

            var isActive = this.defaultTab == tab.name || (this.defaultTab == null && i == 0)
                ? 'active'
                : null;

            var icon = tab.icon != null
                ? '<span class="k-icon ' + tab.icon + '"></span>'
                : '';

            tabs += `<button class="nav-link bg ${isActive}" id="${this.idPrefix}-${tab.name}-tab" data-bs-toggle="tab" data-bs-target="#${this.idPrefix}-${tab.name}" type="button" role="tab" aria-controls="${this.idPrefix}-${tab.name}" aria-selected="true" tabindex="${i}">
                    ${icon} ${tab.label}
                </button>`;
        }

        return tabs;
    }

    generateTabContents() {
        var tabs = '';

        for (var i in this.tabs) {
            var tab = this.tabs[i];

            var isActive = this.defaultTab == tab.name || (this.defaultTab == null && i == 0)
                ? 'active show'
                : '';

            var content = '';

            if (tab.content && tab.content != '') {
                content = tab.content;
            }

            tabs += `<div class="tab-pane fade ${isActive}" id="${this.idPrefix}-${tab.name}" role="tabpanel" aria-labelledby="${this.idPrefix}-${tab.name}-tab" name="${this.idPrefix}-${tab.name}">
                    ${content}
                </div>`;
        }

        return tabs;
    }

    async initEventsAndCallbacks(app) {
        for (let i in this.tabs) {
            let tab = this.tabs[i];

            console.log(`Setup: #${this.idPrefix}-${tab.name}-tab`, tab);

            if (tab.onclick != null) {
                $(`#${this.idPrefix}-${tab.name}-tab`).on('click', tab.onclick);
            }

            if (tab.callback != null) {
                $(`#${this.idPrefix}-${tab.name}-tab`).on('click', tab.callback);
            }

            if (tab.component != null) {
                tab.loaded = false;

                $(`#${this.idPrefix}-${tab.name}-tab`).on('click', () => {
                    if (tab.loaded)
                        return;

                    let container = tab.componentContainer != null
                        ? $(tab.componentContainer)
                        : $(`#${this.idPrefix}-${tab.name}`);

                    if (tab.init != null) {
                        tab.init();
                    } else {
                        loadComponent(container, tab.component, (c) => {
                            let contentContainer = tab.contentContainer != null
                                ? $(tab.contentContainer)
                                : $(`#${this.idPrefix}-${tab.name}`);

                            c.init(app, contentContainer);
                        });
                    }

                    tab.loaded = true;
                });
            }
        }
    }
}
class Chart extends Widget
{
    constructor(element, args) {
        super(element, args);
        this.chartElement = $(element).append("<div></div>").children().first();
        this.text = args.text;
        this.showLegend = args.showLegend;
        this.series = args.series || [];
        this.categories = args.categories || [];
        this.max = args.max;
        this.type = args.type || "bar";
        this.showMinorLines = args.showMinorLines;
        this.valueTemplate = args.valueTemplate ||"#= value #";
        this.categoryTemplate = args.categoryTemplate || "#= value #";
        this.tooltipTemplate = args.tooltipTemplate || "#= series.name #: #= value #";;
        this.axisCrossingValue = args.axisCrossingValue || 0;
        this.colors = args.colors || session.app.Config.Themes.Default.colours.charts;

        for (let i in this.series) {
            this.series[i].color = this.colors[i];
        }
    }

    init() {
        this.kendoObject = $(this.chartElement).kendoChart({
            axisCrossingValue: this.axisCrossingValue,
            title: { text: this.text },
            legend: { visible: this.showLegend, position: "top" },
            seriesDefaults: { type: this.type },
            series: this.series,
            valueAxis: {
                max: this.max + ((this.max / 100) * 5),
                line: { visible: false },
                minorGridLines: { visible: this.showMinorLines },
                labels: { rotation: "auto", template: this.valueTemplate }
            },
            categoryAxis: {
                categories: this.categories,
                majorGridLines: { visible: false },
                labels: { rotation: "auto", template: this.categoryTemplate }
            },
            tooltip: {
                visible: true,
                template: this.tooltipTemplate
            }
        }).data('kendoChart');
    }
}
class PieChart extends Widget {
    constructor(element, data) {
        super(element);
        this.data = data;
        this.chartName = $(element).attr("name");
        $(element).append("<div name='" + this.gridName + "PieChart'></div>");
        this.chartElement = $("[name=" + this.gridName + "PieChart]", $(element));
    }
    
    init() {
        this.kendoObject = $(this.chartElement).kendoChart({
            seriesDefaults: {
                labels: {
                    visible: true,
                    background: "transparent",
                    template: "#= category #: \n #= kendo.toString(value, type.aggregateMoneyFormat)#",
                }
            },
            legend: {
                align: "right"
            },
            series: [{
                type: "pie",
                startAngle: 150,
                data: this.data,
                padding: 60
            }]
        }).data("kendoChart");
    }
    
    setData(data) {
        this.kendoObject.options.series[0].data = data;
    }
    
    refresh() {
        this.kendoObject.refresh();
    }
}
class ConfirmDialog extends Dialog {

    init(callback) {
        if (!this.template) {
            this.template = "<div class='dialog'><p>" + this.args.question + "</p><hr /><button class='btn btn-sm btn-primary float-end' name='confirm'>" + this.args.confirm + `</button></div>`;
        }
        super.init(callback);
    }
}
class ConsoleDialog extends Dialog {
    constructor(args) {
        super(args);
        if (typeof args != 'undefined') {
            this.width = args.width || 800;
            this.height = args.height || 500;
            this.title = args.title || "Console";
        } else {
            this.width = 800;
            this.height = 500;
            this.title = "Console";
        }

        this.template = `
            <div class='console' name='console' style='overflow: auto;position: relative;height: ` + this.height + `px;'>
               <style>
                  [name=console] { padding: 5px; }
                  [name=console] > .message { }
                  [name=console] > .message > * { vertical-align: top; }
                  [name=console] > .message > .message { display: inline-block; border: none; max-width: 90%; word-wrap: break-word; }
                  [name=console] > .message .time { margin-right: 10px; }
                  [name=console] > .message.success > .message { color: green; }
                  [name=console] > .message.info > .message { color: green; }
                  [name=console] > .message.debug > .message { color: blue; }
                  [name=console] > .message.warning > .message { color: #D8A700; }
                  [name=console] > .message.error > .message { color: red; }
                  [name=console] > .message.fatal > .message { color: red; }
               </style>
            </div>
            `;
    }
    
    log(level, message) {
        let d = new Date();
        let time = d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds();
        $("[name=console]", this.element).append($("<div class='message " + level + "'><span class='time'>" + time + "</span><pre class='message'>" + html.encode(message) + "</pre></div>"));
        $("[name=console]", this.element).scrollTop($("[name=flowConsole]", this.element).height());
    }
}
class ExportDialog extends Dialog {
    constructor(args) {
        super(args);
        this.width = args.width || 500;
        this.height = args.height || 150;
        this.exportURL = args.exportURL;
        this.exportColumns = args.exportColumns;
        this.exportFileName = args.exportFileName;
        this.title = args.title;
    }

    async init() {
        this.title = this.title || (await api.getResource("Default", "Export", session.culture)).DisplayName;

        var xmlFormatName = (await api.getResource("Default", "exportXML", session.culture)).DisplayName;
        var csvFormatName = (await api.getResource("Default", "exportCSV", session.culture)).DisplayName;
        var excelFormatName = (await api.getResource("Default", "exportExcel", session.culture)).DisplayName;
        var jsonFormatName = (await api.getResource("Default", "exportJSON", session.culture)).DisplayName;

        var formatsDataSource = {
            data: [
                { format: "xml", formatName: xmlFormatName },
                { format: "csv", formatName: csvFormatName },
                { format: "excel", formatName: excelFormatName },
                { format: "json", formatName: jsonFormatName }
            ]
        };

        this.template = await this.getTemplate();

        var that = this;

        super.init(() => {
            $("[name=exportDropdown]", this.element).kendoDropDownList({
                dataTextField: "formatName",
                dataValueField: "format",
                dataSource: formatsDataSource,
                change: function (e) {
                    e.preventDefault();
                    that.handleFormatSelection.apply(that, [this.value()]);
                }
            });
            $("[name=exportDownloadButton]", this.element).on("click", that.events.close);
            this.handleFormatSelection("xml");
        });
    }

    getDownloadLink(format) {
        let formattedURL = removeQueryParameter("$format", removeQueryParameter("$skip", removeQueryParameter("$top", this.exportURL)));

        formattedURL += (formattedURL.indexOf("?") !== -1)
            ? "&$format=" + format
            : "?$format=" + format;

        formattedURL += "&t=" + session.token + "&moneyFormat=" + type.moneyFormat + "&dateFormat=" + type.dateFormat + "&culture=" + session.culture;

        if (this.exportColumns) {
            formattedURL += "&$select=" + this.exportColumns.join();
        }

        if (format == "csv") {
            formattedURL += "&quotes=&delimiter=;";
        }

        return formattedURL;
    }

    handleFormatSelection(format) {
        $("[name=exportDownloadButton]", this.element).attr("href", this.getDownloadLink(format));

        var extension = (format == "excel") ? ".xlsx" : "." + format;
        if (this.exportFileName) {
            $("[name=exportDownloadButton]", this.element).attr("download", this.exportFileName + extension);
        } else {
            $("[name=exportDownloadButton]", this.element).attr("download", "export" + extension);
        }
    }

    async getTemplate(args) {
        args = args || {};
        var exportAs = args.exportAs || (await api.getResource("Default", "ExportAs", session.culture)).DisplayName;
        var download = args.download || (await api.getResource("Default", "Download", session.culture)).DisplayName;
        var close = args.close || (await api.getResource("Default", "Close", session.culture)).DisplayName;
        return `
            <div class="input-group input-group-sm mb-1">
                <span class="input-group-text">${exportAs}</span>
                <input type="text" class="form-control" name="exportDropdown" />
            </div>

            <hr />

            <a class="btn btn-sm btn-primary float-end" name="exportDownloadButton">
                <span class="k-icon k-i-download"></span>${download}
            </a>
        `;

    }
}
class DetailWidget extends Widget {
    // Consumes https://demos.telerik.com/kendo-ui/templates/expressions to build a read only detail view of an object
    // or portion of an object
    constructor(element, args) {
        super(element);
        args = args || {};
        // default configuation for all grid widgets.
        this.detailName = $(element).attr("name");
        this.detailElement = element;
        this.fields = args.fields;
        this.labelTooltip = true;
        this.title = args.title || "Details";

        if (args.hasOwnProperty("header")) {//false || true -> true. Hence hasOwnProperty is required.
            this.header = args.header;
        } else {
            this.header = true;
        }

        this.editable = false;
        this.splits = args.splits;

        if (args.data) {
            this.data = new kendo.observable(args.data);
        }

        this.config = { endpoint: $(this.detailElement).data("endpoint"), odataAppend: "(" + getQueryParameter("Id") + ")" + $(element).data("odataappend") };

        // attach this object to the widget element and add the grid element to prepare for an init call.
        $(this.detailElement).data("widget", this);
    }

    async init() {
        if (!this.template) { this.buildTemplate(); }

        if (this.data) {
            await this.render();
        }
        else {
            let d = await api.get(this.config.endpoint + this.config.odataAppend);
            this.data = new kendo.observable(d);
            this.parseData(this.fields);
            await this.render();
        }
    }

    parseData(meta) {
        $.each(meta, (i, p) => {
            if (p.type === 'date') {
                this.data[p.field] = this.data[p.field] !== null
                    ? new Date(this.data[p.field])
                    : this.data[p.field];
            }
        });
    }

    async render() {
        $(this.detailElement).append(kendo.template(this.template)(this.data));
    }

    computeFields(exclude, callback) {
        let that = this;
        type.fieldsFor(this.config.endpoint, (fields) => {
            that.fields = fields.filter(f => exclude.IndexOf(f.field) < 0);
            if (callback) { callback(); }
        });
    }

    buildTemplate() {

        let build = (that) => {
            let fieldSet = "";

            that.fields.map((meta) => {
                if (that.splits && that.splits.indexOf(meta.field) > - 1) { fieldSet += "</ul><ul class='fieldList'>"; }
                fieldSet += "<li name='" + that.config.endpoint + "/" + meta.field + "'><label title='" + meta.description + "' for='" + meta.field + "'>" + meta.title + "</label><div class='value'>" + that.fieldValueExpression(meta.field) + "</div></li>";
            });

            that.template = "";

            if (that.header) {
                that.template = "<h3>" + that.title + "</h3>";
            }

            that.template += (that.toolbar
                    ? "<div class='k-header k-grid-toolbar'>" + that.toolbar + "</div><div name='details'><ul class='fieldList'>" + fieldSet + "</ul></div>"
                    : "<div name='details'><ul class='fieldList'>" + fieldSet + "</ul></div>");
        };

        if (!this.fields) {
            this.computeFields(null, () => { build(this); });
        }
        else { build(this); }
    }

    fieldValueExpression(fieldName) { /* Intentional */ }
}
class EditorDialog extends Dialog {
    constructor(args) {
        super(args);

        args = args || {};
        this.args = args;
        this.data = new kendo.observable(args.data);
        this.confirm = this.args.confirm;
        this.config = args.config;
        this.fields = args.fields;
        this.header = false;

        this.events.confirm = async function (e) {
            e.preventDefault();
            this.data.save();
            this.events.close();
        };
    }

    async init() {
        this.template += "<div name='editor' class='editorDialog'></div><hr><div class='value'><button class='btn btn-sm btn-primary float-end' name='confirm'>" + this.confirm + "</button></div>";
        await super.init();
        this.writableEditor = new WritableDetailView($("[name='editor']", this.element), this);
        await this.writableEditor.init();
        this.data = this.writableEditor.data;
    }
}
class GridWidget extends Widget {
    constructor(element, dataSource) {
        super(element);

        // default configuation for all grid widgets.
        this.gridName = $(element).attr("name");
        $(element).prepend("<div name='" + this.gridName + "Grid'></div>");
        this.gridElement = $("[name=" + this.gridName + "Grid]", $(element));
        this.kendoDataSource = dataSource;
        this.toolbar = [];
        this.commands = [];
        this.searchable = false;
        this.editable = true;
        this.commandWidth = 100;
        this.sortable = true;
        this.reorderable = true;
        this.persistSelection = false;
        this.selectColumns = false;
        this.change = function (e) { /* to avoid exceptions when called */ };
        this.resizable = true;
        this.groupable = true;
        this.headerTooltip = true;
        this.exports = false;
        this.scrollable = true;
        this.resizable = {
            rows: true
        };
        this.rowResize = () => { };

        this.pageable = {
            refresh: true,
            pageSizes: [10, 20, 50, 100, 200, 500, 1000],
            numeric: true
        };

        this.endpoint = $(this.gridElement).data("endpoint");
        this.odataAppend = $(this.gridElement).data("odataappend");

        if (session.app.Config.hasOwnProperty("Components") && session.app.Config.Components.hasOwnProperty("Grids") &&
            session.app.Config.Components.Grids.hasOwnProperty("Details")) {

            this.viewCommand = (session.app.Config.Components.Grids.Details.indexOf("Link") !== -1) || false;
            this.expandView = (session.app.Config.Components.Grids.Details.indexOf("Expand") !== -1) || false;
            this.viewDialogCommand = (session.app.Config.Components.Grids.Details.indexOf("Popup") !== -1) || false;
        } else {
            this.viewCommand = true;
            this.expandView = true;
            this.viewDialogCommand = true;
        }
        // if we have a detail template, expansion by convention is supported, else remove expansion functionality.
        let detailTemplate = $("script[name=details]", this.element);

        if (detailTemplate.length === 1) {
            this.detailTemplate = kendo.template($("script[name=details]", this.element).html());
        }
        else {
            delete this.detailExpand;
        }

        // attach this object to the widget element and add the grid element to prepare for an init call.
        $(this.gridElement).data("widget", this);
    }

    dataItem(row) {
        return this.kendoObject.dataItem(row);
    }

    refresh() {
        this.kendoObject.dataSource.read();
        this.kendoObject.refresh();
    }

    dataSource() {
        return this.kendoObject.dataSource;
    }

    clearSelection() {
        this.kendoObject.clearSelection();
    }

    select() {
        let items = [];
        let grid = this;
        this.kendoObject.select().each(function (i) {
            items.push(grid.dataItem(this))
        });
        return items;
    }

    setColumns(columns) {
        this.kendoObject.setOptions({ columns: columns });
        this.postInit();
    }

    async buildConfig() {
        if (this.groupable) {
            this.groupable = {
                messages: {
                    empty: (await api.getResource("Default", "Grouping", session.culture)).Description
                }
            };
        }

        this.config = {
            endpoint: this.endpoint,
            odataAppend: this.odataAppend,
            columns: this.columns,
            scrollable: this.scrollable,
            pageable: this.pageable,
            editable: this.editable,
            sortable: this.sortable ? { mode: "multiple", allowUnsort: true, showIndexes: true } : false,
            reorderable: this.reorderable,
            change: this.change,
            resizable: this.resizable,
            rowResize: this.rowResize,
            filterable: this.filterable,
            groupable: this.groupable,
            dataBound: this.dataBound,
            detailTemplate: this.detailTemplate,
            detailExpand: this.detailExpand,
            search: this.search,
            sort: this.sort,
            persistSelection: this.persistSelection
        };

        if (this.toolbar.length) {
            this.config.toolbar = this.toolbar;
        }

        this.config.dataSource = this.kendoDataSource || (await model.getDatasource(this.config));
        return this.config;
    }

    async init() {
        let that = this;

        if (that.filterable) {
            await that.setupFilteringOperators.apply(that);
        }

        if (!that.columns) {
            that.columns = model.columnsFor(that.endpoint);
        }

        if (that.commands.length > 0) {
            that.columns.push({ width: that.commands.length * that.commandWidth, template: that.commandColumn() });
        }

        for (let i in that.columns) {
            that.enableMultiselectFilteringOn(that.columns[i]);
        }

        await that.buildConfig.apply(that);

        if (that.searchable) {
            if (that.config.toolbar == null) { that.config.toolbar = []; }
            that.config.toolbar.push({ name: "search" });

            that.search = that.search != null
                ? that.search
                : { fields: that.columns.filter(x => x.Type == "string").map(c => c.field) };
        }

        if (that.exports) {
            that.config.enableExport = true;

            that.config.dataSource.setExportUrls = function () {
                that.generateExportLinks.apply(that);
            };

            let exportText = (await api.getResource("Default", "Export", session.culture)).DisplayName;

            if (that.config.toolbar == null) {
                that.config.toolbar = [];
            }

            that.config.toolbar.unshift({
                template: "<button class='btn btn-sm btn-primary' name='exportButton'><span class='k-icon k-i-download'></span>" + exportText + "</button>"
            });
        }

        if (that.selectColumns) {
            let selectColumnsText = (await api.getResource("Default", "selectcolumns", session.culture)).DisplayName;

            if (that.config.toolbar == null) {
                that.config.toolbar = [];
            }

            that.config.toolbar.push({
                template: "<button name='selectColumnsButton'><span class='k-icon k-i-download'></span>" + selectColumnsText + "</button>"
            });
        }

        // initialize the kendo grid
        that.kendoObject = that.gridElement.kendoGrid(that.config).data("kendoGrid");
        this.postInit();
    }
    
    postInit() {
        let that = this;

        if (that.headerTooltip) {
            let gridHead = that.kendoObject.thead;
            let cells = gridHead.find("th");

            $(cells).each(async function (index) {

                let fieldName = $(this).attr("data-field");

                if (that.columns && that.columns.filter(c => c.field == fieldName).length > 0) {
                    let matchingColumn = that.columns.filter(c => c.field == fieldName)[0];

                    if (matchingColumn.description) {
                        $(this).attr("title", matchingColumn.description);
                    }
                }
            });
        }

        if (that.selectColumns) { that.generateSelectColumns(); }
        if (that.exports) { that.generateExportLinks(); }
    }

    async setupFilteringOperators() {
        this.filterable = {
            operators: {
                extra: false,
                number: {
                    gte: (await api.getResource("Default", "greaterthan", session.culture)).DisplayName,
                    lte: (await api.getResource("Default", "lessthan", session.culture)).DisplayName,
                    eq: (await api.getResource("Default", "isequalto", session.culture)).DisplayName,
                    neq: (await api.getResource("Default", "isnotequalto", session.culture)).DisplayName
                },
                string: {
                    contains: (await api.getResource("Default", "contains", session.culture)).DisplayName,
                    startswith: (await api.getResource("Default", "startswith", session.culture)).DisplayName,
                    eq: (await api.getResource("Default", "isequalto", session.culture)).DisplayName,
                    neq: (await api.getResource("Default", "isnotequalto", session.culture)).DisplayName
                },
                date: {
                    gte: (await api.getResource("Default", "fromdate", session.culture)).DisplayName,
                    lte: (await api.getResource("Default", "todate", session.culture)).DisplayName
                }
            },
            messages: {
                selectedItemsFormat: (await api.getResource("Default", "selecteditemsformat", session.culture)).DisplayName
            }
        };

        this.columns.filter(c => c.type === "date" && (c.filterable === null || c.filterable === undefined)).forEach(c => {
            c.filterable = {
                ui: (element) => element.kendoDatePicker({ format: type.dateFormat })
            };
        });
    }

    enableMultiselectFilteringOn(col) {
        if (col.values) {
            col.filterable = {
                multi: true,
                dataSource: col.values,
                itemTemplate: function (e) {
                    return (e.field == "all")
                        ? "<li><input type='checkbox' name='" + e.field + "' value='all'/><span>All</span></li>"
                        : "<li><input type='checkbox' name='" + e.field + "' value='#=data.id#'/><span>#= data.name #</span></li>";
                }
            };
        }
    }

    async generateSelectColumns() {
        $("[name=selectColumnsButton]", this.gridElement).on("click", selectColumns);
    }

    async selectColumns(e) {

        let grid = this;
        let selectText = (await api.getResource("Default", "selectcolumns", session.culture)).DisplayName;
        let resourceKey = $(grid.gridElement).closest(".component").attr("data-resource-key");

        let selectColumnDialog = new Dialog({
            title: selectText,
            width: 400,
            height: 800
        });

        let columnsGrid = null;

        selectColumnDialog.template = "<div name='selectColumnGrid' style='width:400px;height:700px;'></div><hr><div class='value'><button name='select'>" + selectText + "</button></div>";

        selectColumnDialog.events.select = async (e) => {
            let selectedColumns = [];
            let selected = columnsGrid.select();

            for (let s in selected) {
                let text = (await api.getResource(resourceKey, s.Name, session.culture))
                    .ShortDisplayName;

                selectedColumns.push({
                    field: s.Name,
                    type: s.Type,
                    title: text,
                    format: getColumnFormat(s.Type)
                });
            }

            if (grid.commands.length > 0) {
                selectedColumns.push({
                    width: grid.commands.length * grid.commandWidth,
                    template: grid.commandColumn()
                });
            }

            for (let i in grid.columns) {
                grid.enableMultiselectFilteringOn(grid.columns[i]);
            }

            grid.setColumns(selectedColumns);
            selectColumnDialog.events.close();
        };

        selectColumnDialog.init(initSelectColumnsDialog);
    }

    getColumnFormat(colType) {

        if (s.Type === "date") {
            return "{0: " + type.dateFormat + "}";
        }

        if (s.Type === "number") {
            return "{0: " + type.moneyFormat + "}";
        }

        return null;
    }

    async initSelectColumnsDialog() {

        let fields = (await api.getType(grid.config.endpoint)).Properties.filter(r => r.Type !== 'array');

        let columnsGrid = new GridWidget($("[name=selectColumnGrid]"), {
            data: fields
        });

        columnsGrid.sortable = true;
        columnsGrid.filterable = false;
        columnsGrid.pageable = false;
        columnsGrid.groupable = false;

        columnsGrid.columns = [
            { width: 50, selectable: true },
            { field: "Name" }
        ];

        await columnsGrid.init();

        $("tbody > tr", columnsGrid.gridElement).each(function () {
            let uid = $(this).attr("data-uid");
            let dataItem = columnsGrid.dataSource().getByUid(uid);

            if (grid.columns.filter(r => r.field == dataItem.Name).length > 0) {
                columnsGrid.kendoObject.select($(this));
            }
        });
    }

    async generateExportLinks() {
        if (this.dataSource || this.config.dataSource) {
            $("[name=exportButton]", this.gridElement).on("click", (e) => {
                e.preventDefault();

                let exportDialog = new ExportDialog({
                    exportURL: this.dataSource().lastGet.url,
                    exportColumns: this.columns.filter(c => c.field).map(c => c.field),
                    height: 210
                });

                exportDialog.init();
            });
            
        }
    }

    resize() {
        this.kendoObject.resize();
    }

    commandColumn() {
        let result = "<div class='btn-group btn-group-sm'>";

        this.commands.forEach((command) => {
            if (command.template) {
                result += command.template;
            }
            else if (command.href) {
                result += "<a name='" + command.name + "' href='" + command.href + "'><span class='k-icon " + command.icon + "'></span>" + command.text + "</a>";
            } else {
                result += `<button class="btn btn-primary" name="` + command.name + `">
                        <span class='k-icon ` + command.icon + `'></span> ` + command.text + `
                    </button>`
            }
        });

        result += '</div>';

        return result;
    }

    dataBound(e) {
        let grid = this;
        let widget = $(this.element).data("widget");

        widget.commands.forEach(function (command) {
            if (command.click) {
                $("button[name=" + command.name + "]", widget.gridElement).on("click", function (e) {
                    e.preventDefault();
                    let item = grid.dataItem($(e.currentTarget).closest("tr"));
                    command.click(e, item, grid, widget);
                });
            }
        });
    }

    detailExpand(e) {
        let widget = $(this.element).data("widget");
        let item = this.dataItem(e.masterRow);

        let args = {
            event: e,
            item: item,
            grid: this,
            widget: widget,
            tabStrip: $("[name=tabs]", e.detailRow).kendoTabStrip({
                animation: {
                    open: {
                        effects: "fadeIn"
                    }
                }
            })
        };

        if (!item.$expanded) {
            widget.initilizeChildrenOf($(e.detailRow), args, function () {

                if (widget.onDetailInit) {
                    widget.onDetailInit(args);
                }
            });

            item.$expanded = true;
        }
    }
}
class ContextMenuWidget extends Widget {
    constructor(element) {
        super(element);

        this.contextMenuName = $(this.element).attr("name") + "ContextMenu";
        $(element).data("contextMenuWidget", this);
        this.commands = [];
    }

    init(pageX, pageY) {
        if ($("div[name=" + this.contextMenuName + "]", this.element).length > 0) {
            this.close();
        }

        $(this.element).append("<div name='" + this.contextMenuName + "' class='contextMenu'></div>");
        this.contextMenuElement = $("[name=" + this.contextMenuName + "]", $(this.element));

        this.prepareContents();

        $(this.contextMenuElement).focus();
        $(document).off().on('click', () => $(this.contextMenuElement).remove());

        this.setPosition(pageX, pageY);
    }

    close() {
        $("div[name=" + this.contextMenuName + "]", this.element).remove();
    }

    prepareContents() {
        let result = "<ul>";

        this.commands.forEach((command) => {
            if (command.template) {
                result += command.template;
            } else {
                if (command.href) {
                    result += "<li name='" + command.name + "' href='" + command.href + "'><span class='k-icon " + command.icon + "'></span>" + command.text + "</li>";
                }
                else {
                    result += "<li name='" + command.name + "'><span class='k-icon " + command.icon + "'></span>" + command.text + "</li>";
                }
            }
        });

        result += "</ul>";

        $(this.contextMenuElement).html(result);
    }

    setPosition(pageX, pageY) {
        if (pageY + this.commands.length * 35 > window.innerHeight) {
            let difference = pageY + (this.commands.length * 35) - window.innerHeight + 35; // Footer is 35px high.
            pageY -= difference;
        }

        $(this.contextMenuElement).css({
            display: "block",
            position: "absolute",
            left: pageX,
            top: pageY
        });
    }
}
class FileDropContainerWidget extends Widget {
    constructor(element) {
        super(element, null);
        this.container = element;
        this.events = {};
        $(this.container).data("fileDropContainerWidget", this);
    }

    init() {
        this.counter = 0;

        $(this.container).on("dragstart", (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.counter++;
        });

        $(this.container).on("dragover", (e) => {
            e.preventDefault();
            e.stopPropagation();
            if (this.fileUploaderTag == null) {
                $(this.container)[0].style.setProperty("display", "none", "important");

                this.fileUploaderTag = $("<input class='fileUploaderTag' type='file' multiple name='fileUpload'/>").insertAfter(this.container);
                this.fileUploaderTag.on("drop", (e) => {
                    this.events.drop(e);
                    this.remove();
                });

                this.fileUploaderTag.on("change", (e) => {
                    this.events.drop(e);
                    this.remove();
                });
            }
        });

        $(this.container).on("dragleave", (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.counter--;

            if (this.counter == 0) {
                this.remove();
            }
        });
    }

    remove() {
        this.counter = 0;

        if (this.fileUploaderTag != null) {
            this.fileUploaderTag.remove();
        }
        this.fileUploaderTag = null;

        $(this.container)[0].style.setProperty("display", "flex", "important");
    }
}
class Picker extends Dialog {
	constructor(args) {
		super(args);
		args = args || {};
		this.multiSelect = args.multiSelect;
		this.valueTemplate = args.valueTemplate;
		this.displayTemplate = args.displayTemplate;
		this.dataSource = args.dataSource || null;
		this.confirm = this.args.confirm || type.getResource("Core", "Confirm", session.culture).DisplayName;
		this.close = this.args.close || type.getResource("Core", "Close", session.culture).DisplayName;
		this.type = args.type;
		this.query = args.query;

		this.template = `
			<div class='dialog'>
				<input name='term' type='text' />
				<ul class='fieldList' name='items' style='overflow:scroll; overflow-x:hidden;height:400px;'>
				</ul>
				<div class='value'>
					<button name='confirm'>` + this.confirm + `</button>
					<button name='close'>` + this.close + `</button>
				</div>
			</div>
		`;
	}

	/*
			var p = new Picker({
				type: 'B2B/Company',
				valueTemplate: "#:Id#",
				multiSelect: false,
				displayTemplate: "<div class='item'>#:Name#</div>",
				query: "B2B/Company?$expand=References&$filter=References/any(r:contains(r/Id,'TERM')) or contains(Name,'TERM')&$top=50"
			});
			p.pick = function(result) {
				var CompanyId = result;
				var roleRow = $(e.target).closest("tr").prev();
				var role = $(roleRow).closest(".k-grid").data("kendoGrid").dataItem(roleRow);
				api.get("B2B/Company(" + result + ")", function(data) {
					if(data != null) {
						var companyData = data;
						grid.data("kendoGrid").dataSource.add(companyData);
					}
				});
			};
			p.init();
	/*/

	async search(term) {
		let queryReplaced = this.query.replaceAll("TERM", term);
		let data = await api.get(queryReplaced);
		this.dataSource.data(data.value);
		//this.dataSource.read();
		//this.list.data("kendoListView").refresh();
	}

	init(callback) {
		super.init(() => {
			let itemTemplate = "<li>" +
				(this.multiSelect
				? "<input type='checkbox' name='selected' value='" + this.valueTemplate + "'></input>"
				: "<input type='radio' name='selected' value='" + this.valueTemplate + "'></input>"
				)
				+ this.displayTemplate + "</li>";

			let that = this;
			
			let build = () => {
				this.list = $("[name=items]", this.element).kendoListView({
					dataSource: this.dataSource,
					scrollable: true,
					template: kendo.template(itemTemplate)
				});

				$("[name=term]", this.element).on('keyup', function (e) {
					that.search($(this).val());
				});

				$("button[name='confirm']").on("click", () => {
					if(!this.multiSelect) {
						that.pick($("input[name='selected']:checked", that.element).val());
					} else {
						var rows = $("input[name='selected']:checked");
						var items = [];
						$.each(rows, function(i, v) { items.push($(this).val()); });
						this.pick(items);
					}
				});
			};
			if(!this.dataSource) {
				model.getDatasource({ endpoint: this.type }, (ds) => {
					this.dataSource = ds;
					build();
				});
			} else {
				build();
			}
			
		});
    }
}
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
class Workspace extends Widget {
    constructor(element, args) {
        super(element, args);

        this.trees = [];
    }

    init() {
        let that = this;

        /*
         * CSS to be added to theme CSS template
         * 
            [name=splitter] { margin: 0; border: none; box-shadow: none; width: 100%;  height: 100%; }
	        [name=splitter] > .panel { height: 100%; padding: 10px; }
            [name=workspace] { overflow: visible; right: 0; }
            [name=workspace] > .component { margin: 0; width: 100%; height: 100%; overflow: visible; }
         */
        that.element.append(`
            <div name="splitter">
	            <div class="panel left"></div>
	            <div class="panel right"></div>
            </div>`);

        that.splitter = $("[name=splitter]", that.element).kendoSplitter({
            scrollable: false,
            panes: [
                { collapsible: true, size: "320px" },
                { collapsible: false, scrollable: false }
            ]
        }).data("kendoSplitter");

        that.leftPanel = $("[name=splitter] > .panel.left", that.element);
        that.workspace = $("[name=splitter] > .panel.right", that.element);

        $(window).on("resize", function (e) {
            that.resize();
        });

        ths.initialized = true;
    }

    resize() {
        let headerHeight = $("body > header").height();
        let footerHeight = $("body > footer").height();
        let bodyHeight = $("body").height();

        this.element.height(bodyHeight - (headerHeight + footerHeight));
        this.element.width("100%");
    }

    connectTo(tree) {
        if (!this.initialized) {
            this.init();
        }

        this.trees.push(tree);
        this.leftPanel.append(tree.element);
        tree.init();
    }

    disconnectFrom(tree) {
        this.leftPanel.remove(tree.element);
    }
}
class WritableDetailView extends DetailWidget {
    constructor(element, args) {
        super(element, args);
        this.editable = true;
    }

    async init() {
        await super.init();
        kendo.bind($(this.detailElement), this.data);
    }

    fieldValueExpression(fieldName) {
        var meta = this.fields.filter(i => i.field === fieldName)[0];
        var fieldType = meta.fieldType || "text";

        if (!meta.type) {
            meta.type = "string";
        }

        if (meta.template) {
            return meta.template;
        } else {
            return `<input class="form-control input-sm" type="${fieldType}" data-bind="value: ${fieldName}" name="${fieldName}" />`;
        }
    }
}