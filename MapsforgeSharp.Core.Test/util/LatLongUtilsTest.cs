/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 devemux86
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

namespace org.mapsforge.core.util
{
    using NUnit.Framework;
    using System;
    using System.Runtime.InteropServices;

    using BoundingBox = MapsforgeSharp.Core.Model.BoundingBox;
    using Dimension = MapsforgeSharp.Core.Model.Dimension;
    using LatLong = MapsforgeSharp.Core.Model.LatLong;

    public class LatLongUtilsTest
	{
		private const double DEGREES = 123.456789;
		private const string DELIMITER = ",";
		private const double LATITUDE = 1.0;
		private const double LONGITUDE = 2.0;
		private const int MICRO_DEGREES = 123456789;

        /// <summary>
        /// http://realtimemadness.blogspot.de/2012/06/nextafter-in-c-without-allocations-of.html
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        struct DoubleIntUnion
        {
            [FieldOffset(0)]
            public long l;
            [FieldOffset(0)]
            public double d;
        }

        /// <summary>
        /// Returns the next double after x in the direction of y.
        /// http://realtimemadness.blogspot.de/2012/06/nextafter-in-c-without-allocations-of.html
        /// </summary>
        private double NextAfter(double x, double y)
        {
            if (double.IsNaN(x) || double.IsNaN(y))
            {
                return x + y;
            }

            if (x == y)
            {
                return y;  // nextafter(0, -0) = -0
            }

            DoubleIntUnion u;

            u.l = 0;
            u.d = x;  // shut up the compiler

            if (x == 0)
            {
                u.l = 1;
                return y > 0 ? u.d : -u.d;
            }

            if ((x > 0) == (y > x))
                u.l++;
            else
                u.l--;

            return u.d;
        }

        private static void VerifyInvalid(string @string)
		{
			try
			{
				LatLongUtils.FromString(@string);
				Assert.Fail(@string);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

		private static void VerifyInvalidLatitude(double latitude)
		{
			try
			{
				LatLongUtils.ValidateLatitude(latitude);
				Assert.Fail("latitude: " + latitude);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

		private static void VerifyInvalidLongitude(double longitude)
		{
			try
			{
				LatLongUtils.ValidateLongitude(longitude);
				Assert.Fail("longitude: " + longitude);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

        [Test()]
		public virtual void DoubleToIntTest()
		{
			int microdegrees = LatLongUtils.DegreesToMicrodegrees(DEGREES);
			Assert.AreEqual(MICRO_DEGREES, microdegrees);
		}

        [Test()]
        public virtual void FromStringInvalidTest()
		{
			// invalid strings
			VerifyInvalid("1,2,3");
			VerifyInvalid("1,,2");
			VerifyInvalid(",1,2");
			VerifyInvalid("1,2,");
			VerifyInvalid("1,a");
			VerifyInvalid("1,");
			VerifyInvalid("1");
			VerifyInvalid("foo");
			VerifyInvalid("");

			// invalid coordinates
			VerifyInvalid("1,-181");
			VerifyInvalid("1,181");
			VerifyInvalid("-91,2");
			VerifyInvalid("91,2");
		}

        [Test()]
        public virtual void FromStringValidTest()
		{
			LatLong latLong = LatLongUtils.FromString(LATITUDE + DELIMITER + LONGITUDE);
			Assert.AreEqual(LATITUDE, latLong.Latitude, 0);
			Assert.AreEqual(LONGITUDE, latLong.Longitude, 0);
		}

        [Test()]
        public virtual void IntToDoubleTest()
		{
			double degrees = LatLongUtils.MicrodegreesToDegrees(MICRO_DEGREES);
			Assert.AreEqual(DEGREES, degrees, 0);
		}

        [Test()]
        public virtual void ValidateLatitudeTest()
		{
			LatLongUtils.ValidateLatitude(LatLongUtils.LATITUDE_MAX);
			LatLongUtils.ValidateLatitude(LatLongUtils.LATITUDE_MIN);

			VerifyInvalidLatitude(Double.NaN);
			VerifyInvalidLatitude(NextAfter(LatLongUtils.LATITUDE_MAX, double.PositiveInfinity));
			VerifyInvalidLatitude(NextAfter(LatLongUtils.LATITUDE_MIN, double.NegativeInfinity));
		}

        [Test()]
        public virtual void ValidateLongitudeTest()
		{
			LatLongUtils.ValidateLongitude(LatLongUtils.LONGITUDE_MAX);
			LatLongUtils.ValidateLongitude(LatLongUtils.LONGITUDE_MIN);

			VerifyInvalidLongitude(Double.NaN);
			VerifyInvalidLongitude(NextAfter(LatLongUtils.LONGITUDE_MAX, double.PositiveInfinity));
			VerifyInvalidLongitude(NextAfter(LatLongUtils.LONGITUDE_MIN, double.NegativeInfinity));
		}

        [Test()]
        public virtual void ZoomForBoundsTest()
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
						Assert.AreEqual(results[i], LatLongUtils.ZoomForBounds(dimension, boundingBox, tileSize));
						++i;
					}
				}
			}
		}
	}
}