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
            }

            return img;
        }


        private void renderHelplines(Graphics graphics)
        {
            var steps = 360F / pointCount;
            var middlePoint = chartMiddle;

            for (float radPercent = ValueSteps; radPercent < 1 + ValueSteps; radPercent += ValueSteps)
            {
                var radius = lineLength * radPercent;
                var lastPoint = new PointF((float)(middlePoint.X + (Math.Cos(0) * radius)), (float)(middlePoint.Y + (Math.Sin(0) * radius)));
                for (int i = 1; i <= steps; i++)
                {
                    var angle = degreeToRadian(steps * i);
                    var nextPoint = new PointF((float)(middlePoint.X + (Math.Cos(angle) * radius)), (float)(middlePoint.Y + (Math.Sin(angle) * radius)));
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
