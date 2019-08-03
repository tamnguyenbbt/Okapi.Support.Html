
var testResult = {
    data: {
        passCount: function () {
            return document.getElementsByName("passed").length;
        },
        failCount: function () {
            return document.getElementsByName("failed").length;
        },
        ignoredCount: function () {
            return document.getElementsByName("ignored").length;
        },
        skippedCount: function () {
            return document.getElementsByName("skipped").length;
        },
        getPieColors: function () {

            var blue = "#08c";
            var red = "#ff0000";
            var darkRed = "#990000";
            var grey = "#808080";
            var defaultColors = [blue, red, darkRed, grey];

            if (this.ignoredCount() == 0) {
                defaultColors[2] = defaultColors[3];
            }

            if (this.failCount() == 0) {
                defaultColors[1] = defaultColors[2];
                defaultColors[2] = defaultColors[3];
            }

            if (this.passCount() == 0) {
                defaultColors[0] = defaultColors[1];
                defaultColors[1] = defaultColors[2];
                defaultColors[2] = defaultColors[3];
            }

            return defaultColors;
        },
        pieColor1: function () {
            return this.getPieColors()[0];
        },
        pieColor2: function () {
            return this.getPieColors()[1];
        },
        pieColor3: function () {
            return this.getPieColors()[2];
        },
        pieColor4: function () {
            return this.getPieColors()[3];
        }
    }
};

var pieConfig = {
    defaultContent: {
        header: {
            title: {
                text: "",
                color: "#333333",
                fontSize: 18,
                font: "arial"
            },
            subtitle: {
                text: "",
                color: "#666666",
                fontSize: 14,
                font: "arial"
            },
            location: "top-center",
            titleSubtitlePadding: 8
        },
        footer: {
            text: "",
            color: "#666666",
            fontSize: 14,
            font: "arial",
            location: "left"
        },
        size: {
            canvasHeight: 400,
            canvasWidth: 550,
            pieInnerRadius: "0%",
            pieOuterRadius: null
        },
        data: {
            sortOrder: "none",
            ignoreSmallSegments: {
                enabled: false,
                valueType: "percentage",
                value: null
            },
            smallSegmentGrouping: {
                enabled: false,
                value: 1,
                valueType: "percentage",
                label: "Other",
                color: "#cccccc"
            },
            content: []
        },
        labels: {
            outer: {
                format: "label-value1",
                pieDistance: 32
            },
            inner: {
                format: "percentage",
                hideWhenLessThanPercentage: null
            },
            mainLabel: {
                color: "#333333",
                font: "arial",
                fontSize: 14
            },
            percentage: {
                color: "#dddddd",
                font: "arial",
                fontSize: 14,
                decimalPlaces: 1
            },
            value: {
                color: "#cccc44",
                font: "arial",
                fontSize: 10
            },
            lines: {
                enabled: true,
                style: "curved",
                color: "segment"
            },
            truncation: {
                enabled: false,
                length: 30
            }
        },
        effects: {
            load: {
                effect: "default",
                speed: 1000
            },
            pullOutSegmentOnClick: {
                effect: "bounce",
                speed: 300,
                size: 10
            },
            highlightSegmentOnMouseover: true,
            highlightLuminosity: -0.2
        },
        tooltips: {
            enabled: true,
            type: "placeholder",
            string: "{label}: {value}",
            placeholderParser: null,
            styles: {
                fadeInSpeed: 250,
                backgroundColor: "#000000",
                backgroundOpacity: 0.5,
                color: "#efefef",
                borderRadius: 2,
                font: "arial",
                fontSize: 10,
                padding: 4
            }
        },
        misc: {
            colors: {
                background: null,
                segments: [
                    testResult.data.pieColor1(), testResult.data.pieColor2(),
                    testResult.data.pieColor3(), testResult.data.pieColor4(),
                    "#961a1a", "#d8d23a", "#e98125", "#d0743c", "#635222", "#6ada6a",
                    "#0c6197", "#7d9058", "#207f33", "#44b9b0", "#bca44a", "#e4a14b", "#a3acb2", "#8cc3e9", "#69a6f9", "#5b388f",
                    "#546e91", "#8bde95", "#d2ab58", "#273c71", "#98bf6e", "#4daa4b", "#98abc5", "#cc1010", "#31383b", "#006391",
                    "#c2643f", "#b0a474", "#a5a39c", "#a9c2bc", "#22af8c", "#7fcecf", "#987ac6", "#3d3b87", "#b77b1c", "#c9c2b6",
                    "#807ece", "#8db27c", "#be66a2", "#9ed3c6", "#00644b", "#005064", "#77979f", "#77e079", "#9c73ab", "#1f79a7"
                ],
                segmentStroke: "#ffffff"
            },
            gradient: {
                enabled: false,
                percentage: 95,
                color: "#000000"
            },
            canvasPadding: {
                top: 5,
                right: 5,
                bottom: 5,
                left: 5
            },
            pieCenterOffset: {
                x: -50,
                y: 0
            },
            cssPrefix: null
        },
        callbacks: {
            onload: null,
            onMouseoverSegment: null,
            onMouseoutSegment: null,
            onClickSegment: null
        },
        data: {
            content: [
                { label: "Passed", value: Number(testResult.data.passCount()) },
                { label: "Failed", value: Number(testResult.data.failCount()) },
                { label: "Ignored", value: Number(testResult.data.ignoredCount()) },
                { label: "Skipped", value: Number(testResult.data.skippedCount()) }
            ]
        }
    }
}

var pieReport = new d3pie("pie", pieConfig.defaultContent);