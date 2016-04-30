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

    public class MapPositionTest
	{
		private string MAP_POSITION_TO_STRING = string.Format("latLong=latitude={0}, longitude={1}, zoomLevel={2}", 1, 2, 3);

		private static MapPosition invokeConstructor(LatLong latLong, sbyte zoomLevel)
		{
			return new MapPosition(latLong, zoomLevel);
		}

		private static void VerifyBadConstructor(LatLong latLong, sbyte zoomLevel)
		{
			try
			{
				invokeConstructor(latLong, zoomLevel);
				Assert.Fail("latLong: " + latLong + ", zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

        [Test()]
		public virtual void BadConstructorTest()
		{
			VerifyBadConstructor(new LatLong(1.0, 2.0, true), (sbyte) -1);
			VerifyBadConstructor(null, (sbyte) 0);
		}

        [Test()]
        public virtual void EqualsTest()
		{
			MapPosition mapPosition1 = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 3);
			MapPosition mapPosition2 = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 3);
			MapPosition mapPosition3 = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 0);
			MapPosition mapPosition4 = new MapPosition(new LatLong(0, 0, true), (sbyte) 3);

			TestUtils.EqualsTest(mapPosition1, mapPosition2);

			TestUtils.NotEqualsTest(mapPosition1, mapPosition3);
			TestUtils.NotEqualsTest(mapPosition1, mapPosition4);
			TestUtils.NotEqualsTest(mapPosition1, new object());
			TestUtils.NotEqualsTest(mapPosition1, null);
		}

        [Test()]
        public virtual void FieldsTest()
		{
			MapPosition mapPosition = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 3);
			Assert.AreEqual(new LatLong(1.0, 2.0, true), mapPosition.LatLong);
			Assert.AreEqual(3, mapPosition.ZoomLevel);
		}

        [Test()]
        public virtual void SerializeTest()
		{
			MapPosition mapPosition = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 3);
			TestUtils.SerializeTest(mapPosition);
		}

        [Test()]
        public virtual void ToStringTest()
		{
			MapPosition mapPosition = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 3);
			Assert.AreEqual(MAP_POSITION_TO_STRING, mapPosition.ToString());
		}
	}
}