using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace awoGraph.Interfaces
{
  public interface ILayoutable<T> : IDrawable<T>
  {
    public IEnumerable<IDrawable<T>> Children { get; }
  }
}
