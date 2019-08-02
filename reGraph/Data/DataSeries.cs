using System;
using System.Collections.Generic;
using System.Text;

namespace AeoGraphing.Data
{
  public class DataSeries
  {
    public string Name { get; private set; }
    public List<DataPoint> DataPoints { get; private set; } = new List<DataPoint>();
    public DataSeries(string name, params DataPoint[] points)
    {
      this.Name = name;
      DataPoints.AddRange(points);
    }
  }
}
