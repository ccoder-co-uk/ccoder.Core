class PieChart extends Widget {
    constructor(element, data) {
        super(element);
        this.data = data;
        this.chartName = $(element).attr("name");
        $(element).append("<div name='" + this.gridName + "PieChart' style='width:100%; height:100%; display: block;'></div>");
        this.chartElement = $("[name=" + this.gridName + "PieChart]", $(element));
    }
    
    init() {
        this.kendoObject = $(this.chartElement).kendoChart({
            seriesDefaults: {
                labels: {
                    visible: true,
                    background: "transparent",
                    template: "#= category #: \n #= kendo.toString(value, type.aggregateMoneyFormat)#",
                }
            },
            legend: {
                align: "right"
            },
            series: [{
                type: "pie",
                startAngle: 150,
                data: this.data,
                padding: 60
            }]
        }).data("kendoChart");
    }
    
    setData(data) {
        this.kendoObject.options.series[0].data = data;
    }
    
    refresh() {
        this.kendoObject.refresh();
    }
}