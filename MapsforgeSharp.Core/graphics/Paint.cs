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

using MapsforgeSharp.Core.Graphics;

namespace org.mapsforge.core.graphics
{
    
    using Point = org.mapsforge.core.model.Point;

    public interface Paint
	{
		int GetTextHeight(string text);

		int GetTextWidth(string text);

		bool Transparent {get;}

		Bitmap BitmapShader {set;}
		Point BitmapShaderShift {set;}

        int Color { get; set; }

		float[] DashPathEffect {set;}

		/// <summary>
		/// The default value is <seealso cref="Cap#ROUND"/>.
		/// </summary>
		Cap StrokeCap {set;}

		Join StrokeJoin {set;}

		float StrokeWidth {set;}

		/// <summary>
		/// The default value is <seealso cref="Style#FILL"/>.
		/// </summary>
		Style Style {set;}

		Align TextAlign {set;}

		float TextSize {set;}

		void SetTypeface(FontFamily fontFamily, FontStyle fontStyle);
	}
}