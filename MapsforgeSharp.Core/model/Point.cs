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
    /// A Point represents an immutable pair of double coordinates.
    /// </summary>
    [DataContract]
	public class Point : IComparable<Point>
	{
		private const long serialVersionUID = 1L;

        /// <summary>
        /// The x coordinate of this point.
        /// </summary>
        [DataMember]
        public readonly double X;

        /// <summary>
        /// The y coordinate of this point.
        /// </summary>
        [DataMember]
        public readonly double Y;

		/// <param name="x">
		///            the x coordinate of this point. </param>
		/// <param name="y">
		///            the y coordinate of this point. </param>
		public Point(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		public virtual int CompareTo(Point point)
		{
			if (this.X > point.X)
			{
				return 1;
			}
			else if (this.X < point.X)
			{
				return -1;
			}
			else if (this.Y > point.Y)
			{
				return 1;
			}
			else if (this.Y < point.Y)
			{
				return -1;
			}
			return 0;
		}

		/// <returns> the euclidian distance from this point to the given point. </returns>
		public virtual double Distance(Point point)
		{
			return Math.Sqrt(Math.Pow(this.X - point.X, 2) + Math.Pow(this.Y - point.Y, 2));
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is Point))
			{
				return false;
			}
			Point other = (Point) obj;
			if (!this.X.Equals(other.X))
			{
				return false;
			}
			else if (!this.Y.Equals(other.Y))
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			long temp;
			temp = BitConverter.DoubleToInt64Bits(this.X);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.Y);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			return result;
		}

		public virtual Point Offset(double dx, double dy)
		{
			if (0 == dx && 0 == dy)
			{
				return this;
			}
			return new Point(this.X + dx, this.Y + dy);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("x=");
			stringBuilder.Append(this.X);
			stringBuilder.Append(", y=");
			stringBuilder.Append(this.Y);
			return stringBuilder.ToString();
		}
	}
}