/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
 * Copyright 2014, 2015 devemux86
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

namespace org.mapsforge.map.layer.renderer
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Acrotech.PortableLogAdapter;
	using MapsforgeSharp.Core.Graphics;

	using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
	using Color = MapsforgeSharp.Core.Graphics.Color;
	using Display = MapsforgeSharp.Core.Graphics.Display;
	using MapElementContainer = MapsforgeSharp.Core.Mapelements.MapElementContainer;
	using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
	using Paint = MapsforgeSharp.Core.Graphics.Paint;
	using SymbolContainer = MapsforgeSharp.Core.Mapelements.SymbolContainer;
	using TileBitmap = MapsforgeSharp.Core.Graphics.TileBitmap;
	using LatLong = MapsforgeSharp.Core.Model.LatLong;
	using Point = MapsforgeSharp.Core.Model.Point;
	using Rectangle = MapsforgeSharp.Core.Model.Rectangle;
	using Tag = MapsforgeSharp.Core.Model.Tag;
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using MercatorProjection = MapsforgeSharp.Core.Util.MercatorProjection;
	using TileCache = org.mapsforge.map.layer.cache.TileCache;
	using TileBasedLabelStore = org.mapsforge.map.layer.labels.TileBasedLabelStore;
	using MapDataStore = MapsforgeSharp.Core.Datastore.MapDataStore;
	using MapReadResult = MapsforgeSharp.Core.Datastore.MapReadResult;
	using PointOfInterest = MapsforgeSharp.Core.Datastore.PointOfInterest;
	using Way = MapsforgeSharp.Core.Datastore.Way;
	using RenderCallback = org.mapsforge.map.rendertheme.RenderCallback;
	using RenderContext = org.mapsforge.map.rendertheme.RenderContext;
	using RenderTheme = org.mapsforge.map.rendertheme.rule.RenderTheme;
	using LayerUtil = org.mapsforge.map.util.LayerUtil;
	using SkiaSharp;
	using MapsforgeSharp.TileProvider.Graphics; /// <summary>
												/// The DatabaseRenderer renders map tiles by reading from a <seealso cref="org.mapsforge.map.datastore.MapDataStore"/>.
												/// </summary>
	public class DatabaseRenderer : RenderCallback
	{
		private static readonly sbyte? DEFAULT_START_ZOOM_LEVEL = (sbyte) 12;
        private static readonly ILogger LOGGER = (new Acrotech.PortableLogAdapter.Managers.DelegateLogManager((logger, message) => System.Diagnostics.Debug.WriteLine("[{0}]{1}", logger.Name, message), LogLevel.Info)).GetLogger(nameof(DatabaseRenderer));
        private static readonly Tag TAG_NATURAL_WATER = new Tag("natural", "water");
		private const sbyte ZOOM_MAX = 22;

		private static Point[] GetTilePixelCoordinates(int tileSize)
		{
			Point[] result = new Point[5];
			result[0] = new Point(0, 0);
			result[1] = new Point(tileSize, 0);
			result[2] = new Point(tileSize, tileSize);
			result[3] = new Point(0, tileSize);
			result[4] = result[0];
			return result;
		}

		private readonly GraphicFactory graphicFactory;
		private readonly TileBasedLabelStore labelStore;
		private readonly MapDataStore mapDatabase;
		private readonly bool renderLabels;
		private readonly TileCache tileCache;
		private readonly TileDependencies tileDependencies;

		/// <summary>
		/// Constructs a new DatabaseRenderer that will not draw labels, instead it stores the label
		/// information in the labelStore for drawing by a LabelLayer.
		/// </summary>
		/// <param name="mapDatabase">
		///            the MapDatabase from which the map data will be read. </param>
		public DatabaseRenderer(MapDataStore mapDatabase, GraphicFactory graphicFactory, TileBasedLabelStore labelStore)
		{
			this.mapDatabase = mapDatabase;
			this.graphicFactory = graphicFactory;
			this.labelStore = labelStore;
			this.renderLabels = false;
			this.tileCache = null;
			this.tileDependencies = null;
		}

		/// <summary>
		/// Constructs a new DatabaseRenderer that will draw labels onto the tiles.
		/// </summary>
		/// <param name="mapFile">
		///            the MapDatabase from which the map data will be read. </param>
		public DatabaseRenderer(MapDataStore mapFile, GraphicFactory graphicFactory, TileCache tileCache)
		{
			this.mapDatabase = mapFile;
			this.graphicFactory = graphicFactory;

			this.labelStore = null;
			this.renderLabels = true;
			this.tileCache = tileCache;
			this.tileDependencies = new TileDependencies();
		}

        /// <summary>
        /// Called when a job needs to be executed.
        /// </summary>
        /// <param name="rendererJob">
        ///            the job that should be executed. </param>
        public virtual SKImage ExecuteJob(RendererJob rendererJob)
		{
			RenderTheme renderTheme;

			try
			{
                // Wait until RenderTheme is ready
				renderTheme = rendererJob.renderThemeFuture.Result;
			}
			catch (Exception e)
			{
				LOGGER.Fatal("Error to retrieve render theme from future", e);
				return null;
			}

			RenderContext renderContext = null;

			try
			{
				renderContext = new RenderContext(renderTheme, rendererJob, new CanvasRasterer(graphicFactory));

				renderContext.canvasRasterer.Canvas = graphicFactory.CreateCanvas(renderContext.rendererJob.tile.TileSize, renderContext.rendererJob.tile.TileSize);

				if (RenderBitmap(renderContext))
				{
					if (this.mapDatabase != null)
					{
						MapReadResult mapReadResult = this.mapDatabase.ReadMapData(rendererJob.tile);
						ProcessReadMapData(renderContext, mapReadResult);
					}

					if (!rendererJob.labelsOnly)
					{
						//bitmap = this.graphicFactory.CreateTileBitmap(renderContext.rendererJob.tile.TileSize, renderContext.rendererJob.hasAlpha);
						//bitmap.Timestamp = rendererJob.mapDataStore.GetDataTimestamp(renderContext.rendererJob.tile);
						//renderContext.canvasRasterer.CanvasBitmap = bitmap;
						if (!rendererJob.hasAlpha && rendererJob.displayModel.BackgroundColor != renderContext.renderTheme.MapBackground)
						{
							renderContext.canvasRasterer.Fill(renderContext.renderTheme.MapBackground);
						}
						renderContext.canvasRasterer.DrawWays(renderContext);
					}

					if (renderLabels)
					{
						ISet<MapElementContainer> labelsToDraw = ProcessLabels(renderContext);
						// now draw the ways and the labels
						renderContext.canvasRasterer.DrawMapElements(labelsToDraw, renderContext.rendererJob.tile);
					}
					else
					{
						// store elements for this tile in the label cache
						this.labelStore.StoreMapItems(renderContext.rendererJob.tile, renderContext.labels);
					}

					//if (!rendererJob.labelsOnly && renderContext.renderTheme.HasMapBackgroundOutside())
					//{
					//	// blank out all areas outside of map
					//	Rectangle insideArea = this.mapDatabase.BoundingBox.GetPositionRelativeToTile(renderContext.rendererJob.tile);
					//	if (!rendererJob.hasAlpha)
					//	{
					//		//renderContext.canvasRasterer.FillOutsideAreas(renderContext.renderTheme.MapBackgroundOutside, insideArea);
					//	}
					//	else
					//	{
					//		//renderContext.canvasRasterer.FillOutsideAreas(Color.TRANSPARENT, insideArea);
					//	}
					//}

					//renderContext.canvasRasterer.Fill(0x7f808080);
					//var path = new SKPath();
					//path.MoveTo(45, 30);
					//path.LineTo(230, 180);
					//var paint = new SKPaint();
					//paint.IsAntialias = true;
					//paint.IsStroke = true;
					//paint.Color = SKColors.GreenYellow;
					//((SkiaCanvas)renderContext.canvasRasterer.Canvas).NativeCanvas.DrawPath(path, paint);

					return ((SkiaCanvas)renderContext.canvasRasterer.Canvas).Image;
				}

				// Draws a bitmap just with outside colour, used for bitmaps outside of map area.
				if (!renderContext.rendererJob.hasAlpha)
				{
					//renderContext.canvasRasterer.Fill(renderContext.renderTheme.MapBackgroundOutside);
				}

				return ((SkiaCanvas)renderContext.canvasRasterer.Canvas).Image;
			}
			finally
			{
				if (renderContext != null)
				{
					renderContext.Destroy();
				}
			}
		}

		public virtual MapDataStore MapDatabase
		{
			get
			{
				return this.mapDatabase;
			}
		}

		/// <returns> the start point (may be null). </returns>
		public virtual LatLong StartPosition
		{
			get
			{
				if (this.mapDatabase != null)
				{
					return this.mapDatabase.StartPosition;
				}
				return null;
			}
		}

		/// <returns> the start zoom level (may be null). </returns>
		public virtual sbyte? StartZoomLevel
		{
			get
			{
				if (this.mapDatabase != null && null != this.mapDatabase.StartZoomLevel)
				{
					return this.mapDatabase.StartZoomLevel;
				}
				return DEFAULT_START_ZOOM_LEVEL;
			}
		}

		/// <returns> the maximum zoom level. </returns>
		public virtual sbyte ZoomLevelMax
		{
			get
			{
				return ZOOM_MAX;
			}
		}

		internal virtual void RemoveTileInProgress(Tile tile)
		{
			if (this.tileDependencies != null)
			{
				this.tileDependencies.RemoveTileInProgress(tile);
			}
		}

		public void RenderArea(RenderContext renderContext, Paint fill, Paint stroke, int level, PolylineContainer way)
		{
			renderContext.AddToCurrentDrawingLayer(level, new ShapePaintContainer(way, stroke));
			renderContext.AddToCurrentDrawingLayer(level, new ShapePaintContainer(way, fill));
		}

		public void RenderAreaCaption(RenderContext renderContext, Display display, int priority, string caption, float horizontalOffset, float verticalOffset, Paint fill, Paint stroke, Position position, int maxTextWidth, PolylineContainer way)
		{
			Point centerPoint = way.CenterAbsolute.Offset(horizontalOffset, verticalOffset);
			renderContext.labels.Add(this.graphicFactory.CreatePointTextContainer(centerPoint, display, priority, caption, fill, stroke, null, position, maxTextWidth));
		}

		public void RenderAreaSymbol(RenderContext renderContext, Display display, int priority, IBitmap symbol, PolylineContainer way)
		{
			Point centerPosition = way.CenterAbsolute;

			renderContext.labels.Add(new SymbolContainer(centerPosition, display, priority, symbol));
		}

		public void RenderPointOfInterestCaption(RenderContext renderContext, Display display, int priority, string caption, float horizontalOffset, float verticalOffset, Paint fill, Paint stroke, Position position, int maxTextWidth, PointOfInterest poi)
		{
			Point poiPosition = MercatorProjection.GetPixelAbsolute(poi.Position, renderContext.rendererJob.tile.MapSize);

			renderContext.labels.Add(this.graphicFactory.CreatePointTextContainer(poiPosition.Offset(horizontalOffset, verticalOffset), display, priority, caption, fill, stroke, null, position, maxTextWidth));
		}

		public void RenderPointOfInterestCircle(RenderContext renderContext, float radius, Paint fill, Paint stroke, int level, PointOfInterest poi)
		{
			Point poiPosition = MercatorProjection.GetPixelRelativeToTile(poi.Position, renderContext.rendererJob.tile);
			renderContext.AddToCurrentDrawingLayer(level, new ShapePaintContainer(new CircleContainer(poiPosition, radius), stroke));
			renderContext.AddToCurrentDrawingLayer(level, new ShapePaintContainer(new CircleContainer(poiPosition, radius), fill));
		}

		public void RenderPointOfInterestSymbol(RenderContext renderContext, Display display, int priority, IBitmap symbol, PointOfInterest poi)
		{
			Point poiPosition = MercatorProjection.GetPixelAbsolute(poi.Position, renderContext.rendererJob.tile.MapSize);
			renderContext.labels.Add(new SymbolContainer(poiPosition, display, priority, symbol));
		}

		public void RenderWay(RenderContext renderContext, Paint stroke, float dy, int level, PolylineContainer way)
		{
			renderContext.AddToCurrentDrawingLayer(level, new ShapePaintContainer(way, stroke, dy));
		}

		public void RenderWaySymbol(RenderContext renderContext, Display display, int priority, IBitmap symbol, float dy, bool alignCenter, bool repeat, float repeatGap, float repeatStart, bool rotate, PolylineContainer way)
		{
			WayDecorator.RenderSymbol(symbol, display, priority, dy, alignCenter, repeat, repeatGap, repeatStart, rotate, way.CoordinatesAbsolute, renderContext.labels);
		}

		public void RenderWayText(RenderContext renderContext, Display display, int priority, string textKey, float dy, Paint fill, Paint stroke, PolylineContainer way)
		{
			WayDecorator.RenderText(way.Tile, textKey, display, priority, dy, fill, stroke, way.CoordinatesAbsolute, renderContext.labels);
		}

		internal virtual bool RenderBitmap(RenderContext renderContext)
		{
			return !renderContext.renderTheme.HasMapBackgroundOutside() || this.mapDatabase.SupportsTile(renderContext.rendererJob.tile);
		}

		private ISet<MapElementContainer> ProcessLabels(RenderContext renderContext)
		{
			// if we are drawing the labels per tile, we need to establish which tile-overlapping
			// elements need to be drawn.

			ISet<MapElementContainer> labelsToDraw = new HashSet<MapElementContainer>();

			lock (tileDependencies)
			{
				// first we need to get the labels from the adjacent tiles if they have already been drawn
				// as those overlapping items must also be drawn on the current tile. They must be drawn regardless
				// of priority clashes as a part of them has alread been drawn.
				ISet<Tile> neighbours = renderContext.rendererJob.tile.Neighbours;
				ISet<MapElementContainer> undrawableElements = new HashSet<MapElementContainer>();

				tileDependencies.AddTileInProgress(renderContext.rendererJob.tile);
				for (var i = neighbours.Count-1; i >= 0; i--)
				{
					Tile neighbour = neighbours.ElementAt(i);

					if (tileDependencies.IsTileInProgress(neighbour) || tileCache.ContainsKey(renderContext.rendererJob.OtherTile(neighbour)))
					{
						// if a tile has already been drawn, the elements drawn that overlap onto the
						// current tile should be in the tile dependencies, we add them to the labels that
						// need to be drawn onto this tile. For the multi-threaded renderer we also need to take
						// those tiles into account that are not yet in the TileCache: this is taken care of by the
						// set of tilesInProgress inside the TileDependencies.
						labelsToDraw.UnionWith(tileDependencies.GetOverlappingElements(neighbour, renderContext.rendererJob.tile));

						// but we need to remove the labels for this tile that overlap onto a tile that has been drawn
						foreach (MapElementContainer current in renderContext.labels)
						{
							if (current.Intersects(neighbour.BoundaryAbsolute))
							{
								undrawableElements.Add(current);
							}
						}
						// since we already have the data from that tile, we do not need to get the data for
						// it, so remove it from the neighbours list.
						neighbours.Remove(neighbour);
					}
					else
					{
						tileDependencies.RemoveTileData(neighbour);
					}
				}

                // now we remove the elements that overlap onto a drawn tile from the list of labels
                // for this tile
                foreach (var element in undrawableElements)
                {
                    renderContext.labels.Remove(element);
                }

				// at this point we have two lists: one is the list of labels that must be drawn because
				// they already overlap from other tiles. The second one is currentLabels that contains
				// the elements on this tile that do not overlap onto a drawn tile. Now we sort this list and
				// remove those elements that clash in this list already.
				ICollection<MapElementContainer> currentElementsOrdered = LayerUtil.CollisionFreeOrdered(renderContext.labels);

				// now we go through this list, ordered by priority, to see which can be drawn without clashing.
				IEnumerator<MapElementContainer> currentMapElementsIterator = currentElementsOrdered.GetEnumerator();
				while (currentMapElementsIterator.MoveNext())
				{
					MapElementContainer current = currentMapElementsIterator.Current;
					foreach (MapElementContainer label in labelsToDraw)
					{
						if (label.ClashesWith(current))
						{
							break;
						}
					}
				}

				labelsToDraw.UnionWith(currentElementsOrdered);

				// update dependencies, add to the dependencies list all the elements that overlap to the
				// neighbouring tiles, first clearing out the cache for this relation.
				foreach (Tile tile in neighbours)
				{
					tileDependencies.RemoveTileData(renderContext.rendererJob.tile, tile);
					foreach (MapElementContainer element in labelsToDraw)
					{
						if (element.Intersects(tile.BoundaryAbsolute))
						{
							tileDependencies.AddOverlappingElement(renderContext.rendererJob.tile, tile, element);
						}
					}
				}
			}
			return labelsToDraw;
		}

		private void ProcessReadMapData(RenderContext renderContext, MapReadResult mapReadResult)
		{
			if (mapReadResult == null)
			{
				return;
			}

			foreach (PointOfInterest pointOfInterest in mapReadResult.PointOfInterests)
			{
				RenderPointOfInterest(renderContext, pointOfInterest);
			}

			foreach (Way way in mapReadResult.Ways)
			{
				RenderWay(renderContext, new PolylineContainer(way, renderContext.rendererJob.tile));
			}

			if (mapReadResult.IsWater)
			{
				RenderWaterBackground(renderContext);
			}
		}

		private void RenderPointOfInterest(RenderContext renderContext, PointOfInterest pointOfInterest)
		{
			renderContext.DrawingLayers = pointOfInterest.Layer;
			renderContext.renderTheme.MatchNode(this, renderContext, pointOfInterest);
		}

		private void RenderWaterBackground(RenderContext renderContext)
		{
			renderContext.DrawingLayers = (sbyte) 0;
			Point[] coordinates = GetTilePixelCoordinates(renderContext.rendererJob.tile.TileSize);
			PolylineContainer way = new PolylineContainer(coordinates, renderContext.rendererJob.tile, new List<Tag>() { TAG_NATURAL_WATER });
			renderContext.renderTheme.MatchClosedWay(this, renderContext, way);
		}

		private void RenderWay(RenderContext renderContext, PolylineContainer way)
		{
			renderContext.DrawingLayers = way.Layer;

			if (way.ClosedWay)
			{
				renderContext.renderTheme.MatchClosedWay(this, renderContext, way);
			}
			else
			{
				renderContext.renderTheme.MatchLinearWay(this, renderContext, way);
			}
		}
	}
}