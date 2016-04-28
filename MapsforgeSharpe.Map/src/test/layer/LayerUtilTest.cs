using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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
namespace org.mapsforge.map.layer
{

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;
	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using Point = org.mapsforge.core.model.Point;
	using Tile = org.mapsforge.core.model.Tile;
	using LayerUtil = org.mapsforge.map.util.LayerUtil;

	public class LayerUtilTest
	{
		private static readonly int[] TILE_SIZES = new int[] {256, 128, 376, 512, 100};

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getTilePositionsTest()
		public virtual void getTilePositionsTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				BoundingBox boundingBox = new BoundingBox(-1, -1, 1, 1);
				IList<TilePosition> tilePositions = LayerUtil.getTilePositions(boundingBox, (sbyte) 0, new Point(0, 0), tileSize);
				Assert.assertEquals(1, tilePositions.Count);

				TilePosition tilePosition = tilePositions[0];
				Assert.assertEquals(new Tile(0, 0, (sbyte) 0, tileSize), tilePosition.tile);
				Assert.assertEquals(new Point(0, 0), tilePosition.point);
			}
		}
	}

}