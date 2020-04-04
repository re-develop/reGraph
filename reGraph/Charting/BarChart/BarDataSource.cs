using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace reGraph.Charting.BarChart
{
  public class BarDataSource
  {
    public static BarDataSource ExampleData => new BarDataSource
    {
      Title = "Title",
      SubTitle = "Subtitle",
      Series = new BarSeries[]
      {
        new BarSeries { Name = "Series1", Id = 0 },
        new BarSeries { Name = "Series2", Id = 1 },
        new BarSeries { Name = "Series3", Id = 2 },
      },


      Groups = new BarGroup[]
      {
        new BarGroup
        {
          Name = "Group1",
          Bars = new Bar[]
          {
            new Bar { Label = "Bar0", SeriesId = 0, Value = 70 },
            new Bar { Label = "Bar1", SeriesId = 1, Value = 95 },
            new Bar { Label = "Bar2", SeriesId = 2, Value = 63 },
          }
        },

        new BarGroup
        {
          Name = "Group2",
          Bars = new Bar[]
          {
            new Bar { Label = "Bar4", SeriesId = 0, Value = 30 },
            new Bar { Label = "Bar5", SeriesId = 2, Value = 100 },
          }
        },

        new BarGroup
        {
          Name = "Group3",
          Bars = new Bar[]
          {
            new Bar { Label = "Bar6", SeriesId = 0, Value = 45 },
            new Bar { Label = "Bar7", SeriesId = 1, Value = 75 },
            new Bar { Label = "Bar8", SeriesId = 2, Value = 20 },
          }
        }
      }
    };

    public string Title { get; set; }
    public string SubTitle { get; set; }
    public BarGroup[] Groups { get; set; }
    public BarSeries[] Series { get; set; }
    public IEnumerable<Bar> AllBars => Groups.SelectMany(x => x.Bars);

  }



  public class BarSeries
  {
    public string Name { get; set; }
    public int Id { get; set; }
  }


  public class BarGroup
  {
    public Bar[] Bars { get; set; }
    public string Name { get; set; }
  }

  public class Bar
  {
    public float Value { get; set; }
    public string Label { get; set; }
    public int SeriesId { get; set; }
  }
}
