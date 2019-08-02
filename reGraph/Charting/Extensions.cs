using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AeoGraphing.Charting
{
  public static class Extensions
  {
    public static void Background(this Graphics g, Size size, Color color)
    {
      g.FillRectangle(new SolidBrush(color), new RectangleF(new PointF(0, 0), size));
    }

    public static void DrawCircle(this Graphics g, Pen pen, float centerX, float centerY, float radius)
    {
      g.DrawEllipse(pen, centerX - radius, centerY - radius, radius + radius, radius + radius);
    }

    public static void FillCircle(this Graphics g, Brush brush, float centerX, float centerY, float radius)
    {
      g.FillEllipse(brush, centerX - radius, centerY - radius, radius + radius, radius + radius);
    }

    public static PointF TopRight(this RectangleF rect)
    {
      return new PointF(rect.X + rect.Width, rect.Y);
    }

    public static Color Mix(this Color color, Color mixin)
    {
      return Color.FromArgb((color.R + mixin.R) / 2, (color.G + mixin.G) / 2, (color.B + mixin.B) / 2);
    }

    public static Color ReplaceIfTransparent(this Color color, Color replace)
    {
      return color == Color.Transparent ? replace : color;
    }

    public static double SquareSub(this byte @byte, byte with)
    {
      return (@byte - with) * (@byte - with);
    }

    public static double Distance(this Color color, Color to)
    {
      return Math.Abs(Math.Sqrt(color.R.SquareSub(to.R) + color.G.SquareSub(to.G) + color.B.SquareSub(to.B)));
    }
  }
}
