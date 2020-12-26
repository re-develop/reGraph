using awoGraph.Core;
using awoGraph.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace awoGraph.Layouts
{

  public enum Alignment
  {
    Fit,
    Center,
    Start,
    End
  }

  public enum Orientation
  {
    Horizontal,
    Vertical
  }


  public class SplitLayout<T> : ILayoutable<T>
  {

    private class SplitContainer
    {
      public IDrawable<T> Drawable { get; init; }

      public Measure Measure { get; init; }

      public Alignment VerticalAlignment { get; init; }
      public Alignment HorizontalAlignment { get; init; }
      public SplitContainer(IDrawable<T> drawable, Measure measure = null, Alignment verticalAlignment = Alignment.Center, Alignment horizontalAlignment = Alignment.Center)
      {
        this.Drawable = drawable;
        this.Measure = measure ?? "100%";
        this.VerticalAlignment = verticalAlignment;
        this.HorizontalAlignment = horizontalAlignment;
      }

    }

    private readonly List<SplitContainer> _containers = new List<SplitContainer>();

    public IEnumerable<IDrawable<T>> Children => _containers.Select(x => x.Drawable);

    public Orientation Orientation { get; private set; }


    public SplitLayout<T> WithOrientation(Orientation orientation)
    {
      this.Orientation = orientation;
      return this;
    }

    public SplitLayout<T> WithDrawable(IDrawable<T> drawable, Measure measure = null, Alignment verticalAlignment = Alignment.Center, Alignment horizontalAlignment = Alignment.Center)
    {
      _containers.Add(new SplitContainer(drawable, measure, verticalAlignment, horizontalAlignment));
      return this;
    }

    public SplitLayout<T> WithDrawable<G>(Action<G> drawable, Measure measure = null, Alignment verticalAlignment = Alignment.Center, Alignment horizontalAlignment = Alignment.Center) where G : IDrawable<T>
    {
      var obj = Activator.CreateInstance<G>();
      drawable?.Invoke(obj);
      _containers.Add(new SplitContainer(obj, measure, verticalAlignment, horizontalAlignment));
      return this;
    }

    private float calculateAlignedSideLength(Alignment align, float maxSide, float childSide)
    {
      switch (align)
      {
        case Alignment.Fit:
          return childSide;

        case Alignment.Center:
        case Alignment.End:
        case Alignment.Start:
          return maxSide;

        default:
          return 0;
      }
    }

    private float calculateAlignedOffset(Alignment align, float containerSize, float childSize)
    {
      switch (align)
      {
        case Alignment.Fit:
        case Alignment.Start:
          return 0;

        case Alignment.Center:
          return (containerSize - childSize) / 2;

        case Alignment.End:
          return containerSize - childSize;

        default:
          return 0;
      }
    }

    private RectangleF calculateMaxCellBounds(SplitContainer container, float width, float height)
    {
      if (Orientation == Orientation.Horizontal)
        return new RectangleF(0, 0, container.Measure.GetFloatValue(width), height);
      else
        return new RectangleF(0, 0, width, container.Measure.GetFloatValue(height));
    }

    private RectangleF calculateActualCellBounds(SplitContainer container, RectangleF maxBounds, SizeF contentSize)
    {
      var width = calculateAlignedSideLength(container.HorizontalAlignment, maxBounds.Width, contentSize.Width);
      var height = calculateAlignedSideLength(container.VerticalAlignment, maxBounds.Height, contentSize.Height);
      return new RectangleF(0, 0, width, height);
    }

    private RectangleF calculateContentBounds(SplitContainer container, RectangleF cellBounds, SizeF contentSize)
    {
      var offsetLeft = Math.Max(calculateAlignedOffset(container.HorizontalAlignment, cellBounds.Width, contentSize.Width), 0);
      var offsetTop = Math.Max(calculateAlignedOffset(container.VerticalAlignment, cellBounds.Height, contentSize.Height), 0);
      return new RectangleF(offsetLeft, offsetTop, Math.Min(contentSize.Width, cellBounds.Width), Math.Min(contentSize.Height, cellBounds.Height));
    }


    private RectangleF calculateHorizontalContainerSize(SplitContainer container, float givenOffset, float width, float height, IEnumerable<T> data, Graphics graphics)
    {
      var maxWidth = container.Measure.GetFloatValue(width);
      var childSize = container.Drawable.CalculateMinSize(maxWidth, height, data, graphics);
      var childWidth = calculateAlignedSideLength(container.HorizontalAlignment, maxWidth, childSize.Width);
      var offset = givenOffset + calculateAlignedOffset(container.HorizontalAlignment, maxWidth, childSize.Width);
      var offsetTop = calculateAlignedOffset(container.VerticalAlignment, (int)height, childSize.Height);
      return new RectangleF(offset, offsetTop, childSize.Width, height);
    }

    private RectangleF calculateVerticalContainerSize(SplitContainer container, float givenOffset, float width, float height, IEnumerable<T> data, Graphics graphics)
    {
      var maxHeigth = container.Measure.GetFloatValue(height);
      var childSize = container.Drawable.CalculateMinSize(width, maxHeigth, data, graphics);
      var childHeight = calculateAlignedSideLength(container.VerticalAlignment, maxHeigth, childSize.Height);
      var offset = givenOffset + calculateAlignedOffset(container.VerticalAlignment, maxHeigth, childSize.Height);
      var offsetLeft = calculateAlignedOffset(container.HorizontalAlignment, (int)width, childSize.Width);
      return new RectangleF(offsetLeft, offset, width, childSize.Height);
    }


    public SizeF CalculateMinSize(float width, float height, IEnumerable<T> data, Graphics graphics)
    {
      var rects = _containers.Select(container =>
      {
        var cell = calculateMaxCellBounds(container, width, height);
        var maxContentSize = container.Drawable.CalculateMinSize(cell.Width, cell.Height, data, graphics);
        var content = calculateContentBounds(container, cell, maxContentSize);
        var actualCell = calculateActualCellBounds(container, cell, maxContentSize);
        return actualCell;
      });

      if (Orientation == Orientation.Horizontal)
      {
        return new SizeF(rects.Select(x => x.Width).Sum(), rects.Max(x => x.Top + x.Height));
      }
      else
      {
        return new SizeF(rects.Max(x => x.Left + x.Width), rects.Select(x => x.Height).Sum());
      }
    }

    private float render(SplitContainer container, float givenOffset, float width, float height, IEnumerable<T> data, Graphics graphics)
    {
      var cell = calculateMaxCellBounds(container, width, height);
      var maxContentSize = container.Drawable.CalculateMinSize(cell.Width, cell.Height, data, graphics);
      var content = calculateContentBounds(container, cell, maxContentSize);
      var actualCell = calculateActualCellBounds(container, cell, maxContentSize);

      var state = graphics.Save();

      if (Orientation == Orientation.Horizontal)
        graphics.TranslateTransform(content.X + givenOffset, content.Y);
      else
        graphics.TranslateTransform(content.X, content.Y + givenOffset);

      graphics.Clip = new Region(new RectangleF(0, 0, content.Width, content.Height));
      container.Drawable.Render(cell.Width, cell.Height, data, graphics);
      graphics.ResetClip();

#if DEBUG
      graphics.DrawRectangle(new Pen(Color.Red, 1F), 0, 0, content.Width, content.Height);
#endif
      graphics.Restore(state);

#if DEBUG
      if (Orientation == Orientation.Horizontal)
      {
        graphics.DrawRectangle(new Pen(Color.Yellow, 2F), actualCell.X + givenOffset, actualCell.Y, actualCell.Width, actualCell.Height);
        graphics.DrawRectangle(new Pen(Color.Orange, 1F), cell.X + givenOffset, cell.Y, cell.Width, cell.Height);
      }
      else
      {
        graphics.DrawRectangle(new Pen(Color.Yellow, 2F), actualCell.X, actualCell.Y + givenOffset, actualCell.Width, actualCell.Height);
        graphics.DrawRectangle(new Pen(Color.Orange, 1F), cell.X, cell.Y + givenOffset, cell.Width, cell.Height);
      }
#endif
      return Orientation == Orientation.Horizontal ? actualCell.Width : actualCell.Height;

      //if (Orientation == Orientation.Horizontal)
      //{
      //  var rect = calculateHorizontalContainerSize(container, givenOffset, width, height, data, graphics);

      //  var state = graphics.Save();
      //  graphics.TranslateTransform(rect.X, rect.Y);
      //  graphics.Clip = new Region(rect);
      //  container.Drawable.Render(rect.Width, rect.Height, data, graphics);
      //  graphics.ResetClip();
      //  graphics.Restore(state);
      //  return rect.Width;
      //}
      //else
      //{
      //  var rect = calculateVerticalContainerSize(container, givenOffset, width, height, data, graphics);

      //  var state = graphics.Save();
      //  graphics.TranslateTransform(rect.X, rect.Y);
      //  graphics.Clip = new Region(rect);
      //  container.Drawable.Render(rect.Width, rect.Height, data, graphics);
      //  graphics.ResetClip();
      //  graphics.Restore(state);
      //  return rect.Height;
      //} 
    }





    public void Render(float width, float height, IEnumerable<T> data, Graphics graphics)
    {
      float offset = 0;
      foreach (var child in _containers)
        offset += render(child, offset, width, height, data, graphics);
    }
  }
}
