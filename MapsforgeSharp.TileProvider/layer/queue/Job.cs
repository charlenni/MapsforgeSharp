/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2014 mvglasow <michael -at- vonglasow.com>
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

namespace org.mapsforge.map.layer.queue
{
    using PCLStorage;
    using Tile = org.mapsforge.core.model.Tile;

    public class Job
	{
		public readonly bool hasAlpha;
		public readonly Tile tile;
		private readonly string key;

		private static string ComposeKey(sbyte z, long x, long y)
		{
			return PortablePath.Combine(new string[] { z.ToString(), x.ToString(), y.ToString() });
		}

		public static string ComposeKey(string z, string x, string y)
		{
			return PortablePath.Combine(new string[] { z, x, y });
		}

		public Job(Tile tile, bool hasAlpha)
		{
			if (tile == null)
			{
				throw new System.ArgumentException("tile must not be null");
			}

			this.tile = tile;
			this.hasAlpha = hasAlpha;
			this.key = ComposeKey(this.tile.ZoomLevel, this.tile.TileX, this.tile.TileY);
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is Job))
			{
				return false;
			}
			Job other = (Job) obj;
			return this.hasAlpha == other.hasAlpha && this.tile.Equals(other.tile);
		}

		/// <summary>
		/// Returns a unique identifier for the tile.
		/// <para>
		/// The key has the form {@code zoom/x/y}, which is the de-facto standard for tile references. The default path
		/// separator character of the platform is used between {@code zoom}, {@code x} and {@code y}.
		/// 
		/// @since 0.5.0
		/// </para>
		/// </summary>
		public virtual string Key
		{
			get
			{
				return this.key;
			}
		}

		public override int GetHashCode()
		{
			return this.tile.GetHashCode();
		}
	}
}