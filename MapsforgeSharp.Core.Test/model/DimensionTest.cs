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

    public class DimensionTest
	{
		private const string POINT_TO_STRING = "width=1, height=2";

		private static Dimension CreateDimension(int width, int height)
		{
			return new Dimension(width, height);
		}

		private static void VerifyInvalid(int width, int height)
		{
			try
			{
				CreateDimension(width, height);
				Assert.Fail("width: " + width + ", height: " + height);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

        [Test()]
		public virtual void ConstructorTest()
		{
			Dimension dimension = new Dimension(0, 0);
			Assert.AreEqual(0, dimension.Width);
			Assert.AreEqual(0, dimension.Height);

			VerifyInvalid(-1, 0);
			VerifyInvalid(0, -1);
		}

        [Test()]
        public virtual void EqualsTest()
		{
			Dimension dimension1 = new Dimension(1, 2);
			Dimension dimension2 = new Dimension(1, 2);
			Dimension dimension3 = new Dimension(1, 1);
			Dimension dimension4 = new Dimension(2, 2);

			TestUtils.EqualsTest(dimension1, dimension2);

			TestUtils.NotEqualsTest(dimension1, dimension3);
			TestUtils.NotEqualsTest(dimension1, dimension4);
			TestUtils.NotEqualsTest(dimension1, new object());
			TestUtils.NotEqualsTest(dimension1, null);
		}

        [Test()]
        public virtual void SerializeTest()
		{
			Dimension dimension = new Dimension(1, 2);
			TestUtils.SerializeTest(dimension);
		}

        [Test()]
        public virtual void ToStringTest()
		{
			Dimension dimension = new Dimension(1, 2);
			Assert.AreEqual(POINT_TO_STRING, dimension.ToString());
		}
	}
}