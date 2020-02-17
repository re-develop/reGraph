using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using AeoGraphing.Charting.Styling;
using AeoGraphing.Data;

namespace reGraph.Charting
{
  public interface IChart
  {
    void Render(Stream steam, ImageFormat format = null);
    Image Render();
    void SetSize(int width, int heigth);
    void SetDataSource(DataCollection source);
    void SetStyle(ChartStyle style);
    ChartStyle Style { get; }
    DataCollection DataSource { get; }
  }
}
