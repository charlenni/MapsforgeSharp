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
    using System.Xml;
    using System.IO;

    using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
    using Display = MapsforgeSharp.Core.Graphics.Display;
    using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
    using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using PointOfInterest = MapsforgeSharp.Core.Datastore.PointOfInterest;

    /// <summary>
    /// Represents an icon on the map.
    /// </summary>
    public class Symbol : RenderInstruction
	{
		private IBitmap bitmap;
		private bool bitmapInvalid;
		private Display display;
		private string id;
		private int priority;
		private readonly string relativePathPrefix;
		private string src;

		public Symbol(GraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader, string relativePathPrefix) : base(graphicFactory, displayModel)
		{
			this.relativePathPrefix = relativePathPrefix;
			this.display = Display.IFSPACE;
			ExtractValues(elementName, reader);
		}

		public override void Destroy()
		{
			if (this.bitmap != null)
			{
				this.bitmap.DecrementRefCount();
			}
		}

		public virtual IBitmap Bitmap
		{
			get
			{
				if (this.bitmap == null && !bitmapInvalid)
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
				return this.bitmap;
			}
		}

		public virtual string Id
		{
			get
			{
				return this.id;
			}
		}

		public override void RenderNode(RenderCallback renderCallback, RenderContext renderContext, PointOfInterest poi)
		{
			if (Display.NEVER == this.display)
			{
				return;
			}

			if (Bitmap != null)
			{
				renderCallback.RenderPointOfInterestSymbol(renderContext, this.display, this.priority, this.bitmap, poi);
			}
		}

		public override void RenderWay(RenderCallback renderCallback, RenderContext renderContext, PolylineContainer way)
		{
			if (Display.NEVER == this.display)
			{
				return;
			}

			if (this.Bitmap != null)
			{
				renderCallback.RenderAreaSymbol(renderContext, this.display, this.priority, this.bitmap, way);
			}
		}

		public override void ScaleStrokeWidth(float scaleFactor, sbyte zoomLevel)
		{
			// do nothing
		}

		public override void ScaleTextSize(float scaleFactor, sbyte zoomLevel)
		{
			// do nothing
		}

        private void ExtractValues(string elementName, XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
                reader.MoveToAttribute(i);

				string name = reader.Name;
				string value = reader.Value;

				if (SRC.Equals(name))
				{
					this.src = value;
				}
				else if (CAT.Equals(name))
				{
					this.category = value;
				}
				else if (DISPLAY.Equals(name))
				{
					this.display = Display.FromString(value);
				}
				else if (ID.Equals(name))
				{
					this.id = value;
				}
				else if (PRIORITY.Equals(name))
				{
					this.priority = int.Parse(value);
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