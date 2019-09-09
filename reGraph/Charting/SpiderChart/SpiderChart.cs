using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using AeoGraphing.Charting;
using AeoGraphing.Charting.ColorGenerators;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Data;

namespace reGraph.Charting.SpiderChart
{
    public class SpiderChart : IChart
    {
        public static SpiderChartStyle DefaultStyle => new SpiderChartStyle
        {
            Padding = 10,
            TextColor = Color.DarkGray,
            BackgroundColor = Color.WhiteSmoke,
            TitleFont = new Font("Arial", 28),
            DescriptionFont = new Font("Arial", 18),
            AxisCaptionFont = new Font("Arial", 16),
            DataCaptionFont = new Font("Arial", 14),
            AxisLineStyle = new LineStyle { Color = Color.DarkGray, Type = LineType.Solid, Width = 2 },
            DataCaptionPadding = 5,
            NumericFormat = "0.00",
            ThinLineStyle = new LineStyle { Color = Color.LightGray, Type = LineType.Dashed, Width = 1 },
            DrawTitle = true,
            DrawDescription = true,
            StyleName = "DefaultStyle",
            HeightPadding = 10,
            WidthPadding = 10,
            DataColors = new PastelGenerator(Color.LightGray),
            DataConnectionLineStyle = new LineStyle { Color = Color.Transparent, Type = LineType.Solid, Width = 3 },
            DataDotStyle = new BorderedShapeStyle { Color = Color.DarkGray, Width = 3, Border = new ShapeStyle { Color = Color.Transparent, Width = 5 } },
        };

        public SpiderChart(DataCollection data, SpiderChartStyle style, int width, int height)
        {
            this.DataSource = data;
            this._style = style;
            this.Width = width;
            this.Height = height;
            this.ValueSteps = 0.1F;
        }

        private SpiderChartStyle _style;
        public ChartStyle Style => _style;
        protected int Width { get; set; }
        protected int Height { get; set; }
        protected Graphics graphics;

        public DataCollection DataSource { get; private set; }

        protected int pointCount => DataSource.DataSeries.Max(x => x.DataPoints.Count);
        protected float chartHeight => Height - (2 * _style.HeightPadding.GetFloatValue(this.Height));
        protected float chartWidth => Width - (2 * _style.WidthPadding.GetFloatValue(this.Width));
        protected float chartMinSide => Math.Min(chartWidth, chartHeight);
        protected float lineLength => chartMinSide / 2;
        protected PointF chartMiddle => new PointF(Width / 2, Height / 2);
        public float ValueSteps { get; set; }

        public virtual void Render(Stream stream, ImageFormat format = null)
        {
            using (var img = Render())
            {
                img.Save(stream, format ?? ImageFormat.Png);
            }
        }

        public Image Render()
        {
            if (Width <= 0 || Height <= 0)
                return null;

            var img = new Bitmap(Width, Height);
            using (graphics = Graphics.FromImage(img))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.Background(img.Size, _style.BackgroundColor);
                renderAxis(graphics);
                renderHelplines(graphics);
                renderSeries(graphics);
            }

            return img;
        }


        private PointF getPointOnLine(int lineIndex, float radiusPercent)
        {
            var steps = (2 * Math.PI) / pointCount;
            var angle = steps * lineIndex;
            var radius = lineLength * radiusPercent;
            var x = chartMiddle.X + (Math.Cos(angle) * radius);
            var y = chartMiddle.Y + (Math.Sin(angle) * radius);
            return new PointF((float)x, (float)y);
        }


        private void renderDataArea(Graphics graphics)
        {

        }


        private float scaleValue(DataPoint point)
        {
            return (float)((point.Value - DataSource.MinValue) / DataSource.ScaledMaxValue);
        }


        private void drawDataPoint(Graphics graphics, PointF loc, Color color)
        {
            if (_style.DataDotStyle.Border != null)
            {
                var bwidth = _style.DataDotStyle.Border.Width.GetFloatValue(this.Width);
                graphics.FillCircle(new SolidBrush(_style.DataDotStyle.Border.Color.ReplaceIfTransparent(color)), loc.X, loc.Y, bwidth);
            }

            var width = _style.DataDotStyle.Width.GetFloatValue(this.Width);
            graphics.FillCircle(new SolidBrush(_style.DataDotStyle.Color.ReplaceIfTransparent(color)), loc.X, loc.Y, width);
        }


        private void renderData(Graphics graphics, DataSeries series, Color color)
        {
            if (series.DataPoints.Count == 0)
                return;

            var pen = _style.DataConnectionLineStyle.GetPen(this.chartMinSide);
            pen.Color = pen.Color.ReplaceIfTransparent(color);
            var firstPoint = getPointOnLine(0, scaleValue(series.DataPoints[0]));
            var lastPoint = firstPoint;
            for (int i = 1; i < series.DataPoints.Count; i++)
            {
                var nextPoint = getPointOnLine(i, scaleValue(series.DataPoints[i]));
                graphics.DrawLine(pen, lastPoint, nextPoint);
                drawDataPoint(graphics, lastPoint, color);
                lastPoint = nextPoint;
            }
            graphics.DrawLine(pen, lastPoint, firstPoint);
            drawDataPoint(graphics, lastPoint, color);

        }


        private void renderSeries(Graphics graphics)
        {
            var colors = _style.DataColors;
            colors.Reset();
            foreach(var series in DataSource.DataSeries)
            {
                renderData(graphics, series, colors.Current);
                colors.MoveNext();
            }
        }


        private void renderHelplines(Graphics graphics)
        {
            for (float radPercent = ValueSteps; radPercent < 1 + ValueSteps; radPercent += ValueSteps)
            {
                var lastPoint = getPointOnLine(0, radPercent);
                for (int i = 1; i <= pointCount; i++)
                {
                    var nextPoint = getPointOnLine(i, radPercent);
                    graphics.DrawLine(_style.ThinLineStyle.GetPen(chartMinSide), lastPoint, nextPoint);
                    lastPoint = nextPoint;
                }
            }
        }

        private void renderAxis(Graphics graphics)
        {
            var steps = 360F / pointCount;
            for (int i = 0; i < steps; i++)
            {
                var angle = degreeToRadian(steps * i);
                var middlePoint = chartMiddle;
                var edgePoint = new PointF((float)(middlePoint.X + (Math.Cos(angle) * lineLength)), (float)(middlePoint.Y + (Math.Sin(angle) * lineLength)));
                graphics.DrawLine(_style.AxisLineStyle.GetPen(chartMinSide), middlePoint, edgePoint);
            }
        }

        private double degreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public void SetDataSource(DataCollection source)
        {
            this.DataSource = source;
        }

        public void SetSize(int width, int heigth)
        {
            this.Width = width;
            this.Height = heigth;
        }

        public void SetStyle(ChartStyle style)
        {
            if (style is SpiderChartStyle spiderStyle)
            {
                _style = spiderStyle;
            }
        }
    }
}
