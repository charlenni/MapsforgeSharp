/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.core.mapelements
{
    using System;
    using System.Text;

    using Canvas = org.mapsforge.core.graphics.Canvas;
	using Display = org.mapsforge.core.graphics.Display;
	using Matrix = org.mapsforge.core.graphics.Matrix;
	using Paint = org.mapsforge.core.graphics.Paint;
	using Point = org.mapsforge.core.model.Point;
	using Rectangle = org.mapsforge.core.model.Rectangle;

	public class WayTextContainer : MapElementContainer
	{
		private readonly Paint paintFront;
		private readonly Paint paintBack;
		private readonly string text;
		private readonly Point end;

		public WayTextContainer(Point point, Point end, Display display, int priority, string text, Paint paintFront, Paint paintBack, double textHeight) : base(point, display, priority)
		{
			this.text = text;
			this.paintFront = paintFront;
			this.paintBack = paintBack;
			this.end = end;

			this.boundary = null;
			// a way text container should always run left to right, but I leave this in because it might matter
			// if we support right-to-left text.
			// we also need to make the container larger by textHeight as otherwise the end points do
			// not correctly reflect the size of the text on screen
			this.boundaryAbsolute = (new Rectangle(Math.Min(point.X, end.X), Math.Min(point.Y, end.Y), Math.Max(point.X, end.X), Math.Max(point.Y, end.Y))).Envelope(textHeight / 2d);
		}

		public override void Draw(Canvas canvas, Point origin, Matrix matrix)
		{
			Point adjustedStart = xy.Offset(-origin.X, -origin.Y);
			Point adjustedEnd = end.Offset(-origin.X, -origin.Y);

			if (this.paintBack != null)
			{
				canvas.DrawTextRotated(text, (int)(adjustedStart.X), (int)(adjustedStart.Y), (int)(adjustedEnd.X), (int)(adjustedEnd.Y), this.paintBack);
			}
			canvas.DrawTextRotated(text, (int)(adjustedStart.X), (int)(adjustedStart.Y), (int)(adjustedEnd.X), (int)(adjustedEnd.Y), this.paintFront);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.ToString());
			stringBuilder.Append(", text=");
			stringBuilder.Append(this.text);
			return stringBuilder.ToString();
		}
	}
}