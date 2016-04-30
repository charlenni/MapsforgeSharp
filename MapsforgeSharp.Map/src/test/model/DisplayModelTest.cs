/*
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.map.model
{

	using After = org.junit.After;
	using Assert = org.junit.Assert;
	using Test = org.junit.Test;

	public class DisplayModelTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsTest()
		public virtual void equalsTest()
		{
			DisplayModel displayModel1 = new DisplayModel();
			DisplayModel displayModel2 = new DisplayModel();

			DisplayModel displayModel3 = new DisplayModel();
			displayModel3.BackgroundColor = 0xffff0000;

			DisplayModel displayModel4 = new DisplayModel();
			displayModel4.FixedTileSize = 512;

			DisplayModel displayModel5 = new DisplayModel();
			displayModel5.MaxTextWidthFactor = 0.5f;

			DisplayModel displayModel6 = new DisplayModel();
			displayModel6.TileSizeMultiple = 200;

			DisplayModel displayModel7 = new DisplayModel();
			displayModel7.UserScaleFactor = 0.3f;

			TestUtils.equalsTest(displayModel1, displayModel2);

			TestUtils.notEqualsTest(displayModel1, displayModel3);
			TestUtils.notEqualsTest(displayModel1, displayModel4);
			TestUtils.notEqualsTest(displayModel1, displayModel5);
			TestUtils.notEqualsTest(displayModel1, displayModel6);
			TestUtils.notEqualsTest(displayModel1, displayModel7);
			TestUtils.notEqualsTest(displayModel1, new object());
			TestUtils.notEqualsTest(displayModel1, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tileSizeTest()
		public virtual void tileSizeTest()
		{
			DisplayModel displayModel = new DisplayModel();

			Assert.assertEquals(256, displayModel.TileSize);
			displayModel.UserScaleFactor = 2;
			Assert.assertEquals(512, displayModel.TileSize);
			displayModel.UserScaleFactor = 1.5f;
			Assert.assertEquals(384, displayModel.TileSize);
			displayModel.TileSizeMultiple = 100;
			Assert.assertEquals(400, displayModel.TileSize);
			displayModel.UserScaleFactor = 2;
			Assert.assertEquals(500, displayModel.TileSize);
			displayModel.TileSizeMultiple = 64;
			Assert.assertEquals(512, displayModel.TileSize);

			DisplayModel.DeviceScaleFactor = 2;
			displayModel.UserScaleFactor = 1;
			displayModel.TileSizeMultiple = 1;

			Assert.assertEquals(512, displayModel.TileSize);
			displayModel.UserScaleFactor = 2;
			Assert.assertEquals(1024, displayModel.TileSize);
			displayModel.UserScaleFactor = 1.5f;
			Assert.assertEquals(768, displayModel.TileSize);
			displayModel.TileSizeMultiple = 100;
			Assert.assertEquals(800, displayModel.TileSize);
			displayModel.UserScaleFactor = 2;
			Assert.assertEquals(1000, displayModel.TileSize);
			displayModel.TileSizeMultiple = 64;
			Assert.assertEquals(1024, displayModel.TileSize);

			DisplayModel.DeviceScaleFactor = 1;
			displayModel.UserScaleFactor = 1;
			displayModel.TileSizeMultiple = 1;

			Assert.assertEquals(256, displayModel.TileSize);
			displayModel.UserScaleFactor = 2;
			Assert.assertEquals(512, displayModel.TileSize);
			displayModel.UserScaleFactor = 1.5f;
			Assert.assertEquals(384, displayModel.TileSize);
			displayModel.TileSizeMultiple = 100;
			Assert.assertEquals(400, displayModel.TileSize);
			displayModel.UserScaleFactor = 2;
			Assert.assertEquals(500, displayModel.TileSize);
			displayModel.TileSizeMultiple = 64;
			Assert.assertEquals(512, displayModel.TileSize);

			DisplayModel.DeviceScaleFactor = 1.2f;
			displayModel.UserScaleFactor = 1;
			displayModel.TileSizeMultiple = 1;

			Assert.assertEquals(307, displayModel.TileSize);
			displayModel.UserScaleFactor = 2;
			Assert.assertEquals(614, displayModel.TileSize);
			displayModel.UserScaleFactor = 1.5f;
			Assert.assertEquals(461, displayModel.TileSize);
			displayModel.TileSizeMultiple = 100;
			Assert.assertEquals(500, displayModel.TileSize);
			displayModel.UserScaleFactor = 2;
			Assert.assertEquals(600, displayModel.TileSize);
			displayModel.TileSizeMultiple = 64;
			Assert.assertEquals(640, displayModel.TileSize);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		public virtual void tearDown()
		{
			// reset to 1 for all following tests
			DisplayModel.DeviceScaleFactor = 1;
		}

	}

}