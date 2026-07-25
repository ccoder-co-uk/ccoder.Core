class Close extends Collidable {
    constructor(parent) {
        super('x', 0, 0, parent.h - 2, parent.h - 2, 0);
        this.type = 'close';
        this.parent = parent;
        this.flow = this.parent.flow;
        this.text = 'x';
        this.col = window.flowTheme.colours.secondary;
        this.sel = '#f00';
        this.updatePosition();
    }

    updatePosition() {
        this.x = this.parent.x + (this.parent.w - this.parent.h) + 1;
        this.y = this.parent.y + 1;
    }

    draw(ctx) {
        draw.rect(ctx, this.x, this.y, this.w, this.h, this.col, this.sel);
        draw.text(ctx, this.x + 10, this.y + 15, this.text, '#fff');
    }

    mouseup(e) {
        this.flow.Activities = this.flow.Activities.filter(a => !Object.is(a, this.parent.activity));
        this.flow.Links = this.flow.Links.filter(l => l.model.Source !== this.parent.activity.model.Ref);
        this.flow.Links = this.flow.Links.filter(l => l.model.Destination !== this.parent.activity.model.Ref);
    }
}