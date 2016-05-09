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
    using Util;
    public class TileTest
	{
		private const string TILE_TO_STRING = "x=1, y=2, z=3";
		private const int TILE_SIZE = 256;

		private static Tile CreateTile(int tileX, int tileY, sbyte zoomLevel)
		{
			return new Tile(tileX, tileY, zoomLevel, TILE_SIZE);
		}

        private static void VerifyInvalid(int tileX, int tileY, sbyte zoomLevel)
		{
			try
			{
				CreateTile(tileX, tileY, zoomLevel);
				Assert.Fail("x: " + tileX + ", tileY: " + tileY + ", zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

        private static void VerifyInvalidMaxTileNumber(sbyte zoomLevel)
		{
			try
			{
				Tile.GetMaxTileNumber(zoomLevel);
				Assert.Fail("zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

        [Test()]
		public virtual void ConstructorTest()
		{
			CreateTile(0, 0, (sbyte) 0);
			CreateTile(1, 1, (sbyte) 1);

			VerifyInvalid(-1, 0, (sbyte) 0);
			VerifyInvalid(0, -1, (sbyte) 0);
			VerifyInvalid(0, 0, (sbyte) -1);

			VerifyInvalid(1, 0, (sbyte) 0);
			VerifyInvalid(0, 2, (sbyte) 1);
		}

        [Test()]
		public virtual void EqualsTest()
		{
			Tile tile1 = new Tile(1, 2, (sbyte) 3, TILE_SIZE);
			Tile tile2 = new Tile(1, 2, (sbyte) 3, TILE_SIZE);
			Tile tile3 = new Tile(1, 1, (sbyte) 3, TILE_SIZE);
			Tile tile4 = new Tile(2, 2, (sbyte) 3, TILE_SIZE);
			Tile tile5 = new Tile(1, 2, (sbyte) 4, TILE_SIZE);

			TestUtils.EqualsTest(tile1, tile2);

			TestUtils.NotEqualsTest(tile1, tile3);
			TestUtils.NotEqualsTest(tile1, tile4);
			TestUtils.NotEqualsTest(tile1, tile5);
			TestUtils.NotEqualsTest(tile1, new object());
			TestUtils.NotEqualsTest(tile1, null);
		}

        [Test()]
        public virtual void GetBoundingBoxTest()
		{
			Tile tile1 = new Tile(0, 0, (sbyte) 0, TILE_SIZE);
			Assert.True(tile1.BoundingBox.Equals(new BoundingBox(MercatorProjection.LATITUDE_MIN, -180, MercatorProjection.LATITUDE_MAX, 180)));

			Tile tile2 = new Tile(0, 0, (sbyte) 1, TILE_SIZE);
			Assert.AreEqual(tile1.BoundingBox.MaxLatitude, tile2.BoundingBox.MaxLatitude, 0.0001);
			Assert.AreEqual(tile1.BoundingBox.MinLongitude, tile2.BoundingBox.MinLongitude, 0.0001);

			Tile tile3 = new Tile(1, 1, (sbyte) 1, TILE_SIZE);
			Assert.AreEqual(tile1.BoundingBox.MinLatitude, tile3.BoundingBox.MinLatitude, 0.0001);
            // TODO: Not sure, how to do this
			//Assert.AreNotEqual(tile1.BoundingBox.minLongitude, tile3.BoundingBox.minLongitude, 0.0001);
			Assert.AreEqual(tile3.BoundingBox.MinLongitude, 0, 0.0001);
			Assert.AreEqual(tile3.BoundingBox.MaxLongitude, 180, 0.0001);

			Tile tile4 = new Tile(0, 0, (sbyte) 12, TILE_SIZE);
			Assert.AreEqual(tile1.BoundingBox.MaxLatitude, tile4.BoundingBox.MaxLatitude, 0.0001);
			Assert.AreEqual(tile1.BoundingBox.MinLongitude, tile4.BoundingBox.MinLongitude, 0.0001);

			Tile tile5 = new Tile(0, 0, (sbyte) 24, TILE_SIZE);
			Assert.AreEqual(tile1.BoundingBox.MaxLatitude, tile5.BoundingBox.MaxLatitude, 0.0001);
			Assert.AreEqual(tile1.BoundingBox.MinLongitude, tile5.BoundingBox.MinLongitude, 0.0001);
		}

        [Test()]
		public virtual void GetMaxTileNumberTest()
		{
			Assert.AreEqual(0, Tile.GetMaxTileNumber((sbyte) 0));
			Assert.AreEqual(1, Tile.GetMaxTileNumber((sbyte) 1));
			Assert.AreEqual(3, Tile.GetMaxTileNumber((sbyte) 2));
			Assert.AreEqual(7, Tile.GetMaxTileNumber((sbyte) 3));
			Assert.AreEqual(1023, Tile.GetMaxTileNumber((sbyte) 10));
			Assert.AreEqual(1048575, Tile.GetMaxTileNumber((sbyte) 20));
			Assert.AreEqual(1073741823, Tile.GetMaxTileNumber((sbyte) 30));

			VerifyInvalidMaxTileNumber((sbyte) -1);
			VerifyInvalidMaxTileNumber(sbyte.MinValue);
		}

        [Test()]
		public virtual void GetParentTest()
		{
			Tile rootTile = new Tile(0, 0, (sbyte) 0, TILE_SIZE);
			Assert.Null(rootTile.Parent);

			Assert.AreEqual(rootTile, (new Tile(0, 0, (sbyte) 1, TILE_SIZE)).Parent);
			Assert.AreEqual(rootTile, (new Tile(1, 0, (sbyte) 1, TILE_SIZE)).Parent);
			Assert.AreEqual(rootTile, (new Tile(0, 1, (sbyte) 1, TILE_SIZE)).Parent);
			Assert.AreEqual(rootTile, (new Tile(1, 1, (sbyte) 1, TILE_SIZE)).Parent);
		}

        [Test()]
		public virtual void GetShiftXTest()
		{
			Tile tile0 = new Tile(0, 0, (sbyte) 0, TILE_SIZE);
			Tile tile1 = new Tile(0, 1, (sbyte) 1, TILE_SIZE);
			Tile tile2 = new Tile(1, 2, (sbyte) 2, TILE_SIZE);

			Assert.AreEqual(0, tile0.GetShiftX(tile0));
			Assert.AreEqual(0, tile1.GetShiftX(tile0));
			Assert.AreEqual(1, tile2.GetShiftX(tile0));
			Assert.AreEqual(1, tile2.GetShiftX(tile1));
		}

        [Test()]
		public virtual void GetShiftYTest()
		{
			Tile tile0 = new Tile(0, 0, (sbyte) 0, TILE_SIZE);
			Tile tile1 = new Tile(0, 1, (sbyte) 1, TILE_SIZE);
			Tile tile2 = new Tile(1, 2, (sbyte) 2, TILE_SIZE);

			Assert.AreEqual(0, tile0.GetShiftY(tile0));
			Assert.AreEqual(1, tile1.GetShiftY(tile0));
			Assert.AreEqual(2, tile2.GetShiftY(tile0));
			Assert.AreEqual(0, tile2.GetShiftY(tile1));
		}

        [Test()]
		public virtual void GetNeighbourTest()
		{
			Tile tile0 = new Tile(0, 0, (sbyte) 0, TILE_SIZE);
			Assert.True(tile0.Left.Equals(tile0));
			Assert.True(tile0.Right.Equals(tile0));
			Assert.True(tile0.Below.Equals(tile0));
			Assert.True(tile0.Above.Equals(tile0));
			Assert.True(tile0.AboveLeft.Equals(tile0));
			Assert.True(tile0.AboveRight.Equals(tile0));
			Assert.True(tile0.BelowRight.Equals(tile0));
			Assert.True(tile0.BelowLeft.Equals(tile0));

			Tile tile1 = new Tile(0, 1, (sbyte) 1, TILE_SIZE);
			Assert.True(tile1.Left.Left.Equals(tile1));
			Assert.True(tile1.Right.Right.Equals(tile1));
			Assert.True(tile1.Below.Below.Equals(tile1));
			Assert.True(tile1.Above.Above.Equals(tile1));
			Assert.True(tile1.Left.Right.Equals(tile1));
			Assert.True(tile1.Right.Left.Equals(tile1));
			Assert.True(tile1.Below.Above.Equals(tile1));
			Assert.True(tile1.Above.Below.Equals(tile1));
			Assert.True(tile1.Left.Right.Above.Below.Equals(tile1));
			Assert.True(tile1.Left.Right.Left.Right.Equals(tile1));
			Assert.True(tile1.Right.Below.Above.Left.Equals(tile1));
			Assert.True(tile1.Above.Left.Below.Right.Equals(tile1));
			Assert.True(tile1.AboveLeft.BelowRight.Above.Below.Equals(tile1));
			Assert.True(tile1.AboveLeft.BelowRight.Left.Right.Equals(tile1));
			Assert.True(tile1.Right.Below.Above.Left.Equals(tile1));
			Assert.True(tile1.Above.Left.BelowRight.Equals(tile1));

			Assert.False(tile1.AboveLeft.Left.BelowRight.Equals(tile1));
			Assert.False(tile1.Above.Left.BelowRight.Left.Equals(tile1));
			Assert.False(tile1.Above.BelowLeft.BelowRight.Equals(tile1));
			Assert.False(tile1.AboveLeft.Left.BelowRight.Equals(tile1));
			Assert.False(tile1.Above.AboveLeft.BelowRight.Equals(tile1));
			Assert.True(tile1.Above.Left.BelowRight.Equals(tile1));

			Tile tile2 = new Tile(0, 1, (sbyte) 2, TILE_SIZE);
			Assert.False(tile2.Left.Left.Equals(tile2));
			Assert.False(tile2.Right.Right.Equals(tile2));
			Assert.False(tile2.Below.Below.Equals(tile2));
			Assert.False(tile2.Above.Above.Equals(tile2));
			Assert.True(tile2.Left.Right.Equals(tile2));
			Assert.True(tile2.Right.Left.Equals(tile2));
			Assert.True(tile2.Below.Above.Equals(tile2));
			Assert.True(tile2.Above.Below.Equals(tile2));
			Assert.True(tile2.Left.Right.Above.Below.Equals(tile2));
			Assert.True(tile2.Left.Right.Left.Right.Equals(tile2));
			Assert.True(tile2.Right.Below.Above.Left.Equals(tile2));
			Assert.True(tile2.Above.Left.Below.Right.Equals(tile2));

			Tile tile5 = new Tile(0, 1, (sbyte) 5, TILE_SIZE);
			Assert.False(tile5.Left.Left.Equals(tile5));
			Assert.False(tile5.Right.Right.Equals(tile5));
			Assert.False(tile5.Below.Below.Equals(tile5));
			Assert.False(tile5.Above.Above.Equals(tile5));
			Assert.True(tile5.Left.Right.Equals(tile5));
			Assert.True(tile5.Right.Left.Equals(tile5));
			Assert.True(tile5.Below.Above.Equals(tile5));
			Assert.True(tile5.Above.Below.Equals(tile5));
			Assert.True(tile5.Left.Right.AboveLeft.Below.Right.Equals(tile5));
			Assert.True(tile5.Right.Left.BelowRight.AboveLeft.Equals(tile5));
			Assert.True(tile5.Below.Left.Above.Right.Equals(tile5));
			Assert.True(tile5.AboveLeft.Equals(tile5.Left.Above));
			Assert.True(tile5.BelowRight.Equals(tile5.Below.Right));
			Assert.True(tile5.Above.Below.Equals(tile5.Left.Right));

			tile5 = new Tile(0, 1, (sbyte) 14, TILE_SIZE);
			Assert.False(tile5.Left.Left.Equals(tile5));
			Assert.False(tile5.Right.Right.Equals(tile5));
			Assert.False(tile5.Below.Below.Equals(tile5));
			Assert.False(tile5.Above.Above.Equals(tile5));
			Assert.True(tile5.Left.Right.Equals(tile5));
			Assert.True(tile5.Right.Left.Equals(tile5));
			Assert.True(tile5.Below.Above.Equals(tile5));
			Assert.True(tile5.Above.Below.Equals(tile5));
			Assert.True(tile5.Left.Right.AboveLeft.Below.Right.Equals(tile5));
			Assert.True(tile5.Right.Left.BelowRight.AboveLeft.Equals(tile5));
			Assert.True(tile5.Below.Left.Above.Right.Equals(tile5));
			Assert.True(tile5.AboveLeft.Equals(tile5.Left.Above));
			Assert.True(tile5.BelowRight.Equals(tile5.Below.Right));
			Assert.True(tile5.Above.Below.Equals(tile5.Left.Right));
		}

        [Test()]
		public virtual void GetterTest()
		{
			Tile tile = new Tile(1, 2, (sbyte) 3, TILE_SIZE);

			Assert.AreEqual(1, tile.TileX);
			Assert.AreEqual(2, tile.TileY);
			Assert.AreEqual(3, tile.ZoomLevel);
		}

        [Test()]
		public virtual void SerializeTest()
		{
			Tile tile = new Tile(1, 2, (sbyte) 3, TILE_SIZE);
			TestUtils.SerializeTest(tile);
		}

        [Test()]
		public virtual void ToStringTest()
		{
			Tile tile = new Tile(1, 2, (sbyte) 3, TILE_SIZE);
			Assert.AreEqual(TILE_TO_STRING, tile.ToString());
		}
	}
}