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