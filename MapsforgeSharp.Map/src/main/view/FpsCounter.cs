/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

namespace org.mapsforge.map.view
{
    using System;

    using Color = MapsforgeSharp.Core.Graphics.Color;
	using FontFamily = MapsforgeSharp.Core.Graphics.FontFamily;
	using FontStyle = MapsforgeSharp.Core.Graphics.FontStyle;
	using GraphicContext = MapsforgeSharp.Core.Graphics.GraphicContext;
	using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
	using Paint = MapsforgeSharp.Core.Graphics.Paint;
	using Style = MapsforgeSharp.Core.Graphics.Style;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;

	/// <summary>
	/// An FPS counter measures the drawing frame rate.
	/// </summary>
	public class FpsCounter
	{
        private static readonly long ONE_SECOND = TimeSpan.TicksPerSecond;

		private static Paint CreatePaintFront(GraphicFactory graphicFactory, DisplayModel displayModel)
		{
			Paint paint = graphicFactory.CreatePaint();
			paint.Color = Color.RED;
			paint.SetTypeface(FontFamily.DEFAULT, FontStyle.BOLD);
			paint.TextSize = 25 * displayModel.ScaleFactor;
			return paint;
		}

		private static Paint CreatePaintBack(GraphicFactory graphicFactory, DisplayModel displayModel)
		{
			Paint paint = graphicFactory.CreatePaint();
			paint.Color = Color.WHITE;
			paint.SetTypeface(FontFamily.DEFAULT, FontStyle.BOLD);
			paint.TextSize = 25 * displayModel.ScaleFactor;
			paint.StrokeWidth = 2 * displayModel.ScaleFactor;
			paint.Style = Style.STROKE;
			return paint;
		}

		private readonly DisplayModel displayModel;
		private string fps;
		private int frameCounter;
		private long lastTime;
		private readonly Paint paintBack, paintFront;
		private bool visible;

		public FpsCounter(GraphicFactory graphicFactory, DisplayModel displayModel)
		{
			this.displayModel = displayModel;

			this.paintBack = CreatePaintBack(graphicFactory, displayModel);
			this.paintFront = CreatePaintFront(graphicFactory, displayModel);
		}

		public FpsCounter(DisplayModel displayModel, Paint paintBack, Paint paintFront)
		{
			this.displayModel = displayModel;
			this.paintBack = paintBack;
			this.paintFront = paintFront;
		}

		public virtual void Draw(GraphicContext graphicContext)
		{
			if (!this.visible)
			{
				return;
			}

			long currentTime = System.DateTime.Now.Ticks;
			long elapsedTime = currentTime - this.lastTime;
			if (elapsedTime > ONE_SECOND)
			{
				this.fps = Math.Round((float)(this.frameCounter * ONE_SECOND) / elapsedTime).ToString();
				this.lastTime = currentTime;
				this.frameCounter = 0;
			}

			int x = (int)(20 * displayModel.ScaleFactor);
			int y = (int)(40 * displayModel.ScaleFactor);
			graphicContext.DrawText(this.fps, x, y, this.paintBack);
			graphicContext.DrawText(this.fps, x, y, this.paintFront);
			++this.frameCounter;
		}

		public virtual bool Visible
		{
			get
			{
				return this.visible;
			}
			set
			{
				this.visible = value;
			}
		}
	}
}