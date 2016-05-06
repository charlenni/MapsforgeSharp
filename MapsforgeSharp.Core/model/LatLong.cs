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
    using System.Text;
    using System.Runtime.Serialization;

    using LatLongUtils = org.mapsforge.core.util.LatLongUtils;

    /// <summary>
    /// A LatLong represents an immutable pair of latitude and longitude coordinates.
    /// </summary>
    [DataContract]
	public class LatLong : IComparable<LatLong>
	{
		private const long serialVersionUID = 1L;

        /// <summary>
        /// The latitude coordinate of this LatLong in degrees.
        /// </summary>
        [DataMember]
        public readonly double Latitude;

        /// <summary>
        /// The longitude coordinate of this LatLong in degrees.
        /// </summary>
        [DataMember]
        public readonly double Longitude;

		/// <param name="latitude">
		///            the latitude coordinate in degrees. </param>
		/// <param name="longitude">
		///            the longitude coordinate in degrees. </param>
		/// <param name="validate"> </param>
		/// <exception cref="IllegalArgumentException">
		///             if a coordinate is invalid. </exception>
		public LatLong(double latitude, double longitude, bool validate)
		{
			if (validate)
			{
				LatLongUtils.ValidateLatitude(latitude);
				LatLongUtils.ValidateLongitude(longitude);
			}

			this.Latitude = latitude;
			this.Longitude = longitude;
		}

		public LatLong(double latitude, double longitude)
		{
			this.Latitude = latitude;
			this.Longitude = longitude;
		}

		public virtual int CompareTo(LatLong latLong)
		{
			if (this.Longitude > latLong.Longitude)
			{
				return 1;
			}
			else if (this.Longitude < latLong.Longitude)
			{
				return -1;
			}
			else if (this.Latitude > latLong.Latitude)
			{
				return 1;
			}
			else if (this.Latitude < latLong.Latitude)
			{
				return -1;
			}
			return 0;
		}

		/// <summary>
		/// Returns the approximate distance in degrees between this location and the
		/// given location, calculated in Euclidean space.
		/// </summary>
		public virtual double Distance(LatLong other)
		{
			return Math.Sqrt(Math.Pow(this.Longitude - other.Longitude, 2) + Math.Pow(this.Latitude - other.Latitude, 2));
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is LatLong))
			{
				return false;
			}
			LatLong other = (LatLong) obj;
			if (!this.Latitude.Equals(other.Latitude))
			{
				return false;
			}
			else if (!this.Longitude.Equals(other.Longitude))
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
			temp = BitConverter.DoubleToInt64Bits(this.Latitude);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.Longitude);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			return result;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("latitude=");
			stringBuilder.Append(this.Latitude);
			stringBuilder.Append(", longitude=");
			stringBuilder.Append(this.Longitude);
			return stringBuilder.ToString();
		}
	}

}