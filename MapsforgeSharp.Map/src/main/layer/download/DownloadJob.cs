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

namespace org.mapsforge.map.layer.download
{
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using TileSource = org.mapsforge.map.layer.download.tilesource.TileSource;
	using Job = org.mapsforge.map.layer.queue.Job;

	public class DownloadJob : Job
	{
		public readonly TileSource tileSource;

		public DownloadJob(Tile tile, TileSource tileSource) : base(tile, tileSource.HasAlpha())
		{

			this.tileSource = tileSource;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!base.Equals(obj))
			{
				return false;
			}
			else if (!(obj is DownloadJob))
			{
				return false;
			}
			DownloadJob other = (DownloadJob) obj;
			return this.tileSource.Equals(other.tileSource);
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = base.GetHashCode();
			result = prime * result + this.tileSource.GetHashCode();
			return result;
		}
	}
}