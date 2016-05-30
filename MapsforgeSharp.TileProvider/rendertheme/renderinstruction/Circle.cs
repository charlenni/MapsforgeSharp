/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
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
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using MapsforgeSharp.Core.Graphics;

    using Color = MapsforgeSharp.Core.Graphics.Color;
    using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
    using IPaint = MapsforgeSharp.Core.Graphics.IPaint;
    using Style = MapsforgeSharp.Core.Graphics.Style;
    using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using PointOfInterest = MapsforgeSharp.Core.Datastore.PointOfInterest;

    /// <summary>
    /// Represents a round area on the map.
    /// </summary>
    public class Circle : RenderInstruction
	{
		private readonly IPaint fill;
		private readonly IDictionary<sbyte?, IPaint> fills;
		private readonly int level;
		private float radius;
		private float renderRadius;
		private readonly IDictionary<sbyte?, float?> renderRadiusScaled;
		private bool scaleRadius;
		private readonly IPaint stroke;
		private readonly IDictionary<sbyte?, IPaint> strokes;
		private float strokeWidth;

		public Circle(IGraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader, int level) : base(graphicFactory, displayModel)
		{
			this.level = level;

			this.fill = graphicFactory.CreatePaint();
			this.fill.Color = Color.Transparent.ToARGB();
			this.fill.Style = Style.Fill;
			this.fills = new Dictionary<sbyte?, IPaint>();

			this.stroke = graphicFactory.CreatePaint();
			this.stroke.Color = Color.Transparent.ToARGB();
			this.stroke.Style = Style.Stroke;
			this.strokes = new Dictionary<sbyte?, IPaint>();
			this.renderRadiusScaled = new Dictionary<sbyte?, float?>();

			ExtractValues(graphicFactory, displayModel, elementName, reader);


			if (!this.scaleRadius)
			{
				this.renderRadius = this.radius;
				this.stroke.StrokeWidth = this.strokeWidth;
			}
		}

		public override void Destroy()
		{
			// no-op
		}

		public override void RenderNode(RenderCallback renderCallback, RenderContext renderContext, PointOfInterest poi)
		{
			renderCallback.RenderPointOfInterestCircle(renderContext, GetRenderRadius(renderContext.rendererJob.tile.ZoomLevel), GetFillPaint(renderContext.rendererJob.tile.ZoomLevel), GetStrokePaint(renderContext.rendererJob.tile.ZoomLevel), this.level, poi);
		}

		public override void RenderWay(RenderCallback renderCallback, RenderContext renderContext, PolylineContainer way)
		{
			// do nothing
		}

		public override void ScaleStrokeWidth(float scaleFactor, sbyte zoomLevel)
		{
			if (this.scaleRadius)
			{
				this.renderRadiusScaled[zoomLevel] = this.radius * scaleFactor;
				if (this.stroke != null)
				{
					IPaint s = graphicFactory.CreatePaint(stroke);
					s.StrokeWidth = this.strokeWidth * scaleFactor;
					strokes[zoomLevel] = s;
				}
			}
		}

		public override void ScaleTextSize(float scaleFactor, sbyte zoomLevel)
		{
			// do nothing
		}

		private void ExtractValues(IGraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
                reader.MoveToAttribute(i);

				string name = reader.Name;
				string value = reader.Value;

				if (RADIUS.Equals(name) || (XmlUtils.supportOlderRenderThemes && R.Equals(name)))
				{
					this.radius = Convert.ToSingle(XmlUtils.ParseNonNegativeFloat(name, value)) * displayModel.ScaleFactor;
				}
				else if (SCALE_RADIUS.Equals(name))
				{
					this.scaleRadius = bool.Parse(value);
				}
				else if (CAT.Equals(name))
				{
					this.category = value;
				}
				else if (FILL.Equals(name))
				{
					this.fill.Color = XmlUtils.GetColor(value);
				}
				else if (STROKE.Equals(name))
				{
					this.stroke.Color = XmlUtils.GetColor(value);
				}
				else if (STROKE_WIDTH.Equals(name))
				{
					this.strokeWidth = XmlUtils.ParseNonNegativeFloat(name, value) * displayModel.ScaleFactor;
				}
				else
				{
					throw XmlUtils.CreateXmlReaderException(elementName, name, value, i);
				}
			}

			XmlUtils.CheckMandatoryAttribute(elementName, RADIUS, this.radius);
		}

		private IPaint GetFillPaint(sbyte zoomLevel)
		{
			if (fills.Count == 0)
			{
				return this.fill;
			}

			return fills[zoomLevel] ?? this.fill;
		}

		private IPaint GetStrokePaint(sbyte zoomLevel)
		{
			if (strokes.Count == 0)
			{
				return this.stroke;
			}

			return strokes[zoomLevel] ?? this.stroke;
		}

		private float GetRenderRadius(sbyte zoomLevel)
		{
			if (renderRadiusScaled.Count == 0)
			{
				return this.renderRadius;
			}

			return renderRadiusScaled[zoomLevel] ?? this.renderRadius;
		}
	}
}