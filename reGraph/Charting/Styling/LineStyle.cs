using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;


namespace AeoGraphing.Charting.Styling
{
  public class LineStyle : ShapeStyle
  {
    public LineType Type { get; set; }

    public Pen GetPen( float toScale )
    {
      switch( Type )
      {
        case LineType.Solid:
          return new Pen( Color, Width.GetFloatValue( toScale ) );
        case LineType.Dotted:
          return new Pen( Color, Width.GetFloatValue( toScale ) ) { DashPattern = new float[] { 1f, 1f } };
        case LineType.Dashed:
          return new Pen( Color, Width.GetFloatValue( toScale ) ) { DashPattern = new float[] { 3f, 1f } };
        case LineType.DashDotted:
          return new Pen( Color, Width.GetFloatValue( toScale ) ) { DashPattern = new float[] { 3f, 1f, 1f, 1f } };
      }

      return null;
    }
  }

  public enum LineType
  {
    Solid,
    Dotted,
    Dashed,
    DashDotted
  }
}
