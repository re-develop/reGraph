using AeoGraphing;
using AeoGraphing.Charting;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text;

namespace reGraph.Charting
{
    public abstract class Chart2D : Chart
    {
        public override ChartStyle Style => style;
        private Chart2DStyle style { get; set; }
 
        public virtual double? ValueSteps { get; set; }
        public virtual double? BaseValueSteps { get; set; }
        public virtual bool HasValueSteps => ValueSteps != null;
        public virtual bool HasBaseValueStep => BaseValueSteps != null;

        protected virtual int baseLineHeight { get => (int)(style.AxisXPosition.GetFloatValue(this.Height) + (style.AxisTicksLength?.GetFloatValue(this.Height) ?? 0) + style.Padding.GetFloatValue(this.Height) + (style.DrawDataLabels ? style.DataLabelsPosition.GetFloatValue(this.Height) : 0)); }
        protected virtual int valueLineWidth { get => (int)(style.AxisYPosition.GetFloatValue(this.Width) + (style.AxisTicksLength?.GetFloatValue(this.Width) ?? 0) + style.Padding.GetFloatValue(this.Width)); }
        protected virtual double pixelPerBaseValue => (paddedWidth - valueLineWidth) / DataSource.ScaledBaseValue;
        protected virtual double pixelPerValue => (baseLinePos - chartTop) / DataSource.ScaledMaxValue;
        protected virtual float baseLinePos => paddedHeight - baseLineHeight;
        protected virtual float valueLinePos => valueLineWidth;

        public Chart2D(DataCollection data, Chart2DStyle style, int width, int height) : base(data, style, width, height)
        {
            this.style = style;
            ValueSteps = (DataSource.ScaledMaxValue * 0.1);
        }



        public override Image Render()
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
                graphics.Background(img.Size, style.BackgroundColor);
                drawLines(graphics);
                drawBaseLabels(graphics);
                drawValueLabels(graphics);
                drawDataSeries(graphics);
                drawTitleAndDescription(graphics);
                render(graphics);
            }

