using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AeoGraphing.Charting.Styling;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AeoGraphingTest
{
  public partial class EditStyleForm : Form
  {
    public ChartStyle Style { get; private set; }
    public EditStyleForm(ChartStyle style)
    {
      InitializeComponent();
      Style = style;
      var settings = new JsonSerializerSettings { Formatting = Formatting.Indented, Converters = new JsonConverter[] { new MeasureConverter(), new ColorHexConverter(), new StringEnumConverter(), new FontConverter() }, TypeNameHandling = TypeNameHandling.Auto };
      jsonBox.Text = JsonConvert.SerializeObject(style, Formatting.Indented, settings);
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void BtnOk_Click(object sender, EventArgs e)
    {
      var txt = jsonBox.Text;
      try
      {
        var settings = new JsonSerializerSettings { Formatting = Formatting.Indented, Converters = new JsonConverter[] { new MeasureConverter(), new ColorHexConverter(), new StringEnumConverter(), new FontConverter() }, TypeNameHandling = TypeNameHandling.Auto };
        Style = (ChartStyle)JsonConvert.DeserializeObject(txt, Style.GetType(), settings);
        DialogResult = DialogResult.OK;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString(), "Error Occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
        DialogResult = DialogResult.None;
      }
      finally
      {
        Close();
      }
    }
  }
}
