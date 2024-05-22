class FileVersionGridWidget extends GridWidget {
    constructor(element, file, readOnly) {
        super(element, null);
        this.file = file;
        this.groupable = false;
        this.editable = true;
        this.pageable = false;
        this.readOnly = readOnly;

        this.endpoint = "Core/FileContent";
        this.odataAppend = "?$select=Id,FileId,Description,Size,CreatedBy,CreatedOn,Version&$filter=FileId eq " + file.Id;

        
    }

    async init() {
        this.commands.push({
            name: "download",
            icon: "k-i-download",
            text: await api.getResource("DMS", "Download", session.culture).ShortDisplayName,
            href: api.apiRoot + "DMS/" + this.file.Path + "?version=#=Version#&t=" + session.token + "&download"
        });

        if (!this.readOnly) {
            this.commands.push({
                name: "delete",
                icon: "k-i-delete",
                text: await api.getResource("DMS", "Delete", session.culture).ShortDisplayName
            });
        }

        this.columns = [
            {
                field: "Version",
                width: 40, title: "\#",
                editable: false
            },
            {
                title: await api.getResource("DMS", "Name", session.culture).ShortDisplayName,
                template: this.file.Name
            },
            {
                field: 'Size',
                width: 80,
                title: await api.getResource("DMS", "Size", session.culture).ShortDisplayName
            },
            {
                field: "CreatedBy",
                width: 200,
                title: await api.getResource("DMS", "CreatedBy", session.culture).ShortDisplayName,
                editable: false
            },
            {
                field: "CreatedOn",
                width: 130,
                title: await api.getResource("DMS", "CreatedOn", session.culture).ShortDisplayName,
                editable: false,
                template: "#=kendo.toString(new Date(CreatedOn), '" + type.dateFormat + " HH:mm:ss')#"
            },
        ];

        this.dataBound = async () => {
            $("[name=delete]", this.gridElement).on("click", async (e) => await this.deleteFileVersion(e));
        };

        return super.init();
    }

    async deleteFileVersionApiCall(version, confirmDialog) {
        await api.file.destroy(this.file.Path + "?version=" + version).then(async () => {
            this.refresh();
            confirmDialog.events.close();
        }).catch((err) => { error(err); });
    }

    async deleteFileVersion(e) {
        e.preventDefault();

        var fileVersion = this.dataItem($(e.currentTarget).closest("tr"));

        var confirmDialog = new ConfirmDialog({
            title: await api.getResource("DMS", "areyousure", session.culture).DisplayName,
            question: await api.getResource("DMS", "areyousure", session.culture).DisplayName,
            close: await api.getResource("DMS", "Close", session.culture).DisplayName,
            confirm: await api.getResource("DMS", "Confirm", session.culture).DisplayName
        });

        confirmDialog.events.confirm = () => this.deleteFileVersionApiCall(fileVersion.Version, confirmDialog);

        confirmDialog.init();
    }
}