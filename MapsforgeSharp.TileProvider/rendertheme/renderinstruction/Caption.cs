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
    using core.graphics;
    using MapsforgeSharp.Core.Graphics;

    using Bitmap = org.mapsforge.core.graphics.Bitmap;
    using Color = org.mapsforge.core.graphics.Color;
    using Display = org.mapsforge.core.graphics.Display;
    using FontFamily = org.mapsforge.core.graphics.FontFamily;
    using FontStyle = org.mapsforge.core.graphics.FontStyle;
    using GraphicFactory = org.mapsforge.core.graphics.GraphicFactory;
    using Paint = org.mapsforge.core.graphics.Paint;
    using Style = org.mapsforge.core.graphics.Style;
    using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using PointOfInterest = org.mapsforge.core.datastore.PointOfInterest;

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

		private readonly Paint fill;
		private readonly IDictionary<sbyte?, Paint> fills;

		private float fontSize;
		private readonly float gap;
		private readonly int maxTextWidth;
		private int priority;
		private readonly Paint stroke;
		private readonly IDictionary<sbyte?, Paint> strokes;

		private TextKey textKey;
		public const float DEFAULT_GAP = 5f;

		internal string symbolId;

		public Caption(GraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader, IDictionary<string, Symbol> symbols) : base(graphicFactory, displayModel)
		{
			this.fill = graphicFactory.CreatePaint();
			this.fill.Color = Color.BLACK.ToARGB();
			this.fill.Style = Style.FILL;
			this.fills = new Dictionary<sbyte?, Paint>();

			this.stroke = graphicFactory.CreatePaint();
			this.stroke.Color = Color.BLACK.ToARGB();
			this.stroke.Style = Style.STROKE;
			this.strokes = new Dictionary<sbyte?, Paint>();
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

            if (this.position == Position.Auto)
            {
                // sensible defaults: below if symbolContainer is present, center if not
                if (this.bitmap == null)
                {
                    this.position = Position.Center;
                }
                else
                {
                    this.position = Position.Below;
                }
            }
            else if (this.position == Position.Center || this.position == Position.Below || this.position == Position.Above)
            {
                this.stroke.TextAlign = Align.Center;
                this.fill.TextAlign = Align.Center;
            }
            else if (this.position == Position.BelowLeft || this.position == Position.AboveLeft || this.position == Position.Left)
            {
                this.stroke.TextAlign = Align.Right;
                this.fill.TextAlign = Align.Right;
            }
            else if (this.position == Position.BelowRight || this.position == Position.AboveRight || this.position == Position.Right)
            {
                this.stroke.TextAlign = Align.Left;
                this.fill.TextAlign = Align.Left;
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
			Paint f = graphicFactory.CreatePaint(this.fill);
			f.TextSize = this.fontSize * scaleFactor;
			this.fills[zoomLevel] = f;

			Paint s = graphicFactory.CreatePaint(this.stroke);
			s.TextSize = this.fontSize * scaleFactor;
			this.strokes[zoomLevel] = s;

			this.dyScaled[zoomLevel] = this.dy * scaleFactor;
		}

		private float ComputeHorizontalOffset()
		{
			// compute only the offset required by the bitmap, not the text size,
			// because at this point we do not know the text boxing
			if (Position.Right == this.position || Position.Left == this.position || Position.BelowRight == this.position || Position.BelowLeft == this.position || Position.AboveRight == this.position || Position.AboveLeft == this.position)
			{
				float horizontalOffset = this.bitmap.Width / 2f + this.gap;
				if (Position.Left == this.position || Position.BelowLeft == this.position || Position.AboveLeft == this.position)
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

			if (Position.Above == this.position || Position.AboveLeft == this.position || Position.AboveRight == this.position)
			{
				verticalOffset -= this.bitmap.Height / 2f + this.gap;
			}
			else if (Position.Below == this.position || Position.BelowLeft == this.position || Position.BelowRight == this.position)
			{
				verticalOffset += this.bitmap.Height / 2f + this.gap;
			}
			return verticalOffset;
		}

        private void ExtractValues(GraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader)
		{
			FontFamily fontFamily = FontFamily.DEFAULT;
			FontStyle fontStyle = FontStyle.NORMAL;

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
					this.position = value.ToPosition();
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
					fontFamily = FontFamily.FromString(value);
				}
				else if (FONT_STYLE.Equals(name))
				{
					fontStyle = FontStyle.FromString(value);
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

			this.fill.SetTypeface(fontFamily, fontStyle);
			this.stroke.SetTypeface(fontFamily, fontStyle);

			XmlUtils.CheckMandatoryAttribute(elementName, K, this.textKey);
		}

		private Paint getFillPaint(sbyte zoomLevel)
		{
			Paint paint = fills[zoomLevel];
			if (paint == null)
			{
				paint = this.fill;
			}
			return paint;
		}

		private Paint getStrokePaint(sbyte zoomLevel)
		{
			Paint paint = strokes[zoomLevel];
			if (paint == null)
			{
				paint = this.stroke;
			}
			return paint;
		}
	}
}