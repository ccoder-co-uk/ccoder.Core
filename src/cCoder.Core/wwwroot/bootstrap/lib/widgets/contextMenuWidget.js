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