async function initialisePageEditing() {
    if ($(".component[name=Login]").length > 0) {
        return;
    }

    var pageToolbar = new PageToolbar();
    await pageToolbar.init();

    $("[contenteditable]").each(function (i) {
        (new ContentEditor($(this), pageToolbar.page))
            .init();
    });
}

$(initialisePageEditing);
class ContentEditor {
    constructor(element, page) {
        this.element = element;
        this.page = page;
        let matchedContents = page.Contents
            .filter(r => r.Name === $(this.element).attr("name") && r.CultureId === session.culture);

        if (matchedContents.length > 0) {
            this.pageContent = matchedContents[0];
        } else {
            page.Contents.push({
                CultureId: session.culture,
                Name: $(this.element).attr("name"),
                Html: page.Contents.filter(r => r.Name === $(this.element).attr("name") && r.CultureId === '')[0].Html,
                PageId: page.Id
            });

            this.pageContent = page.Contents
                .filter(r => r.Name === $(this.element).attr("name") && r.CultureId === session.culture)[0];
        }
        window.currentContentWidget = this;
    }

    async init() {
        $(this.element).data("contentEditor", this);

        this.setupToolbars();
    }

    setupToolbars() {
        this.kendoEditor = $(this.element).kendoEditor({

            tools: [
                "bold",
                "italic",
                "underline",
                "undo",
                "redo",
                "justifyLeft",
                "justifyCenter",
                "justifyRight",
                "insertUnorderedList",
                "createLink",
                "unlink",
                "insertImage",
                "tableWizard",
                "createTable",
                "addRowAbove",
                "addRowBelow",
                "addColumnLeft",
                "addColumnRight",
                "deleteRow",
                "deleteColumn",
                "mergeCellsHorizontally",
                "mergeCellsVertically",
                "splitCellHorizontally",
                "splitCellVertically",
                "tableAlignLeft",
                "tableAlignCenter",
                "tableAlignRight",
                "formatting",
                {
                    name: "fontName",
                    items: [
                        { text: "Andale Mono", value: "\"Andale Mono\"" }, // Font-family names composed of several words should be wrapped in \" \"
                        { text: "Arial", value: "Arial" },
                        { text: "Arial Black", value: "\"Arial Black\"" },
                        { text: "Book Antiqua", value: "\"Book Antiqua\"" },
                        { text: "Comic Sans MS", value: "\"Comic Sans MS\"" },
                        { text: "Courier New", value: "\"Courier New\"" },
                        { text: "Georgia", value: "Georgia" },
                        { text: "Helvetica", value: "Helvetica" },
                        { text: "Impact", value: "Impact" },
                        { text: "Symbol", value: "Symbol" },
                        { text: "Tahoma", value: "Tahoma" },
                        { text: "Terminal", value: "Terminal" },
                        { text: "Times New Roman", value: "\"Times New Roman\"" },
                        { text: "Trebuchet MS", value: "\"Trebuchet MS\"" },
                        { text: "Verdana", value: "Verdana" },
                    ]
                },
                "fontSize",
                "foreColor",
                "backColor",
                {
                    type: 'button',
                    name: 'viewSource',
                    template: '<button role="button" title="View Source" class="k-button k-tool" name="viewSource"><span class="k-icon k-i-file"></span></button>'
                }
            ],
            change: () => {
                this.pageContent.Html = this.kendoEditor.value();
            }
        }).data("kendoEditor");

        this.setupViewSourceButton();
    }

    setupViewSourceButton() {
        $("button[name=viewSource]", this.toolbarElement).click((e) => {
            e.preventDefault();

            let viewSourceDialog = new Dialog({
                width: 1000,
                height: 610,
                title: "View Source",
                template: `
                    <div class="editorContainer">
		                <textarea name="sourceEditor"></textarea>
                    </div>
                    <hr>
	                <div class="value">
		                <button name="close">Close</button>
	                </div>
                    <style scoped>
                         .editorContainer 				{ display: inline-block; width: 100%; height: 535px; margin-right: 10px; }
                         .editorContainer > textarea	{ width: 99%; height: 100%; }
                    </style>
                `
            });

            viewSourceDialog.init(() => {
                $("[name=sourceEditor]", viewSourceDialog.element).val(this.kendoEditor.value());
                $("[name=sourceEditor]", viewSourceDialog.element).on("keyup", () => {
                    this.kendoEditor.value($("[name=sourceEditor]", viewSourceDialog.element).val());
                    this.pageContent.Html = $("[name=sourceEditor]", viewSourceDialog.element).val();
                });
            });
        });
    }
}
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
            <style>
                .pageToolbar { 
                    position: static; 
                    width: 300px; 
                    padding: 10px; 
                    top: 20px; 
                    left: 30px; 
                    z-index: 10000;
                    padding: 0;
                }
                .pageToolbar > * { display: inline-block; }
                .pageToolbar > label { margin: 0 5px; }
                .pageToolbar > button[name=pageSave] { margin-left: 10px; width: 100px; }
            </style>
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

        this.page = (await api.get("Core/Page?$filter=AppId eq " + session.app.Id + " and Path eq '" + path + "'&$expand=Contents"))
            .value[0];
    }

    async save(e) {
        e.preventDefault();
        this.page.Contents = this.page.Contents
            .filter(r => r.Html.length > 0 || r.CultureId === "");

        await api.update("Core/Page(" + this.page.Id + ")", this.page)
            .then((newPage) => {
                $("[name=lastUpdatedTime]", this.toolbarElement).html(kendo.toString(new Date(newPage.LastUpdated), "yyyy-MM-dd hh:mm"));
                notification.success("Saved");

                if (this.page.Contents.filter(r => r.CultureId === session.culture).length === 0) {
                    setQueryParameter("culture", "");
                }
            }).catch((err) => error(err));
    }

    async setupCultureDropdown() {
        let cultures = (await api.get("Core/Culture?$filter=Apps/any(a: a/AppId eq " + session.app.Id + ") or Id eq ''")).value;

        for (let i = 0; i < cultures.length; i++) {
            cultures[i].InPage = this.page.Contents.filter(r => r.CultureId === cultures[i].Id).length > 0;
        }

        $("[name=cultureDropdown]").kendoDropDownList({
            dataSource: cultures,
            dataTextField: "Name",
            dataValueField: "Id",
            width: 150,
            valueTemplate: "#=(InPage) ? '<span class=\"k-icon k-i-check\"></span>' : '<span class=\"k-icon k-i-x\"></span>'# #=Name#",
            template: "#=(InPage) ? '<span class=\"k-icon k-i-check\"></span>' : '<span class=\"k-icon k-i-x\"></span>'# #=Name#",
            change: function (_) {
                setQueryParameter("culture", this.value());
            }
        }).data("kendoDropDownList").value(session.culture);
    }
}