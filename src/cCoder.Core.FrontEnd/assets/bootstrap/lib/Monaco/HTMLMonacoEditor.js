class HTMLMonacoEditor extends MonacoEditor {
    constructor(container, args) {
        super(container, args);
        this.language = "html";
    }

    init(callback) {
        super.init(() => {
            this.editor.addAction({
                id: "w3validate",
                label: "w3c validation",
                precondition: null,
                keybindingContext: null,
                run: (ed) => {
                    $.ajax({
                        url: "https://validator.w3.org/nu/?out=json\n",
                        method: "POST",
                        data: this.getValue(),
                        contentType: "text/html"
                    }).done((data) => {
                        if (typeof data !== "object") {
                            data = JSON.parse(data);
                        }
                        var markers = [];
                        if (data.messages.length > 0) {
                            for (var i = 0; i < data.messages.length; i = i + 1) {
                                var message = data.messages[i];
                                markers.push({
                                    startLineNumber: message.lastLine - 1,
                                    endLineNumber: message.lastLine - 1,
                                    message: message.message,
                                    startColumn: message.firstColumn,
                                    endColumn: message.lastColumn,
                                    severity: this.monaco.MarkerSeverity.Warning,
                                });
                            }
                        }
                        this.monaco.editor.setModelMarkers(this.model, "owner", markers);
                    });
                    return null;
                }
            });

            if (callback)
                callback();

            this.editor.updateOptions({
                hover: {
                    enabled: false
                }
            });
        });
    }
}