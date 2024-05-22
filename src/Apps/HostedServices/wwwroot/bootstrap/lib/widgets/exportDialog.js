class ExportDialog extends Dialog {
    constructor(args) {
        super(args);
        this.width = args.width || 500;
        this.height = args.height || 150;
        this.exportURL = args.exportURL;
        this.exportColumns = args.exportColumns;
        this.exportFileName = args.exportFileName;
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
            $("a[name=exportDownloadButton]", this.element).on("click", that.events.close);
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
        $("a[name=exportDownloadButton]", this.element).attr("href", this.getDownloadLink(format));

        var extension = (format == "excel") ? ".xlsx" : "." + format;
        if (this.exportFileName) {
            $("a[name=exportDownloadButton]", this.element).attr("download", this.exportFileName + extension);
        } else {
            $("a[name=exportDownloadButton]", this.element).attr("download", "export" + extension);
        }
    }

    async getTemplate(args) {
        args = args || {};
        var exportAs = args.exportAs || (await api.getResource("Default", "ExportAs", session.culture)).DisplayName;
        var download = args.download || (await api.getResource("Default", "Download", session.culture)).DisplayName;
        var close = args.close || (await api.getResource("Default", "Close", session.culture)).DisplayName;
        return `
                <ul class="fieldList">
                    <li>
                        <label>${exportAs}</label>
                        <div class="value">
                            <input type='text' name='exportDropdown' />
                        </div>
                    </li>
                </ul>
                <hr>
                <div class="value" style="float:right;">
                    <button name="close">${close}</button>
                    <a download="" href="" name="exportDownloadButton" style="margin-top:10px;margin-right:10px;">
                        <button name="download" style='margin-top:10px;'>${download}</button>
                    </a>
                </div>
        `;

    }
}