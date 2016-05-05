/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2014, 2015 devemux86
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

namespace org.mapsforge.map.layer.debug
{
	using Canvas = MapsforgeSharp.Core.Graphics.Canvas;
	using Color = MapsforgeSharp.Core.Graphics.Color;
	using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
	using Paint = MapsforgeSharp.Core.Graphics.Paint;
	using Style = MapsforgeSharp.Core.Graphics.Style;
	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using Point = org.mapsforge.core.model.Point;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;

	public class TileGridLayer : Layer
	{
		private static Paint createPaintFront(GraphicFactory graphicFactory, DisplayModel displayModel)
		{
			Paint paint = graphicFactory.CreatePaint();
			paint.Color = Color.RED;
			paint.StrokeWidth = 2 * displayModel.ScaleFactor;
			paint.Style = Style.STROKE;
			return paint;
		}

		private static Paint createPaintBack(GraphicFactory graphicFactory, DisplayModel displayModel)
		{
			Paint paint = graphicFactory.CreatePaint();
			paint.Color = Color.WHITE;
			paint.StrokeWidth = 4 * displayModel.ScaleFactor;
			paint.Style = Style.STROKE;
			return paint;
		}

		private readonly DisplayModel displayModel;
		private readonly Paint paintBack, paintFront;

		public TileGridLayer(GraphicFactory graphicFactory, DisplayModel displayModel) : base()
		{
			this.displayModel = displayModel;

			this.paintBack = createPaintBack(graphicFactory, displayModel);
			this.paintFront = createPaintFront(graphicFactory, displayModel);
		}

		public TileGridLayer(DisplayModel displayModel, Paint paintBack, Paint paintFront) : base()
		{
			this.displayModel = displayModel;
			this.paintBack = paintBack;
			this.paintFront = paintFront;
		}

		public override void Draw(BoundingBox boundingBox, sbyte zoomLevel, Canvas canvas, Point topLeftPoint)
		{
			long tileLeft = MercatorProjection.LongitudeToTileX(boundingBox.MinLongitude, zoomLevel);
			long tileTop = MercatorProjection.LatitudeToTileY(boundingBox.MaxLatitude, zoomLevel);
			long tileRight = MercatorProjection.LongitudeToTileX(boundingBox.MaxLongitude, zoomLevel);
			long tileBottom = MercatorProjection.LatitudeToTileY(boundingBox.MinLatitude, zoomLevel);

			int tileSize = this.displayModel.TileSize;
			int pixelX1 = (int)(MercatorProjection.TileToPixel(tileLeft, tileSize) - topLeftPoint.X);
			int pixelY1 = (int)(MercatorProjection.TileToPixel(tileTop, tileSize) - topLeftPoint.Y);
			int pixelX2 = (int)(MercatorProjection.TileToPixel(tileRight, tileSize) - topLeftPoint.X + tileSize);
			int pixelY2 = (int)(MercatorProjection.TileToPixel(tileBottom, tileSize) - topLeftPoint.Y + tileSize);

			for (int lineX = pixelX1; lineX <= pixelX2 + 1; lineX += tileSize)
			{
				canvas.DrawLine(lineX, pixelY1, lineX, pixelY2, this.paintBack);
			}

			for (int lineY = pixelY1; lineY <= pixelY2 + 1; lineY += tileSize)
			{
				canvas.DrawLine(pixelX1, lineY, pixelX2, lineY, this.paintBack);
			}

			for (int lineX = pixelX1; lineX <= pixelX2 + 1; lineX += tileSize)
			{
				canvas.DrawLine(lineX, pixelY1, lineX, pixelY2, this.paintFront);
			}

			for (int lineY = pixelY1; lineY <= pixelY2 + 1; lineY += tileSize)
			{
				canvas.DrawLine(pixelX1, lineY, pixelX2, lineY, this.paintFront);
			}
		}
	}
}