/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2015 devemux86
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

namespace org.mapsforge.map.layer
{
    using System;
    using System.Collections.Generic;

    using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
    using ICanvas = MapsforgeSharp.Core.Graphics.ICanvas;
    using IMatrix = MapsforgeSharp.Core.Graphics.IMatrix;
    using ITileBitmap = MapsforgeSharp.Core.Graphics.ITileBitmap;
    using BoundingBox = MapsforgeSharp.Core.Model.BoundingBox;
    using Point = MapsforgeSharp.Core.Model.Point;
    using Tile = MapsforgeSharp.Core.Model.Tile;
    using TileCache = org.mapsforge.map.layer.cache.TileCache;
    using Job = org.mapsforge.map.layer.queue.Job;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using MapViewPosition = org.mapsforge.map.model.MapViewPosition;
    using LayerUtil = org.mapsforge.map.util.LayerUtil;
    using queue;

    public abstract class TileLayer<T> : Layer where T : org.mapsforge.map.layer.queue.Job
	{
		protected internal readonly bool hasJobQueue;
		protected internal readonly bool isTransparent;
		protected internal JobQueue<T> jobQueue;
		protected internal readonly TileCache tileCache;
		private readonly MapViewPosition mapViewPosition;
		private readonly IMatrix matrix;

		public TileLayer(TileCache tileCache, MapViewPosition mapViewPosition, IMatrix matrix, bool isTransparent) : this(tileCache, mapViewPosition, matrix, isTransparent, true)
		{
		}

		public TileLayer(TileCache tileCache, MapViewPosition mapViewPosition, IMatrix matrix, bool isTransparent, bool hasJobQueue) : base()
		{

			if (tileCache == null)
			{
				throw new System.ArgumentException("tileCache must not be null");
			}
			else if (mapViewPosition == null)
			{
				throw new System.ArgumentException("mapViewPosition must not be null");
			}

			this.hasJobQueue = hasJobQueue;
			this.tileCache = tileCache;
			this.mapViewPosition = mapViewPosition;
			this.matrix = matrix;
			this.isTransparent = isTransparent;
		}

		public override void Draw(BoundingBox boundingBox, sbyte zoomLevel, ICanvas canvas, Point topLeftPoint)
		{
			IList<TilePosition> tilePositions = LayerUtil.GetTilePositions(boundingBox, zoomLevel, topLeftPoint, this.displayModel.TileSize);

			// In a rotation situation it is possible that drawParentTileBitmap sets the
			// clipping bounds to portrait, while the device is just being rotated into
			// landscape: the result is a partially painted screen that only goes away
			// after zooming (which has the effect of resetting the clip bounds if drawParentTileBitmap
			// is called again).
			// Always resetting the clip bounds here seems to avoid the problem,
			// I assume that this is a pretty cheap operation, otherwise it would be better
			// to hook this into the onConfigurationChanged call chain.
			canvas.ResetClip();

			if (!isTransparent)
			{
				canvas.FillColor(this.displayModel.BackgroundColor);
			}

			ISet<Job> jobs = new HashSet<Job>();
			foreach (TilePosition tilePosition in tilePositions)
			{
				jobs.Add(CreateJob(tilePosition.Tile));
			}
			this.tileCache.WorkingSet = jobs;

			for (int i = tilePositions.Count - 1; i >= 0; --i)
			{
				TilePosition tilePosition = tilePositions[i];
				Point point = tilePosition.Point;
				Tile tile = tilePosition.Tile;
				T job = CreateJob(tile);
				ITileBitmap bitmap = this.tileCache.GetImmediately(job);

				if (bitmap == null)
				{
					if (this.hasJobQueue && !this.tileCache.ContainsKey(job))
					{
						this.jobQueue.Add(job);
					}
					DrawParentTileBitmap(canvas, point, tile);
				}
				else
				{
					if (IsTileStale(tile, bitmap) && this.hasJobQueue && !this.tileCache.ContainsKey(job))
					{
						this.jobQueue.Add(job);
					}
					RetrieveLabelsOnly(job);
					canvas.DrawBitmap(bitmap, (int) Math.Round(point.X), (int) Math.Round(point.Y));
					bitmap.DecrementRefCount();
				}
			}
			if (this.hasJobQueue)
			{
				this.jobQueue.NotifyWorkers();
			}

		}

