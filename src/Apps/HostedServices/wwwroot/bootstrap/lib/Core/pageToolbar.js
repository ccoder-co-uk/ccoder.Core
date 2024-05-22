class PageToolbar {
    async init() {
        await this.getPage();

        this.toolbarElement = $("body").prepend(`
            <div class="pageToolbar k-toolbar k-widget">
                <div class="tools-left">
                    <button name="selectPage"><span class="k-icon k-i-file" style="margin-right: 10px;"></span>Select Page</button>
                    <button name="newChildPage"><span class="k-icon k-i-plus"></span>New Child Page</button>
                    <button name="newRootPage"><span class="k-icon k-i-plus"></span>New Root Page</button>
                    <button name="deletePage"><span class="k-icon k-i-delete"></span>Delete Page</button>
                    <button name="pageProperties"><span class="k-icon k-i-page-properties"></span>Page Properties</button>
                </div>
                <div class="tools-right">
                    <div class="k-toolbar-item">
                        <label>Last Updated: </label>
                        <label name="lastUpdatedTime"></label>
                    </div>
                    <div class="k-toolbar-item">
                        <label>Culture: </label>
                        <input name="cultureDropdown" />
                    </div>
                    <div class="k-toolbar-item">
                        <button name="pageSave"><span class="k-icon k-i-save"></span>Save</button>
                    </div>
                </div>
            </div>
        `);

        $("[name=pageSave]", this.toolbarElement).click((e) => this.save(e));
        await this.setupCultureDropdown();
        $("[name=lastUpdatedTime]", this.toolbarElement).html(kendo.toString(new Date(this.page.LastUpdated), "yyyy-MM-dd hh:mm"));
        $("[name=selectPage]", this.toolbarElement).click((e) => this.selectPage(e));
        $("[name=newChildPage]", this.toolbarElement).click((e) => this.newPage(e, this.page.Id));
        $("[name=newRootPage]", this.toolbarElement).click((e) => this.newPage(e, null));
        $("[name=deletePage]", this.toolbarElement).click((e) => this.deletePage(e));
        $("[name=pageProperties]", this.toolbarElement).click((e) => this.pageProperties(e));
    }

    async getPage() {
        var path = window.location.pathname.substring(1);
        this.page = (await api.get("Core/Page?$filter=AppId eq " + session.app.Id + " and Path eq '" + path + "'&$expand=Contents")).value[0];
    }

    async save(e) {
        e.preventDefault();
        this.page.Contents = this.page.Contents.filter(r => r.Html.length > 0 || r.CultureId === "");

        await api.update("Core/Page(" + this.page.Id + ")", this.page)
            .then((newPage) => {
                $("[name=lastUpdatedTime]", this.toolbarElement).html(kendo.toString(new Date(newPage.LastUpdated), "yyyy-MM-dd hh:mm"));
                notification.success("Saved");
                if (this.page.Contents.filter(r => r.CultureId === session.culture).length === 0) {
                    setQueryParameter("culture", "");
                }
            }).catch((err) => error(err));
    }

    async deletePage(e) {
        e.preventDefault();
        await api.destroy("Core/Page(" + this.page.Id + ")").then(async () => {
            if (this.page.ParentId != null) {
                var parentPage = await api.get("Core/Page(" + this.page.ParentId + ")");
                window.location.href = api.apiRoot.replaceAll("/Api/", "/") + parentPage.Path + "?edit=true";
            } else {
                window.location.href = api.apiRoot.replaceAll("/Api/", "/?edit=true");
            }
        }).catch((err) => error(err));
    }

    async newPage(e, parentId) {
        e.preventDefault();

        var newPageDialog = new EditorDialog({
            title: "New Page",
            fields: [
                { field: "Name", title: "Name" }
            ],
            data: {
                Name: "",
                ParentId: parentId,
                AppId: this.page.AppId,
                ShowOnMenus: false,
                Path: null,
                Layout: "Default",
                Order: 0,
                Contents: [],
                PageInfo: []
            },
            confirm: "Create"
        });

        newPageDialog.events.confirm = async function () {
            var newPage = newPageDialog.data.toJSON();
            newPage.Contents.push({ CultureId: "", Name: "body", Html: "" });
            newPage.PageInfo.push({ CultureId: "", Title: newPage.Name });
            await api.post("Core/Page", newPage).then((returnedPage) => {
                notification.success("Child page created");
                window.location.href = api.apiRoot.replaceAll("/Api/", "/") + returnedPage.Path + "?edit=true"
                newPageDialog.events.close();
            }).catch((err) => error(err));
        };

        newPageDialog.init();
    }

    async selectPage(e) {
        e.preventDefault();

        var selectPageDialog = new Dialog({ width: 400, height: 600, title: "Select Page" });
        selectPageDialog.init(async () => {
            var cmsComponent = await loadComponent(selectPageDialog.element, "CMS");
            await cmsComponent.init(session.app, $(".component[name=CMS]", selectPageDialog.element), (selectPageAction) => {
                window.location.href = window.location.href.replaceAll(this.page.Path, selectPageAction.Path);
            });
        })
    }

    async setupCultureDropdown() {
        var cultures = (await api.get("Core/Culture?$filter=Apps/any(a: a/AppId eq " + session.app.Id + ") or Id eq ''")).value;

        for (let i = 0; i < cultures.length; i++) {
            cultures[i].InPage = this.page.Contents.filter(r => r.CultureId === cultures[i].Id).length > 0;
        }

        $("[name=cultureDropdown]").kendoDropDownList({
            dataSource: cultures,
            dataTextField: "Name",
            dataValueField: "Id",
            valueTemplate: "#=(InPage) ? '<span class=\"k-icon k-i-check\"></span>' : '<span class=\"k-icon k-i-x\"></span>'# #=Name#",
            template: "#=(InPage) ? '<span class=\"k-icon k-i-check\"></span>' : '<span class=\"k-icon k-i-x\"></span>'# #=Name#",
            change: function (_) {
                setQueryParameter("culture", this.value());
            }
        }).data("kendoDropDownList").value(session.culture);
    }

    async pageProperties(e) {
        e.preventDefault();
        var pagePropertiesDialog = new Dialog({ title: "Page Properties: " + this.page.Path, width: 900, height: 250 });
        pagePropertiesDialog.init(async () => {
            var pageProperties = await loadComponent(pagePropertiesDialog.element, "PageProperties")
            pageProperties.init(session.app, $(".component[name=PageProperties]", $(pagePropertiesDialog.element)), this.page, (saveEvent) => {
                this.save(saveEvent);
            });
        });
    }
}