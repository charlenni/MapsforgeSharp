using System;
using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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
namespace org.mapsforge.map.writer.model
{


	/// <summary>
	/// Represents a coordinate in the tile space.
	/// </summary>
	public class TileCoordinate
	{
		private readonly int x;
		private readonly int y;
		private readonly sbyte zoomlevel;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="x">
		///            the x value of the tile on the given zoom level </param>
		/// <param name="y">
		///            the y value of the tile on the given zoom level </param>
		/// <param name="zoomlevel">
		///            the zoom level </param>
		public TileCoordinate(int x, int y, sbyte zoomlevel) : base()
		{
			this.x = x;
			this.y = y;
			this.zoomlevel = zoomlevel;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj == null)
			{
				return false;
			}
			if (this.GetType() != obj.GetType())
			{
				return false;
			}
			TileCoordinate other = (TileCoordinate) obj;
			if (this.x != other.x)
			{
				return false;
			}
			if (this.y != other.y)
			{
				return false;
			}
			if (this.zoomlevel != other.zoomlevel)
			{
				return false;
			}
			return true;
		}

		/// <returns> the x </returns>
		public virtual int X
		{
			get
			{
				return this.x;
			}
		}

		/// <returns> the y </returns>
		public virtual int Y
		{
			get
			{
				return this.y;
			}
		}

		/// <returns> the zoomlevel </returns>
		public virtual sbyte Zoomlevel
		{
			get
			{
				return this.zoomlevel;
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + this.x;
			result = prime * result + this.y;
			result = prime * result + this.zoomlevel;
			return result;
		}

		public override string ToString()
		{
			return "TileCoordinate [x=" + this.x + ", y=" + this.y + ", zoomlevel=" + this.zoomlevel + "]";
		}

		/// <summary>
		/// Computes which tile on a lower zoom level covers this given tile or which tiles on a higher zoom level together
		/// cover this tile.
		/// </summary>
		/// <param name="zoomlevelNew">
		///            the zoom level </param>
		/// <returns> a list of tiles (represented by tile coordinates) which cover this tile </returns>
		public virtual IList<TileCoordinate> translateToZoomLevel(sbyte zoomlevelNew)
		{
			IList<TileCoordinate> tiles = null;
			int zoomlevelDistance = zoomlevelNew - this.zoomlevel;

			int factor = (int) Math.Pow(2, Math.Abs(zoomlevelDistance));
			if (zoomlevelDistance > 0)
			{
				tiles = new List<>((int) Math.Pow(4, Math.Abs(zoomlevelDistance)));
				int tileUpperLeftX = this.x * factor;
				int tileUpperLeftY = this.y * factor;
				for (int i = 0; i < factor; i++)
				{
					for (int j = 0; j < factor; j++)
					{
						tiles.Add(new TileCoordinate(tileUpperLeftX + j, tileUpperLeftY + i, zoomlevelNew));
					}
				}
			}
			else
			{
				tiles = new List<>(1);
				tiles.Add(new TileCoordinate(this.x / factor, this.y / factor, zoomlevelNew));
			}
			return tiles;
		}
	}

}