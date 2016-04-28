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

	public class MapPositionTest
	{
		private const string MAP_POSITION_TO_STRING = "latLong=latitude=1.0, longitude=2.0, zoomLevel=3";

		private static MapPosition invokeConstructor(LatLong latLong, sbyte zoomLevel)
		{
			return new MapPosition(latLong, zoomLevel);
		}

		private static void verifyBadConstructor(LatLong latLong, sbyte zoomLevel)
		{
			try
			{
				invokeConstructor(latLong, zoomLevel);
				Assert.fail("latLong: " + latLong + ", zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void badConstructorTest()
		public virtual void badConstructorTest()
		{
			verifyBadConstructor(new LatLong(1.0, 2.0, true), (sbyte) -1);
			verifyBadConstructor(null, (sbyte) 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsTest()
		public virtual void equalsTest()
		{
			MapPosition mapPosition1 = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 3);
			MapPosition mapPosition2 = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 3);
			MapPosition mapPosition3 = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 0);
			MapPosition mapPosition4 = new MapPosition(new LatLong(0, 0, true), (sbyte) 3);

			TestUtils.equalsTest(mapPosition1, mapPosition2);

			TestUtils.notEqualsTest(mapPosition1, mapPosition3);
			TestUtils.notEqualsTest(mapPosition1, mapPosition4);
			TestUtils.notEqualsTest(mapPosition1, new object());
			TestUtils.notEqualsTest(mapPosition1, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fieldsTest()
		public virtual void fieldsTest()
		{
			MapPosition mapPosition = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 3);
			Assert.assertEquals(new LatLong(1.0, 2.0, true), mapPosition.latLong);
			Assert.assertEquals(3, mapPosition.zoomLevel);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serializeTest() throws java.io.IOException, ClassNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void serializeTest()
		{
			MapPosition mapPosition = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 3);
			TestUtils.serializeTest(mapPosition);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toStringTest()
		public virtual void toStringTest()
		{
			MapPosition mapPosition = new MapPosition(new LatLong(1.0, 2.0, true), (sbyte) 3);
			Assert.assertEquals(MAP_POSITION_TO_STRING, mapPosition.ToString());
		}
	}

}