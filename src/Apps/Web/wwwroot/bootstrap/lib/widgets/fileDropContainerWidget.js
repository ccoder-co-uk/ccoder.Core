class FileDropContainerWidget extends Widget {
    constructor(element) {
        super(element, null);
        this.container = element;
        this.events = {};
        $(this.container).data("fileDropContainerWidget", this);
    }

    init() {
        this.counter = 0;

        $(this.container).on("dragstart", (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.counter++;
        });

        $(this.container).on("dragover", (e) => {
            e.preventDefault();
            e.stopPropagation();
            if (this.fileUploaderTag == null) {
                $(this.container)[0].style.setProperty("display", "none", "important");

                this.fileUploaderTag = $("<input class='fileUploaderTag' type='file' multiple name='fileUpload'/>").insertAfter(this.container);
                this.fileUploaderTag.on("drop", (e) => {
                    this.events.drop(e);
                    this.remove();
                });

                this.fileUploaderTag.on("change", (e) => {
                    this.events.drop(e);
                    this.remove();
                });
            }
        });

        $(this.container).on("dragleave", (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.counter--;

            if (this.counter == 0) {
                this.remove();
            }
        });
    }

    remove() {
        this.counter = 0;

        if (this.fileUploaderTag != null) {
            this.fileUploaderTag.remove();
        }
        this.fileUploaderTag = null;

        $(this.container)[0].style.setProperty("display", "flex", "important");
    }
}