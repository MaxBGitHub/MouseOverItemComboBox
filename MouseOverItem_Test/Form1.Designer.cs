namespace MouseOverItem_Test
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
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mouseOverItemComboBox1 = new MouseOverItem_Test.MouseOverItemComboBox();
			this.SuspendLayout();
			// 
			// mouseOverItemComboBox1
			// 
			this.mouseOverItemComboBox1.FormattingEnabled = true;
			this.mouseOverItemComboBox1.Items.AddRange(new object[] {
            "this is a short string",
            "this is a much longer string that won\'t be displayed all the way",
            "another short one",
            "another very long string that won\'t be displayed all the way"});
			this.mouseOverItemComboBox1.Location = new System.Drawing.Point(118, 64);
			this.mouseOverItemComboBox1.Name = "mouseOverItemComboBox1";
			this.mouseOverItemComboBox1.Size = new System.Drawing.Size(168, 21);
			this.mouseOverItemComboBox1.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(420, 140);
			this.Controls.Add(this.mouseOverItemComboBox1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private MouseOverItemComboBox mouseOverItemComboBox1;
	}
}

