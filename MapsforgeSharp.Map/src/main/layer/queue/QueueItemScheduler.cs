using System;
using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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
namespace org.mapsforge.map.layer.queue
{

	using LatLong = MapsforgeSharp.Core.Model.LatLong;
	using MapPosition = MapsforgeSharp.Core.Model.MapPosition;
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;

	internal sealed class QueueItemScheduler
	{
		internal const double PENALTY_PER_ZOOM_LEVEL = 10;

		internal static void schedule<T>(ICollection<QueueItem<T>> queueItems, MapPosition mapPosition, int tileSize) where T : Job
		{
			foreach (QueueItem<T> queueItem in queueItems)
			{
				queueItem.Priority = calculatePriority(queueItem.@object.tile, mapPosition, tileSize);
			}
		}

		private static double calculatePriority(Tile tile, MapPosition mapPosition, int tileSize)
		{
			double tileLatitude = MercatorProjection.TileYToLatitude(tile.TileY, tile.ZoomLevel);
			double tileLongitude = MercatorProjection.TileXToLongitude(tile.TileX, tile.ZoomLevel);

			int halfTileSize = tileSize / 2;
			long mapSize = MercatorProjection.GetMapSize(mapPosition.ZoomLevel, tileSize);
			double tilePixelX = MercatorProjection.LongitudeToPixelX(tileLongitude, mapSize) + halfTileSize;
			double tilePixelY = MercatorProjection.LatitudeToPixelY(tileLatitude, mapSize) + halfTileSize;

			LatLong latLong = mapPosition.LatLong;
			double mapPixelX = MercatorProjection.LongitudeToPixelX(latLong.Longitude, mapSize);
			double mapPixelY = MercatorProjection.LatitudeToPixelY(latLong.Latitude, mapSize);

			double diffPixel = Math.Sqrt(Math.Pow(tilePixelX - mapPixelX, 2) + Math.Pow(tilePixelY - mapPixelY, 2));
			int diffZoom = Math.Abs(tile.ZoomLevel - mapPosition.ZoomLevel);

			return diffPixel + PENALTY_PER_ZOOM_LEVEL * tileSize * diffZoom;
		}

		private QueueItemScheduler()
		{
			throw new System.InvalidOperationException();
		}
	}

}