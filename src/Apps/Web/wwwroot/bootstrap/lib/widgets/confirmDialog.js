class ConfirmDialog extends Dialog {

    init(callback) {
        if (!this.template) {
            this.template = "<div class='dialog'><p>" + this.args.question + "</p><hr /><button class='btn btn-sm btn-primary float-end' name='confirm'>" + this.args.confirm + `</button></div>`;
        }
        super.init(callback);
    }
}