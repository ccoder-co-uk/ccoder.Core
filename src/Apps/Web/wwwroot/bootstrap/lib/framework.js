const draw = {
    rect: function(ctx, x, y, w, h, col, sel, stroke) {
        ctx.beginPath();
        ctx.rect(x, y, w, h);
        ctx.lineWidth = 1;
        ctx.strokeStyle = '#000';

        if (stroke !== false) {
            ctx.stroke();
        }

        ctx.beginPath();
        ctx.rect(x, y, w, h);

        col = ctx.isPointInPath(mouseposition.x, mouseposition.y)
            ? (sel || col || '#cbcbcb')
            : (col || '#cbcbcb');

        ctx.fillStyle = col;
        ctx.fill();
    },

    text: function(ctx, x, y, text, col) {
        ctx.font = "11px Arial";
        ctx.fillStyle = col || '#000';
        ctx.fillText(text, x, y);
    },

    circle: function(ctx, x, y, r, col, sel) {
        ctx.beginPath();
        ctx.arc(x, y, r, 0, 2 * Math.PI);
        const hover = ctx.isPointInPath(mouseposition.x, mouseposition.y);

        col = hover
            ? (sel || col || '#cbcbcb')
            : (col || '#cbcbcb');

        r = hover
            ? (r + 1)
            : r;

        ctx.arc(x, y, r, 0, 2 * Math.PI);
        ctx.fillStyle = col;
        ctx.fill();
    },

    semiCircle: function (ctx, x, y, r, start, end, col, sel) {
        ctx.beginPath();
        ctx.arc(x, y, r, start, end);
        const hover = ctx.isPointInPath(mouseposition.x, mouseposition.y);

        col = hover
            ? (sel || col || '#cbcbcb')
            : (col || '#cbcbcb');

        ctx.arc(x, y, r, start, end);
        ctx.fillStyle = col;
        ctx.fill();
    },

    line: function(ctx, fromX, fromY, toX, toY, col, width) {
        ctx.beginPath();
        ctx.moveTo(fromX, fromY);
        ctx.lineTo(fromX + (width || 19), fromY);
        ctx.lineTo(toX - (width || 19), toY);
        ctx.lineTo(toX, toY);
        ctx.lineWidth = width || 2;
        ctx.strokeStyle = col;
        ctx.stroke();
    },

    enableShadows: function(ctx, x, y, w, h, col, offsetY) {
        ctx.shadowColor = col;
        ctx.shadowBlur = 6;
        ctx.shadowOffsetX =  4;
        ctx.shadowOffsetY = offsetY;

        ctx.strokeRect(x, y, w, h);
    },

    disableShadows: function(ctx) {
        ctx.shadowBlur = 0;
        ctx.shadowOffsetX = 0;
        ctx.shadowOffsetY = 0;
        ctx.shadowColor = null;
    }
};

class Drawable {
    constructor(text, x, y, w, h, r) {
        this.x = x;
        this.y = y;
        this.r = r;
        this.h = h;
        this.w = w;
        this.text = text;
    }

    draw(ctx) { /* Abstract method only */ }
}

class Collidable extends Drawable {

    collides() {
        const withinX = (mouseposition.x < this.x + (this.r || this.w)) && (mouseposition.x > this.x - (this.r || 0));
        const withinY = (mouseposition.y < this.y + (this.r || this.h)) && (mouseposition.y > this.y - (this.r || 0));

        let collision = (withinX && withinY)
            ? this
            : null;

        if (collision) {
            collision = this.objects
                ? this.objects.map(o => o.collides()).filter(r => r)[0] || this
                : this;
        }

        return collision;
    }
}
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
/* Api Management */
class Api
{
    constructor(args) {
        args = args || {};
        this.apiRoot = args.apiRoot || session.apiRoot;

        this.cache = {
            meta: [],
            resources: [],
            resourceLoads: []
        };
        this.app = args.app || session.app;
        this.file = {
            upload: async (to, file, callback) => {
                return new Promise((resolve, reject) => {
                    try {
                        let reader = new FileReader();
                        reader.onload = () => this.onFileReaderLoad(to, file, reader, resolve, callback);
                        reader.readAsArrayBuffer(file);
                    } catch(err) {
                        reject(err);
                    }
                });
            },

            destroy: async (path) => {
                await this.destroy("DMS/" + path)
                    .then(() => notification.success("File deleted."));
            }
        };
    }

    onFileReaderLoad(to, file, reader, resolve, callback) {
        let xhr = new XMLHttpRequest();
        xhr.open('POST', this.apiRoot + "DMS/" + to, true);
        xhr.setRequestHeader("Content-Type", file.type);

        xhr.onreadystatechange = () =>
            this.onReadyStateChange(xhr, file, resolve, callback);

        xhr.send(new Uint8Array(reader.result));
    }

    onReadyStateChange(xhr, file, resolve, callback) {
        switch (xhr.readyState) {
            case 2: //HEADERS_RECEIVED
                if (notification)
                    notification.info("Uploading file " + file.name + " ...");
                break;
            case 4: //DONE
                if (notification) {
                    notification.success(file.name + " has been uploaded.");
                } else {
                    notification.fail(file.name + " failed to upload.");
                    log(file.name + " upload fail.", "success");
                }
                resolve(file);
                if (callback)
                    callback(file);
                break;
            default:
                break;
        }
    }

    async login(user, pass, keepToken) {
        return this.send("POST", "Account/Login", {
            User: user,
            Pass: pass
        }).then((token) => {
            if (keepToken)
                this.token = token.id;
            return token;
        }).catch((e) => {
            error(e);
            return false;
        });
    }

    async logout() {
        if (this.token)
            delete this.token;

        return this.send("POST", "Account/Logout", '').catch((e) => error(e));
    }

    async register(details) {
        return api.post("Account/Register", details);
    }

    addToMetaCache(metaSet) {
        for (let i in metaSet) {
            let ctx = metaSet[i];
            let existing = this.cache.meta.filter(x => x.Name == ctx.Name);

            if (existing.length > 0) {
                for (let t in ctx.Types) {
                    existing[0].Types.push(ctx.Types[t]);
                }
            }
            else
                this.cache.meta.push(ctx);
        }
    }

    addToResourceCache(resourceSet) {
        for (let r in resourceSet)
            this.cache.resources.push(resourceSet[r]);
    }

    getType(endpointRef) {
        let context = endpointRef.split('/')[0];
        let typeName = endpointRef.split('/')[1];

        if (this.cache.meta.filter(x => x.Name === context).length === 0)
            throw new Error("Missing " + context + " type information in meta cache");

        let typeGroup = this.cache.meta.filter(x => x.Name === context)[0];

        if (typeGroup.Types.filter(x => x.ServerTypeName === typeName).length === 0)
            throw new Error("Missing " + endpointRef + " type information in meta cache");

        return typeGroup.Types.filter(x => x.ServerTypeName === typeName)[0];
    }

    getResource(key, name, culture) {
        culture = culture || "";
        if (!key || !name) {
            return {
                Key: key,
                Name: name,
                DisplayName: name,
                ShortDisplayName: name,
                Description: name
            };
        }

        const subSet = this.cache.resources.filter(r => r.Key.toLowerCase() === key.toLowerCase() && r.Name.toLowerCase() === name.toLowerCase());
        const resultSet = subSet
            .filter(r => culture === r.Culture || (culture.indexOf(r.Culture) > -1) || r.Culture === '')
            .sort((a, b) => b.Culture.length - a.Culture.length);

        return (resultSet.length > 0)
            ? resultSet[0]
            : {
                Key: key,
                Name: name,
                DisplayName: name,
                ShortDisplayName: name,
                Description: name
            };
    }

    async send(type, query, data, contentType) {
        // if no query provided set to blank //
        query = query || '';
        // construct the promise //
        // if not return promise for client to handle //
        return new Promise((resolve, reject) => {
            // create the config that will be used by the ajax call //
            const ajaxConfig = {
                type: type,
                contentType: contentType || 'application/json',
                crossDomain: window.location.href.indexOf(this.apiRoot) > -1,
                url: this.apiRoot + query,
                success: resolve,
                error: reject,
                beforeSend: (xhr) => this.beforeSend(xhr, contentType),
                //timeout: this.timeout
            };

            // if we have been provided data, then add it to the call //
            if (data) {
                ajaxConfig.data = JSON.stringify(data);
            }

            $.ajax(ajaxConfig);
        });
    }

