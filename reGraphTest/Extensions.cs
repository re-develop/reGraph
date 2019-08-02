using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeoGraphingTest
{
  public static class Extensions
  {
    public static double TotalMilliseconds( this DateTime dt ) => dt.Ticks / 10000;
    public static double TotalSeconds( this DateTime dt ) => dt.TotalMilliseconds() / 1000;
    public static double TotalMinutes( this DateTime dt ) => dt.TotalSeconds() / 60;
    public static double TotalHours( this DateTime dt ) => dt.TotalMinutes() / 60;
    public static double TotalDays( this DateTime dt ) => dt.TotalHours() / 24;
  }
}
