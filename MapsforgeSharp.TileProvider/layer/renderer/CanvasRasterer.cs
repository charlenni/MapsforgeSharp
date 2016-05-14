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

namespace org.mapsforge.map.layer.renderer
{
	using System.Collections.Generic;
	using System.Linq;

  using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
	using Canvas = MapsforgeSharp.Core.Graphics.Canvas;
	using Color = MapsforgeSharp.Core.Graphics.Color;
	using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
	using GraphicUtils = MapsforgeSharp.Core.Graphics.GraphicUtils;
	using Matrix = MapsforgeSharp.Core.Graphics.Matrix;
	using Path = MapsforgeSharp.Core.Graphics.Path;
	using MapElementContainer = MapsforgeSharp.Core.Mapelements.MapElementContainer;
	using Point = MapsforgeSharp.Core.Model.Point;
	using Rectangle = MapsforgeSharp.Core.Model.Rectangle;
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using RenderContext = org.mapsforge.map.rendertheme.RenderContext;

	public class CanvasRasterer
	{
		private Canvas canvas;
		private readonly Path path;
		private readonly Matrix symbolMatrix;
		// TODO: Delete
		private readonly GraphicFactory graphicFactory;

		internal CanvasRasterer(GraphicFactory graphicFactory)
		{
			this.symbolMatrix = graphicFactory.CreateMatrix();
			this.path = graphicFactory.CreatePath();
			// TODO: Delete
			this.graphicFactory = graphicFactory;
		}

		public virtual void Destroy()
		{
			this.canvas?.Destroy();
		}

		public Canvas Canvas
		{
			get { return this.canvas; }
			set { this.canvas = value; }
		}

		internal virtual void DrawWays(RenderContext renderContext)
		{
			int levelsPerLayer = renderContext.ways[0].Count;

			for (int layer = 0, layers = renderContext.ways.Count; layer < layers; ++layer)
			{
				IList<IList<ShapePaintContainer>> shapePaintContainers = renderContext.ways[layer];

				for (int level = 0; level < levelsPerLayer; ++level)
				{
					IList<ShapePaintContainer> wayList = shapePaintContainers[level];

					for (int index = wayList.Count - 1; index >= 0; --index)
					{
						DrawShapePaintContainer(wayList[index]);
					}
				}
			}
		}

		internal virtual void DrawMapElements(ISet<MapElementContainer> elements, Tile tile)
		{
			if (canvas == null)
			{
				return;
			}

            // we have a set of all map elements (needed so we do not draw elements twice),
            // but we need to draw in priority order as we now allow overlaps. So we
            // convert into list, then sort, then draw.
            // draw elements in order of priority: lower priority first, so more important
            // elements will be drawn on top (in case of display=true) items.
            var elementsAsList = from element in elements orderby element.Priority ascending select element;

			foreach (MapElementContainer element in elementsAsList)
			{
				element.Draw(canvas, tile.Origin, this.symbolMatrix);
			}
		}

		internal virtual void Fill(int color)
		{
			if (canvas == null)
			{
				return;
			}

			if (GraphicUtils.GetAlpha(color) > 0)
			{
				this.canvas.FillColor(color);
			}
		}

		/// <summary>
		/// Fills the area outside the specificed rectangle with color. Use this method when
		/// overpainting with a transparent color as it sets the PorterDuff mode.
		/// This method is used to blank out areas that fall outside the map area. </summary>
		/// <param name="color"> the fill color for the outside area </param>
		/// <param name="insideArea"> the inside area on which not to draw </param>
		internal virtual void FillOutsideAreas(Color color, Rectangle insideArea)
		{
			if (canvas == null)
			{
				return;
			}

			this.canvas.SetClipDifference((int) insideArea.Left, (int) insideArea.Top, (int) insideArea.Width, (int) insideArea.Height);
			this.canvas.FillColor(color);
			this.canvas.ResetClip();
		}

		/// <summary>
		/// Fills the area outside the specificed rectangle with color.
		/// This method is used to blank out areas that fall outside the map area. </summary>
		/// <param name="color"> the fill color for the outside area </param>
		/// <param name="insideArea"> the inside area on which not to draw </param>
		internal virtual void FillOutsideAreas(int color, Rectangle insideArea)
		{
			if (canvas == null)
			{
				return;
			}

			this.canvas.SetClipDifference((int) insideArea.Left, (int) insideArea.Top, (int) insideArea.Width, (int) insideArea.Height);
			this.canvas.FillColor(color);
			this.canvas.ResetClip();
		}

		private void DrawCircleContainer(ShapePaintContainer shapePaintContainer)
		{
			if (canvas == null)
			{
				return;
			}

			CircleContainer circleContainer = (CircleContainer) shapePaintContainer.shapeContainer;
			Point point = circleContainer.point;
			this.canvas.DrawCircle((int) point.X, (int) point.Y, (int) circleContainer.radius, shapePaintContainer.paint);
		}

		private void DrawPath(ShapePaintContainer shapePaintContainer, Point[][] coordinates, float dy)
		{
			if (canvas == null)
			{
				return;
			}

			// TODO
			//this.path.Clear();
			var path = this.graphicFactory.CreatePath();

			foreach (Point[] innerList in coordinates)
			{
				Point[] points;
				if (dy != 0f)
				{
					points = RendererUtils.ParallelPath(innerList, dy);
				}
				else
				{
					points = innerList;
				}
				if (points.Length >= 2)
				{
					Point point = points[0];
					path.MoveTo((float)point.X, (float)point.Y);
					for (int i = 1; i < points.Length; ++i)
					{
						point = points[i];
						path.LineTo((int)point.X, (int)point.Y);
					}
				}
			}

			this.canvas.DrawPath(path, shapePaintContainer.paint);
		}

		private void DrawShapePaintContainer(ShapePaintContainer shapePaintContainer)
		{
			if (canvas == null)
			{
				return;
			}

			ShapeType shapeType = shapePaintContainer.shapeContainer.ShapeType;
			switch (shapeType)
			{
				case org.mapsforge.map.layer.renderer.ShapeType.Circle:
					DrawCircleContainer(shapePaintContainer);
					return;

				case org.mapsforge.map.layer.renderer.ShapeType.Polyline:
					PolylineContainer polylineContainer = (PolylineContainer) shapePaintContainer.shapeContainer;
					DrawPath(shapePaintContainer, polylineContainer.CoordinatesRelativeToTile, shapePaintContainer.dy);
					return;
			}
		}
	}
}