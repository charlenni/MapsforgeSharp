/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Christian Pesch
 * Copyright 2015 devemux86
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

	public class BoundingBoxTest
	{
		private const string BOUNDING_BOX_TO_STRING = "minLatitude=2.0, minLongitude=1.0, maxLatitude=4.0, maxLongitude=3.0";
		private const string DELIMITER = ",";
		private const double MAX_LATITUDE = 4.0;
		private const double MAX_LONGITUDE = 3.0;
		private const double MIN_LATITUDE = 2.0;
		private const double MIN_LONGITUDE = 1.0;

		private static void assertIntersection(BoundingBox boundingBox1, BoundingBox boundingBox2)
		{
			Assert.assertTrue(boundingBox1.intersects(boundingBox2));
			Assert.assertTrue(boundingBox2.intersects(boundingBox1));
		}

		private static void assertNoIntersection(BoundingBox boundingBox1, BoundingBox boundingBox2)
		{
			Assert.assertFalse(boundingBox1.intersects(boundingBox2));
			Assert.assertFalse(boundingBox2.intersects(boundingBox1));
		}

		private static void verifyInvalid(string @string)
		{
			try
			{
				BoundingBox.fromString(@string);
				Assert.fail(@string);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void containsCoordinatesTest()
		public virtual void containsCoordinatesTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);

			Assert.assertTrue(boundingBox.contains(MIN_LATITUDE, MIN_LONGITUDE));
			Assert.assertTrue(boundingBox.contains(MAX_LATITUDE, MAX_LONGITUDE));
			Assert.assertFalse(boundingBox.contains(MIN_LONGITUDE, MIN_LONGITUDE));
			Assert.assertFalse(boundingBox.contains(MAX_LATITUDE, MAX_LATITUDE));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void containsLatLongTest()
		public virtual void containsLatLongTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			LatLong latLong1 = new LatLong(MIN_LATITUDE, MIN_LONGITUDE, true);
			LatLong latLong2 = new LatLong(MAX_LATITUDE, MAX_LONGITUDE, true);
			LatLong latLong3 = new LatLong(MIN_LONGITUDE, MIN_LONGITUDE, true);
			LatLong latLong4 = new LatLong(MAX_LATITUDE, MAX_LATITUDE, true);

			Assert.assertTrue(boundingBox.contains(latLong1));
			Assert.assertTrue(boundingBox.contains(latLong2));
			Assert.assertFalse(boundingBox.contains(latLong3));
			Assert.assertFalse(boundingBox.contains(latLong4));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsTest()
		public virtual void equalsTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MAX_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox4 = new BoundingBox(MIN_LATITUDE, MAX_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox5 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MIN_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox6 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MIN_LONGITUDE);

			TestUtils.equalsTest(boundingBox1, boundingBox2);

			TestUtils.notEqualsTest(boundingBox1, boundingBox3);
			TestUtils.notEqualsTest(boundingBox1, boundingBox4);
			TestUtils.notEqualsTest(boundingBox1, boundingBox5);
			TestUtils.notEqualsTest(boundingBox1, boundingBox6);
			TestUtils.notEqualsTest(boundingBox1, new object());
			TestUtils.notEqualsTest(boundingBox1, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void extendBoundingBoxTest()
		public virtual void extendBoundingBoxTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);

			Assert.assertEquals(boundingBox1, boundingBox1.extendBoundingBox(boundingBox1));
			Assert.assertEquals(boundingBox2, boundingBox1.extendBoundingBox(boundingBox2));
			Assert.assertEquals(boundingBox3, boundingBox1.extendBoundingBox(boundingBox3));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void extendCoordinatesTest()
		public virtual void extendCoordinatesTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);

			Assert.assertEquals(boundingBox1, boundingBox1.extendCoordinates(MIN_LATITUDE, MIN_LONGITUDE));
			Assert.assertEquals(boundingBox2, boundingBox2.extendCoordinates(MAX_LATITUDE, MAX_LONGITUDE));
			Assert.assertEquals(boundingBox3, boundingBox3.extendCoordinates(boundingBox3.CenterPoint.latitude, boundingBox3.CenterPoint.longitude));

			Assert.assertTrue(boundingBox1.extendCoordinates(MIN_LATITUDE - 1, MAX_LONGITUDE + 1).contains(MIN_LATITUDE - 1, MAX_LONGITUDE + 1));
			Assert.assertTrue(boundingBox1.extendCoordinates(MAX_LATITUDE + 1, MAX_LONGITUDE + 1).contains(MAX_LATITUDE + 1, MAX_LONGITUDE + 1));
			Assert.assertTrue(boundingBox1.extendCoordinates(MAX_LATITUDE + 1, MIN_LONGITUDE - 1).contains(MAX_LATITUDE + 1, MIN_LONGITUDE - 1));
			Assert.assertTrue(boundingBox1.extendCoordinates(MIN_LATITUDE - 1, MIN_LONGITUDE - 1).contains(MIN_LATITUDE - 1, MIN_LONGITUDE - 1));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void extendDegreesTest()
		public virtual void extendDegreesTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);

			Assert.assertEquals(boundingBox1, boundingBox1.extendDegrees(0, 0));
			Assert.assertEquals(boundingBox2, boundingBox2.extendDegrees(0, 0));
			Assert.assertEquals(boundingBox3, boundingBox3.extendDegrees(0, 0));

			Assert.assertTrue(boundingBox1.extendDegrees(1, 1).contains(new LatLong(MIN_LATITUDE - 1, MAX_LONGITUDE + 1)));
			Assert.assertTrue(boundingBox1.extendDegrees(1, 1).contains(new LatLong(MAX_LATITUDE + 1, MAX_LONGITUDE + 1)));
			Assert.assertTrue(boundingBox1.extendDegrees(1, 1).contains(new LatLong(MAX_LATITUDE + 1, MIN_LONGITUDE - 1)));
			Assert.assertTrue(boundingBox1.extendDegrees(1, 1).contains(new LatLong(MIN_LATITUDE - 1, MIN_LONGITUDE - 1)));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void extendLatLongTest()
		public virtual void extendLatLongTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);

			Assert.assertEquals(boundingBox1, boundingBox1.extendCoordinates(new LatLong(MIN_LATITUDE, MIN_LONGITUDE)));
			Assert.assertEquals(boundingBox2, boundingBox2.extendCoordinates(new LatLong(MAX_LATITUDE, MAX_LONGITUDE)));
			Assert.assertEquals(boundingBox3, boundingBox3.extendCoordinates(new LatLong(boundingBox3.CenterPoint.latitude, boundingBox3.CenterPoint.longitude)));

			LatLong latLong = new LatLong(MIN_LATITUDE - 1, MAX_LONGITUDE + 1);
			Assert.assertTrue(boundingBox1.extendCoordinates(latLong).contains(latLong));
			latLong = new LatLong(MAX_LATITUDE + 1, MAX_LONGITUDE + 1);
			Assert.assertTrue(boundingBox1.extendCoordinates(latLong).contains(latLong));
			latLong = new LatLong(MAX_LATITUDE + 1, MIN_LONGITUDE - 1);
			Assert.assertTrue(boundingBox1.extendCoordinates(latLong).contains(latLong));
			latLong = new LatLong(MIN_LATITUDE - 1, MIN_LONGITUDE - 1);
			Assert.assertTrue(boundingBox1.extendCoordinates(latLong).contains(latLong));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void extendMetersTest()
		public virtual void extendMetersTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);

			Assert.assertEquals(boundingBox1, boundingBox1.extendMeters(0));
			Assert.assertEquals(boundingBox2, boundingBox2.extendMeters(0));
			Assert.assertEquals(boundingBox3, boundingBox3.extendMeters(0));

			Assert.assertTrue(boundingBox1.extendMeters(20).contains(new LatLong(MIN_LATITUDE, MAX_LONGITUDE)));
			Assert.assertTrue(boundingBox1.extendMeters(20).contains(new LatLong(MAX_LATITUDE, MAX_LONGITUDE)));
			Assert.assertTrue(boundingBox1.extendMeters(20).contains(new LatLong(MAX_LATITUDE, MIN_LONGITUDE)));
			Assert.assertTrue(boundingBox1.extendMeters(20).contains(new LatLong(MIN_LATITUDE, MIN_LONGITUDE)));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fieldsTest()
		public virtual void fieldsTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			Assert.assertEquals(MIN_LATITUDE, boundingBox.minLatitude, 0);
			Assert.assertEquals(MIN_LONGITUDE, boundingBox.minLongitude, 0);
			Assert.assertEquals(MAX_LATITUDE, boundingBox.maxLatitude, 0);
			Assert.assertEquals(MAX_LONGITUDE, boundingBox.maxLongitude, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fromStringInvalidTest()
		public virtual void fromStringInvalidTest()
		{
			// invalid strings
			verifyInvalid("1,2,3,4,5");
			verifyInvalid("1,2,3,,4");
			verifyInvalid(",1,2,3,4");
			verifyInvalid("1,2,3,4,");
			verifyInvalid("1,2,3,a");
			verifyInvalid("1,2,3,");
			verifyInvalid("1,2,3");
			verifyInvalid("foo");
			verifyInvalid("");

			// invalid coordinates
			verifyInvalid("1,-181,3,4");
			verifyInvalid("1,2,3,181");
			verifyInvalid("-91,2,3,4");
			verifyInvalid("1,2,91,4");
			verifyInvalid("3,2,1,4");
			verifyInvalid("1,4,3,2");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fromStringValidTest()
		public virtual void fromStringValidTest()
		{
			string boundingBoxString = MIN_LATITUDE + DELIMITER + MIN_LONGITUDE + DELIMITER + MAX_LATITUDE + DELIMITER + MAX_LONGITUDE;
			BoundingBox boundingBox = BoundingBox.fromString(boundingBoxString);
			Assert.assertEquals(MIN_LATITUDE, boundingBox.minLatitude, 0);
			Assert.assertEquals(MIN_LONGITUDE, boundingBox.minLongitude, 0);
			Assert.assertEquals(MAX_LATITUDE, boundingBox.maxLatitude, 0);
			Assert.assertEquals(MAX_LONGITUDE, boundingBox.maxLongitude, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getCenterPointTest()
		public virtual void getCenterPointTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			LatLong centerPoint = boundingBox.CenterPoint;
			Assert.assertEquals((MIN_LATITUDE + MAX_LATITUDE) / 2, centerPoint.latitude, 0);
			Assert.assertEquals((MIN_LONGITUDE + MAX_LONGITUDE) / 2, centerPoint.longitude, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getLatitudeSpanTest()
		public virtual void getLatitudeSpanTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			Assert.assertEquals(MAX_LATITUDE - MIN_LATITUDE, boundingBox.LatitudeSpan, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getLongitudeSpanTest()
		public virtual void getLongitudeSpanTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			Assert.assertEquals(MAX_LONGITUDE - MIN_LONGITUDE, boundingBox.LongitudeSpan, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void intersectsTest()
		public virtual void intersectsTest()
		{
			BoundingBox boundingBox1 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox2 = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			BoundingBox boundingBox3 = new BoundingBox(0, 0, MIN_LATITUDE, MIN_LONGITUDE);
			BoundingBox boundingBox4 = new BoundingBox(MIN_LATITUDE - 1, MIN_LONGITUDE - 1, MAX_LATITUDE + 1, MAX_LONGITUDE + 1);
			BoundingBox boundingBox5 = new BoundingBox(0, 0, 0, 0);
			BoundingBox boundingBox6 = new BoundingBox(-4, -3, -2, -1);

			assertIntersection(boundingBox1, boundingBox1);
			assertIntersection(boundingBox1, boundingBox2);
			assertIntersection(boundingBox1, boundingBox3);
			assertIntersection(boundingBox1, boundingBox4);

			assertNoIntersection(boundingBox1, boundingBox5);
			assertNoIntersection(boundingBox1, boundingBox6);
			assertNoIntersection(boundingBox5, boundingBox6);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serializeTest() throws java.io.IOException, ClassNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void serializeTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			TestUtils.serializeTest(boundingBox);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toStringTest()
		public virtual void toStringTest()
		{
			BoundingBox boundingBox = new BoundingBox(MIN_LATITUDE, MIN_LONGITUDE, MAX_LATITUDE, MAX_LONGITUDE);
			Assert.assertEquals(BOUNDING_BOX_TO_STRING, boundingBox.ToString());
		}
	}

}