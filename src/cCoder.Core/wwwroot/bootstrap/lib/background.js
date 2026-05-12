class BackgroundAnimation {

    init(primaryColour, secondaryColour, baseColour, elementCount, linkDistance) {
        this.primaryColour = primaryColour || "red";
        this.secondaryColour = secondaryColour || "blue";
        this.baseColour = baseColour || "white";
        this.elementCount = elementCount || 130;
        this.linkDistance = linkDistance || 140;
        this.elements = [];
        this.lines = [];
        this.initCanvas();

        this.addElements();
        this.draw(this);
    }

    initCanvas() {
        var canvas = this.canvas || document.createElement("canvas");
        document.body.prepend(canvas);
        canvas.style.position = "absolute";
        canvas.style.top = 0;
        canvas.style.left = 0;
        canvas.style.zIndex = -1;
        canvas.width = "100%";
        canvas.height = "100%";
        canvas.style.pointerEvents = "none";
        this.canvas = canvas;
    }

    draw(that) {
        var ctx = that.canvas.getContext("2d");
        ctx.fillStyle = that.baseColour;
        ctx.rect(0, 0, that.canvas.width, that.canvas.height);
        ctx.fill();

        for (let el in that.elements) {
            that.drawElement(ctx, that.elements[el]);
        }

        that.elements.map(function(e) {
            if (e.y < -10) { e.y = that.canvas.height + e.r; }
            if (e.x < -10) { e.x = that.canvas.width + e.r; }
            if (e.x > that.canvas.width + 10) { e.x = 0 - e.r; }
            if (e.y > that.canvas.height + 10) { e.y = 0 - e.r; }
        });

        window.requestAnimationFrame(() => that.draw(that));
    }

    addElements() {
        for (var i = 0; i < this.elementCount; i++) {
            this.elements.push({
                i: i,
                x: (Math.random() * (this.canvas.width + 10)) - 10,
                y: (Math.random() * (this.canvas.height + 10)) - 10,
                r: (Math.random() * 2) + 0.5,
                dir: {
                    x: Math.random() * (Math.floor(Math.random() * 2) == 1 ? 1 : -1),
                    y: Math.random() * (Math.floor(Math.random() * 2) == 1 ? 1 : -1)
                },
                col: Math.random() > 0.5 ? this.primaryColour : this.secondaryColour
            });
        }
    }

    drawElement(ctx, e) {
        var that = this;
        ctx.beginPath();
        ctx.arc(e.x, e.y, e.r, 0, 2 * Math.PI);
        ctx.fillStyle = e.col;
        ctx.fill();

        var nearby = that.elements.filter((el, i) => that.getNearbyElements(that, e, el, i));

        that.lines = that.lines.concat(nearby.map(function(n) { return { from: e.i, to: n.i }; }));

        nearby.map(function(n) { that.drawLine(ctx, e, n); });

        e.x += (e.dir.x * 0.2) + 0.1;
        e.y += (e.dir.y * 0.2) + 0.1;
    }

    getNearbyElements(anim, e, el, i) {
        var xDif = Math.abs(el.x - e.x);
        var yDif = Math.abs(el.y - e.y);

        return xDif + yDif <= anim.linkDistance && (xDif + yDif !== 0) && !anim.lines.some(function(l) { return l.from.i === e.i && l.to.i === el.i; });
    }

    drawLine(ctx, n1, n2) {
        ctx.beginPath();
        ctx.moveTo(n1.x, n1.y);
        ctx.lineTo(n2.x, n2.y);
        var grd = ctx.createLinearGradient(n1.x, n1.y, n2.x, n2.y);
        grd.addColorStop(0, n1.col);
        grd.addColorStop(1, n2.col);
        ctx.strokeStyle = grd;
        ctx.lineWidth = 0.2;
        ctx.stroke();
    }
}
