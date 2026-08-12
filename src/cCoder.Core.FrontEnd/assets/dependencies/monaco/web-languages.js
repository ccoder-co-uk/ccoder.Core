if (window.monaco && window.monaco.languages) {
    const registerLanguage = (id, definition) => {
        if (!window.monaco.languages.getLanguages().some(language => language.id === id)) {
            window.monaco.languages.register({ id: id });
        }

        window.monaco.languages.setMonarchTokensProvider(id, definition);
    };

    registerLanguage("json", { tokenizer: { root: [[/\"(?:[^\"\\]|\\.)*\"(?=\s*:)/, "key"], [/\"(?:[^\"\\]|\\.)*\"/, "string"], [/-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?/, "number"], [/true|false|null/, "keyword"], [/[{}\[\],:]/, "delimiter"], [/\s+/, "white"]] } });
    registerLanguage("javascript", { tokenizer: { root: [[/\b(?:async|await|break|case|catch|class|const|continue|default|delete|do|else|export|extends|false|finally|for|from|function|if|import|in|instanceof|let|new|null|of|return|static|super|switch|this|throw|true|try|typeof|undefined|var|void|while|yield)\b/, "keyword"], [/\"(?:[^\"\\]|\\.)*\"|'(?:[^'\\]|\\.)*'|`(?:[^`\\]|\\.)*`/, "string"], [/\/\*.*\*\//, "comment"], [/\/\/.*/, "comment"], [/\b\d+(?:\.\d+)?\b/, "number"], [/[{}\[\]().,;:]/, "delimiter"]] } });
    registerLanguage("html", { tokenizer: { root: [[/<!--/, "comment", "@comment"], [/<[\w-]+/, "tag", "@tag"], [/<\/[\w-]+\s*>/, "tag"], [/&[\w#]+;/, "string.escape"]], comment: [[/-->/, "comment", "@pop"], [/./, "comment"]], tag: [[/[\w-]+/, "attribute.name"], [/="(?:[^"]*)"|='(?:[^']*)'/, "attribute.value"], [/>/, "tag", "@pop"], [/\/>/, "tag", "@pop"]] } });
}