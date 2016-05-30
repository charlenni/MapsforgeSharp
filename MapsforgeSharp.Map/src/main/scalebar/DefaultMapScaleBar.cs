/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2014, 2015 devemux86
 * Copyright 2014 Erik Duisters
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

namespace org.mapsforge.map.scalebar
{
    using System;

    using Canvas = MapsforgeSharp.Core.Graphics.Canvas;
	using Cap = MapsforgeSharp.Core.Graphics.Cap;
	using Color = MapsforgeSharp.Core.Graphics.Color;
	using FontFamily = MapsforgeSharp.Core.Graphics.FontFamily;
	using FontStyle = MapsforgeSharp.Core.Graphics.FontStyle;
	using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
	using IPaint = MapsforgeSharp.Core.Graphics.IPaint;
	using Style = MapsforgeSharp.Core.Graphics.Style;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;
	using MapViewDimension = org.mapsforge.map.model.MapViewDimension;
	using MapViewPosition = org.mapsforge.map.model.MapViewPosition;

	/// <summary>
	/// Displays the default mapsforge MapScaleBar
	/// </summary>
	public class DefaultMapScaleBar : MapScaleBar
	{
		private const int BITMAP_HEIGHT = 40;
		private const int BITMAP_WIDTH = 120;
		private const int SCALE_BAR_MARGIN = 10;
		private const float STROKE_EXTERNAL = 4;
		private const float STROKE_INTERNAL = 2;
		private const int TEXT_MARGIN = 1;

		public enum ScaleBarMode
		{
			BOTH,
			SINGLE
		}
		private ScaleBarMode scaleBarMode;
		private DistanceUnitAdapter secondaryDistanceUnitAdapter;

		private readonly IPaint paintScaleBar;
		private readonly IPaint paintScaleBarStroke;
		private readonly IPaint paintScaleText;
		private readonly IPaint paintScaleTextStroke;

		public DefaultMapScaleBar(MapViewPosition mapViewPosition, MapViewDimension mapViewDimension, IGraphicFactory graphicFactory, DisplayModel displayModel) : base(mapViewPosition, mapViewDimension, displayModel, graphicFactory, BITMAP_WIDTH, BITMAP_HEIGHT)
		{

			this.scaleBarMode = ScaleBarMode.BOTH;
			this.secondaryDistanceUnitAdapter = ImperialUnitAdapter.INSTANCE;

			this.paintScaleBar = CreateScaleBarPaint(Color.BLACK, STROKE_INTERNAL, Style.FILL);
			this.paintScaleBarStroke = CreateScaleBarPaint(Color.WHITE, STROKE_EXTERNAL, Style.STROKE);
			this.paintScaleText = CreateTextPaint(Color.BLACK, 0, Style.FILL);
			this.paintScaleTextStroke = CreateTextPaint(Color.WHITE, 2, Style.STROKE);
		}

		/// <returns> the secondary <seealso cref="DistanceUnitAdapter"/> in use by this MapScaleBar </returns>
		public virtual DistanceUnitAdapter SecondaryDistanceUnitAdapter
		{
			get
			{
				return this.secondaryDistanceUnitAdapter;
			}
			set
			{
				if (value == null)
				{
					throw new System.ArgumentException("adapter must not be null");
				}
				this.secondaryDistanceUnitAdapter = value;
				this.redrawNeeded = true;
			}
		}


		public virtual ScaleBarMode GetScaleBarMode()
		{
			return this.scaleBarMode;
		}

		public virtual void SetScaleBarMode(ScaleBarMode scaleBarMode)
		{
			this.scaleBarMode = scaleBarMode;
			this.redrawNeeded = true;
		}

		private IPaint CreateScaleBarPaint(Color color, float strokeWidth, Style style)
		{
			IPaint paint = this.graphicFactory.CreatePaint();
			paint.Color = color;
			paint.StrokeWidth = strokeWidth * this.displayModel.ScaleFactor;
			paint.Style = style;
			paint.StrokeCap = Cap.SQUARE;
			return paint;
		}

