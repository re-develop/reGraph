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
using reGraph.Charting.PieChart;

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
      _charts.Add(getChart<PieChart>(maxPoints: 5, minPoints: 3));
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



    private void testQuery()
    {
      var data = new Dictionary<DateTime, double>
            {
              {DateTime.Parse("2019-07-12 21:37:57"),0.792523},
              {DateTime.Parse("2019-07-12 21:40:39"),0.549819},
              {DateTime.Parse("2019-07-12 21:41:18"),0.361533},
              {DateTime.Parse("2019-07-12 21:41:28"),0.689133},
              {DateTime.Parse("2019-07-12 21:49:07"),0.621274},
              {DateTime.Parse("2019-07-12 21:49:32"),0.992156},
              {DateTime.Parse("2019-07-12 21:49:39"),0.5},
              {DateTime.Parse("2019-07-12 21:52:23"),0.47323},
              {DateTime.Parse("2019-07-12 21:52:24"),0.5},
              {DateTime.Parse("2019-07-12 21:53:00"),0.774048},
              {DateTime.Parse("2019-07-12 22:09:25"),0.82602},
              {DateTime.Parse("2019-07-12 22:11:13"),0.5},
              {DateTime.Parse("2019-07-12 23:48:34"),0.82602},
              {DateTime.Parse("2019-07-13 06:43:13"),0.5},
              {DateTime.Parse("2019-07-13 09:26:40"),0.525748},
              {DateTime.Parse("2019-07-13 09:53:57"),0.5},
              {DateTime.Parse("2019-07-13 10:11:06"),0.5},
              {DateTime.Parse("2019-07-13 10:56:29"),0.5},
              {DateTime.Parse("2019-07-13 11:04:57"),0.793689},
              {DateTime.Parse("2019-07-13 11:05:20"),0.765082},
              {DateTime.Parse("2019-07-13 11:05:30"),0.438039},
              {DateTime.Parse("2019-07-13 11:07:10"),0.787975},
              {DateTime.Parse("2019-07-13 11:16:54"),0.406862},
              {DateTime.Parse("2019-07-13 11:17:01"),1},
              {DateTime.Parse("2019-07-13 11:40:18"),0.532279},
              {DateTime.Parse("2019-07-13 11:40:29"),0.940772},
              {DateTime.Parse("2019-07-13 11:40:37"),0.315213},
              {DateTime.Parse("2019-07-13 11:55:41"),0.752674},
              {DateTime.Parse("2019-07-13 11:56:16"),0.780299},
              {DateTime.Parse("2019-07-13 12:59:46"),0.570275},
              {DateTime.Parse("2019-07-13 13:02:31"),0.98028},
              {DateTime.Parse("2019-07-13 13:03:05"),0.717274},
              {DateTime.Parse("2019-07-13 13:03:07"),0.804145},
              {DateTime.Parse("2019-07-13 13:10:43"),0.5},
              {DateTime.Parse("2019-07-13 13:22:26"),0.585128},
              {DateTime.Parse("2019-07-13 13:26:25"),0.907053},
              {DateTime.Parse("2019-07-13 13:26:29"),0.463044},
              {DateTime.Parse("2019-07-13 16:18:24"),0.5},
              {DateTime.Parse("2019-07-13 16:19:57"),0.515068},
              {DateTime.Parse("2019-07-13 16:20:36"),0.323729},
              {DateTime.Parse("2019-07-13 16:20:51"),0.448339},
              {DateTime.Parse("2019-07-13 16:21:07"),0.530101},
              {DateTime.Parse("2019-07-13 16:46:38"),0.5},
              {DateTime.Parse("2019-07-13 20:05:49"),0.722248},
              {DateTime.Parse("2019-07-13 20:17:58"),0.31929},
              {DateTime.Parse("2019-07-13 20:22:29"),0.724998},
              {DateTime.Parse("2019-07-13 21:30:33"),0.0593428},
              {DateTime.Parse("2019-07-14 07:14:54"),0.801141},
              {DateTime.Parse("2019-07-14 07:31:35"),0.5},
              {DateTime.Parse("2019-07-14 08:43:27"),0.5},
              {DateTime.Parse("2019-07-14 10:46:38"),0.888262},
              {DateTime.Parse("2019-07-14 10:47:22"),0.820125},
              {DateTime.Parse("2019-07-14 10:47:35"),0.000629008},
              {DateTime.Parse("2019-07-14 11:30:51"),0.547706},
              {DateTime.Parse("2019-07-14 12:03:51"),0.860375},
              {DateTime.Parse("2019-07-14 12:04:06"),0.55883},
              {DateTime.Parse("2019-07-14 12:04:24"),0.5},
              {DateTime.Parse("2019-07-14 12:08:34"),0.42278},
              {DateTime.Parse("2019-07-14 14:07:50"),0.5},
              {DateTime.Parse("2019-07-14 15:58:59"),0.5},
              {DateTime.Parse("2019-07-14 15:59:02"),0.5},
              {DateTime.Parse("2019-07-14 15:59:12"),0.988422},
              {DateTime.Parse("2019-07-14 15:59:24"),0.451729},
              {DateTime.Parse("2019-07-14 15:59:28"),0.5},
              {DateTime.Parse("2019-07-14 19:51:34"),0.71457},
              {DateTime.Parse("2019-07-15 06:44:02"),0.525748},
              {DateTime.Parse("2019-07-15 07:09:32"),0.5},
              {DateTime.Parse("2019-07-15 07:09:35"),0.71457},
              {DateTime.Parse("2019-07-15 09:45:21"),0.5},
              {DateTime.Parse("2019-07-15 10:10:16"),0.71457},
            };

      var random = new Random();
      List<MessageLog> logs = data.Select(x => new MessageLog { DateTime = x.Key, Sentiment = x.Value, Message = new string('a', random.Next(5, 50)) }).ToList();
      var query = new DataQuery(logs);
      var res = query.Query("count(),avg(sentiment)*10,avg(message.length),sum(message.length)/10 | 2:0:0 since 12.7.2019 options Theme=DarkTheme,DateFormat=hh:MM:ss", "Messages", out var options);
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
        case PieChart pie:
          newData = generateData(3, 5);
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
      BarWidthPercentage = new Measure(0.8F, MeasureType.Percentage),
      GroupLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DrawGroupLabel = true,
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

    public static PieChartStyle PieDarkStyle => new PieChartStyle()
    {
      Padding = 10,
      TextColor = Color.WhiteSmoke,
      BackgroundColor = Color.FromArgb(54, 57, 62),
      TitleFont = new Font("Arial", 28),
      DescriptionFont = new Font("Arial", 18),
      AxisCaptionFont = new Font("Arial", 16),
      DataCaptionFont = new Font("Arial", 14),
      AxisLineStyle = new LineStyle { Color = Color.WhiteSmoke, Type = LineType.Solid, Width = 2 },
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.WhiteSmoke } },
      DataCaptionPadding = 5,
      NumericFormat = "0.00",
      DrawTitle = true,
      DrawDescription = true,
      DrawDataLabels = true,
      DataLabelPadding = new Measure(0.01F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      DataColors = /*new PastelGenerator(Color.LightGray)*/new HarmonicContrastGenerator(0.5, 0.8, 80),
      FullCircleDegrees = 320F,
      RenderCircleDescription = true,
      CircleInnerSpace = 0.2F,
      HeightPadding = 10,
      WidthPadding = 10,
      StyleName = "PieDarkStyle"
    };

    public static SpiderChartStyle SpiderDarkStyle => new SpiderChartStyle()
    {
      Padding = 10,
      TextColor = Color.WhiteSmoke,
      BackgroundColor = Color.FromArgb(54, 57, 62),
      TitleFont = new Font("Arial", 28),
      DescriptionFont = new Font("Arial", 18),
      AxisCaptionFont = new Font("Arial", 16),
      DataCaptionFont = new Font("Arial", 14),
      AxisLineStyle = new LineStyle { Color = Color.WhiteSmoke, Type = LineType.Solid, Width = 2 },
      DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
      DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.WhiteSmoke } },
      DataCaptionPadding = 5,
      DataConnectionLineStyle = new LineStyle { Color = Color.Transparent, Type = LineType.Solid, Width = 3 },
      DataDotStyle = new BorderedShapeStyle { Color = Color.WhiteSmoke, Width = 3, Border = new ShapeStyle { Color = Color.Transparent, Width = 5 } },
      NumericFormat = "0.00",
      ThinLineStyle = new LineStyle { Color = Color.DarkGray, Type = LineType.Dashed, Width = 1 },
      DrawTitle = true,
      DrawDescription = true,
      DrawDataLabels = true,
      DataColors = /*new PastelGenerator(Color.LightGray)*/new HarmonicContrastGenerator(0.5, 0.8, 80),
      HeightPadding = 10,
      WidthPadding = 10,
      DataLabelPadding = new Measure(0.05F, MeasureType.Percentage),
      DataLabelSquarePadding = 5,
      FillAreaOfDataSeries = true,
      AreaFillAlpha = 60,
      AxisCaptionDistance = 0.01F,
      DrawValueLabels = true
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
