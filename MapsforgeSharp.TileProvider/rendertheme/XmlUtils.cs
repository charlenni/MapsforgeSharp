/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.map.rendertheme
{
    using System;
    using System.Text;
    using System.IO;
    using System.Reflection;
    using PCLStorage;
    using SkiaSharp;
    using MapsforgeSharp.Core.Graphics;
  	using System.Globalization;

    using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
    using IResourceBitmap = MapsforgeSharp.Core.Graphics.IResourceBitmap;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;

    public sealed class XmlUtils
	 {
		public static bool supportOlderRenderThemes = true;
		private const string PREFIX_FILE = "file:";
		private const string PREFIX_JAR = "jar:";

		private const string PREFIX_JAR_V1 = "jar:/org/mapsforge/android/maps/rendertheme";

		private const string UNSUPPORTED_COLOR_FORMAT = "unsupported color format: ";

		public static void CheckMandatoryAttribute(string elementName, string attributeName, object attributeValue)
		{
			if (attributeValue == null)
			{
				throw new XmlReaderException("missing attribute '" + attributeName + "' for element: " + elementName);
			}
		}

		public static IResourceBitmap CreateBitmap(IGraphicFactory graphicFactory, DisplayModel displayModel, string relativePathPrefix, string src, int width, int height, int percent)
		{
			if (string.ReferenceEquals(src, null) || src.Length == 0)
			{
				// no image source defined
				return null;
			}

			System.IO.Stream inputStream = graphicFactory.PlatformSpecificSources(relativePathPrefix, src);

			if (inputStream == null)
			{
				inputStream = CreateInputStream(relativePathPrefix, src);
			}

			try
			{
				string absoluteName = GetAbsoluteName(relativePathPrefix, src);
				// we need to hash with the width/height included as the same symbol could be required
				// in a different size and must be cached with a size-specific hash
				int hash = (new StringBuilder()).Append(absoluteName).Append(width).Append(height).Append(percent).ToString().GetHashCode();

				if (src.EndsWith(".svg", StringComparison.Ordinal))
				{
					try
					{
						return graphicFactory.RenderSvg(inputStream, displayModel.ScaleFactor, width, height, percent, hash);
					}
					catch (IOException e)
					{
						throw new IOException("SVG render failed " + src, e);
					}
				}
				try
				{
					return graphicFactory.CreateResourceBitmap(inputStream, absoluteName.GetHashCode());
				}
				catch (IOException e)
				{
					throw new IOException("Reading bitmap file failed " + src, e);
				}
			}
			finally
			{
				inputStream = null;
			}
		}

		public static XmlReaderException CreateXmlReaderException(string element, string name, string value, int attributeIndex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("unknown attribute (");
			stringBuilder.Append(attributeIndex);
			stringBuilder.Append(") in element '");
			stringBuilder.Append(element);
			stringBuilder.Append("': ");
			stringBuilder.Append(name);
			stringBuilder.Append('=');
			stringBuilder.Append(value);

			return new XmlReaderException(stringBuilder.ToString());
		}

		/// <summary>
		/// Supported formats are {@code #RRGGBB} and {@code #AARRGGBB}.
		/// </summary>
		public static int GetColor(string colorString)
		{
            int argb = 0;

			if (colorString.Length == 0 || colorString[0] != '#')
			{
				throw new System.ArgumentException(UNSUPPORTED_COLOR_FORMAT + colorString);
			}
			else if (colorString.Length == 7)
			{
                var rgb = int.Parse(colorString.Substring(1), System.Globalization.NumberStyles.HexNumber);
                argb = 255 << 24 | rgb;
			}
			else if (colorString.Length == 9)
			{
                argb = int.Parse(colorString.Substring(1), System.Globalization.NumberStyles.HexNumber);
            }
            else
			{
				throw new System.ArgumentException(UNSUPPORTED_COLOR_FORMAT + colorString);
			}

            return argb;
		}

        public static sbyte ParseNonNegativeByte(string name, string value)
		{
			sbyte parsedByte = sbyte.Parse(value);
			CheckForNegativeValue(name, parsedByte);
			return parsedByte;
		}

		public static float ParseNonNegativeFloat(string name, string value)
		{
			float parsedFloat = float.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
			CheckForNegativeValue(name, parsedFloat);
			return parsedFloat;
		}

		public static int ParseNonNegativeInteger(string name, string value)
		{
			int parsedInt = int.Parse(value);
			CheckForNegativeValue(name, parsedInt);
			return parsedInt;
		}

		private static void CheckForNegativeValue(string name, float value)
		{
			if (value < 0)
			{
				throw new XmlReaderException("Attribute '" + name + "' must not be negative: " + value);
			}
		}

		private static System.IO.Stream CreateInputStream(string relativePathPrefix, string src)
		{
			if (src.StartsWith(PREFIX_JAR, StringComparison.Ordinal))
			{
				string prefixJar;
				if (!supportOlderRenderThemes)
				{
					prefixJar = PREFIX_JAR;
				}
				else
				{
					prefixJar = src.StartsWith(PREFIX_JAR_V1, StringComparison.Ordinal) ? PREFIX_JAR_V1 : PREFIX_JAR;
				}
				string absoluteName = GetAbsoluteName(relativePathPrefix, src.Substring(prefixJar.Length));
				System.IO.Stream inputStream = typeof(XmlUtils).GetTypeInfo().Assembly.GetManifestResourceStream(absoluteName);
				if (inputStream == null)
				{
					throw new FileNotFoundException("resource not found: " + absoluteName);
				}
				return inputStream;
			}
			else if (src.StartsWith(PREFIX_FILE, StringComparison.Ordinal))
			{
				IFile file = GetFile(relativePathPrefix, src.Substring(PREFIX_FILE.Length));
				if (file == null)
				{
					string pathName = src.Substring(PREFIX_FILE.Length);
					if (pathName.Length > 0 && pathName[0] == PortablePath.DirectorySeparatorChar)
					{
						file = GetFile(relativePathPrefix, pathName.Substring(1));
					}
					if (file == null)
					{
						throw new FileNotFoundException("file does not exist: " + file.Path);
					}
				}
                return file.OpenAsync(FileAccess.Read).Result;
			}

			throw new FileNotFoundException("invalid bitmap source: " + src);
		}

		private static string GetAbsoluteName(string relativePathPrefix, string name)
		{
			if (name[0] == PortablePath.DirectorySeparatorChar)
			{
				return name;
			}
			return relativePathPrefix + name;
		}

		private static IFile GetFile(string parentPath, string pathName)
		{
			if (pathName[0] == PortablePath.DirectorySeparatorChar)
			{
				return FileSystem.Current.GetFileFromPathAsync(pathName).Result;
			}
			return FileSystem.Current.GetFileFromPathAsync(PortablePath.Combine(parentPath, pathName)).Result;
		}

		private XmlUtils()
		{
			throw new System.InvalidOperationException();
		}
	}
}