using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AeoGraphing.Charting
{

  public struct HSV
  {
    private double _h;
    private double _s;
    private double _v;

    public HSV(double h, double s, double v)
    {
      this._h = h;
      this._s = s;
      this._v = v;
    }

    public double H 
    {
      get { return this._h; }
      set { this._h = value; }
    }

    public double S
    {
      get { return this._s; }
      set { this._s = value; }
    }

    public double V
    {
      get { return this._v; }
      set { this._v = value; }
    }

    public bool Equals(HSV hsv)
    {
      return (this.H == hsv.H) && (this.S == hsv.S) && (this.V == hsv.V);
    }
  }


  public static class Extensions
  {
    public static void Background(this Graphics g, Size size, Color color)
    {
      g.FillRectangle(new SolidBrush(color), new RectangleF(new PointF(0, 0), size));
    }

    public static void DrawCircle(this Graphics g, Pen pen, PointF center, float radius)
    {
      g.DrawCircle(pen, center.X, center.Y, radius);
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

    public static PointF TopLeft(this RectangleF rect)
    {
      return new PointF(rect.X, rect.Y);
    }

    public static PointF BottomRight(this RectangleF rect)
    {
      return new PointF(rect.X + rect.Width, rect.Y + rect.Height);
    }

    public static PointF BottomLeft(this RectangleF rect)
    {
      return new PointF(rect.X, rect.Y + rect.Height);
    }


    public static PointF TopCenter(this RectangleF rect)
    {
      return new PointF(rect.X + (rect.Width / 2), rect.Y);
    }

    public static PointF BottomCenter(this RectangleF rect)
    {
      return new PointF(rect.X + (rect.Width / 2), rect.Y + rect.Height);
    }

    public static PointF CenterLeft(this RectangleF rect)
    {
      return new PointF(rect.X, rect.Y + (rect.Height / 2));
    }

    public static PointF CenterRight(this RectangleF rect)
    {
      return new PointF(rect.X + rect.Width, rect.Y + (rect.Height / 2));
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

    public static Color ToRgb(this HSV hsv)
    {
      var h = hsv.H % 360;
      var c = hsv.V * hsv.S;
      var x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
      var m = hsv.V - c;

      double r, g, b;

      switch ((int)(h / 60))
      {
        case 0:
          r = c;
          g = x;
          b = 0;
          break;
        case 1:
          r = x;
          g = c;
          b = 0;
          break;
        case 2:
          r = 0;
          g = c;
          b = x;
          break;
        case 3:
          r = 0;
          g = x;
          b = c;
          break;
        case 4:
          r = x;
          g = 0;
          b = c;
          break;
        case 5:
          r = c;
          g = 0;
          b = x;
          break;
        default:
          r = 0;
          g = 0;
          b = 0;
          break;
      }

      r += m;
      g += m;
      b += m;

      return Color.FromArgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }
  }
}
