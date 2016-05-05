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

namespace org.mapsforge.map.rendertheme.rule
{
    using MapsforgeSharp.Core.Graphics;
    using System;
    using System.Xml;

    using Color = MapsforgeSharp.Core.Graphics.Color;
    using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;

    /// <summary>
    /// A builder for <seealso cref="RenderTheme"/> instances.
    /// </summary>
    public class RenderThemeBuilder
	{

		private const string BASE_STROKE_WIDTH = "base-stroke-width";
		private const string BASE_TEXT_SIZE = "base-text-size";
		private const string MAP_BACKGROUND = "map-background";
		private const string MAP_BACKGROUND_OUTSIDE = "map-background-outside";
		private const int RENDER_THEME_VERSION = 4;
		private const string VERSION = "version";
		private const string XMLNS = "xmlns";
		private const string XMLNS_XSI = "xmlns:xsi";
		private const string XSI_SCHEMALOCATION = "xsi:schemaLocation";

		internal float baseStrokeWidth;
		internal float baseTextSize;
		internal bool hasBackgroundOutside;
		internal int mapBackground;
		internal int mapBackgroundOutside;
		private int? version;

		public RenderThemeBuilder(GraphicFactory graphicFactory, string elementName, XmlReader reader)
		{
			this.baseStrokeWidth = 1f;
			this.baseTextSize = 1f;
			this.mapBackground = graphicFactory.CreateColor(Color.WHITE.ToARGB());

			ExtractValues(graphicFactory, elementName, reader);
		}

		/// <returns> a new {@code RenderTheme} instance. </returns>
		public virtual RenderTheme build()
		{
			return new RenderTheme(this);
		}

		private void ExtractValues(GraphicFactory graphicFactory, string elementName, XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
                reader.MoveToAttribute(i);

				string name = reader.Name;
				string value = reader.Value;

				if (XMLNS.Equals(name))
				{
					continue;
				}
				else if (XMLNS_XSI.Equals(name))
				{
					continue;
				}
				else if (XSI_SCHEMALOCATION.Equals(name))
				{
					continue;
				}
				else if (VERSION.Equals(name))
				{
					this.version = Convert.ToInt32(XmlUtils.ParseNonNegativeInteger(name, value));
				}
				else if (MAP_BACKGROUND.Equals(name))
				{
					this.mapBackground = XmlUtils.GetColor(graphicFactory, value);
				}
				else if (MAP_BACKGROUND_OUTSIDE.Equals(name))
				{
					this.mapBackgroundOutside = XmlUtils.GetColor(graphicFactory, value);
					this.hasBackgroundOutside = true;
				}
				else if (BASE_STROKE_WIDTH.Equals(name))
				{
					this.baseStrokeWidth = XmlUtils.ParseNonNegativeFloat(name, value);
				}
				else if (BASE_TEXT_SIZE.Equals(name))
				{
					this.baseTextSize = XmlUtils.ParseNonNegativeFloat(name, value);
				}
				else
				{
					throw XmlUtils.CreateXmlReaderException(elementName, name, value, i);
				}
			}

			Validate(elementName);
		}

		private void Validate(string elementName)
		{
			XmlUtils.CheckMandatoryAttribute(elementName, VERSION, this.version);

			if (!XmlUtils.supportOlderRenderThemes && this.version != RENDER_THEME_VERSION)
			{
				throw new InvalidOperationException("unsupported render theme version: " + this.version);
			}
			else if (this.version > RENDER_THEME_VERSION)
			{
				throw new InvalidOperationException("unsupported newer render theme version: " + this.version);
			}
		}
	}
}