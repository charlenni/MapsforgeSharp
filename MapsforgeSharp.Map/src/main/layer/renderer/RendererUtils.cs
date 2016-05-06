/*
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

namespace org.mapsforge.map.layer.renderer
{
    using System;

    using Point = MapsforgeSharp.Core.Model.Point;

	internal class RendererUtils
	{
		/// <summary>
		/// Computes a polyline with distance dy parallel to given coordinates.
		/// http://objectmix.com/graphics/132987-draw-parallel-polyline-algorithm-needed.html
		/// </summary>
		internal static Point[] ParallelPath(Point[] p, double dy)
		{
			int n = p.Length - 1;
			Point[] u = new Point[n];
			Point[] h = new Point[p.Length];

			// Generate an array u[] of unity vectors of each direction
			for (int k = 0; k < n; ++k)
			{
				double c = p[k + 1].X - p[k].X;
				double s = p[k + 1].Y - p[k].Y;
				double l = Math.Sqrt(c * c + s * s);
				if (l == 0)
				{
					u[k] = new Point(0, 0);
				}
				else
				{
					u[k] = new Point(c / l, s / l);
				}
			}

			// For the start point calculate the normal
			h[0] = new Point(p[0].X - dy * u[0].Y, p[0].Y + dy * u[0].X);

			// For 1 to N-1 calculate the intersection of the offset lines
			for (int k = 1; k < n; k++)
			{
				double l = dy / (1 + u[k].X * u[k - 1].X + u[k].Y * u[k - 1].Y);
				h[k] = new Point(p[k].X - l * (u[k].Y + u[k - 1].Y), p[k].Y + l * (u[k].X + u[k - 1].X));
			}

			// For the end point use the normal
			h[n] = new Point(p[n].X - dy * u[n - 1].Y, p[n].Y + dy * u[n - 1].X);

			return h;
		}

		private RendererUtils()
		{
			throw new System.InvalidOperationException();
		}
	}
}