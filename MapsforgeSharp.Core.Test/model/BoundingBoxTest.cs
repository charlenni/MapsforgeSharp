/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Christian Pesch
 * Copyright 2015 devemux86
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

    public class BoundingBoxTest
    {
		private const string DELIMITER = ",";
		private const double MAX_LATITUDE = 4.0;
		private const double MAX_LONGITUDE = 3.0;
		private const double MIN_LATITUDE = 2.0;
		private const double MIN_LONGITUDE = 1.0;
        private string BOUNDING_BOX_TO_STRING = string.Format("minLatitude={0}, minLongitude={1}, maxLatitude={2}, maxLongitude={3}", new object[] { MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE });

        private static void AssertIntersection(BoundingBox boundingBox1, BoundingBox boundingBox2)
		{
			Assert.True(boundingBox1.Intersects(boundingBox2));
			Assert.True(boundingBox2.Intersects(boundingBox1));
		}

		private static void AssertNoIntersection(BoundingBox boundingBox1, BoundingBox boundingBox2)
		{
			Assert.False(boundingBox1.Intersects(boundingBox2));
			Assert.False(boundingBox2.Intersects(boundingBox1));
		}

		private static void VerifyInvalid(string @string)
		{
			try
			{
				BoundingBox.FromString(@string);
				Assert.Fail(@string);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

        [Test()]
		public virtual void ContainsCoordinatesTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);

			Assert.True(boundingBox.Contains(MIN_LATITUDE, MIN_LONGITUDE));
			Assert.True(boundingBox.Contains(MAX_LATITUDE, MAX_LONGITUDE));
			Assert.False(boundingBox.Contains(MIN_LONGITUDE, MIN_LONGITUDE));
			Assert.False(boundingBox.Contains(MAX_LATITUDE, MAX_LATITUDE));
		}

        [Test()]
        public virtual void ContainsLatLongTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			LatLong latLong1 = new LatLong(MIN_LATITUDE, MIN_LONGITUDE, true);
			LatLong latLong2 = new LatLong(MAX_LATITUDE, MAX_LONGITUDE, true);
			LatLong latLong3 = new LatLong(MIN_LONGITUDE, MIN_LONGITUDE, true);
			LatLong latLong4 = new LatLong(MAX_LATITUDE, MAX_LATITUDE, true);

			Assert.True(boundingBox.Contains(latLong1));
			Assert.True(boundingBox.Contains(latLong2));
			Assert.False(boundingBox.Contains(latLong3));
			Assert.False(boundingBox.Contains(latLong4));
		}

        [Test()]
        public virtual void EqualsTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MAX_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox4 = new BoundingBox(MIN_LATITUDE, MAX_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox5 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MIN_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox6 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MIN_LONGITUDE);

			TestUtils.EqualsTest(boundingBox1, boundingBox2);

			TestUtils.NotEqualsTest(boundingBox1, boundingBox3);
			TestUtils.NotEqualsTest(boundingBox1, boundingBox4);
			TestUtils.NotEqualsTest(boundingBox1, boundingBox5);
			TestUtils.NotEqualsTest(boundingBox1, boundingBox6);
			TestUtils.NotEqualsTest(boundingBox1, new object());
			TestUtils.NotEqualsTest(boundingBox1, null);
		}

        [Test()]
        public virtual void ExtendBoundingBoxTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);

			Assert.AreEqual(boundingBox1, boundingBox1.ExtendBoundingBox(boundingBox1));
			Assert.AreEqual(boundingBox2, boundingBox1.ExtendBoundingBox(boundingBox2));
			Assert.AreEqual(boundingBox3, boundingBox1.ExtendBoundingBox(boundingBox3));
		}

        [Test()]
        public virtual void ExtendCoordinatesTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);

			Assert.AreEqual(boundingBox1, boundingBox1.ExtendCoordinates(MIN_LATITUDE, MIN_LONGITUDE));
			Assert.AreEqual(boundingBox2, boundingBox2.ExtendCoordinates(MAX_LATITUDE, MAX_LONGITUDE));
			Assert.AreEqual(boundingBox3, boundingBox3.ExtendCoordinates(boundingBox3.CenterPoint.Latitude, boundingBox3.CenterPoint.Longitude));

			Assert.True(boundingBox1.ExtendCoordinates(MIN_LATITUDE - 1, MAX_LONGITUDE + 1).Contains(MIN_LATITUDE - 1, MAX_LONGITUDE + 1));
			Assert.True(boundingBox1.ExtendCoordinates(MAX_LATITUDE + 1, MAX_LONGITUDE + 1).Contains(MAX_LATITUDE + 1, MAX_LONGITUDE + 1));
			Assert.True(boundingBox1.ExtendCoordinates(MAX_LATITUDE + 1, MIN_LONGITUDE - 1).Contains(MAX_LATITUDE + 1, MIN_LONGITUDE - 1));
			Assert.True(boundingBox1.ExtendCoordinates(MIN_LATITUDE - 1, MIN_LONGITUDE - 1).Contains(MIN_LATITUDE - 1, MIN_LONGITUDE - 1));
		}

        [Test()]
        public virtual void ExtendDegreesTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);

			Assert.AreEqual(boundingBox1, boundingBox1.ExtendDegrees(0, 0));
			Assert.AreEqual(boundingBox2, boundingBox2.ExtendDegrees(0, 0));
			Assert.AreEqual(boundingBox3, boundingBox3.ExtendDegrees(0, 0));

			Assert.True(boundingBox1.ExtendDegrees(1, 1).Contains(new LatLong(MIN_LATITUDE - 1, MAX_LONGITUDE + 1)));
			Assert.True(boundingBox1.ExtendDegrees(1, 1).Contains(new LatLong(MAX_LATITUDE + 1, MAX_LONGITUDE + 1)));
			Assert.True(boundingBox1.ExtendDegrees(1, 1).Contains(new LatLong(MAX_LATITUDE + 1, MIN_LONGITUDE - 1)));
			Assert.True(boundingBox1.ExtendDegrees(1, 1).Contains(new LatLong(MIN_LATITUDE - 1, MIN_LONGITUDE - 1)));
		}

        [Test()]
        public virtual void ExtendLatLongTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);

			Assert.AreEqual(boundingBox1, boundingBox1.ExtendCoordinates(new LatLong(MIN_LATITUDE, MIN_LONGITUDE)));
			Assert.AreEqual(boundingBox2, boundingBox2.ExtendCoordinates(new LatLong(MAX_LATITUDE, MAX_LONGITUDE)));
			Assert.AreEqual(boundingBox3, boundingBox3.ExtendCoordinates(new LatLong(boundingBox3.CenterPoint.Latitude, boundingBox3.CenterPoint.Longitude)));

			LatLong latLong = new LatLong(MIN_LATITUDE - 1, MAX_LONGITUDE + 1);
			Assert.True(boundingBox1.ExtendCoordinates(latLong).Contains(latLong));
			latLong = new LatLong(MAX_LATITUDE + 1, MAX_LONGITUDE + 1);
			Assert.True(boundingBox1.ExtendCoordinates(latLong).Contains(latLong));
			latLong = new LatLong(MAX_LATITUDE + 1, MIN_LONGITUDE - 1);
			Assert.True(boundingBox1.ExtendCoordinates(latLong).Contains(latLong));
			latLong = new LatLong(MIN_LATITUDE - 1, MIN_LONGITUDE - 1);
			Assert.True(boundingBox1.ExtendCoordinates(latLong).Contains(latLong));
		}

        [Test()]
        public virtual void ExtendMetersTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);

			Assert.AreEqual(boundingBox1, boundingBox1.ExtendMeters(0));
			Assert.AreEqual(boundingBox2, boundingBox2.ExtendMeters(0));
			Assert.AreEqual(boundingBox3, boundingBox3.ExtendMeters(0));

			Assert.True(boundingBox1.ExtendMeters(20).Contains(new LatLong(MIN_LATITUDE, MAX_LONGITUDE)));
			Assert.True(boundingBox1.ExtendMeters(20).Contains(new LatLong(MAX_LATITUDE, MAX_LONGITUDE)));
			Assert.True(boundingBox1.ExtendMeters(20).Contains(new LatLong(MAX_LATITUDE, MIN_LONGITUDE)));
			Assert.True(boundingBox1.ExtendMeters(20).Contains(new LatLong(MIN_LATITUDE, MIN_LONGITUDE)));
		}

        [Test()]
        public virtual void FieldsTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			Assert.AreEqual(MIN_LATITUDE, boundingBox.MinLatitude, 0);
			Assert.AreEqual(MIN_LONGITUDE, boundingBox.MinLongitude, 0);
			Assert.AreEqual(MAX_LATITUDE, boundingBox.MaxLatitude, 0);
			Assert.AreEqual(MAX_LONGITUDE, boundingBox.MaxLongitude, 0);
		}

        [Test()]
        public virtual void FromStringInvalidTest()
		{
			// invalid strings
			VerifyInvalid("1,2,3,4,5");
			VerifyInvalid("1,2,3,,4");
			VerifyInvalid(",1,2,3,4");
			VerifyInvalid("1,2,3,4,");
			VerifyInvalid("1,2,3,a");
			VerifyInvalid("1,2,3,");
			VerifyInvalid("1,2,3");
			VerifyInvalid("foo");
			VerifyInvalid("");

			// invalid coordinates
			VerifyInvalid("1,-181,3,4");
			VerifyInvalid("1,2,3,181");
			VerifyInvalid("-91,2,3,4");
			VerifyInvalid("1,2,91,4");
			VerifyInvalid("3,2,1,4");
			VerifyInvalid("1,4,3,2");
		}

        [Test()]
        public virtual void FromStringValidTest()
		{
			string boundingBoxString = MIN_LATITUDE + DELIMITER + MIN_LONGITUDE + DELIMITER + MAX_LATITUDE + DELIMITER + MAX_LONGITUDE;
			BoundingBox boundingBox = BoundingBox.FromString(boundingBoxString);
			Assert.AreEqual(MIN_LATITUDE, boundingBox.MinLatitude, 0);
			Assert.AreEqual(MIN_LONGITUDE, boundingBox.MinLongitude, 0);
			Assert.AreEqual(MAX_LATITUDE, boundingBox.MaxLatitude, 0);
			Assert.AreEqual(MAX_LONGITUDE, boundingBox.MaxLongitude, 0);
		}

        [Test()]
        public virtual void GetCenterPointTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			LatLong centerPoint = boundingBox.CenterPoint;
			Assert.AreEqual((MIN_LATITUDE + MAX_LATITUDE) / 2, centerPoint.Latitude, 0);
			Assert.AreEqual((MIN_LONGITUDE + MAX_LONGITUDE) / 2, centerPoint.Longitude, 0);
		}

        [Test()]
        public virtual void GetLatitudeSpanTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			Assert.AreEqual(MAX_LATITUDE - MIN_LATITUDE, boundingBox.LatitudeSpan, 0);
		}

        [Test()]
        public virtual void GetLongitudeSpanTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			Assert.AreEqual(MAX_LONGITUDE - MIN_LONGITUDE, boundingBox.LongitudeSpan, 0);
		}

        [Test()]
        public virtual void IntersectsTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(0, 0, MIN_LATITUDE, MIN_LONGITUDE);
			BoundingBox boundingBox4 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);
			BoundingBox boundingBox5 = new BoundingBox(0, 0, 0, 0);
			BoundingBox boundingBox6 = new BoundingBox(-4, -3, -2, -1);

			AssertIntersection(boundingBox1, boundingBox1);
			AssertIntersection(boundingBox1, boundingBox2);
			AssertIntersection(boundingBox1, boundingBox3);
			AssertIntersection(boundingBox1, boundingBox4);

			AssertNoIntersection(boundingBox1, boundingBox5);
			AssertNoIntersection(boundingBox1, boundingBox6);
			AssertNoIntersection(boundingBox5, boundingBox6);
		}

        [Test()]
        public virtual void SerializeTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			TestUtils.SerializeTest(boundingBox);
		}

        [Test()]
        public virtual void ToStringTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			Assert.AreEqual(BOUNDING_BOX_TO_STRING, boundingBox.ToString());
		}
	}
}