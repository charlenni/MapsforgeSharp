/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
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

    using Align = org.mapsforge.core.graphics.Align;
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
    /// Represents a text along a polyline on the map.
    /// </summary>
    public class PathText : RenderInstruction
	{
		private Display display;
		private float dy;
		private readonly IDictionary<sbyte?, float?> dyScaled;
		private readonly Paint fill;
		private readonly IDictionary<sbyte?, Paint> fills;
		private float fontSize;
		private int priority;
		private readonly Paint stroke;
		private readonly IDictionary<sbyte?, Paint> strokes;

		private TextKey textKey;

		public PathText(GraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader) : base(graphicFactory, displayModel)
		{
			this.fill = graphicFactory.CreatePaint();
			this.fill.Color = Color.BLACK.ToARGB();
			this.fill.Style = Style.FILL;
			this.fill.TextAlign = Align.CENTER;
			this.fills = new Dictionary<sbyte?, Paint>();

			this.stroke = graphicFactory.CreatePaint();
			this.stroke.Color = Color.BLACK.ToARGB();
			this.stroke.Style = Style.STROKE;
			this.stroke.TextAlign = Align.CENTER;
			this.strokes = new Dictionary<sbyte?, Paint>();
			this.dyScaled = new Dictionary<sbyte?, float?>();
			this.display = Display.IFSPACE;

			ExtractValues(graphicFactory, displayModel, elementName, reader);
		}

		public override void Destroy()
		{
			// no-op
		}

		public override void RenderNode(RenderCallback renderCallback, RenderContext renderContext, PointOfInterest poi)
		{
			// do nothing
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

			float dyScale = this.dyScaled[renderContext.rendererJob.tile.ZoomLevel] ?? this.dy;

			renderCallback.RenderWayText(renderContext, this.display, this.priority, caption, dyScale, GetFillPaint(renderContext.rendererJob.tile.ZoomLevel), getStrokePaint(renderContext.rendererJob.tile.ZoomLevel), way);
		}

		public override void ScaleStrokeWidth(float scaleFactor, sbyte zoomLevel)
		{
			this.dyScaled[zoomLevel] = this.dy * scaleFactor;
		}

		public override void ScaleTextSize(float scaleFactor, sbyte zoomLevel)
		{
			Paint zlPaint = graphicFactory.CreatePaint(this.fill);
			zlPaint.TextSize = this.fontSize * scaleFactor;
			this.fills[zoomLevel] = zlPaint;

			Paint zlStroke = graphicFactory.CreatePaint(this.stroke);
			zlStroke.TextSize = this.fontSize * scaleFactor;
			this.strokes[zoomLevel] = zlStroke;

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
				else
				{
					throw XmlUtils.CreateXmlReaderException(elementName, name, value, i);
				}
			}

			this.fill.SetTypeface(fontFamily, fontStyle);
			this.stroke.SetTypeface(fontFamily, fontStyle);

			XmlUtils.CheckMandatoryAttribute(elementName, K, this.textKey);
		}

		private Paint GetFillPaint(sbyte zoomLevel)
		{
			return fills[zoomLevel] ?? this.fill;
		}

		private Paint getStrokePaint(sbyte zoomLevel)
		{
			return strokes[zoomLevel] ?? this.stroke;
		}
	}
}