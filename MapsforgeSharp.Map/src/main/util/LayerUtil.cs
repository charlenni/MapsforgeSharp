/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.map.util
{
    using System.Collections.Generic;
    using System.Linq;

    using MapElementContainer = MapsforgeSharp.Core.Mapelements.MapElementContainer;
	using BoundingBox = MapsforgeSharp.Core.Model.BoundingBox;
	using Point = MapsforgeSharp.Core.Model.Point;
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using TilePosition = org.mapsforge.map.layer.TilePosition;

	public sealed class LayerUtil
	{
		public static IList<TilePosition> GetTilePositions(BoundingBox boundingBox, sbyte zoomLevel, Point topLeftPoint, int tileSize)
		{
			int tileLeft = MercatorProjection.LongitudeToTileX(boundingBox.MinLongitude, zoomLevel);
			int tileTop = MercatorProjection.LatitudeToTileY(boundingBox.MaxLatitude, zoomLevel);
			int tileRight = MercatorProjection.LongitudeToTileX(boundingBox.MaxLongitude, zoomLevel);
			int tileBottom = MercatorProjection.LatitudeToTileY(boundingBox.MinLatitude, zoomLevel);

			int initialCapacity = (tileRight - tileLeft + 1) * (tileBottom - tileTop + 1);
			IList<TilePosition> tilePositions = new List<TilePosition>(initialCapacity);

			for (int tileY = tileTop; tileY <= tileBottom; ++tileY)
			{
				for (int tileX = tileLeft; tileX <= tileRight; ++tileX)
				{
					double pixelX = MercatorProjection.TileToPixel(tileX, tileSize) - topLeftPoint.X;
					double pixelY = MercatorProjection.TileToPixel(tileY, tileSize) - topLeftPoint.Y;

					tilePositions.Add(new TilePosition(new Tile(tileX, tileY, zoomLevel, tileSize), new Point(pixelX, pixelY)));
				}
			}

			return tilePositions;
		}

		public static ISet<Tile> GetTiles(BoundingBox boundingBox, sbyte zoomLevel, int tileSize)
		{
			int tileLeft = MercatorProjection.LongitudeToTileX(boundingBox.MinLongitude, zoomLevel);
			int tileTop = MercatorProjection.LatitudeToTileY(boundingBox.MaxLatitude, zoomLevel);
			int tileRight = MercatorProjection.LongitudeToTileX(boundingBox.MaxLongitude, zoomLevel);
			int tileBottom = MercatorProjection.LatitudeToTileY(boundingBox.MinLatitude, zoomLevel);

			ISet<Tile> tiles = new HashSet<Tile>();

			for (int tileY = tileTop; tileY <= tileBottom; ++tileY)
			{
				for (int tileX = tileLeft; tileX <= tileRight; ++tileX)
				{
					tiles.Add(new Tile(tileX, tileY, zoomLevel, tileSize));
				}
			}
			return tiles;
		}

		/// <summary>
		/// Transforms a list of MapElements, orders it and removes those elements that overlap.
		/// This operation is useful for an early elimination of elements in a list that will never
		/// be drawn because they overlap.
		/// </summary>
		/// <param name="input"> list of MapElements </param>
		/// <returns> collision-free, ordered list, a subset of the input. </returns>

		public static IList<MapElementContainer> CollisionFreeOrdered(IList<MapElementContainer> input)
		{
			// sort items by priority (highest first)
			input.OrderByDescending<MapElementContainer,int>((container) => container.Priority);
			// in order of priority, see if an item can be drawn, i.e. none of the items
			// in the currentItemsToDraw list clashes with it.
			IList<MapElementContainer> output = (IList < MapElementContainer > )new LinkedList<MapElementContainer>();
			foreach (MapElementContainer item in input)
			{
				bool hasSpace = true;
				foreach (MapElementContainer outputElement in output)
				{
					if (outputElement.ClashesWith(item))
					{
						hasSpace = false;
						break;
					}
				}
				if (hasSpace)
				{
					output.Add(item);
				}
			}
			return output;
		}

		private LayerUtil()
		{
			throw new System.InvalidOperationException();
		}
	}
}