using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AeoGraphing.Charting.Styling
{
    public class ChartStyle
    {
        public Measure Padding { get; set; }
        public string NumericFormat { get; set; }
        public Font TitleFont { get; set; }
        public Font DescriptionFont { get; set; }
        public Font AxisCaptionFont { get; set; }
        public Font DataCaptionFont { get; set; }
        public Measure DataCaptionPadding { get; set; }
        public Color TextColor { get; set; }
        public Color BackgroundColor { get; set; }
        public IEnumerator<Color> DataColors { get; set; }
        public bool DrawTitle { get; set; }
        public bool DrawDescription { get; set; }

        public Measure DataLabelsPosition { get; set; }
        public Measure DataLabelPadding { get; set; }
        public Measure DataLabelSquarePadding { get; set; }
        public BorderedShapeStyle DataLabelSquare { get; set; }
        public bool DrawDataLabels { get; set; }

        public string StyleName { get; set; }

        public override string ToString()
        {
            return StyleName;
        }
    }
}
