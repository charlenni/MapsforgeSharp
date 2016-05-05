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
    using System.IO;

    using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
    using Display = MapsforgeSharp.Core.Graphics.Display;
    using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
    using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using PointOfInterest = org.mapsforge.core.datastore.PointOfInterest;

    /// <summary>
    /// Represents an icon along a polyline on the map.
    /// </summary>
    public class LineSymbol : RenderInstruction
	{
		private const float REPEAT_GAP_DEFAULT = 200f;
		private const float REPEAT_START_DEFAULT = 30f;

		private bool alignCenter;
		private IBitmap bitmap;
		private bool bitmapInvalid;
		private Display display;
		private float dy;
		private readonly IDictionary<sbyte?, float?> dyScaled;
		private int priority;
		private readonly string relativePathPrefix;
		private bool repeat;
		private float repeatGap;
		private float repeatStart;
		private bool rotate;
		private string src;

		public LineSymbol(GraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader, string relativePathPrefix) : base(graphicFactory, displayModel)
		{
			this.display = Display.IFSPACE;
			this.rotate = true;
			this.relativePathPrefix = relativePathPrefix;
			this.dyScaled = new Dictionary<sbyte?, float?>();

			ExtractValues(elementName, reader);
		}

		public override void Destroy()
		{
			if (this.bitmap != null)
			{
				this.bitmap.DecrementRefCount();
			}
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

			if (this.bitmap == null && !this.bitmapInvalid)
			{
				try
				{
					this.bitmap = CreateBitmap(relativePathPrefix, src);
				}
				catch (IOException)
				{
					this.bitmapInvalid = true;
				}
			}

			float dyScale = this.dyScaled[renderContext.rendererJob.tile.ZoomLevel] ?? this.dy;

			if (this.bitmap != null)
			{
				renderCallback.RenderWaySymbol(renderContext, this.display, this.priority, this.bitmap, dyScale, this.alignCenter, this.repeat, this.repeatGap, this.repeatStart, this.rotate, way);
			}
		}

		public override void ScaleStrokeWidth(float scaleFactor, sbyte zoomLevel)
		{
			this.dyScaled[zoomLevel] = this.dy * scaleFactor;
		}

		public override void ScaleTextSize(float scaleFactor, sbyte zoomLevel)
		{
			// do nothing
		}

		private void ExtractValues(string elementName, XmlReader reader)
		{
			this.repeatGap = REPEAT_GAP_DEFAULT * displayModel.ScaleFactor;
			this.repeatStart = REPEAT_START_DEFAULT * displayModel.ScaleFactor;

			for (int i = 0; i < reader.AttributeCount; ++i)
			{
                reader.MoveToAttribute(i);

				string name = reader.Name;
				string value = reader.Value;

				if (SRC.Equals(name))
				{
					this.src = value;
				}
				else if (DISPLAY.Equals(name))
				{
					this.display = Display.FromString(value);
				}
				else if (DY.Equals(name))
				{
					this.dy = float.Parse(value) * displayModel.ScaleFactor;
				}
				else if (ALIGN_CENTER.Equals(name))
				{
					this.alignCenter = bool.Parse(value);
				}
				else if (CAT.Equals(name))
				{
					this.category = value;
				}
				else if (PRIORITY.Equals(name))
				{
					this.priority = int.Parse(value);
				}
				else if (REPEAT.Equals(name))
				{
					this.repeat = bool.Parse(value);
				}
				else if (REPEAT_GAP.Equals(name))
				{
					this.repeatGap = float.Parse(value) * displayModel.ScaleFactor;
				}
				else if (REPEAT_START.Equals(name))
				{
					this.repeatStart = float.Parse(value) * displayModel.ScaleFactor;
				}
				else if (ROTATE.Equals(name))
				{
					this.rotate = bool.Parse(value);
				}
				else if (SYMBOL_HEIGHT.Equals(name))
				{
					this.height = XmlUtils.ParseNonNegativeInteger(name, value) * displayModel.ScaleFactor;
				}
				else if (SYMBOL_PERCENT.Equals(name))
				{
					this.percent = XmlUtils.ParseNonNegativeInteger(name, value);
				}
				else if (SYMBOL_SCALING.Equals(name))
				{
					this.scaling = FromValue(value);
				}
				else if (SYMBOL_WIDTH.Equals(name))
				{
					this.width = XmlUtils.ParseNonNegativeInteger(name, value) * displayModel.ScaleFactor;
				}
				else
				{
					throw XmlUtils.CreateXmlReaderException(elementName, name, value, i);
				}
			}
		}
	}
}