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

namespace org.mapsforge.map.layer
{
    using System.Threading;

    using Bitmap = org.mapsforge.core.graphics.Bitmap;
	using Canvas = org.mapsforge.core.graphics.Canvas;
	using GraphicFactory = org.mapsforge.core.graphics.GraphicFactory;
	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using Dimension = org.mapsforge.core.model.Dimension;
	using MapPosition = org.mapsforge.core.model.MapPosition;
	using Point = org.mapsforge.core.model.Point;
	using MapViewPosition = org.mapsforge.map.model.MapViewPosition;
	using MapPositionUtil = org.mapsforge.map.util.MapPositionUtil;
	using PausableThread = org.mapsforge.map.util.PausableThread;
	using FrameBuffer = org.mapsforge.map.view.FrameBuffer;
	using MapView = org.mapsforge.map.view.MapView;

	public class LayerManager : PausableThread, Redrawer
	{
		private const int MILLISECONDS_PER_FRAME = 30;

		private readonly Canvas drawingCanvas;
		private readonly Layers layers;
		private readonly MapView mapView;
		private readonly MapViewPosition mapViewPosition;
		private bool redrawNeeded;

		public LayerManager(MapView mapView, MapViewPosition mapViewPosition, GraphicFactory graphicFactory) : base()
		{

			this.mapView = mapView;
			this.mapViewPosition = mapViewPosition;

			this.drawingCanvas = graphicFactory.CreateCanvas();
			this.layers = new Layers(this, mapView.Model.displayModel);
		}

		public virtual Layers Layers
		{
			get
			{
				return this.layers;
			}
		}

		public virtual void RedrawLayers()
		{
			this.redrawNeeded = true;
			lock (this)
			{
				Monitor.Pulse(this);
			}
		}

		protected internal override void AfterRun()
		{
			foreach (Layer layer in this.layers)
			{
				layer.OnDestroy();
			}
			this.drawingCanvas.Destroy();
		}

		protected internal override void DoWork()
		{
			long startTime = System.DateTime.Now.Ticks;
			this.redrawNeeded = false;

			FrameBuffer frameBuffer = this.mapView.FrameBuffer;
			Bitmap bitmap = frameBuffer.DrawingBitmap;
			if (bitmap != null)
			{
				this.drawingCanvas.Bitmap = bitmap;

				MapPosition mapPosition = this.mapViewPosition.MapPosition;
				Dimension canvasDimension = this.drawingCanvas.Dimension;
				int tileSize = this.mapView.Model.displayModel.TileSize;
				BoundingBox boundingBox = MapPositionUtil.GetBoundingBox(mapPosition, canvasDimension, tileSize);
				Point topLeftPoint = MapPositionUtil.GetTopLeftPoint(mapPosition, canvasDimension, tileSize);

				foreach (Layer layer in this.layers)
				{
					if (layer.Visible)
					{
						layer.Draw(boundingBox, mapPosition.zoomLevel, this.drawingCanvas, topLeftPoint);
					}
				}

				if (!mapViewPosition.AnimationInProgress())
				{
					// this causes a lot of flickering when an animation
					// is in progress
					frameBuffer.FrameFinished(mapPosition);
					this.mapView.Repaint();
				}
				else
				{
					// make sure that we redraw at the end
					this.redrawNeeded = true;
				}
			}

			long elapsedMilliseconds = (System.DateTime.Now.Ticks - startTime) / 10000;
			long timeSleep = MILLISECONDS_PER_FRAME - elapsedMilliseconds;

			if (timeSleep > 1 && !Interrupted)
			{
                System.Threading.Tasks.Task.Delay((int)timeSleep);
			}
		}

		protected internal override ThreadPriority ThreadPriority
		{
			get
			{
				return ThreadPriority.NORMAL;
			}
		}

		protected internal override bool HasWork()
		{
			return this.redrawNeeded;
		}
	}
}