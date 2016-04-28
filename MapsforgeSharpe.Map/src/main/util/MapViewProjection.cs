/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.map.util
{
    using System;

    using LatLong = org.mapsforge.core.model.LatLong;
	using MapPosition = org.mapsforge.core.model.MapPosition;
	using Point = org.mapsforge.core.model.Point;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using MapView = org.mapsforge.map.view.MapView;

	public class MapViewProjection
	{
		private const string INVALID_MAP_VIEW_DIMENSIONS = "invalid MapView dimensions";

		private readonly MapView mapView;

		public MapViewProjection(MapView mapView)
		{
			this.mapView = mapView;
		}

		/// <summary>
		/// Computes the geographic coordinates of a screen point.
		/// </summary>
		/// <returns> the coordinates of the x/y point </returns>
		public virtual LatLong FromPixels(double x, double y)
		{
			if (this.mapView.Width <= 0 || this.mapView.Height <= 0)
			{
				return null;
			}

			// this uses the framebuffer position, the mapview position can be out of sync with
			// what the user sees on the screen if an animation is in progress
			MapPosition mapPosition = this.mapView.Model.frameBufferModel.MapPosition;

			// this means somehow the mapview is not yet properly set up, see issue #308.
			if (mapPosition == null)
			{
				return null;
			}

			// calculate the pixel coordinates of the top left corner
			LatLong latLong = mapPosition.latLong;
			long mapSize = MercatorProjection.GetMapSize(mapPosition.zoomLevel, this.mapView.Model.displayModel.TileSize);
			double pixelX = MercatorProjection.LongitudeToPixelX(latLong.longitude, mapSize);
			double pixelY = MercatorProjection.LatitudeToPixelY(latLong.latitude, mapSize);
			pixelX -= this.mapView.Width >> 1;
			pixelY -= this.mapView.Height >> 1;

			// catch outer map limits
			try
			{
				// convert the pixel coordinates to a LatLong and return it
				return new LatLong(MercatorProjection.PixelYToLatitude(pixelY + y, mapSize), MercatorProjection.PixelXToLongitude(pixelX + x, mapSize));
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Computes vertical extend of the map view.
		/// </summary>
		/// <returns> the latitude span of the map in degrees </returns>
		public virtual double LatitudeSpan
		{
			get
			{
				if (this.mapView.Width > 0 && this.mapView.Height > 0)
				{
					LatLong top = FromPixels(0, 0);
					LatLong bottom = FromPixels(0, this.mapView.Height);
					return Math.Abs(top.latitude - bottom.latitude);
				}
				throw new System.InvalidOperationException(INVALID_MAP_VIEW_DIMENSIONS);
			}
		}

		/// <summary>
		/// Computes horizontal extend of the map view.
		/// </summary>
		/// <returns> the longitude span of the map in degrees </returns>
		public virtual double LongitudeSpan
		{
			get
			{
				if (this.mapView.Width > 0 && this.mapView.Height > 0)
				{
					LatLong left = FromPixels(0, 0);
					LatLong right = FromPixels(this.mapView.Width, 0);
					return Math.Abs(left.longitude - right.longitude);
				}
				throw new System.InvalidOperationException(INVALID_MAP_VIEW_DIMENSIONS);
			}
		}

		/// <summary>
		/// Converts geographic coordinates to view x/y coordinates in the map view.
		/// </summary>
		/// <param name="in">
		///            the geographic coordinates </param>
		/// <returns> x/y view coordinates for the given location </returns>
		public virtual Point ToPixels(LatLong @in)
		{
			if (@in == null || this.mapView.Width <= 0 || this.mapView.Height <= 0)
			{
				return null;
			}

			MapPosition mapPosition = this.mapView.Model.mapViewPosition.MapPosition;

			// this means somehow the mapview is not yet properly set up, see issue #308.
			if (mapPosition == null)
			{
				return null;
			}

			// calculate the pixel coordinates of the top left corner
			LatLong latLong = mapPosition.latLong;
			long mapSize = MercatorProjection.GetMapSize(mapPosition.zoomLevel, this.mapView.Model.displayModel.TileSize);
			double pixelX = MercatorProjection.LongitudeToPixelX(latLong.longitude, mapSize);
			double pixelY = MercatorProjection.LatitudeToPixelY(latLong.latitude, mapSize);
			pixelX -= this.mapView.Width >> 1;
			pixelY -= this.mapView.Height >> 1;

			// create a new point and return it
			return new Point((int)(MercatorProjection.LongitudeToPixelX(@in.longitude, mapSize) - pixelX), (int)(MercatorProjection.LatitudeToPixelY(@in.latitude, mapSize) - pixelY));
		}
	}
}