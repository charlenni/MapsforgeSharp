/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Christian Pesch
 * Copyright 2015 devemux86
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
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;

    using LatLongUtils = org.mapsforge.core.util.LatLongUtils;
    using MercatorProjection = org.mapsforge.core.util.MercatorProjection;

    /// <summary>
    /// A BoundingBox represents an immutable set of two latitude and two longitude coordinates.
    /// </summary>
    [DataContract]
	public class BoundingBox
	{
		private const long serialVersionUID = 1L;

		/// <summary>
		/// Creates a new BoundingBox from a comma-separated string of coordinates in the order minLat, minLon, maxLat,
		/// maxLon. All coordinate values must be in degrees.
		/// </summary>
		/// <param name="boundingBoxString">
		///            the string that describes the BoundingBox. </param>
		/// <returns> a new BoundingBox with the given coordinates. </returns>
		/// <exception cref="IllegalArgumentException">
		///             if the string cannot be parsed or describes an invalid BoundingBox. </exception>
		public static BoundingBox FromString(string boundingBoxString)
		{
			double[] coordinates = LatLongUtils.ParseCoordinateString(boundingBoxString, 4);
			return new BoundingBox(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
		}

		/// <summary>
		/// The maximum latitude coordinate of this BoundingBox in degrees.
		/// </summary>
		public readonly double maxLatitude;

		/// <summary>
		/// The maximum longitude coordinate of this BoundingBox in degrees.
		/// </summary>
		public readonly double maxLongitude;

		/// <summary>
		/// The minimum latitude coordinate of this BoundingBox in degrees.
		/// </summary>
		public readonly double minLatitude;

		/// <summary>
		/// The minimum longitude coordinate of this BoundingBox in degrees.
		/// </summary>
		public readonly double minLongitude;

		/// <param name="minLatitude">
		///            the minimum latitude coordinate in degrees. </param>
		/// <param name="minLongitude">
		///            the minimum longitude coordinate in degrees. </param>
		/// <param name="maxLatitude">
		///            the maximum latitude coordinate in degrees. </param>
		/// <param name="maxLongitude">
		///            the maximum longitude coordinate in degrees. </param>
		/// <exception cref="IllegalArgumentException">
		///             if a coordinate is invalid. </exception>
		public BoundingBox(double minLatitude, double minLongitude, double maxLatitude, double maxLongitude)
		{
			LatLongUtils.ValidateLatitude(minLatitude);
			LatLongUtils.ValidateLongitude(minLongitude);
			LatLongUtils.ValidateLatitude(maxLatitude);
			LatLongUtils.ValidateLongitude(maxLongitude);

			if (minLatitude > maxLatitude)
			{
				throw new System.ArgumentException("invalid latitude range: " + minLatitude + ' ' + maxLatitude);
			}
			else if (minLongitude > maxLongitude)
			{
				throw new System.ArgumentException("invalid longitude range: " + minLongitude + ' ' + maxLongitude);
			}

			this.minLatitude = minLatitude;
			this.minLongitude = minLongitude;
			this.maxLatitude = maxLatitude;
			this.maxLongitude = maxLongitude;
		}

		/// <param name="latLongs">
		///            the coordinates list. </param>
		public BoundingBox(IList<LatLong> latLongs)
		{
			double minLatitude = double.PositiveInfinity;
			double minLongitude = double.PositiveInfinity;
			double maxLatitude = double.NegativeInfinity;
			double maxLongitude = double.NegativeInfinity;
			foreach (LatLong latLong in latLongs)
			{
				double latitude = latLong.latitude;
				double longitude = latLong.longitude;

				minLatitude = Math.Min(minLatitude, latitude);
				minLongitude = Math.Min(minLongitude, longitude);
				maxLatitude = Math.Max(maxLatitude, latitude);
				maxLongitude = Math.Max(maxLongitude, longitude);
			}

			this.minLatitude = minLatitude;
			this.minLongitude = minLongitude;
			this.maxLatitude = maxLatitude;
			this.maxLongitude = maxLongitude;
		}

		/// <param name="latitude">
		///            the latitude coordinate in degrees. </param>
		/// <param name="longitude">
		///            the longitude coordinate in degrees. </param>
		/// <returns> true if this BoundingBox contains the given coordinates, false otherwise. </returns>
		public virtual bool Contains(double latitude, double longitude)
		{
			return this.minLatitude <= latitude && this.maxLatitude >= latitude && this.minLongitude <= longitude && this.maxLongitude >= longitude;
		}

		/// <param name="latLong">
		///            the LatLong whose coordinates should be checked. </param>
		/// <returns> true if this BoundingBox contains the given LatLong, false otherwise. </returns>
		public virtual bool Contains(LatLong latLong)
		{
			return Contains(latLong.latitude, latLong.longitude);
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is BoundingBox))
			{
				return false;
			}
			BoundingBox other = (BoundingBox) obj;
			if (!this.maxLatitude.Equals(other.maxLatitude))
			{
				return false;
			}
			else if (!this.maxLongitude.Equals(other.maxLongitude))
			{
				return false;
			}
			else if (!this.minLatitude.Equals(other.minLatitude))
			{
				return false;
			}
			else if (!this.minLongitude.Equals(other.minLongitude))
			{
				return false;
			}
			return true;
		}

		/// <param name="boundingBox">
		///            the BoundingBox which this BoundingBox should be extended if it is larger </param>
		/// <returns> a BoundingBox that covers this BoundingBox and the given BoundingBox. </returns>
		public virtual BoundingBox ExtendBoundingBox(BoundingBox boundingBox)
		{
			return new BoundingBox(Math.Min(this.minLatitude, boundingBox.minLatitude), Math.Min(this.minLongitude, boundingBox.minLongitude), Math.Max(this.maxLatitude, boundingBox.maxLatitude), Math.Max(this.maxLongitude, boundingBox.maxLongitude));
		}

		/// <summary>
		/// Creates a BoundingBox extended up to coordinates (but does not cross date line/poles). </summary>
		/// <param name="latitude"> up to the extension </param>
		/// <param name="longitude"> up to the extension </param>
		/// <returns> an extended BoundingBox or this (if contains coordinates) </returns>
		public virtual BoundingBox ExtendCoordinates(double latitude, double longitude)
		{
			if (Contains(latitude, longitude))
			{
				return this;
			}

			double minLat = Math.Max(MercatorProjection.LATITUDE_MIN, Math.Min(this.minLatitude, latitude));
			double minLon = Math.Max(-180, Math.Min(this.minLongitude, longitude));
			double maxLat = Math.Min(MercatorProjection.LATITUDE_MAX, Math.Max(this.maxLatitude, latitude));
			double maxLon = Math.Min(180, Math.Max(this.maxLongitude, longitude));

			return new BoundingBox(minLat, minLon, maxLat, maxLon);
		}

		/// <summary>
		/// Creates a BoundingBox extended up to <code>LatLong</code> (but does not cross date line/poles). </summary>
		/// <param name="latLong"> coordinates up to the extension </param>
		/// <returns> an extended BoundingBox or this (if contains coordinates) </returns>
		public virtual BoundingBox ExtendCoordinates(LatLong latLong)
		{
			return ExtendCoordinates(latLong.latitude, latLong.longitude);
		}

		/// <summary>
		/// Creates a BoundingBox that is a fixed degree amount larger on all sides (but does not cross date line/poles). </summary>
		/// <param name="verticalExpansion"> degree extension (must be >= 0) </param>
		/// <param name="horizontalExpansion"> degree extension (must be >= 0) </param>
		/// <returns> an extended BoundingBox or this (if degrees == 0) </returns>
		public virtual BoundingBox ExtendDegrees(double verticalExpansion, double horizontalExpansion)
		{
			if (verticalExpansion == 0 && horizontalExpansion == 0)
			{
				return this;
			}
			else if (verticalExpansion < 0 || horizontalExpansion < 0)
			{
				throw new System.ArgumentException("BoundingBox extend operation does not accept negative values");
			}

			double minLat = Math.Max(MercatorProjection.LATITUDE_MIN, this.minLatitude - verticalExpansion);
			double minLon = Math.Max(-180, this.minLongitude - horizontalExpansion);
			double maxLat = Math.Min(MercatorProjection.LATITUDE_MAX, this.maxLatitude + verticalExpansion);
			double maxLon = Math.Min(180, this.maxLongitude + horizontalExpansion);

			return new BoundingBox(minLat, minLon, maxLat, maxLon);
		}

		/// <summary>
		/// Creates a BoundingBox that is a fixed meter amount larger on all sides (but does not cross date line/poles). </summary>
		/// <param name="meters"> extension (must be >= 0) </param>
		/// <returns> an extended BoundingBox or this (if meters == 0) </returns>
		public virtual BoundingBox ExtendMeters(int meters)
		{
			if (meters == 0)
			{
				return this;
			}
			else if (meters < 0)
			{
				throw new System.ArgumentException("BoundingBox extend operation does not accept negative values");
			}

			double verticalExpansion = LatLongUtils.LatitudeDistance(meters);
			double horizontalExpansion = LatLongUtils.LongitudeDistance(meters, Math.Max(Math.Abs(minLatitude), Math.Abs(maxLatitude)));

			double minLat = Math.Max(MercatorProjection.LATITUDE_MIN, this.minLatitude - verticalExpansion);
			double minLon = Math.Max(-180, this.minLongitude - horizontalExpansion);
			double maxLat = Math.Min(MercatorProjection.LATITUDE_MAX, this.maxLatitude + verticalExpansion);
			double maxLon = Math.Min(180, this.maxLongitude + horizontalExpansion);

			return new BoundingBox(minLat, minLon, maxLat, maxLon);
		}

		/// <returns> a new LatLong at the horizontal and vertical center of this BoundingBox. </returns>
		public virtual LatLong CenterPoint
		{
			get
			{
				double latitudeOffset = (this.maxLatitude - this.minLatitude) / 2;
				double longitudeOffset = (this.maxLongitude - this.minLongitude) / 2;
				return new LatLong(this.minLatitude + latitudeOffset, this.minLongitude + longitudeOffset, true);
			}
		}

		/// <returns> the latitude span of this BoundingBox in degrees. </returns>
		public virtual double LatitudeSpan
		{
			get
			{
				return this.maxLatitude - this.minLatitude;
			}
		}

		/// <returns> the longitude span of this BoundingBox in degrees. </returns>
		public virtual double LongitudeSpan
		{
			get
			{
				return this.maxLongitude - this.minLongitude;
			}
		}

		/// <summary>
		/// Computes the coordinates of this bounding box relative to a tile. </summary>
		/// <param name="tile"> the tile to compute the relative position for. </param>
		/// <returns> rectangle giving the relative position. </returns>
		public virtual Rectangle GetPositionRelativeToTile(Tile tile)
		{
			Point upperLeft = MercatorProjection.GetPixelRelativeToTile(new LatLong(this.maxLatitude, minLongitude), tile);
			Point lowerRight = MercatorProjection.GetPixelRelativeToTile(new LatLong(this.minLatitude, maxLongitude), tile);
			return new Rectangle(upperLeft.x, upperLeft.y, lowerRight.x, lowerRight.y);
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			long temp;
			temp = BitConverter.DoubleToInt64Bits(this.maxLatitude);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.maxLongitude);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.minLatitude);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			temp = BitConverter.DoubleToInt64Bits(this.minLongitude);
			result = prime * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
			return result;
		}

		/// <param name="boundingBox">
		///            the BoundingBox which should be checked for intersection with this BoundingBox. </param>
		/// <returns> true if this BoundingBox intersects with the given BoundingBox, false otherwise. </returns>
		public virtual bool Intersects(BoundingBox boundingBox)
		{
			if (this == boundingBox)
			{
				return true;
			}

			return this.maxLatitude >= boundingBox.minLatitude && this.maxLongitude >= boundingBox.minLongitude && this.minLatitude <= boundingBox.maxLatitude && this.minLongitude <= boundingBox.maxLongitude;
		}

		/// <summary>
		/// Returns if an area built from the latLongs intersects with a bias towards
		/// returning true.
		/// The method returns fast if any of the points lie within the bbox. If none of the points
		/// lie inside the box, it constructs the outer bbox for all the points and tests for intersection
		/// (so it is possible that the area defined by the points does not actually intersect)
		/// </summary>
		/// <param name="latLongs"> the points that define an area </param>
		/// <returns> false if there is no intersection, true if there could be an intersection </returns>
		public virtual bool IntersectsArea(LatLong[][] latLongs)
		{
			if (latLongs.Length == 0 || latLongs[0].Length == 0)
			{
				return false;
			}
			foreach (LatLong[] outer in latLongs)
			{
				foreach (LatLong latLong in outer)
				{
					if (this.Contains(latLong))
					{
						// if any of the points is inside the bbox return early
						return true;
					}
				}
			}

			// no fast solution, so accumulate boundary points
			double tmpMinLat = latLongs[0][0].latitude;
			double tmpMinLon = latLongs[0][0].longitude;
			double tmpMaxLat = latLongs[0][0].latitude;
			double tmpMaxLon = latLongs[0][0].longitude;

			foreach (LatLong[] outer in latLongs)
			{
				foreach (LatLong latLong in outer)
				{
					tmpMinLat = Math.Min(tmpMinLat, latLong.latitude);
					tmpMaxLat = Math.Max(tmpMaxLat, latLong.latitude);
					tmpMinLon = Math.Min(tmpMinLon, latLong.longitude);
					tmpMaxLon = Math.Max(tmpMaxLon, latLong.longitude);
				}
			}
			return this.Intersects(new BoundingBox(tmpMinLat, tmpMinLon, tmpMaxLat, tmpMaxLon));
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("minLatitude=");
			stringBuilder.Append(this.minLatitude);
			stringBuilder.Append(", minLongitude=");
			stringBuilder.Append(this.minLongitude);
			stringBuilder.Append(", maxLatitude=");
			stringBuilder.Append(this.maxLatitude);
			stringBuilder.Append(", maxLongitude=");
			stringBuilder.Append(this.maxLongitude);
			return stringBuilder.ToString();
		}
	}
}