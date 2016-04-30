/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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
	/// <summary>
	/// Interface for a render theme which is defined in XML.
	/// </summary>
	public interface IXmlRenderTheme
	{
		XmlRenderThemeMenuCallback MenuCallback {get;}

		/// <returns> the prefix for all relative resource paths. </returns>
		string RelativePathPrefix {get;}

		/// <returns> an InputStream to read the render theme data from. </returns>
		/// <exception cref="FileNotFoundException">
		///             if the render theme file cannot be found. </exception>
		System.IO.Stream RenderThemeAsStream {get;}
	}
}