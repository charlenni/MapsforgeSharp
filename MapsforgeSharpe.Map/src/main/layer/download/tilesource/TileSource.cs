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

namespace org.mapsforge.map.layer.download.tilesource
{
    using System;

    using Tile = org.mapsforge.core.model.Tile;

    public interface TileSource
	{
		/// <summary>
		/// Returns the default time-to-live (TTL) for cached tiles.
		/// </summary>
		long DefaultTimeToLive {get;}

		/// <returns> the maximum number of parallel requests which this {@code TileSource} supports. </returns>
		int ParallelRequestsLimit {get;}

		/// <returns> the download URL for the given {@code Tile}. </returns>
		Uri GetTileUrl(Tile tile);

		/// <returns> the maximum zoom level which this {@code TileSource} supports. </returns>
		sbyte ZoomLevelMax {get;}

		/// <returns> the minimum zoom level which this {@code TileSource} supports. </returns>
		sbyte ZoomLevelMin {get;}

		/// <returns> the if the {@code TileSource} supports transparent images. </returns>
		bool HasAlpha();
	}
}