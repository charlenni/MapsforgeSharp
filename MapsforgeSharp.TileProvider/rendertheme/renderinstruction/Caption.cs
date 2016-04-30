/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
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

namespace org.mapsforge.map.rendertheme.renderinstruction
{
    using System.Collections.Generic;
    using System.Xml;
    using SkiaSharp;

    using Align = org.mapsforge.core.graphics.Align;
    using Bitmap = org.mapsforge.core.graphics.Bitmap;
    using Color = org.mapsforge.core.graphics.Color;
    using Display = org.mapsforge.core.graphics.Display;
    using FontFamily = org.mapsforge.core.graphics.FontFamily;
    using FontStyle = org.mapsforge.core.graphics.FontStyle;
    using GraphicFactory = org.mapsforge.core.graphics.GraphicFactory;
    using Position = org.mapsforge.core.graphics.Position;
    using Style = org.mapsforge.core.graphics.Style;
    using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using PointOfInterest = org.mapsforge.core.datastore.PointOfInterest;
    using System;
    /// <summary>
    /// Represents a text label on the map.
    /// 
    /// If a bitmap symbol is present the caption position is calculated relative to the bitmap, the
    /// center of which is at the point of the POI. The bitmap itself is never rendered.
    /// 
    /// </summary>
    public class Caption : RenderInstruction
	{

		private Bitmap bitmap;
		private Position position;
		private Display display;
		private float dy;
		private readonly IDictionary<sbyte?, float?> dyScaled;

		private readonly SKPaint fill;
		private readonly IDictionary<sbyte?, SKPaint> fills;

		private float fontSize;
		private readonly float gap;
		private readonly int maxTextWidth;
		private int priority;
		private readonly SKPaint stroke;
		private readonly IDictionary<sbyte?, SKPaint> strokes;

		private TextKey textKey;
		public const float DEFAULT_GAP = 5f;

		internal string symbolId;

		public Caption(GraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader, IDictionary<string, Symbol> symbols) : base(graphicFactory, displayModel)
		{
			this.fill = new SKPaint();
			this.fill.Color = SKColors.Black;
			this.fill.IsStroke = false;
			this.fills = new Dictionary<sbyte?, SKPaint>();

			this.stroke = new SKPaint();
			this.stroke.Color = SKColors.Black;
			this.stroke.IsStroke = true;
			this.strokes = new Dictionary<sbyte?, SKPaint>();
			this.dyScaled = new Dictionary<sbyte?, float?>();


			this.display = Display.IFSPACE;

			this.gap = DEFAULT_GAP * displayModel.ScaleFactor;

			ExtractValues(graphicFactory, displayModel, elementName, reader);

			if (!string.ReferenceEquals(this.symbolId, null))
			{
				Symbol symbol = symbols[this.symbolId];
				if (symbol != null)
				{
					this.bitmap = symbol.Bitmap;
				}
			}

            if (this.position == null)
            {
                // sensible defaults: below if symbolContainer is present, center if not
                if (this.bitmap == null)
                {
                    this.position = Position.CENTER;
                }
                else
                {
                    this.position = Position.BELOW;
                }
            }
            else if (this.position == Position.CENTER || this.position == Position.BELOW || this.position == Position.ABOVE)
            {
                this.stroke.TextAlign = SKTextAlign.Center;
                this.fill.TextAlign = SKTextAlign.Center;
            }
            else if (this.position == Position.BELOW_LEFT || this.position == Position.ABOVE_LEFT || this.position == Position.LEFT)
            {
                this.stroke.TextAlign = SKTextAlign.Right;
                this.fill.TextAlign = SKTextAlign.Right;
            }
            else if (this.position == Position.BELOW_RIGHT || this.position == Position.ABOVE_RIGHT || this.position == Position.RIGHT)
            {
                this.stroke.TextAlign = SKTextAlign.Left;
                this.fill.TextAlign = SKTextAlign.Left;
            }
            else {
                throw new System.ArgumentException("Position invalid");
            }

			this.maxTextWidth = displayModel.MaxTextWidth;
		}

		public override void Destroy()
		{
			// no-op
		}

		public override void RenderNode(RenderCallback renderCallback, RenderContext renderContext, PointOfInterest poi)
		{

			if (Display.NEVER == this.display)
			{
				return;
			}

			string caption = this.textKey.GetValue(poi.Tags);
			if (string.ReferenceEquals(caption, null))
			{
				return;
			}

			float horizontalOffset = 0f;

			float verticalOffset = this.dyScaled[renderContext.rendererJob.tile.ZoomLevel] ?? this.dy;

			if (this.bitmap != null)
			{
				horizontalOffset = ComputeHorizontalOffset();
				verticalOffset = ComputeVerticalOffset(renderContext.rendererJob.tile.ZoomLevel);
			}

			renderCallback.RenderPointOfInterestCaption(renderContext, this.display, this.priority, caption, horizontalOffset, verticalOffset, getFillPaint(renderContext.rendererJob.tile.ZoomLevel), getStrokePaint(renderContext.rendererJob.tile.ZoomLevel), this.position, this.maxTextWidth, poi);
		}

