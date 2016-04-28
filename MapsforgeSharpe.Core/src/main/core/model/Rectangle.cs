/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2016 Dirk Weltz
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

namespace org.mapsforge.core.model
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

		public readonly double bottom;
		public readonly double left;
		public readonly double right;
		public readonly double top;

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

			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		/// <returns> true if this Rectangle contains the given point, false otherwise. </returns>
		public virtual bool Contains(Point point)
		{
			return this.left <= point.x && this.right >= point.x && this.top <= point.y && this.bottom >= point.y;
		}

		public virtual Rectangle Envelope(double padding)
		{
			return new Rectangle(this.left - padding, this.top - padding, this.right + padding, this.bottom + padding);
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
			if (!this.left.Equals(other.left))
			{
				return false;
			}
			else if (!this.top.Equals(other.top))
			{
				return false;
			}
			else if (!this.right.Equals(other.right))
			{
				return false;
			}
			else if (!this.bottom.Equals(other.bottom))
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
				return (this.left + this.right) / 2;
			}
		}

		/// <returns> the vertical center of this Rectangle. </returns>
		public virtual double CenterY
		{
			get
			{
				return (this.top + this.bottom) / 2;
			}
		}

		public virtual double Height
		{
			get
			{
				return this.bottom - this.top;
			}
		}

		public virtual double Width
		{
			get
			{
				return this.right - this.left;
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			long temp;
			temp = BitConverter.DoubleToInt64Bits(this.left);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.top);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.right);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.bottom);
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

			return this.left <= rectangle.right && rectangle.left <= this.right && this.top <= rectangle.bottom && rectangle.top <= this.bottom;
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
			if (origin.x == 0 && origin.y == 0)
			{
				return this;
			}
			return new Rectangle(this.left + origin.x, this.top + origin.y, this.right + origin.x, this.bottom + origin.y);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("left=");
			stringBuilder.Append(this.left);
			stringBuilder.Append(", top=");
			stringBuilder.Append(this.top);
			stringBuilder.Append(", right=");
			stringBuilder.Append(this.right);
			stringBuilder.Append(", bottom=");
			stringBuilder.Append(this.bottom);
			return stringBuilder.ToString();
		}
	}
}