    async sendRaw(type, query, data) {
        // if no query provided set to blank //
        query = query || '';
        // construct the promise //
        // if not return promise for client to handle //
        return new Promise((resolve, reject) => {
            // create the config that will be used by the ajax call //
            const ajaxConfig = {
                type: type,
                contentType: "text/plain",
                crossDomain: window.location.href.indexOf(this.apiRoot) > -1,
                url: this.apiRoot + query,
                success: resolve,
                error: reject,
                beforeSend: (xhr) => this.beforeSend(xhr, "text/plain"),
                data: data
            };

            $.ajax(ajaxConfig);
        });
    }

    beforeSend(xhr, contentType) {
        xhr.setRequestHeader("Content-Type", contentType || "application/json;odata=minimalmetadata");
        xhr.setRequestHeader("Accept", contentType || "application/json;odata=minimalmetadata");

        if (this.token) {
            xhr.setRequestHeader("Authorization", "bearer " + this.token);
        }
    }

    async get(query) {
        return this.send("GET", query, null);
    }

    async add(query, model) {
        return this.send("POST", query, model);
    }

    async update(query, model) {
        return this.send("PUT", query, model);
    }

    async post(query, model) {
        return this.send("POST", query, model);
    }

    async put(query, model) {
        return this.send("PUT", query, model);
    }

    async destroy(query) {
        return this.send("DELETE", query, null);
    }

    success() {
        notification.success("Request Complete!");
    }
}

window.api = new Api({
    baseUrl: session.apiRoot,
    token: session.token
});
const html = {
    encode: function (value) { return $('<div />').text(value).html(); },
    decode: function (value) { return $('<div/>').html(value).text(); }
};

const notification = {
    popup: null,
    success: function (message) {
        if (notification.popup != null && notification.popup.show != null)
            notification.popup.show(message, "success");
        else
            console.log('Success', message);
    },
    info: function (message) {
        if (notification.popup != null && notification.popup.show != null)
            notification.popup.show(message, "info");
        else
            console.log('Info', message);
    },
    warning: function (message) {
        if (notification.popup != null && notification.popup.show != null)
            notification.popup.show(message, "warning");
        else
            console.log('Warning', message);
    },
    error: function (message) {
        if (notification.popup != null && notification.popup.show != null)
            notification.popup.show(message, "error");
        else
            console.log('Error', message);
    }
};

const type = {
    dateFormat: 'yyyy-MM-dd',
    moneyFormat: 'n',
    aggregateMoneyFormat: 'n0'
};

String.prototype.replaceAll = function (search, replacement) { return this.split(search).join(replacement); };

async function date() {
    let dateElement = document.getElementById('date');

    if (dateElement) {
        let today = new Date();
        dateElement.innerHTML = today.getDate() + ' ' + await api.getResource("Default", "month-" + (today.getMonth() + 1), session.culture).ShortDisplayName + ' ' + today.getFullYear();
    }
}

function time() {
    let timeElement = document.getElementById('time');

    let appendZero = function (i) {
        if (i < 10) { i = "0" + i; }  // add zero in front of numbers < 10
        return i;
    };

    if (timeElement) {
        let today = new Date();
        let h = today.getHours();
        let m = today.getMinutes();
        let s = today.getSeconds();
        m = appendZero(m);
        s = appendZero(s);
        timeElement.innerHTML = h + ":" + m + ":" + s;
        setTimeout(function () { time(); }, 500);
    }
}

function getQueryParameter(name) {
    let query = window.location.search.replace("?", "").split('&').map(p => { return { k: p.split('=')[0], v: p.split('=')[1] }; });
    let result = null;

    for (let i in query) {
        if (query[i].k === name) { result = query[i].v; }
    }

    return result !== null ? unescape(result) : result;
}

function setQueryParameter(name, value) {
    let query = window.location;

    if (query.search === '') {
        let result = window.location.href + '?' + name + '=' + value;
        window.location.href = result;
    } else {
        let queryStrings = query.search
            .replace('?', '')
            .split('&');

        let validStrings = queryStrings.filter(function (s) {
            return s.indexOf(name + '=') === -1;
        });

        let newSearch = '?' + validStrings.join('&') + (validStrings.length > 0 ? '&' : '') + [name, value].join('=');
        let currentUrl = window.location.href;
        let baseUrl = currentUrl.split('?')[0];
        window.location.href = baseUrl + newSearch;
    }
}


function removeQueryParameter(key, sourceURL) {
    let rtn = sourceURL.split("?")[0],
        param,
        params_arr = [],
        queryString = (sourceURL.indexOf("?") !== -1) ? sourceURL.split("?")[1] : "";

    if (queryString !== "") {
        params_arr = queryString.split("&");

        for (let i = params_arr.length - 1; i >= 0; i -= 1) {
            param = params_arr[i].split("=")[0];
            if (param === key) {
                params_arr.splice(i, 1);
            }
        }

        rtn = rtn + "?" + params_arr.join("&");
    }
    return rtn;
};


    async function loadComponent(container, componentName, callback) {
        let result = await api
            .get("Core/Component/Render()?AppId=" + session.app.Id + "&Name=" + componentName + "&culture=" + session.culture + "&theme=" + session.theme);

        try {
            $(container).append(result.value);

            if (callback) {
                callback(window[componentName]);
            }

            return window[componentName];
        } 
        catch (ex) {
            $(container).empty();
            $(container).append($("<h3>Component Loading Failed</h3><p>" + componentName + " could not be found or loaded because of the following exception:<br>" + ex.message + "</p>"));
            $(container).append($("<pre>" + ex.stack + "</pre>"));
        }
    }

function clone(object) {
    return JSON.parse(JSON.stringify(object));
}

function log(data, level) {

    if (data.responseText) {
        if (data.responseText.startsWith("{")) {
            var item = JSON.parse(data.responseText) || data.responseJson;

            if (item.Message) {
                log(item.Message, level);
            }

            if (item.message) {
                log(item.message, level);
            }

            if (item.error) {
                log(item.error, level);
            }
            else if (item.ErrorMessage) {
                log(item.ErrorMessage, "error");
            }
            else {
                log(data.responseText, level);
            }
        }
        else {
            log(data.responseText, level);
        }

    } else if(notification) {
        if (level === "log") {
            notification.info(data);
        }
        else if (notification[level]) {
            notification[level](data);
        }
    }

    if (console[level]) {
        console[level](data);
    }
}

async function error(e) {
    log(e, "error");
    if (typeof e == 'string') {
        if (e.indexOf("Access Denied!") !== -1) {
            notification.error(await api.getResource("Core", "AccessDenied", "").Description);
        }
        else {
            notification.error(await api.getResource("Core", "ServerRejectedAttemptedAction", "").Description);
        }
    } else if (typeof e == 'object') {
        if (e.hasOwnProperty("message")) {
            if (e.message.indexOf("Access Denied!") !== -1) {
                notification.error(await api.getResource("Core", "AccessDenied", "").Description);
            }
            else {
                notification.error(await api.getResource("Core", "ServerRejectedAttemptedAction", "").Description);
            }
        }
    }
}

function Guid() {
    return crypto.randomUUID();
};

function initContent() {
    // auto init components dropped on the page
    $.each($(".component"), function (i, c) {
        try {
            let name = $(c).attr("name");

            if (window[name]) {
                window[name].init(session.app, $(c));
            }
        }
        catch (ex) {
            log("Component loading error", "error");
            log(ex, "error");
        }
    });

    date();
    time();
}

$(async function() {
    if (session.culture && session.culture.split("-").length == 2) {
        kendo.culture(session.culture);
    }

    //setup notifcations 
    $(document.body).append("<div id='notif'></div>");
    notification.popup = $("#notif").kendoNotification({
        autoHideAfter: 5000,
        stacking: 'up',
        position: { bottom: 35, right: 10 },
        // these are not really needed as they basically put out the default
        // they are purely in place as a hook to say to me later "if you want to change notification rendering here's how".
        templates: [
            { type: "success", template: $("<div><div class='notification'><p>#= data.content #</p></div></div>").html() },
            { type: "info", template: $("<div><div class='notification'><p>#= data.content #</p></div></div>").html() },
            { type: "warning", template: $("<div><div class='notification'><p>#= data.content #</p></div></div>").html() },
            { type: "error", template: $("<div><div class='notification'><p>#= data.content #</p></div></div>").html() }
        ]
    }).data("kendoNotification");
});

