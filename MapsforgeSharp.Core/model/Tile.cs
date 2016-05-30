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

namespace MapsforgeSharp.Core.Model
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using MapsforgeSharp.Core.Util;

    /// <summary>
    /// A tile represents a rectangular part of the world map. All tiles can be identified by their X and Y number together
    /// with their zoom level. The actual area that a tile covers on a map depends on the underlying map projection.
    /// </summary>
    [DataContract]
	public class Tile
	{
		private const long serialVersionUID = 1L;

		/// <returns> the maximum valid tile number for the given zoom level, 2<sup>zoomLevel</sup> -1. </returns>
		public static int GetMaxTileNumber(sbyte zoomLevel)
		{
			if (zoomLevel < 0)
			{
				throw new System.ArgumentException("zoomLevel must not be negative: " + zoomLevel);
			}
			else if (zoomLevel == 0)
			{
				return 0;
			}
			return (2 << zoomLevel - 1) - 1;
		}

		/// <summary>
		/// the map size implied by zoom level and tileSize, to avoid multiple computations.
		/// </summary>
		public readonly long MapSize;

        [DataMember]
        public readonly int TileSize;

        /// <summary>
        /// The X number of this tile.
        /// </summary>
        [DataMember]
        public readonly int TileX;

        /// <summary>
        /// The Y number of this tile.
        /// </summary>
        [DataMember]
        public readonly int TileY;

        /// <summary>
        /// The zoom level of this tile.
        /// </summary>
        [DataMember]
        public readonly sbyte ZoomLevel;

		private BoundingBox boundingBox;
		private Point origin;

		/// <param name="tileX">
		///            the X number of the tile. </param>
		/// <param name="tileY">
		///            the Y number of the tile. </param>
		/// <param name="zoomLevel">
		///            the zoom level of the tile. </param>
		/// <exception cref="IllegalArgumentException">
		///             if any of the parameters is invalid. </exception>
		public Tile(int tileX, int tileY, sbyte zoomLevel, int tileSize)
		{
			if (tileX < 0)
			{
				throw new System.ArgumentException("tileX must not be negative: " + tileX);
			}
			else if (tileY < 0)
			{
				throw new System.ArgumentException("tileY must not be negative: " + tileY);
			}
			else if (zoomLevel < 0)
			{
				throw new System.ArgumentException("zoomLevel must not be negative: " + zoomLevel);
			}

			long maxTileNumber = GetMaxTileNumber(zoomLevel);
			if (tileX > maxTileNumber)
			{
				throw new System.ArgumentException("invalid tileX number on zoom level " + zoomLevel + ": " + tileX);
			}
			else if (tileY > maxTileNumber)
			{
				throw new System.ArgumentException("invalid tileY number on zoom level " + zoomLevel + ": " + tileY);
			}

			this.TileSize = tileSize;
			this.TileX = tileX;
			this.TileY = tileY;
			this.ZoomLevel = zoomLevel;
			this.MapSize = MercatorProjection.GetMapSize(zoomLevel, tileSize);
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is Tile))
			{
				return false;
			}
			Tile other = (Tile) obj;
			if (this.TileX != other.TileX)
			{
				return false;
			}
			else if (this.TileY != other.TileY)
			{
				return false;
			}
			else if (this.ZoomLevel != other.ZoomLevel)
			{
				return false;
			}
			else if (this.TileSize != other.TileSize)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Gets the geographic extend of this Tile as a BoundingBox. </summary>
		/// <returns> boundaries of this tile. </returns>
		public virtual BoundingBox BoundingBox
		{
			get
			{
				if (this.boundingBox == null)
				{
					double minLatitude = Math.Max(MercatorProjection.LATITUDE_MIN, MercatorProjection.TileYToLatitude(TileY + 1, ZoomLevel));
					double minLongitude = Math.Max(-180, MercatorProjection.TileXToLongitude(this.TileX, ZoomLevel));
					double maxLatitude = Math.Min(MercatorProjection.LATITUDE_MAX, MercatorProjection.TileYToLatitude(this.TileY, ZoomLevel));
					double maxLongitude = Math.Min(180, MercatorProjection.TileXToLongitude(TileX + 1, ZoomLevel));
					if (maxLongitude == -180)
					{
						// fix for dateline crossing, where the right tile starts at -180 and causes an invalid bbox
						maxLongitude = 180;
					}
					this.boundingBox = new BoundingBox(minLatitude, minLongitude, maxLatitude, maxLongitude);
				}
				return this.boundingBox;
			}
		}

		/// <summary>
		/// Returns a set of the eight neighbours of this tile. </summary>
		/// <returns> neighbour tiles as a set </returns>
		public virtual ISet<Tile> Neighbours
		{
			get
			{
				ISet<Tile> neighbours = new HashSet<Tile>();
				neighbours.Add(Left);
				neighbours.Add(AboveLeft);
				neighbours.Add(Above);
				neighbours.Add(AboveRight);
				neighbours.Add(Right);
				neighbours.Add(BelowRight);
				neighbours.Add(Below);
				neighbours.Add(BelowLeft);
				return neighbours;
			}
		}

		/// <summary>
		/// Extend of this tile in absolute coordinates. </summary>
		/// <returns> rectangle with the absolute coordinates. </returns>
		public virtual Rectangle BoundaryAbsolute
		{
			get
			{
				return new Rectangle(Origin.X, Origin.Y, Origin.X + TileSize, Origin.Y + TileSize);
			}
		}

		/// <summary>
		/// Extend of this tile in relative (tile) coordinates. </summary>
		/// <returns> rectangle with the relative coordinates. </returns>
		public virtual Rectangle BoundaryRelative
		{
			get
			{
				return new Rectangle(0, 0, TileSize, TileSize);
			}
		}


		/// <summary>
		/// Returns the top-left point of this tile in absolute coordinates. </summary>
		/// <returns> the top-left point </returns>
		public virtual Point Origin
		{
			get
			{
				if (this.origin == null)
				{
					double x = MercatorProjection.TileToPixel(this.TileX, this.TileSize);
					double y = MercatorProjection.TileToPixel(this.TileY, this.TileSize);
					this.origin = new Point(x, y);
				}
				return this.origin;
			}
		}

		/// <summary>
		/// Returns the tile to the left of this tile. </summary>
		/// <returns> tile to the left. </returns>
		public virtual Tile Left
		{
			get
			{
				int x = TileX - 1;
				if (x < 0)
				{
					x = GetMaxTileNumber(this.ZoomLevel);
				}
				return new Tile(x, this.TileY, this.ZoomLevel, this.TileSize);
			}
		}

		/// <summary>
		/// Returns the tile to the right of this tile. </summary>
		/// <returns> tile to the right </returns>
		public virtual Tile Right
		{
			get
			{
				int x = TileX + 1;
				if (x > GetMaxTileNumber(this.ZoomLevel))
				{
					x = 0;
				}
				return new Tile(x, this.TileY, this.ZoomLevel, this.TileSize);
			}
		}

		/// <summary>
		/// Returns the tile above this tile. </summary>
		/// <returns> tile above </returns>
		public virtual Tile Above
		{
			get
			{
				int y = TileY - 1;
				if (y < 0)
				{
					y = GetMaxTileNumber(this.ZoomLevel);
				}
				return new Tile(this.TileX, y, this.ZoomLevel, this.TileSize);
			}
		}

		/// <summary>
		/// Returns the tile below this tile. </summary>
		/// <returns> tile below </returns>

		public virtual Tile Below
		{
			get
			{
				int y = TileY + 1;
				if (y > GetMaxTileNumber(this.ZoomLevel))
				{
					y = 0;
				}
				return new Tile(this.TileX, y, this.ZoomLevel, this.TileSize);
			}
		}

		/// <summary>
		/// Returns the tile above left </summary>
		/// <returns> tile above left </returns>
		public virtual Tile AboveLeft
		{
			get
			{
				int y = TileY - 1;
				int x = TileX - 1;
				if (y < 0)
				{
					y = GetMaxTileNumber(this.ZoomLevel);
				}
				if (x < 0)
				{
					x = GetMaxTileNumber(this.ZoomLevel);
				}
				return new Tile(x, y, this.ZoomLevel, this.TileSize);
			}
		}

		/// <summary>
		/// Returns the tile above right </summary>
		/// <returns> tile above right </returns>
		public virtual Tile AboveRight
		{
			get
			{
				int y = TileY - 1;
				int x = TileX + 1;
				if (y < 0)
				{
					y = GetMaxTileNumber(this.ZoomLevel);
				}
				if (x > GetMaxTileNumber(this.ZoomLevel))
				{
					x = 0;
				}
				return new Tile(x, y, this.ZoomLevel, this.TileSize);
			}
		}

		/// <summary>
		/// Returns the tile below left </summary>
		/// <returns> tile below left </returns>
		public virtual Tile BelowLeft
		{
			get
			{
				int y = TileY + 1;
				int x = TileX - 1;
				if (y > GetMaxTileNumber(this.ZoomLevel))
				{
					y = 0;
				}
				if (x < 0)
				{
					x = GetMaxTileNumber(this.ZoomLevel);
				}
				return new Tile(x, y, this.ZoomLevel, this.TileSize);
			}
		}

		/// <summary>
		/// Returns the tile below right </summary>
		/// <returns> tile below right </returns>
		public virtual Tile BelowRight
		{
			get
			{
				int y = TileY + 1;
				int x = TileX + 1;
				if (y > GetMaxTileNumber(this.ZoomLevel))
				{
					y = 0;
				}
				if (x > GetMaxTileNumber(this.ZoomLevel))
				{
					x = 0;
				}
				return new Tile(x, y, this.ZoomLevel, this.TileSize);
			}
		}

		/// <returns> the parent tile of this tile or null, if the zoom level of this tile is 0. </returns>
		public virtual Tile Parent
		{
			get
			{
				if (this.ZoomLevel == 0)
				{
					return null;
				}
    
				return new Tile(this.TileX / 2, this.TileY / 2, (sbyte)(this.ZoomLevel - 1), this.TileSize);
			}
		}

		public virtual int GetShiftX(Tile otherTile)
		{
			if (this.Equals(otherTile))
			{
				return 0;
			}

			return this.TileX % 2 + 2 * Parent.GetShiftX(otherTile);
		}

		public virtual int GetShiftY(Tile otherTile)
		{
			if (this.Equals(otherTile))
			{
				return 0;
			}

			return this.TileY % 2 + 2 * Parent.GetShiftY(otherTile);
		}

		public override int GetHashCode()
		{
			int result = 7;
			result = 31 * result + (int)(this.TileX ^ ((int)((uint)this.TileX >> 16)));
			result = 31 * result + (int)(this.TileY ^ ((int)((uint)this.TileY >> 16)));
			result = 31 * result + this.ZoomLevel;
			result = 31 * result + this.TileSize;
			return result;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("x=");
			stringBuilder.Append(this.TileX);
			stringBuilder.Append(", y=");
			stringBuilder.Append(this.TileY);
			stringBuilder.Append(", z=");
			stringBuilder.Append(this.ZoomLevel);
			return stringBuilder.ToString();
		}
	}
}