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

namespace org.mapsforge.map.layer.download
{
    using CorruptedInputStreamException = MapsforgeSharp.Core.Graphics.CorruptedInputStreamException;
    using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
    using TileBitmap = MapsforgeSharp.Core.Graphics.TileBitmap;
    using System;
    using System.Net.Http;
    using System.IO.Compression;
    using System.IO;
    internal class TileDownloader : HttpClientHandler
	{
		private readonly DownloadJob downloadJob;
		private readonly GraphicFactory graphicFactory;

		internal TileDownloader(DownloadJob downloadJob, GraphicFactory graphicFactory)
		{
			if (downloadJob == null)
			{
				throw new System.ArgumentException("downloadJob must not be null");
			}
			else if (graphicFactory == null)
			{
				throw new System.ArgumentException("graphicFactory must not be null");
			}

			this.downloadJob = downloadJob;
			this.graphicFactory = graphicFactory;
		}

		internal virtual TileBitmap DownloadImage()
		{
			Uri url = this.downloadJob.tileSource.GetTileUrl(this.downloadJob.tile);

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.Timeout = new TimeSpan(50000);

                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    var response = httpClient.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        Stream stream;
                        if (response.Headers.TransferEncoding.Contains(new System.Net.Http.Headers.TransferCodingHeaderValue("gzip")))
                        {
                            stream = new GZipStream(response.Content.ReadAsStreamAsync().Result, CompressionMode.Decompress);
                        }
                        else
                        {
                            stream = response.Content.ReadAsStreamAsync().Result;
                        }

                        try {
                            TileBitmap result = this.graphicFactory.CreateTileBitmap(stream, this.downloadJob.tile.TileSize, this.downloadJob.hasAlpha);
                            result.Expiration = response.Headers..Content..httpClient.Expiration;
                            return result;
                        }
                        catch (CorruptedInputStreamException)
                        {
                            // the creation of the tile bit map can fail at, at least on Android,
                            // when the connection is slow or busy, returning null here ensures that
                            // the tile will be downloaded again
                            return null;
                        }
                    }
                }
            }

            return null;
		}
	}
}