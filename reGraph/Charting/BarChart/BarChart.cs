using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AeoGraphing.Charting.ColorGenerators;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Data;

namespace reGraph.Charting.BarChart
{
  public class BarChart : Chart2D
  {
    public static BarChartStyle DefaultStyle => new BarChartStyle()
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
      DrawAxis = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisCaption = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisHelpLine = Axis2D.AxisX,
      DrawAxisTicks = Axis2D.AxisY,
      NumericFormat = "0.00",
      ThinLineStyle = new LineStyle { Color = Color.LightGray, Type = LineType.Dashed, Width = 1 },
      DrawTitle = true,
      DrawDescription = true,
      DrawDataLabels = true,
      DataLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      DataColors = new PastelGenerator(Color.LightGray),
      MaxBarWidth = new Measure(0.03F, MeasureType.Percentage),
      BarGroupPadding = new Measure(0.07F, MeasureType.Percentage),
      BarInGroupPadding = new Measure(0.005F, MeasureType.Percentage)
    };

    private BarChartStyle _style;
    public BarChart(DataCollection data, BarChartStyle style, int width, int height) : base(data, style, width, height)
    {
      _style = style;
    }

    private int barGroupCount => DataSource.DataSeries.Max(x => x.DataPoints.Count);
    private float barRenderSpace => paddedWidth - valueLineWidth;
    private float barGroupSpace => (barRenderSpace - ((barGroupCount - 1) * _style.BarGroupPadding.GetFloatValue(this.Width))) / DataSource.DataSeries.Count;
    private float barSpace => (barGroupSpace - ((DataSource.DataSeries.Count - 1) * _style.BarInGroupPadding.GetFloatValue(this.Width))) / barGroupCount;
    private float barWidth => Math.Min(_style.MaxBarWidth.GetFloatValue(this.Width), barSpace);
    private float barCenterOffset => (barGroupSpace - ((barSpace - _style.MaxBarWidth.GetFloatValue(this.Width)) * DataSource.DataSeries.Count)) / 2;

    protected override void drawBaseLabels(Graphics ctx)
    {
      //base.drawBaseLabels(ctx);
    }

    private void drawBars(Graphics graphics, DataSeries series, int barNum, Color color)
    {
      var index = 0;
      var brush = new SolidBrush(color);
      foreach (var point in series.DataPoints)
      {
        var x = (index * barGroupSpace) + (index * _style.BarGroupPadding.GetFloatValue(this.Width)) + (barNum * barWidth) + (barNum * _style.BarInGroupPadding.GetFloatValue(this.Width)) + valueLinePos;
        if (barCenterOffset > 0)
          x += barCenterOffset;

        var height = (float)(pixelPerValue * (point.Value - DataSource.MinValue));
        var rect = new RectangleF(x, baseLinePos - height, barWidth, height);
        graphics.FillRectangle(brush, rect);
        drawBaseLabel(graphics, x + (barWidth / 2), point.BaseLabel);
        index++;
      }
    }

    private void drawData(Graphics graphics)
    {
      var cgen = _style.DataColors;
      cgen.Reset();
      int num = 0;
      foreach (var series in DataSource.DataSeries)
      {
        drawBars(graphics, series, num++, cgen.Current);
        cgen.MoveNext();
      }
    }

    protected override void render(Graphics graphics)
    {
      drawData(graphics);
    }
  }
}
