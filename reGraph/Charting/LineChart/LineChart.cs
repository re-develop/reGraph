using AeoGraphing.Charting.ColorGenerators;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Data;
using reGraph.Charting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace AeoGraphing.Charting.LineChart
{
  public class LineChart : Chart2D
  {
    public static LineChartStyle DefaultStyle => new LineChartStyle()
    {
      Padding = 10,
      TextColor = Color.DarkGray,
      BackgroundColor = Color.WhiteSmoke,
      TitleFont = new Font("Arial", 28),
      DescriptionFont = new Font("Arial", 18),
      AxisCaptionFont = new Font("Arial", 16),
      DataCaptionFont = new Font("Arial", 14),
      AxisLineStyle = new LineStyle { Color = Color.DarkGray, Type = LineType.Solid, Width = 2 },
      AxisTicksLineStyle = new LineStyle { Color = Color.DarkGray, Type = LineType.Solid, Width = 2 },
      AxisTicksLength = 5,
      AxisXPosition = new Measure(0.15F, MeasureType.Percentage),
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.DarkGray } },
      AxisYPosition = new Measure(0.05F, MeasureType.Percentage),
      DataCaptionPadding = 5,
      DataConnectionLineStyle = new LineStyle { Color = Color.Transparent, Type = LineType.Solid, Width = 3 },
      DataDotStyle = new BorderedShapeStyle { Color = Color.Transparent, Width = 2, Border = new ShapeStyle { Color = Color.Transparent, Width = 2 } },
      DrawAxis = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisCaption = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisHelpLine = Axis2D.AxisX,
      DrawAxisTicks = Axis2D.AxisX | Axis2D.AxisY,
      NumericFormat = "0.00",
      ThinLineStyle = new LineStyle { Color = Color.LightGray, Type = LineType.Dashed, Width = 1 },
      DrawTitle = true,
      DrawDescription = true,
      DrawDataLabels = true,
      DataLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      DataColors = new PastelGenerator(Color.LightGray),
      GroupingNameSpace = new Measure(0.05F, MeasureType.Percentage),
      GroupLineStyle = new LineStyle { Color = Color.DarkGray, Type = LineType.DashDotted, Width = 1 }
    };


    public LineChartStyle _style;
    protected override float chartTop => base.chartTop + (DataSource.HasGrouping ? _style.GroupingNameSpace.GetFloatValue(this.Height) : 0);



    public LineChart(DataCollection dataSource, LineChartStyle style, int width, int height) : base(dataSource, style, width, height)
    {
      _style = style;
    }


    private void drawDataPoints(Graphics graphics)
    {
      _style.DataColors.Reset();
      foreach (var series in DataSource.DataSeries)
      {
        var color = _style.DataColors.Current;
        drawDataPoints(graphics, series, color);
        _style.DataColors.MoveNext();
      }
    }

    private void drawDataGroups(Graphics ctx)
    {
      if (DataSource.HasGrouping == false)
        return;

      var pen = _style.GroupLineStyle.GetPen(this.Width);
      StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      float lastX = valueLineWidth;
      int i = 0;
      for (; i < DataSource.DataGroupValues.Count; i++)
      {
        var x = (float)(pixelPerBaseValue * (DataSource.DataGroupValues[i] - DataSource.MinBaseValue)) + valueLineWidth;
        if (x <= valueLineWidth || x > paddedWidth)
          continue;

        float width = x - lastX;

        ctx.DrawLine(pen, new PointF(x, chartTop), new PointF(x, baseLinePos));
        if (DataSource.DataGroupNames.Count > i - 1)
        {
          var heigth = _style.GroupingNameSpace.GetFloatValue(this.Height);
          var rect = new RectangleF(lastX, chartTop - heigth, width, heigth);
          ctx.DrawString(DataSource.DataGroupNames[i - 1], _style.DataCaptionFont, new SolidBrush(_style.TextColor), rect, stringFormat);
        }

        lastX = x;
      }

      var dwidth = paddedWidth - lastX;
      var requiredWidth = ctx.MeasureString(DataSource.DataGroupNames[i - 1], _style.DataCaptionFont).Width;

      if (DataSource.DataGroupNames.Count > i - 1 && dwidth >= requiredWidth)
      {
        var heigth = _style.GroupingNameSpace.GetFloatValue(this.Height);
        var rect = new RectangleF(lastX, chartTop - heigth, dwidth, heigth);
        ctx.DrawString(DataSource.DataGroupNames[i - 1], _style.DataCaptionFont, new SolidBrush(_style.TextColor), rect, stringFormat);
      }
    }

    private void drawDataPoint(Graphics ctx, PointF loc, Color color)
    {
      if (_style.DataDotStyle.Border != null)
      {
        var bwidth = _style.DataDotStyle.Border.Width.GetFloatValue(this.Width);
        ctx.FillCircle(new SolidBrush(_style.DataDotStyle.Border.Color.ReplaceIfTransparent(color)), loc.X, loc.Y, bwidth);
      }

      var width = _style.DataDotStyle.Width.GetFloatValue(this.Width);
      ctx.FillCircle(new SolidBrush(_style.DataDotStyle.Color.ReplaceIfTransparent(color)), loc.X, loc.Y, width);
    }

    private void drawDataPoints(Graphics ctx, DataSeries series, Color color)
    {
      PointF? lastPoint = null;
      var pen = _style.DataConnectionLineStyle.GetPen(this.Width);
      pen.Color = pen.Color.ReplaceIfTransparent(color);
      foreach (var point in series.DataPoints.OrderBy(x => x.BaseValue ?? 0))
      {
        var y = (float)(baseLinePos - ((point.Value - DataSource.MinValue) * pixelPerValue));
        var x = (float)(pixelPerBaseValue * (point.BaseValue.Value - DataSource.MinBaseValue)) + valueLineWidth;
        var p = new PointF(x, y);
        if (lastPoint != null)
        {
          ctx.DrawLine(pen, p, lastPoint.Value);
          drawDataPoint(ctx, lastPoint.Value, color);
        }
        lastPoint = p;
      }

      drawDataPoint(ctx, lastPoint.Value, color);
    }

    protected override void render(Graphics graphics)
    {
      drawDataGroups(graphics);
      drawDataPoints(graphics);
    }
  }
}
