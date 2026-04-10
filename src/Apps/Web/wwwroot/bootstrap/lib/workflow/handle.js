class Handle extends Collidable {
    constructor(parent, col, textCol) {
        super(parent.model.Ref, parent.x, parent.y, parent.w - 1, handleHeight, 0);

        if (this.text.length > 25) { this.text = this.text.substring(0, 25) + "..."; }

        this.type = 'handle';
        this.col = col;
        this.textCol = textCol || $('h2').css('color');
        this.parent = parent;
        this.activity = parent;
        this.flow = this.parent.flow;
        this.objects = [new Close(this)]; //, new EditProps(this) ];
    }

    draw(ctx) {
        draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#CCC", 1);
        draw.rect(ctx, this.x, this.y, this.w, this.h, this.col);
        draw.disableShadows(ctx);

        draw.text(ctx, this.x + 8, this.y + 17, this.text, this.textCol);
        this.objects.forEach(o => o.draw(ctx));
    }

    mousedown(e) {
        this.moving = true;
        this.start = mouseposition;
    }

    mouseup(e) {
        this.moving = false;
    }

    updatePosition() {
        this.x = this.parent.x;
        this.y = this.parent.y;
        this.objects.forEach(o => o.updatePosition());
    }

    move(e) {
        if (this.moving) {
            this.parent.setPosition(
                this.x + (mouseposition.x - this.start.x),
                this.y + (mouseposition.y - this.start.y)
            );
            this.start = mouseposition;
        }
    }
}