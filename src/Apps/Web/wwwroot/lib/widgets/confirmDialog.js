class ConfirmDialog extends Dialog {
    constructor(args) {
        super(args);
    }

    init(callback) {
        if (!this.template) {
            this.template = "<div class='dialog'><p>" + this.args.question + "</p><hr><div class='value'><button name='close'>" + this.args.close + "</button><button name='confirm'>" + this.args.confirm + `</button></div></div>`;
        }
        super.init(callback);
    }
}