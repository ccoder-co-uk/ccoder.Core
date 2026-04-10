/// <reference path="workflowdesigner.js" />
class Connector extends Collidable {
    constructor(parent, type) {
        super('', 0, 0, 0, 0, nodeSize);
        this.parent = parent;
        this.type = type;
        this.col = window.flowTheme.colours.primary;
        this.sel = window.flowTheme.colours.secondary;
        this.updatePosition();
    }

    mousedown() {
        this.active = true;

        window.addEventListener('keydown', this.listenForEsc);
    }

    removeLink(otherConnector) {
        let from = this.type === 'output' ? this : otherConnector;
        let to = this.type === 'output' ? otherConnector : this;

        if (otherConnector) {
            var fromRef = from.parent.model.Ref;
            var toRef = to.parent.model.Ref;

            var removal = this.parent.flow.Links
                .filter(l => l.model.Source === fromRef && l.model.Destination === toRef)[0];

            this.parent.flow.Links = this.parent.flow.Links.filter(i => i !== removal);
        } else if(this.type == 'input') {
            this.parent.flow.Links = this.parent.flow.Links
                .filter(l => l.model.Destination !== to.parent.model.Ref);
        }
    }

    addLink(otherConnector) {
        if (otherConnector && this.type !== otherConnector) {
            let from = this.type === 'output' ? this : otherConnector;
            let to = this.type === 'output' ? otherConnector : this;

            if (from && from.type === 'output') {
                let newLink = {
                    "Source": from.parent.model.Ref,
                    "Destination": to.parent.model.Ref,
                    "Expression": ""
                };

                this.parent.flow.Links.push(new Link(newLink, this.parent.flow));
            }
        }
    }

    mouseup(e, editor, otherConnector) {
        var node1 = this.parent.model.Ref;
        var node2 = otherConnector ? otherConnector.parent.model.Ref : null;

        if (node2 == null) {
            this.parent.flow.Links = this.type == 'output'
                ? this.parent.flow.Links.filter(l => l.model.Source != node1)
                : this.parent.flow.Links.filter(l => l.model.Destination != node1);
        } else {
            var existingLinks = this.parent.flow.Links
                .filter(l => {
                    return (l.model.Source === node1 && ((l.model.Destination === node2) || !node2)) ||
                        (((l.model.Source === node2) || !node2) && l.model.Destination === node1)
                });

            if (existingLinks.length > 0)
                this.removeLink(otherConnector);
            else
                this.addLink(otherConnector);
        }

        window.removeEventListener('keydown', this.listenForEsc);
        this.active = false;
    }

    updatePosition() {
        if (this.type === "input")
            this.x = this.parent.x + 2;

        if (this.type === "output")
            this.x = this.parent.x + this.parent.w - 2;

        this.y = this.parent.y + (this.parent.h / 2) + (handleHeight / 2);
    }

    draw(ctx) {
        if (this.type == "output")
            draw.semiCircle(ctx, this.x, this.y, this.r, Math.PI * 0.5, Math.PI * 1.5, this.sel, this.col);
        else
            draw.semiCircle(ctx, this.x, this.y, this.r, Math.PI * 1.5, Math.PI * 2.5, this.sel, this.col);

        if (this.active)
            draw.line(ctx, this.x, this.y, mouseposition.x, mouseposition.y, window.flowTheme.colours.secondary);
    }

    listenForEsc(e) {
        if (e.keyCode == 27) { // 27 = esc key.
            this.active = false;
            window.removeEventListener('keydown', this.listenForEsc);
        }
    }
}