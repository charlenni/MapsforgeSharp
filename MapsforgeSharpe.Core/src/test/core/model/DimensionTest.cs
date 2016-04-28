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

	public class DimensionTest
	{
		private const string POINT_TO_STRING = "width=1, height=2";

		private static Dimension createDimension(int width, int height)
		{
			return new Dimension(width, height);
		}

		private static void verifyInvalid(int width, int height)
		{
			try
			{
				createDimension(width, height);
				Assert.fail("width: " + width + ", height: " + height);
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
			Dimension dimension = new Dimension(0, 0);
			Assert.assertEquals(0, dimension.width);
			Assert.assertEquals(0, dimension.height);

			verifyInvalid(-1, 0);
			verifyInvalid(0, -1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsTest()
		public virtual void equalsTest()
		{
			Dimension dimension1 = new Dimension(1, 2);
			Dimension dimension2 = new Dimension(1, 2);
			Dimension dimension3 = new Dimension(1, 1);
			Dimension dimension4 = new Dimension(2, 2);

			TestUtils.equalsTest(dimension1, dimension2);

			TestUtils.notEqualsTest(dimension1, dimension3);
			TestUtils.notEqualsTest(dimension1, dimension4);
			TestUtils.notEqualsTest(dimension1, new object());
			TestUtils.notEqualsTest(dimension1, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serializeTest() throws java.io.IOException, ClassNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void serializeTest()
		{
			Dimension dimension = new Dimension(1, 2);
			TestUtils.serializeTest(dimension);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toStringTest()
		public virtual void toStringTest()
		{
			Dimension dimension = new Dimension(1, 2);
			Assert.assertEquals(POINT_TO_STRING, dimension.ToString());
		}
	}

}