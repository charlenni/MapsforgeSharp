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

namespace org.mapsforge.core.model
{
    using NUnit.Framework;
    using System;

    public class PointTest
	{
		private string POINT_TO_STRING = string.Format("x={0}, y={1}", 1, 2);

        [Test()]
		public virtual void CompareToTest()
		{
			Point point1 = new Point(1, 2);
			Point point2 = new Point(1, 2);
			Point point3 = new Point(1, 1);
			Point point4 = new Point(2, 2);

			Assert.AreEqual(0, point1.CompareTo(point2));

			TestUtils.NotCompareToTest(point1, point3);
			TestUtils.NotCompareToTest(point1, point4);
		}

        [Test()]
        public virtual void DistanceTest()
		{
			Point point1 = new Point(0, 0);
			Point point2 = new Point(1, 1);
			Point point3 = new Point(0, -1);

			Assert.AreEqual(0, point1.Distance(point1), 0);
			Assert.AreEqual(0, point2.Distance(point2), 0);
			Assert.AreEqual(0, point3.Distance(point3), 0);

			Assert.AreEqual(Math.Sqrt(2), point1.Distance(point2), 0);
			Assert.AreEqual(Math.Sqrt(2), point2.Distance(point1), 0);

			Assert.AreEqual(1, point1.Distance(point3), 0);
			Assert.AreEqual(1, point3.Distance(point1), 0);
		}

        [Test()]
        public virtual void EqualsTest()
		{
			Point point1 = new Point(1, 2);
			Point point2 = new Point(1, 2);
			Point point3 = new Point(1, 1);
			Point point4 = new Point(2, 2);

			TestUtils.EqualsTest(point1, point2);

			TestUtils.NotEqualsTest(point1, point3);
			TestUtils.NotEqualsTest(point1, point4);
			TestUtils.NotEqualsTest(point1, new object());
			TestUtils.NotEqualsTest(point1, null);
		}

        [Test()]
        public virtual void FieldsTest()
		{
			Point point = new Point(1, 2);
			Assert.AreEqual(1, point.X, 0);
			Assert.AreEqual(2, point.Y, 0);
		}

        [Test()]
        public virtual void SerializeTest()
		{
			Point point = new Point(1, 2);
			TestUtils.SerializeTest(point);
		}

        [Test()]
        public virtual void ToStringTest()
		{
			Point point = new Point(1, 2);
			Assert.AreEqual(POINT_TO_STRING, point.ToString());
		}
	}
}