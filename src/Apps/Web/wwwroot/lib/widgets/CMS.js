class CMS extends Tree {
    constructor(element, args) {
		super(element, args);
    }

	dragStart(e) {
		var nodeData = this.dataItem(e.sourceNode);
		if (nodeData.data == null) {
			e.preventDefault();
			$(e.element).addClass("k-denied");
		}
	}

	async drop(e) {
		e.preventDefault();

		// grab source and destination nodes
		var dropNode = this.dataItem($(e.dropTarget).closest(".k-in"));

		var dragNode = this.dataItem($(e.sourceNode));

		// apply move
		var index = this.dataItem($(e.sourceNode).parent()).items.indexOf(dragNode);
		this.dataItem($(e.sourceNode).parent()).items.splice(index, 1);

		if (dropNode.type != "Root") {
			dropNode.items.push(dragNode);
			dragNode.data.ParentId = dropNode.data.Id;
		}
		else {
			dropNode.items.push(dragNode);
			dragNode.data.ParentId = null;
		}

		// update the server
		await api.update("Core/Page(" + dragNode.data.Id + ")", dragNode.data);
		notification.success("[resource_displayname[PageMoved]]");
	}

	rightClick(e) {
		$("[name=contextMenu]").remove();
		e.preventDefault();
		var tree = $("[name=treeRoot]", container).data("kendoTreeView");
		var page = tree.dataItem(e.target).data;
		// if we dont have nodedata.data then we have a root level page
		container.append("<div name='contextMenu' class='contextMenu'></div>");
		var menu = $("[name=contextMenu]");
		menu.css({
			display: "block",
			position: "absolute",
			left: e.pageX - 3,
			top: e.pageY - 3
		})

		loadComponent(menu, "PageContextMenu", function (comp) {
			PageContextMenu.init(app, page, function (operation) {
				menu.remove();
				var node = operation === "delete"
					? tree.parent(e.target)
					: e.target;

				tree.collapse(node);
				setTimeout(function () { tree.expand(node); }, 400);
			});
		});
	}

	select(e) {
		var workspace = $("[name=workspace]", container);
		var tree = $("[name=treeRoot]", container).data("kendoTreeView");
		PageManagement.init(app, workspace, tree.dataItem(e.node).data);
	}

	collapse(e) {
		var nodeData = this.dataItem(e.node);
		$(nodeData).children('.k-group').remove();
		if (nodeData.data != null) {
			nodeData.loaded(false);
			e.node.loaded = false;
		}
	}

	async expand(e) {
		var tree = $("[name=treeRoot]", container).data("kendoTreeView");
		var nodeData = tree.dataItem(e.node);
		var page = nodeData.data;

		if (!page) { page = { Id: "null" }; }

		var items = nodeData.children.data();
		for (var i = 0, max = items.length; i < max; i++) {
			var item = tree.findByUid(items[i].uid);
			tree.remove(item);
		}

		var pages = await api.get("Core/Page?$filter=AppId eq " + app.Id + " and ParentId eq " + page.Id + "&$expand=PageInfo,Roles&$orderby=Order");
		var newNodes = pages.value.map(function (p) {
			var pageName = "Unknown";
			if (p.PageInfo && p.PageInfo.length > 0) {
				pageName = p.PageInfo[0].Title;
			}
			return {
				text: pageName,
				type: "Page",
				spriteCssClass: "page",
				expanded: false,
				hasChildren: true,
				data: p,
				draggable: true,
				droppable: ["Page"]
			};

		});

		for (var node in newNodes) {
			nodeData.items.push(node);
		}
	}
}