using System;
using System.Runtime.Serialization;
using System.Text;

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
	/// <summary>
	/// A MapPosition represents an immutable pair of <seealso cref="LatLong"/> and zoom level.
	/// </summary>
	[DataContract]
	public class MapPosition
	{
		private const long serialVersionUID = 1L;

		/// <summary>
		/// The geographical coordinates of the map center.
		/// </summary>
		public readonly LatLong latLong;

		/// <summary>
		/// The zoom level of the map.
		/// </summary>
		public readonly sbyte zoomLevel;

		/// <param name="latLong">
		///            the geographical coordinates of the map center. </param>
		/// <param name="zoomLevel">
		///            the zoom level of the map. </param>
		/// <exception cref="IllegalArgumentException">
		///             if {@code latLong} is null or {@code zoomLevel} is negative. </exception>
		public MapPosition(LatLong latLong, sbyte zoomLevel)
		{
			if (latLong == null)
			{
				throw new System.ArgumentException("latLong must not be null");
			}
			else if (zoomLevel < 0)
			{
				throw new System.ArgumentException("zoomLevel must not be negative: " + zoomLevel);
			}
			this.latLong = latLong;
			this.zoomLevel = zoomLevel;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is MapPosition))
			{
				return false;
			}
			MapPosition other = (MapPosition) obj;
			if (!this.latLong.Equals(other.latLong))
			{
				return false;
			}
			else if (this.zoomLevel != other.zoomLevel)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + this.latLong.GetHashCode();
			result = prime * result + this.zoomLevel;
			return result;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("latLong=");
			stringBuilder.Append(this.latLong);
			stringBuilder.Append(", zoomLevel=");
			stringBuilder.Append(this.zoomLevel);
			return stringBuilder.ToString();
		}
	}
}