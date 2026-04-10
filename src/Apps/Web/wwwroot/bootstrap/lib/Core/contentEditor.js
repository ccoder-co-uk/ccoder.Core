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