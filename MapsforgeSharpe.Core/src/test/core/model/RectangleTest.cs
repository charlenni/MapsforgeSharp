/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;

	public class RectangleTest
	{
		private const string RECTANGLE_TO_STRING = "left=1.0, top=2.0, right=3.0, bottom=4.0";

		private static void assertIntersection(Rectangle rectangle1, Rectangle rectangle2)
		{
			Assert.assertTrue(rectangle1.intersects(rectangle2));
			Assert.assertTrue(rectangle2.intersects(rectangle1));
		}

		private static void assertNoIntersection(Rectangle rectangle1, Rectangle rectangle2)
		{
			Assert.assertFalse(rectangle1.intersects(rectangle2));
			Assert.assertFalse(rectangle2.intersects(rectangle1));
		}

		private static Rectangle create(double left, double top, double right, double bottom)
		{
			return new Rectangle(left, top, right, bottom);
		}

		private static void verifyInvalidConstructor(double left, double top, double right, double bottom)
		{
			try
			{
				create(left, top, right, bottom);
				Assert.fail("left: " + left + ", top: " + top + ", right: " + right + ", bottom: " + bottom);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void constructorTest()
		public virtual void constructorTest()
		{
			create(1, 2, 3, 4);

			verifyInvalidConstructor(1, 2, 0, 4);
			verifyInvalidConstructor(1, 2, 3, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void containsTest()
		public virtual void containsTest()
		{
			Rectangle rectangle = create(1, 2, 3, 4);

			Assert.assertTrue(rectangle.contains(new Point(1, 2)));
			Assert.assertTrue(rectangle.contains(new Point(1, 4)));
			Assert.assertTrue(rectangle.contains(new Point(3, 2)));
			Assert.assertTrue(rectangle.contains(new Point(3, 4)));
			Assert.assertTrue(rectangle.contains(new Point(2, 3)));

			Assert.assertFalse(rectangle.contains(new Point(0, 0)));
			Assert.assertFalse(rectangle.contains(new Point(1, 1)));
			Assert.assertFalse(rectangle.contains(new Point(4, 4)));
			Assert.assertFalse(rectangle.contains(new Point(5, 5)));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsTest()
		public virtual void equalsTest()
		{
			Rectangle rectangle1 = create(1, 2, 3, 4);
			Rectangle rectangle2 = create(1, 2, 3, 4);
			Rectangle rectangle3 = create(3, 2, 3, 4);
			Rectangle rectangle4 = create(1, 4, 3, 4);
			Rectangle rectangle5 = create(1, 2, 1, 4);
			Rectangle rectangle6 = create(1, 2, 3, 2);

			TestUtils.equalsTest(rectangle1, rectangle2);

			TestUtils.notEqualsTest(rectangle1, rectangle3);
			TestUtils.notEqualsTest(rectangle1, rectangle4);
			TestUtils.notEqualsTest(rectangle1, rectangle5);
			TestUtils.notEqualsTest(rectangle1, rectangle6);
			TestUtils.notEqualsTest(rectangle1, new object());
			TestUtils.notEqualsTest(rectangle1, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getCenterTest()
		public virtual void getCenterTest()
		{
			Rectangle rectangle = create(1, 2, 3, 4);
			Assert.assertEquals(new Point(2, 3), rectangle.Center);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getCenterXTest()
		public virtual void getCenterXTest()
		{
			Rectangle rectangle = create(1, 2, 3, 4);
			Assert.assertEquals(2, rectangle.CenterX, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getCenterYTest()
		public virtual void getCenterYTest()
		{
			Rectangle rectangle = create(1, 2, 3, 4);
			Assert.assertEquals(3, rectangle.CenterY, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getHeightTest()
		public virtual void getHeightTest()
		{
			Rectangle rectangle = create(1, 2, 3, 4);
			Assert.assertEquals(2, rectangle.Height, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getWidthTest()
		public virtual void getWidthTest()
		{
			Rectangle rectangle = create(1, 2, 3, 4);
			Assert.assertEquals(2, rectangle.Width, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void intersectsCircleTest()
		public virtual void intersectsCircleTest()
		{
			Rectangle rectangle1 = create(1, 2, 3, 4);

			Assert.assertTrue(rectangle1.intersectsCircle(1, 2, 0));
			Assert.assertTrue(rectangle1.intersectsCircle(1, 2, 1));
			Assert.assertTrue(rectangle1.intersectsCircle(1, 2, 10));

			Assert.assertTrue(rectangle1.intersectsCircle(2, 3, 0));
			Assert.assertTrue(rectangle1.intersectsCircle(2, 3, 1));
			Assert.assertTrue(rectangle1.intersectsCircle(2, 3, 10));

			Assert.assertTrue(rectangle1.intersectsCircle(3.5, 4, 0.5));
			Assert.assertTrue(rectangle1.intersectsCircle(3, 4.5, 0.5));

			Assert.assertTrue(rectangle1.intersectsCircle(4, 4, 1));
			Assert.assertTrue(rectangle1.intersectsCircle(4, 4, 10));

			Assert.assertFalse(rectangle1.intersectsCircle(0, 0, 0));
			Assert.assertFalse(rectangle1.intersectsCircle(0, 1, 0));
			Assert.assertFalse(rectangle1.intersectsCircle(0, 1, 1));

			Assert.assertFalse(rectangle1.intersectsCircle(3.5, 4, 0.49999));
			Assert.assertFalse(rectangle1.intersectsCircle(3, 4.5, 0.49999));

			Assert.assertFalse(rectangle1.intersectsCircle(4, 5, 1));
			Assert.assertFalse(rectangle1.intersectsCircle(4, 5, 1.4));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void intersectsTest()
		public virtual void intersectsTest()
		{
			Rectangle rectangle1 = create(1, 2, 3, 4);
			Rectangle rectangle2 = create(1, 2, 3, 4);
			Rectangle rectangle3 = create(3, 4, 3, 4);
			Rectangle rectangle4 = create(0, 0, 3, 4);
			Rectangle rectangle5 = create(0, 0, 5, 5);
			Rectangle rectangle6 = create(5, 5, 6, 6);
			Rectangle rectangle7 = create(1, 0, 3, 1);

			assertIntersection(rectangle1, rectangle1);
			assertIntersection(rectangle1, rectangle2);
			assertIntersection(rectangle1, rectangle3);
			assertIntersection(rectangle1, rectangle4);
			assertIntersection(rectangle1, rectangle5);

			assertNoIntersection(rectangle1, rectangle6);
			assertNoIntersection(rectangle1, rectangle7);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serializeTest() throws java.io.IOException, ClassNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void serializeTest()
		{
			Rectangle rectangle = create(1, 2, 3, 4);
			TestUtils.serializeTest(rectangle);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toStringTest()
		public virtual void toStringTest()
		{
			Rectangle rectangle = create(1, 2, 3, 4);
			Assert.assertEquals(RECTANGLE_TO_STRING, rectangle.ToString());
		}
	}

}