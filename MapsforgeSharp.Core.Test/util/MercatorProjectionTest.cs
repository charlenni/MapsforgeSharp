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

namespace org.mapsforge.core.util
{
    using NUnit.Framework;
    using System;

    using Tile = MapsforgeSharp.Core.Model.Tile;

    public class MercatorProjectionTest
	{
		private static readonly int[] TILE_SIZES = new int[] {256, 128, 376, 512, 100};
		private const int ZOOM_LEVEL_MAX = 30;
		private const int ZOOM_LEVEL_MIN = 0;

		private static void VerifyInvalidGetMapSize(sbyte zoomLevel, int tileSize)
		{
			try
			{
				MercatorProjection.GetMapSize(zoomLevel, tileSize);
				Assert.Fail("zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

		private static void VerifyInvalidPixelXToLongitude(double pixelX, sbyte zoomLevel, int tileSize)
		{
			try
			{
				MercatorProjection.PixelXToLongitude(pixelX, MercatorProjection.GetMapSize(zoomLevel, tileSize));
				Assert.Fail("pixelX: " + pixelX + ", zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

		private static void VerifyInvalidPixelYToLatitude(double pixelY, sbyte zoomLevel, int tileSize)
		{
			try
			{
				MercatorProjection.PixelYToLatitude(pixelY, MercatorProjection.GetMapSize(zoomLevel, tileSize));
				Assert.Fail("pixelY: " + pixelY + ", zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

        [Test()]
		public virtual void GetMapSizeTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					long factor = (long)Math.Round(MercatorProjection.ZoomLevelToScaleFactor(zoomLevel));
					Assert.AreEqual(tileSize * factor, MercatorProjection.GetMapSize(zoomLevel, tileSize));
					Assert.AreEqual(MercatorProjection.GetMapSizeWithScaleFactor(MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize), MercatorProjection.GetMapSize(zoomLevel, tileSize));
				}
				VerifyInvalidGetMapSize((sbyte) -1, tileSize);
			}
		}

        [Test()]
        public virtual void LatitudeToPixelYTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					long mapSize = MercatorProjection.GetMapSize(zoomLevel, tileSize);
					double pixelY = MercatorProjection.LatitudeToPixelY(MercatorProjection.LATITUDE_MAX, mapSize);
					Assert.AreEqual(0, pixelY, 0);

					pixelY = MercatorProjection.LatitudeToPixelYWithScaleFactor(MercatorProjection.LATITUDE_MAX, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual(0, pixelY, 0);

					pixelY = MercatorProjection.LatitudeToPixelY(0, mapSize);
					Assert.AreEqual((float) mapSize / 2, pixelY, 0);
					pixelY = MercatorProjection.LatitudeToPixelYWithScaleFactor(0, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual((float) mapSize / 2, pixelY, 0);

					pixelY = MercatorProjection.LatitudeToPixelY(MercatorProjection.LATITUDE_MIN, mapSize);
					Assert.AreEqual(mapSize, pixelY, 0);
					pixelY = MercatorProjection.LatitudeToPixelYWithScaleFactor(MercatorProjection.LATITUDE_MIN, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual(mapSize, pixelY, 0);
				}
			}
		}

        [Test()]
        public virtual void LatitudeToTileYTest()
		{
			for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
				long tileY = MercatorProjection.LatitudeToTileY(MercatorProjection.LATITUDE_MAX, zoomLevel);
				Assert.AreEqual(0, tileY);
				tileY = MercatorProjection.LatitudeToTileY(MercatorProjection.LATITUDE_MAX, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel));
				Assert.AreEqual(0, tileY);

				tileY = MercatorProjection.LatitudeToTileY(MercatorProjection.LATITUDE_MIN, zoomLevel);
				Assert.AreEqual(Tile.GetMaxTileNumber(zoomLevel), tileY);
				tileY = MercatorProjection.LatitudeToTileY(MercatorProjection.LATITUDE_MIN, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel));
				Assert.AreEqual(Tile.GetMaxTileNumber(zoomLevel), tileY);
			}
		}

        [Test()]
        public virtual void LongitudeToPixelXTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					long mapSize = MercatorProjection.GetMapSize(zoomLevel, tileSize);
					double pixelX = MercatorProjection.LongitudeToPixelX(LatLongUtils.LONGITUDE_MIN, mapSize);
					Assert.AreEqual(0, pixelX, 0);
					pixelX = MercatorProjection.LongitudeToPixelXWithScaleFactor(LatLongUtils.LONGITUDE_MIN, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual(0, pixelX, 0);

					pixelX = MercatorProjection.LongitudeToPixelX(0, mapSize);
					Assert.AreEqual((float) mapSize / 2, pixelX, 0);
					mapSize = MercatorProjection.GetMapSizeWithScaleFactor(MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					pixelX = MercatorProjection.LongitudeToPixelXWithScaleFactor(0, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual((float) mapSize / 2, pixelX, 0);

					pixelX = MercatorProjection.LongitudeToPixelX(LatLongUtils.LONGITUDE_MAX, mapSize);
					Assert.AreEqual(mapSize, pixelX, 0);
					pixelX = MercatorProjection.LongitudeToPixelXWithScaleFactor(LatLongUtils.LONGITUDE_MAX, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual(mapSize, pixelX, 0);
				}
			}
		}

        [Test()]
        public virtual void LongitudeToTileXTest()
		{
			for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
				long tileX = MercatorProjection.LongitudeToTileX(LatLongUtils.LONGITUDE_MIN, zoomLevel);
				Assert.AreEqual(0, tileX);
				tileX = MercatorProjection.LongitudeToTileX(LatLongUtils.LONGITUDE_MIN, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel));
				Assert.AreEqual(0, tileX);

				tileX = MercatorProjection.LongitudeToTileX(LatLongUtils.LONGITUDE_MAX, zoomLevel);
				Assert.AreEqual(Tile.GetMaxTileNumber(zoomLevel), tileX);
				tileX = MercatorProjection.LongitudeToTileX(LatLongUtils.LONGITUDE_MAX, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel));
				Assert.AreEqual(Tile.GetMaxTileNumber(zoomLevel), tileX);
			}
		}

        [Test()]
        public virtual void MetersToPixelTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				Assert.True(MercatorProjection.MetersToPixels(10, 10.0, MercatorProjection.GetMapSize((sbyte) 1, tileSize)) < 1);
				Assert.True(MercatorProjection.MetersToPixels((long)(40 * 10e7), 10.0, MercatorProjection.GetMapSize((sbyte) 1, tileSize)) > 1);
				Assert.True(MercatorProjection.MetersToPixels(10, 10.0, MercatorProjection.GetMapSize((sbyte) 20, tileSize)) > 1);
				Assert.True(MercatorProjection.MetersToPixels(10, 89.0, MercatorProjection.GetMapSize((sbyte) 1, tileSize)) < 1);
				Assert.True(MercatorProjection.MetersToPixels((int)(40 * 10e3), 50, MercatorProjection.GetMapSize((sbyte) 10, tileSize)) > 1);
			}
		}

