class MonacoEditor {
    constructor(container, args) {
        this.container = container;
        this.language = args.language;//default to javascript language.
        this.code = args.code || "";
        this.fullscreen = false;
        //Load from CDN.
        require.config({
            paths: {
                'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.20.0/min/vs'
            }
        });
        window.MonacoEnvrionment = { getWorkerUrl: () => proxy };
        let proxy = URL.createObjectURL(new Blob([`
			self.MonacoEnvironment = {
				baseUrl: 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.20.0/min/'
			};
		`],
            { type: 'text/javascript' }));
        if(typeof require !== 'undefined') {
            require(["vs/editor/editor.main"], () => {
                this.monaco = monaco;
            });
        }

    }

    dispose() {
        this.editor.onDidChangeModelContent(function(event) {  });
        $(this.container).html("");//Clear the container of the editor as well.
    }

    showAutoCompletion() {//Default implementation.

    }

    getValue() {//Function exists as it seems there are multiple ways to get the content... 
        return this.editor.getValue();
    }

    init(callback) {
        require(["vs/editor/editor.main"], () => {
            this.monaco = monaco;
            this.showAutoCompletion();//Register autocomplete before monaco editor gets created.
            this.model = this.monaco.editor.createModel(this.code, this.language, null);
            this.editor = this.monaco.editor.create(this.container, {
                value: this.code,
                language: this.language,
                model: this.model,
                automaticLayout: true,
                tabIndex: 4,
                fontFamily: 'monospace',
                minimap: {
                    enabled: true
                }
            });

            if (session.app.Config.Themes[session.theme].IsDark) {
                monaco.editor.setTheme("vs-dark");
            }
            
            this.editor.addAction({
                id: "fullscreen",
                label: "Make Editor Fullscreen",
                precondition: null,
                keybindingContext: null,
                run: (ed) => {
                    if (this.fullscreen) {
                        $(this.container).css(this.backupStyle);
                        $("nav").show();
                    } else {
                        this.backupStyle = {
                            position: $(this.container).css("position"),
                            top: $(this.container).css("top"),
                            left: $(this.container).css("left"),
                            width: $(this.container).css("width"),
                            height: $(this.container).css("height"),
                            "z-index": $(this.container).css("z-index"),
                            display: $(this.container).css("display"),
                            clear: $(this.container).css("clear")
                        };
                        $(this.container).css("position", "fixed");
                        $(this.container).css("top", "0px");
                        $(this.container).css("left", "0px");
                        $(this.container).css("width", window.innerWidth);
                        $(this.container).css("height", window.innerHeight);
                        $(this.container).css("z-index", 9999);
                        $(this.container).css("display", "block");
                        $(this.container).css("clear", "both");
                        $("nav").hide();
                    }
                    this.fullscreen = !this.fullscreen;//Flip it.
                    return null;
                }
            });
            this.editor.onDidChangeModelContent(this.onChange || function(event) {  });
            if(callback) callback();
        });

    }
}

