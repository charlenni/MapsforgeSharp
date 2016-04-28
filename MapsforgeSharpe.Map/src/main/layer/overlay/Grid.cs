/*
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2015 devemux86
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

namespace org.mapsforge.map.layer.overlay
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Canvas = org.mapsforge.core.graphics.Canvas;
	using Color = org.mapsforge.core.graphics.Color;
	using FontFamily = org.mapsforge.core.graphics.FontFamily;
	using FontStyle = org.mapsforge.core.graphics.FontStyle;
	using GraphicFactory = org.mapsforge.core.graphics.GraphicFactory;
	using Paint = org.mapsforge.core.graphics.Paint;
	using Style = org.mapsforge.core.graphics.Style;
	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using Point = org.mapsforge.core.model.Point;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;

	/// <summary>
	/// The Grid layer draws a geographical grid.
	/// </summary>
	public class Grid : Layer
	{
		private static string ConvertCoordinate(double coordinate)
		{
			StringBuilder sb = new StringBuilder();

			if (coordinate < 0)
			{
				sb.Append('-');
				coordinate = -coordinate;
			}

			string df = "00";
			int degrees = (int) Math.Floor(coordinate);
			sb.Append(string.Format(df, degrees));
			sb.Append('°');
			coordinate -= degrees;
			coordinate *= 60.0;
			int minutes = (int) Math.Floor(coordinate);
			sb.Append(string.Format(df, minutes));
			sb.Append('′');
			coordinate -= minutes;
			coordinate *= 60.0;
			sb.Append(string.Format(df, coordinate));
			sb.Append('″');
			return sb.ToString();
		}

		private static Paint CreateLineFront(GraphicFactory graphicFactory, DisplayModel displayModel)
		{
			Paint paint = graphicFactory.CreatePaint();
			paint.Color = Color.BLUE;
			paint.StrokeWidth = 2 * displayModel.ScaleFactor;
			paint.Style = Style.STROKE;
			return paint;
		}

		private static Paint CreateLineBack(GraphicFactory graphicFactory, DisplayModel displayModel)
		{
			Paint paint = graphicFactory.CreatePaint();
			paint.Color = Color.WHITE;
			paint.StrokeWidth = 4 * displayModel.ScaleFactor;
			paint.Style = Style.STROKE;
			return paint;
		}

		private static Paint CreateTextFront(GraphicFactory graphicFactory, DisplayModel displayModel)
		{
			Paint paint = graphicFactory.CreatePaint();
			paint.Color = Color.BLUE;
			paint.setTypeface(FontFamily.DEFAULT, FontStyle.BOLD);
			paint.TextSize = 12 * displayModel.ScaleFactor;
			return paint;
		}

		private static Paint CreateTextBack(GraphicFactory graphicFactory, DisplayModel displayModel)
		{
			Paint paint = graphicFactory.CreatePaint();
			paint.Color = Color.WHITE;
			paint.setTypeface(FontFamily.DEFAULT, FontStyle.BOLD);
			paint.TextSize = 12 * displayModel.ScaleFactor;
			paint.StrokeWidth = 4 * displayModel.ScaleFactor;
			paint.Style = Style.STROKE;
			return paint;
		}

		private readonly Paint lineBack, lineFront, textBack, textFront;
		private readonly IDictionary<sbyte?, double?> spacingConfig;

		/// <summary>
		/// Ctor. </summary>
		/// <param name="graphicFactory"> the graphic factory. </param>
		/// <param name="displayModel"> the display model of the map view. </param>
		/// <param name="spacingConfig"> a map containing the spacing for every zoom level. </param>
		public Grid(GraphicFactory graphicFactory, DisplayModel displayModel, IDictionary<sbyte?, double?> spacingConfig) : base()
		{
			this.displayModel = displayModel;
			this.spacingConfig = spacingConfig;

			this.lineBack = CreateLineBack(graphicFactory, displayModel);
			this.lineFront = CreateLineFront(graphicFactory, displayModel);
			this.textBack = CreateTextBack(graphicFactory, displayModel);
			this.textFront = CreateTextFront(graphicFactory, displayModel);
		}

		/// <summary>
		/// Ctor. </summary>
		/// <param name="displayModel"> the display model of the map view. </param>
		/// <param name="spacingConfig"> a map containing the spacing for every zoom level. </param>
		/// <param name="lineBack"> the back line paint. </param>
		/// <param name="lineFront"> the top line paint. </param>
		/// <param name="textBack"> the back text paint. </param>
		/// <param name="textFront"> the top text paint. </param>
		public Grid(DisplayModel displayModel, IDictionary<sbyte?, double?> spacingConfig, Paint lineBack, Paint lineFront, Paint textBack, Paint textFront) : base()
		{
			this.displayModel = displayModel;
			this.spacingConfig = spacingConfig;
			this.lineBack = lineBack;
			this.lineFront = lineFront;
			this.textBack = textBack;
			this.textFront = textFront;
		}

		public override void Draw(BoundingBox boundingBox, sbyte zoomLevel, Canvas canvas, Point topLeftPoint)
		{
			if (spacingConfig.ContainsKey(zoomLevel))
			{
				double spacing = spacingConfig[zoomLevel].Value;

				double minLongitude = spacing * (Math.Floor(boundingBox.minLongitude / spacing));
				double maxLongitude = spacing * (Math.Ceiling(boundingBox.maxLongitude / spacing));
				double minLatitude = spacing * (Math.Floor(boundingBox.minLatitude / spacing));
				double maxLatitude = spacing * (Math.Ceiling(boundingBox.maxLatitude / spacing));

				long mapSize = MercatorProjection.GetMapSize(zoomLevel, this.displayModel.TileSize);

				int bottom = (int)(MercatorProjection.LatitudeToPixelY(minLatitude, mapSize) - topLeftPoint.y);
				int top = (int)(MercatorProjection.LatitudeToPixelY(maxLatitude, mapSize) - topLeftPoint.y);
				int left = (int)(MercatorProjection.LongitudeToPixelX(minLongitude, mapSize) - topLeftPoint.x);
				int right = (int)(MercatorProjection.LongitudeToPixelX(maxLongitude, mapSize) - topLeftPoint.x);

				for (double latitude = minLatitude; latitude <= maxLatitude; latitude += spacing)
				{
					int pixelY = (int)(MercatorProjection.LatitudeToPixelY(latitude, mapSize) - topLeftPoint.y);
					canvas.DrawLine(left, pixelY, right, pixelY, this.lineBack);
				}

				for (double longitude = minLongitude; longitude <= maxLongitude; longitude += spacing)
				{
					int pixelX = (int)(MercatorProjection.LongitudeToPixelX(longitude, mapSize) - topLeftPoint.x);
					canvas.DrawLine(pixelX, bottom, pixelX, top, this.lineBack);
				}

				for (double latitude = minLatitude; latitude <= maxLatitude; latitude += spacing)
				{
					int pixelY = (int)(MercatorProjection.LatitudeToPixelY(latitude, mapSize) - topLeftPoint.y);
					canvas.DrawLine(left, pixelY, right, pixelY, this.lineFront);
				}

				for (double longitude = minLongitude; longitude <= maxLongitude; longitude += spacing)
				{
					int pixelX = (int)(MercatorProjection.LongitudeToPixelX(longitude, mapSize) - topLeftPoint.x);
					canvas.DrawLine(pixelX, bottom, pixelX, top, this.lineFront);
				}

				for (double latitude = minLatitude; latitude <= maxLatitude; latitude += spacing)
				{
					string text = ConvertCoordinate(latitude);
					int pixelX = (canvas.Width - this.textFront.GetTextWidth(text)) / 2;
					int pixelY = (int)(MercatorProjection.LatitudeToPixelY(latitude, mapSize) - topLeftPoint.y) + this.textFront.GetTextHeight(text) / 2;
					canvas.DrawText(text, pixelX, pixelY, this.textBack);
					canvas.DrawText(text, pixelX, pixelY, this.textFront);
				}

				for (double longitude = minLongitude; longitude <= maxLongitude; longitude += spacing)
				{
					string text = ConvertCoordinate(longitude);
					int pixelX = (int)(MercatorProjection.LongitudeToPixelX(longitude, mapSize) - topLeftPoint.x) - this.textFront.GetTextWidth(text) / 2;
					int pixelY = (canvas.Height + this.textFront.GetTextHeight(text)) / 2;
					canvas.DrawText(text, pixelX, pixelY, this.textBack);
					canvas.DrawText(text, pixelX, pixelY, this.textFront);
				}
			}
		}
	}
}