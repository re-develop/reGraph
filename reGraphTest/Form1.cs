using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AeoGraphing.Data;
using AeoGraphing.Data.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using AeoGraphing.Charting.LineChart;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Charting.ColorGenerators;
using reGraph.Charting.BarChart;
using reGraph.Charting.ColorGenerators;
using reGraph.Charting;
using reGraph.Charting.ScatterChart;
using reGraph.Charting.StackedBarChart;
using reGraph.Charting.SpiderChart;

namespace AeoGraphingTest
{
  public partial class Form1 : Form
  {
    private List<IChart> _charts = new List<IChart>();
    private Dictionary<PictureBox, IChart> _pictures = new Dictionary<PictureBox, IChart>();
    private Dictionary<PictureBox, TabPage> _pages = new Dictionary<PictureBox, TabPage>();
    private Dictionary<TabPage, List<ChartStyle>> _styles = new Dictionary<TabPage, List<ChartStyle>>();
    private Random _random = new Random();

    public void fillControl()
    {
      _pictures.Clear();
      _pages.Clear();
      _styles.Clear();
      tabControl.TabPages.Clear();
      foreach (var chart in _charts)
      {
        var tab = new TabPage(chart.GetType().Name);
        var pb = new PictureBox();
        pb.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pb.Image = chart.Render();
        pb.SizeChanged += Pb_SizeChanged;
        pb.Width = tab.Width;
        pb.Height = tab.Height;
        tab.Controls.Add(pb);
        _pictures[pb] = chart;
        _pages[pb] = tab;
        tabControl.TabPages.Add(tab);

        if (chart is ScatterChart scatter)
        {
          scatter.DataSource.MinMinBaseValue = 0;
          scatter.DataSource.MinMaxBaseValue = 500;
          scatter.BaseValueSteps = (scatter.DataSource.ScaledBaseValue * 0.025);
        }

        if (chart is StackedBarChart stacked)
        {
          var data = stacked.DataSource;
          data.MinMaxValue = Enumerable.Range(0, data.DataSeries.Max(x => x.DataPoints.Count)).Select(x => data.DataSeries.Select(y => x < y.DataPoints.Count ? y.DataPoints[x].Value : 0D).Sum()).Max();
          data.MaxValue = ((int)Math.Ceiling(data.MaxValue / 10.0) * 10);
          stacked.ValueSteps = (stacked.DataSource.ScaledMaxValue * 0.1);
        }

        var styles = new List<ChartStyle>();
        styles.Add(chart.Style);
        styles.AddRange(typeof(Form1).GetProperties().Where(x => x.PropertyType.Name == chart.GetType().Name + "Style").Select(x => { var val = (ChartStyle)x.GetValue(null); val.StyleName = x.Name; return val; }));
        _styles[tab] = styles;
      }
    }



    private void Pb_SizeChanged(object sender, EventArgs e)
    {
      if (sender is PictureBox pb)
      {
        if (_pages.ContainsKey(pb) == false || tabControl.SelectedTab != _pages[pb])
          return;

        var chart = _pictures[pb];
        chart.SetSize(pb.Width, pb.Height);
        pb.Image = chart.Render();
      }
    }




    private DataCollection generateData(int minPoints = 10, int maxPoints = 20, bool randomizeX = false, int? seriesCount = null)
    {
      var res = new DataCollection();
      res.Title = "Test Data";
      res.Description = "Randomly generated Testdata";

      var max = seriesCount ?? _random.Next(3, 7);
      var pointCount = _random.Next(minPoints, maxPoints);
      for (int i = 0; i < max; i++)
      {
        var series = new DataSeries($"Series {i}");
        for (int p = 0; p < pointCount; p++)
        {
          double x;
          if (randomizeX)
            x = _random.NextDouble() * 500;
          else
            x = ((double)p / pointCount) * 500;

          var y = _random.NextDouble() * 100;
          series.DataPoints.Add(new DataPoint(y, y.ToString("0.00"), x, x.ToString("0.00")));
        }
        res.DataSeries.Add(series);
      }

      res.MinMinValue = 0;
      res.MinMaxValue = 100;
      return res;
    }



