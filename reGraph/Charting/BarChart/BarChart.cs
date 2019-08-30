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
            MaxBarWidth = new Measure(0.03F, MeasureType.Percentage),
            BarGroupPadding = new Measure(0.07F, MeasureType.Percentage),
            BarInGroupPadding = new Measure(0.005F, MeasureType.Percentage),
            DrawValueLabelAboveBar = true,
            StyleName = "DefaultStyle"
        };

        private BarChartStyle _style;
        public BarChart(DataCollection data, BarChartStyle style, int width, int height) : base(data, style, width, height)
        {
            _style = style;
        }

        private float chartRenderSpace => paddedWidth - valueLinePos;
        private int groupCount => DataSource.DataSeries.Max(x => x.DataPoints.Count);
        private float groupSpace => (chartRenderSpace - ((groupCount - 1) * _style.BarGroupPadding.GetFloatValue(chartRenderSpace))) / groupCount;
        private float barSpace => (groupSpace - ((DataSource.DataSeries.Count - 1) * _style.BarInGroupPadding.GetFloatValue(chartRenderSpace))) / DataSource.DataSeries.Count;
        private float barWidth => Math.Min(_style.MaxBarWidth.GetFloatValue(chartRenderSpace), barSpace);
        private float barOffset => (chartRenderSpace - requiredSpace) / 2;
        // 3% Error dunno why but that fixes it, pls dont touch
        private float requiredSpace => (chartRenderSpace * 0.03F) + ((groupCount - 1) * groupSpace) + ((groupCount - 1) * _style.BarGroupPadding.GetFloatValue(chartRenderSpace)) + ((DataSource.DataSeries.Count - 1) * barWidth) + ((DataSource.DataSeries.Count - 1) * (_style.BarInGroupPadding.GetFloatValue(chartRenderSpace)));

        protected override void drawBaseLabels(Graphics ctx)
        {
            //base.drawBaseLabels(ctx);
        }

        public override void SetStyle(ChartStyle style)
        {
            if(style is BarChartStyle barStyle)
            {
                _style = barStyle;
                base.SetStyle(style);
            }
        }

        private void drawBars(Graphics graphics, DataSeries series, int barNum, Color color)
        {
            var groupNum = 0;
            var brush = new SolidBrush(color);
            foreach (var point in series.DataPoints)
            {
                var x = (groupNum * groupSpace) + (groupNum * _style.BarGroupPadding.GetFloatValue(chartRenderSpace)) + (barNum * barWidth) + (barNum * (_style.BarInGroupPadding.GetFloatValue(chartRenderSpace))) + valueLinePos;
                if (barSpace > barWidth)
                    x += barOffset;

                var height = (float)(pixelPerValue * (point.Value - DataSource.MinValue));
                var rect = new RectangleF(x, baseLinePos - height, barWidth, height);
                graphics.FillRectangle(brush, rect);
                drawBaseLabel(graphics, x + (barWidth / 2), point.BaseLabel);
                if (_style.DrawValueLabelAboveBar && string.IsNullOrEmpty(point.ValueLabel) == false)
                    drawValueLabel(graphics, point.ValueLabel, rect);

                groupNum++;
            }
        }


        private void drawValueLabel(Graphics graphics, String label, RectangleF bar)
        {
            var heigth = graphics.MeasureString(label, _style.DataCaptionFont).Height;
            bar.Y -= heigth;
            bar.Height = heigth;
            graphics.DrawString(label, _style.DataCaptionFont, new SolidBrush(_style.TextColor), bar, new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center });
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
            drawLines(graphics);
        }
    }
}
