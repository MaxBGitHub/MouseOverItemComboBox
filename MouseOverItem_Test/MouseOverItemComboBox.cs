using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System;

namespace MouseOverItem_Test
{

	class MouseOverItemComboBox : ComboBox
	{
		public delegate void OnMouseOverItem(object sender, MouseOverEventArgs e);
		public event OnMouseOverItem MouseOverItem;

		// The possible combo box button states.
		enum ButtonState {
			STATE_SYSTEM_NONE		= 0,			// Button exists and is not pressed.
			STATE_SYSTEM_INVISIBLE	= 0x00008000,	// There is no button.
			STATE_SYSTEM_PRESSED	= 0x00000008,	// Button is pressed.
		}

		/* Native COMBOBOXINFO struct implementation.
		** Contains combo box status information. */
		[StructLayout(LayoutKind.Sequential)]
		struct COMBOBOXINFO
		{
			public int			cbSize;			// Size in bytes of struct.
			public RECT			rcItem;			// RECT that specifies the coordinates of the edit box.
			public RECT			rcButton;		// RECT that specifies the coordinates of the drop-down button.
			public ButtonState	stateButton;	// Drop-down button state.
			public IntPtr		hwndCombo;		// Handle to the combo box.
			public IntPtr		hwndEdit;		// Handle to the edit box.
			public IntPtr		hwndList;		// Handle to the drop-down list.
		}

		const int WM_CTLCOLORLISTBOX    = 0x0134;


		// Native function that retreives information about the specified combo box.
		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcomboboxinfo
		[DllImport("user32.dll", SetLastError = true)]
		static extern bool GetComboBoxInfo(IntPtr hWnd, [In][Out] ref COMBOBOXINFO pcbi);

		// Native function that retreives the index of the item at the specified point in a list box.
		// https://docs.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-lbitemfrompt
		[DllImport("Comctl32.dll", SetLastError = true)]
		static extern int LBItemFromPt(IntPtr hLB, POINT pt, bool bAutoScroll);


		// Helper method which will invoke the MouseOverItem event.
		private void HandleMouseOverItem()
		{
			// cbSize must be set before calling GetComboBoxInfo.
			COMBOBOXINFO pcbi = new COMBOBOXINFO();			
			pcbi.cbSize = Marshal.SizeOf(pcbi);

			// Get combo box information.
			GetComboBoxInfo(Handle, ref pcbi);
			// Check for invalid pointer... just in case.
			if (pcbi.hwndList == IntPtr.Zero)
				return;

			// Current position of cursor.
			POINT pt = Cursor.Position;
			/* LBItemFromPt will return the Index of the Item on success.
			** The documentation states that this function will return zero  
			** if it fails. That is not true though. It will return 0 for the
			** first item in the list box. This function returns -1 on error! */
			int retVal = LBItemFromPt(pcbi.hwndList, pt, false);
			if (retVal == -1)
				return;
			
			// Invoke the event.
			MouseOverItem?.Invoke(this, new MouseOverEventArgs(retVal));
		}


		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				/* This message is sent by the list box of the combo box.
				** It is sent before the system draws the list box.
				** Whenever the cursor enters a list item this message will be
				** sent to the parent i.e. the combox box.
				** NOTE that this message is always sent twice.
				** First time for drawing the default item background.
				** Second time for drawing the highlighted item background. */
				case WM_CTLCOLORLISTBOX:
					{
						// Let the helper method do the rest.
						HandleMouseOverItem();
						base.WndProc(ref m);
						break;
					}
				default:
					{
						base.WndProc(ref m);
						break;
					}
			}
		}
	}
	
}