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
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
	public class Dimension
	{
		private const long serialVersionUID = 1L;

        [DataMember]
        public readonly int Height;

        [DataMember]
        public readonly int Width;

		public Dimension(int width, int height)
		{
			if (width < 0)
			{
				throw new System.ArgumentException("width must not be negative: " + width);
			}
			else if (height < 0)
			{
				throw new System.ArgumentException("height must not be negative: " + height);
			}

			this.Width = width;
			this.Height = height;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is Dimension))
			{
				return false;
			}
			Dimension other = (Dimension) obj;
			if (this.Width != other.Width)
			{
				return false;
			}
			else if (this.Height != other.Height)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Gets the center point of the dimension.
		/// </summary>
		/// <returns> the center point </returns>
		public virtual Point Center
		{
			get
			{
				return new Point((float) this.Width / 2, (float) this.Height / 2);
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + this.Width;
			result = prime * result + this.Height;
			return result;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("width=");
			stringBuilder.Append(this.Width);
			stringBuilder.Append(", height=");
			stringBuilder.Append(this.Height);
			return stringBuilder.ToString();
		}
	}
}