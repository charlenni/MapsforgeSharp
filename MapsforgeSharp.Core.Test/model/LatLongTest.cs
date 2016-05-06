/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2016 Dirk Weltz
 * Copyright 2016 Michael Oed
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

namespace MapsforgeSharp.Core.Model
{
    using NUnit.Framework;

    public class LatLongTest
	{
		private const double LATITUDE = 1.0;
		private const double LONGITUDE = 2.0;
        private string GEO_POINT_TO_STRING = string.Format("latitude={0}, longitude={1}", LATITUDE, LONGITUDE);

        [Test()]
		public virtual void CompareToTest()
		{
			LatLong latLong1 = new LatLong(LATITUDE, LONGITUDE, true);
			LatLong latLong2 = new LatLong(LATITUDE, LONGITUDE, true);
			LatLong latLong3 = new LatLong(LATITUDE, LATITUDE, true);
			LatLong latLong4 = new LatLong(LONGITUDE, LONGITUDE, true);

			Assert.AreEqual(0, latLong1.CompareTo(latLong2));

			TestUtils.NotCompareToTest(latLong1, latLong3);
			TestUtils.NotCompareToTest(latLong1, latLong4);
		}

        [Test()]
        public virtual void EqualsTest()
		{
			LatLong latLong1 = new LatLong(LATITUDE, LONGITUDE, true);
			LatLong latLong2 = new LatLong(LATITUDE, LONGITUDE, true);
			LatLong latLong3 = new LatLong(LATITUDE, LATITUDE, true);
			LatLong latLong4 = new LatLong(LONGITUDE, LONGITUDE, true);

			TestUtils.EqualsTest(latLong1, latLong2);

			TestUtils.NotEqualsTest(latLong1, latLong3);
			TestUtils.NotEqualsTest(latLong1, latLong4);
			TestUtils.NotEqualsTest(latLong1, new object());
			TestUtils.NotEqualsTest(latLong1, null);
		}

        [Test()]
        public virtual void FieldsTest()
		{
			LatLong latLong = new LatLong(LATITUDE, LONGITUDE, true);
			Assert.AreEqual(LATITUDE, latLong.Latitude, 0);
			Assert.AreEqual(LONGITUDE, latLong.Longitude, 0);
		}

        [Test()]
        public virtual void SerializeTest()
		{
			LatLong latLong = new LatLong(LATITUDE, LONGITUDE, true);
			TestUtils.SerializeTest(latLong);
		}

        [Test()]
        public virtual void ToStringTest()
		{
			LatLong latLong = new LatLong(LATITUDE, LONGITUDE, true);
			Assert.AreEqual(GEO_POINT_TO_STRING, latLong.ToString());
		}
	}
}