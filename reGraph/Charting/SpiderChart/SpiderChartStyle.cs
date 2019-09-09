using System;
using System.Collections.Generic;
using System.Text;
using AeoGraphing.Charting.Styling;

namespace reGraph.Charting.SpiderChart
{
    public class SpiderChartStyle : ChartStyle
    {
        public Measure WidthPadding { get; set; }
        public Measure HeightPadding { get; set; }
        public LineStyle ThinLineStyle { get; set; }
        public LineStyle AxisLineStyle { get; set; }
        public LineStyle DataConnectionLineStyle { get; set; }
        public BorderedShapeStyle DataDotStyle { get; set; }
    }
}