function getMonthlyDateRange(start, end, format) {
    let months = [];
    let current = new Date(start);
    current.setUTCDate(1);

    while (current < end) {
        let thisMonth = new Date(current);
        let thisStart = new Date(thisMonth);
        let thisEnd = new Date(thisMonth);
        thisEnd.setUTCMonth(thisEnd.getUTCMonth() + 1);
        thisEnd.setUTCDate(-0);

        months.push({
            from: thisStart.toISOString().split('T')[0],
            to: thisEnd.toISOString().split('T')[0],
            formatted: kendo.toString(thisMonth, format)
        });

        current.setUTCMonth(current.getUTCMonth() + 1);
    }

    return months;
}
var form = {
    get: async function(id) {
        let formData = await api.get("Core/Form(" + id + ")");
        formData.render = form.render;
        return formData;
    },

    new: async function(appId) {
        let newForm = await api.get("Core/Form/NewForm()");
        newForm.render = form.render;
        newForm.AppId = appId;
        return newForm;
    },

    render: async function(element, id, callback) {
        $(element).empty();
        let result = await api.call("Core/Form(" + id + ")/Render()", this);
        $(element).append(result.value);
    },

    modelFor: function(theForm) {
        var rootMeta = null;

        for (var i in theForm.Meta) {
            if (theForm.Meta[i].Name === theForm.RootMetaItem) {
                rootMeta = theForm.Meta[i];
                break;
            }
        }

        return kendo.observable(form.buildObject(rootMeta, theForm.Meta));
    },

    buildObject: function(meta, metaCollection) {

        if (meta !== null) {
            var res = {};
            for (var i in meta.Properties) {
                var p = meta.Properties[i];
                switch (p.Type) {
                    case "text": res[p.Name] = ""; break;
                    case "string": res[p.Name] = ""; break;
                    case "email": res[p.Name] = ""; break;
                    case "password": res[p.Name] = ""; break;
                    case "date": res[p.Name] = new Date(); break;
                    case "number": res[p.Name] = 0; break;
                    case "checkbox": res[p.Name] = false; break;
                    case "file": res[p.Name] = null; break;
                    case "array": res[p.Name] = []; break;
                    case "bool": res[p.Name] = false; break;
                    case "int": res[p.Name] = 0; break;
                    default:
                        var subMeta = null;
                        if (metaCollection) {
                            for (var j in metaCollection) {
                                if (metaCollection[j].Name === p.Type) {
                                    subMeta = metaCollection[j];
                                    break;
                                }
                            }

                            res[p.Name] = form.buildObject(metaCollection, subMeta);
                        }
                        break;
                }
            }
            return res;
        }

        return null;
    },

    getMetaForType: function(typeName, formDef) {
        var result = null;
        for (var i in formDef.Meta) {
            if (formDef.Meta[i].Name === typeName) {
                result = formDef.Meta[i];
                break;
            }
        }

        return result;
    }
};

