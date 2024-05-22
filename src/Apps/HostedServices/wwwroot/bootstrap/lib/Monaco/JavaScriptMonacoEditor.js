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