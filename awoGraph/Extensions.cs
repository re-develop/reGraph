using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace awoGraph
{
  public static class Extensions
  {
    public static string RemoveLast(this string @string, int count)
    {
      if (count >= @string.Length)
        return string.Empty;

      return @string.Substring(0, @string.Length - count);
    }
  }
}