const model = {
    clone: (obj) => JSON.parse(JSON.stringify(obj)),

    prepareItem: function (item, meta) {
        item.type = meta.Category + "/" + meta.ServerTypeName;

        meta.Properties.forEach(p => {
            if (p.Type === 'date' || p.Type === 'time') {
                item[p.Name] = item[p.Name] !== null ? new Date(item[p.Name]) : item[p.Name];
            }
        });

        // add functions for working with this object
        for (let f in model.item) {
            if (f !== "createInstance") { item[f] = model.item[f]; }
        }

        return item;
    },

    prepareItemFromModelInfo: function (item, model) {
        item.type = 'dynamic';
        item.context = '';
        $.each(model.fields, function (i, f) {
            if (f.type === 'date' || f.type === 'time') {
                item[f.Name] = item[f.Name] !== null
                    ? new Date(item[f.Name])
                    : item[f.Name];
            }
        });
    },

    prepConfigForGrid: function (config, callback) {
        let ds = model.getDatasource(config.sourceInitParams);

        if (callback) {
            config.dataSource = ds;
            callback(config);
        }

        return config;
    },

    createforListOf: function (type) {
        return kendo.observable({
            type: type,
            context: context,
            items: model.getDatasource(type)
        });
    },

    createFor: async function (type, id) {
        let meta = api.getType(type);
        let result = new kendo.observable(await model.item.createInstance(type));

        result.fetch = async function (callback) {
            if (id !== 'new') {
                let res = await api.get(type + "(" + id + ")");
                model.prepareItem(res, meta);
                result.item = res;

                if (typeof callback !== 'undefined') {
                    callback();
                }
            }
            else if (typeof callback !== 'undefined') {
                callback();
            }
        };

        return result;
    },

    getDatasource: function (context) {
        let result = null;
        let modelInfo = context.model
            ? context.model
            : null;

        if (modelInfo === null) {
            var m = model.fieldsFor(context.endpoint);
            result = model.build(context, {fields: m});
        }
        else {
            result = model.build(context, modelInfo);
        }
        return result;
    },

    odata: {
        joinFilters: function(url) {
            let query = url.split("?");
            let params = query[1].split("&");

            if (params.filter(p => p.indexOf("$filter=") > -1).length > 1) {

                let filter = "$filter=" + params
                    .filter(p => p.indexOf("$filter=") > -1).map(f => f.replace("$filter=", ""))
                    .join(" AND ");

                params = params.filter(p => p.indexOf("$filter=") === -1);
                params.push(filter);
                query[1] = params.join("&");
            }

            return query[0] + "?" + query[1];
        }
    },

    build: function (context, m) {
        if (!context.base) { context.base = session.apiRoot; }
        if (!context.serverOptions) {
            context.serverOptions = {
                filtering: true,
                sorting: true,
                paging: true,
                grouping: false
            };
        }

        if (!context.pageSize) {
            context.pageSize = session.app.Config.DefaultPageSize || 50;
        }

        // construct the root url to be used by the datasource
        let baseUrl = context.base + context.endpoint;

        let metaType = context.dataType !== undefined
            ? context.dataType
            : context.endpoint;

        let schemaModel = m;
        let typeInfo = api.getType(metaType);

        let dateFieldNames = typeInfo.Properties
            .filter(p => p.Type === "date")
            .map(c => c.Name);

        let result = null;

        let cfg = {
            serverFiltering: context.serverOptions.filtering,
            serverSorting: context.serverOptions.sorting,
            serverGrouping: context.serverOptions.grouping,
            serverPaging: context.serverOptions.paging,
            pageSize: context.pageSize,
            group: context.group ? context.group : [],
            filter: context.filter ? context.filter : { logic: 'and', filters: [] },
            sort: context.sort ? context.sort : [],
            type: 'odata-v4',

            change: context.onChange
                ? context.onChange
                : function (e) { /* to ensure don't try calling undefined */ },

            transport: context.transport
                ? context.transport
                : {
                    read: {
                        url: function (data) {
                            let result = baseUrl;

                            if (typeof context.odataAppend !== 'undefined') {
                                result += context.odataAppend;
                            }

                            return result;
                        },
                        dataType: 'json',
                        beforeSend: function (xhr, request) {
                            api.beforeSend(xhr);
                            request.url = request.url.replaceAll("%24", "$");
                            if (request.url.indexOf("?") > 0) {
                                request.url = model.odata.joinFilters(request.url);
                            }
                            result.lastGet = request;
                            if (context.enableExport) {
                                result.setExportUrls();
                            }
                        }
                    },

                    update: {
                        url: function (data) {
                            return baseUrl + "(" + result.getIdForUrl(data) + ")";
                        },

                        dataType: 'json',
                        beforeSend: api.beforeSend
                    },
                    destroy: {
                        url: function (data) {
                            return baseUrl + "(" + result.getIdForUrl(data) + ")";
                        },

                        dataType: 'json',
                        beforeSend: api.beforeSend
                    },
                    create: {
                        url: baseUrl,
                        dataType: 'json',
                        beforeSend: api.beforeSend
                    },
                    parameterMap: function (data, operation) {
                        let dataCopy = JSON.parse(JSON.stringify(data));

                        if (dataCopy.filter && dataCopy.filter.filters && dataCopy.filter.filters.length > 0) {
                            for (const element of dataCopy.filter.filters) {
                                let entry = element

                                if (dateFieldNames.indexOf(entry.field) !== -1) {
                                    entry.value = new Date(entry.value);
                                }
                            }
                        }

                        if (context.customFields && dataCopy.filter && dataCopy.filter.filters) {
                            let fieldsToIgnore = context.customFields
                                .map(c => c.templateField);

                            dataCopy.filter.filters = data.filter
                                .filters
                                .filter(c => fieldsToIgnore.indexOf(c.field) === -1);
                        }

                        let filterApplied = kendo.data.transports['odata-v4']
                            .parameterMap
                            .call(result, dataCopy, operation);

                        if (operation === "update") {
                            delete dataCopy.type;  // this will annoy the server, so lets remove it before we send.
                            delete dataCopy.source;
                            delete dataCopy.context;
                            return JSON.stringify(dataCopy);
                        }

                        if (operation === "read" && context.customFields) {
                            if (context.customFields) {
                                context.customFields
                                    .filter(cf => data.filter && data.filter.filters && data.filter.filters.filter(c => c.field == cf.templateField).length > 0)//where a filter has been applied
                                    .forEach(cf => {
                                        if (cf.type === "array") {

                                            let filterEntry = data.filter.filters
                                                .filter(c => c.field == cf.templateField)[0];

                                            let operatorString = "";

                                            if (filterEntry.operator === "contains") {
                                                operatorString = "contains(" + cf.field + ", '" + filterEntry.value + "')";
                                            } else {
                                                operatorString = " " + cf.field + " " + filterEntry.operator + " '" + filterEntry.value + "'";
                                            }

                                            let filterString = cf.arrayField + "/any(c: " + operatorString + (cf.fieldAppend ? cf.fieldAppend + ")" : ")");

                                            if (filterApplied.hasOwnProperty("$filter")) {
                                                filterApplied["$filter"] = "(" + filterApplied["$filter"] + ") and " + filterString;
                                            } else {
                                                filterApplied["$filter"] = filterString;
                                            }
                                        } else {
                                            let operatorString = "";

                                            if (filterEntry.operator === "contains") {
                                                operatorString = "contains(" + cf.field + ", '" + filterEntry.value + "')";
                                            } else {
                                                operatorString = " " + cf.field + " " + filterEntry.operator + " '" + filterEntry.value + "'";
                                            }

                                            let filterString = operatorString + (cf.fieldAppend ? cf.fieldAppend + ")" : ")");

                                            if (filterApplied.hasOwnProperty("$filter")) {
                                                filterApplied["$filter"] = "(" + filterApplied["$filter"] + ") and " + filterString;
                                            } else {
                                                filterApplied["$filter"] = filterString;
                                            }
                                        }
                                    });
                            }
                            if (context.postCustomServerFiltering) {
                                context.postCustomServerFiltering(data, filterApplied);
                            }
                        }

                        return filterApplied;
                    }
                },

            schema: {
                model: schemaModel,
                data: function (data) {
                    return data.value;
                },

                total: function (data) {
                    return data['@odata.count'];
                },

                parse: function (data) {
                    if ($.isArray(data.value)) {  // if we got an array of items prep each item
                        $.each(data.value, function (idx, item) {
                            model.prepareItem(item, typeInfo);

                            if (context.computeFields) {
                                context.computeFields(item);
                            }
                        });
                    }
                    else {
                        model.prepareItem(data, typeInfo);
                        if (context.computeFields) {
                            context.computeFields(item);
                        }
                    }

                    return data;
                }
            },

            error: function (e) {
                log({ event: e, source: this }, "error");
            },

            save: function () {
                this.sync();
                setTimeout(this.read, 1000);
            }
        };

        result = new kendo.data.DataSource(cfg);
        result.baseUrl = baseUrl;
        result.odataAppend = context.odataAppend;
        result.meta = typeInfo;

        result.getIdForUrl = function (data) {
            let meta = result.meta.Properties.filter(function (p) {
                return p.Template === "key";
            })[0];

            return meta.Type === "string"
                ? "'" + data[meta.Name] + "'"
                : data[meta.Name];
        };

        return result;
    },

    item: {
        createFor: function (metaArray, rootObjectType, maxDepth, currentDepth) {
            maxDepth = maxDepth || 1;
            currentDepth = currentDepth || 0;

            let meta = metaArray
                .filter(function (m) { return m.Name === rootObjectType; })[0];

            if (meta) {
                let result = {};
                for (let i in meta.Properties) {
                    let p = meta.Properties[i];

                    let propMeta = metaArray
                        .filter(function (m) { return m.ServerType === p.ServerType; })[0];

                    if (propMeta && currentDepth + 1 <= maxDepth) {
                        result[p.Name] = buildModel(metaArray, p.Name, maxDepth, currentDepth + 1);
                    }
                    else {
                        if (p.Type === 'string' || p.Type === 'password' || p.Type === 'email') { result[p.Name] = ''; }

                        if (p.Type !== 'object' && p.Type !== 'array') {
                            if (p.Type === 'date') {
                                result[p.Name] = new Date();
                            }

                            if (p.Type === 'number' || p.Type === 'key') {
                                result[p.Name] = 0;
                            }

                            if (p.Type === 'guid') {
                                result[p.Name] = '00000000-0000-0000-0000-000000000000';
                            }

                            if (p.Type === 'bool') {
                                result[p.Name] = false;
                            }
                        }
                    }
                }

                return result;
            }

            return {};
        },

        createInstance: async function (endpoint, callback) {
            let meta = await api.getType(endpoint);
            let result = { type: endpoint };

            $.each(meta.Properties, function (i, p) {
                if (p.Type !== 'object' && p.Type !== 'array') {
                    if (p.Name === 'Id') {
                        result[p.Name] = 'new';
                    } else {
                        result[p.Name] = '';
                        if (p.Type === 'date') {
                            result[p.Name] = new Date();
                        }
                        if (p.Type === 'number' || p.Type === 'key') {
                            result[p.Name] = 0;
                        }
                        if (p.Type === 'guid') {
                            result[p.Name] = model.newGuid;
                        }
                        if (p.Type === 'bool') {
                            result[p.Name] = false;
                        }
                    }
                }
            });

            model.prepareItem(result, meta);

            if (callback) {
                callback(result);
            }

            return result;
        },

        prepForPush: function () {
            // kendo will remove its gubbins, I then simply have to parse the 
            //  resulting string back in to an object to get a clean version
            var result = JSON.parse(kendo.stringify(this));

            // added by kendo / knockout during binding
            delete result.guid;

            // added by me to track the object type
            delete result.type;

            //add by me to track the apiContext that this object came from
            delete result.context;

            // returns a dto that matches the server definition of the object

            // delete auditing information
            delete result.CreatedOn;
            delete result.LastUpdated;

            return result;
        },

        save: async function (e) {
            let obj = this;
            let valid = true;

            if (e) {
                e.preventDefault();
                let form = $(e.currentTarget).closest("form");

                if (form.length > 0) {
                    try {
                        valid = form.valid();
                    }
                    catch (ex) {
                        error(ex);
                    }
                }
            }

            let url = this.type;
            let method = "update";

            log({ Operation: "Save", item: this }, "debug");
            if (valid) {
                let meta = await api.getType(this.type);
                let idProp = meta.Properties.filter(function (p) {
                    return p.Name === "Id" || p.Template === "key";
                })[0];

                let key = idProp.Type === "string"
                    ? "('" + obj[idProp.Name] + "')"
                    : "(" + obj[idProp.Name] + ")";

                // save existing entry to system
                if (obj.Id !== 'new') {
                    url += key;
                }
                // push new entry to system
                else if (idProp.type !== "string") {
                    delete obj.Id;
                    method = "add";
                }

                let payload = obj.prepForPush();
                return await api[method](url, payload);
            }
        },

        destroy: async function (e) {
            e.preventDefault();
            let retiredObject = this;
            let meta = await api.getType(this.type);

            let idProp = meta.Properties.filter(function (p) {
                return p.Template === "key";
            })[0];

            let key = idProp.Type === "string"
                ? "('" + retiredObject[idProp.Name] + "')"
                : "(" + retiredObject[idProp.Name] + ")";

            return await api.destroy(retiredObject.type + key);
        }
    },

    fieldsFor: function (endpoint) {
        let typeInfo = api.getType(endpoint);
        let result = {};

        typeInfo.Properties
            .forEach(p => result[p.Name] = { field: p.Name, type: p.Type });

        return result;
    },

    columnsFor: function (endpoint, extraCols, removeCols) {
        let typeInfo = api.getType(endpoint);
        let result = new Array();
        let scalarProperties = typeInfo.Properties.filter(p => p.IsValueType);

        for (let p in scalarProperties) {
            let prop = scalarProperties[p];
            prop.type = prop.Type;
            prop.field = prop.Name;
            prop.title = prop.ShortDisplayName;
            prop.editable = !prop.IsReadOnly;

            if (prop.Type === "date") {
                prop.format = "{0:" + type.dateFormat + "}";
                prop.filterable = {
                    ui: function (element) {
                        element.kendoDatePicker({ format: type.dateFormat })
                    }
                };

                prop.attributes = { style: "text-align: right;" };
            }

            if (prop.ServerTypeName === "Decimal") {
                prop.format = "{0:" + type.moneyFormat + "}";
            }

            if (prop.type === "number") {
                prop.attributes = { style: "text-align: right;" };
            }

            result.push(prop);
        }

        if (typeof extraCols !== 'undefined') {
            for (let i in extraCols)
                result.push(extraCols[i]);
        }

        if (typeof removeCols !== 'undefined') {
            for (let j in removeCols) {
                for (let c in result) {
                    if (result[c].field === removeCols[j] || result[c].name === removeCols[j]) {
                        result.splice(c, 1);
                    }
                }
            }
        }

        return result;
    },

    propertiesFor: function (endpoint, fields, extraFields) {
        let typeInfo = api.getType(endpoint);

        let scalarProperties = typeInfo.Properties
            .filter(p => p.IsValueType);

        return scalarProperties.filter(prop => (fields)
            ? (fields.indexOf(prop.Name) !== -1)
            : true).map((prop) => {
                let p = {
                    type: prop.Type,
                    field: prop.Name,
                    title: prop.ShortDisplayName,
                    description: prop.Description,
                    name: prop.DisplayName,
                    editable: !prop.IsReadOnly
                };

                if (prop.Type === "date") {
                    p.format = "{0:" + type.dateFormat + "}";
                    p.filterable = {
                        ui: (element) => element.kendoDatePicker({ format: type.dateFormat })
                    };

                    p.attributes = { style: "text-align: right;" };
                }

                if (prop.ServerTypeName === "Decimal") {
                    p.format = "{0:" + type.moneyFormat + "}";
                }

                if (prop.type === "number") {
                    p.attributes = { style: "text-align: right;" };
                }

                return p;
            }).concat(extraFields ? extraFields : []);
    }
};
class Close extends Collidable {
    constructor(parent) {
        super('x', 0, 0, parent.h - 2, parent.h - 2, 0);
        this.type = 'close';
        this.parent = parent;
        this.flow = this.parent.flow;
        this.text = 'x';
        this.col = window.flowTheme.colours.secondary;
        this.sel = '#f00';
        this.updatePosition();
    }

