# Combo box item from mouse position
How to get the index of the combo box item which is currently under the cursor.

The provided code is written in C# for the Windows Forms library of the .NET Framework
and uses native function calls and structs.

This repo provides an example Windows Forms project.



# Native functions and constants
In order to register the mouse on the drop-down list of a combo box we will need two native WinAPI functions 
as well as three native structs.

List of native components:
* ``` POINT structure ```
* ``` RECT structure ```
* ``` COMBOXBOXINFO structure ```
* ``` GetComboBoxInfo function ```
* ``` LBItemFromPt function ```

### The LBItemFromPt function
To get a list box item from a point we have to use the function [LBItemFromPt](https://docs.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-lbitemfrompt). The function requires the following parameters:
* ``` HWND hLB (Handle to list box) ```
* ``` POINT pt (POINT struct that contains the screen coordinates to check) ```
* ``` BOOL bAutoScroll (Scroll flag. Ignore this if you are only interested in getting the items index) ```

The function returns the item index if the point is over a list item, or -1 otherwise.

### The GetComboBoxInfo function
To get informations such as the edit box handle or drop-down list handle we have to use the function [GetComboBoxInfo](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcomboboxinfo). The function requires the following parameters:
* ``` HWND hwndCombo (Handle to the combo box) ```
* ```PCOMBOBOXINFO pcbi (Pointer to a COMBOBOXINFO structure) ```

The function returns a boolean value.
The COMBOBOXINFO structure will be populated and contains all informations needed.

### POINT, RECT and COMBOBOXINFO structs
I recommand just copying the structs from [pinvoke](https://pinvoke.net/).
There is no need to wrap your head around these.
* [POINT](https://pinvoke.net/default.aspx/Structures.POINT)
* [RECT](https://pinvoke.net/default.aspx/Structures.RECT)
* [COMBOBOXINFO](https://pinvoke.net/default.aspx/Structures.COMBOBOXINFO)



# Implementation
Step by step implementation of a combo box class that implements the native functions and structures above.
The code was written in C# with Visual Studio 2017 using the .Net 4.5.2 framework.
You will need the following namespaces:

* System.Runtime.InteropServices
* System.Windows.Forms
* System.Drawing
* System



## Step 1
Create a new class that derives from the System.Windows.Forms.ComboBox class.
I will call it MouseOverItemComboBox.
``` csharp
namespace X.Y.Z
{   
    class MouseOverItemComboBox : ComboBox
    {
    }
}
```

Add the structs above to the same namespace.


## Step 2
Create a custom EventArgs class that suits your needs.

``` csharp
using System;

namespace X.Y.Z
{
    public class MouseOverEventArgs : EventArgs 
    {
        public int Index { get; }
        public MouseOverEventArgs(int index)
        {
            Index = index;
        }
    }
}
```


## Step 3
Implement MouseOver event and native functions.

``` csharp
    class MouseOverItemComboBox : ComboBox
    {
        // Event delegate
        public delegate void OnMouseOverItem(object sender, MouseOverEventArgs e);
        // Event which the parent can subscribe to.
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
        
        
        /* Sent to parent window of a list box before the system draws the list box.
        ** Can set text and background color of the list box by using the specified
        ** device context handle. */
        const int WM_CTLCOLORLISTBOX = 0x0134;


        // Native function that retreives information about the specified combo box.
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcomboboxinfo
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetComboBoxInfo(IntPtr hWnd, [In][Out] ref COMBOBOXINFO pcbi);

        // Native function that retreives the index of the item at the specified point in a list box.
        // https://docs.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-lbitemfrompt
        [DllImport("Comctl32.dll", SetLastError = true)]
        static extern int LBItemFromPt(IntPtr hLB, POINT pt, bool bAutoScroll);
``` 

## Step 4
Override the WndProc method of the parent and implement a helper method that will do the actual job
of getting the list box item index.

``` csharp
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
            // LBItemFromPt will return the Index of the Item on success.
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
```


# Summary
You do not need much code to get this to work nor is it hard.
This code is not limited to combo boxes. Any control that utilizes a list box supports this.
If you cannot get a hold of the list box handle it won't work though.

For the sake of completeness, the whole implementation:
``` csharp
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System;


class MouseOverItemComboBox : ComboBox
{
    // Event delegate
    public delegate void OnMouseOverItem(object sender, MouseOverEventArgs e);
    // Event which the parent can subscribe to.
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
    
    
    /* Sent to parent window of a list box before the system draws the list box.
    ** Can set text and background color of the list box by using the specified
    ** device context handle. */
    const int WM_CTLCOLORLISTBOX = 0x0134;


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
        // LBItemFromPt will return the Index of the Item on success.
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
