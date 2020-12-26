using awoGraph.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace awoGraph
{
  public class ChartBuilder<T>
  {
    public int Width { get; private set; }
    public int Height { get; private set; }

    public IDrawable<T> Content { get; private set; }

    public IEnumerable<T> Data { get; init; }

    internal ChartBuilder(IEnumerable<T> data)
    {
      this.Data = data;
    }

    public ChartBuilder<T> WithWidth(int width)
    {
      this.Width = width;
      return this;
    }

    public ChartBuilder<T> WithHeight(int heigth)
    {
      this.Height = heigth;
      return this;
    }

    public ChartBuilder<T> WithContent(IDrawable<T> drawable)
    {
      this.Content = drawable;
      return this;
    }

    public ChartBuilder<T> WithContent<D>(Action<D> configure) where D : IDrawable<T>
    {
      var inst = Activator.CreateInstance<D>();
      configure?.Invoke(inst);
      this.Content = inst;
      return this;
    }

    public Bitmap Render()
    {
      var bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      using(var graphics = Graphics.FromImage(bitmap))
      {
        Content.Render(Width, Height, Data, graphics);
      }

      return bitmap;
    }

    public static ChartBuilder<T> Builder(IEnumerable<T> data) => new ChartBuilder<T>(data);
  }
}
