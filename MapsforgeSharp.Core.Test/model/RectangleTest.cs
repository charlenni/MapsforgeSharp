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

    public class RectangleTest
	{
		private string RECTANGLE_TO_STRING = string.Format("left={0}, top={1}, right={2}, bottom={3}", 1, 2, 3, 4);

		private static void AssertIntersection(Rectangle rectangle1, Rectangle rectangle2)
		{
			Assert.True(rectangle1.Intersects(rectangle2));
			Assert.True(rectangle2.Intersects(rectangle1));
		}

		private static void AssertNoIntersection(Rectangle rectangle1, Rectangle rectangle2)
		{
			Assert.False(rectangle1.Intersects(rectangle2));
			Assert.False(rectangle2.Intersects(rectangle1));
		}

		private static Rectangle Create(double left, double top, double right, double bottom)
		{
			return new Rectangle(left, top, right, bottom);
		}

		private static void VerifyInvalidConstructor(double left, double top, double right, double bottom)
		{
			try
			{
				Create(left, top, right, bottom);
				Assert.Fail("left: " + left + ", top: " + top + ", right: " + right + ", bottom: " + bottom);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

        [Test()]
		public virtual void ConstructorTest()
		{
			Create(1, 2, 3, 4);

			VerifyInvalidConstructor(1, 2, 0, 4);
			VerifyInvalidConstructor(1, 2, 3, 0);
		}

        [Test()]
        public virtual void ContainsTest()
		{
			Rectangle rectangle = Create(1, 2, 3, 4);

			Assert.True(rectangle.Contains(new Point(1, 2)));
			Assert.True(rectangle.Contains(new Point(1, 4)));
			Assert.True(rectangle.Contains(new Point(3, 2)));
			Assert.True(rectangle.Contains(new Point(3, 4)));
			Assert.True(rectangle.Contains(new Point(2, 3)));

			Assert.False(rectangle.Contains(new Point(0, 0)));
			Assert.False(rectangle.Contains(new Point(1, 1)));
			Assert.False(rectangle.Contains(new Point(4, 4)));
			Assert.False(rectangle.Contains(new Point(5, 5)));
		}

        [Test()]
        public virtual void EqualsTest()
		{
			Rectangle rectangle1 = Create(1, 2, 3, 4);
			Rectangle rectangle2 = Create(1, 2, 3, 4);
			Rectangle rectangle3 = Create(3, 2, 3, 4);
			Rectangle rectangle4 = Create(1, 4, 3, 4);
			Rectangle rectangle5 = Create(1, 2, 1, 4);
			Rectangle rectangle6 = Create(1, 2, 3, 2);

			TestUtils.EqualsTest(rectangle1, rectangle2);

			TestUtils.NotEqualsTest(rectangle1, rectangle3);
			TestUtils.NotEqualsTest(rectangle1, rectangle4);
			TestUtils.NotEqualsTest(rectangle1, rectangle5);
			TestUtils.NotEqualsTest(rectangle1, rectangle6);
			TestUtils.NotEqualsTest(rectangle1, new object());
			TestUtils.NotEqualsTest(rectangle1, null);
		}

        [Test()]
        public virtual void GetCenterTest()
		{
			Rectangle rectangle = Create(1, 2, 3, 4);
			Assert.AreEqual(new Point(2, 3), rectangle.Center);
		}

        [Test()]
        public virtual void GetCenterXTest()
		{
			Rectangle rectangle = Create(1, 2, 3, 4);
			Assert.AreEqual(2, rectangle.CenterX, 0);
		}

        [Test()]
        public virtual void GetCenterYTest()
		{
			Rectangle rectangle = Create(1, 2, 3, 4);
			Assert.AreEqual(3, rectangle.CenterY, 0);
		}

        [Test()]
        public virtual void GetHeightTest()
		{
			Rectangle rectangle = Create(1, 2, 3, 4);
			Assert.AreEqual(2, rectangle.Height, 0);
		}

        [Test()]
        public virtual void GetWidthTest()
		{
			Rectangle rectangle = Create(1, 2, 3, 4);
			Assert.AreEqual(2, rectangle.Width, 0);
		}

        [Test()]
        public virtual void IntersectsCircleTest()
		{
			Rectangle rectangle1 = Create(1, 2, 3, 4);

			Assert.True(rectangle1.IntersectsCircle(1, 2, 0));
			Assert.True(rectangle1.IntersectsCircle(1, 2, 1));
			Assert.True(rectangle1.IntersectsCircle(1, 2, 10));

			Assert.True(rectangle1.IntersectsCircle(2, 3, 0));
			Assert.True(rectangle1.IntersectsCircle(2, 3, 1));
			Assert.True(rectangle1.IntersectsCircle(2, 3, 10));

			Assert.True(rectangle1.IntersectsCircle(3.5, 4, 0.5));
			Assert.True(rectangle1.IntersectsCircle(3, 4.5, 0.5));

			Assert.True(rectangle1.IntersectsCircle(4, 4, 1));
			Assert.True(rectangle1.IntersectsCircle(4, 4, 10));

			Assert.False(rectangle1.IntersectsCircle(0, 0, 0));
			Assert.False(rectangle1.IntersectsCircle(0, 1, 0));
			Assert.False(rectangle1.IntersectsCircle(0, 1, 1));

			Assert.False(rectangle1.IntersectsCircle(3.5, 4, 0.49999));
			Assert.False(rectangle1.IntersectsCircle(3, 4.5, 0.49999));

			Assert.False(rectangle1.IntersectsCircle(4, 5, 1));
			Assert.False(rectangle1.IntersectsCircle(4, 5, 1.4));
		}

        [Test()]
		public virtual void IntersectsTest()
		{
			Rectangle rectangle1 = Create(1, 2, 3, 4);
			Rectangle rectangle2 = Create(1, 2, 3, 4);
			Rectangle rectangle3 = Create(3, 4, 3, 4);
			Rectangle rectangle4 = Create(0, 0, 3, 4);
			Rectangle rectangle5 = Create(0, 0, 5, 5);
			Rectangle rectangle6 = Create(5, 5, 6, 6);
			Rectangle rectangle7 = Create(1, 0, 3, 1);

			AssertIntersection(rectangle1, rectangle1);
			AssertIntersection(rectangle1, rectangle2);
			AssertIntersection(rectangle1, rectangle3);
			AssertIntersection(rectangle1, rectangle4);
			AssertIntersection(rectangle1, rectangle5);

			AssertNoIntersection(rectangle1, rectangle6);
			AssertNoIntersection(rectangle1, rectangle7);
		}

        [Test()]
        public virtual void SerializeTest()
		{
			Rectangle rectangle = Create(1, 2, 3, 4);
			TestUtils.SerializeTest(rectangle);
		}

        [Test()]
        public virtual void ToStringTest()
		{
			Rectangle rectangle = Create(1, 2, 3, 4);
			Assert.AreEqual(RECTANGLE_TO_STRING, rectangle.ToString());
		}
	}
}