		public override DisplayModel DisplayModel
		{
			set
			{
				lock (this)
				{
					base.DisplayModel = value;
					if (value != null && this.hasJobQueue)
					{
						this.jobQueue = new JobQueue<T>(this.mapViewPosition, this.displayModel);
					}
					else
					{
						this.jobQueue = null;
					}
				}
			}
		}

		protected internal abstract T CreateJob(Tile tile);

		/// <summary>
		/// Whether the tile is stale and should be refreshed.
		/// <para>
		/// This method is called from <seealso cref="#draw(BoundingBox, byte, Canvas, Point)"/> to determine whether the tile needs to
		/// be refreshed. Subclasses must override this method and implement appropriate checks to determine when a tile is
		/// stale.
		/// </para>
		/// <para>
		/// Return {@code false} to use the cached copy without attempting to refresh it.
		/// </para>
		/// <para>
		/// Return {@code true} to cause the layer to attempt to obtain a fresh copy of the tile. The layer will first
		/// display the tile referenced by {@code bitmap} and attempt to obtain a fresh copy in the background. When a fresh
		/// copy becomes available, the layer will replace is and update the cache. If a fresh copy cannot be obtained (e.g.
		/// because the tile is obtained from an online source which cannot be reached), the stale tile will continue to be
		/// used until another {@code #draw(BoundingBox, byte, Canvas, Point)} operation requests it again.
		/// 
		/// </para>
		/// </summary>
		/// <param name="tile">
		///            A tile. </param>
		/// <param name="bitmap">
		///            The bitmap for {@code tile} currently held in the layer's cache. </param>
		protected internal abstract bool IsTileStale(Tile tile, ITileBitmap bitmap);

		protected internal virtual void RetrieveLabelsOnly(T job)
		{
		}

		private void DrawParentTileBitmap(ICanvas canvas, Point point, Tile tile)
		{
			Tile cachedParentTile = GetCachedParentTile(tile, 4);
			if (cachedParentTile != null)
			{
				IBitmap bitmap = this.tileCache.GetImmediately(CreateJob(cachedParentTile));
				if (bitmap != null)
				{
					int tileSize = this.displayModel.TileSize;
					long translateX = tile.GetShiftX(cachedParentTile) * tileSize;
					long translateY = tile.GetShiftY(cachedParentTile) * tileSize;
					sbyte zoomLevelDiff = (sbyte)(tile.ZoomLevel - cachedParentTile.ZoomLevel);
					float scaleFactor = (float) Math.Pow(2, zoomLevelDiff);

					int x = (int) Math.Round(point.X);
					int y = (int) Math.Round(point.Y);

					this.matrix.Reset();
					this.matrix.Translate(x - translateX, y - translateY);
					this.matrix.Scale(scaleFactor, scaleFactor);

					canvas.SetClip(x, y, this.displayModel.TileSize, this.displayModel.TileSize);
					canvas.DrawBitmap(bitmap, this.matrix);
					canvas.ResetClip();
					bitmap.DecrementRefCount();
				}
			}
		}

		/// <returns> the first parent object of the given object whose tileCacheBitmap is cached (may be null). </returns>
		private Tile GetCachedParentTile(Tile tile, int level)
		{
			if (level == 0)
			{
				return null;
			}

			Tile parentTile = tile.Parent;
			if (parentTile == null)
			{
				return null;
			}
			else if (this.tileCache.ContainsKey(CreateJob(parentTile)))
			{
				return parentTile;
			}

			return GetCachedParentTile(parentTile, level - 1);
		}

		public virtual TileCache TileCache
		{
			get
			{
				return this.tileCache;
			}
		}
	}
}