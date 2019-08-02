using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace AeoGraphing.Charting.ColorGenerators
{
  public class PastelGenerator : IEnumerator<Color>
  {
    public Color PastelMixin { get; set; }
    private Color _current;
    private int _rseed;
    private Random _random;
    public int Seed { get => _rseed; set { _rseed = value; _random = new Random(value); MoveNext(); } }

    public PastelGenerator()
    {

    }

    public PastelGenerator(Color? pastelMixin = null)
    {
      this.PastelMixin = pastelMixin ?? Color.White;
      this._rseed = new Random().Next();
      this._random = new Random(_rseed);
      MoveNext();
    }

    public Color Current => _current;

    object IEnumerator.Current => Current;

    public void Dispose()
    {
      //throw new NotImplementedException();
    }

    public bool MoveNext()
    {
      byte[] bytes = new byte[3];
      _random.NextBytes(bytes);
      Debug.WriteLine(string.Join(" ", bytes));
      _current = Color.FromArgb(bytes[0], bytes[1], bytes[2]).Mix(PastelMixin);
      return true;
    }

    public void Reset()
    {
      this._random = new Random(_rseed);
      Debug.WriteLine($"Seed: {_rseed}, Reset: {_random.Next()}");
      this.MoveNext();
    }
  }
}
