using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseOverItem_Test
{
	public partial class Form1 : Form
	{
		ToolTip _itemTip;
		int _lastItemIndex = -1;


		private void ComboBox_MouseOverItem(object sender, MouseOverEventArgs e)
		{
			var cb = sender as ComboBox;
			if (cb == null || e.Index < 0)
				return;

			/* Validate the item index.
			** Don't forget that the MouseOverItem event will be fired twice 
			** every time you move the cursor into the items rectangle! */
			if (_lastItemIndex == e.Index)
				return;
			else
				_lastItemIndex = e.Index;

			// Make sure to clean up the old tool tip.
			_itemTip?.Dispose();

			// Measure the length of the text and check if its wider than the combo box.
			string sItem = cb.Items[e.Index].ToString();
			int itemWidth = TextRenderer.MeasureText(sItem, cb.Font).Width;
			if (itemWidth <= cb.Width - SystemInformation.VerticalScrollBarWidth)
				return;

			var pt = cb.PointToClient(Cursor.Position);
			_itemTip = new ToolTip();
			/* Increment the X coordinate by 10 to prevent the cursor
			** from overlapping the tool tip */
			_itemTip.Show(sItem, cb, pt.X + 10, pt.Y);
		}


		public Form1()
		{
			InitializeComponent();

			mouseOverItemComboBox1.MouseOverItem += ComboBox_MouseOverItem;
		}		
	}
}
