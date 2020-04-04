using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using AeoGraphing.Charting;
using AeoGraphing.Charting.ColorGenerators;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Data;

namespace reGraph.Charting.SpiderChart
{
  public class SpiderChart : Chart
  {
    const double HALF_PI = Math.PI / 2;
    public static SpiderChartStyle DefaultStyle => new SpiderChartStyle
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
      ThinLineStyle = new LineStyle { Color = Color.LightGray, Type = LineType.Dashed, Width = 1 },
      DrawTitle = true,
      DrawDescription = true,
      StyleName = "DefaultStyle",
      HeightPadding = 10,
      WidthPadding = 10,
      DataColors = new PastelGenerator(Color.LightGray),
      DataConnectionLineStyle = new LineStyle { Color = Color.Transparent, Type = LineType.Solid, Width = 3 },
      DataDotStyle = new BorderedShapeStyle { Color = Color.DarkGray, Width = 3, Border = new ShapeStyle { Color = Color.Transparent, Width = 5 } },
      DrawDataLabels = true,
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.DarkGray } },
      DataLabelPadding = new Measure(0.05F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      FillAreaOfDataSeries = true,
      AreaFillAlpha = 80,
      AxisCaptionDistance = 0.01F,
      DrawValueLabels = true
    };

    public SpiderChart(DataCollection data, SpiderChartStyle style, int width, int height) : base(data, style, width, height)
    {
      this._style = style;
      this.ValueSteps = 0.1F;
    }


    private SpiderChartStyle _style;
    public override ChartStyle Style => _style;

    protected int pointCount => DataSource.DataSeries.Min(x => x.DataPoints.Count);
    protected override float chartTop => base.chartTop + (_style.DrawDataLabels ? _style.DataLabelPadding.GetFloatValue(Height) : 0);
    protected float chartHeight => Height - (2 * _style.HeightPadding.GetFloatValue(this.Height)) - chartTop - (_style.DrawDataLabels ? _style.DataLabelPadding.GetFloatValue(Height) : 0);
    protected float chartWidth => Width - (2 * _style.WidthPadding.GetFloatValue(this.Width));
    protected float chartMinSide => Math.Min(chartWidth, chartHeight);
    protected float lineLength => chartMinSide / 2;
    protected PointF chartMiddle => new PointF(Width / 2, (chartHeight / 2) + chartTop);
    protected float ValueSteps { get; set; }



    private PointF getPointOnLine(int lineIndex, float radiusPercent)
    {
      var steps = (2 * Math.PI) / pointCount;
      var angle = steps * lineIndex;
      angle -= HALF_PI;
      var radius = lineLength * radiusPercent;
      var x = chartMiddle.X + (Math.Cos(angle) * radius);
      var y = chartMiddle.Y + (Math.Sin(angle) * radius);
      return new PointF((float)x, (float)y);
    }



    private float scaleValue(DataPoint point)
    {
      return (float)((point.Value - DataSource.MinValue) / DataSource.ScaledMaxValue);
    }



    private void drawDataPoint(Graphics graphics, PointF loc, Color color)
    {
      if (_style.DataDotStyle.Border != null)
      {
        var bwidth = _style.DataDotStyle.Border.Width.GetFloatValue(this.Width);
        graphics.FillCircle(new SolidBrush(_style.DataDotStyle.Border.Color.ReplaceIfTransparent(color)), loc.X, loc.Y, bwidth);
      }

      var width = _style.DataDotStyle.Width.GetFloatValue(this.Width);
      graphics.FillCircle(new SolidBrush(_style.DataDotStyle.Color.ReplaceIfTransparent(color)), loc.X, loc.Y, width);
    }



    private void renderData(Graphics graphics, DataSeries series, Color color)
    {
      if (series.DataPoints.Count == 0)
        return;

      var pen = _style.DataConnectionLineStyle.GetPen(this.chartMinSide);
      pen.Color = pen.Color.ReplaceIfTransparent(color);
      var firstPoint = getPointOnLine(0, scaleValue(series.DataPoints[0]));
      var lastPoint = firstPoint;
      for (int i = 1; i < series.DataPoints.Count; i++)
      {
        var nextPoint = getPointOnLine(i, scaleValue(series.DataPoints[i]));
        graphics.DrawLine(pen, lastPoint, nextPoint);
        drawDataPoint(graphics, lastPoint, color);
        lastPoint = nextPoint;
      }

      graphics.DrawLine(pen, lastPoint, firstPoint);
      drawDataPoint(graphics, lastPoint, color);
      drawDataPoint(graphics, firstPoint, color);
    }



    private void renderSeries(Graphics graphics)
    {
      var colors = _style.DataColors;
      colors.Reset();
      foreach (var series in DataSource.DataSeries)
      {
        renderData(graphics, series, colors.Current);
        colors.MoveNext();
      }
    }



    private void renderValueLabel(Graphics graphics)
    {
      for (float radPercent = ValueSteps; radPercent < 1 + ValueSteps; radPercent += ValueSteps)
        renderValueLabel(graphics, radPercent);

      renderValueLabel(graphics, 0);
    }



    private void renderHelplines(Graphics graphics)
    {
      for (float radPercent = ValueSteps; radPercent < 1 + ValueSteps; radPercent += ValueSteps)
      {
        var lastPoint = getPointOnLine(0, radPercent);
        for (int i = 1; i <= pointCount; i++)
        {
          var nextPoint = getPointOnLine(i, radPercent);
          graphics.DrawLine(_style.ThinLineStyle.GetPen(chartMinSide), lastPoint, nextPoint);
          lastPoint = nextPoint;
        }
      }
    }



    private void renderAxisLabel(Graphics graphics, int index, string label)
    {
      var measure = graphics.MeasureString(label, _style.AxisCaptionFont);
      var origin = getPointOnLine(index, _style.AxisCaptionDistance + 1F);
      var point = origin;
      var rect = new RectangleF(point, measure);
      var mid = chartMiddle;

      var pen = new Pen(Color.Red, 1F);

      // get corner of rectangle which would move the rect furtest away from the axis
      if (point.X == mid.X && point.Y < mid.Y)
        point = rect.BottomCenter();
      else if (point.X > mid.X && point.Y < mid.Y)
        point = rect.BottomLeft();
      else if (point.X > mid.X && point.Y == mid.Y)
        point = rect.CenterLeft();
      else if (point.X > mid.X && point.Y > mid.Y)
        point = rect.TopLeft();
      else if (point.X == mid.X && point.Y > mid.Y)
        point = rect.TopCenter();
      else if (point.X < mid.X && point.Y > mid.Y)
        point = rect.TopRight();
      else if (point.X < mid.X && point.Y == mid.Y)
        point = rect.CenterRight();
      else if (point.X < mid.X && point.Y < mid.Y)
        point = rect.BottomRight();

      // translate the rectangle to the new location
      rect.X += origin.X - point.X;
      rect.Y += origin.Y - point.Y;


      //graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
      //graphics.DrawCircle(pen, rect.TopCenter(), 3);
      //graphics.DrawCircle(pen, rect.TopRight(), 3);
      //graphics.DrawCircle(pen, rect.CenterRight(), 3);
      //graphics.DrawCircle(pen, rect.BottomRight(), 3);
      //graphics.DrawCircle(pen, rect.BottomCenter(), 3);
      //graphics.DrawCircle(pen, rect.BottomLeft(), 3);
      //graphics.DrawCircle(pen, rect.CenterLeft(), 3);
      //graphics.DrawCircle(pen, rect.TopLeft(), 3);

      graphics.DrawString(label, _style.AxisCaptionFont, new SolidBrush(_style.TextColor), rect);
    }



    private void renderValueLabel(Graphics graphics, float percentage)
    {
      var value = (DataSource.ScaledMaxValue - DataSource.MinValue) * percentage;
      var point = getPointOnLine(0, percentage);
      var text = value.ToString(_style.NumericFormat);
      var measure = graphics.MeasureString(text, _style.DataCaptionFont);
      var rectangle = new RectangleF(point.X, point.Y - (measure.Height / 2F), measure.Width, measure.Height);
      graphics.DrawString(text, _style.DataCaptionFont, new SolidBrush(_style.TextColor), rectangle, new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
    }



    private string getLabel(int index)
    {
      if (index < DataSource.DataSeries[0].DataPoints.Count)
        return DataSource.DataSeries[0].DataPoints[index].BaseLabel;

      return string.Empty;
    }



    private void renderAxis(Graphics graphics)
    {
      var steps = (2 * Math.PI) / pointCount;
      int index = 0;
      for (double i = -HALF_PI; i < (HALF_PI * 3); i += steps)
      {
        var middlePoint = chartMiddle;
        var edgePoint = new PointF((float)(middlePoint.X + (Math.Cos(i) * lineLength)), (float)(middlePoint.Y + (Math.Sin(i) * lineLength)));
        graphics.DrawLine(_style.AxisLineStyle.GetPen(chartMinSide), middlePoint, edgePoint);

        if (_style.DrawDataLabels)
          renderAxisLabel(graphics, index, getLabel(index++));
      }
    }



    private void renderFilledAreas(Graphics graphics)
    {
      var colors = _style.DataColors;
      colors.Reset();
      foreach (var series in DataSource.DataSeries)
      {
        renderFilledArea(graphics, series, colors.Current);
        colors.MoveNext();
      }
    }



    private void renderFilledArea(Graphics graphics, DataSeries series, Color color)
    {
      var shape = new GraphicsPath();
      var firstPoint = getPointOnLine(0, scaleValue(series.DataPoints[0]));
      var lastPoint = firstPoint;
      for (int i = 1; i < series.DataPoints.Count; i++)
      {
        var point = getPointOnLine(i, scaleValue(series.DataPoints[i]));
        shape.AddLine(lastPoint, point);
        lastPoint = point;
      }

      color = Color.FromArgb(_style.AreaFillAlpha, color);
      graphics.FillPath(new SolidBrush(color), shape);
    }



    public override void SetStyle(ChartStyle style)
    {
      if (style is SpiderChartStyle spiderStyle)
      {
        base.SetStyle(style);
        _style = spiderStyle;
      }
    }



    protected override void render(Graphics graphics)
    {
      renderAxis(graphics);

      if (_style.FillAreaOfDataSeries)
        renderFilledAreas(graphics);

      renderSeries(graphics);
      renderHelplines(graphics);

      if (_style.DrawValueLabels)
        renderValueLabel(graphics);
    }
  }
}
