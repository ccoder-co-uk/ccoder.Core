class ConsoleDialog extends Dialog {
    constructor(args) {
        super(args);
        this.width = 800 || args.width;
        this.height = 500 || args.height;
        this.title = "Console" || args.title;
        this.template = `
            <div class='console' name='console' style='overflow: auto;position: relative;height: 500px;'>
               <style>
                  [name=console] { padding: 5px; }
                  [name=console] > .message { }
                  [name=console] > .message > * { vertical-align: top; }
                  [name=console] > .message > .message { display: inline-block; border: none; max-width: 90%; word-wrap: break-word; }
                  [name=console] > .message .time { margin-right: 10px; }
                  [name=console] > .message.success > .message { color: green; }
                  [name=console] > .message.info > .message { color: green; }
                  [name=console] > .message.debug > .message { color: blue; }
                  [name=console] > .message.warning > .message { color: #D8A700; }
                  [name=console] > .message.error > .message { color: red; }
                  [name=console] > .message.fatal > .message { color: red; }
               </style>
            </div>
            `;
    }
    
    log(level, message) {
        var d = new Date();
        var time = d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds();
        $("[name=console]", this.element).append($("<div class='message " + level + "'><span class='time'>" + time + "</span><pre class='message'>" + html.encode(message) + "</pre></div>"));
        $("[name=console]", this.element).scrollTop($("[name=flowConsole]", this.element).height());
    }
}