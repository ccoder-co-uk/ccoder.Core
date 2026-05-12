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


class JavaScriptMonacoEditor extends MonacoEditor {
    constructor(container, args) {
        super(container, args);
        this.language = "javascript";
    }

    getType(thing, isMember) {
        isMember =  (isMember == undefined) ? (typeof isMember == "boolean") ? isMember : false : false; // Give isMember a default value of false
        switch ((typeof thing).toLowerCase()) {
            case "object":
                return this.monaco.languages.CompletionItemKind.Class;
            case "function":
                return (isMember) ? this.monaco.languages.CompletionItemKind.Method : this.monaco.languages.CompletionItemKind.Function;
            default:
                return (isMember) ? this.monaco.languages.CompletionItemKind.Property : this.monaco.languages.CompletionItemKind.Variable;
        }
    }

    init(callback) {
        super.init(() => {
            require(['https://unpkg.com/esprima@~4.0/dist/esprima.js'], (parser) => {
                this.editor.onDidChangeModelContent(e => {
                    try {
                        var hintInfo = parser.parse(this.getValue(), {tolerant: true, loc: true, range: true});
                    } catch(err) {
                        var hintInfo = { errors: [err]};
                    }
                    var markers = [];
                    if (hintInfo.errors.length > 0) {
                        for (var i = 0; i < hintInfo.errors.length; i = i + 1) {
                            var error = hintInfo.errors[i];
                            markers.push({
                                startLineNumber: error.lineNumber - 1,
                                endLineNumber: error.lineNumber - 1,
                                message: error.description,
                                startColumn: 1,
                                endColumn: error.column,
                                severity: this.monaco.MarkerSeverity.Error,
                                
                            });
                        }
                    }
                    this.monaco.editor.setModelMarkers(this.model, "owner", markers);
                    if (this.onChange) {
                        this.onChange(e);
                    }
                });
            });
            if (callback) callback();
        });
    }

    showAutoCompletion() {
        var obj = window;

        this.monaco.languages.registerCompletionItemProvider('javascript', {
            // Run this function when the period or open parenthesis is typed (and anything after a space)
            triggerCharacters: ['.', '('],

            // Function to generate autocompletion results
            provideCompletionItems: (model, position, token) => {
                //TODO: Add support for class definitions by doing window.ConfirmDialog = new ConfirmDialog and so forth.

                // Split everything the user has typed on the current line up at each space, and only look at the last word
                var last_chars = model.getValueInRange({startLineNumber: position.lineNumber, startColumn: 0, endLineNumber: position.lineNumber, endColumn: position.column});
                var words = last_chars.replace("\t", "").split(" ");
                var active_typing = words[words.length - 1]; // What the user is currently typing (everything after the last space)

                // If the last character typed is a period then we need to look at member objects of the obj object 
                var is_member = active_typing.charAt(active_typing.length - 1) == ".";

                // Array of autocompletion results
                var result = [];

                // Used for generic handling between member and non-member objects
                var last_token = obj;
                var prefix = '';

                if (is_member) {
                    // Is a member, get a list of all members, and the prefix
                    var parents = active_typing.substring(0, active_typing.length - 1).split(".");
                    last_token = obj[parents[0]];
                    prefix = parents[0];

                    // Loop through all the parents the current one will have (to generate prefix)
                    for (var i = 1; i < parents.length; i++) {
                        if (last_token.hasOwnProperty(parents[i])) {
                            prefix += '.' + parents[i];
                            last_token = last_token[parents[i]];
                        } else {
                            // Not valid
                            return result;
                        }
                    }

                    prefix += '.';
                }

                // Get all the child properties of the last token
                for (var prop in last_token) {
                    // Do not show properites that begin with "__"
                    if (last_token.hasOwnProperty(prop) && !prop.startsWith("__")) {
                        // Get the detail type (try-catch) incase object does not have prototype 
                        var details = '';
                        try {
                            details = last_token[prop].__proto__.constructor.name;
                        } catch (e) {
                            details = typeof last_token[prop];
                        }

                        // Create completion object
                        var to_push = {
                            label: prefix + prop,
                            kind: this.getType(last_token[prop], is_member),
                            detail: details,
                            insertText: prop
                        };

                        // Change insertText and documentation for functions
                        if (to_push.detail.toLowerCase() == 'function') {
                            to_push.insertText += "(";
                            to_push.documentation = (last_token[prop].toString()).split("{")[0]; // Show function prototype in the documentation popup
                        }

                        // Add to final results
                        result.push(to_push);
                    }
                }
                return {
                    suggestions: result
                };
            }
        });
    }
}
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
class CSharpMonacoEditor extends MonacoEditor {
    constructor(container, args) {
        super(container, args);
        this.language = "csharp";//testing 123.
    }

    showAutoCompletion() {
        /*
        TODO: Add CSharp Autocomplete.
        var obj = window;

        this.monaco.languages.registerCompletionItemProvider('csharp', {
            // Run this function when the period or open parenthesis is typed (and anything after a space)
            triggerCharacters: ['.', '('],

            // Function to generate autocompletion results
            provideCompletionItems: (model, position, token) => {
                //TODO: Add support for class definitions by doing window.ConfirmDialog = new ConfirmDialog and so forth.

                // Split everything the user has typed on the current line up at each space, and only look at the last word
                var last_chars = model.getValueInRange({startLineNumber: position.lineNumber, startColumn: 0, endLineNumber: position.lineNumber, endColumn: position.column});
                var words = last_chars.replace("\t", "").split(" ");
                var active_typing = words[words.length - 1]; // What the user is currently typing (everything after the last space)

                // If the last character typed is a period then we need to look at member objects of the obj object 
                var is_member = active_typing.charAt(active_typing.length - 1) == ".";

                // Array of autocompletion results
                var result = [];

                // Used for generic handling between member and non-member objects
                var last_token = obj;
                var prefix = '';

                if (is_member) {
                    // Is a member, get a list of all members, and the prefix
                    var parents = active_typing.substring(0, active_typing.length - 1).split(".");
                    last_token = obj[parents[0]];
                    prefix = parents[0];

                    // Loop through all the parents the current one will have (to generate prefix)
                    for (var i = 1; i < parents.length; i++) {
                        if (last_token.hasOwnProperty(parents[i])) {
                            prefix += '.' + parents[i];
                            last_token = last_token[parents[i]];
                        } else {
                            // Not valid
                            return result;
                        }
                    }

                    prefix += '.';
                }

                // Get all the child properties of the last token
                for (var prop in last_token) {
                    // Do not show properites that begin with "__"
                    if (last_token.hasOwnProperty(prop) && !prop.startsWith("__")) {
                        // Get the detail type (try-catch) incase object does not have prototype 
                        var details = '';
                        try {
                            details = last_token[prop].__proto__.constructor.name;
                        } catch (e) {
                            details = typeof last_token[prop];
                        }

                        // Create completion object
                        var to_push = {
                            label: prefix + prop,
                            kind: this.getType(last_token[prop], is_member),
                            detail: details,
                            insertText: prop
                        };

                        // Change insertText and documentation for functions
                        if (to_push.detail.toLowerCase() == 'function') {
                            to_push.insertText += "(";
                            to_push.documentation = (last_token[prop].toString()).split("{")[0]; // Show function prototype in the documentation popup
                        }

                        // Add to final results
                        result.push(to_push);
                    }
                }
                return {
                    suggestions: result
                };
            }
        });
         */
    }
}