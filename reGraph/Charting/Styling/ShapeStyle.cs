using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AeoGraphing.Charting.Styling
{
  public class ShapeStyle
  {
    public Measure Width { get; set; }
    public Color Color { get; set; }
  }

  public class BorderedShapeStyle : ShapeStyle
  {
    public ShapeStyle Border { get; set; }
  }
}