		public override void RenderWay(RenderCallback renderCallback, RenderContext renderContext, PolylineContainer way)
		{

			if (Display.NEVER == this.display)
			{
				return;
			}

			string caption = this.textKey.GetValue(way.Tags);
			if (string.ReferenceEquals(caption, null))
			{
				return;
			}

			float horizontalOffset = 0f;
            float verticalOffset = this.dyScaled[renderContext.rendererJob.tile.ZoomLevel] ?? this.dy;

			if (this.bitmap != null)
			{
				horizontalOffset = ComputeHorizontalOffset();
				verticalOffset = ComputeVerticalOffset(renderContext.rendererJob.tile.ZoomLevel);
			}

			renderCallback.RenderAreaCaption(renderContext, this.display, this.priority, caption, horizontalOffset, verticalOffset, getFillPaint(renderContext.rendererJob.tile.ZoomLevel), getStrokePaint(renderContext.rendererJob.tile.ZoomLevel), this.position, this.maxTextWidth, way);
		}

		public override void ScaleStrokeWidth(float scaleFactor, sbyte zoomLevel)
		{
			// do nothing
		}

		public override void ScaleTextSize(float scaleFactor, sbyte zoomLevel)
		{
			SKPaint f = graphicFactory.CreatePaint(this.fill);
			f.TextSize = this.fontSize * scaleFactor;
			this.fills[zoomLevel] = f;

			SKPaint s = graphicFactory.CreatePaint(this.stroke);
			s.TextSize = this.fontSize * scaleFactor;
			this.strokes[zoomLevel] = s;

			this.dyScaled[zoomLevel] = this.dy * scaleFactor;
		}

		private float ComputeHorizontalOffset()
		{
			// compute only the offset required by the bitmap, not the text size,
			// because at this point we do not know the text boxing
			if (Position.RIGHT == this.position || Position.LEFT == this.position || Position.BELOW_RIGHT == this.position || Position.BELOW_LEFT == this.position || Position.ABOVE_RIGHT == this.position || Position.ABOVE_LEFT == this.position)
			{
				float horizontalOffset = this.bitmap.Width / 2f + this.gap;
				if (Position.LEFT == this.position || Position.BELOW_LEFT == this.position || Position.ABOVE_LEFT == this.position)
				{
					horizontalOffset *= -1f;
				}
				return horizontalOffset;
			}
			return 0;
		}

		private float ComputeVerticalOffset(sbyte zoomLevel)
		{
			float verticalOffset = this.dyScaled[zoomLevel].Value;

			if (Position.ABOVE == this.position || Position.ABOVE_LEFT == this.position || Position.ABOVE_RIGHT == this.position)
			{
				verticalOffset -= this.bitmap.Height / 2f + this.gap;
			}
			else if (Position.BELOW == this.position || Position.BELOW_LEFT == this.position || Position.BELOW_RIGHT == this.position)
			{
				verticalOffset += this.bitmap.Height / 2f + this.gap;
			}
			return verticalOffset;
		}

        private void ExtractValues(GraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader)
		{
			string fontFamily = null;
			SKTypefaceStyle fontStyle = SKTypefaceStyle.Normal;

			for (int i = 0; i < reader.AttributeCount; ++i)
			{
                reader.MoveToAttribute(i);

				string name = reader.Name;
				string value = reader.Value;

				if (K.Equals(name))
				{
					this.textKey = TextKey.getInstance(value);
				}
				else if (POSITION.Equals(name))
				{
					this.position = Position.FromString(value);
				}
				else if (CAT.Equals(name))
				{
					this.category = value;
				}
				else if (DISPLAY.Equals(name))
				{
					this.display = Display.FromString(value);
				}
				else if (DY.Equals(name))
				{
					this.dy = float.Parse(value) * displayModel.ScaleFactor;
				}
				else if (FONT_FAMILY.Equals(name))
				{
					fontFamily = value;
				}
				else if (FONT_STYLE.Equals(name))
				{
                    if (!Enum.TryParse<SKTypefaceStyle>(value, true, out fontStyle))
                    {
                        throw new System.ArgumentException("Invalid value for FontStyle: " + value);
                    }
				}
				else if (FONT_SIZE.Equals(name))
				{
					this.fontSize = XmlUtils.ParseNonNegativeFloat(name, value) * displayModel.ScaleFactor;
				}
				else if (FILL.Equals(name))
				{
					this.fill.Color = XmlUtils.GetColor(value);
				}
				else if (PRIORITY.Equals(name))
				{
					this.priority = int.Parse(value);
				}
				else if (STROKE.Equals(name))
				{
					this.stroke.Color = XmlUtils.GetColor(value);
				}
				else if (STROKE_WIDTH.Equals(name))
				{
					this.stroke.StrokeWidth = XmlUtils.ParseNonNegativeFloat(name, value) * displayModel.ScaleFactor;
				}
				else if (SYMBOL_ID.Equals(name))
				{
					this.symbolId = value;
				}
				else
				{
					throw XmlUtils.CreateXmlReaderException(elementName, name, value, i);
				}
			}

            this.fill.Typeface = SKTypeface.FromFamilyName(fontFamily, fontStyle);
			this.stroke.Typeface = SKTypeface.FromFamilyName(fontFamily, fontStyle);

            XmlUtils.CheckMandatoryAttribute(elementName, K, this.textKey);
		}

		private SKPaint getFillPaint(sbyte zoomLevel)
		{
			SKPaint paint = fills[zoomLevel];
			if (paint == null)
			{
				paint = this.fill;
			}
			return paint;
		}

		private SKPaint getStrokePaint(sbyte zoomLevel)
		{
			SKPaint paint = strokes[zoomLevel];
			if (paint == null)
			{
				paint = this.stroke;
			}
			return paint;
		}
	}
}