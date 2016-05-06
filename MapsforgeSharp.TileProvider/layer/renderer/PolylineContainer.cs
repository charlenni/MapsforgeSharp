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

namespace org.mapsforge.map.layer.renderer
{
    using System;
    using System.Collections.Generic;

    using LatLong = MapsforgeSharp.Core.Model.LatLong;
	using Point = MapsforgeSharp.Core.Model.Point;
	using Tag = MapsforgeSharp.Core.Model.Tag;
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using Way = MapsforgeSharp.Core.Datastore.Way;

	/// <summary>
	/// A PolylineContainer encapsulates the way data retrieved from a map file.
	/// 
	/// The class uses deferred evaluation for computing the absolute and relative
	/// pixel coordinates of the way as many ways will not actually be rendered on a
	/// map. In order to save memory, after evaluation, the internally stored way is
	/// released.
	/// </summary>

	public class PolylineContainer : ShapeContainer
	{
		private Point center;
		private Point[][] coordinatesAbsolute;
		private Point[][] coordinatesRelativeToTile;
		private readonly IList<Tag> tags;
		private readonly sbyte layer;
		private readonly Tile tile;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly bool isClosedWay_Renamed;
		private Way way;

		internal PolylineContainer(Way way, Tile tile)
		{
			this.coordinatesAbsolute = null;
			this.coordinatesRelativeToTile = null;
			this.tags = way.Tags;
			this.tile = tile;
			layer = way.Layer;
			this.way = way;
			this.isClosedWay_Renamed = IsClosedWay(way.LatLongs[0]);
		}

		internal PolylineContainer(Point[] coordinates, Tile tile, IList<Tag> tags)
		{
			this.coordinatesAbsolute = new Point[1][];
			this.coordinatesRelativeToTile = null;
			this.coordinatesAbsolute[0] = new Point[coordinates.Length];
			Array.Copy(coordinates, 0, coordinatesAbsolute[0], 0, coordinates.Length);
			this.tags = tags;
			this.tile = tile;
			this.layer = 0;
			isClosedWay_Renamed = coordinates[0].Equals(coordinates[coordinates.Length - 1]);
		}

		public virtual Point CenterAbsolute
		{
			get
			{
				if (null == center)
				{
					this.center = GeometryUtils.CalculateCenterOfBoundingBox(CoordinatesAbsolute[0]);
				}
				return this.center;
			}
		}

		public virtual Point[][] CoordinatesAbsolute
		{
			get
			{
				// deferred evaluation as some PolyLineContainers will never be drawn. However,
				// to save memory, after computing the absolute coordinates, the way is released.
				if (coordinatesAbsolute == null)
				{
					coordinatesAbsolute = new Point[way.LatLongs.GetLength(0)][];
					for (int i = 0; i < way.LatLongs.Length; ++i)
					{
						coordinatesAbsolute[i] = new Point[way.LatLongs[i].Length];
						for (int j = 0; j < way.LatLongs[i].Length; ++j)
						{
							coordinatesAbsolute[i][j] = MercatorProjection.GetPixelAbsolute(way.LatLongs[i][j], tile.MapSize);
						}
					}
					this.way = null;
				}
				return coordinatesAbsolute;
			}
		}

		public virtual Point[][] CoordinatesRelativeToTile
		{
			get
			{
				if (coordinatesRelativeToTile == null)
				{
					Point tileOrigin = tile.Origin;
					coordinatesRelativeToTile = new Point[CoordinatesAbsolute.Length][];
					for (int i = 0; i < coordinatesRelativeToTile.Length; ++i)
					{
						coordinatesRelativeToTile[i] = new Point[coordinatesAbsolute[i].Length];
						for (int j = 0; j < coordinatesRelativeToTile[i].Length; ++j)
						{
							coordinatesRelativeToTile[i][j] = coordinatesAbsolute[i][j].Offset(-tileOrigin.X, -tileOrigin.Y);
						}
					}
				}
				return coordinatesRelativeToTile;
			}
		}

		public virtual sbyte Layer
		{
			get
			{
				return layer;
			}
		}

		public virtual ShapeType ShapeType
		{
			get
			{
				return ShapeType.POLYLINE;
			}
		}

		public virtual IList<Tag> Tags
		{
			get
			{
				return tags;
			}
		}

		public virtual bool ClosedWay
		{
			get
			{
				return isClosedWay_Renamed;
			}
		}

		public virtual Tile Tile
		{
			get
			{
				return tile;
			}
		}

		private bool IsClosedWay(LatLong[] latLongs)
		{
			return latLongs[0].Distance(latLongs[latLongs.Length - 1]) < 0.000000001;
		}
	}
}