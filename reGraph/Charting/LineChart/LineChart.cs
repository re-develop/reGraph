using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using AeoGraphing.Data;
using AeoGraphing.Charting;
using AeoGraphing.Charting.ColorGenerators;
using AeoGraphing.Charting.Styling;

namespace AeoGraphing.Charting.LineChart
{
  public class LineChart : ISizeable
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
    };


    public LineChartStyle _style;
    public DataCollection DataSource { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private int baseLineHeight { get => (int)(_style.AxisXPosition.GetFloatValue(this.Height) + (_style.AxisTicksLength?.GetFloatValue(this.Height) ?? 0) + _style.Padding.GetFloatValue(this.Height) + (_style.DrawDataLabels ? _style.DataLabelsPosition.GetFloatValue(this.Height) : 0)); }
    private int valueLineWidth { get => (int)(_style.AxisYPosition.GetFloatValue(this.Width) + (_style.AxisTicksLength?.GetFloatValue(this.Width) ?? 0) + _style.Padding.GetFloatValue(this.Width)); }

    private int pWidth => Width - _style.Padding.GetIntValue(this.Width);
    private int pHeight => Height + _style.Padding.GetIntValue(this.Height);
    public double? ValueSteps { get; set; }
    public bool HasValueSteps => ValueSteps != null;
    private Graphics graphics { get; set; }
    private float titleHeight => (_style.DrawTitle && string.IsNullOrEmpty(DataSource.Title) == false) ? graphics.MeasureString(DataSource.Title, _style.TitleFont).Height + _style.Padding.GetFloatValue(this.Height) : 0;
    private float descriptionHeight => (_style.DrawDescription && string.IsNullOrEmpty(DataSource.Description) == false) ? graphics.MeasureString(DataSource.Description, _style.DescriptionFont).Height + _style.Padding.GetFloatValue(this.Height) : 0;
    private float chartTop => _style.Padding.GetFloatValue(this.Height) + titleHeight + descriptionHeight;

    private double pixelPerBaseValue => (pWidth - valueLineWidth) / DataSource.ScaledBaseValue;
    private double pixelPerValue => (baseLinePos - chartTop) / DataSource.ScaledMaxValue;
    private float baseLinePos => pHeight - baseLineHeight;
    private float valueLinePos => valueLineWidth;


    public LineChart(DataCollection dataSource, LineChartStyle style, int width, int height)
    {
      DataSource = dataSource;
      Width = width;
      Height = height;
      ValueSteps = (DataSource.MaxValue * 0.1);
      _style = style;
    }

    public void Render(Stream stream)
    {
      using (var img = Render())
      {
        img.Save(stream, ImageFormat.Png);
      }
    }

    public Image Render()
    {
      var img = new Bitmap(Width, Height);
      using (graphics = Graphics.FromImage(img))
      {
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        graphics.Background(img.Size, _style.BackgroundColor);
        drawLines(graphics);
        drawValueLabels(graphics);
        drawDataSeries();
        drawTitleAndDescription(graphics);
        //graphics.FillRectangle(new SolidBrush(Color.Yellow), new RectangleF(_style.Padding.GetFloatValue(this), pHeight - _style.DataLabelsPosition.GetFloatValue(this), pWidth - _style.Padding.GetFloatValue(this), _style.DataLabelsPosition.GetFloatValue(this) - (2 * _style.Padding.GetFloatValue(this))));
      }

      return img;
    }

    private void drawDataSeries()
    {
      PointF? last = new PointF(_style.Padding.GetFloatValue(this.Width), pHeight - _style.DataLabelsPosition.GetFloatValue(this.Height));
      _style.DataColors.Reset();
      foreach (var series in DataSource.DataSeries)
      {
        var color = _style.DataColors.Current;
        drawDataPoints(graphics, series, color);
        if (_style.DrawDataLabels && last != null)
          last = drawDataLabel(graphics, last.Value.X, last.Value.Y, series.Name, color)?.TopRight();

        _style.DataColors.MoveNext();
      }
    }

    private RectangleF? drawDataLabel(Graphics graphics, float x, float y, string name, Color color)
    {
      var size = graphics.MeasureString(name, _style.DataCaptionFont);
      var squareWidth = Math.Max(_style.DataLabelSquare.Width.GetFloatValue(this.Width), _style.DataLabelSquare.Border?.Width?.GetFloatValue(this.Width) ?? 0);
      var heigth = Math.Max(size.Height, squareWidth);
      var width = size.Width + _style.DataLabelSquarePadding.GetFloatValue(this.Width) + squareWidth + _style.DataLabelPadding.GetFloatValue(this.Width);
      if (x + width > pWidth)
      {
        x = _style.Padding.GetFloatValue(this.Width);
        y += heigth + _style.DataLabelPadding.GetFloatValue(this.Height);
      }

      if (y > Height - _style.Padding.GetFloatValue(this.Height))
        return null;

      var midPointY = y + (heigth / 2);      
      if (_style.DataLabelSquare.Border != null)
      {
        var sqbHeight = _style.DataLabelSquare.Border.Width.GetFloatValue(this.Width);
        graphics.FillRectangle(new SolidBrush(_style.DataLabelSquare.Border.Color.ReplaceIfTransparent(color)), new RectangleF(x + ((squareWidth - sqbHeight) / 2), midPointY - (sqbHeight / 2), sqbHeight, sqbHeight));
      }

      var sqHeight = _style.DataLabelSquare.Width.GetFloatValue(this.Width);
      graphics.FillRectangle(new SolidBrush(color), new RectangleF(x + ((squareWidth - sqHeight) / 2), midPointY - (sqHeight / 2), sqHeight, sqHeight));
      StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
      graphics.DrawString(name, _style.DataCaptionFont, new SolidBrush(_style.DataLabelSquare.Color.ReplaceIfTransparent(color)), new PointF(x + squareWidth + _style.DataLabelSquarePadding.GetFloatValue(this.Width), midPointY), stringFormat);
    
      return new RectangleF(x, y, width, heigth);
    }

    private void drawTitleAndDescription(Graphics ctx)
    {
      StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      var color = new SolidBrush(_style.TextColor);
      if (_style.DrawTitle)
        ctx.DrawString(DataSource.Title, _style.TitleFont, color, new PointF(pWidth / 2, titleHeight / 2), stringFormat);

      if (_style.DrawDescription)
        ctx.DrawString(DataSource.Description, _style.DescriptionFont, color, new PointF(pWidth / 2, titleHeight + (descriptionHeight / 2)), stringFormat);

    }

    private void drawLines(Graphics ctx)
    {
      var drawn = new HashSet<double>();
      foreach (var dataPoint in DataSource.DataPoints)
      {
        if (drawn.Contains(dataPoint.BaseValue.Value))
          continue;

        drawBaseLabel(ctx, dataPoint);
        drawn.Add(dataPoint.BaseValue.Value);
      }

      var pen = _style.AxisLineStyle.GetPen(this.Width);
      if (_style.DrawAxis.HasFlag(Axis2D.AxisX))
        ctx.DrawLine(pen, new PointF(valueLineWidth, chartTop), new PointF(valueLineWidth, baseLinePos));

      if (_style.DrawAxis.HasFlag(Axis2D.AxisY))
        ctx.DrawLine(pen, new PointF(valueLineWidth, baseLinePos), new PointF(pWidth, baseLinePos));
    }

    private void drawValueLabel(Graphics ctx, float y, string labelContent, bool firstValue = false)
    {
      if (_style.DrawAxisTicks.HasFlag(Axis2D.AxisY))
      {
        ctx.DrawLine(_style.AxisTicksLineStyle.GetPen(this.Width), new PointF(valueLinePos, y), new PointF(valueLinePos - _style.AxisTicksLength.GetFloatValue(this.Width), y));
      }

      if (_style.DrawAxisHelpLine.HasFlag(Axis2D.AxisX) && firstValue == false)
      {
        ctx.DrawLine(_style.ThinLineStyle.GetPen(this.Width), new PointF(valueLinePos + 1, y), new PointF(valueLinePos + (pWidth - valueLineWidth), y));
      }

      StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
      var origin = new PointF(valueLinePos - (_style.DrawAxisTicks.HasFlag(Axis2D.AxisY) ? _style.AxisTicksLength.GetFloatValue(this.Width) : 0) - _style.DataCaptionPadding.GetFloatValue(this.Width), y);
      ctx.DrawString(labelContent, _style.DataCaptionFont, new SolidBrush(_style.TextColor), origin, stringFormat);
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

    private void drawValueLabels(Graphics ctx)
    {
      if (HasValueSteps)
      {
        for (double f = 0; f <= DataSource.ScaledMaxValue; f += ValueSteps.Value)
        {
          var y = baseLinePos - (float)(f * pixelPerValue);
          drawValueLabel(ctx, y, (f + DataSource.MinValue).ToString(_style.NumericFormat), f == DataSource.MinValue);
        }
      }
      else
      {
        var drawn = new HashSet<float>();
        foreach (var point in DataSource.DataPoints)
        {
          var y = pHeight - (float)(pixelPerValue * (point.Value - DataSource.MinValue)) - baseLineHeight;
          if (drawn.Contains(y))
            continue;

          if (point.HasValueLabel == true)
            drawValueLabel(ctx, y, point.ValueLabel, point.Value == DataSource.MinValue);
          else
            drawValueLabel(ctx, y, point.Value.ToString(_style.NumericFormat), point.Value == DataSource.MinValue);
          drawn.Add(y);
        }
      }
    }

    private void drawBaseLabel(Graphics ctx, DataPoint point)
    {
      if (point.BaseValue != null)
      {
        var x = (float)(pixelPerBaseValue * (point.BaseValue.Value - DataSource.MinBaseValue)) + valueLineWidth;
        if (_style.DrawAxisTicks.HasFlag(Axis2D.AxisX))
        {
          ctx.DrawLine(_style.AxisTicksLineStyle.GetPen(this.Width), new PointF(x, baseLinePos), new PointF(x, baseLinePos + _style.AxisTicksLength.GetFloatValue(this.Height)));
        }

        if (_style.DrawAxisHelpLine.HasFlag(Axis2D.AxisY) && point.BaseValue.Value > DataSource.MinBaseValue)
        {
          ctx.DrawLine(_style.ThinLineStyle.GetPen(this.Width), new PointF(x, baseLinePos - 1), new PointF(x, chartTop));
        }

        if (point.HasBaseLabel == true)
        {
          var origin = new PointF(x, baseLinePos + (baseLineHeight / 2));
          var state = ctx.Save();
          ctx.ResetTransform();
          ctx.RotateTransform(288);
          origin.Y = baseLinePos + (_style.DrawAxisTicks.HasFlag(Axis2D.AxisX) ? _style.AxisTicksLength.GetFloatValue(this.Height) : 0) + _style.DataCaptionPadding.GetFloatValue(this.Height);
          ctx.TranslateTransform(origin.X, origin.Y, MatrixOrder.Append);
          StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
          ctx.DrawString(point.BaseLabel, _style.DataCaptionFont, new SolidBrush(_style.TextColor), 0, 0, stringFormat);
          ctx.Restore(state);
        }
      }
    }
  }
}
