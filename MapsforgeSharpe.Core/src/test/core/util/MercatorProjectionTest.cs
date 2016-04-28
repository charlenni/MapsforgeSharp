using System;

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
namespace org.mapsforge.core.util
{

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;
	using Tile = org.mapsforge.core.model.Tile;

	public class MercatorProjectionTest
	{
		private static readonly int[] TILE_SIZES = new int[] {256, 128, 376, 512, 100};
		private const int ZOOM_LEVEL_MAX = 30;
		private const int ZOOM_LEVEL_MIN = 0;

		private static void verifyInvalidGetMapSize(sbyte zoomLevel, int tileSize)
		{
			try
			{
				MercatorProjection.getMapSize(zoomLevel, tileSize);
				Assert.fail("zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

		private static void verifyInvalidPixelXToLongitude(double pixelX, sbyte zoomLevel, int tileSize)
		{
			try
			{
				MercatorProjection.pixelXToLongitude(pixelX, MercatorProjection.getMapSize(zoomLevel, tileSize));
				Assert.fail("pixelX: " + pixelX + ", zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

		private static void verifyInvalidPixelYToLatitude(double pixelY, sbyte zoomLevel, int tileSize)
		{
			try
			{
				MercatorProjection.pixelYToLatitude(pixelY, MercatorProjection.getMapSize(zoomLevel, tileSize));
				Assert.fail("pixelY: " + pixelY + ", zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getMapSizeTest()
		public virtual void getMapSizeTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					long factor = Math.Round(MercatorProjection.zoomLevelToScaleFactor(zoomLevel));
					Assert.assertEquals(tileSize * factor, MercatorProjection.getMapSize(zoomLevel, tileSize));
					Assert.assertEquals(MercatorProjection.getMapSizeWithScaleFactor(MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize), MercatorProjection.getMapSize(zoomLevel, tileSize));
				}
				verifyInvalidGetMapSize((sbyte) -1, tileSize);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void latitudeToPixelYTest()
		public virtual void latitudeToPixelYTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					long mapSize = MercatorProjection.getMapSize(zoomLevel, tileSize);
					double pixelY = MercatorProjection.latitudeToPixelY(MercatorProjection.LATITUDE_MAX, mapSize);
					Assert.assertEquals(0, pixelY, 0);

					pixelY = MercatorProjection.latitudeToPixelYWithScaleFactor(MercatorProjection.LATITUDE_MAX, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals(0, pixelY, 0);

					pixelY = MercatorProjection.latitudeToPixelY(0, mapSize);
					Assert.assertEquals((float) mapSize / 2, pixelY, 0);
					pixelY = MercatorProjection.latitudeToPixelYWithScaleFactor(0, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals((float) mapSize / 2, pixelY, 0);

					pixelY = MercatorProjection.latitudeToPixelY(MercatorProjection.LATITUDE_MIN, mapSize);
					Assert.assertEquals(mapSize, pixelY, 0);
					pixelY = MercatorProjection.latitudeToPixelYWithScaleFactor(MercatorProjection.LATITUDE_MIN, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals(mapSize, pixelY, 0);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void latitudeToTileYTest()
		public virtual void latitudeToTileYTest()
		{
			for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
				long tileY = MercatorProjection.latitudeToTileY(MercatorProjection.LATITUDE_MAX, zoomLevel);
				Assert.assertEquals(0, tileY);
				tileY = MercatorProjection.latitudeToTileY(MercatorProjection.LATITUDE_MAX, MercatorProjection.zoomLevelToScaleFactor(zoomLevel));
				Assert.assertEquals(0, tileY);

				tileY = MercatorProjection.latitudeToTileY(MercatorProjection.LATITUDE_MIN, zoomLevel);
				Assert.assertEquals(Tile.getMaxTileNumber(zoomLevel), tileY);
				tileY = MercatorProjection.latitudeToTileY(MercatorProjection.LATITUDE_MIN, MercatorProjection.zoomLevelToScaleFactor(zoomLevel));
				Assert.assertEquals(Tile.getMaxTileNumber(zoomLevel), tileY);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void longitudeToPixelXTest()
		public virtual void longitudeToPixelXTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					long mapSize = MercatorProjection.getMapSize(zoomLevel, tileSize);
					double pixelX = MercatorProjection.longitudeToPixelX(LatLongUtils.LONGITUDE_MIN, mapSize);
					Assert.assertEquals(0, pixelX, 0);
					pixelX = MercatorProjection.longitudeToPixelXWithScaleFactor(LatLongUtils.LONGITUDE_MIN, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals(0, pixelX, 0);

					pixelX = MercatorProjection.longitudeToPixelX(0, mapSize);
					Assert.assertEquals((float) mapSize / 2, pixelX, 0);
					mapSize = MercatorProjection.getMapSizeWithScaleFactor(MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					pixelX = MercatorProjection.longitudeToPixelXWithScaleFactor(0, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals((float) mapSize / 2, pixelX, 0);

					pixelX = MercatorProjection.longitudeToPixelX(LatLongUtils.LONGITUDE_MAX, mapSize);
					Assert.assertEquals(mapSize, pixelX, 0);
					pixelX = MercatorProjection.longitudeToPixelXWithScaleFactor(LatLongUtils.LONGITUDE_MAX, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals(mapSize, pixelX, 0);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void longitudeToTileXTest()
		public virtual void longitudeToTileXTest()
		{
			for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
				long tileX = MercatorProjection.longitudeToTileX(LatLongUtils.LONGITUDE_MIN, zoomLevel);
				Assert.assertEquals(0, tileX);
				tileX = MercatorProjection.longitudeToTileX(LatLongUtils.LONGITUDE_MIN, MercatorProjection.zoomLevelToScaleFactor(zoomLevel));
				Assert.assertEquals(0, tileX);

				tileX = MercatorProjection.longitudeToTileX(LatLongUtils.LONGITUDE_MAX, zoomLevel);
				Assert.assertEquals(Tile.getMaxTileNumber(zoomLevel), tileX);
				tileX = MercatorProjection.longitudeToTileX(LatLongUtils.LONGITUDE_MAX, MercatorProjection.zoomLevelToScaleFactor(zoomLevel));
				Assert.assertEquals(Tile.getMaxTileNumber(zoomLevel), tileX);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void metersToPixelTest()
		public virtual void metersToPixelTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				Assert.assertTrue(MercatorProjection.metersToPixels(10, 10.0, MercatorProjection.getMapSize((sbyte) 1, tileSize)) < 1);
				Assert.assertTrue(MercatorProjection.metersToPixels((int)(40 * 10e7), 10.0, MercatorProjection.getMapSize((sbyte) 1, tileSize)) > 1);
				Assert.assertTrue(MercatorProjection.metersToPixels(10, 10.0, MercatorProjection.getMapSize((sbyte) 20, tileSize)) > 1);
				Assert.assertTrue(MercatorProjection.metersToPixels(10, 89.0, MercatorProjection.getMapSize((sbyte) 1, tileSize)) < 1);
				Assert.assertTrue(MercatorProjection.metersToPixels((int)(40 * 10e3), 50, MercatorProjection.getMapSize((sbyte) 10, tileSize)) > 1);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pixelXToLongitudeTest()
		public virtual void pixelXToLongitudeTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					long mapSize = MercatorProjection.getMapSize(zoomLevel, tileSize);
					double longitude = MercatorProjection.pixelXToLongitude(0, mapSize);
					Assert.assertEquals(LatLongUtils.LONGITUDE_MIN, longitude, 0);
					longitude = MercatorProjection.pixelXToLongitudeWithScaleFactor(0, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals(LatLongUtils.LONGITUDE_MIN, longitude, 0);

					longitude = MercatorProjection.pixelXToLongitude((float) mapSize / 2, mapSize);
					Assert.assertEquals(0, longitude, 0);
					mapSize = MercatorProjection.getMapSizeWithScaleFactor(MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					longitude = MercatorProjection.pixelXToLongitudeWithScaleFactor((float) mapSize / 2, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals(0, longitude, 0);

					longitude = MercatorProjection.pixelXToLongitude(mapSize, mapSize);
					Assert.assertEquals(LatLongUtils.LONGITUDE_MAX, longitude, 0);
					longitude = MercatorProjection.pixelXToLongitudeWithScaleFactor(mapSize, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals(LatLongUtils.LONGITUDE_MAX, longitude, 0);
				}

				verifyInvalidPixelXToLongitude(-1, (sbyte) 0, tileSize);
				verifyInvalidPixelXToLongitude(tileSize + 1, (sbyte) 0, tileSize);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pixelXToTileXTest()
		public virtual void pixelXToTileXTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					Assert.assertEquals(0, MercatorProjection.pixelXToTileX(0, zoomLevel, tileSize));
					Assert.assertEquals(0, MercatorProjection.pixelXToTileX(0, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize));
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pixelYToLatitudeTest()
		public virtual void pixelYToLatitudeTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					long mapSize = MercatorProjection.getMapSize(zoomLevel, tileSize);
					double latitude = MercatorProjection.pixelYToLatitude(0, mapSize);
					Assert.assertEquals(MercatorProjection.LATITUDE_MAX, latitude, 0);
					latitude = MercatorProjection.pixelYToLatitudeWithScaleFactor(0, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals(MercatorProjection.LATITUDE_MAX, latitude, 0);

					latitude = MercatorProjection.pixelYToLatitude((float) mapSize / 2, mapSize);
					Assert.assertEquals(0, latitude, 0);
					mapSize = MercatorProjection.getMapSizeWithScaleFactor(MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					latitude = MercatorProjection.pixelYToLatitudeWithScaleFactor((float) mapSize / 2, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals(0, latitude, 0);

					latitude = MercatorProjection.pixelYToLatitude(mapSize, mapSize);
					Assert.assertEquals(MercatorProjection.LATITUDE_MIN, latitude, 0);
					latitude = MercatorProjection.pixelYToLatitudeWithScaleFactor(mapSize, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.assertEquals(MercatorProjection.LATITUDE_MIN, latitude, 0);
				}

				verifyInvalidPixelYToLatitude(-1, (sbyte) 0, tileSize);
				verifyInvalidPixelYToLatitude(tileSize + 1, (sbyte) 0, tileSize);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pixelYToTileYTest()
		public virtual void pixelYToTileYTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					Assert.assertEquals(0, MercatorProjection.pixelYToTileY(0, zoomLevel, tileSize));
					Assert.assertEquals(0, MercatorProjection.pixelYToTileY(0, MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize));
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tileToPixelTest()
		public virtual void tileToPixelTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				Assert.assertEquals(0, MercatorProjection.tileToPixel(0, tileSize));
				Assert.assertEquals(tileSize, MercatorProjection.tileToPixel(1, tileSize));
				Assert.assertEquals(tileSize * 2, MercatorProjection.tileToPixel(2, tileSize));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tileXToLongitudeTest()
		public virtual void tileXToLongitudeTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					double longitude = MercatorProjection.tileXToLongitude(0, zoomLevel);
					Assert.assertEquals(LatLongUtils.LONGITUDE_MIN, longitude, 0);
					longitude = MercatorProjection.tileXToLongitude(0, MercatorProjection.zoomLevelToScaleFactor(zoomLevel));
					Assert.assertEquals(LatLongUtils.LONGITUDE_MIN, longitude, 0);

					long tileX = MercatorProjection.getMapSize(zoomLevel, tileSize) / tileSize;
					longitude = MercatorProjection.tileXToLongitude(tileX, zoomLevel);
					Assert.assertEquals(LatLongUtils.LONGITUDE_MAX, longitude, 0);
					tileX = MercatorProjection.getMapSizeWithScaleFactor(MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize) / tileSize;
					longitude = MercatorProjection.tileXToLongitude(tileX, MercatorProjection.zoomLevelToScaleFactor(zoomLevel));
					Assert.assertEquals(LatLongUtils.LONGITUDE_MAX, longitude, 0);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tileYToLatitudeTest()
		public virtual void tileYToLatitudeTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					double latitude = MercatorProjection.tileYToLatitude(0, zoomLevel);
					Assert.assertEquals(MercatorProjection.LATITUDE_MAX, latitude, 0);
					latitude = MercatorProjection.tileYToLatitude(0, MercatorProjection.zoomLevelToScaleFactor(zoomLevel));
					Assert.assertEquals(MercatorProjection.LATITUDE_MAX, latitude, 0);

					long tileY = MercatorProjection.getMapSize(zoomLevel, tileSize) / tileSize;
					latitude = MercatorProjection.tileYToLatitude(tileY, zoomLevel);
					Assert.assertEquals(MercatorProjection.LATITUDE_MIN, latitude, 0);
					tileY = MercatorProjection.getMapSizeWithScaleFactor(MercatorProjection.zoomLevelToScaleFactor(zoomLevel), tileSize) / tileSize;
					latitude = MercatorProjection.tileYToLatitude(tileY, MercatorProjection.zoomLevelToScaleFactor(zoomLevel));
					Assert.assertEquals(MercatorProjection.LATITUDE_MIN, latitude, 0);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void zoomLevelToScaleFactorTest()
		public virtual void zoomLevelToScaleFactorTest()
		{
			for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
				double scaleFactor = MercatorProjection.zoomLevelToScaleFactor(zoomLevel);
				Assert.assertEquals(zoomLevel, MercatorProjection.scaleFactorToZoomLevel(scaleFactor), 0.0001f);
			}
		}
	}

}