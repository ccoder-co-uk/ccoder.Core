class WorkflowDesigner {
    constructor(container, flow) {

        //TODO: handle this not being available for some reason
        window.flowTheme = session.app.Config.Themes[session.theme];

        this.stepTypes = window.knownTypes.filter(ctx => ctx.Name === "Workflow")[0].Types;
        this.workspace = $(".workspace", container);
        this.canvas = $("canvas", this.workspace);
        this.flow = new Flow(flow);
        this.autoArrange();
        this.init(container);
    }

    init(container) {
        $("[name=splitter]", container).kendoSplitter({
            orientation: "horizontal",
            panes: [
                { collapsable: false, size: '250px' },
                { collapsable: false, scrollable: false }
            ]
        });

        this.initMenu(container);
        this.canvas.off('mousedown').on('mousedown', this.mousedown.bind(this));
        this.canvas.off('mouseup').on('mouseup', this.mouseup.bind(this));
        this.canvas.off('mousemove').on('mousemove', this.mousemove.bind(this));

        let canvas = this.canvas[0];

        window.addEventListener('mousemove', function(e) {
            let bounds = canvas.getBoundingClientRect();

            window.mouseposition = {
                x: parseInt(e.clientX - bounds.left),
                y: parseInt(e.clientY - bounds.top)
            };
        });

        window.addEventListener('keydown', function(e) {
            if (e.ctrlKey && (e.keyCode || e.which) === 83) {
                e.preventDefault();
                editor.flow.save();
            }
        });

        window.mouseposition = { x: 0, y: 0 };

        this.canvas.height = this.workspace.height();
        this.canvas.width = this.workspace.width();

        $(window).on("resize", this.resize.bind(this));
        this.resize();
        this.draw();
    }

    getCollision() {
        let collision = this.flow.Activities.map(a => a.collides()).filter(r => r)[0];
        collision = collision || this.flow.Links.map(l => l.collides()).filter(r => r)[0];
        return collision;
    }

    mousedown(e) {
        if (e.target.localName.toLowerCase() === 'canvas') {
            this.active = this.getCollision();
            this.dragging = this.active ? false : true;
            this.dragstart = mouseposition;

            if (!this.dragging && this.active && this.active.mousedown) {
                this.active.mousedown(e, this);
            }

            window.addEventListener('keydown', this.listenForEsc);
        }
    }

    mouseup(e) {
        if (e.target.localName.toLowerCase() === 'canvas') {
            var collision = this.getCollision();
            this.dragging = false;

            if (this.active && this.active.mouseup && !Object.is(this.active, collision)) {
                this.active.mouseup(e, this, collision);
            }

            if (this.active && this.active.mouseup && Object.is(this.active, collision)) {
                this.active.mouseup(e, this);
            }

            this.active = null;
            window.removeEventListener('keydown', this.listenForEsc);
        }
    }

    mousemove(e) {
        if (e.target.localName.toLowerCase() === 'canvas') {
            if (!this.dragging && this.active && this.active.move) {
                this.active.move(e, this);
            }
            else if (this.dragging) {
                let x = mouseposition.x - this.dragstart.x;
                let y = mouseposition.y - this.dragstart.y;
                this.flow.Activities.map(a => a.setPosition(a.x + x, a.y + y));
                this.dragstart = mouseposition;
            }
        }
    }

    initMenu(container) {
        let menu = $(".flowmenu", container);

        $('[name=settings]', menu).off('click')
            .on('click', this.settings.bind(this));

        $('[name=run]', menu).off('click')
            .on('click', this.flow.run.bind(this.flow));

        $('[name=save]', menu).off('click')
            .on('click', this.flow.save.bind(this.flow));

        this.buildMenu(menu);
    }

    categoryIconMap(category) {
        switch (category) {
            case "Api":
                return "k-i-hyperlink-globe";
            case "DMS":
                return "k-i-folder";
            case "Sftp":
                return "k-i-folder-more";
            case "Transaction":
                return "k-i-dollar";
            case "Log":
                return "k-i-track-changes-enable";
            case "Transformation":
                return "k-i-shape";
            case "FlowControl":
                return "k-i-connector";
            case "Templating":
                return "k-i-template-manager";
            default:
                return "k-i-gear";
        }
    }

    buildMenu(container) {
        let groups = Array.from(new Set(this.stepTypes.map((t) =>  t.Category )));
        groups.map(g => container.append(this.buildMenuCategory.bind(this)({ Name: g, Types: this.stepTypes.filter((st) => st.Category === g) })));
    }

    buildMenuCategory(type) {
        let that = this;

        if (type.Types.length > 0) {
            let categoryName = type.Name.split("`")[0];

            categoryName = categoryName === 'Activity'
                ? 'Activity'
                : categoryName.replace('Activity', '');

            let category = $('<ul class="categorytypes" data-category="' + type.Name + '"></ul>');
            category.append('<li class="header"><span class="k-icon ' + this.categoryIconMap(categoryName) + '"></span>' + categoryName + '</li>');
            let sublist = $('<ul class="subtypes"></ul>');
            category.append(sublist);

            type.Types.map(t => {
                let typeName = t.DisplayName.split("`")[0];

                typeName = typeName === 'Activity'
                    ? 'Activity'
                    : typeName.replace('Activity', '');

                let newType = $('<li><span name="add" class="k-icon k-i-add"></span>' + typeName + '</li>');
                newType.data('model', t);
                newType.css('cursor', 'pointer');
                let helper = $('<div class="flow-helper"></div>');
                helper.css({ 'padding': '8px 16px', 'border': '1px solid #000' });

                newType.draggable({
                    helper: function(e) {
                        helper.html($(e.target).html());
                        return helper.appendTo(document.body);
                    },
                    stop: function(e) { that.menuItemClicked.bind(that)(e, mouseposition); }
                });

                sublist.append(newType);
            });

            sublist.css({ 'max-height': 0, transition: 'all 0.3s', overflow: 'hidden' });

            $('li', category).css('padding', '8px');
            $('ul', category).css('padding', '0 0 0 8px');

            $('.header', category).on('click', function () {
                let h = sublist.css('max-height');
                sublist.css({ 'max-height': h !== '500px' ? '500px' : 0 });
            });

            return category;
        }
        else { return null; }
    }

    menuItemClicked(e, position) {
        let activity = $(e.target).data('model');
        if (activity) { this.flow.addActivity(activity, position); }
    }

    resize() {
        this.canvas[0].height = this.workspace.height();
        this.canvas[0].width = this.workspace.width();
    }

    settings() {
        let that = this;
        let dialog = $('<div></div>').appendTo(document.body);

        let d = dialog.kendoWindow({
            title: 'Settings',
            modal: true,
            visible: false,
            close: function() { this.destroy(); },
            resizable: true,
            width: '50%'
        }).data("kendoWindow");

        d.content('<div name="content"></div>');

        let container = $("[name=content]", d.element);

        loadComponent(container, "FlowSettings", function(c) {
            FlowSettings.init(session.app, container, that.flow, function () {
                d.close();
            });
        });

        d.wrapper.css({
            top: 140,
            left: '20%'
        });

        d.open();
    }

    draw() {
        let ctx = this.canvas[0].getContext('2d');
        ctx.clearRect(0, 0, this.workspace.width(), this.workspace.height());
        this.drawGrid(ctx, this.workspace.width(), this.workspace.height());
        this.flow.draw(ctx);
        setTimeout(this.draw.bind(this), 1000 / 60);
    }

    drawGrid(ctx, width, height) {
        let yLines = Math.ceil(width / 10);
        let xLines = Math.ceil(height / 10);

        for (let y = 0; y < yLines; y++) {
            let start = { x: (10 * y), y: 0 };
            let end = { x: (10 * y), y: height };
            draw.line(ctx, start.x, start.y, end.x, end.y, y % 5 === 0 ? '#99b' : '#dde', 0.5);
        }

        for (let x = 0; x < xLines; x++) {
            let start = { x: 0, y: (x * 10) };
            let end = { x: width, y: (x * 10) };
            draw.line(ctx, start.x, start.y, end.x, end.y, x % 5 === 0 ? '#99b' : '#dde', 0.5);
        }
    }

    autoArrange() {
        let that = this;

        function getActivitiesAt(x, y) {
            return that.flow.Activities.filter(a => a.x === x && a.y === y);
        }

        function placeActivity(prev, a, i) {
            let x = (a.x == 0) ? prev.x + a.w + 100 : a.x;
            let y = prev.y + (i * 1.5);

            a.setPosition(x, y);
            getChildActivities(a).map(function (b, j) {
                placeActivity(a, b, j * a.h);
            });
        }

        function getChildActivities(a) {
            let links = that.flow.Links.filter(l => l.model.Source === a.model.Ref);
            return that.flow.Activities.filter(b => links.filter(l => l.model.Destination === b.model.Ref).length > 0);
        }

        // find the first set of activities
        that.flow.Activities
            .filter(a => that.flow.Links.filter(l => l.model.Destination === a.model.Ref).length === 0)
            .map(function (c, i) { placeActivity({ x: -200, y: 100 }, c, i); });

        for (let i in that.flow.Activities) {
            let a = that.flow.Activities[i];
            let clash = getActivitiesAt(a.x, a.y);

            if (clash.length > 1) {
                clash[1].setPosition(clash[1].x, clash[1].y += 150);
            }
        }
    }

    listenForEsc(e) {
        if (e.keyCode == 27) { // 27 = esc key.
            this.dragging = false;
            this.active = null;
            window.removeEventListener('keydown', this.listenForEsc);
        }
    }
}