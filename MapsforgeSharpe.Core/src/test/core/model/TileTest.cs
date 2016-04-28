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
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;

	public class TileTest
	{
		private const string TILE_TO_STRING = "x=1, y=2, z=3";

		private const int TILE_SIZE = 256;

		private static Tile createTile(int tileX, int tileY, sbyte zoomLevel)
		{
			return new Tile(tileX, tileY, zoomLevel, TILE_SIZE);
		}

		private static void verifyInvalid(int tileX, int tileY, sbyte zoomLevel)
		{
			try
			{
				createTile(tileX, tileY, zoomLevel);
				Assert.fail("x: " + tileX + ", tileY: " + tileY + ", zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

		private static void verifyInvalidMaxTileNumber(sbyte zoomLevel)
		{
			try
			{
				Tile.getMaxTileNumber(zoomLevel);
				Assert.fail("zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void constructorTest()
		public virtual void constructorTest()
		{
			createTile(0, 0, (sbyte) 0);
			createTile(1, 1, (sbyte) 1);

			verifyInvalid(-1, 0, (sbyte) 0);
			verifyInvalid(0, -1, (sbyte) 0);
			verifyInvalid(0, 0, (sbyte) -1);

			verifyInvalid(1, 0, (sbyte) 0);
			verifyInvalid(0, 2, (sbyte) 1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsTest()
		public virtual void equalsTest()
		{
			Tile tile1 = new Tile(1, 2, (sbyte) 3, TILE_SIZE);
			Tile tile2 = new Tile(1, 2, (sbyte) 3, TILE_SIZE);
			Tile tile3 = new Tile(1, 1, (sbyte) 3, TILE_SIZE);
			Tile tile4 = new Tile(2, 2, (sbyte) 3, TILE_SIZE);
			Tile tile5 = new Tile(1, 2, (sbyte) 4, TILE_SIZE);

			TestUtils.equalsTest(tile1, tile2);

			TestUtils.notEqualsTest(tile1, tile3);
			TestUtils.notEqualsTest(tile1, tile4);
			TestUtils.notEqualsTest(tile1, tile5);
			TestUtils.notEqualsTest(tile1, new object());
			TestUtils.notEqualsTest(tile1, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getBoundingBoxTest()
		public virtual void getBoundingBoxTest()
		{
			Tile tile1 = new Tile(0, 0, (sbyte) 0, TILE_SIZE);
			Assert.assertTrue(tile1.BoundingBox.Equals(new BoundingBox(MercatorProjection.LATITUDE_MIN, -180, MercatorProjection.LATITUDE_MAX, 180)));

			Tile tile2 = new Tile(0, 0, (sbyte) 1, TILE_SIZE);
			Assert.assertEquals(tile1.BoundingBox.maxLatitude, tile2.BoundingBox.maxLatitude, 0.0001);
			Assert.assertEquals(tile1.BoundingBox.minLongitude, tile2.BoundingBox.minLongitude, 0.0001);

			Tile tile3 = new Tile(1, 1, (sbyte) 1, TILE_SIZE);
			Assert.assertEquals(tile1.BoundingBox.minLatitude, tile3.BoundingBox.minLatitude, 0.0001);
			Assert.assertNotEquals(tile1.BoundingBox.minLongitude, tile3.BoundingBox.minLongitude, 0.0001);
			Assert.assertEquals(tile3.BoundingBox.minLongitude, 0, 0.0001);
			Assert.assertEquals(tile3.BoundingBox.maxLongitude, 180, 0.0001);

			Tile tile4 = new Tile(0, 0, (sbyte) 12, TILE_SIZE);
			Assert.assertEquals(tile1.BoundingBox.maxLatitude, tile4.BoundingBox.maxLatitude, 0.0001);
			Assert.assertEquals(tile1.BoundingBox.minLongitude, tile4.BoundingBox.minLongitude, 0.0001);

			Tile tile5 = new Tile(0, 0, (sbyte) 24, TILE_SIZE);
			Assert.assertEquals(tile1.BoundingBox.maxLatitude, tile5.BoundingBox.maxLatitude, 0.0001);
			Assert.assertEquals(tile1.BoundingBox.minLongitude, tile5.BoundingBox.minLongitude, 0.0001);

		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getMaxTileNumberTest()
		public virtual void getMaxTileNumberTest()
		{
			Assert.assertEquals(0, Tile.getMaxTileNumber((sbyte) 0));
			Assert.assertEquals(1, Tile.getMaxTileNumber((sbyte) 1));
			Assert.assertEquals(3, Tile.getMaxTileNumber((sbyte) 2));
			Assert.assertEquals(7, Tile.getMaxTileNumber((sbyte) 3));
			Assert.assertEquals(1023, Tile.getMaxTileNumber((sbyte) 10));
			Assert.assertEquals(1048575, Tile.getMaxTileNumber((sbyte) 20));
			Assert.assertEquals(1073741823, Tile.getMaxTileNumber((sbyte) 30));

			verifyInvalidMaxTileNumber((sbyte) -1);
			verifyInvalidMaxTileNumber(sbyte.MinValue);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getParentTest()
		public virtual void getParentTest()
		{
			Tile rootTile = new Tile(0, 0, (sbyte) 0, TILE_SIZE);
			Assert.assertNull(rootTile.Parent);

			Assert.assertEquals(rootTile, (new Tile(0, 0, (sbyte) 1, TILE_SIZE)).Parent);
			Assert.assertEquals(rootTile, (new Tile(1, 0, (sbyte) 1, TILE_SIZE)).Parent);
			Assert.assertEquals(rootTile, (new Tile(0, 1, (sbyte) 1, TILE_SIZE)).Parent);
			Assert.assertEquals(rootTile, (new Tile(1, 1, (sbyte) 1, TILE_SIZE)).Parent);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getShiftXTest()
		public virtual void getShiftXTest()
		{
			Tile tile0 = new Tile(0, 0, (sbyte) 0, TILE_SIZE);
			Tile tile1 = new Tile(0, 1, (sbyte) 1, TILE_SIZE);
			Tile tile2 = new Tile(1, 2, (sbyte) 2, TILE_SIZE);

			Assert.assertEquals(0, tile0.getShiftX(tile0));
			Assert.assertEquals(0, tile1.getShiftX(tile0));
			Assert.assertEquals(1, tile2.getShiftX(tile0));
			Assert.assertEquals(1, tile2.getShiftX(tile1));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getShiftYTest()
		public virtual void getShiftYTest()
		{
			Tile tile0 = new Tile(0, 0, (sbyte) 0, TILE_SIZE);
			Tile tile1 = new Tile(0, 1, (sbyte) 1, TILE_SIZE);
			Tile tile2 = new Tile(1, 2, (sbyte) 2, TILE_SIZE);

			Assert.assertEquals(0, tile0.getShiftY(tile0));
			Assert.assertEquals(1, tile1.getShiftY(tile0));
			Assert.assertEquals(2, tile2.getShiftY(tile0));
			Assert.assertEquals(0, tile2.getShiftY(tile1));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getNeighbourTest()
		public virtual void getNeighbourTest()
		{
			Tile tile0 = new Tile(0, 0, (sbyte) 0, TILE_SIZE);
			Assert.assertTrue(tile0.Left.Equals(tile0));
			Assert.assertTrue(tile0.Right.Equals(tile0));
			Assert.assertTrue(tile0.Below.Equals(tile0));
			Assert.assertTrue(tile0.Above.Equals(tile0));
			Assert.assertTrue(tile0.AboveLeft.Equals(tile0));
			Assert.assertTrue(tile0.AboveRight.Equals(tile0));
			Assert.assertTrue(tile0.BelowRight.Equals(tile0));
			Assert.assertTrue(tile0.BelowLeft.Equals(tile0));

			Tile tile1 = new Tile(0, 1, (sbyte) 1, TILE_SIZE);
			Assert.assertTrue(tile1.Left.Left.Equals(tile1));
			Assert.assertTrue(tile1.Right.Right.Equals(tile1));
			Assert.assertTrue(tile1.Below.Below.Equals(tile1));
			Assert.assertTrue(tile1.Above.Above.Equals(tile1));
			Assert.assertTrue(tile1.Left.Right.Equals(tile1));
			Assert.assertTrue(tile1.Right.Left.Equals(tile1));
			Assert.assertTrue(tile1.Below.Above.Equals(tile1));
			Assert.assertTrue(tile1.Above.Below.Equals(tile1));
			Assert.assertTrue(tile1.Left.Right.Above.Below.Equals(tile1));
			Assert.assertTrue(tile1.Left.Right.Left.Right.Equals(tile1));
			Assert.assertTrue(tile1.Right.Below.Above.Left.Equals(tile1));
			Assert.assertTrue(tile1.Above.Left.Below.Right.Equals(tile1));
			Assert.assertTrue(tile1.AboveLeft.BelowRight.Above.Below.Equals(tile1));
			Assert.assertTrue(tile1.AboveLeft.BelowRight.Left.Right.Equals(tile1));
			Assert.assertTrue(tile1.Right.Below.Above.Left.Equals(tile1));
			Assert.assertTrue(tile1.Above.Left.BelowRight.Equals(tile1));

			Assert.assertFalse(tile1.AboveLeft.Left.BelowRight.Equals(tile1));
			Assert.assertFalse(tile1.Above.Left.BelowRight.Left.Equals(tile1));
			Assert.assertFalse(tile1.Above.BelowLeft.BelowRight.Equals(tile1));
			Assert.assertFalse(tile1.AboveLeft.Left.BelowRight.Equals(tile1));
			Assert.assertFalse(tile1.Above.AboveLeft.BelowRight.Equals(tile1));
			Assert.assertTrue(tile1.Above.Left.BelowRight.Equals(tile1));

			Tile tile2 = new Tile(0, 1, (sbyte) 2, TILE_SIZE);
			Assert.assertFalse(tile2.Left.Left.Equals(tile2));
			Assert.assertFalse(tile2.Right.Right.Equals(tile2));
			Assert.assertFalse(tile2.Below.Below.Equals(tile2));
			Assert.assertFalse(tile2.Above.Above.Equals(tile2));
			Assert.assertTrue(tile2.Left.Right.Equals(tile2));
			Assert.assertTrue(tile2.Right.Left.Equals(tile2));
			Assert.assertTrue(tile2.Below.Above.Equals(tile2));
			Assert.assertTrue(tile2.Above.Below.Equals(tile2));
			Assert.assertTrue(tile2.Left.Right.Above.Below.Equals(tile2));
			Assert.assertTrue(tile2.Left.Right.Left.Right.Equals(tile2));
			Assert.assertTrue(tile2.Right.Below.Above.Left.Equals(tile2));
			Assert.assertTrue(tile2.Above.Left.Below.Right.Equals(tile2));

			Tile tile5 = new Tile(0, 1, (sbyte) 5, TILE_SIZE);
			Assert.assertFalse(tile5.Left.Left.Equals(tile5));
			Assert.assertFalse(tile5.Right.Right.Equals(tile5));
			Assert.assertFalse(tile5.Below.Below.Equals(tile5));
			Assert.assertFalse(tile5.Above.Above.Equals(tile5));
			Assert.assertTrue(tile5.Left.Right.Equals(tile5));
			Assert.assertTrue(tile5.Right.Left.Equals(tile5));
			Assert.assertTrue(tile5.Below.Above.Equals(tile5));
			Assert.assertTrue(tile5.Above.Below.Equals(tile5));
			Assert.assertTrue(tile5.Left.Right.AboveLeft.Below.Right.Equals(tile5));
			Assert.assertTrue(tile5.Right.Left.BelowRight.AboveLeft.Equals(tile5));
			Assert.assertTrue(tile5.Below.Left.Above.Right.Equals(tile5));
			Assert.assertTrue(tile5.AboveLeft.Equals(tile5.Left.Above));
			Assert.assertTrue(tile5.BelowRight.Equals(tile5.Below.Right));
			Assert.assertTrue(tile5.Above.Below.Equals(tile5.Left.Right));


			tile5 = new Tile(0, 1, (sbyte) 14, TILE_SIZE);
			Assert.assertFalse(tile5.Left.Left.Equals(tile5));
			Assert.assertFalse(tile5.Right.Right.Equals(tile5));
			Assert.assertFalse(tile5.Below.Below.Equals(tile5));
			Assert.assertFalse(tile5.Above.Above.Equals(tile5));
			Assert.assertTrue(tile5.Left.Right.Equals(tile5));
			Assert.assertTrue(tile5.Right.Left.Equals(tile5));
			Assert.assertTrue(tile5.Below.Above.Equals(tile5));
			Assert.assertTrue(tile5.Above.Below.Equals(tile5));
			Assert.assertTrue(tile5.Left.Right.AboveLeft.Below.Right.Equals(tile5));
			Assert.assertTrue(tile5.Right.Left.BelowRight.AboveLeft.Equals(tile5));
			Assert.assertTrue(tile5.Below.Left.Above.Right.Equals(tile5));
			Assert.assertTrue(tile5.AboveLeft.Equals(tile5.Left.Above));
			Assert.assertTrue(tile5.BelowRight.Equals(tile5.Below.Right));
			Assert.assertTrue(tile5.Above.Below.Equals(tile5.Left.Right));

		}



//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getterTest()
		public virtual void getterTest()
		{
			Tile tile = new Tile(1, 2, (sbyte) 3, TILE_SIZE);

			Assert.assertEquals(1, tile.tileX);
			Assert.assertEquals(2, tile.tileY);
			Assert.assertEquals(3, tile.zoomLevel);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serializeTest() throws java.io.IOException, ClassNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		public virtual void serializeTest()
		{
			Tile tile = new Tile(1, 2, (sbyte) 3, TILE_SIZE);
			TestUtils.serializeTest(tile);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toStringTest()
		public virtual void toStringTest()
		{
			Tile tile = new Tile(1, 2, (sbyte) 3, TILE_SIZE);
			Assert.assertEquals(TILE_TO_STRING, tile.ToString());
		}
	}

}