    updatePosition() {
        this.x = this.parent.x + (this.parent.w - this.parent.h) + 1;
        this.y = this.parent.y + 1;
    }

    draw(ctx) {
        draw.rect(ctx, this.x, this.y, this.w, this.h, this.col, this.sel);
        draw.text(ctx, this.x + 10, this.y + 15, this.text, '#fff');
    }

    mouseup(e) {
        this.flow.Activities = this.flow.Activities.filter(a => !Object.is(a, this.parent.activity));
        this.flow.Links = this.flow.Links.filter(l => l.model.Source !== this.parent.activity.model.Ref);
        this.flow.Links = this.flow.Links.filter(l => l.model.Destination !== this.parent.activity.model.Ref);
    }
}
class Handle extends Collidable {
    constructor(parent, col, textCol) {
        super(parent.model.Ref, parent.x, parent.y, parent.w - 1, handleHeight, 0);

        if (this.text.length > 25) { this.text = this.text.substring(0, 25) + "..."; }

        this.type = 'handle';
        this.col = col;
        this.textCol = textCol || $('h2').css('color');
        this.parent = parent;
        this.activity = parent;
        this.flow = this.parent.flow;
        this.objects = [new Close(this)]; //, new EditProps(this) ];
    }

    draw(ctx) {
        draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#CCC", 1);
        draw.rect(ctx, this.x, this.y, this.w, this.h, this.col);
        draw.disableShadows(ctx);

        draw.text(ctx, this.x + 8, this.y + 17, this.text, this.textCol);
        this.objects.forEach(o => o.draw(ctx));
    }

    mousedown(e) {
        this.moving = true;
        this.start = mouseposition;
    }

    mouseup(e) {
        this.moving = false;
    }

    updatePosition() {
        this.x = this.parent.x;
        this.y = this.parent.y;
        this.objects.forEach(o => o.updatePosition());
    }

    move(e) {
        if (this.moving) {
            this.parent.setPosition(
                this.x + (mouseposition.x - this.start.x),
                this.y + (mouseposition.y - this.start.y)
            );
            this.start = mouseposition;
        }
    }
}
class Link extends Collidable {
    constructor(model, flow) {
        super('', 0, 0, 0, 0, 5);
        this.model = model;
        this.flow = flow;
    }

    draw(ctx) {
        let source = this.flow.Activities.filter(a => a.model.Ref === this.model.Source)[0];
        let destination = this.flow.Activities.filter(a => a.model.Ref === this.model.Destination)[0];

        if (source && destination) {
            if (source.out && destination.in) {
                this.x = source.out.x + ((destination.in.x - source.out.x) / 2);
                this.y = source.out.y + ((destination.in.y - source.out.y) / 2);

                draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#111");
                draw.line(ctx, source.out.x, source.out.y, destination.in.x, destination.in.y, '#37e83a');
                draw.circle(ctx, this.x, this.y, this.r, '#37e83a', '#00ff04');
                draw.disableShadows(ctx);
            }
        }
    }

    mouseup(e) {
        let that = this;

        let dialog = $('<div><div name="content"></div></div>')
            .appendTo(document.body);

        let d = dialog.kendoWindow({
            title: 'Expression Editor',
            modal: true,
            visible: false,
            resizable: false,
            width: '1000px',
            close: function() { this.destroy(); }
        }).data("kendoWindow");

        d.wrapper.css({ top: 140, left: '20%' });
        d.open();

        let container = $("[name=content]", $(d.element));

        loadComponent(container, "ExpressionBuilder", function(c) {
            let builderParams = {
                expression: that.model.Expression,
                args: {
                    destination: that.flow.Activities
                        .filter(a => a.model.Ref === that.model.Destination)[0]
                        .meta.ServerTypeName,
                    source: that.flow.Activities
                        .filter(a => a.model.Ref === that.model.Source)[0]
                        .meta.ServerTypeName,
                    variables: "IDictionary<String,Object>",
                    flow: "Flow"
                }
            };

            ExpressionBuilder.init(null, container, builderParams, function(expression) {
                that.model.Expression = expression;
                d.close();
            });
        });
    }
}
class Action extends Collidable {
    constructor(parent, text, offsetX, offsetY, height, width, col, onClick) {
        super(text, parent.x + offsetX, parent.y + offsetY, height, width, 0);
        this.type = 'Action';
        this.parent = parent;
        this.flow = this.parent.flow;
        this.text = text;
        this.col = col;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.updatePosition();
        this.click = onClick;
    }

    updatePosition() {
        this.x = this.parent.x + this.offsetX;
        this.y = this.parent.y + this.offsetY;
    }

    mouseup(e) {
        this.click(e);
    }

    draw(ctx) {
        draw.text(ctx, this.x + 10, this.y + 15, this.text, this.col);
    }
}
class Activity extends Collidable {
    constructor(meta, flow, model) {
        super(meta.DisplayName.replace("Activity", ""), 0, 0, activityWidth, 100, 0);

        this.text = this.text.replace(/([A-Z])/g, " $1").replace("D M S", "DMS");

        if (this.text.length > 20) {
            this.text = this.text.substring(0, 18) + "...";
        }

        if (meta && flow) {
            this.flow = flow;
            this.meta = meta;
            this.model = model;
            this.handle = new Handle(this, window.flowTheme.colours.primary, '#fff');
            meta.Properties.map(p => this.model[p.Name] = this.model[p.Name] || null);
            this.objects = [];
            this.objects.push(this.handle);

            if (this.meta.name !== "Start") {
                this.in = new Connector(this, 'input');
                this.objects.push(this.in);
            }

            this.out = new Connector(this, 'output');
            this.objects.push(this.out);
            this.objects.push(new Action(this, 'Edit', 50, 65, 35, 25, window.flowTheme.colours.primary, this.edit));
            return this;
        }
    }

