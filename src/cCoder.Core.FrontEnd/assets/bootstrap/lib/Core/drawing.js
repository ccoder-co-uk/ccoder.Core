const draw = {
    rect: function(ctx, x, y, w, h, col, sel, stroke) {
        ctx.beginPath();
        ctx.rect(x, y, w, h);
        ctx.lineWidth = 1;
        ctx.strokeStyle = '#000';

        if (stroke !== false) {
            ctx.stroke();
        }

        ctx.beginPath();
        ctx.rect(x, y, w, h);

        col = ctx.isPointInPath(mouseposition.x, mouseposition.y)
            ? (sel || col || '#cbcbcb')
            : (col || '#cbcbcb');

        ctx.fillStyle = col;
        ctx.fill();
    },

    text: function(ctx, x, y, text, col) {
        ctx.font = "11px Arial";
        ctx.fillStyle = col || '#000';
        ctx.fillText(text, x, y);
    },

    circle: function(ctx, x, y, r, col, sel) {
        ctx.beginPath();
        ctx.arc(x, y, r, 0, 2 * Math.PI);
        const hover = ctx.isPointInPath(mouseposition.x, mouseposition.y);

        col = hover
            ? (sel || col || '#cbcbcb')
            : (col || '#cbcbcb');

        r = hover
            ? (r + 1)
            : r;

        ctx.arc(x, y, r, 0, 2 * Math.PI);
        ctx.fillStyle = col;
        ctx.fill();
    },

    semiCircle: function (ctx, x, y, r, start, end, col, sel) {
        ctx.beginPath();
        ctx.arc(x, y, r, start, end);
        const hover = ctx.isPointInPath(mouseposition.x, mouseposition.y);

        col = hover
            ? (sel || col || '#cbcbcb')
            : (col || '#cbcbcb');

        ctx.arc(x, y, r, start, end);
        ctx.fillStyle = col;
        ctx.fill();
    },

    line: function(ctx, fromX, fromY, toX, toY, col, width) {
        ctx.beginPath();
        ctx.moveTo(fromX, fromY);
        ctx.lineTo(fromX + (width || 19), fromY);
        ctx.lineTo(toX - (width || 19), toY);
        ctx.lineTo(toX, toY);
        ctx.lineWidth = width || 2;
        ctx.strokeStyle = col;
        ctx.stroke();
    },

    enableShadows: function(ctx, x, y, w, h, col, offsetY) {
        ctx.shadowColor = col;
        ctx.shadowBlur = 6;
        ctx.shadowOffsetX =  4;
        ctx.shadowOffsetY = offsetY;

        ctx.strokeRect(x, y, w, h);
    },

    disableShadows: function(ctx) {
        ctx.shadowBlur = 0;
        ctx.shadowOffsetX = 0;
        ctx.shadowOffsetY = 0;
        ctx.shadowColor = null;
    }
};

class Drawable {
    constructor(text, x, y, w, h, r) {
        this.x = x;
        this.y = y;
        this.r = r;
        this.h = h;
        this.w = w;
        this.text = text;
    }

    draw(ctx) { /* Abstract method only */ }
}

class Collidable extends Drawable {

    collides() {
        const withinX = (mouseposition.x < this.x + (this.r || this.w)) && (mouseposition.x > this.x - (this.r || 0));
        const withinY = (mouseposition.y < this.y + (this.r || this.h)) && (mouseposition.y > this.y - (this.r || 0));

        let collision = (withinX && withinY)
            ? this
            : null;

        if (collision) {
            collision = this.objects
                ? this.objects.map(o => o.collides()).filter(r => r)[0] || this
                : this;
        }

        return collision;
    }
}