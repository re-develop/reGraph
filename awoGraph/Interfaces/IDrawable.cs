using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace awoGraph.Interfaces
{
  public interface IDrawable<T>
  {
    public void Render(float width, float height, IEnumerable<T> data, Graphics graphics);
    public SizeF CalculateMinSize(float width, float height, IEnumerable<T> data, Graphics graphics);
  }
}
