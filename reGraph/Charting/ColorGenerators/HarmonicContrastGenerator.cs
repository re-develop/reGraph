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
        private double hue, saturation, value, stepsize, current;
        private Color currentColor;
        private Random random = new Random();

        public Color Current => currentColor;
        object IEnumerator.Current => Current;

        public HarmonicContrastGenerator(double saturation, double value, double stepsize)
        {
            this.hue = random.NextDouble() * 360;
            this.saturation = saturation;
            this.value = value;
            this.stepsize = stepsize;
            this.current = hue;
            var hsv = new HSV(current, saturation, value);
            currentColor = hsv.ToRgb();
        }

        public bool MoveNext()
        {
            current += stepsize;
            var hsv = new HSV(current, saturation, value);
            currentColor = hsv.ToRgb();
            return true;
        }

        public void Reset()
        {
            current = hue;
            var hsv = new HSV(current, saturation, value);
            currentColor = hsv.ToRgb();
        }

        public void Dispose()
        {

        }
    }
}
