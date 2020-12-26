using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using AeoGraphing.Charting;
using AeoGraphing.Charting.ColorGenerators;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Data;

namespace reGraph.Charting.PieChart
{
  public class PieChart : Chart
  {
    private PieChartStyle _style { get; set; }

    public static PieChartStyle DefaultStyle => new PieChartStyle
    {
      Padding = 10,
      TextColor = Color.DarkGray,
      BackgroundColor = Color.WhiteSmoke,
      TitleFont = new Font("Arial", 28),
      DescriptionFont = new Font("Arial", 18),
      AxisCaptionFont = new Font("Arial", 16),
      DataCaptionFont = new Font("Arial", 14),
      AxisLineStyle = new LineStyle { Color = Color.DarkGray, Type = LineType.Solid, Width = 2 },
      DataCaptionPadding = 5,
      NumericFormat = "0.00",
      DrawTitle = true,
      DrawDescription = true,
      StyleName = "DefaultStyle",
      HeightPadding = 10,
      WidthPadding = 10,
      DataColors = new PastelGenerator(Color.LightGray),
      DrawDataLabels = true,
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.DarkGray } },
      DataLabelPadding = new Measure(0.05F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      FullCircleDegrees = 320F,
      RenderCircleDescription = true,
      CircleInnerSpace = 0.2F
    }; 



    protected override float chartTop => base.chartTop + (_style.DrawDataLabels ? _style.DataLabelPadding.GetFloatValue(Height) : 0);
    protected float chartHeight => Height - (2 * _style.HeightPadding.GetFloatValue(this.Height)) - chartTop - (_style.DrawDataLabels ? _style.DataLabelPadding.GetFloatValue(Height) : 0);
    protected float chartWidth => Width - (2 * _style.WidthPadding.GetFloatValue(this.Width));
    protected float chartMinSide => Math.Min(chartWidth, chartHeight);
    protected PointF chartMiddle => new PointF(Width / 2, (chartHeight / 2) + chartTop);
    protected int circleCount => DataSource.DataSeries.Max(x => x.DataPoints.Count); /* + (chartMinSide == 0F ? 0 : 1);*/
    protected float singleCircleRadius => ((chartMinSide / 2) * (1F - _style.CircleInnerSpace)) / circleCount;
    protected float rotateBy => 360F - _style.FullCircleDegrees;



    private PointF getPointOnLine(int circle, double angle)
    {
      var radius = getCirclePoint(circle);
      var x = chartMiddle.X + (Math.Cos(angle) * radius);
      var y = chartMiddle.Y + (Math.Sin(angle) * radius);
      return new PointF((float)x, (float)y);
    }



    private float circleMaxValue(int circle)
    {
      return (float)DataSource.DataSeries.Sum(x => x.DataPoints.Count > circle ? x.DataPoints[circle].Value : 0);
    }



    private float getPointAngle(DataPoint point, int circle)
    {
      var circleMax = circleMaxValue(circle);
      return (float)((degToRad(_style.FullCircleDegrees) / circleMax) * point?.Value ?? 0);
    }



    public PieChart(DataCollection data, PieChartStyle style, int width, int height) : base(data, style, width, height)
    {
      this._style = style;
    }




    protected virtual float getCirclePoint(int circle)
    {
      return (circle * singleCircleRadius) + ((chartMinSide / 2) * _style.CircleInnerSpace);
    }




