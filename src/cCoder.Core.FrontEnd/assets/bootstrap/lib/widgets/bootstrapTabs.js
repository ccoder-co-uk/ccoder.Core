class BootstrapTabs extends Widget {
    constructor(args) {
        super(null, args);

        if (!args)
            args = {};

        this.args = args;

        this.name = args.name.replace(" ", "-") || "Tabs";
        this.id = Guid();
        this.title = args.title || (args.name + " Tabs");
        this.defaultTab = args.defaultTab || null;
        this.tabs = args.tabs;
        this.idPrefix = null;

        this.container = args.container;
    }

    async init(app, callback) {
        this.idPrefix = this.name + '-' + this.id;
        var tabButtons = this.generateTabButtons();
        var tabContents = this.generateTabContents();

        $(this.container).append(`
            <div class="tab-control" name="${this.idPrefix}-tabs">
                <nav>
                    <div class="nav nav-tabs" id="${this.idPrefix}-nav-tab" role="tablist">
                        ${tabButtons}
                    </div>
                </nav>
                <div class="tab-content" id="${this.idPrefix}-tabContent">
                    ${tabContents}
                </div>
            </div>
        `);

        await this.initEventsAndCallbacks(app);

        if (this.defaultTab != null) {
            $(`#${this.idPrefix}-${this.defaultTab}-tab`, this.container).click();
        }

        if (callback)
            await callback();
    }

    generateTabButtons() {
        var tabs = ``;

        for (var i in this.tabs) {
            var tab = this.tabs[i];

            var isActive = this.defaultTab == tab.name || (this.defaultTab == null && i == 0)
                ? 'active'
                : null;

            var icon = tab.icon != null
                ? '<span class="k-icon ' + tab.icon + '"></span>'
                : '';

            tabs += `<button class="nav-link bg ${isActive}" id="${this.idPrefix}-${tab.name}-tab" data-bs-toggle="tab" data-bs-target="#${this.idPrefix}-${tab.name}" type="button" role="tab" aria-controls="${this.idPrefix}-${tab.name}" aria-selected="true" tabindex="${i}">
                    ${icon} ${tab.label}
                </button>`;
        }

        return tabs;
    }

    generateTabContents() {
        var tabs = '';

        for (var i in this.tabs) {
            var tab = this.tabs[i];

            var isActive = this.defaultTab == tab.name || (this.defaultTab == null && i == 0)
                ? 'active show'
                : '';

            var content = '';

            if (tab.content && tab.content != '') {
                content = tab.content;
            }

            tabs += `<div class="tab-pane fade ${isActive}" id="${this.idPrefix}-${tab.name}" role="tabpanel" aria-labelledby="${this.idPrefix}-${tab.name}-tab" name="${this.idPrefix}-${tab.name}">
                    ${content}
                </div>`;
        }

        return tabs;
    }

    async initEventsAndCallbacks(app) {
        for (let i in this.tabs) {
            let tab = this.tabs[i];

            console.log(`Setup: #${this.idPrefix}-${tab.name}-tab`, tab);

            if (tab.onclick != null) {
                $(`#${this.idPrefix}-${tab.name}-tab`).on('click', tab.onclick);
            }

            if (tab.callback != null) {
                $(`#${this.idPrefix}-${tab.name}-tab`).on('click', tab.callback);
            }

            if (tab.component != null) {
                tab.loaded = false;

                $(`#${this.idPrefix}-${tab.name}-tab`).on('click', () => {
                    if (tab.loaded)
                        return;

                    let container = tab.componentContainer != null
                        ? $(tab.componentContainer)
                        : $(`#${this.idPrefix}-${tab.name}`);

                    if (tab.init != null) {
                        tab.init();
                    } else {
                        loadComponent(container, tab.component, (c) => {
                            let contentContainer = tab.contentContainer != null
                                ? $(tab.contentContainer)
                                : $(`#${this.idPrefix}-${tab.name}`);

                            c.init(app, contentContainer);
                        });
                    }

                    tab.loaded = true;
                });
            }
        }
    }
}