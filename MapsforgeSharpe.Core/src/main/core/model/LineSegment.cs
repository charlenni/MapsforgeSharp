/*
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

namespace org.mapsforge.core.model
{
    using System;
    using System.Text;

    /// <summary>
    /// A directed line segment between two Points.
    /// </summary>
    public sealed class LineSegment
	{

		private static int INSIDE = 0; // 0000
		private static int LEFT = 1; // 0001
		private static int RIGHT = 2; // 0010
		private static int BOTTOM = 4; // 0100
		private static int TOP = 8; // 1000

		public readonly Point start;
		public readonly Point end;

		/// <summary>
		/// Ctor with given start and end point
		/// </summary>
		/// <param name="start"> start point </param>
		/// <param name="end"> end point </param>
		public LineSegment(Point start, Point end)
		{
			this.start = start;
			this.end = end;
		}

		/// <summary>
		/// Ctor with given start point, a point that defines the direction of the line and a length
		/// </summary>
		/// <param name="start"> start point </param>
		/// <param name="direction"> point that defines the direction (a line from start to direction point) </param>
		/// <param name="distance"> how long to move along the line between start and direction </param>
		public LineSegment(Point start, Point direction, double distance)
		{
			this.start = start;
			this.end = (new LineSegment(start, direction)).PointAlongLineSegment(distance);
		}

		/// <summary>
		/// Intersection of this LineSegment with the Rectangle as another LineSegment.
		/// 
		/// Algorithm is Cohen-Sutherland, see https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm .
		/// </summary>
		/// <param name="r"> the rectangle to clip to. </param>
		/// <returns> the LineSegment that falls into the Rectangle, null if there is no intersection.
		///  </returns>
		public LineSegment ClipToRectangle(Rectangle r)
		{

			Point a = this.start;
			Point b = this.end;

			int codeStart = Code(r, a);
			int codeEnd = Code(r, b);

			while (true)
			{
				if (0 == (codeStart | codeEnd))
				{
					// both points are inside, intersection is the computed line
					return new LineSegment(a, b);
				}
				else if (0 != (codeStart & codeEnd))
				{
					// both points are either below, above, left or right of the box, no intersection
					return null;
				}
				else
				{
					double newX;
					double newY;
					// At least one endpoint is outside the clip rectangle; pick it.
					int outsideCode = (0 != codeStart) ? codeStart : codeEnd;

					if (0 != (outsideCode & TOP))
					{
						// point is above the clip rectangle
						newX = a.x + (b.x - a.x) * (r.top - a.y) / (b.y - a.y);
						newY = r.top;
					}
					else if (0 != (outsideCode & BOTTOM))
					{
						// point is below the clip rectangle
						newX = a.x + (b.x - a.x) * (r.bottom - a.y) / (b.y - a.y);
						newY = r.bottom;
					}
					else if (0 != (outsideCode & RIGHT))
					{
						// point is to the right of clip rectangle
						newY = a.y + (b.y - a.y) * (r.right - a.x) / (b.x - a.x);
						newX = r.right;
					}
					else if (0 != (outsideCode & LEFT))
					{
						// point is to the left of clip rectangle
						newY = a.y + (b.y - a.y) * (r.left - a.x) / (b.x - a.x);
						newX = r.left;
					}
					else
					{
						throw new System.InvalidOperationException("Should not get here");
					}
					// Now we move outside point to intersection point to clip
					// and get ready for next pass.
					if (outsideCode == codeStart)
					{
						a = new Point(newX, newY);
						codeStart = Code(r, a);
					}
					else
					{
						b = new Point(newX, newY);
						codeEnd = Code(r, b);
					}
				}
			}
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is LineSegment))
			{
				return false;
			}
			LineSegment other = (LineSegment) obj;
			if (other.start.Equals(this.start) && other.end.Equals(this.end))
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + this.start.GetHashCode();
			result = prime * result + this.end.GetHashCode();
			return result;
		}

		/// <summary>
		/// Returns a fast computation if the line intersects the rectangle or bias if there
		/// is no fast way to compute the intersection. </summary>
		/// <param name="r"> retangle to test </param>
		/// <param name="bias"> the result if no fast computation is possible </param>
		/// <returns> either the fast and correct result or the bias (which might be wrong). </returns>
		public bool IntersectsRectangle(Rectangle r, bool bias)
		{
			int codeStart = Code(r, this.start);
			int codeEnd = Code(r, this.end);

			if (0 == (codeStart | codeEnd))
			{
				// both points are inside, trivial case
				return true;
			}
			else if (0 != (codeStart & codeEnd))
			{
				// both points are either below, above, left or right of the box, no intersection
				return false;
			}
			return bias;
		}

		/// <summary>
		/// Euclidian distance between start and end.
		/// </summary>
		/// <returns> the length of the segment. </returns>
		public double Length()
		{
			return start.Distance(end);
		}


		/// <summary>
		/// Computes a Point along the line segment with a given distance to the start Point. </summary>
		/// <param name="distance"> distance from start point </param>
		/// <returns> point at given distance from start point </returns>
		public Point PointAlongLineSegment(double distance)
		{
			if (start.x == end.x)
			{
				// we have a vertical line
				return new Point(start.x, start.y + distance);
			}
			else
			{
				double slope = (end.y - start.y) / (end.x - start.x);
				double dx = Math.Sqrt((distance * distance) / (1 + (slope * slope)));
				if (end.x < start.x)
				{
					dx *= -1;
				}
				return new Point(start.x + dx, start.y + slope * dx);
			}
		}

		/// <summary>
		/// New line segment with start and end reversed. </summary>
		/// <returns> new LineSegment with start and end reversed </returns>
		public LineSegment Reverse()
		{
			return new LineSegment(this.end, this.start);
		}

		/// <summary>
		/// LineSegment that starts at offset from start and runs for length towards end point </summary>
		/// <param name="offset"> offset applied at begin of line </param>
		/// <param name="length"> length of the new segment </param>
		/// <returns> new LineSegment computed </returns>
		public LineSegment SubSegment(double offset, double length)
		{
			Point subSegmentStart = PointAlongLineSegment(offset);
			Point subSegmentEnd = PointAlongLineSegment(offset + length);
			return new LineSegment(subSegmentStart, subSegmentEnd);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(start).Append(" ").Append(end);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Computes the location code according to Cohen-Sutherland,
		/// see https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm.
		/// </summary>
		private static int Code(Rectangle r, Point p)
		{
			int code = INSIDE;
			if (p.x < r.left)
			{
				// to the left of clip window
				code |= LEFT;
			}
			else if (p.x > r.right)
			{
				// to the right of clip window
				code |= RIGHT;
			}

			if (p.y > r.bottom)
			{
				// below the clip window
				code |= BOTTOM;
			}
			else if (p.y < r.top)
			{
				// above the clip window
				code |= TOP;
			}
			return code;
		}
	}
}