        [Test()]
        public virtual void PixelXToLongitudeTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					long mapSize = MercatorProjection.GetMapSize(zoomLevel, tileSize);
					double longitude = MercatorProjection.PixelXToLongitude(0, mapSize);
					Assert.AreEqual(LatLongUtils.LONGITUDE_MIN, longitude, 0);
					longitude = MercatorProjection.PixelXToLongitudeWithScaleFactor(0, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual(LatLongUtils.LONGITUDE_MIN, longitude, 0);

					longitude = MercatorProjection.PixelXToLongitude((float) mapSize / 2, mapSize);
					Assert.AreEqual(0, longitude, 0);
					mapSize = MercatorProjection.GetMapSizeWithScaleFactor(MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					longitude = MercatorProjection.PixelXToLongitudeWithScaleFactor((float) mapSize / 2, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual(0, longitude, 0);

					longitude = MercatorProjection.PixelXToLongitude(mapSize, mapSize);
					Assert.AreEqual(LatLongUtils.LONGITUDE_MAX, longitude, 0);
					longitude = MercatorProjection.PixelXToLongitudeWithScaleFactor(mapSize, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual(LatLongUtils.LONGITUDE_MAX, longitude, 0);
				}

				VerifyInvalidPixelXToLongitude(-1, (sbyte) 0, tileSize);
				VerifyInvalidPixelXToLongitude(tileSize + 1, (sbyte) 0, tileSize);
			}
		}

        [Test()]
		public virtual void PixelXToTileXTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					Assert.AreEqual(0, MercatorProjection.PixelXToTileX(0, zoomLevel, tileSize));
					Assert.AreEqual(0, MercatorProjection.PixelXToTileX(0, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize));
				}
			}
		}

        [Test()]
        public virtual void PixelYToLatitudeTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					long mapSize = MercatorProjection.GetMapSize(zoomLevel, tileSize);
					double latitude = MercatorProjection.PixelYToLatitude(0, mapSize);
					Assert.AreEqual(MercatorProjection.LATITUDE_MAX, latitude, 0);
					latitude = MercatorProjection.PixelYToLatitudeWithScaleFactor(0, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual(MercatorProjection.LATITUDE_MAX, latitude, 0);

					latitude = MercatorProjection.PixelYToLatitude((float) mapSize / 2, mapSize);
					Assert.AreEqual(0, latitude, 0);
					mapSize = MercatorProjection.GetMapSizeWithScaleFactor(MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					latitude = MercatorProjection.PixelYToLatitudeWithScaleFactor((float) mapSize / 2, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual(0, latitude, 0);

					latitude = MercatorProjection.PixelYToLatitude(mapSize, mapSize);
					Assert.AreEqual(MercatorProjection.LATITUDE_MIN, latitude, 0);
					latitude = MercatorProjection.PixelYToLatitudeWithScaleFactor(mapSize, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize);
					Assert.AreEqual(MercatorProjection.LATITUDE_MIN, latitude, 0);
				}

				VerifyInvalidPixelYToLatitude(-1, (sbyte) 0, tileSize);
				VerifyInvalidPixelYToLatitude(tileSize + 1, (sbyte) 0, tileSize);
			}
		}

        [Test()]
        public virtual void PixelYToTileYTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					Assert.AreEqual(0, MercatorProjection.PixelYToTileY(0, zoomLevel, tileSize));
					Assert.AreEqual(0, MercatorProjection.PixelYToTileY(0, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize));
				}
			}
		}

        [Test()]
        public virtual void TileToPixelTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				Assert.AreEqual(0, MercatorProjection.TileToPixel(0, tileSize));
				Assert.AreEqual(tileSize, MercatorProjection.TileToPixel(1, tileSize));
				Assert.AreEqual(tileSize * 2, MercatorProjection.TileToPixel(2, tileSize));
			}
		}

        [Test()]
        public virtual void TileXToLongitudeTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					double longitude = MercatorProjection.TileXToLongitude(0, zoomLevel);
					Assert.AreEqual(LatLongUtils.LONGITUDE_MIN, longitude, 0);
					longitude = MercatorProjection.TileXToLongitude(0, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel));
					Assert.AreEqual(LatLongUtils.LONGITUDE_MIN, longitude, 0);

					long tileX = MercatorProjection.GetMapSize(zoomLevel, tileSize) / tileSize;
					longitude = MercatorProjection.TileXToLongitude(tileX, zoomLevel);
					Assert.AreEqual(LatLongUtils.LONGITUDE_MAX, longitude, 0);
					tileX = MercatorProjection.GetMapSizeWithScaleFactor(MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize) / tileSize;
					longitude = MercatorProjection.TileXToLongitude(tileX, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel));
					Assert.AreEqual(LatLongUtils.LONGITUDE_MAX, longitude, 0);
				}
			}
		}

        [Test()]
        public virtual void TileYToLatitudeTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
				{
					double latitude = MercatorProjection.TileYToLatitude(0, zoomLevel);
					Assert.AreEqual(MercatorProjection.LATITUDE_MAX, latitude, 0);
					latitude = MercatorProjection.TileYToLatitude(0, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel));
					Assert.AreEqual(MercatorProjection.LATITUDE_MAX, latitude, 0);

					long tileY = MercatorProjection.GetMapSize(zoomLevel, tileSize) / tileSize;
					latitude = MercatorProjection.TileYToLatitude(tileY, zoomLevel);
					Assert.AreEqual(MercatorProjection.LATITUDE_MIN, latitude, 0);
					tileY = MercatorProjection.GetMapSizeWithScaleFactor(MercatorProjection.ZoomLevelToScaleFactor(zoomLevel), tileSize) / tileSize;
					latitude = MercatorProjection.TileYToLatitude(tileY, MercatorProjection.ZoomLevelToScaleFactor(zoomLevel));
					Assert.AreEqual(MercatorProjection.LATITUDE_MIN, latitude, 0);
				}
			}
		}

        [Test()]
        public virtual void ZoomLevelToScaleFactorTest()
		{
			for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
				double scaleFactor = MercatorProjection.ZoomLevelToScaleFactor(zoomLevel);
				Assert.AreEqual(zoomLevel, MercatorProjection.ScaleFactorToZoomLevel(scaleFactor), 0.0001f);
			}
		}
	}
}