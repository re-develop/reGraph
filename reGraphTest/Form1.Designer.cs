namespace AeoGraphingTest
{
  partial class Form1
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose( bool disposing )
    {
      if( disposing && ( components != null ) )
      {
        components.Dispose();
      }
      base.Dispose( disposing );
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnNewData = new System.Windows.Forms.Button();
            this.styleBox = new System.Windows.Forms.ComboBox();
            this.btnEditStyle = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPage);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Location = new System.Drawing.Point(13, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1118, 641);
            this.tabControl.TabIndex = 1;
            // 
            // tabPage
            // 
            this.tabPage.Location = new System.Drawing.Point(4, 22);
            this.tabPage.Name = "tabPage";
            this.tabPage.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage.Size = new System.Drawing.Size(1110, 615);
            this.tabPage.TabIndex = 0;
            this.tabPage.Text = "tabPage1";
            this.tabPage.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1110, 615);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnNewData
            // 
            this.btnNewData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNewData.Location = new System.Drawing.Point(13, 660);
            this.btnNewData.Name = "btnNewData";
            this.btnNewData.Size = new System.Drawing.Size(75, 23);
            this.btnNewData.TabIndex = 2;
            this.btnNewData.Text = "New Data";
            this.btnNewData.UseVisualStyleBackColor = true;
            this.btnNewData.Click += new System.EventHandler(this.BtnNewData_Click);
            // 
            // styleBox
            // 
            this.styleBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.styleBox.FormattingEnabled = true;
            this.styleBox.Location = new System.Drawing.Point(95, 661);
            this.styleBox.Name = "styleBox";
            this.styleBox.Size = new System.Drawing.Size(205, 21);
            this.styleBox.TabIndex = 3;
            // 
            // btnEditStyle
            // 
            this.btnEditStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditStyle.Location = new System.Drawing.Point(1051, 652);
            this.btnEditStyle.Name = "btnEditStyle";
            this.btnEditStyle.Size = new System.Drawing.Size(75, 23);
            this.btnEditStyle.TabIndex = 4;
            this.btnEditStyle.Text = "Edit Style";
            this.btnEditStyle.UseVisualStyleBackColor = true;
            this.btnEditStyle.Click += new System.EventHandler(this.BtnEditStyle_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1143, 687);
            this.Controls.Add(this.btnEditStyle);
            this.Controls.Add(this.styleBox);
            this.Controls.Add(this.btnNewData);
            this.Controls.Add(this.tabControl);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);

    }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnNewData;
        private System.Windows.Forms.ComboBox styleBox;
        private System.Windows.Forms.Button btnEditStyle;
    }
}

