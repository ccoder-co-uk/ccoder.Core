class Action extends Collidable {
    constructor(parent, text, offsetX, offsetY, height, width, col, onClick) {
        super(text, parent.x + offsetX, parent.y + offsetY, height, width, 0);
        this.type = 'Action';
        this.parent = parent;
        this.flow = this.parent.flow;
        this.text = text;
        this.col = col;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.updatePosition();
        this.click = onClick;
    }

    updatePosition() {
        this.x = this.parent.x + this.offsetX;
        this.y = this.parent.y + this.offsetY;
    }

    mouseup(e) {
        this.click(e);
    }

    draw(ctx) {
        draw.text(ctx, this.x + 10, this.y + 15, this.text, this.col);
    }
}