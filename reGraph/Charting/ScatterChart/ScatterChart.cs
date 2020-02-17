using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using AeoGraphing.Charting;
using AeoGraphing.Charting.ColorGenerators;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Data;

namespace reGraph.Charting.ScatterChart
{
  public class ScatterChart : Chart2D
  {
    public static ScatterChartStyle DefaultStyle => new ScatterChartStyle()
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
      AxisXPosition = new Measure(0.17F, MeasureType.Percentage),
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.DarkGray } },
      AxisYPosition = new Measure(0.07F, MeasureType.Percentage),
      DataCaptionPadding = 5,
      DrawAxis = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisCaption = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisHelpLine = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisTicks = Axis2D.AxisX | Axis2D.AxisY,
      NumericFormat = "0.00",
      ThinLineStyle = new LineStyle { Color = Color.LightGray, Type = LineType.Dashed, Width = 1 },
      DrawTitle = true,
      DrawDescription = true,
      DrawDataLabels = true,
      DataLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      DataColors = new PastelGenerator(Color.LightGray),
      ScatterDotStyle = new BorderedShapeStyle { Color = Color.LightGray, Width = 3, Border = new ShapeStyle { Color = Color.Transparent, Width = 5 } },
      StyleName = "DefaultStyle"
    };

    private ScatterChartStyle _style;
    public ScatterChart(DataCollection data, ScatterChartStyle style, int width, int height) : base(data, style, width, height)
    {
      _style = style;
    }

    public override void SetStyle(ChartStyle style)
    {
      if (style is ScatterChartStyle scatterStyle)
      {
        _style = scatterStyle;
        base.SetStyle(style);
      }
    }

    private void drawData(Graphics graphics)
    {
      var cgen = _style.DataColors;
      cgen.Reset();
      foreach (var series in DataSource.DataSeries)
      {
        drawDataSeries(graphics, series, cgen.Current);
        cgen.MoveNext();
      }
    }



    private void drawDataSeries(Graphics graphics, DataSeries series, Color color)
    {
      foreach (var point in series.DataPoints)
      {
        var y = (float)(baseLinePos - ((point.Value - DataSource.MinValue) * pixelPerValue));
        var x = (float)(pixelPerBaseValue * (point.BaseValue.Value - DataSource.MinBaseValue)) + valueLineWidth;
        var loc = new PointF(x, y);
        drawDataPoint(graphics, loc, color);
      }
    }



    private void drawDataPoint(Graphics ctx, PointF loc, Color color)
    {
      if (_style.ScatterDotStyle.Border != null)
      {
        var bwidth = _style.ScatterDotStyle.Border.Width.GetFloatValue(this.Width);
        ctx.FillCircle(new SolidBrush(_style.ScatterDotStyle.Border.Color.ReplaceIfTransparent(color)), loc.X, loc.Y, bwidth);
      }

      var width = _style.ScatterDotStyle.Width.GetFloatValue(this.Width);
      ctx.FillCircle(new SolidBrush(_style.ScatterDotStyle.Color.ReplaceIfTransparent(color)), loc.X, loc.Y, width);
    }



    protected override void render(Graphics graphics)
    {
      drawData(graphics);
    }
  }
}