		private IPaint CreateTextPaint(Color color, float strokeWidth, Style style)
		{
			IPaint paint = this.graphicFactory.CreatePaint();
			paint.Color = color;
			paint.StrokeWidth = strokeWidth * this.displayModel.ScaleFactor;
			paint.Style = style;
			paint.SetTypeface(FontFamily.DEFAULT, FontStyle.BOLD);
			paint.TextSize = 12 * this.displayModel.ScaleFactor;

			return paint;
		}

		protected internal override void Redraw(Canvas canvas)
		{
			canvas.FillColor(Color.TRANSPARENT);

			float scale = this.displayModel.ScaleFactor;
			ScaleBarLengthAndValue lengthAndValue = this.CalculateScaleBarLengthAndValue();
			ScaleBarLengthAndValue lengthAndValue2;

			if (this.scaleBarMode == ScaleBarMode.BOTH)
			{
				lengthAndValue2 = this.CalculateScaleBarLengthAndValue(this.secondaryDistanceUnitAdapter);
			}
			else
			{
				lengthAndValue2 = new ScaleBarLengthAndValue(0, 0);
			}

			DrawScaleBar(canvas, lengthAndValue.scaleBarLength, lengthAndValue2.scaleBarLength, this.paintScaleBarStroke, scale);
			DrawScaleBar(canvas, lengthAndValue.scaleBarLength, lengthAndValue2.scaleBarLength, this.paintScaleBar, scale);

			string scaleText1 = this.distanceUnitAdapter.GetScaleText(lengthAndValue.scaleBarValue);
			string scaleText2 = this.scaleBarMode == ScaleBarMode.BOTH ? this.secondaryDistanceUnitAdapter.GetScaleText(lengthAndValue2.scaleBarValue) : "";

			DrawScaleText(canvas, scaleText1, scaleText2, this.paintScaleTextStroke, scale);
			DrawScaleText(canvas, scaleText1, scaleText2, this.paintScaleText, scale);
		}

