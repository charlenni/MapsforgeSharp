/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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

    using MercatorProjection = org.mapsforge.core.util.MercatorProjection;

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
		public readonly long mapSize;

		public readonly int tileSize;

		/// <summary>
		/// The X number of this tile.
		/// </summary>
		public readonly int tileX;

		/// <summary>
		/// The Y number of this tile.
		/// </summary>
		public readonly int tileY;

		/// <summary>
		/// The zoom level of this tile.
		/// </summary>
		public readonly sbyte zoomLevel;

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

			this.tileSize = tileSize;
			this.tileX = tileX;
			this.tileY = tileY;
			this.zoomLevel = zoomLevel;
			this.mapSize = MercatorProjection.GetMapSize(zoomLevel, tileSize);


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
			if (this.tileX != other.tileX)
			{
				return false;
			}
			else if (this.tileY != other.tileY)
			{
				return false;
			}
			else if (this.zoomLevel != other.zoomLevel)
			{
				return false;
			}
			else if (this.tileSize != other.tileSize)
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
					double minLatitude = Math.Max(MercatorProjection.LATITUDE_MIN, MercatorProjection.TileYToLatitude(tileY + 1, zoomLevel));
					double minLongitude = Math.Max(-180, MercatorProjection.TileXToLongitude(this.tileX, zoomLevel));
					double maxLatitude = Math.Min(MercatorProjection.LATITUDE_MAX, MercatorProjection.TileYToLatitude(this.tileY, zoomLevel));
					double maxLongitude = Math.Min(180, MercatorProjection.TileXToLongitude(tileX + 1, zoomLevel));
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
				return new Rectangle(Origin.x, Origin.y, Origin.x + tileSize, Origin.y + tileSize);
			}
		}

		/// <summary>
		/// Extend of this tile in relative (tile) coordinates. </summary>
		/// <returns> rectangle with the relative coordinates. </returns>
		public virtual Rectangle BoundaryRelative
		{
			get
			{
				return new Rectangle(0, 0, tileSize, tileSize);
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
					double x = MercatorProjection.TileToPixel(this.tileX, this.tileSize);
					double y = MercatorProjection.TileToPixel(this.tileY, this.tileSize);
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
				int x = tileX - 1;
				if (x < 0)
				{
					x = GetMaxTileNumber(this.zoomLevel);
				}
				return new Tile(x, this.tileY, this.zoomLevel, this.tileSize);
			}
		}

		/// <summary>
		/// Returns the tile to the right of this tile. </summary>
		/// <returns> tile to the right </returns>
		public virtual Tile Right
		{
			get
			{
				int x = tileX + 1;
				if (x > GetMaxTileNumber(this.zoomLevel))
				{
					x = 0;
				}
				return new Tile(x, this.tileY, this.zoomLevel, this.tileSize);
			}
		}

		/// <summary>
		/// Returns the tile above this tile. </summary>
		/// <returns> tile above </returns>
		public virtual Tile Above
		{
			get
			{
				int y = tileY - 1;
				if (y < 0)
				{
					y = GetMaxTileNumber(this.zoomLevel);
				}
				return new Tile(this.tileX, y, this.zoomLevel, this.tileSize);
			}
		}

		/// <summary>
		/// Returns the tile below this tile. </summary>
		/// <returns> tile below </returns>

		public virtual Tile Below
		{
			get
			{
				int y = tileY + 1;
				if (y > GetMaxTileNumber(this.zoomLevel))
				{
					y = 0;
				}
				return new Tile(this.tileX, y, this.zoomLevel, this.tileSize);
			}
		}

		/// <summary>
		/// Returns the tile above left </summary>
		/// <returns> tile above left </returns>
		public virtual Tile AboveLeft
		{
			get
			{
				int y = tileY - 1;
				int x = tileX - 1;
				if (y < 0)
				{
					y = GetMaxTileNumber(this.zoomLevel);
				}
				if (x < 0)
				{
					x = GetMaxTileNumber(this.zoomLevel);
				}
				return new Tile(x, y, this.zoomLevel, this.tileSize);
			}
		}

		/// <summary>
		/// Returns the tile above right </summary>
		/// <returns> tile above right </returns>
		public virtual Tile AboveRight
		{
			get
			{
				int y = tileY - 1;
				int x = tileX + 1;
				if (y < 0)
				{
					y = GetMaxTileNumber(this.zoomLevel);
				}
				if (x > GetMaxTileNumber(this.zoomLevel))
				{
					x = 0;
				}
				return new Tile(x, y, this.zoomLevel, this.tileSize);
			}
		}

		/// <summary>
		/// Returns the tile below left </summary>
		/// <returns> tile below left </returns>
		public virtual Tile BelowLeft
		{
			get
			{
				int y = tileY + 1;
				int x = tileX - 1;
				if (y > GetMaxTileNumber(this.zoomLevel))
				{
					y = 0;
				}
				if (x < 0)
				{
					x = GetMaxTileNumber(this.zoomLevel);
				}
				return new Tile(x, y, this.zoomLevel, this.tileSize);
			}
		}

		/// <summary>
		/// Returns the tile below right </summary>
		/// <returns> tile below right </returns>
		public virtual Tile BelowRight
		{
			get
			{
				int y = tileY + 1;
				int x = tileX + 1;
				if (y > GetMaxTileNumber(this.zoomLevel))
				{
					y = 0;
				}
				if (x > GetMaxTileNumber(this.zoomLevel))
				{
					x = 0;
				}
				return new Tile(x, y, this.zoomLevel, this.tileSize);
			}
		}

		/// <returns> the parent tile of this tile or null, if the zoom level of this tile is 0. </returns>
		public virtual Tile Parent
		{
			get
			{
				if (this.zoomLevel == 0)
				{
					return null;
				}
    
				return new Tile(this.tileX / 2, this.tileY / 2, (sbyte)(this.zoomLevel - 1), this.tileSize);
			}
		}

		public virtual int GetShiftX(Tile otherTile)
		{
			if (this.Equals(otherTile))
			{
				return 0;
			}

			return this.tileX % 2 + 2 * Parent.GetShiftX(otherTile);
		}

		public virtual int GetShiftY(Tile otherTile)
		{
			if (this.Equals(otherTile))
			{
				return 0;
			}

			return this.tileY % 2 + 2 * Parent.GetShiftY(otherTile);
		}

		public override int GetHashCode()
		{
			int result = 7;
			result = 31 * result + (int)(this.tileX ^ ((int)((uint)this.tileX >> 16)));
			result = 31 * result + (int)(this.tileY ^ ((int)((uint)this.tileY >> 16)));
			result = 31 * result + this.zoomLevel;
			result = 31 * result + this.tileSize;
			return result;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("x=");
			stringBuilder.Append(this.tileX);
			stringBuilder.Append(", y=");
			stringBuilder.Append(this.tileY);
			stringBuilder.Append(", z=");
			stringBuilder.Append(this.zoomLevel);
			return stringBuilder.ToString();
		}
	}
}