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

	public class LatLongTest
	{
		private const string GEO_POINT_TO_STRING = "latitude=1.0, longitude=2.0";
		private const double LATITUDE = 1.0;
		private const double LONGITUDE = 2.0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compareToTest()
		public virtual void compareToTest()
		{
			LatLong latLong1 = new LatLong(LATITUDE, LONGITUDE, true);
			LatLong latLong2 = new LatLong(LATITUDE, LONGITUDE, true);
			LatLong latLong3 = new LatLong(LATITUDE, LATITUDE, true);
			LatLong latLong4 = new LatLong(LONGITUDE, LONGITUDE, true);

			Assert.assertEquals(0, latLong1.compareTo(latLong2));

			TestUtils.notCompareToTest(latLong1, latLong3);
			TestUtils.notCompareToTest(latLong1, latLong4);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsTest()
		public virtual void equalsTest()
		{
			LatLong latLong1 = new LatLong(LATITUDE, LONGITUDE, true);
			LatLong latLong2 = new LatLong(LATITUDE, LONGITUDE, true);
			LatLong latLong3 = new LatLong(LATITUDE, LATITUDE, true);
			LatLong latLong4 = new LatLong(LONGITUDE, LONGITUDE, true);

			TestUtils.equalsTest(latLong1, latLong2);

			TestUtils.notEqualsTest(latLong1, latLong3);
			TestUtils.notEqualsTest(latLong1, latLong4);
			TestUtils.notEqualsTest(latLong1, new object());
			TestUtils.notEqualsTest(latLong1, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fieldsTest()
		public virtual void fieldsTest()
		{
			LatLong latLong = new LatLong(LATITUDE, LONGITUDE, true);
			Assert.assertEquals(LATITUDE, latLong.latitude, 0);
			Assert.assertEquals(LONGITUDE, latLong.longitude, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serializeTest() throws java.io.IOException, ClassNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void serializeTest()
		{
			LatLong latLong = new LatLong(LATITUDE, LONGITUDE, true);
			TestUtils.serializeTest(latLong);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toStringTest()
		public virtual void toStringTest()
		{
			LatLong latLong = new LatLong(LATITUDE, LONGITUDE, true);
			Assert.assertEquals(GEO_POINT_TO_STRING, latLong.ToString());
		}
	}

}