		private void DrawScaleBar(Canvas canvas, int scaleBarLength1, int scaleBarLength2, IPaint paint, float scale)
		{
			int maxScaleBarLength = Math.Max(scaleBarLength1, scaleBarLength2);

			switch (scaleBarPosition)
			{
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_CENTER:
				if (scaleBarLength2 == 0)
				{
					canvas.DrawLine((int)Math.Round((canvas.Width - maxScaleBarLength) * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), (int)Math.Round((canvas.Width + maxScaleBarLength) * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round((canvas.Width - maxScaleBarLength) * 0.5f), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round((canvas.Width - maxScaleBarLength) * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round((canvas.Width + maxScaleBarLength) * 0.5f), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round((canvas.Width + maxScaleBarLength) * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
				}
				else
				{
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + maxScaleBarLength), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength1), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength1), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength2), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength2), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
				}
				break;
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_LEFT:
				if (scaleBarLength2 == 0)
				{
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + maxScaleBarLength), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + maxScaleBarLength), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + maxScaleBarLength), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
				}
				else
				{
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + maxScaleBarLength), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength1), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength1), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength2), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength2), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
				}
				break;
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_RIGHT:
				if (scaleBarLength2 == 0)
				{
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - maxScaleBarLength), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - maxScaleBarLength), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - maxScaleBarLength), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
				}
				else
				{
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - maxScaleBarLength), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - scaleBarLength1), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - scaleBarLength1), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - scaleBarLength2), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - scaleBarLength2), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
				}
				break;
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_CENTER:
				if (scaleBarLength2 == 0)
				{
					canvas.DrawLine((int)Math.Round((canvas.Width - maxScaleBarLength) * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round((canvas.Width + maxScaleBarLength) * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round((canvas.Width - maxScaleBarLength) * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round((canvas.Width - maxScaleBarLength) * 0.5f), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round((canvas.Width + maxScaleBarLength) * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round((canvas.Width + maxScaleBarLength) * 0.5f), (int)Math.Round(canvas.Height * 0.5f), paint);
				}
				else
				{
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + maxScaleBarLength), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength1), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength1), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength2), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength2), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
				}
				break;
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_LEFT:
				if (scaleBarLength2 == 0)
				{
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + maxScaleBarLength), (int)Math.Round(SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + maxScaleBarLength), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + maxScaleBarLength), (int)Math.Round(canvas.Height * 0.5f), paint);
				}
				else
				{
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + maxScaleBarLength), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength1), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength1), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength2), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(STROKE_EXTERNAL * scale * 0.5f + scaleBarLength2), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
				}
				break;
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_RIGHT:
				if (scaleBarLength2 == 0)
				{
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - maxScaleBarLength), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - maxScaleBarLength), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - maxScaleBarLength), (int)Math.Round(canvas.Height * 0.5f), paint);
				}
				else
				{
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - maxScaleBarLength), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - scaleBarLength1), (int)Math.Round(SCALE_BAR_MARGIN * scale), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - scaleBarLength1), (int)Math.Round(canvas.Height * 0.5f), paint);
					canvas.DrawLine((int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - scaleBarLength2), (int)Math.Round(canvas.Height * 0.5f), (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale * 0.5f - scaleBarLength2), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale), paint);
				}
				break;
			}
		}

		private void DrawScaleText(Canvas canvas, string scaleText1, string scaleText2, IPaint paint, float scale)
		{
			switch (scaleBarPosition)
			{
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_CENTER:
				if (scaleText2.Length == 0)
				{
					canvas.DrawText(scaleText1, (int)Math.Round((canvas.Width - this.paintScaleTextStroke.GetTextWidth(scaleText1)) * 0.5f), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale - STROKE_EXTERNAL * scale * 0.5f - TEXT_MARGIN * scale), paint);
				}
				else
				{
					canvas.DrawText(scaleText1, (int)Math.Round(STROKE_EXTERNAL * scale + TEXT_MARGIN * scale), (int)Math.Round(canvas.Height * 0.5f - STROKE_EXTERNAL * scale * 0.5f - TEXT_MARGIN * scale), paint);
					canvas.DrawText(scaleText2, (int)Math.Round(STROKE_EXTERNAL * scale + TEXT_MARGIN * scale), (int)Math.Round(canvas.Height * 0.5f + STROKE_EXTERNAL * scale * 0.5f + TEXT_MARGIN * scale + this.paintScaleTextStroke.GetTextHeight(scaleText2)), paint);
				}
				break;
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_LEFT:
				if (scaleText2.Length == 0)
				{
					canvas.DrawText(scaleText1, (int)Math.Round(STROKE_EXTERNAL * scale + TEXT_MARGIN * scale), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale - STROKE_EXTERNAL * scale * 0.5f - TEXT_MARGIN * scale), paint);
				}
				else
				{
					canvas.DrawText(scaleText1, (int)Math.Round(STROKE_EXTERNAL * scale + TEXT_MARGIN * scale), (int)Math.Round(canvas.Height * 0.5f - STROKE_EXTERNAL * scale * 0.5f - TEXT_MARGIN * scale), paint);
					canvas.DrawText(scaleText2, (int)Math.Round(STROKE_EXTERNAL * scale + TEXT_MARGIN * scale), (int)Math.Round(canvas.Height * 0.5f + STROKE_EXTERNAL * scale * 0.5f + TEXT_MARGIN * scale + this.paintScaleTextStroke.GetTextHeight(scaleText2)), paint);
				}
				break;
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_RIGHT:
				if (scaleText2.Length == 0)
				{
					canvas.DrawText(scaleText1, (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale - TEXT_MARGIN * scale - this.paintScaleTextStroke.GetTextWidth(scaleText1)), (int)Math.Round(canvas.Height - SCALE_BAR_MARGIN * scale - STROKE_EXTERNAL * scale * 0.5f - TEXT_MARGIN * scale), paint);
				}
				else
				{
					canvas.DrawText(scaleText1, (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale - TEXT_MARGIN * scale - this.paintScaleTextStroke.GetTextWidth(scaleText1)), (int)Math.Round(canvas.Height * 0.5f - STROKE_EXTERNAL * scale * 0.5f - TEXT_MARGIN * scale), paint);
					canvas.DrawText(scaleText2, (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale - TEXT_MARGIN * scale - this.paintScaleTextStroke.GetTextWidth(scaleText2)), (int)Math.Round(canvas.Height * 0.5f + STROKE_EXTERNAL * scale * 0.5f + TEXT_MARGIN * scale + this.paintScaleTextStroke.GetTextHeight(scaleText2)), paint);
				}
				break;
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_CENTER:
				if (scaleText2.Length == 0)
				{
					canvas.DrawText(scaleText1, (int)Math.Round((canvas.Width - this.paintScaleTextStroke.GetTextWidth(scaleText1)) * 0.5f), (int)Math.Round(SCALE_BAR_MARGIN * scale + STROKE_EXTERNAL * scale * 0.5f + TEXT_MARGIN * scale + this.paintScaleTextStroke.GetTextHeight(scaleText1)), paint);
				}
				else
				{
					canvas.DrawText(scaleText1, (int)Math.Round(STROKE_EXTERNAL * scale + TEXT_MARGIN * scale), (int)Math.Round(canvas.Height * 0.5f - STROKE_EXTERNAL * scale * 0.5f - TEXT_MARGIN * scale), paint);
					canvas.DrawText(scaleText2, (int)Math.Round(STROKE_EXTERNAL * scale + TEXT_MARGIN * scale), (int)Math.Round(canvas.Height * 0.5f + STROKE_EXTERNAL * scale * 0.5f + TEXT_MARGIN * scale + this.paintScaleTextStroke.GetTextHeight(scaleText2)), paint);
				}
				break;
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_LEFT:
				if (scaleText2.Length == 0)
				{
					canvas.DrawText(scaleText1, (int)Math.Round(STROKE_EXTERNAL * scale + TEXT_MARGIN * scale), (int)Math.Round(SCALE_BAR_MARGIN * scale + STROKE_EXTERNAL * scale * 0.5f + TEXT_MARGIN * scale + this.paintScaleTextStroke.GetTextHeight(scaleText1)), paint);
				}
				else
				{
					canvas.DrawText(scaleText1, (int)Math.Round(STROKE_EXTERNAL * scale + TEXT_MARGIN * scale), (int)Math.Round(canvas.Height * 0.5f - STROKE_EXTERNAL * scale * 0.5f - TEXT_MARGIN * scale), paint);
					canvas.DrawText(scaleText2, (int)Math.Round(STROKE_EXTERNAL * scale + TEXT_MARGIN * scale), (int)Math.Round(canvas.Height * 0.5f + STROKE_EXTERNAL * scale * 0.5f + TEXT_MARGIN * scale + this.paintScaleTextStroke.GetTextHeight(scaleText2)), paint);
				}
				break;
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_RIGHT:
				if (scaleText2.Length == 0)
				{
					canvas.DrawText(scaleText1, (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale - TEXT_MARGIN * scale - this.paintScaleTextStroke.GetTextWidth(scaleText1)), (int)Math.Round(SCALE_BAR_MARGIN * scale + STROKE_EXTERNAL * scale * 0.5f + TEXT_MARGIN * scale + this.paintScaleTextStroke.GetTextHeight(scaleText1)), paint);
				}
				else
				{
					canvas.DrawText(scaleText1, (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale - TEXT_MARGIN * scale - this.paintScaleTextStroke.GetTextWidth(scaleText1)), (int)Math.Round(canvas.Height * 0.5f - STROKE_EXTERNAL * scale * 0.5f - TEXT_MARGIN * scale), paint);
					canvas.DrawText(scaleText2, (int)Math.Round(canvas.Width - STROKE_EXTERNAL * scale - TEXT_MARGIN * scale - this.paintScaleTextStroke.GetTextWidth(scaleText2)), (int)Math.Round(canvas.Height * 0.5f + STROKE_EXTERNAL * scale * 0.5f + TEXT_MARGIN * scale + this.paintScaleTextStroke.GetTextHeight(scaleText2)), paint);
				}
				break;
			}
		}
	}
}