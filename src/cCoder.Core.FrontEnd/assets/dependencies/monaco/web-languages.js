if (window.monaco && window.monaco.languages) {
    const registerLanguage = (id, definition) => {
        if (!window.monaco.languages.getLanguages().some(language => language.id === id)) {
            window.monaco.languages.register({ id: id });
        }

        window.monaco.languages.setMonarchTokensProvider(id, definition);
    };

    registerLanguage("json", { tokenizer: { root: [[/\"(?:[^\"\\]|\\.)*\"(?=\s*:)/, "key"], [/\"(?:[^\"\\]|\\.)*\"/, "string"], [/-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?/, "number"], [/true|false|null/, "keyword"], [/[{}\[\],:]/, "delimiter"], [/\s+/, "white"]] } });
    registerLanguage("javascript", { tokenizer: { root: [[/\b(?:async|await|break|case|catch|class|const|continue|default|delete|do|else|export|extends|false|finally|for|from|function|if|import|in|instanceof|let|new|null|of|return|static|super|switch|this|throw|true|try|typeof|undefined|var|void|while|yield)\b/, "keyword"], [/\"(?:[^\"\\]|\\.)*\"|'(?:[^'\\]|\\.)*'|`(?:[^`\\]|\\.)*`/, "string"], [/\/\*.*\*\//, "comment"], [/\/\/.*/, "comment"], [/\b\d+(?:\.\d+)?\b/, "number"], [/[{}\[\]().,;:]/, "delimiter"]] } });
    registerLanguage("csharp", { tokenizer: { root: [[/\b(?:abstract|as|async|await|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|required|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|var|virtual|void|volatile|while|yield)\b/, "keyword"], [/@?\"(?:[^\"\\]|\\.)*\"|'(?:[^'\\]|\\.)'/, "string"], [/\/\*/, "comment", "@comment"], [/\/\/.*/, "comment"], [/\b(?:0[xX][0-9a-fA-F]+|\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)[dDfFmMuUlL]*\b/, "number"], [/[{}\[\]().,;:]/, "delimiter"]], comment: [[/\*\//, "comment", "@pop"], [/./, "comment"]] } });
    registerLanguage("html", { tokenizer: { root: [[/<!--/, "comment", "@comment"], [/<[\w-]+/, "tag", "@tag"], [/<\/[\w-]+\s*>/, "tag"], [/&[\w#]+;/, "string.escape"]], comment: [[/-->/, "comment", "@pop"], [/./, "comment"]], tag: [[/[\w-]+/, "attribute.name"], [/=\"(?:[^\"]*)\"|='(?:[^']*)'/, "attribute.value"], [/>/, "tag", "@pop"], [/\/>/, "tag", "@pop"]] } });
}