    private T getChart<T>(int minPoints = 10, int maxPoints = 20, bool randomizeX = false, int? seriesCount = null) where T : IChart
    {
      var t = typeof(T);
      var data = generateData(minPoints, maxPoints, randomizeX, seriesCount);
      var style = t.GetProperty("DefaultStyle").GetValue(null);
      return (T)t.GetConstructors().First(x => x.GetParameters().Count() == 4).Invoke(new object[] { data, style, 1000, 1000 });
    }



    private void generateCharts()
    {
      _charts.Add(getChart<LineChart>());
      _charts.Add(getChart<BarChart>(1, 5));
      _charts.Add(getChart<StackedBarChart>(1, 7));
      _charts.Add(getChart<ScatterChart>(randomizeX: true));
      _charts.Add(getChart<SpiderChart>(seriesCount: 2));
    }



    public Form1()
    {
      InitializeComponent();
      tabControl.SelectedIndexChanged += TabControl_TabIndexChanged;
      styleBox.SelectedIndexChanged += StyleBox_SelectedIndexChanged;
      generateCharts();
      fillControl();
      styleBox.Items.AddRange(_styles[tabControl.SelectedTab].ToArray());
      styleBox.SelectedIndex = 0;
    }



    private void StyleBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (styleBox.Items.Count == 0 || tabControl.TabCount == 0)
        return;

      var page = tabControl.SelectedTab;
      var pb = _pages.First(x => x.Value == page).Key;
      var chart = _pictures[pb];
      chart.SetStyle((ChartStyle)styleBox.SelectedItem);
      pb.Image = chart.Render();
    }



    private void TabControl_TabIndexChanged(object sender, EventArgs e)
    {
      if (tabControl.TabCount == 0)
        return;

      var page = tabControl.SelectedTab;
      var pb = _pages.First(x => x.Value == page).Key;

      styleBox.Items.Clear();
      styleBox.Items.AddRange(_styles[page].ToArray());

      var chart = _pictures[pb];
      var index = styleBox.Items.IndexOf(chart.Style);
      styleBox.SelectedIndex = index;

      chart.SetSize(pb.Width, pb.Height);
      pb.Image = chart.Render();
    }



    private void BtnNewData_Click(object sender, EventArgs e)
    {
      if (tabControl.TabCount == 0)
        return;

      var page = tabControl.SelectedTab;
      var pb = _pages.First(x => x.Value == page).Key;

      var chart = _pictures[pb];
      DataCollection newData;
      switch (chart)
      {
        case BarChart bar:
          newData = generateData(1, 5);
          break;

        case ScatterChart scatter:
          newData = generateData(randomizeX: true);
          newData.MinMinBaseValue = 0;
          newData.MinMaxBaseValue = 500;
          scatter.BaseValueSteps = (newData.ScaledBaseValue * 0.025);
          break;

        case StackedBarChart stacked:
          newData = generateData(1, 7);
          newData.MinMaxValue = Enumerable.Range(0, newData.DataSeries.Max(x => x.DataPoints.Count)).Select(x => newData.DataSeries.Select(y => x < y.DataPoints.Count ? y.DataPoints[x].Value : 0D).Sum()).Max();
          newData.MaxValue = ((int)Math.Ceiling(newData.MaxValue / 10.0) * 10);
          stacked.ValueSteps = (newData.ScaledMaxValue * 0.1);
          break;

        case SpiderChart spider:
          newData = generateData(seriesCount: 2);
          break;
        default:
          newData = generateData();
          break;
      }


      chart.SetDataSource(newData);
      pb.Image = chart.Render();
    }

