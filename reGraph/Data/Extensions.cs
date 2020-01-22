using System;
using System.Collections.Generic;
using System.Text;

namespace reGraph.Data
{
  public static class Extensions
  {

    public static DateTime RoundUp(this DateTime dt, TimeSpan d)
    {
      return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
    }


    public static string[] SplitIgnore(this string @string, char split, char open, char close)
    {
      List<string> res = new List<string>();
      bool dontSplit = false;
      int length = 0;
      int index = 0;
      foreach (char c in @string)
      {
        if (c == open)
          dontSplit = true;

        if (c == close)
          dontSplit = false;

        if (c == split && dontSplit == false)
        {
          res.Add(@string.Substring(index, length));
          index += length + 1;
          length = 0;
        }
        else
        {
          length++;
        }
      }

      if (length > 0)
        res.Add(@string.Substring(index, length));

      return res.ToArray();
    }
  }
}
