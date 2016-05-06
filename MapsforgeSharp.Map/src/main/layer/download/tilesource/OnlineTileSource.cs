/*
 * Copyright 2014 devemux86
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

namespace org.mapsforge.map.layer.download.tilesource
{
    using System;
    using System.Text;

    using Tile = MapsforgeSharp.Core.Model.Tile;

    public class OnlineTileSource : AbstractTileSource
	{
		private bool alpha = false;
		private string baseUrl = "/";
		private string extension = "png";
		private string name;
		private int parallelRequestsLimit = 8;
		private string protocol = "http";
		private int tileSize = 256;
		private sbyte zoomLevelMax = 18;
		private sbyte zoomLevelMin = 0;

		public OnlineTileSource(string[] hostNames, int port) : base(hostNames, port)
		{
		}

		public virtual string BaseUrl
		{
			get
			{
				return baseUrl;
			}
		}

		public virtual string Extension
		{
			get
			{
				return extension;
			}
		}

		public virtual string Name
		{
			get
			{
				return name;
			}
		}

		public override int ParallelRequestsLimit
		{
			get
			{
				return parallelRequestsLimit;
			}
		}

		public virtual string Protocol
		{
			get
			{
				return protocol;
			}
		}

		public virtual int TileSize
		{
			get
			{
				return tileSize;
			}
		}

		public override Uri GetTileUrl(Tile tile)
		{
			StringBuilder stringBuilder = new StringBuilder(32);

			stringBuilder.Append(baseUrl);
			stringBuilder.Append(tile.ZoomLevel);
			stringBuilder.Append('/');
			stringBuilder.Append(tile.TileX);
			stringBuilder.Append('/');
			stringBuilder.Append(tile.TileY);
			stringBuilder.Append('.').Append(extension);

			return new Uri(string.Format("{0}://{1}:{2}{3}", this.protocol, HostName, this.port, stringBuilder.ToString()));
		}

		public override sbyte ZoomLevelMax
		{
			get
			{
				return zoomLevelMax;
			}
		}

		public override sbyte ZoomLevelMin
		{
			get
			{
				return zoomLevelMin;
			}
		}

		public override bool HasAlpha()
		{
			return alpha;
		}

		public virtual OnlineTileSource SetAlpha(bool alpha)
		{
			this.alpha = alpha;
			return this;
		}

		public virtual OnlineTileSource SetBaseUrl(string baseUrl)
		{
			this.baseUrl = baseUrl;
			return this;
		}

		public virtual OnlineTileSource SetExtension(string extension)
		{
			this.extension = extension;
			return this;
		}

		public virtual OnlineTileSource SetName(string name)
		{
			this.name = name;
			return this;
		}

		public virtual OnlineTileSource SetParallelRequestsLimit(int parallelRequestsLimit)
		{
			this.parallelRequestsLimit = parallelRequestsLimit;
			return this;
		}

		public virtual OnlineTileSource SetProtocol(string protocol)
		{
			this.protocol = protocol;
			return this;
		}

		public virtual OnlineTileSource SetTileSize(int tileSize)
		{
			this.tileSize = tileSize;
			return this;
		}

		public virtual OnlineTileSource SetZoomLevelMax(sbyte zoomLevelMax)
		{
			this.zoomLevelMax = zoomLevelMax;
			return this;
		}

		public virtual OnlineTileSource SetZoomLevelMin(sbyte zoomLevelMin)
		{
			this.zoomLevelMin = zoomLevelMin;
			return this;
		}
	}
}