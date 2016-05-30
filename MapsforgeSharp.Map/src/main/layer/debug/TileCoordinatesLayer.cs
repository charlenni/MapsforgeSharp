/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.map.layer.debug
{
    using System.Collections.Generic;
    using System.Text;

    using Canvas = MapsforgeSharp.Core.Graphics.Canvas;
	using Color = MapsforgeSharp.Core.Graphics.Color;
	using FontFamily = MapsforgeSharp.Core.Graphics.FontFamily;
	using FontStyle = MapsforgeSharp.Core.Graphics.FontStyle;
	using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
	using IPaint = MapsforgeSharp.Core.Graphics.IPaint;
	using Style = MapsforgeSharp.Core.Graphics.Style;
	using BoundingBox = MapsforgeSharp.Core.Model.BoundingBox;
	using Point = MapsforgeSharp.Core.Model.Point;
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using LayerUtil = org.mapsforge.map.util.LayerUtil;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;

	public class TileCoordinatesLayer : Layer
	{
		private static IPaint createPaintFront(IGraphicFactory graphicFactory, DisplayModel displayModel)
		{
			IPaint paint = graphicFactory.CreatePaint();
			paint.Color = Color.RED;
			paint.SetTypeface(FontFamily.DEFAULT, FontStyle.BOLD);
			paint.TextSize = 16 * displayModel.ScaleFactor;
			return paint;
		}

		private static IPaint createPaintBack(IGraphicFactory graphicFactory, DisplayModel displayModel)
		{
			IPaint paint = graphicFactory.CreatePaint();
			paint.Color = Color.WHITE;
			paint.SetTypeface(FontFamily.DEFAULT, FontStyle.BOLD);
			paint.TextSize = 16 * displayModel.ScaleFactor;
			paint.StrokeWidth = 2 * displayModel.ScaleFactor;
			paint.Style = Style.STROKE;
			return paint;
		}

		private readonly DisplayModel displayModel;
		private readonly IPaint paintBack, paintFront;

		public TileCoordinatesLayer(IGraphicFactory graphicFactory, DisplayModel displayModel) : base()
		{

			this.displayModel = displayModel;

			this.paintBack = createPaintBack(graphicFactory, displayModel);
			this.paintFront = createPaintFront(graphicFactory, displayModel);
		}

		public TileCoordinatesLayer(DisplayModel displayModel, IPaint paintBack, IPaint paintFront) : base()
		{

			this.displayModel = displayModel;
			this.paintBack = paintBack;
			this.paintFront = paintFront;
		}

		public override void Draw(BoundingBox boundingBox, sbyte zoomLevel, Canvas canvas, Point topLeftPoint)
		{
			IList<TilePosition> tilePositions = LayerUtil.GetTilePositions(boundingBox, zoomLevel, topLeftPoint, this.displayModel.TileSize);
			for (int i = tilePositions.Count - 1; i >= 0; --i)
			{
				DrawTileCoordinates(tilePositions[i], canvas);
			}
		}

		private void DrawTileCoordinates(TilePosition tilePosition, Canvas canvas)
		{
			int x = (int)(tilePosition.Point.X + 8 * displayModel.ScaleFactor);
			int y = (int)(tilePosition.Point.Y + 24 * displayModel.ScaleFactor);
			Tile tile = tilePosition.Tile;

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("X: ");
			stringBuilder.Append(tile.TileX);
			string text = stringBuilder.ToString();
			canvas.DrawText(text, x, y, this.paintBack);
			canvas.DrawText(text, x, y, this.paintFront);

			stringBuilder.Length = 0;
			stringBuilder.Append("Y: ");
			stringBuilder.Append(tile.TileY);
			text = stringBuilder.ToString();
			canvas.DrawText(text, x, (int)(y + 24 * displayModel.ScaleFactor), this.paintBack);
			canvas.DrawText(text, x, (int)(y + 24 * displayModel.ScaleFactor), this.paintFront);

			stringBuilder.Length = 0;
			stringBuilder.Append("Z: ");
			stringBuilder.Append(tile.ZoomLevel);
			text = stringBuilder.ToString();
			canvas.DrawText(text, x, (int)(y + 48 * displayModel.ScaleFactor), this.paintBack);
			canvas.DrawText(text, x, (int)(y + 48 * displayModel.ScaleFactor), this.paintFront);
		}
	}
}