class BootstrapDialog extends Widget {
    constructor(args) {
        super(null, args);

        if (!args)
            args = {};

        this.args = args;

        this.name = args.name || "Dialog";
        this.title = args.title || "Dialog";
        this.height = args.height || "auto";
        this.width = args.width || 'md';
        this.template = args.template || "";
        this.component = args.component || false;
        this.elementId = Guid();
        this.container = null;
        this.footer = args.footer;
        this.modal = null;

        this.events = args.events || {}
    }

    async init(callback) {
        $("body").append(`<div class="modal fade" name="${this.name}" id="${this.elementId}" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-${this.width}">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            ${this.title}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body" name="${this.name}-content">
                        ${this.template}
                    </div>
                    <div class="modal-footer" name="footer">
                        ${this.footer}
                    </div>
                </div>
            </div>
        </div>`);

        this.element = $(`#${this.elementId}`);

        var dialog = $(`[name=${this.name}-content]`, this.element);
        this.modal = new bootstrap.Modal(this.element, {});

        var wireUp =  () => {
            for (var i in this.events) {
                $("button[name=" + i + "]", this.element).on("click", this.events[i]);
            }
        }

        if (this.component) {
            loadComponent(dialog, this.component, (c) => {
                wireUp();

                if (callback)
                    callback();
            
                return this;
            });
        } else {
            wireUp();

            if (callback)
                callback();

            return this;
        }
    }

    show() {
        this.modal.show();
    }

    hide() {
        this.modal.hide();
    }
}