    public static LineChartStyle SlateStyle => new LineChartStyle()
    {
      Padding = 10,
      TextColor = Color.DarkSlateGray,
      BackgroundColor = Color.SlateGray,
      TitleFont = new Font("Arial", 28),
      DescriptionFont = new Font("Arial", 18),
      AxisCaptionFont = new Font("Arial", 16),
      DataCaptionFont = new Font("Arial", 14),
      AxisLineStyle = new LineStyle { Color = Color.DarkSlateGray, Type = LineType.Solid, Width = 2 },
      AxisTicksLineStyle = new LineStyle { Color = Color.DarkSlateBlue, Type = LineType.Solid, Width = 2 },
      AxisTicksLength = 5,
      AxisXPosition = new Measure(0.17F, MeasureType.Percentage),
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.DarkSlateGray } },
      AxisYPosition = new Measure(0.07F, MeasureType.Percentage),
      DataCaptionPadding = 5,
      DataConnectionLineStyle = new LineStyle { Color = Color.Transparent, Type = LineType.Solid, Width = 3 },
      DataDotStyle = new BorderedShapeStyle { Color = Color.DarkSlateGray, Width = 3, Border = new ShapeStyle { Color = Color.Transparent, Width = 5 } },
      DrawAxis = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisCaption = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisHelpLine = Axis2D.AxisX,
      DrawAxisTicks = Axis2D.AxisX | Axis2D.AxisY,
      NumericFormat = "0.00",
      ThinLineStyle = new LineStyle { Color = Color.SlateBlue, Type = LineType.Dashed, Width = 1 },
      DrawTitle = true,
      DrawDescription = true,
      DrawDataLabels = true,
      DataLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      DataColors = new PastelGenerator(Color.Salmon),
      GroupingNameSpace = new Measure(0.05F, MeasureType.Percentage),
      GroupLineStyle = new LineStyle { Color = Color.SlateBlue, Type = LineType.DashDotted, Width = 1 }
    };
    public static LineChartStyle WhiteStyle => new LineChartStyle()
    {
      Padding = 10,
      TextColor = Color.FromArgb(54, 57, 62),
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
      DataConnectionLineStyle = new LineStyle { Color = Color.Transparent, Type = LineType.Solid, Width = 3 },
      DataDotStyle = new BorderedShapeStyle { Color = Color.LightGray, Width = 3, Border = new ShapeStyle { Color = Color.Transparent, Width = 5 } },
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
      DataColors = new PastelGenerator(Color.SlateGray),
    };
    public static LineChartStyle DarkStyle => new LineChartStyle()
    {
      Padding = 10,
      TextColor = Color.WhiteSmoke,
      BackgroundColor = Color.FromArgb(54, 57, 62),
      TitleFont = new Font("Arial", 28),
      DescriptionFont = new Font("Arial", 18),
      AxisCaptionFont = new Font("Arial", 16),
      DataCaptionFont = new Font("Arial", 14),
      AxisLineStyle = new LineStyle { Color = Color.WhiteSmoke, Type = LineType.Solid, Width = 2 },
      AxisTicksLineStyle = new LineStyle { Color = Color.WhiteSmoke, Type = LineType.Solid, Width = 2 },
      AxisTicksLength = 5,
      AxisXPosition = new Measure(0.17F, MeasureType.Percentage),
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.WhiteSmoke } },
      AxisYPosition = new Measure(0.07F, MeasureType.Percentage),
      DataCaptionPadding = 5,
      DataConnectionLineStyle = new LineStyle { Color = Color.Transparent, Type = LineType.Solid, Width = 3 },
      DataDotStyle = new BorderedShapeStyle { Color = Color.WhiteSmoke, Width = 3, Border = new ShapeStyle { Color = Color.Transparent, Width = 5 } },
      DrawAxis = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisCaption = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisHelpLine = Axis2D.AxisX,
      DrawAxisTicks = Axis2D.AxisX | Axis2D.AxisY,
      NumericFormat = "0.00",
      ThinLineStyle = new LineStyle { Color = Color.DarkGray, Type = LineType.Dashed, Width = 1 },
      DrawTitle = true,
      DrawDescription = true,
      DrawDataLabels = true,
      DataLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      DataColors = /*new PastelGenerator(Color.LightGray)*/new HarmonicContrastGenerator(0.5, 0.8, 80),
      GroupingNameSpace = new Measure(0.05F, MeasureType.Percentage),
      GroupLineStyle = new LineStyle { Color = Color.GhostWhite, Type = LineType.DashDotted, Width = 1 }
    };
    public static BarChartStyle BarDarkStyle => new BarChartStyle()
    {
      Padding = 10,
      TextColor = Color.WhiteSmoke,
      BackgroundColor = Color.FromArgb(54, 57, 62),
      TitleFont = new Font("Arial", 28),
      DescriptionFont = new Font("Arial", 18),
      AxisCaptionFont = new Font("Arial", 16),
      DataCaptionFont = new Font("Arial", 14),
      AxisLineStyle = new LineStyle { Color = Color.WhiteSmoke, Type = LineType.Solid, Width = 2 },
      AxisTicksLineStyle = new LineStyle { Color = Color.WhiteSmoke, Type = LineType.Solid, Width = 2 },
      AxisTicksLength = 5,
      AxisXPosition = new Measure(0.17F, MeasureType.Percentage),
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.WhiteSmoke } },
      AxisYPosition = new Measure(0.07F, MeasureType.Percentage),
      DataCaptionPadding = 5,
      DrawAxis = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisCaption = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisHelpLine = Axis2D.AxisX,
      DrawAxisTicks = Axis2D.AxisX | Axis2D.AxisY,
      NumericFormat = "0.00",
      ThinLineStyle = new LineStyle { Color = Color.DarkGray, Type = LineType.Dashed, Width = 1 },
      DrawTitle = true,
      DrawDescription = true,
      DrawDataLabels = true,
      DataLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      DataColors = /*new PastelGenerator(Color.LightGray)*/new HarmonicContrastGenerator(0.5, 0.8, 80),
      MaxBarWidth = new Measure(0.03F, MeasureType.Percentage),
      BarGroupPadding = new Measure(0.07F, MeasureType.Percentage),
      BarInGroupPadding = new Measure(0.005F, MeasureType.Percentage),
      DrawValueLabelAboveBar = true
    };
    public static ScatterChartStyle ScatterDarkStyle => new ScatterChartStyle()
    {
      Padding = 10,
      TextColor = Color.WhiteSmoke,
      BackgroundColor = Color.FromArgb(54, 57, 62),
      TitleFont = new Font("Arial", 28),
      DescriptionFont = new Font("Arial", 18),
      AxisCaptionFont = new Font("Arial", 16),
      DataCaptionFont = new Font("Arial", 14),
      AxisLineStyle = new LineStyle { Color = Color.WhiteSmoke, Type = LineType.Solid, Width = 2 },
      AxisTicksLineStyle = new LineStyle { Color = Color.WhiteSmoke, Type = LineType.Solid, Width = 2 },
      AxisTicksLength = 5,
      AxisXPosition = new Measure(0.17F, MeasureType.Percentage),
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.WhiteSmoke } },
      AxisYPosition = new Measure(0.07F, MeasureType.Percentage),
      DataCaptionPadding = 5,
      DrawAxis = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisCaption = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisHelpLine = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisTicks = Axis2D.AxisX | Axis2D.AxisY,
      NumericFormat = "0.00",
      ThinLineStyle = new LineStyle { Color = Color.DarkGray, Type = LineType.Dashed, Width = 1 },
      DrawTitle = true,
      DrawDescription = true,
      DrawDataLabels = true,
      DataLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      DataColors = /*new PastelGenerator(Color.LightGray)*/new HarmonicContrastGenerator(0.5, 0.8, 80),
      ScatterDotStyle = new BorderedShapeStyle { Color = Color.LightGray, Width = 3, Border = new ShapeStyle { Color = Color.Transparent, Width = 5 } },
    };
    public static StackedBarChartStyle StackedBarDarkStyle => new StackedBarChartStyle()
    {
      Padding = 10,
      TextColor = Color.WhiteSmoke,
      BackgroundColor = Color.FromArgb(54, 57, 62),
      TitleFont = new Font("Arial", 28),
      DescriptionFont = new Font("Arial", 18),
      AxisCaptionFont = new Font("Arial", 16),
      DataCaptionFont = new Font("Arial", 14),
      AxisLineStyle = new LineStyle { Color = Color.WhiteSmoke, Type = LineType.Solid, Width = 2 },
      AxisTicksLineStyle = new LineStyle { Color = Color.WhiteSmoke, Type = LineType.Solid, Width = 2 },
      AxisTicksLength = 5,
      AxisXPosition = new Measure(0.17F, MeasureType.Percentage),
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.WhiteSmoke } },
      AxisYPosition = new Measure(0.07F, MeasureType.Percentage),
      DataCaptionPadding = 5,
      DrawAxis = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisCaption = Axis2D.AxisX | Axis2D.AxisY,
      DrawAxisHelpLine = Axis2D.AxisX,
      DrawAxisTicks = Axis2D.AxisX | Axis2D.AxisY,
      NumericFormat = "0.00",
      ThinLineStyle = new LineStyle { Color = Color.DarkGray, Type = LineType.Dashed, Width = 1 },
      DrawTitle = true,
      DrawDescription = true,
      DrawDataLabels = true,
      DataLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      DataColors = /*new PastelGenerator(Color.LightGray)*/new HarmonicContrastGenerator(0.5, 0.8, 80),
      MaxBarWidth = new Measure(0.05F, MeasureType.Percentage),
      BarPadding = new Measure(0.07F, MeasureType.Percentage),
      DrawValueLabelInBar = true
    };

    private void BtnEditStyle_Click(object sender, EventArgs e)
    {
      var form = new EditStyleForm((ChartStyle)styleBox.SelectedItem);
      if (form.ShowDialog() == DialogResult.OK)
      {
        var page = tabControl.SelectedTab;
        var pb = _pages.First(x => x.Value == page).Key;
        var chart = _pictures[pb];
        chart.SetStyle(form.Style);
        pb.Image = chart.Render();
      }
    }
  }

  public class FontConverter : JsonConverter<Font>
  {
    public override Font ReadJson(JsonReader reader, Type objectType, Font existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      var value = ((string)reader.Value).Split(',');
      return new Font(value[0], float.Parse(value[1]));
    }

    public override void WriteJson(JsonWriter writer, Font value, JsonSerializer serializer)
    {
      writer.WriteValue($"{value.Name}, {value.Size}");
    }
  }

  public class MeasureConverter : JsonConverter<Measure>
  {
    public override Measure ReadJson(JsonReader reader, Type objectType, Measure existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      if (objectType != typeof(Measure))
        return null;

      return Measure.FromString((string)reader.Value);
    }

    public override void WriteJson(JsonWriter writer, Measure value, JsonSerializer serializer)
    {
      writer.WriteValue(value.ToString());
    }
  }

  public class ColorHexConverter : JsonConverter
  {

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var color = (Color)value;
      var hexString = color.IsEmpty ? string.Empty : string.Concat("#", (color.ToArgb() & 0xFFFFFFFF).ToString("X8"));
      writer.WriteValue(hexString);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      var hexString = (string)reader.Value;
      if (hexString == null || !hexString.StartsWith("#")) return Color.Empty;
      return ColorTranslator.FromHtml(hexString);
    }

    public override bool CanConvert(Type objectType)
    {
      return objectType == typeof(Color);
    }
  }

  public class MessageLog : IDateable
  {
    public DateTime DateTime { get; set; }
    public double Sentiment { get; set; }
    public string Message { get; set; }
  }
}
