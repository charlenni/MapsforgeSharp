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
    using NUnit.Framework;
    using System;

    public class LineSegmentTest
	{
        [Test()]
		public virtual void CtorTest()
		{
			// tests the second ctor that computes a line segment from point, target and distance

			Point[] points = new Point[]
			{
				new Point(1, 0),
				new Point(0, 1),
				new Point(0, 0),
				new Point(1, 1),
				new Point(2,2),
				new Point(0,4),
				new Point(-182.9934, 0),
				new Point(34.6, -356.1)
			};
			Point point1 = new Point(0, 0);
			Point point2 = new Point(1, 0);
			Point point3 = new Point(2, 2);

			LineSegment ls1 = new LineSegment(point1, point2, 0.5);
			Assert.AreEqual(new Point(0.5, 0), ls1.End);

			Assert.AreEqual(Math.Sqrt(8), point1.Distance(point3), 0.001d);

			foreach (Point point in points)
			{
				LineSegment ls2 = new LineSegment(point1, point, point1.Distance(point));
				Assert.AreEqual(point, ls2.End);
			}

			LineSegment ls3 = new LineSegment(point1, point3, 0.5 * point1.Distance(point3));
			Assert.AreEqual(new Point(1, 1), ls3.End);
		}

        [Test()]
        public virtual void EqualsTest()
		{
			Point point1 = new Point(1, 2);
			Point point2 = new Point(1, 2);
			Point point3 = new Point(1, 1);

			LineSegment point1ToPoint2 = new LineSegment(point1, point2);
			LineSegment point1ToPoint3 = new LineSegment(point1, point3);
			LineSegment point1ToPoint1 = new LineSegment(point1, point1);

			Assert.AreNotEqual(point1ToPoint1, point1ToPoint3);
			Assert.AreEqual(point1ToPoint1, point1ToPoint1);
			Assert.AreEqual(point1ToPoint1, point1ToPoint2);
		}

        [Test()]
        public virtual void IntersectionTest()
		{
			Point point1 = new Point(1, 2);
			// Point point2 = new Point(1, 2);
			Point point3 = new Point(1, 1);
			Point point4 = new Point(2, -22);
			Point point5 = new Point(2, 22);
			Point point6 = new Point(2, 0);
			Point point7 = new Point(2, 5);

			LineSegment point1ToPoint3 = new LineSegment(point1, point3);
			LineSegment point1ToPoint1 = new LineSegment(point1, point1);
			LineSegment vertical = new LineSegment(point4, point5);

			Rectangle r1 = new Rectangle(0,0,5,5);
			Rectangle r2 = new Rectangle(-22,-22,-11,-11);

			LineSegment s1 = point1ToPoint3.ClipToRectangle(r1);
			Assert.AreEqual(point1ToPoint3, s1);

			LineSegment s2 = point1ToPoint1.ClipToRectangle(r1);
			Assert.AreEqual(s2, point1ToPoint1);

			LineSegment s3 = point1ToPoint1.ClipToRectangle(r2);
			Assert.AreEqual(s3, null);

			LineSegment verticalClipped = new LineSegment(point6, point7);
			LineSegment s4 = vertical.ClipToRectangle(r1);
			Assert.AreEqual(s4, verticalClipped);

			LineSegment s5 = vertical.ClipToRectangle(r2);
			Assert.AreEqual(s5, null);

			Rectangle r3 = new Rectangle(-1,-1,1,1);
			Point point8 = new Point(10, 10);
			Point point9 = new Point(-10, -10);
			Point point10 = new Point(-1, -1);
			Point point11 = new Point(1, 1);
			LineSegment s6 = new LineSegment(point8, point9);
			LineSegment s7 = s6.ClipToRectangle(r3);
			LineSegment s8 = new LineSegment(point11, point10);
			Assert.AreEqual(s8, s7);

			LineSegment s6r = new LineSegment(point9, point8);
			LineSegment s7r = s6r.ClipToRectangle(r3);
			LineSegment s8r = new LineSegment(point10, point11);
			Assert.AreEqual(s8r, s7r);
		}
	}
}