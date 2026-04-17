class CMS extends Tree {

	dragStart(e) {
		let nodeData = this.dataItem(e.sourceNode);
		if (nodeData.data == null) {
			e.preventDefault();
			$(e.element).addClass("k-denied");
		}
	}

	async drop(e) {
		e.preventDefault();

		// grab source and destination nodes
		let dropNode = this.dataItem($(e.dropTarget).closest(".k-in"));

		let dragNode = this.dataItem($(e.sourceNode));

		// apply move
		let index = this.dataItem($(e.sourceNode).parent()).items.indexOf(dragNode);
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
		await api.update("ContentManagement/Page(" + dragNode.data.Id + ")", dragNode.data);
	}

	rightClick(e) {
		$("[name=contextMenu]").remove();
		e.preventDefault();
		let tree = $("[name=treeRoot]", container).data("kendoTreeView");
		let page = tree.dataItem(e.target).data;
		// if we dont have nodedata.data then we have a root level page
		container.append("<div name='contextMenu' class='contextMenu'></div>");
		let menu = $("[name=contextMenu]");
		menu.css({
			display: "block",
			position: "absolute",
			left: e.pageX - 3,
			top: e.pageY - 3
		})

		loadComponent(menu, "PageContextMenu", function (comp) {
			PageContextMenu.init(app, page, function (operation) {
				menu.remove();
				let node = operation === "delete"
					? tree.parent(e.target)
					: e.target;

				tree.collapse(node);
				setTimeout(function () { tree.expand(node); }, 400);
			});
		});
	}

	select(e) {
		let workspace = $("[name=workspace]", container);
		let tree = $("[name=treeRoot]", container).data("kendoTreeView");
		PageManagement.init(app, workspace, tree.dataItem(e.node).data);
	}

	collapse(e) {
		let nodeData = this.dataItem(e.node);
		$(nodeData).children('.k-group').remove();
		if (nodeData.data != null) {
			nodeData.loaded(false);
			e.node.loaded = false;
		}
	}

	async expand(e) {
		let tree = $("[name=treeRoot]", container).data("kendoTreeView");
		let nodeData = tree.dataItem(e.node);
		let page = nodeData.data;

		let items = nodeData.children.data();
		for (let i = 0, max = items.length; i < max; i++) {
			let item = tree.findByUid(items[i].uid);
			tree.remove(item);
		}

		let pages = await this.loadChildPages(page);
		let allPages = this.allPages || [];
		let newNodes = this.sortPages(pages).map(function (p) {
			let pageName = p.Name || "Unknown";
			return {
				text: pageName,
				type: "Page",
				spriteCssClass: "page",
				expanded: false,
				hasChildren: CMS.prototype.hasChildren(p, allPages),
				data: p,
				draggable: true,
				droppable: ["Page"]
			};

		});

		for (let node in newNodes) {
			nodeData.items.push(node);
		}
	}

	async loadChildPages(page) {
		let result = await api.get("ContentManagement/Page");
		this.allPages = result.value || [];
		return this.getDirectChildren(page, this.allPages);
	}

	sortPages(pages) {
		return (pages || []).slice().sort((left, right) => (left.Order || 0) - (right.Order || 0));
	}

	getDirectChildren(page, pages) {
		let parentPath = page && page.Path ? page.Path : null;
		return (pages || []).filter((candidate) => {
			let path = candidate.Path || "";
			if (path === "") {
				return false;
			}

			return this.parentPathOf(path) === parentPath;
		});
	}

	hasChildren(page, pages) {
		let pagePath = page && page.Path ? page.Path : null;
		return (pages || []).some((candidate) => this.parentPathOf(candidate.Path || "") === pagePath);
	}

	parentPathOf(path) {
		if (!path) {
			return null;
		}

		let separatorIndex = path.lastIndexOf("/");
		return separatorIndex === -1
			? null
			: path.substring(0, separatorIndex);
	}
}
