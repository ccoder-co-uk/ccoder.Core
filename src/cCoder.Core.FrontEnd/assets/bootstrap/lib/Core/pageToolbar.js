class PageToolbar {
    async init() {
        await this.getPage();

        $("body").prepend(`
            <div class="pageToolbar k-window k-window-titleless">
                <div class="editorToolbarWindow k-editor-window k-window-content">
                    <span class="k-editortoolbar-dragHandle"><span class="k-icon k-i-handle-drag"></span></span>
                    <div class="k-editor-toolbar k-toolbar k-toolbar-md">
                        <span data-role="buttongroup" class="k-widget k-button-group k-toolbar-button-group" role="group">
                            <label>Culture</label>
                            <input name="cultureDropdown" />
                        </span>
                        <span data-role="buttongroup" class="k-widget k-button-group k-toolbar-button-group" role="group">
                            <button name="pageSave" class="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-icon-button k-toggle-button k-toolbar-tool"><span class="k-icon k-i-save"></span>Save</button>
                        </span>
                    </div>
                </div>
            </div>
        `);

        this.toolbarElement = $(".pageToolbar");
        this.toolbarElement.draggable();

        $("[name=pageSave]", this.toolbarElement)
            .click((e) => this.save(e));

        await this.setupCultureDropdown();
    }

    async getPage() {
        let path = window.location.pathname
            .substring(1);

        this.page = (await api.get("ContentManagement/Page?$filter=AppId eq " + session.app.Id + " and Path eq '" + path + "'&$expand=Contents"))
            .value[0];
    }

    async save(e) {
        e.preventDefault();
        this.page.Contents = this.page.Contents
            .filter(r => r.Html.length > 0 || r.CultureId === "");

        await api.update("ContentManagement/Page(" + this.page.Id + ")", this.page)
            .then((newPage) => {
                $("[name=lastUpdatedTime]", this.toolbarElement).html(kendo.toString(new Date(newPage.LastUpdated), "yyyy-MM-dd hh:mm"));
                notification.success("Saved");

                if (this.page.Contents.filter(r => r.CultureId === session.culture).length === 0) {
                    setQueryParameter("culture", "");
                }
            }).catch((err) => error(err));
    }

    async setupCultureDropdown() {
        let cultures = (await api.get("ContentManagement/Culture?$filter=Apps/any(a: a/AppId eq " + session.app.Id + ") or Id eq ''")).value;

        for (let i = 0; i < cultures.length; i++) {
            cultures[i].InPage = this.page.Contents.filter(r => r.CultureId === cultures[i].Id).length > 0;
        }

        const renderCulture = culture => {
            const icon = culture.InPage ? "k-i-check" : "k-i-x";
            return `<span class="k-icon ${icon}"></span> ${kendo.htmlEncode(culture.Name)}`;
        };

        $("[name=cultureDropdown]").kendoDropDownList({
            dataSource: cultures,
            dataTextField: "Name",
            dataValueField: "Id",
            width: 150,
            valueTemplate: renderCulture,
            template: renderCulture,
            change: function (_) {
                setQueryParameter("culture", this.value());
            }
        }).data("kendoDropDownList").value(session.culture);
    }
}
