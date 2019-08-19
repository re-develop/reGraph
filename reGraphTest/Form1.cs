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

namespace AeoGraphingTest
{
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

    public partial class Form1 : Form
    {
        LineChartStyle _sdStyle;
        LineChartStyle _dfStyle;
        DataCollection _data;
        public Form1()
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
            _data = query.Query("count(),avg(sentiment)*10,avg(message.length),sum(message.length)/10 | 2:0:0 since 12.7.2019 options Theme=DarkTheme,DateFormat=hh:MM:ss", "Messages", out var options);
            _data.MaxValue = ((int)Math.Ceiling(_data.MaxValue / 10.0) * 10);
            _data.GroupingInterval = TimeSpan.FromDays(1).Ticks;
            _data.DataGroupNames.AddRange(data.Keys.Select(x => x.ToString("dd.yyyy")).Distinct());
            var dt = data.Keys.Max();
            _data.MaxBaseValue = new DateTime(dt.Year, dt.Month, dt.Day, 23, 0, 0).Ticks;

            InitializeComponent();
            var sdStyle = LineChart.DefaultStyle;
            sdStyle.Padding = 20;
            sdStyle.BackgroundColor = Color.FromArgb(44, 49, 53);
            sdStyle.TextColor = Color.AntiqueWhite;
            sdStyle.ThinLineStyle.Color = Color.FromArgb(111, 197, 238);
            sdStyle.ThinLineStyle.Type = LineType.Dashed;
            sdStyle.AxisLineStyle.Color = Color.FromArgb(90, 120, 255);
            sdStyle.AxisTicksLineStyle.Color = Color.FromArgb(90, 120, 255);
            sdStyle.TextColor = Color.FromArgb(255, 200, 0);
            sdStyle.DataCaptionFont = new Font("Arial", 12F);
            sdStyle.DrawAxisHelpLine = Axis2D.AxisY | Axis2D.AxisX;
            sdStyle.DrawDescription = true;
            sdStyle.DrawTitle = true;
            sdStyle.DataConnectionLineStyle.Color = /*Color.FromArgb(25, 200, 46)*/ Color.Transparent;
            sdStyle.DataDotStyle.Border.Color = /*Color.FromArgb(25, 200, 46)*/ Color.Transparent;
            sdStyle.DataDotStyle.Border.Width = 5;
            sdStyle.DataDotStyle.Color = Color.LightGray;
            sdStyle.DataDotStyle.Width = 3;
            sdStyle.DataLabelPadding = 10;
            sdStyle.DataColors = new PastelGenerator(Color.Gray);
            sdStyle.AxisYPosition = new Measure(0.07F, MeasureType.Percentage);
            sdStyle.AxisXPosition = new Measure(0.20F, MeasureType.Percentage);
            sdStyle.DataLabelsPosition = new Measure(0.15F, MeasureType.Percentage);
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented, Converters = new JsonConverter[] { new MeasureConverter(), new ColorHexConverter(), new StringEnumConverter(), new FontConverter() }, TypeNameHandling = TypeNameHandling.Auto };
            var json = JsonConvert.SerializeObject(SlateStyle, settings);

            //sdStyle = JsonConvert.DeserializeObject<LineChartStyle>(json, settings); //  WhiteStyle;
            _sdStyle = DarkStyle;
            _dfStyle = LineChart.DefaultStyle;
            draw();
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
            AxisXPosition = new Measure(0.15F, MeasureType.Percentage),
            DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
            DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.DarkSlateGray } },
            AxisYPosition = new Measure(0.05F, MeasureType.Percentage),
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
            AxisXPosition = new Measure(0.15F, MeasureType.Percentage),
            DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
            DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.DarkGray } },
            AxisYPosition = new Measure(0.05F, MeasureType.Percentage),
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
            AxisXPosition = new Measure(0.15F, MeasureType.Percentage),
            DataLabelsPosition = new Measure(0.1F, MeasureType.Percentage),
            DataLabelSquare = new BorderedShapeStyle { Color = Color.Transparent, Width = 10, Border = new ShapeStyle { Width = 12, Color = Color.WhiteSmoke } },
            AxisYPosition = new Measure(0.05F, MeasureType.Percentage),
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
            DataColors = new PastelGenerator(Color.LightGray),
            GroupingNameSpace = new Measure(0.05F, MeasureType.Percentage),
            GroupLineStyle = new LineStyle { Color = Color.GhostWhite, Type = LineType.DashDotted, Width = 1 }
        };

        void draw()
        {
            var chart = new LineChart(_data, _sdStyle /*_dfStyle*/, pb1.Width, pb1.Height);
            //chart.ValueSteps = null;
            pb1.Image = chart.Render();
        }

        private void Pb1_SizeChanged(object sender, EventArgs e)
        {
            draw();
        }
    }
}
