using System;
using System.Collections.Generic;
using System.Text;
using AeoGraphing.Charting.Styling;

namespace reGraph.Charting.PieChart
{
  public class PieChartStyle : ChartStyle
  {
    public Measure WidthPadding { get; set; }
    public Measure HeightPadding { get; set; }
    public float CircleInnerSpace { get; set; }
    public LineStyle AxisLineStyle { get; set; }
    public float FullCircleDegrees { get; set; }
    public bool RenderCircleDescription { get; set; }
  }
}
