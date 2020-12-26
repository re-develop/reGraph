using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using awoGraph;
using awoGraph.Core;
using awoGraph.Interfaces;
using awoGraph.Layouts;
using Orientation = awoGraph.Layouts.Orientation;

namespace awoGraphTest
{

  public class DummyDrawable<T> : IDrawable<T>
  {
    public SizeF Size { get; private set; }
    public Color Color { get; private set; }

    public DummyDrawable<T> WithSize(float width, float heigh)
    {
      this.Size = new SizeF(width, heigh);
      return this;
    }

    public DummyDrawable<T> WithColor(Color color)
    {
      this.Color = color;
      return this;
    }

    public SizeF CalculateMinSize(float width, float height, IEnumerable<T> data, Graphics graphics)
    {
      return Size;
    }

    public void Render(float width, float height, IEnumerable<T> data, Graphics graphics)
    {
      graphics.FillRectangle(new SolidBrush(Color), 0, 0, Size.Width, Size.Height);
    }
  }

  public class FixedDrawable<T> : IDrawable<T>
  {
    public SizeF Size { get; private set; }
    public Color Color { get; private set; }

    public FixedDrawable<T> WithSize(float width, float heigh)
    {
      this.Size = new SizeF(width, heigh);
      return this;
    }

    public FixedDrawable<T> WithColor(Color color)
    {
      this.Color = color;
      return this;
    }

    public SizeF CalculateMinSize(float width, float height, IEnumerable<T> data, Graphics graphics)
    {
      return Size;
    }

    public void Render(float width, float height, IEnumerable<T> data, Graphics graphics)
    {
      graphics.FillRectangle(new SolidBrush(Color), 0, 0, 3000, 3000);
    }
  }

  public partial class Form1 : Form
  {
    private IDrawable<string> _drawable;

    public Form1()
    {
      InitializeComponent();
      
      _drawable = new SplitLayout<string>()
        .WithOrientation(Orientation.Vertical)
        .WithDrawable<DummyDrawable<string>>(x => x.WithSize(200, 200).WithColor(Color.Black), 0.7F, Alignment.Center, Alignment.Center)
        .WithDrawable<SplitLayout<string>>(x =>
          x.WithOrientation(Orientation.Horizontal)
          .WithDrawable<DummyDrawable<string>>(y => y.WithSize(50, 50).WithColor(Color.Blue), "25%", Alignment.Start, Alignment.Start)
          .WithDrawable<DummyDrawable<string>>(y => y.WithSize(250, 50).WithColor(Color.Yellow), "25%", Alignment.Center, Alignment.Center)
          .WithDrawable<DummyDrawable<string>>(y => y.WithSize(50, 50).WithColor(Color.Green), "25%", Alignment.End, Alignment.End)
          .WithDrawable<DummyDrawable<string>>(y => y.WithSize(50, 50).WithColor(Color.Gray), "25%", Alignment.Center, Alignment.Fit)
        , .30F, Alignment.Center, Alignment.Center);


      this.SizeChanged += Form1_SizeChanged;
    }

    private void Form1_SizeChanged(object sender, EventArgs e)
    {
      render();
    }


    private void render()
    {
      using (var graphics = this.CreateGraphics())
      {
        graphics.Clear(Color.White);
        var size = _drawable.CalculateMinSize(this.ClientSize.Width - 1, this.ClientSize.Height - 1, new string[0], graphics);
        _drawable.Render(this.ClientSize.Width - 1, this.ClientSize.Height - 1, new string[0], graphics);   
      }
    }

  }
}
