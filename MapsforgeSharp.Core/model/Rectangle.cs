/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2016 Dirk Weltz
 * Copyright 2016 Michael Oed
 *
 * This program is free software: you can redistribute it and/or modify it under the
 * terms of the GNU Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace MapsforgeSharp.Core.Model
{
    using System;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// A Rectangle represents an immutable set of four double coordinates.
    /// </summary>
    [DataContract]
	public class Rectangle
	{
		private const long serialVersionUID = 1L;

        [DataMember]
        public readonly double Bottom;

        [DataMember]
        public readonly double Left;

        [DataMember]
        public readonly double Right;

        [DataMember]
        public readonly double Top;

		public Rectangle(double left, double top, double right, double bottom)
		{
			if (left > right)
			{
				throw new System.ArgumentException("left: " + left + ", right: " + right);
			}
			else if (top > bottom)
			{
				throw new System.ArgumentException("top: " + top + ", bottom: " + bottom);
			}

			this.Left = left;
			this.Top = top;
			this.Right = right;
			this.Bottom = bottom;
		}

		/// <returns> true if this Rectangle contains the given point, false otherwise. </returns>
		public virtual bool Contains(Point point)
		{
			return this.Left <= point.X && this.Right >= point.X && this.Top <= point.Y && this.Bottom >= point.Y;
		}

		public virtual Rectangle Envelope(double padding)
		{
			return new Rectangle(this.Left - padding, this.Top - padding, this.Right + padding, this.Bottom + padding);
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is Rectangle))
			{
				return false;
			}
			Rectangle other = (Rectangle) obj;
			if (!this.Left.Equals(other.Left))
			{
				return false;
			}
			else if (!this.Top.Equals(other.Top))
			{
				return false;
			}
			else if (!this.Right.Equals(other.Right))
			{
				return false;
			}
			else if (!this.Bottom.Equals(other.Bottom))
			{
				return false;
			}
			return true;
		}

		/// <returns> a new Point at the horizontal and vertical center of this Rectangle. </returns>
		public virtual Point Center
		{
			get
			{
				return new Point(CenterX, CenterY);
			}
		}

		/// <returns> the horizontal center of this Rectangle. </returns>
		public virtual double CenterX
		{
			get
			{
				return (this.Left + this.Right) / 2;
			}
		}

		/// <returns> the vertical center of this Rectangle. </returns>
		public virtual double CenterY
		{
			get
			{
				return (this.Top + this.Bottom) / 2;
			}
		}

		public virtual double Height
		{
			get
			{
				return this.Bottom - this.Top;
			}
		}

		public virtual double Width
		{
			get
			{
				return this.Right - this.Left;
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			long temp;
			temp = BitConverter.DoubleToInt64Bits(this.Left);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.Top);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.Right);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.Bottom);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			return result;
		}

		/// <returns> true if this Rectangle intersects with the given Rectangle, false otherwise. </returns>
		public virtual bool Intersects(Rectangle rectangle)
		{
			if (this == rectangle)
			{
				return true;
			}

			return this.Left <= rectangle.Right && rectangle.Left <= this.Right && this.Top <= rectangle.Bottom && rectangle.Top <= this.Bottom;
		}

		public virtual bool IntersectsCircle(double pointX, double pointY, double radius)
		{
			double halfWidth = Width / 2;
			double halfHeight = Height / 2;

			double centerDistanceX = Math.Abs(pointX - CenterX);
			double centerDistanceY = Math.Abs(pointY - CenterY);

			// is the circle is far enough away from the rectangle?
			if (centerDistanceX > halfWidth + radius)
			{
				return false;
			}
			else if (centerDistanceY > halfHeight + radius)
			{
				return false;
			}

			// is the circle close enough to the rectangle?
			if (centerDistanceX <= halfWidth)
			{
				return true;
			}
			else if (centerDistanceY <= halfHeight)
			{
				return true;
			}

			double cornerDistanceX = centerDistanceX - halfWidth;
			double cornerDistanceY = centerDistanceY - halfHeight;
			return cornerDistanceX * cornerDistanceX + cornerDistanceY * cornerDistanceY <= radius * radius;
		}

		public virtual Rectangle Shift(Point origin)
		{
			if (origin.X == 0 && origin.Y == 0)
			{
				return this;
			}
			return new Rectangle(this.Left + origin.X, this.Top + origin.Y, this.Right + origin.X, this.Bottom + origin.Y);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("left=");
			stringBuilder.Append(this.Left);
			stringBuilder.Append(", top=");
			stringBuilder.Append(this.Top);
			stringBuilder.Append(", right=");
			stringBuilder.Append(this.Right);
			stringBuilder.Append(", bottom=");
			stringBuilder.Append(this.Bottom);
			return stringBuilder.ToString();
		}
	}
}