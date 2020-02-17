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
  public abstract class Chart : ISizeable, IChart
  {
    public virtual ChartStyle Style => style;
    private ChartStyle style { get; set; }
    public virtual DataCollection DataSource { get; set; }
    protected Graphics graphics { get; set; }

    public int Width { get; private set; }
    public int Height { get; private set; }


    protected virtual int paddedWidth => Width - style.Padding.GetIntValue(this.Width);
    protected virtual int paddedHeight => Height + style.Padding.GetIntValue(this.Height);
    protected virtual float titleHeight => (style.DrawTitle && string.IsNullOrEmpty(DataSource.Title) == false) ? graphics.MeasureString(DataSource.Title, style.TitleFont).Height + style.Padding.GetFloatValue(this.Height) : 0;
    protected virtual float descriptionHeight => (style.DrawDescription && string.IsNullOrEmpty(DataSource.Description) == false) ? graphics.MeasureString(DataSource.Description, style.DescriptionFont).Height + style.Padding.GetFloatValue(this.Height) : 0;
    protected virtual float chartTop => style.Padding.GetFloatValue(this.Height) + titleHeight + descriptionHeight;



    public Chart(DataCollection data, ChartStyle style, int width, int height)
    {
      this.DataSource = data;
      this.Width = width;
      this.Height = height;
      this.style = style;
    }



    public virtual void Render(Stream stream, ImageFormat format = null)
    {
      using (var img = Render())
      {
        img.Save(stream, format ?? ImageFormat.Png);
      }
    }



    public virtual Image Render()
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
        drawDataSeries(graphics);
        drawTitleAndDescription(graphics);
        render(graphics);
      }

      return img;
    }



    protected abstract void render(Graphics graphics);



    protected virtual RectangleF? drawDataLabel(Graphics graphics, float x, float y, string name, Color color)
    {
      var size = graphics.MeasureString(name, style.DataCaptionFont);
      var squareWidth = Math.Max(style.DataLabelSquare.Width.GetFloatValue(this.Width), style.DataLabelSquare.Border?.Width?.GetFloatValue(this.Width) ?? 0);
      var heigth = Math.Max(size.Height, squareWidth);
      var width = size.Width + style.DataLabelSquarePadding.GetFloatValue(this.Width) + squareWidth + style.DataLabelPadding.GetFloatValue(this.Width);
      if (x + width > paddedWidth)
      {
        x = style.Padding.GetFloatValue(this.Width);
        y += heigth + style.DataLabelPadding.GetFloatValue(this.Height);
      }

      if (y > Height - style.Padding.GetFloatValue(this.Height))
        return null;

      var midPointY = y + (heigth / 2);
      if (style.DataLabelSquare.Border != null)
      {
        var sqbHeight = style.DataLabelSquare.Border.Width.GetFloatValue(this.Width);
        graphics.FillRectangle(new SolidBrush(style.DataLabelSquare.Border.Color.ReplaceIfTransparent(color)), new RectangleF(x + ((squareWidth - sqbHeight) / 2), midPointY - (sqbHeight / 2), sqbHeight, sqbHeight));
      }

      var sqHeight = style.DataLabelSquare.Width.GetFloatValue(this.Width);
      graphics.FillRectangle(new SolidBrush(color), new RectangleF(x + ((squareWidth - sqHeight) / 2), midPointY - (sqHeight / 2), sqHeight, sqHeight));
      StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
      graphics.DrawString(name, style.DataCaptionFont, new SolidBrush(style.DataLabelSquare.Color.ReplaceIfTransparent(color)), new PointF(x + squareWidth + style.DataLabelSquarePadding.GetFloatValue(this.Width), midPointY), stringFormat);

      return new RectangleF(x, y, width, heigth);
    }



    protected virtual void drawDataSeries(Graphics graphics)
    {
      PointF? last = new PointF(style.Padding.GetFloatValue(this.Width), paddedHeight - style.DataLabelsPosition.GetFloatValue(this.Height));
      style.DataColors.Reset();
      foreach (var series in DataSource.DataSeries)
      {
        var color = style.DataColors.Current;
        if (style.DrawDataLabels && last != null)
          last = drawDataLabel(graphics, last.Value.X, last.Value.Y, series.Name, color)?.TopRight();

        style.DataColors.MoveNext();
      }
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



    public void SetSize(int width, int heigth)
    {
      this.Width = width;
      this.Height = heigth;
    }



    public void SetDataSource(DataCollection source)
    {
      this.DataSource = source;
    }



    public virtual void SetStyle(ChartStyle style)
    {
      this.style = style;
    }
  }
}
