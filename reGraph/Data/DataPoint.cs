using System;
using System.Collections.Generic;
using System.Text;

namespace AeoGraphing.Data
{
  public class DataPoint
  {
    public double Value { get; private set; }
    public double? BaseValue { get; private set; }

    public string BaseLabel { get; private set; }
    public string ValueLabel { get; private set; }

    public bool HasBaseLabel => string.IsNullOrEmpty( BaseLabel ) == false;
    public bool HasValueLabel => string.IsNullOrEmpty( ValueLabel ) == false;

    public DataPoint( double value, double? baseValue = null ) : this( value, null, baseValue, null )
    {
    }

    public DataPoint( double value, string valueLabel ) : this( value, valueLabel, null, null )
    {
    }

    public DataPoint( double value, double baseValue, string baseLabel ) : this( value, null, baseValue, baseLabel )
    {
    }

    public DataPoint( double value, string valueLabel, double? baseValue, string baseLabel )
    {
      this.BaseLabel = baseLabel;
      this.ValueLabel = valueLabel;
      this.Value = value;
      this.BaseValue = baseValue;
    }
  }
}
