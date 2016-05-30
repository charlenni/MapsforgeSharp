/*
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

namespace MapsforgeSharp.Graphics
{
	using SkiaSharp;
	using MapsforgeSharp.Core.Graphics;
	
	public static class SkiaExtensions
    {
        /// <summary>
        /// Convert Mapsforge FontStyle to Skia SKTypefaceStyle
        /// </summary>
        /// <param name="style">Mapsforge FontStyle</param>
        /// <returns>Skia SKTypefaceStyle</returns>
        public static SKTypefaceStyle ToSkia(this FontStyle style)
        {
            SKTypefaceStyle result = SKTypefaceStyle.Normal;

            if (style == FontStyle.Bold)
            {
                result = SKTypefaceStyle.Bold;
            }
            if (style == FontStyle.Italic)
            {
                result = SKTypefaceStyle.Italic;
            }
            if (style == FontStyle.BoldItalic)
            {
                result = SKTypefaceStyle.BoldItalic;
            }

            return result;
        }

        /// <summary>
        /// Convert Mapsforge Align to Skia SKTextAlign
        /// </summary>
        /// <param name="align">Mapsforge Align</param>
        /// <returns>Skia SKTextAlign</returns>
        public static SKTextAlign ToSkia(this Align align)
        {
            SKTextAlign result = SKTextAlign.Center;

            if (align == Align.Left)
            {
                result = SKTextAlign.Left;
            }
            if (align == Align.Right)
            {
                result = SKTextAlign.Right;
            }

            return result;
        }
    }
}