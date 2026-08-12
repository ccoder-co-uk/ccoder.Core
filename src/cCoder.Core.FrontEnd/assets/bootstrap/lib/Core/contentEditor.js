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

        this.prepareEditableRegion();
        this.setupToolbars();
    }

    prepareEditableRegion() {
        addPageEditorStyle("ccoder-page-editor-styles", `
            [contenteditable].ccoder-editable-content {
                box-sizing: border-box;
                min-height: 1.5rem;
                border: 2px dashed #0d6efd !important;
                cursor: text;
            }
            [contenteditable].ccoder-editable-content.ccoder-editable-content-active {
                border-color: #fd7e14 !important;
            }
            .ccoder-content-editor-toolbar {
                position: fixed !important;
                display: none;
                flex-direction: column;
                flex-wrap: nowrap !important;
                align-items: center;
                width: fit-content;
                max-width: calc(100vw - 16px);
                z-index: 10001;
                background: #fff;
                box-shadow: 0 4px 12px rgba(0, 0, 0, .2);
            }
            .ccoder-content-editor-toolbar > .ccoder-toolbar-row {
                display: flex;
                flex-wrap: wrap;
                align-items: center;
                width: max-content;
                max-width: 100%;
            }
            .ccoder-content-editor-toolbar::before {
                content: none;
                display: none;
            }
            .ccoder-content-editor-toolbar .k-editortoolbar-dragHandle {
                align-self: stretch;
                display: inline-flex;
                align-items: center;
                padding: 0 .25rem;
                cursor: move;
            }
            .ccoder-content-editor-toolbar button[name='viewSource'] .k-icon {
                min-width: calc(var(--kendo-font-size, inherit) * var(--kendo-line-height, normal));
                min-height: calc(var(--kendo-font-size, inherit) * var(--kendo-line-height, normal));
                display: inline-flex;
                align-items: center;
                justify-content: center;
            }
        `);

        $(this.element).addClass("ccoder-editable-content");
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
                    template: '<button role="button" title="View Source" class="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-icon-button k-toolbar-tool" name="viewSource"><span class="k-icon k-i-file k-button-icon"></span></button>'
                }
            ],
            change: () => {
                this.pageContent.Html = this.kendoEditor.value();
            }
        }).data("kendoEditor");

        const toolbarWindow = $(this.kendoEditor.toolbar.element)
            .closest(".k-window");

        const dragHandle = $(".k-editortoolbar-dragHandle", toolbarWindow)
            .first()
            .detach();

        this.toolbarElement = $(this.kendoEditor.toolbar.element)
            .addClass("ccoder-content-editor-toolbar")
            .appendTo("body")
            .hide();

        toolbarWindow.remove();

        const toolbarTools = this.toolbarElement.children().detach();
        const firstDropdown = toolbarTools
            .filter(".k-toolbar-item[data-command='formatting']")
            .first();
        const firstDropdownIndex = toolbarTools.index(firstDropdown);
        const viewSourceTool = toolbarTools
            .filter((_, tool) =>
                $(tool).is("button[name='viewSource']")
                    || $("button[name='viewSource']", tool).length > 0)
            .first();

        const primaryRow = $("<div class='ccoder-toolbar-row'></div>")
            .append(dragHandle)
            .append(toolbarTools.slice(0, firstDropdownIndex))
            .append(viewSourceTool);

        const dropdownRow = $("<div class='ccoder-toolbar-row'></div>")
            .append(toolbarTools.slice(firstDropdownIndex).not(viewSourceTool));

        this.toolbarElement.append(primaryRow, dropdownRow);

        this.toolbarElement.draggable({ handle: dragHandle });

        $(this.element).on("click focusin", () => this.showToolbar());

        $(document).on("mousedown", (event) => {
            if (!$(event.target).closest(this.element).length
                && !$(event.target).closest(this.toolbarElement).length) {
                this.hideToolbar();
            }
        });

        $(window).on("resize scroll", () => {
            if (this.toolbarElement.is(":visible")) {
                this.positionToolbar();
            }
        });

        this.setupViewSourceButton();
    }

    showToolbar() {
        if (ContentEditor.activeEditor && ContentEditor.activeEditor !== this) {
            ContentEditor.activeEditor.hideToolbar();
        }

        ContentEditor.activeEditor = this;
        $(this.element).addClass("ccoder-editable-content-active");
        this.toolbarElement.css("display", "flex");
        this.positionToolbar();
    }

    hideToolbar() {
        $(this.element).removeClass("ccoder-editable-content-active");
        this.toolbarElement.hide();

        if (ContentEditor.activeEditor === this) {
            ContentEditor.activeEditor = null;
        }
    }

    positionToolbar() {
        const contentBounds = this.element[0].getBoundingClientRect();
        const toolbarHeight = this.toolbarElement.outerHeight();
        const top = Math.max(8, contentBounds.top - toolbarHeight - 8);
        const left = Math.min(
            Math.max(8, contentBounds.left),
            Math.max(8, window.innerWidth - this.toolbarElement.outerWidth() - 8));

        this.toolbarElement.css({ left: `${left}px`, top: `${top}px` });
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
