/*
 * Copyright 2015 Ludwig M Brinckmann
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

namespace org.mapsforge.map.rendertheme
{
    using System;
    using System.Collections.Generic;

    using MapElementContainer = org.mapsforge.core.mapelements.MapElementContainer;
	using CanvasRasterer = org.mapsforge.map.layer.renderer.CanvasRasterer;
	using RendererJob = org.mapsforge.map.layer.renderer.RendererJob;
	using ShapePaintContainer = org.mapsforge.map.layer.renderer.ShapePaintContainer;
	using RenderTheme = org.mapsforge.map.rendertheme.rule.RenderTheme;

	/// 
	/// <summary>
	/// A RenderContext contains all the information and data to render a map area, it is passed between
	/// calls in order to avoid local data stored in the DatabaseRenderer.
	/// 
	/// </summary>
	public class RenderContext
	{
		private const sbyte LAYERS = 11;

		private const double STROKE_INCREASE = 1.5;
		private const sbyte STROKE_MIN_ZOOM_LEVEL = 12;

		// Configuration that drives the rendering
		public readonly RenderTheme renderTheme;
		public readonly RendererJob rendererJob;
		public readonly CanvasRasterer canvasRasterer;

		// Data generated for the rendering process
		private IList<IList<ShapePaintContainer>> drawingLayers;
		public readonly ICollection<MapElementContainer> labels;
		public readonly IList<IList<IList<ShapePaintContainer>>> ways;

		public RenderContext(RenderTheme renderTheme, RendererJob rendererJob, CanvasRasterer canvasRasterer)
		{
			this.rendererJob = rendererJob;
			this.labels = (ICollection<MapElementContainer>) new LinkedList<MapElementContainer>();
			this.renderTheme = renderTheme;
			this.canvasRasterer = canvasRasterer;
			this.ways = CreateWayLists();
			ScaleStrokeWidth = rendererJob.tile.ZoomLevel;
			renderTheme.ScaleTextSize(rendererJob.textScale, this.rendererJob.tile.ZoomLevel);
		}

		public virtual void Destroy()
		{
			this.canvasRasterer.Destroy();
		}

		public virtual sbyte DrawingLayers
		{
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				else if (value >= RenderContext.LAYERS)
				{
					value = RenderContext.LAYERS - 1;
				}
				this.drawingLayers = ways[value];
			}
		}

		public virtual void AddToCurrentDrawingLayer(int level, ShapePaintContainer element)
		{
			this.drawingLayers[level].Add(element);
		}

		private IList<IList<IList<ShapePaintContainer>>> CreateWayLists()
		{
			IList<IList<IList<ShapePaintContainer>>> result = new List<IList<IList<ShapePaintContainer>>>(LAYERS);
			int levels = this.renderTheme.Levels;

			for (sbyte i = LAYERS - 1; i >= 0; --i)
			{
				IList<IList<ShapePaintContainer>> innerWayList = new List<IList<ShapePaintContainer>>(levels);
				for (int j = levels - 1; j >= 0; --j)
				{
					innerWayList.Add(new List<ShapePaintContainer>(0));
				}
				result.Add(innerWayList);
			}
			return result;
		}

		/// <summary>
		/// Sets the scale stroke factor for the given zoom level.
		/// </summary>
		/// <param name="zoomLevel">
		///            the zoom level for which the scale stroke factor should be set. </param>
		private sbyte ScaleStrokeWidth
		{
			set
			{
				int zoomLevelDiff = Math.Max(value - STROKE_MIN_ZOOM_LEVEL, 0);
				renderTheme.ScaleStrokeWidth((float) Math.Pow(STROKE_INCREASE, zoomLevelDiff), this.rendererJob.tile.ZoomLevel);
			}
		}
	}
}