using AeoGraphing.Charting.Styling;
using System;
using System.Collections.Generic;
using System.Text;

namespace AeoGraphing.Charting.LineChart
{
  public class LineChartStyle : Chart2DStyle
  {
    public LineStyle DataConnectionLineStyle { get; set; }
    public LineStyle GroupLineStyle { get; set; }
    public Measure GroupingNameSpace { get; set; }
    public BorderedShapeStyle DataDotStyle { get; set; }
  }
}
