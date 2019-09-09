using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AeoGraphing.Charting.Styling
{
  public class Chart2DStyle : ChartStyle
  {
    public LineStyle ThinLineStyle { get; set; }
    public LineStyle AxisLineStyle { get; set; }
    public LineStyle AxisTicksLineStyle { get; set; }

    public Axis2D DrawAxis { get; set; }
    public Axis2D DrawAxisCaption { get; set; }
    public Axis2D DrawAxisTicks { get; set; }
    public Axis2D DrawAxisHelpLine { get; set; }

    public Measure AxisTicksLength { get; set; }
    public Measure AxisXPosition { get; set; }
    public Measure AxisYPosition { get; set; }
    public Measure DataLabelsPosition { get; set; }
    public Measure DataLabelPadding { get; set; }
    public Measure DataLabelSquarePadding { get; set; }
    public BorderedShapeStyle DataLabelSquare { get; set; }
    public bool DrawDataLabels { get; set; }
  }

  [Flags]
  public enum Axis2D
  {
    None = 0x0,
    AxisX = 0x1,
    AxisY = 0x2,
  }
}
