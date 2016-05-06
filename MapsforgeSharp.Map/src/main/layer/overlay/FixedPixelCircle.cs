/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2015 devemux86
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

namespace org.mapsforge.map.layer.overlay
{
	using Paint = MapsforgeSharp.Core.Graphics.Paint;
	using LatLong = MapsforgeSharp.Core.Model.LatLong;
	using Point = MapsforgeSharp.Core.Model.Point;

	/// <summary>
	/// A Circle class that is always drawn with the same size in pixels.
	/// </summary>
	public class FixedPixelCircle : Circle
	{
		/// <param name="latLong">
		///            the initial center point of this circle (may be null). </param>
		/// <param name="radius">
		///            the initial non-negative radius of this circle in pixels. </param>
		/// <param name="paintFill">
		///            the initial {@code Paint} used to fill this circle (may be null). </param>
		/// <param name="paintStroke">
		///            the initial {@code Paint} used to stroke this circle (may be null). </param>
		/// <exception cref="IllegalArgumentException">
		///             if the given {@code radius} is negative or <seealso cref="Float#NaN"/>. </exception>
		public FixedPixelCircle(LatLong latLong, float radius, Paint paintFill, Paint paintStroke) : this(latLong, radius, paintFill, paintStroke, false)
		{
		}

		/// <param name="latLong">
		///            the initial center point of this circle (may be null). </param>
		/// <param name="radius">
		///            the initial non-negative radius of this circle in pixels. </param>
		/// <param name="paintFill">
		///            the initial {@code Paint} used to fill this circle (may be null). </param>
		/// <param name="paintStroke">
		///            the initial {@code Paint} used to stroke this circle (may be null). </param>
		/// <param name="keepAligned"> if set to true it will keep the bitmap aligned with the map, to avoid
		///                    a moving effect of a bitmap shader. </param>
		/// <exception cref="IllegalArgumentException">
		///             if the given {@code radius} is negative or <seealso cref="Float#NaN"/>. </exception>
		public FixedPixelCircle(LatLong latLong, float radius, Paint paintFill, Paint paintStroke, bool keepAligned) : base(latLong, radius, paintFill, paintStroke, keepAligned)
		{
		}

		public virtual bool Contains(Point center, Point point)
		{
			return center.Distance(point) < this.Radius;
		}

		/// <returns> the non-negative radius of this circle in pixels. </returns>
		protected internal override int GetRadiusInPixels(double latitude, sbyte zoomLevel)
		{
			return (int)(this.Radius * this.displayModel.ScaleFactor);
		}
	}
}