using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeoGraphing.Data
{
  public class DataCollection
  {
    private List<DataSeries> _dataSeries = new List<DataSeries>();

    public List<DataPoint> DataPoints => _dataSeries.SelectMany(x => x.DataPoints).ToList();
    public List<DataSeries> DataSeries => _dataSeries;
    public List<string> DataGroupNames { get; } = new List<string>();
    public List<double> DataGroupValues { get; } = new List<double>();
    public bool HasGrouping => DataGroupValues.Count > 0;

    private double? _maxBaseValue;
    private double? _maxValue;
    public double? MinMaxValue { get; set; } = null;
    public double? MinMaxBaseValue { get; set; } = null;
    public double? MinMinValue { get; set; } = null;
    public double? MinMinBaseValue { get; set; } = null;
    public double MaxBaseValue
    {
      get => Math.Max(_maxBaseValue ?? DataPoints.Max(x => x.BaseValue ?? 0), MinMaxBaseValue ?? DataPoints.Max(x => x.BaseValue ?? 0));
      set
      {
        if (value < 0)
          _maxBaseValue = null;
        else
          _maxBaseValue = value;
      }
    }

    public double MaxValue
    {
      get => Math.Max(_maxValue ?? DataPoints.Max(x => x.Value), MinMaxValue ?? DataPoints.Max(x => x.Value));
      set
      {
        if (value < 0)
          _maxValue = null;
        else
          _maxValue = value;
      }
    }

    public double MinBaseValue
    {
      get => Math.Min(DataPoints.Min(x => x.BaseValue ?? 0), MinMinBaseValue ?? DataPoints.Min(x => x.BaseValue ?? 0));
    }

    public double MinValue
    {
      get => Math.Min(DataPoints.Min(x => x.Value), MinMinValue ?? DataPoints.Min(x => x.Value));
    }

    public double ScaledMaxValue => MaxValue - MinValue;
    public double ScaledBaseValue => MaxBaseValue - MinBaseValue;

    public string Title { get; set; }
    public string Description { get; set; }

    public string XAxisCaption { get; set; }
    public string YAxisCaption { get; set; }
    public DataCollection(params DataSeries[] dataSeries) : this(null, null, null, null, dataSeries)
    {

    }

    public DataCollection(double? maxValue = null, double? maxBaseValue = null, params DataSeries[] dataSeries) : this(null, null, maxValue, maxBaseValue, dataSeries)
    {

    }

    public DataCollection(string title, string description, double? maxValue, double? maxBaseValue, params DataSeries[] dataSeries)
    {
      _dataSeries.AddRange(dataSeries);
      this.Title = title;
      this.Description = description;
      this._maxValue = maxValue;
      this._maxBaseValue = maxBaseValue;
    }

  }
}
