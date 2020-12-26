using System;
using System.Collections.Generic;
using System.Text;

namespace awoGraph.Core
{
  public class Measure
  {
    public float Value { get; set; }
    public MeasureType Type { get; set; }

    public Measure(float value, MeasureType type)
    {
      this.Value = value;
      this.Type = type;
    }

    public float GetFloatValue(float toScale)
    {
      switch (Type)
      {
        case MeasureType.Fix:
          return Value;
        case MeasureType.Percentage:
          return Value * toScale;
        default:
          return 0;
      }
    }

    public override string ToString()
    {
      return $"{Value * 100}{(Type == MeasureType.Percentage ? "%" : "")}";
    }

    public static Measure FromString(string @string)
    {
      var type = MeasureType.Fix;
      if (@string.EndsWith("%"))
      {
        type = MeasureType.Percentage;
        @string = @string.Remove(@string.Length - 1);
      }

      if (float.TryParse(@string, out var value) == true)
        return new Measure(type == MeasureType.Percentage ? (value / 100) : value, type);

      return null;
    }

    public int GetIntValue(float toScale)
    {
      return (int)GetFloatValue(toScale);
    }

    public static implicit operator Measure(float measure)
    {
      return new Measure(measure, (measure >= 0 && measure <= 1.0F) ? MeasureType.Percentage : MeasureType.Fix);
    }

    public static implicit operator Measure(int measure)
    {
      return new Measure(measure, MeasureType.Fix);
    }

    public static implicit operator Measure(string measure)
    {
      return FromString(measure);
    }
  }

  public enum MeasureType
  {
    Fix,
    Percentage
  }
}
