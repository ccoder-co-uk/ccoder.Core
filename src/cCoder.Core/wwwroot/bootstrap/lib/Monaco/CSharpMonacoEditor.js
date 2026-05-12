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