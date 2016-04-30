/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2014 devemux86
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

    /// <summary>
    /// A tile source which fetches standard Mapnik tiles from OpenStreetMap.
    /// <para>
    /// Layers using this tile source will enforce a time-to-live (TTL) of 8,279,000 milliseconds for cached tiles (unless
    /// the application explicitly sets a different TTL for that layer). The default TTL corresponds to the lifetime which
    /// the OSM server sets on a newly rendered tile.
    /// </para>
    /// <para>
    /// Refer to <seealso cref="org.mapsforge.map.layer.download.TileDownloadLayer"/> for details on the TTL mechanism.
    /// </para>
    /// </summary>
    public class OpenStreetMapMapnik : AbstractTileSource
	{
		public static readonly OpenStreetMapMapnik INSTANCE = new OpenStreetMapMapnik(new string[] {"a.tile.openstreetmap.org", "b.tile.openstreetmap.org", "c.tile.openstreetmap.org"}, 80);
		private const int PARALLEL_REQUESTS_LIMIT = 8;
		private const string PROTOCOL = "http";
		private const int ZOOM_LEVEL_MAX = 18;
		private const int ZOOM_LEVEL_MIN = 0;

		public OpenStreetMapMapnik(string[] hostNames, int port) : base(hostNames, port)
		{
			/* Default TTL: 8279 seconds (the TTL currently set by the OSM server). */
			defaultTimeToLive = 8279000;
		}

		public override int ParallelRequestsLimit
		{
			get
			{
				return PARALLEL_REQUESTS_LIMIT;
			}
		}

		public override Uri GetTileUrl(Tile tile)
		{

			return new Uri(string.Format("{0}://{1}:{2}{3}", PROTOCOL, HostName, this.port, "/" + tile.ZoomLevel + '/' + tile.TileX + '/' + tile.TileY + ".png"));
		}

		public override sbyte ZoomLevelMax
		{
			get
			{
				return ZOOM_LEVEL_MAX;
			}
		}

		public override sbyte ZoomLevelMin
		{
			get
			{
				return ZOOM_LEVEL_MIN;
			}
		}

		public override bool HasAlpha()
		{
			return false;
		}
	}
}