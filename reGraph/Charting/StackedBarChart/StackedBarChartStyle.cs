using System;
using System.Collections.Generic;
using System.Text;
using AeoGraphing.Charting.Styling;

namespace reGraph.Charting.StackedBarChart
{
  public class StackedBarChartStyle : Chart2DStyle
  {
    public Measure BarPadding { get; set; }
    public Measure MaxBarWidth { get; set; }
    public bool DrawValueLabelInBar { get; set; }
  }
}
