using System;

namespace MouseOverItem_Test
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
