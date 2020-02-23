using System;
using System.Collections.Generic;
using System.Text;

namespace AeoGraphing.Data
{
  public class DataPoint
  {
    public double Value { get; set; }
    public double? BaseValue { get; set; }

    public string BaseLabel { get; set; }
    public string ValueLabel { get; set; }

    public bool HasBaseLabel => string.IsNullOrEmpty(BaseLabel) == false;
    public bool HasValueLabel => string.IsNullOrEmpty(ValueLabel) == false;

    public DataPoint(double value, double? baseValue = null) : this(value, null, baseValue, null)
    {
    }

    public DataPoint(double value, string valueLabel) : this(value, valueLabel, null, null)
    {
    }

    public DataPoint(double value, double baseValue, string baseLabel) : this(value, null, baseValue, baseLabel)
    {
    }

    public DataPoint(double value, string valueLabel, double? baseValue, string baseLabel)
    {
      this.BaseLabel = baseLabel;
      this.ValueLabel = valueLabel;
      this.Value = value;
      this.BaseValue = baseValue;
    }
  }
}
