/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

using System;

namespace MapsforgeSharp.Core.Graphics
{
    public enum FontStyle
    {
        Bold,
        BoldItalic,
        Italic,
        Normal
    }

    public static class FontStyleHelper
    {
        public static FontStyle ToFontStyle(this string value)
        {
            FontStyle ret;
            if (!Enum.TryParse<FontStyle>(value, true, out ret))
            {
                throw new ArgumentException("Invalid value for FontStyle: " + value);
            }
            return ret;
        }
    }    
}