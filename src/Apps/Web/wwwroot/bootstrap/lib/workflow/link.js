class Link extends Collidable {
    constructor(model, flow) {
        super('', 0, 0, 0, 0, 5);
        this.model = model;
        this.flow = flow;
    }

    draw(ctx) {
        let source = this.flow.Activities.filter(a => a.model.Ref === this.model.Source)[0];
        let destination = this.flow.Activities.filter(a => a.model.Ref === this.model.Destination)[0];

        if (source && destination) {
            if (source.out && destination.in) {
                this.x = source.out.x + ((destination.in.x - source.out.x) / 2);
                this.y = source.out.y + ((destination.in.y - source.out.y) / 2);

                draw.enableShadows(ctx, this.x, this.y, this.w, this.h, "#111");
                draw.line(ctx, source.out.x, source.out.y, destination.in.x, destination.in.y, '#37e83a');
                draw.circle(ctx, this.x, this.y, this.r, '#37e83a', '#00ff04');
                draw.disableShadows(ctx);
            }
        }
    }

    mouseup(e) {
        let that = this;

        let dialog = $('<div><div name="content"></div></div>')
            .appendTo(document.body);

        let d = dialog.kendoWindow({
            title: 'Expression Editor',
            modal: true,
            visible: false,
            resizable: false,
            width: '1000px',
            close: function() { this.destroy(); }
        }).data("kendoWindow");

        d.wrapper.css({ top: 140, left: '20%' });
        d.open();

        let container = $("[name=content]", $(d.element));

        loadComponent(container, "ExpressionBuilder", function(c) {
            let builderParams = {
                expression: that.model.Expression,
                args: {
                    destination: that.flow.Activities
                        .filter(a => a.model.Ref === that.model.Destination)[0]
                        .meta.ServerTypeName,
                    source: that.flow.Activities
                        .filter(a => a.model.Ref === that.model.Source)[0]
                        .meta.ServerTypeName,
                    variables: "IDictionary<String,Object>",
                    flow: "Flow"
                }
            };

            ExpressionBuilder.init(null, container, builderParams, function(expression) {
                that.model.Expression = expression;
                d.close();
            });
        });
    }
}