    addInstanceData(state, log) {
        this.InstanceState = state;
        this.InstanceLog = log;
        this.objects.push(new Action(this, 'State', 80, 65, 35, 25, window.flowTheme.colours.secondary, this.showExecutionState));
        this.objects.push(new Action(this, 'Log', 110, 65, 35, 25, window.flowTheme.colours.secondary, this.showExecutionLog));
    }

    removeInstanceData() {
        delete this.InstanceState;
        delete this.InstanceLog;
        this.objects = this.objects.filter(o => o.type !== 'State' && o.type !== 'Log');
    }

    setPosition(x, y) {
        this.x = x;
        this.y = y;
        this.objects.forEach(o => o.updatePosition());
    }

    // overide for drawing activities
    draw(ctx) {
        draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#555");

        if (!this.InstanceState) {
            draw.rect(ctx, this.x, this.y + handleHeight, this.w, this.h - handleHeight + 1, '#fff');
        }
        else {
            switch (this.InstanceState.State) {
                case 0:
                case 1:
                case 4:
                    draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#ccc");
                    draw.rect(ctx, this.x, this.y + handleHeight, this.w, this.h - handleHeight + 1, "#eee");
                    break;
                case 2:
                    if (this.InstanceLog.filter(l => l.Level === "Warning").length === 0) {
                        draw.rect(ctx, this.x, this.y + handleHeight, this.w, this.h - handleHeight + 1, "#ded");
                    } else {
                        draw.rect(ctx, this.x, this.y + handleHeight, this.w, this.h - handleHeight + 1, window.flowTheme.colours.primary);
                    }
                    break;
                case 3:
                    draw.rect(ctx, this.x, this.y + handleHeight, this.w, this.h - handleHeight + 1, "#edd");
                    break;
            }
        }

        draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#111");
        ctx.font = "30px WebComponentsIcons";
        ctx.fillStyle = window.flowTheme.colours.primary;
        ctx.fillText(this.getIcon(this.meta.category), this.x + 20, this.y + 73);

        draw.disableShadows(ctx);

        this.objects.forEach(o => o.draw(ctx));
        draw.text(ctx, this.x + 60, this.y + 62, this.text);
    }

    getIcon(forName) {
        switch (forName) {
            case "ApiActivity":
                return "\ue672";
            case "DMSActivity":
                return "\ue900";
            case "SftpActivity":
                return "\ue904";
            case "TransactionActivity":
                return "\ue694";
            case "LogActivity":
                return "\ue64a";
            case "TransformationActivity":
                return "\ue518"
            case "FlowControlActivity":
                return "\ue144";
            case "TemplatingActivity":
                return "\ue647";
            default:
                return "\ue13a";
        }
    }

    activeConnector() {
        if (this.in && this.in.active) { return this.in; }
        if (this.out && this.out.active) { return this.out; }
        return null;
    }

    edit(e) {
        var that = this.parent;
        var dialog = $('<div><div name="content"></div></div>').appendTo(document.body);
        var d = dialog.kendoWindow({
            title: 'Edit Acitvity',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: false,
            width: '820px'
        }).data("kendoWindow");
        d.wrapper.css({ top: 140, left: '20%' });
        d.open();

        loadComponent($("[name=content]", dialog), "ActivityEditor", function(c) {
            ActivityEditor.init(null, dialog, that, function(ac) { that.handle.text = ac.model.Ref; d.close(); });
        });
    }

    showExecutionState(e) {
        var dialog = $('<div class="stateDialog"><pre class="state" style="padding: 10px; width: 625px; height: 500px; overflow:auto;">' + JSON.stringify(this.parent.InstanceState, null, "\t") + '</pre></div>').appendTo(document.body);
        var d = dialog.kendoWindow({
            title: `Activity ${this.parent.model.Ref} :: Execution State`,
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: false,
            width: '650px'
        }).data("kendoWindow");
        d.wrapper.css({ top: 140, left: '20%' });
        d.open();
    }

    showExecutionLog(e) {
        var dialog = $($("script[name=executionLog]").html()).appendTo(document.body);
        var d = dialog.kendoWindow({
            title: 'Activity ' + this.parent.model.Ref + ' :: Execution Log',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: false,
            width: '810px'
        }).data("kendoWindow");
        d.wrapper.css({ top: 140, left: '20%' });
        d.open();

        var messages = this.parent.InstanceLog.map(function(msg) {
            var msgStamp = new Date(msg.Timestamp);
            var time = `${msgStamp.getHours()}:${msgStamp.getMinutes()}:${msgStamp.getSeconds()}`;
            return $(`<div class='message ${msg.Level}'><span class='time'>${time}</span><pre class='message'>${html.encode(msg.Message)}</pre></div>`);
        });

        $(".flowConsole", dialog).append(messages);
    }
}
/// <reference path="workflowdesigner.js" />
class Connector extends Collidable {
    constructor(parent, type) {
        super('', 0, 0, 0, 0, nodeSize);
        this.parent = parent;
        this.type = type;
        this.col = window.flowTheme.colours.primary;
        this.sel = window.flowTheme.colours.secondary;
        this.updatePosition();
    }

    mousedown() {
        this.active = true;

        window.addEventListener('keydown', this.listenForEsc);
    }

    removeLink(otherConnector) {
        let from = this.type === 'output' ? this : otherConnector;
        let to = this.type === 'output' ? otherConnector : this;

        if (otherConnector) {
            var fromRef = from.parent.model.Ref;
            var toRef = to.parent.model.Ref;

            var removal = this.parent.flow.Links
                .filter(l => l.model.Source === fromRef && l.model.Destination === toRef)[0];

            this.parent.flow.Links = this.parent.flow.Links.filter(i => i !== removal);
        } else if(this.type == 'input') {
            this.parent.flow.Links = this.parent.flow.Links
                .filter(l => l.model.Destination !== to.parent.model.Ref);
        }
    }

    addLink(otherConnector) {
        if (otherConnector && this.type !== otherConnector) {
            let from = this.type === 'output' ? this : otherConnector;
            let to = this.type === 'output' ? otherConnector : this;

            if (from && from.type === 'output') {
                let newLink = {
                    "Source": from.parent.model.Ref,
                    "Destination": to.parent.model.Ref,
                    "Expression": ""
                };

                this.parent.flow.Links.push(new Link(newLink, this.parent.flow));
            }
        }
    }

    mouseup(e, editor, otherConnector) {
        var node1 = this.parent.model.Ref;
        var node2 = otherConnector ? otherConnector.parent.model.Ref : null;

        if (node2 == null) {
            this.parent.flow.Links = this.type == 'output'
                ? this.parent.flow.Links.filter(l => l.model.Source != node1)
                : this.parent.flow.Links.filter(l => l.model.Destination != node1);
        } else {
            var existingLinks = this.parent.flow.Links
                .filter(l => {
                    return (l.model.Source === node1 && ((l.model.Destination === node2) || !node2)) ||
                        (((l.model.Source === node2) || !node2) && l.model.Destination === node1)
                });

            if (existingLinks.length > 0)
                this.removeLink(otherConnector);
            else
                this.addLink(otherConnector);
        }

        window.removeEventListener('keydown', this.listenForEsc);
        this.active = false;
    }

    updatePosition() {
        if (this.type === "input")
            this.x = this.parent.x + 2;

        if (this.type === "output")
            this.x = this.parent.x + this.parent.w - 2;

        this.y = this.parent.y + (this.parent.h / 2) + (handleHeight / 2);
    }

    draw(ctx) {
        if (this.type == "output")
            draw.semiCircle(ctx, this.x, this.y, this.r, Math.PI * 0.5, Math.PI * 1.5, this.sel, this.col);
        else
            draw.semiCircle(ctx, this.x, this.y, this.r, Math.PI * 1.5, Math.PI * 2.5, this.sel, this.col);

        if (this.active)
            draw.line(ctx, this.x, this.y, mouseposition.x, mouseposition.y, window.flowTheme.colours.secondary);
    }

    listenForEsc(e) {
        if (e.keyCode == 27) { // 27 = esc key.
            this.active = false;
            window.removeEventListener('keydown', this.listenForEsc);
        }
    }
}
const activityWidth = 200;
const handleHeight = 25;
const nodeSize = 16;

