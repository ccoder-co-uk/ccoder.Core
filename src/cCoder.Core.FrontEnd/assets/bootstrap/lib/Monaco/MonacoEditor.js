var monacoEditorLoader = {
    loaderUrl: "/lib/monaco/min/vs/loader.js",
    vsPath: "/lib/monaco/min/vs",
    baseUrl: "/lib/monaco/min/",
    loadPromise: null,

    configureWorker: function() {
        if (window.MonacoEnvironment) return;

        let proxy = URL.createObjectURL(new Blob([
            "self.MonacoEnvironment = { baseUrl: '" + monacoEditorLoader.baseUrl + "' };"
        ],
            { type: 'text/javascript' }));
        window.MonacoEnvironment = { getWorkerUrl: () => proxy };
    },

    configureNonce: function() {
        let requestNonce = document.querySelector("script[nonce]")?.nonce;
        if (!requestNonce || Node.prototype.monacoNonceConfigured) return;

        let appendChild = Node.prototype.appendChild;
        Node.prototype.appendChild = function(node) {
            if (node.tagName === "STYLE" && !node.nonce) {
                node.nonce = requestNonce;
            }

            return appendChild.call(this, node);
        };

        Node.prototype.monacoNonceConfigured = true;
    },

    loadMonaco: function() {
        if (window.monaco && window.monaco.editor) {
            return Promise.resolve(window.monaco);
        }

        if (monacoEditorLoader.loadPromise) {
            return monacoEditorLoader.loadPromise;
        }

        monacoEditorLoader.configureWorker();
        monacoEditorLoader.configureNonce();

        monacoEditorLoader.loadPromise = new Promise((resolve, reject) => {
            const configure = () => {
                if (typeof window.require === 'undefined' || !window.require.config) {
                    reject(new Error("Monaco loader did not initialise."));
                    return;
                }

                window.require.config({
                    paths: {
                        'vs': monacoEditorLoader.vsPath
                    },
                    cspNonce: document.querySelector("script[nonce]")?.nonce
                });
                window.require(["vs/editor/editor.main"], () => resolve(window.monaco), reject);
            };

            if (typeof window.require !== 'undefined' && window.require.config) {
                configure();
                return;
            }

            let existing = document.querySelector("script[src='" + monacoEditorLoader.loaderUrl + "']");
            if (existing) {
                let timeout = setTimeout(() => reject(new Error("Timed out loading Monaco loader.")), 10000);
                const configureOnce = () => {
                    clearTimeout(timeout);
                    configure();
                };

                existing.addEventListener("load", configureOnce, { once: true });
                existing.addEventListener("error", () => reject(new Error("Failed to load Monaco loader.")), { once: true });
                setTimeout(() => {
                    if (typeof window.require !== 'undefined' && window.require.config) {
                        configureOnce();
                    }
                }, 0);
                return;
            }

            let script = document.createElement("script");
            script.src = monacoEditorLoader.loaderUrl;
            script.async = true;
            script.addEventListener("load", configure, { once: true });
            script.addEventListener("error", () => reject(new Error("Failed to load Monaco loader.")), { once: true });
            document.head.appendChild(script);
        });

        return monacoEditorLoader.loadPromise;
    }
};

monacoEditorLoader.configureNonce();

class MonacoEditor {
    constructor(container, args) {
        this.container = container;
        this.language = args.language;//default to javascript language.
        this.code = args.code || "";
        this.fullscreen = false;

    }

    dispose() {
        if (!this.editor) return;

        this.editor.onDidChangeModelContent(function(event) {  });
        $(this.container).html("");//Clear the container of the editor as well.
    }

    showAutoCompletion() {//Default implementation.

    }

    getValue() {//Function exists as it seems there are multiple ways to get the content... 
        return this.editor.getValue();
    }

    init(callback) {
        monacoEditorLoader.loadMonaco().then((monacoInstance) => {
            this.monaco = monacoInstance;
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

            if (session.app.Config?.Themes?.[session.theme]?.IsDark === true) {
                this.monaco.editor.setTheme("vs-dark");
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
        }).catch(error);

    }
}

