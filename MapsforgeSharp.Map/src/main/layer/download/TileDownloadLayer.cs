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
    using System;

    using ICanvas = MapsforgeSharp.Core.Graphics.ICanvas;
	using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
	using ITileBitmap = MapsforgeSharp.Core.Graphics.ITileBitmap;
	using BoundingBox = MapsforgeSharp.Core.Model.BoundingBox;
	using Point = MapsforgeSharp.Core.Model.Point;
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using TileCache = org.mapsforge.map.layer.cache.TileCache;
	using TileSource = org.mapsforge.map.layer.download.tilesource.TileSource;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;
	using MapViewPosition = org.mapsforge.map.model.MapViewPosition;
	using Observer = org.mapsforge.map.model.common.Observer;

	public class TileDownloadLayer : TileLayer<DownloadJob>, Observer
	{
		private const int DOWNLOAD_THREADS_MAX = 8;

		private long cacheTimeToLive = 0;
		private readonly IGraphicFactory graphicFactory;
		private bool started;
		private readonly TileCache tileCache;
		private TileDownloadThread[] tileDownloadThreads;
		private readonly TileSource tileSource;

		public TileDownloadLayer(TileCache tileCache, MapViewPosition mapViewPosition, TileSource tileSource, IGraphicFactory graphicFactory) : base(tileCache, mapViewPosition, graphicFactory.CreateMatrix(), tileSource.HasAlpha())
		{

			this.tileCache = tileCache;
			this.tileSource = tileSource;
			this.cacheTimeToLive = tileSource.DefaultTimeToLive;
			this.graphicFactory = graphicFactory;
		}

		public override void Draw(BoundingBox boundingBox, sbyte zoomLevel, ICanvas canvas, Point topLeftPoint)
		{
			if (zoomLevel < this.tileSource.ZoomLevelMin || zoomLevel > this.tileSource.ZoomLevelMax)
			{
				return;
			}

			base.Draw(boundingBox, zoomLevel, canvas, topLeftPoint);
		}

		/// <summary>
		/// Returns the time-to-live (TTL) for tiles in the cache, or 0 if not set.
		/// <para>
		/// Refer to <seealso cref="#isTileStale(Tile, TileBitmap)"/> for information on how the TTL is enforced.
		/// </para>
		/// </summary>
		public virtual long CacheTimeToLive
		{
			get
			{
				return cacheTimeToLive;
			}
			set
			{
				cacheTimeToLive = value;
			}
		}

		public override void OnDestroy()
		{
			foreach (TileDownloadThread tileDownloadThread in this.tileDownloadThreads)
			{
				tileDownloadThread.Interrupt();
			}

			base.OnDestroy();
		}

		public virtual void OnPause()
		{
			foreach (TileDownloadThread tileDownloadThread in this.tileDownloadThreads)
			{
				tileDownloadThread.Pause();
			}
		}

		public virtual void OnResume()
		{
			if (!started)
			{
				Start();
			}
			foreach (TileDownloadThread tileDownloadThread in this.tileDownloadThreads)
			{
				tileDownloadThread.Proceed();
			}
		}

		public override DisplayModel DisplayModel
		{
			set
			{
				lock (this)
				{
					base.DisplayModel = value;
					int numberOfDownloadThreads = Math.Min(tileSource.ParallelRequestsLimit, DOWNLOAD_THREADS_MAX);
					if (this.displayModel != null)
					{
						this.tileDownloadThreads = new TileDownloadThread[numberOfDownloadThreads];
						for (int i = 0; i < numberOfDownloadThreads; ++i)
						{
							this.tileDownloadThreads[i] = new TileDownloadThread(this.tileCache, this.jobQueue, this, this.graphicFactory, this.displayModel);
						}
					}
					else
					{
						if (this.tileDownloadThreads != null)
						{
							foreach (TileDownloadThread tileDownloadThread in tileDownloadThreads)
							{
								tileDownloadThread.Interrupt();
							}
						}
					}
            
				}
			}
		}

		public virtual void Start()
		{
			foreach (TileDownloadThread tileDownloadThread in this.tileDownloadThreads)
			{
				tileDownloadThread.Start();
			}
			started = true;
		}

		protected internal override DownloadJob CreateJob(Tile tile)
		{
			return new DownloadJob(tile, this.tileSource);
		}

		/// <summary>
		/// Whether the tile is stale and should be refreshed.
		/// <para>
		/// This method is called from <seealso cref="#draw(BoundingBox, byte, Canvas, Point)"/> to determine whether the tile needs to
		/// be refreshed.
		/// </para>
		/// <para>
		/// A tile is considered stale if one or more of the following two conditions apply:
		/// <ul>
		/// <li>The {@code bitmap}'s <seealso cref="MapsforgeSharp.Core.Graphics.ITileBitmap#isExpired()"/> method returns {@code True}.</li>
		/// <li>The layer has a time-to-live (TTL) set (<seealso cref="#getCacheTimeToLive()"/> returns a nonzero value) and the sum of
		/// the {@code bitmap}'s <seealso cref="MapsforgeSharp.Core.Graphics.ITileBitmap#getTimestamp()"/> and TTL is less than current
		/// time (as returned by <seealso cref="java.lang.System#currentTimeMillis()"/>).</li>
		/// </ul>
		/// </para>
		/// <para>
		/// When a tile has become stale, the layer will first display the tile referenced by {@code bitmap} and attempt to
		/// obtain a fresh copy in the background. When a fresh copy becomes available, the layer will replace it and update
		/// the cache. If a fresh copy cannot be obtained (e.g. because the tile is obtained from an online source which
		/// cannot be reached), the stale tile will continue to be used until another
		/// {@code #draw(BoundingBox, byte, Canvas, Point)} operation requests it again.
		/// 
		/// </para>
		/// </summary>
		/// <param name="tile">
		///            A tile. This parameter is not used for a {@code TileDownloadLayer} and can be null. </param>
		/// <param name="bitmap">
		///            The bitmap for {@code tile} currently held in the layer's cache. </param>
		protected internal override bool IsTileStale(Tile tile, ITileBitmap bitmap)
		{
			if (bitmap.Expired)
			{
				return true;
			}
			return cacheTimeToLive != 0 && ((bitmap.Timestamp + cacheTimeToLive) < DateTimeHelperClass.CurrentUnixTimeMillis());
		}

		protected internal override void OnAdd()
		{
			if (tileCache != null)
			{
				tileCache.AddObserver(this);
			}

			base.OnAdd();
		}

		protected internal override void OnRemove()
		{
			if (tileCache != null)
			{
				tileCache.RemoveObserver(this);
			}
			base.OnRemove();
		}

		public void OnChange()
		{
			this.RequestRedraw();
		}
	}
}