class Flow {
    constructor(flow) {
        if (!flow.DefinitionJson) {
            flow.DefinitionJson = JSON.stringify({
                RequiredRoles: null,
                Activities: [{
                    "$type": "cCoder.Core.Objects.Workflow.Activities.Start, cCoder.Core.Objects",
                    "AuthToken": null,
                    "Data": null,
                    "Ref": "Start",
                    "State": 0
                }],
                Links: [],
                Name: flow.Name
            });
        }

        this.model = flow;
        this.Config = flow.ConfigJson ? JSON.parse(flow.ConfigJson) : {};
        this.Definition = JSON.parse(flow.DefinitionJson);
        this.stepTypes = window.knownTypes.filter(ctx => ctx.Name === "Workflow")[0];
        this.Activities = this.Definition.Activities.map(a => {
            try {
                var activityType = a['$type'].split('[');
                var meta = this.stepTypes.Types.filter(f => f.ServerType.indexOf(activityType[0]) === 0);
                if (meta.length === 0) {
                    var activityNamespaces = activityType[0].split(",")[0].split("`")[0].split(".");
                    var activityName = activityNamespaces[activityNamespaces.length - 1];
                    var matchedMeta = this.stepTypes.Types.filter(r => {
                        let namespaces = r.ServerType.split(",")[0].split("`")[0].split(".");
                        let workflowActivityName = namespaces[namespaces.length - 1].toLowerCase();
                        return workflowActivityName === activityName.toLowerCase();
                    })[0];
                    meta = matchedMeta;
                    var hasTypeArguments = a['$type'].indexOf("[") !== -1;

                    a['$type'] = (hasTypeArguments) ? matchedMeta.ServerType + "[" + activityType[activityType.length - 1] : matchedMeta.ServerType;
                } else {
                    meta = meta[0];
                }
                return new Activity(meta, this, a);
            }
            catch (ex) {
                notification.error("An activity could not be constructed, removed.");
                error(ex);
                return null;
            }
        }).filter(a => a);
        this.Links = this.Definition.Links.map(l => new Link(l, this));
        return this;
    }

    addActivity(meta, pos) {
        var nameParts = meta.Name.split("`");
        var typesRequired = nameParts.length > 1 ? parseInt(nameParts[1]) : 0;
        var that = this;

        var next = function (m, type) {
            if (m && type) {
                let serverType = m.ServerType;
                let start = serverType.indexOf('[');
                let end = serverType.lastIndexOf(']');

                serverType = `${serverType.substring(0, start)}[${type}]${serverType.substring(end + 1)}`;
                m.ServerType = serverType;
            }

            var activity = new Activity(m, that, { "$type": m.ServerType, Ref: Guid() });
            activity.setPosition(pos.x, pos.y);
            that.Activities.push(activity);
        };

        if (typesRequired > 0) {
            this.getActivityTypes(meta, typesRequired, next);
        } else {
            next(meta);
        }
    }

    getActivityTypes(type, count, callback) {
        var dialog = $('<div></div>').appendTo(document.body);
        var d = dialog.kendoWindow({
            title: 'Pick Activity Type',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: true,
            width: '50%'
        }).data("kendoWindow");

        d.content(
            `<ul class="fieldList">
                <li><label>Types</label><div name="typeLists"></div></li>
                <li><label></label><div><button class="btn" name="submit">Submit</button></div></li>
            </ul>`
        );

        var ds = {
            data: [].concat.apply([], knownTypes
                .filter(t => t.Name !== "Workflow")
                .map(t => t.Types)
            ),
            group: { field: "Category" }
        };


        for (var i = 0; i < count; i++) {
            $(`<input class="type-isArray" name="isArray${i}" type="checkbox">Array<input class="type-picker" name="type${i}" >`)
                .appendTo($('[name=typeLists]', d.element));

            $(`input[name=type${i}]`, d.element)
                .kendoComboBox({
                    dataTextField: "ServerTypeName",
                    dataValueField: "ServerType",
                    height: 400,
                    dataSource: ds
                });
        }

        $('[name=submit]', d.element).on('click', {
            root: $('[name=typeLists]', d.element),
            dialog: d,
            count: count,
            callback: function(typestring) {
                d.close();
                callback(type, typestring);
            }
        }, this.confirmTypes.bind(this));

        $('.k-combobox', d.element).css({ 'width': 'auto', 'display': 'block', 'margin': '4px 0' });
        d.wrapper.css({ top: 140, left: '20%' });
        d.open();
    }

    confirmTypes(e) {
        var types = [];
        var index = 0;

        function asArrayType(meta, asArray) {
            if (asArray) {
                meta.Name = meta.DisplayName + " Array";
                meta.Description = meta.Description + " Array";
                meta.ServerType = meta.ServerType.replace(",", "[],");
            }
            return meta;
        }

        function next(type) {
            types.push('[' + type.ServerType + ']');

            if (types.length === e.data.count)
                if (e.data.callback)
                    e.data.callback(types.join(','));

            if (index < e.data.count)
                getType();
        }

        function getType() {
            var type = $('[name=type' + index + ']').data('kendoComboBox').dataItem();
            var asArray = $('[name=isArray' + index + ']:checked').length;

            if (type) {
                index++;
                next(asArrayType(type, asArray));
            }
        }

        getType();
    }

    value() {
        this.Definition.Activities = this.Activities.map(a => a.model);
        this.Definition.Links = this.Links.map(l => l.model);
        this.model.DefinitionJson = JSON.stringify(this.Definition);
        return this.model;
    }

    save() {
        var flow = this.value();

        flow.DefinitionJson = JSON.parse(flow.DefinitionJson);
        flow.DefinitionJson.Name = "";
        flow.DefinitionJson = JSON.stringify(flow.DefinitionJson);

        api.update('Core/FlowDefinition(' + flow.Id + ')', flow)
            .then(() => notification.success('Flow Saved Succesfully'))
            .catch(error);
    }

    run(save=true) {
        var that = this;
        if (save) {
            that.save();
        }

        $(document.body).append($('script[name=flowExecution]').html());
        var dialog = $(".execution");
        var d = dialog.kendoWindow({
            title: 'Flow Execution',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: true,
            width: '50%'
        }).data("kendoWindow");

        loadComponent($(".executionConsole", dialog), "FlowRunner", function(c) {
            var autoClose = true;
            var load = true;
            FlowRunner.init(
                session.app,
                $(".executionConsole", dialog),
                that.value(),
                () => {
                    autoClose = $("input[name=autoClose]:checked", dialog).length;
                    load = $("input[name=loadInDesigner]:checked", dialog).length;
                    d.close();
                },
                function(exId, exD) { that.runComplete(exId, exD, autoClose, load); }
            );
        });

        d.wrapper.css({ top: 140, left: '20%' });
        d.open();
    }

    runComplete(id, dialog, autoClose, load) {
        var d = dialog.data("kendoWindow");
        if (load) { this.loadInstance(id); }
        if (autoClose) { d.close(); }
    }

    async loadInstance(id) {
        var that = this;
        var instance = await api.get("Core/FlowInstanceData(" + id + ")");
        var ctx = JSON.parse(instance.ContextString);
        that.Activities.map(function (a) {
            a.addInstanceData(
                ctx.Flow.Activities.filter(ia => a.model.Ref === ia.Ref)[0],
                ctx.ExecutionLog.filter(l => l.Message.startsWith(a.model.Ref + "::"))
            );
        });
    }

    draw(ctx) {
        this.Activities.map(a => a.draw(ctx));
        this.Links.map(l => l.draw(ctx));
    }
}
class WorkflowDesigner {
    constructor(container, flow) {

        //TODO: handle this not being available for some reason
        window.flowTheme = session.app.Config.Themes[session.theme];

        this.stepTypes = window.knownTypes.filter(ctx => ctx.Name === "Workflow")[0].Types;
        this.workspace = $(".workspace", container);
        this.canvas = $("canvas", this.workspace);
        this.flow = new Flow(flow);
        this.autoArrange();
        this.init(container);
    }

