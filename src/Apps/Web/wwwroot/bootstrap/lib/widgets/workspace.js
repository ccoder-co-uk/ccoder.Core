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