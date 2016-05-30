/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
 * Copyright 2014 devemux86
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

namespace org.mapsforge.map.rendertheme.renderinstruction
{
    using System.Collections.Generic;
    using System.Xml;
    using MapsforgeSharp.Core.Graphics;

    using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
    using Color = MapsforgeSharp.Core.Graphics.Color;
    using Display = MapsforgeSharp.Core.Graphics.Display;
    using FontFamily = MapsforgeSharp.Core.Graphics.FontFamily;
    using FontStyle = MapsforgeSharp.Core.Graphics.FontStyle;
    using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
    using IPaint = MapsforgeSharp.Core.Graphics.IPaint;
    using Style = MapsforgeSharp.Core.Graphics.Style;
    using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using PointOfInterest = MapsforgeSharp.Core.Datastore.PointOfInterest;

    /// <summary>
    /// Represents a text label on the map.
    /// 
    /// If a bitmap symbol is present the caption position is calculated relative to the bitmap, the
    /// center of which is at the point of the POI. The bitmap itself is never rendered.
    /// 
    /// </summary>
    public class Caption : RenderInstruction
	{

		private IBitmap bitmap;
		private Position position;
		private Display display;
		private float dy;
		private readonly IDictionary<sbyte?, float?> dyScaled;

		private readonly IPaint fill;
		private readonly IDictionary<sbyte?, IPaint> fills;

		private float fontSize;
		private readonly float gap;
		private readonly int maxTextWidth;
		private int priority;
		private readonly IPaint stroke;
		private readonly IDictionary<sbyte?, IPaint> strokes;

		private TextKey textKey;
		public const float DEFAULT_GAP = 5f;

		internal string symbolId;

		public Caption(IGraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader, IDictionary<string, Symbol> symbols) : base(graphicFactory, displayModel)
		{
			this.fill = graphicFactory.CreatePaint();
			this.fill.Color = Color.Black.ToARGB();
			this.fill.Style = Style.Fill;
			this.fills = new Dictionary<sbyte?, IPaint>();

			this.stroke = graphicFactory.CreatePaint();
			this.stroke.Color = Color.Black.ToARGB();
			this.stroke.Style = Style.Stroke;
			this.strokes = new Dictionary<sbyte?, IPaint>();
			this.dyScaled = new Dictionary<sbyte?, float?>();


			this.display = Display.Ifspace;

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

			if (Display.Never == this.display)
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

			if (Display.Never == this.display)
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
			IPaint f = graphicFactory.CreatePaint(this.fill);
			f.TextSize = this.fontSize * scaleFactor;
			this.fills[zoomLevel] = f;

			IPaint s = graphicFactory.CreatePaint(this.stroke);
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

        private void ExtractValues(IGraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader)
		{
			FontFamily fontFamily = FontFamily.Default;
			FontStyle fontStyle = FontStyle.Normal;

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
					this.display = value.ToDisplay();
				}
				else if (DY.Equals(name))
				{
					this.dy = float.Parse(value) * displayModel.ScaleFactor;
				}
				else if (FONT_FAMILY.Equals(name))
				{
					fontFamily = value.ToFontFamily();
				}
				else if (FONT_STYLE.Equals(name))
				{
					fontStyle = value.ToFontStyle();
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

		private IPaint getFillPaint(sbyte zoomLevel)
		{
			IPaint paint = fills[zoomLevel];
			if (paint == null)
			{
				paint = this.fill;
			}
			return paint;
		}

		private IPaint getStrokePaint(sbyte zoomLevel)
		{
			IPaint paint = strokes[zoomLevel];
			if (paint == null)
			{
				paint = this.stroke;
			}
			return paint;
		}
	}
}