    protected virtual void renderCircles(Graphics graphics)
    {
      var pen = _style.AxisLineStyle.GetPen(chartMinSide);
      for (int i = 0; i < circleCount + 1; i++)
      {
        var lowerLine = getCirclePoint(i);
        var lowerCircleBB = new RectangleF(chartMiddle.X - lowerLine, chartMiddle.Y - lowerLine, lowerLine * 2, lowerLine * 2);

        graphics.DrawArc(pen, lowerCircleBB, -(90 - rotateBy), _style.FullCircleDegrees);
        // graphics.DrawCircle(pen, chartMiddle, getCirclePoint(i));
      }

      if (_style.FullCircleDegrees < 360F)
      {
        float start = (float)(1.5 * Math.PI) + degToRad(rotateBy);
        var lower = getPointOnLine(0, start);
        var upper = getPointOnLine(circleCount, start);
        graphics.DrawLine(pen, lower, upper);

        lower = getPointOnLine(0, start + degToRad(_style.FullCircleDegrees));
        upper = getPointOnLine(circleCount, start + degToRad(_style.FullCircleDegrees));
        graphics.DrawLine(pen, lower, upper);
      }
    }



    protected virtual void renderCircleLabel(Graphics graphics)
    {
      var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      for (int i = 0; i < circleCount; i++)
      {
        var lowerLine = getCirclePoint(i);
        var upperLine = getCirclePoint(i + 1);
        var label = DataSource.DataSeries.OrderByDescending(x => x.DataPoints.Count).First().DataPoints[i].BaseLabel;

        var measure = graphics.MeasureString(label, _style.AxisCaptionFont);
        var rect = new RectangleF(chartMiddle.X, chartMiddle.Y - upperLine, measure.Width, upperLine - lowerLine);
        graphics.DrawString(label, _style.AxisCaptionFont, new SolidBrush(_style.TextColor), rect, format);
      }
    }



    protected void renderCircleData(Graphics graphics)
    {
      for (int i = 0; i < circleCount; i++)
        renderDataCircle(graphics, i);
    }



    protected float degToRad(float deg)
    {
      return (float)(Math.PI / 180F) * deg;
    }


    protected float radToDeg(float rad)
    {
      return (float)(rad * 180F / Math.PI);
    }



    protected virtual void renderDataCircle(Graphics graphics, int circle)
    {
      var points = DataSource.DataSeries.Select(x => x.DataPoints.Count > circle ? x.DataPoints[circle] : null).ToList();
      _style.DataColors.Reset();
      float lastAngle = (float)(1.5 * Math.PI) + degToRad(rotateBy);
      foreach (var point in points)
      {
        var color = _style.DataColors.Current;
        _style.DataColors.MoveNext();
        var angleDelta = getPointAngle(point, circle);
        if (angleDelta == 0)
          continue;

        var path = new GraphicsPath();

        var lowerLine = getCirclePoint(circle);
        var upperLine = getCirclePoint(circle + 1);

        var lower = getPointOnLine(circle, lastAngle);
        var upper = getPointOnLine(circle + 1, lastAngle);

        var lowerEnd = getPointOnLine(circle, angleDelta + lastAngle);
        var upperEnd = getPointOnLine(circle + 1, angleDelta + lastAngle);

        var lowerCircleBB = new RectangleF(chartMiddle.X - lowerLine, chartMiddle.Y - lowerLine, lowerLine * 2, lowerLine * 2);
        var upperCircleBB = new RectangleF(chartMiddle.X - upperLine, chartMiddle.Y - upperLine, upperLine * 2, upperLine * 2);

        path.AddLine(lower, upper);
        path.AddArc(upperCircleBB, radToDeg(lastAngle), radToDeg(angleDelta));
        path.AddLine(upperEnd, lowerEnd);
        path.AddArc(lowerCircleBB, radToDeg(lastAngle), radToDeg(angleDelta));

        graphics.FillPath(new SolidBrush(color), path);

        lastAngle += angleDelta;
      }
    }



    public override void SetStyle(ChartStyle style)
    {
      base.SetStyle(style);

      if (style is PieChartStyle ps)
      {
        _style = ps;
      }
    }



    protected override void render(Graphics graphics)
    {
      graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
      renderCircleData(graphics);
      renderCircles(graphics);
      if (_style.RenderCircleDescription)
        renderCircleLabel(graphics);
    }
  }
}
