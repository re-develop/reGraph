using AeoGraphing.Charting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace reGraph.Charting.ColorGenerators
{
    public class HarmonicContrastGenerator : IEnumerator<Color>
    {
        public double Hue { get; set; }
        public double Saturation { get; set; }
        public double Value { get; set; }
        public double Stepsize { get; set; }
        public double CurrentHue { get; set; }

        private Color currentColor;
        private Random _random = new Random();
        private int _rseed;

        public int Seed { get => _rseed; set { _rseed = value; _random = new Random(value); MoveNext(); } }

        public Color Current => currentColor;
        object IEnumerator.Current => Current;

        public HarmonicContrastGenerator(double saturation, double value, double stepsize)
        {
            this.Hue = _random.NextDouble() * 360;
            this.Saturation = saturation;
            this.Value = value;
            this.Stepsize = stepsize;
            this.CurrentHue = Hue;
            var hsv = new HSV(CurrentHue, saturation, value);
            currentColor = hsv.ToRgb();
        }

        public bool MoveNext()
        {
            CurrentHue += Stepsize;
            var hsv = new HSV(CurrentHue, Saturation, Value);
            currentColor = hsv.ToRgb();
            return true;
        }

        public void Reset()
        {
            CurrentHue = Hue;
            this._random = new Random(_rseed);
            var hsv = new HSV(CurrentHue, Saturation, Value);
            currentColor = hsv.ToRgb();
        }

        public void Dispose()
        {

        }
    }
}
