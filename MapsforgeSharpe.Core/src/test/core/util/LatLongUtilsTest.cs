using System;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 devemux86
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
namespace org.mapsforge.core.util
{

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;
	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using Dimension = org.mapsforge.core.model.Dimension;
	using LatLong = org.mapsforge.core.model.LatLong;

	public class LatLongUtilsTest
	{
		private const double DEGREES = 123.456789;
		private const string DELIMITER = ",";
		private const double LATITUDE = 1.0;
		private const double LONGITUDE = 2.0;
		private const int MICRO_DEGREES = 123456789;

		private static void verifyInvalid(string @string)
		{
			try
			{
				LatLongUtils.fromString(@string);
				Assert.fail(@string);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

		private static void verifyInvalidLatitude(double latitude)
		{
			try
			{
				LatLongUtils.validateLatitude(latitude);
				Assert.fail("latitude: " + latitude);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

		private static void verifyInvalidLongitude(double longitude)
		{
			try
			{
				LatLongUtils.validateLongitude(longitude);
				Assert.fail("longitude: " + longitude);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doubleToIntTest()
		public virtual void doubleToIntTest()
		{
			int microdegrees = LatLongUtils.degreesToMicrodegrees(DEGREES);
			Assert.assertEquals(MICRO_DEGREES, microdegrees);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fromStringInvalidTest()
		public virtual void fromStringInvalidTest()
		{
			// invalid strings
			verifyInvalid("1,2,3");
			verifyInvalid("1,,2");
			verifyInvalid(",1,2");
			verifyInvalid("1,2,");
			verifyInvalid("1,a");
			verifyInvalid("1,");
			verifyInvalid("1");
			verifyInvalid("foo");
			verifyInvalid("");

			// invalid coordinates
			verifyInvalid("1,-181");
			verifyInvalid("1,181");
			verifyInvalid("-91,2");
			verifyInvalid("91,2");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fromStringValidTest()
		public virtual void fromStringValidTest()
		{
			LatLong latLong = LatLongUtils.fromString(LATITUDE + DELIMITER + LONGITUDE);
			Assert.assertEquals(LATITUDE, latLong.latitude, 0);
			Assert.assertEquals(LONGITUDE, latLong.longitude, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void intToDoubleTest()
		public virtual void intToDoubleTest()
		{
			double degrees = LatLongUtils.microdegreesToDegrees(MICRO_DEGREES);
			Assert.assertEquals(DEGREES, degrees, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateLatitudeTest()
		public virtual void validateLatitudeTest()
		{
			LatLongUtils.validateLatitude(LatLongUtils.LATITUDE_MAX);
			LatLongUtils.validateLatitude(LatLongUtils.LATITUDE_MIN);

			verifyInvalidLatitude(Double.NaN);
			verifyInvalidLatitude(Math.nextAfter(LatLongUtils.LATITUDE_MAX, double.PositiveInfinity));
			verifyInvalidLatitude(Math.nextAfter(LatLongUtils.LATITUDE_MIN, double.NegativeInfinity));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateLongitudeTest()
		public virtual void validateLongitudeTest()
		{
			LatLongUtils.validateLongitude(LatLongUtils.LONGITUDE_MAX);
			LatLongUtils.validateLongitude(LatLongUtils.LONGITUDE_MIN);

			verifyInvalidLongitude(Double.NaN);
			verifyInvalidLongitude(Math.nextAfter(LatLongUtils.LONGITUDE_MAX, double.PositiveInfinity));
			verifyInvalidLongitude(Math.nextAfter(LatLongUtils.LONGITUDE_MIN, double.NegativeInfinity));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void zoomForBoundsTest()
		public virtual void zoomForBoundsTest()
		{
			// TODO rewrite this unit tests to make it easier to understand
			Dimension[] dimensions = new Dimension[]
			{
				new Dimension(200, 300),
				new Dimension(500, 400),
				new Dimension(1000, 600),
				new Dimension(3280, 1780),
				new Dimension(100, 200),
				new Dimension(500, 200)
			};
			BoundingBox[] boundingBoxes = new BoundingBox[]
			{
				new BoundingBox(12.2, 0, 34.3, 120),
				new BoundingBox(-30, 20, 30, 30),
				new BoundingBox(20.3, 100, 30.4, 120),
				new BoundingBox(4.4, 2, 4.5, 2.2),
				new BoundingBox(50.43, 12.23, 50.44, 12.24),
				new BoundingBox(50.43, 12, 50.44, 40)
			};
			int[] tileSizes = new int[] {256, 512, 500, 620, 451};

			sbyte[] results = new sbyte[] {1, 0, 0, 0, 0, 2, 1, 1, 1, 1, 3, 2, 2, 2, 2, 10, 9, 9, 9, 9, 14, 13, 13, 13, 13, 3, 2, 2, 2, 2, 2, 1, 1, 1, 1, 3, 2, 2, 1, 2, 5, 4, 4, 3, 4, 11, 10, 10, 10, 10, 15, 14, 14, 13, 14, 4, 3, 3, 3, 3, 3, 2, 2, 2, 2, 3, 2, 2, 2, 2, 6, 5, 5, 4, 5, 12, 11, 11, 11, 11, 15, 14, 14, 14, 14, 5, 4, 4, 4, 4, 5, 4, 4, 3, 4, 5, 4, 4, 4, 4, 7, 6, 6, 6, 6, 14, 13, 13, 13, 13, 17, 16, 16, 16, 16, 7, 6, 6, 6, 6, 0, 0, 0, 0, 0, 2, 1, 1, 0, 1, 2, 1, 1, 1, 1, 9, 8, 8, 8, 8, 13, 12, 12, 12, 12, 2, 1, 1, 1, 1, 2, 1, 1, 1, 1, 2, 1, 1, 0, 1, 4, 3, 3, 3, 3, 11, 10, 10, 10, 10, 14, 13, 13, 12, 13, 4, 3, 3, 3, 3};

			int i = 0;
			foreach (Dimension dimension in dimensions)
			{
				foreach (BoundingBox boundingBox in boundingBoxes)
				{
					foreach (int tileSize in tileSizes)
					{
						Assert.assertEquals(results[i], LatLongUtils.zoomForBounds(dimension, boundingBox, tileSize));
						++i;
					}
				}
			}
		}
	}

}