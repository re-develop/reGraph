using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AeoGraphing.Charting.ColorGenerators;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Data;
using reGraph.Charting.ColorGenerators;

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
      AxisXPosition = new Measure(0.17F, MeasureType.Percentage),
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.DarkGray } },
      AxisYPosition = new Measure(0.07F, MeasureType.Percentage),
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
      MaxBarWidth = new Measure(0.05F, MeasureType.Percentage),
      BarGroupPadding = new Measure(0.07F, MeasureType.Percentage),
      BarWidthPercentage = new Measure(0.8F, MeasureType.Percentage),
      DrawValueLabelAboveBar = true,
      GroupLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DrawGroupLabel = true,
      StyleName = "DefaultStyle"
    };

    private BarChartStyle _style;
    protected BarDataSource DataSource { get; set; }
    protected Dictionary<int, Brush> seriesColorAtlas { get; set; } = new Dictionary<int, Brush>();

    public BarChart(DataCollection data, BarChartStyle style, int width, int height) : base(data, style, width, height)
    {
      _style = style;
      this.DataSource = BarDataSource.ExampleData;
      var gen = _style.DataColors;
      gen.Reset();

      foreach (var series in DataSource.Series)
      {
        seriesColorAtlas[series.Id] = new SolidBrush(gen.Current);
        gen.MoveNext();
      }
    }


    protected float totalGroupSpace => (chartRenderSpace - ((groupCount - 1) * _style.BarGroupPadding.GetFloatValue(chartRenderSpace)));
    protected float barWidth => Math.Min(totalGroupSpace / DataSource.AllBars.Count(), _style.MaxBarWidth.GetFloatValue(chartRenderSpace));
    protected float barOverflownSpace => DataSource.AllBars.Count() * Math.Max(0, (totalGroupSpace / DataSource.AllBars.Count()) - _style.MaxBarWidth.GetFloatValue(chartRenderSpace));
    protected float totalSpaceBetweenGroups => ((groupCount - 1) * _style.BarGroupPadding.GetFloatValue(chartRenderSpace)) + barOverflownSpace;
    protected float realSpaceBetweenGroups => totalSpaceBetweenGroups / (groupCount - 1);
    protected float barRealWidth => _style.BarWidthPercentage.GetPercentageValue(barWidth);
    protected float barOffeset => (barWidth - barRealWidth) / 2;
    protected float groupNameHeight => paddedHeight - baseLinePos;

    protected float getGroupWidth(BarGroup group)
    {
      return barWidth * group.Bars.Length;
    }

    private float chartRenderSpace => paddedWidth - valueLinePos;
    private int groupCount => DataSource.Groups.Count();
    private float groupSpace => (chartRenderSpace - ((groupCount - 1) * _style.BarGroupPadding.GetFloatValue(chartRenderSpace))) / groupCount;
   // private float barSpace => (groupSpace - ((DataSource.Series.Length - 1) * _style.BarInGroupPadding.GetFloatValue(chartRenderSpace))) / DataSource.Series.Length;
    // private float barWidth => Math.Min(_style.MaxBarWidth.GetFloatValue(chartRenderSpace), barSpace);
    //private float barOffset => (chartRenderSpace - requiredSpace) / 2;
    // 3% Error dunno why but that fixes it, pls dont touch
    //private float requiredSpace => (chartRenderSpace * 0.03F) + ((groupCount - 1) * groupSpace) + ((groupCount - 1) * _style.BarGroupPadding.GetFloatValue(chartRenderSpace)) + ((DataSource.DataSeries.Count - 1) * barWidth) + ((DataSource.DataSeries.Count - 1) * (_style.BarInGroupPadding.GetFloatValue(chartRenderSpace)));

    protected override void drawBaseLabels(Graphics ctx)
    {
      //base.drawBaseLabels(ctx);
    }

    public override void SetStyle(ChartStyle style)
    {
      if (style is BarChartStyle barStyle)
      {
        _style = barStyle;
        base.SetStyle(style);
      }
    }

 
    private void drawValueLabel(Graphics graphics, String label, RectangleF bar)
    {
      var heigth = graphics.MeasureString(label, _style.DataCaptionFont).Height;
      bar.Y -= heigth;
      bar.Height = heigth;
      graphics.DrawString(label, _style.DataCaptionFont, new SolidBrush(_style.TextColor), bar, new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center });
    }



    protected void drawGroupLabel(Graphics graphics, string label, float xStart, float xEnd)
    {
      var pad = _style.GroupLabelPadding.GetFloatValue(groupNameHeight);
      var height = groupNameHeight - pad;
      var y = baseLinePos + pad;
      var rect = new RectangleF(xStart, y, xEnd - xStart, height);
      var format = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center };
      graphics.DrawString(label, _style.DataCaptionFont, new SolidBrush(_style.TextColor), rect, format);
    }


    protected float drawGroup(Graphics graphics, BarGroup group, float offset)
    {
      var posx = offset;
      foreach (var bar in group.Bars.OrderBy(x => Array.IndexOf(DataSource.Series, DataSource.Series.FirstOrDefault(y => y.Id == x.SeriesId) ?? DataSource.Series.First())))
      {
        var height = (float)(pixelPerValue * (bar.Value - base.DataSource.MinValue));   
        var rect = new RectangleF(posx + barOffeset, baseLinePos - height, barRealWidth, height);
        graphics.FillRectangle(seriesColorAtlas[bar.SeriesId], rect);
        drawValueLabel(graphics, bar.Value.ToString(_style.NumericFormat), rect);
        posx += barWidth;
      }

      if (_style.DrawGroupLabel && string.IsNullOrEmpty(group.Name) == false)
        drawGroupLabel(graphics, group.Name, offset, posx);

      return posx + realSpaceBetweenGroups;
    }


    private void drawData(Graphics graphics)
    {
      //  var cgen = _style.DataColors;
      //  cgen.Reset();
      //  int num = 0;
      //  foreach (var series in DataSource.DataSeries)
      //  {
      //    drawBars(graphics, series, num++, cgen.Current);
      //    cgen.MoveNext();
      //  }

      float posx = valueLinePos;
      foreach (var group in DataSource.Groups)
      {
        posx = drawGroup(graphics, group, posx);
      }
    }

    protected override void render(Graphics graphics)
    {
      drawData(graphics);
      //drawLines(graphics);
    }
  }
}