    init(container) {
        $("[name=splitter]", container).kendoSplitter({
            orientation: "horizontal",
            panes: [
                { collapsable: false, size: '250px' },
                { collapsable: false, scrollable: false }
            ]
        });

        this.initMenu(container);
        this.canvas.off('mousedown').on('mousedown', this.mousedown.bind(this));
        this.canvas.off('mouseup').on('mouseup', this.mouseup.bind(this));
        this.canvas.off('mousemove').on('mousemove', this.mousemove.bind(this));

        let canvas = this.canvas[0];

        window.addEventListener('mousemove', function(e) {
            let bounds = canvas.getBoundingClientRect();

            window.mouseposition = {
                x: parseInt(e.clientX - bounds.left),
                y: parseInt(e.clientY - bounds.top)
            };
        });

        window.addEventListener('keydown', function(e) {
            if (e.ctrlKey && (e.keyCode || e.which) === 83) {
                e.preventDefault();
                editor.flow.save();
            }
        });

        window.mouseposition = { x: 0, y: 0 };

        this.canvas.height = this.workspace.height();
        this.canvas.width = this.workspace.width();

        $(window).on("resize", this.resize.bind(this));
        this.resize();
        this.draw();
    }

    getCollision() {
        let collision = this.flow.Activities.map(a => a.collides()).filter(r => r)[0];
        collision = collision || this.flow.Links.map(l => l.collides()).filter(r => r)[0];
        return collision;
    }

    mousedown(e) {
        if (e.target.localName.toLowerCase() === 'canvas') {
            this.active = this.getCollision();
            this.dragging = this.active ? false : true;
            this.dragstart = mouseposition;

            if (!this.dragging && this.active && this.active.mousedown) {
                this.active.mousedown(e, this);
            }

            window.addEventListener('keydown', this.listenForEsc);
        }
    }

    mouseup(e) {
        if (e.target.localName.toLowerCase() === 'canvas') {
            var collision = this.getCollision();
            this.dragging = false;

            if (this.active && this.active.mouseup && !Object.is(this.active, collision)) {
                this.active.mouseup(e, this, collision);
            }

            if (this.active && this.active.mouseup && Object.is(this.active, collision)) {
                this.active.mouseup(e, this);
            }

            this.active = null;
            window.removeEventListener('keydown', this.listenForEsc);
        }
    }

    mousemove(e) {
        if (e.target.localName.toLowerCase() === 'canvas') {
            if (!this.dragging && this.active && this.active.move) {
                this.active.move(e, this);
            }
            else if (this.dragging) {
                let x = mouseposition.x - this.dragstart.x;
                let y = mouseposition.y - this.dragstart.y;
                this.flow.Activities.map(a => a.setPosition(a.x + x, a.y + y));
                this.dragstart = mouseposition;
            }
        }
    }

    initMenu(container) {
        let menu = $(".flowmenu", container);

        $('[name=settings]', menu).off('click')
            .on('click', this.settings.bind(this));

        $('[name=run]', menu).off('click')
            .on('click', this.flow.run.bind(this.flow));

        $('[name=save]', menu).off('click')
            .on('click', this.flow.save.bind(this.flow));

        this.buildMenu(menu);
    }

    categoryIconMap(category) {
        switch (category) {
            case "Api":
                return "k-i-hyperlink-globe";
            case "DMS":
                return "k-i-folder";
            case "Sftp":
                return "k-i-folder-more";
            case "Transaction":
                return "k-i-dollar";
            case "Log":
                return "k-i-track-changes-enable";
            case "Transformation":
                return "k-i-shape";
            case "FlowControl":
                return "k-i-connector";
            case "Templating":
                return "k-i-template-manager";
            default:
                return "k-i-gear";
        }
    }

    buildMenu(container) {
        let groups = Array.from(new Set(this.stepTypes.map((t) =>  t.Category )));
        groups.map(g => container.append(this.buildMenuCategory.bind(this)({ Name: g, Types: this.stepTypes.filter((st) => st.Category === g) })));
    }

    buildMenuCategory(type) {
        let that = this;

        if (type.Types.length > 0) {
            let categoryName = type.Name.split("`")[0];

            categoryName = categoryName === 'Activity'
                ? 'Activity'
                : categoryName.replace('Activity', '');

            let category = $('<ul class="categorytypes" data-category="' + type.Name + '"></ul>');
            category.append('<li class="header"><span class="k-icon ' + this.categoryIconMap(categoryName) + '"></span>' + categoryName + '</li>');
            let sublist = $('<ul class="subtypes"></ul>');
            category.append(sublist);

            type.Types.map(t => {
                let typeName = t.DisplayName.split("`")[0];

                typeName = typeName === 'Activity'
                    ? 'Activity'
                    : typeName.replace('Activity', '');

                let newType = $('<li><span name="add" class="k-icon k-i-add"></span>' + typeName + '</li>');
                newType.data('model', t);
                newType.css('cursor', 'pointer');
                let helper = $('<div class="flow-helper"></div>');
                helper.css({ 'padding': '8px 16px', 'border': '1px solid #000' });

                newType.draggable({
                    helper: function(e) {
                        helper.html($(e.target).html());
                        return helper.appendTo(document.body);
                    },
                    stop: function(e) { that.menuItemClicked.bind(that)(e, mouseposition); }
                });

                sublist.append(newType);
            });

            sublist.css({ 'max-height': 0, transition: 'all 0.3s', overflow: 'hidden' });

            $('li', category).css('padding', '8px');
            $('ul', category).css('padding', '0 0 0 8px');

            $('.header', category).on('click', function () {
                let h = sublist.css('max-height');
                sublist.css({ 'max-height': h !== '500px' ? '500px' : 0 });
            });

            return category;
        }
        else { return null; }
    }

    menuItemClicked(e, position) {
        let activity = $(e.target).data('model');
        if (activity) { this.flow.addActivity(activity, position); }
    }

    resize() {
        this.canvas[0].height = this.workspace.height();
        this.canvas[0].width = this.workspace.width();
    }

    settings() {
        let that = this;
        let dialog = $('<div></div>').appendTo(document.body);

        let d = dialog.kendoWindow({
            title: 'Settings',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: true,
            width: '50%'
        }).data("kendoWindow");

        d.content('<div name="content"></div>');

        let container = $("[name=content]", d.element);

        loadComponent(container, "FlowSettings", function(c) {
            FlowSettings.init(session.app, container, that.flow, function () {
                d.close();
            });
        });

        d.wrapper.css({
            top: 140,
            left: '20%'
        });

        d.open();
    }

    draw() {
        let ctx = this.canvas[0].getContext('2d');
        ctx.clearRect(0, 0, this.workspace.width(), this.workspace.height());
        this.drawGrid(ctx, this.workspace.width(), this.workspace.height());
        this.flow.draw(ctx);
        setTimeout(this.draw.bind(this), 1000 / 60);
    }

    drawGrid(ctx, width, height) {
        let yLines = Math.ceil(width / 10);
        let xLines = Math.ceil(height / 10);

        for (let y = 0; y < yLines; y++) {
            let start = { x: (10 * y), y: 0 };
            let end = { x: (10 * y), y: height };
            draw.line(ctx, start.x, start.y, end.x, end.y, y % 5 === 0 ? '#99b' : '#dde', 0.5);
        }

        for (let x = 0; x < xLines; x++) {
            let start = { x: 0, y: (x * 10) };
            let end = { x: width, y: (x * 10) };
            draw.line(ctx, start.x, start.y, end.x, end.y, x % 5 === 0 ? '#99b' : '#dde', 0.5);
        }
    }

    autoArrange() {
        let that = this;

        function getActivitiesAt(x, y) {
            return that.flow.Activities.filter(a => a.x === x && a.y === y);
        }

        function placeActivity(prev, a, i) {
            let x = (a.x == 0) ? prev.x + a.w + 100 : a.x;
            let y = prev.y + (i * 1.5);

            a.setPosition(x, y);
            getChildActivities(a).map(function (b, j) {
                placeActivity(a, b, j * a.h);
            });
        }

        function getChildActivities(a) {
            let links = that.flow.Links.filter(l => l.model.Source === a.model.Ref);
            return that.flow.Activities.filter(b => links.filter(l => l.model.Destination === b.model.Ref).length > 0);
        }

        // find the first set of activities
        that.flow.Activities
            .filter(a => that.flow.Links.filter(l => l.model.Destination === a.model.Ref).length === 0)
            .map(function (c, i) { placeActivity({ x: -200, y: 100 }, c, i); });

        for (let i in that.flow.Activities) {
            let a = that.flow.Activities[i];
            let clash = getActivitiesAt(a.x, a.y);

            if (clash.length > 1) {
                clash[1].setPosition(clash[1].x, clash[1].y += 150);
            }
        }
    }

    listenForEsc(e) {
        if (e.keyCode == 27) { // 27 = esc key.
            this.dragging = false;
            this.active = null;
            window.removeEventListener('keydown', this.listenForEsc);
        }
    }
}