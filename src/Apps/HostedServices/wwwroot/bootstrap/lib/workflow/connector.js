class Connector extends Collidable {
    constructor(parent, type) {
        super('', 0, 0, 0, 0, nodeSize);
        this.parent = parent;
        this.type = type;
        this.col = '#37e83a';
        this.sel = '#00ff04';
        this.updatePosition();
    }

    mousedown() { this.active = true; }

    removeLink() {
        this.parent.flow.Links = this.parent.flow.Links
            .filter(l => l.model.Destination !== this.parent.model.Ref);
    }

    mouseup(e, editor, otherConnector) {
        if (this.active && this.type === 'input') { this.removeLink(); }

        if (otherConnector && this.type !== otherConnector) {
            var from = this.type === 'output' ? this : otherConnector;
            var to = this.type === 'output' ? otherConnector : this;

            if (from && from.type === 'output') {
                var newLink = {
                    "Source": from.parent.model.Ref,
                    "Destination": to.parent.model.Ref,
                    "Expression": ""
                };
                this.parent.flow.Links.push(new Link(newLink, this.parent.flow));
            }
        }

        this.active = false;
    }

    updatePosition() {
        if (this.type === "input") { this.x = this.parent.x + 2; }
        if (this.type === "output") { this.x = this.parent.x + this.parent.w - 2; }
        this.y = this.parent.y + (this.parent.h / 2) + (handleHeight / 2);
    }

    draw(ctx) {
        draw.circle(ctx, this.x, this.y, this.r, this.col, this.sel);
        if (this.active) { draw.line(ctx, this.x, this.y, mouseposition.x, mouseposition.y, '#37e83a'); }
    }
}