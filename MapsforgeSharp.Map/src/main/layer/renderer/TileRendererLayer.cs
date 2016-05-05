/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
 * Copyright 2014 Christian Pesch
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

namespace org.mapsforge.map.layer.renderer
{
    using System.Threading;

    using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
	using TileBitmap = MapsforgeSharp.Core.Graphics.TileBitmap;
	using Tile = org.mapsforge.core.model.Tile;
	using TileCache = org.mapsforge.map.layer.cache.TileCache;
	using LabelStore = org.mapsforge.map.layer.labels.LabelStore;
	using TileBasedLabelStore = org.mapsforge.map.layer.labels.TileBasedLabelStore;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;
	using MapViewPosition = org.mapsforge.map.model.MapViewPosition;
	using Observer = org.mapsforge.map.model.common.Observer;
	using MapDataStore = org.mapsforge.core.datastore.MapDataStore;
	using IXmlRenderTheme = org.mapsforge.map.rendertheme.IXmlRenderTheme;
	using RenderThemeFuture = org.mapsforge.map.rendertheme.rule.RenderThemeFuture;

	public class TileRendererLayer : TileLayer<RendererJob>, Observer
	{
		private readonly DatabaseRenderer databaseRenderer;
		private readonly GraphicFactory graphicFactory;
		private readonly MapDataStore mapDataStore;
		private MapWorkerPool mapWorkerPool;
		private RenderThemeFuture renderThemeFuture;
		private float textScale;
		private readonly TileBasedLabelStore tileBasedLabelStore;
		private IXmlRenderTheme xmlRenderTheme;

		/// <summary>
		/// Creates a TileRendererLayer. </summary>
		/// <param name="tileCache"> cache where tiles are stored </param>
		/// <param name="mapDataStore"> the mapsforge map file </param>
		/// <param name="mapViewPosition"> the mapViewPosition to know which tiles to render </param>
		/// <param name="isTransparent"> true if the tile should have an alpha/transparency </param>
		/// <param name="renderLabels"> true if labels should be rendered onto tiles </param>
		/// <param name="graphicFactory"> the graphicFactory to carry out platform specific operations </param>
		public TileRendererLayer(TileCache tileCache, MapDataStore mapDataStore, MapViewPosition mapViewPosition, bool isTransparent, bool renderLabels, GraphicFactory graphicFactory) : base(tileCache, mapViewPosition, graphicFactory.CreateMatrix(), isTransparent)
		{
			this.graphicFactory = graphicFactory;
			this.mapDataStore = mapDataStore;
			if (renderLabels)
			{
				this.tileBasedLabelStore = null;
				this.databaseRenderer = new DatabaseRenderer(this.mapDataStore, graphicFactory, tileCache);
			}
			else
			{
				this.tileBasedLabelStore = new TileBasedLabelStore(tileCache.CapacityFirstLevel);
				this.databaseRenderer = new DatabaseRenderer(this.mapDataStore, graphicFactory, tileBasedLabelStore);
			}
			this.textScale = 1;
		}

		/// <summary>
		/// If the labels are not rendered onto the tile directly, they are stored in a LabelStore for
		/// rendering on a separate Layer. </summary>
		/// <returns> the LabelStore used for storing labels, null if labels are rendered onto tiles directly. </returns>
		public virtual LabelStore LabelStore
		{
			get
			{
				return tileBasedLabelStore;
			}
		}

		public virtual MapDataStore MapDataStore
		{
			get
			{
				return mapDataStore;
			}
		}

		public virtual float TextScale
		{
			get
			{
				return this.textScale;
			}
			set
			{
				this.textScale = value;
			}
		}

		public override void OnDestroy()
		{
			if (this.renderThemeFuture != null)
			{
				this.renderThemeFuture.DecrementRefCount();
			}
			this.mapDataStore.Close();
			base.OnDestroy();
		}

		public override DisplayModel DisplayModel
		{
			set
			{
				lock (this)
				{
					base.DisplayModel = value;
					if (value != null)
					{
						CompileRenderTheme();
						this.mapWorkerPool = new MapWorkerPool(this.tileCache, this.jobQueue, this.databaseRenderer, this);
						this.mapWorkerPool.start();
					}
					else
					{
						// if we do not have a value any more we can stop rendering.
						if (this.mapWorkerPool != null)
						{
							this.mapWorkerPool.stop();
						}
					}
				}
			}
		}

		public virtual IXmlRenderTheme XmlRenderTheme
		{
			set
			{
				this.xmlRenderTheme = value;
				CompileRenderTheme();
			}
		}

		protected internal virtual void CompileRenderTheme()
		{
			this.renderThemeFuture = new RenderThemeFuture(this.graphicFactory, this.xmlRenderTheme, this.displayModel);
		}

		protected internal override RendererJob CreateJob(Tile tile)
		{
			return new RendererJob(tile, this.mapDataStore, this.renderThemeFuture, this.displayModel, this.textScale, this.isTransparent, false);
		}

		/// <summary>
		/// Whether the tile is stale and should be refreshed.
		/// <para>
		/// This method is called from <seealso cref="#draw(org.mapsforge.core.model.BoundingBox, byte, MapsforgeSharp.Core.Graphics.Canvas, org.mapsforge.core.model.Point)"/> to determine whether the tile needs to
		/// be refreshed.
		/// </para>
		/// <para>
		/// A tile is considered stale if the timestamp of the layer's <seealso cref="#mapDataStore"/> is more recent than the
		/// {@code bitmap}'s <seealso cref="MapsforgeSharp.Core.Graphics.TileBitmap#getTimestamp()"/>.
		/// </para>
		/// <para>
		/// When a tile has become stale, the layer will first display the tile referenced by {@code bitmap} and attempt to
		/// obtain a fresh copy in the background. When a fresh copy becomes available, the layer will replace is and update
		/// the cache. If a fresh copy cannot be obtained for whatever reason, the stale tile will continue to be used until
		/// another {@code #draw(BoundingBox, byte, Canvas, Point)} operation requests it again.
		/// 
		/// </para>
		/// </summary>
		/// <param name="tile">
		///            A tile. </param>
		/// <param name="bitmap">
		///            The bitmap for {@code tile} currently held in the layer's cache. </param>
		protected internal override bool IsTileStale(Tile tile, TileBitmap bitmap)
		{
			return this.mapDataStore.GetDataTimestamp(tile) > bitmap.Timestamp;
		}

		protected internal override void OnAdd()
		{
			this.mapWorkerPool.start();
			if (tileCache != null)
			{
				tileCache.AddObserver(this);
			}

			base.OnAdd();
		}

		protected internal override void OnRemove()
		{
			this.mapWorkerPool.stop();
			if (tileCache != null)
			{
				tileCache.RemoveObserver(this);
			}
			base.OnRemove();
		}

		protected internal override void RetrieveLabelsOnly(RendererJob job)
		{
			if (this.hasJobQueue && this.tileBasedLabelStore != null && this.tileBasedLabelStore.requiresTile(job.tile))
			{
				job.RetrieveLabelsOnly = true;
				this.jobQueue.Add(job);
			}
		}

		public void OnChange()
		{
			this.RequestRedraw();
		}
	}

}