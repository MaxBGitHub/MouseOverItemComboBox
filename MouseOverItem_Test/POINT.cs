using System.Drawing;

namespace MouseOverItem_Test
{
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
}
