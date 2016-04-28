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

    public class OpenCycleMap : AbstractTileSource
	{
		public static readonly OpenCycleMap INSTANCE = new OpenCycleMap(new string[] {"a.tile.opencyclemap.org", "b.tile.opencyclemap.org", "c.tile.opencyclemap.org"}, 80);
		private const int PARALLEL_REQUESTS_LIMIT = 8;
		private const string PROTOCOL = "http";
		private const int ZOOM_LEVEL_MAX = 18;
		private const int ZOOM_LEVEL_MIN = 0;

		public OpenCycleMap(string[] hostNames, int port) : base(hostNames, port)
		{
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

			return new Uri(string.Format("{0}://{1}:{2}{3}", PROTOCOL, HostName, this.port, "/cycle/" + tile.zoomLevel + '/' + tile.tileX + '/' + tile.tileY + ".png"));
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