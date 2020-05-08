# MouseOverItemComboBox
How to get the index of the combo box item which is currently under the cursor.

The provided code is written in C# for the Windows Forms library of the .NET Framework
and uses native function calls and structs.

This repo provides an example Windows Forms project.


# The important piece of code
```csharp
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System;

class MouseOverItemComboBox : ComboBox
{
    public delegate void OnMouseOverItem(object sender, MouseOverEventArgs e);
    public event OnMouseOverItem MouseOverItem;

    // The possible combo box button states.
    enum ButtonState {
        STATE_SYSTEM_NONE       = 0,            // Button exists and is not pressed.
        STATE_SYSTEM_INVISIBLE  = 0x00008000,   // There is no button.
        STATE_SYSTEM_PRESSED    = 0x00000008,   // Button is pressed.
    }

    /* Native COMBOBOXINFO struct implementation.
    ** Contains combo box status information. */
    [StructLayout(LayoutKind.Sequential)]
    struct COMBOBOXINFO
    {
        public int          cbSize;         // Size in bytes of struct.
        public RECT         rcItem;         // RECT that specifies the coordinates of the edit box.
        public RECT         rcButton;       // RECT that specifies the coordinates of the drop-down button.
        public ButtonState  stateButton;    // Drop-down button state.
        public IntPtr       hwndCombo;      // Handle to the combo box.
        public IntPtr       hwndEdit;       // Handle to the edit box.
        public IntPtr       hwndList;       // Handle to the drop-down list.
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
```


# Full code with RECT and POINT struct in one piece
``` csharp
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System;


class MouseOverItemComboBox : ComboBox
{
    public delegate void OnMouseOverItem(object sender, MouseOverEventArgs e);
    public event OnMouseOverItem MouseOverItem;

    // The possible combo box button states.
    enum ButtonState {
        STATE_SYSTEM_NONE       = 0,            // Button exists and is not pressed.
        STATE_SYSTEM_INVISIBLE  = 0x00008000,   // There is no button.
        STATE_SYSTEM_PRESSED    = 0x00000008,   // Button is pressed.
    }

    /* Native COMBOBOXINFO struct implementation.
    ** Contains combo box status information. */
    [StructLayout(LayoutKind.Sequential)]
    struct COMBOBOXINFO
    {
        public int          cbSize;         // Size in bytes of struct.
        public RECT         rcItem;         // RECT that specifies the coordinates of the edit box.
        public RECT         rcButton;       // RECT that specifies the coordinates of the drop-down button.
        public ButtonState  stateButton;    // Drop-down button state.
        public IntPtr       hwndCombo;      // Handle to the combo box.
        public IntPtr       hwndEdit;       // Handle to the edit box.
        public IntPtr       hwndList;       // Handle to the drop-down list.
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


public class MouseOverEventArgs : EventArgs 
{
    public int Index { get; }
    public MouseOverEventArgs(int index) 
    {
        Index = index;
    }
}


/// <summary>
/// Native RECT struct implementation
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left, Top, Right, Bottom;

    public RECT(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

    public int X
    {
        get { return Left; }
        set { Right -= (Left - value); Left = value; }
    }

    public int Y
    {
        get { return Top; }
        set { Bottom -= (Top - value); Top = value; }
    }

    public int Height
    {
        get { return Bottom - Top; }
        set { Bottom = value + Top; }
    }

    public int Width
    {
        get { return Right - Left; }
        set { Right = value + Left; }
    }

    public Point Location
    {
        get { return new Point(Left, Top); }
        set { X = value.X; Y = value.Y; }
    }

    public Size Size
    {
        get { return new Size(Width, Height); }
        set { Width = value.Width; Height = value.Height; }
    }

    public static implicit operator Rectangle(RECT r)
    {
        return new Rectangle(r.Left, r.Top, r.Width, r.Height);
    }

    public static implicit operator RECT(Rectangle r)
    {
        return new RECT(r);
    }

    public static bool operator ==(RECT r1, RECT r2)
    {
        return r1.Equals(r2);
    }

    public static bool operator !=(RECT r1, RECT r2)
    {
        return !r1.Equals(r2);
    }

    public bool Equals(RECT r)
    {
        return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
    }

    public override bool Equals(object obj)
    {
        if (obj is RECT)
            return Equals((RECT)obj);
        else if (obj is Rectangle)
            return Equals(new RECT((Rectangle)obj));

        return false;
    }

    public override int GetHashCode()
    {
        return ((Rectangle)this).GetHashCode();
    }

    public override string ToString()
    {
        return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
    }
}


/// <summary>
///  Native POINT struct implementation.
/// </summary>
public struct POINT
{
    public int X;
    public int Y;

    public POINT(Point p)
    {
        X = p.X;
        Y = p.Y;
    }

    public POINT(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Point(POINT p)
    {
        return new Point(p.X, p.Y);
    }

    public static implicit operator POINT(Point p)
    {
        return new POINT(p.X, p.Y);
    }

    public static bool operator ==(POINT p1, POINT p2)
    {
        return (p1.X == p2.X && p1.Y == p2.Y);
    }

    public static bool operator !=(POINT p1, POINT p2)
    {
        return (p1.X == p2.X && p1.Y == p2.Y);
    }

    public bool Equals(POINT p)
    {
        return X == p.X && Y == p.Y;
    }

    public override bool Equals(object obj)
    {
        if (obj is POINT)
            return Equals((POINT)obj);
        else if (obj is Point)
            return Equals(new POINT((Point)obj));

        return false;
    }

    public override int GetHashCode()
    {
        return ((POINT)this).GetHashCode();
    }

    public override string ToString()
    {
        return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{X={0},Y={1}}}", X, Y);
    }
}
```
