async function initialisePageEditing() {
    if ($(".component[name=Login]").length > 0) {
        return;
    }

    var pageToolbar = new PageToolbar();
    await pageToolbar.init();

    $("[contenteditable]").each(function (i) {
        (new ContentEditor($(this), pageToolbar.page))
            .init();
    });
}

function addPageEditorStyle(id, css) {
    if (document.getElementById(id)) {
        return;
    }

    const style = document.createElement("style");
    const nonceSource = document.querySelector("script[nonce], style[nonce]");

    style.id = id;
    style.textContent = css;

    if (nonceSource?.nonce) {
        style.nonce = nonceSource.nonce;
    }

    document.head.appendChild(style);
}

$(initialisePageEditing);
