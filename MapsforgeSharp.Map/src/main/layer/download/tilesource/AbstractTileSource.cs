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

    /// <summary>
    /// The abstract base class for tiles downloaded from a web server.
    /// <para>
    /// This class defines a default TTL for cached tiles, accessible through the <seealso cref="#getDefaultTimeToLive()"/>  method. The value
    /// here will be used as the initial TTL by the <seealso cref="org.mapsforge.map.layer.download.TileDownloadLayer"/> using this
    /// tile source, but applications can change the TTL at any time (refer to
    /// <seealso cref="org.mapsforge.map.layer.download.TileDownloadLayer"/> for details). The default value is set to one day, or
    /// 86,400,000 milliseconds. Subclasses should set {@code #defaultTTL} in their constructor to a value that is
    /// appropriate for their tile source.
    /// </para>
    /// </summary>
    public abstract class AbstractTileSource : TileSource
	{
		public abstract bool HasAlpha();
		public abstract sbyte ZoomLevelMin {get;}
		public abstract sbyte ZoomLevelMax {get;}
		public abstract Uri GetTileUrl(org.mapsforge.core.model.Tile tile);
		public abstract int ParallelRequestsLimit {get;}

		/// <summary>
		/// The default time-to-live (TTL) for cached tiles (one day, or 86,400,000 milliseconds).
		/// </summary>
		protected internal long defaultTimeToLive = 86400000;

		protected internal readonly string[] hostNames;
		protected internal readonly int port;
		protected internal readonly Random random = new Random();

		protected internal AbstractTileSource(string[] hostNames, int port)
		{
			if (hostNames == null || hostNames.Length == 0)
			{
				throw new System.ArgumentException("no host names specified");
			}
			if (port < 0 || port > 65535)
			{
				throw new System.ArgumentException("invalid port number: " + port);
			}
			foreach (string hostname in hostNames)
			{
				if (hostname.Length == 0)
				{
					throw new System.ArgumentException("empty host name in host name list");
				}
			}

			this.hostNames = hostNames;
			this.port = port;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is AbstractTileSource))
			{
				return false;
			}
			AbstractTileSource other = (AbstractTileSource) obj;
			if (!this.hostNames.Equals(other.hostNames))
			{
				return false;
			}
			else if (this.port != other.port)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Returns the default time-to-live (TTL) for cached tiles.
		/// </summary>
		public virtual long DefaultTimeToLive
		{
			get
			{
				return defaultTimeToLive;
			}
		}

		protected internal virtual string HostName
		{
			get
			{
				return this.hostNames[random.Next(this.hostNames.Length)];
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + this.hostNames.GetHashCode();
			result = prime * result + this.port;
			return result;
		}
	}
}