using AeoGraphing.Charting.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace reGraph.Charting.BarChart
{
    public class BarChartStyle : Chart2DStyle
    {
        public Measure BarGroupPadding { get; set; }
        public Measure BarInGroupPadding { get; set; }
        public Measure MaxBarWidth { get; set; }
        public bool DrawValueLabelAboveBar { get; set; }
    }
}
