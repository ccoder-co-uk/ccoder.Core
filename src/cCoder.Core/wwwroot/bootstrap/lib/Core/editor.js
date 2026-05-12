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

$(initialisePageEditing);