            return img;
        }


        protected virtual void drawValueLabels(Graphics ctx)
        {
            if (HasValueSteps)
            {
                for (double f = 0; f <= DataSource.ScaledMaxValue; f += ValueSteps.Value)
                {
                    var y = baseLinePos - (float)(f * pixelPerValue);
                    drawValueLabel(ctx, y, (f + DataSource.MinValue).ToString(style.NumericFormat), f == DataSource.MinValue);
                }
            }
            else
            {
                var drawn = new HashSet<float>();
                foreach (var point in DataSource.DataPoints)
                {
                    var y = paddedHeight - (float)(pixelPerValue * (point.Value - DataSource.MinValue)) - baseLineHeight;
                    if (drawn.Contains(y))
                        continue;

                    if (point.HasValueLabel == true)
                        drawValueLabel(ctx, y, point.ValueLabel, point.Value == DataSource.MinValue);
                    else
                        drawValueLabel(ctx, y, point.Value.ToString(style.NumericFormat), point.Value == DataSource.MinValue);
                    drawn.Add(y);
                }
            }
        }



        protected virtual void drawValueLabel(Graphics ctx, float y, string labelContent, bool firstValue = false)
        {
            if (style.DrawAxisTicks.HasFlag(Axis2D.AxisY))
            {
                ctx.DrawLine(style.AxisTicksLineStyle.GetPen(this.Width), new PointF(valueLinePos, y), new PointF(valueLinePos - style.AxisTicksLength.GetFloatValue(this.Width), y));
            }

            if (style.DrawAxisHelpLine.HasFlag(Axis2D.AxisX) && firstValue == false)
            {
                ctx.DrawLine(style.ThinLineStyle.GetPen(this.Width), new PointF(valueLinePos + 1, y), new PointF(valueLinePos + (paddedWidth - valueLineWidth), y));
            }

            StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            var origin = new PointF(valueLinePos - (style.DrawAxisTicks.HasFlag(Axis2D.AxisY) ? style.AxisTicksLength.GetFloatValue(this.Width) : 0) - style.DataCaptionPadding.GetFloatValue(this.Width), y);
            ctx.DrawString(labelContent, style.DataCaptionFont, new SolidBrush(style.TextColor), origin, stringFormat);
        }



        protected virtual void drawTitleAndDescription(Graphics ctx)
        {
            StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var color = new SolidBrush(style.TextColor);
            if (style.DrawTitle)
                ctx.DrawString(DataSource.Title, style.TitleFont, color, new PointF(paddedWidth / 2, titleHeight / 2), stringFormat);

            if (style.DrawDescription)
                ctx.DrawString(DataSource.Description, style.DescriptionFont, color, new PointF(paddedWidth / 2, titleHeight + (descriptionHeight / 2)), stringFormat);

        }



        protected virtual void drawBaseLabel(Graphics ctx, float x, string content)
        {
            if (style.DrawAxisTicks.HasFlag(Axis2D.AxisX))
            {
                ctx.DrawLine(style.AxisTicksLineStyle.GetPen(this.Width), new PointF(x, baseLinePos), new PointF(x, baseLinePos + style.AxisTicksLength.GetFloatValue(this.Height)));
            }

            if (style.DrawAxisHelpLine.HasFlag(Axis2D.AxisY) && x > valueLinePos && x <= paddedWidth)
            {
                ctx.DrawLine(style.ThinLineStyle.GetPen(this.Width), new PointF(x, baseLinePos - 1), new PointF(x, chartTop));
            }

            if (string.IsNullOrEmpty(content) == false)
            {
                var origin = new PointF(x, baseLinePos + (baseLineHeight / 2));
                var state = ctx.Save();
                ctx.ResetTransform();
                ctx.RotateTransform(288);
                origin.Y = baseLinePos + (style.DrawAxisTicks.HasFlag(Axis2D.AxisX) ? style.AxisTicksLength.GetFloatValue(this.Height) : 0) + style.DataCaptionPadding.GetFloatValue(this.Height);
                ctx.TranslateTransform(origin.X, origin.Y, MatrixOrder.Append);
                StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                ctx.DrawString(content, style.DataCaptionFont, new SolidBrush(style.TextColor), 0, 0, stringFormat);
                ctx.Restore(state);
            }

        }



        protected virtual void drawBaseLabels(Graphics ctx)
        {
            if (HasBaseValueStep)
            {
                for (double i = DataSource.MinBaseValue; i <= DataSource.MaxBaseValue; i += BaseValueSteps.Value)
                {
                    var x = (float)(pixelPerBaseValue * (i - DataSource.MinBaseValue)) + valueLineWidth;
                    drawBaseLabel(ctx, x, i.ToString(style.NumericFormat));
                }
            }
            else
            {
                var drawn = new HashSet<double>();
                foreach (var dataPoint in DataSource.DataPoints)
                {
                    if (drawn.Contains(dataPoint.BaseValue.Value))
                        continue;

                    var x = (float)(pixelPerBaseValue * (dataPoint.BaseValue.Value - DataSource.MinBaseValue)) + valueLineWidth;
                    drawBaseLabel(ctx, x, dataPoint.BaseLabel);
                    drawn.Add(dataPoint.BaseValue.Value);
                }
            }
        }



        protected virtual void drawLines(Graphics ctx)
        {
            var pen = style.AxisLineStyle.GetPen(this.Width);
            if (style.DrawAxis.HasFlag(Axis2D.AxisX))
                ctx.DrawLine(pen, new PointF(valueLineWidth, chartTop), new PointF(valueLineWidth, baseLinePos));

            if (style.DrawAxis.HasFlag(Axis2D.AxisY))
                ctx.DrawLine(pen, new PointF(valueLineWidth, baseLinePos), new PointF(paddedWidth, baseLinePos));
        }



        public override void SetStyle(ChartStyle style)
        {
            if (style is Chart2DStyle style2d)
            {
                base.SetStyle(style);
                this.style = style2d;
            }
        }
    }
}
