using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AeoGraphing.Charting.ColorGenerators;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Data;

namespace reGraph.Charting.StackedBarChart
{
    public class StackedBarChart : Chart2D
    {
        public static StackedBarChartStyle DefaultStyle => new StackedBarChartStyle()
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
            BarPadding = new Measure(0.07F, MeasureType.Percentage),
            DrawValueLabelInBar = true,
            StyleName = "DefaultStyle"
        };

        private StackedBarChartStyle _style;
        public StackedBarChart(DataCollection data, StackedBarChartStyle style, int width, int height) : base(data, style, width, height)
        {
            this._style = style;
        }

        private float chartRenderSpace => paddedWidth - valueLinePos;
        private float barCount => DataSource.DataSeries.Max(x => x.DataPoints.Count);
        private float barPadding => _style.BarPadding.GetFloatValue(chartRenderSpace);
        private float barSpace => (chartRenderSpace - ((barCount - 1) * barPadding)) / barCount;
        private float barWidth => Math.Min(_style.MaxBarWidth.GetFloatValue(chartRenderSpace), barSpace);
        private float requiredSpace => (barCount * barWidth) + ((barCount - 1) * barPadding);
        private float offeset => (chartRenderSpace - requiredSpace) / 2;

        protected override void drawBaseLabels(Graphics ctx)
        {
            //base.drawBaseLabels(ctx);
        }

        public override void SetStyle(ChartStyle style)
        {
            if (style is StackedBarChartStyle barStyle)
            {
                _style = barStyle;
                base.SetStyle(style);
            }
        }

        private void renderBar(Graphics graphics, int barNum)
        {
            var cgen = _style.DataColors;
            cgen.Reset();
            var x = (barNum * barPadding) + (barNum * barWidth) + valueLinePos + offeset;
            float lastY = baseLinePos;
            foreach (var series in DataSource.DataSeries)
            {
                if (series.DataPoints.Count <= barNum)
                {
                    cgen.MoveNext();
                    continue;
                }

                var point = series.DataPoints[barNum];
                var height = (float)(pixelPerValue * (point.Value - DataSource.MinValue));
                var rect = new RectangleF(x, lastY - height, barWidth, height);
                lastY = rect.Y;

                graphics.FillRectangle(new SolidBrush(cgen.Current), rect);
                if (_style.DrawValueLabelInBar && string.IsNullOrEmpty(point.ValueLabel) == false && graphics.MeasureString(point.ValueLabel, _style.DataCaptionFont).Height <= rect.Height)
                    graphics.DrawString(point.ValueLabel, _style.DataCaptionFont, new SolidBrush(_style.TextColor), rect, new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                cgen.MoveNext();
            }

            var label = DataSource.DataSeries.Select(z => barNum < z.DataPoints.Count() ? z.DataPoints[barNum].BaseLabel : null).FirstOrDefault();
            if (string.IsNullOrEmpty(label) == false)
                drawBaseLabel(graphics, x + (barWidth / 2), label);
        }


        private void renderBars(Graphics graphics)
        {
            for (int i = 0; i < barCount; i++)
                renderBar(graphics, i);
        }


        protected override void render(Graphics graphics)
        {
            renderBars(graphics);
        }
    }
}
