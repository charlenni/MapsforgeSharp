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
namespace org.mapsforge.map.reader
{

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;
	using Tile = org.mapsforge.core.model.Tile;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using MapReadResult = org.mapsforge.map.datastore.MapReadResult;

	public class MapFileEmptyTest
	{
		private static readonly File MAP_FILE = new File("src/test/resources/empty/output.map");
		private const sbyte ZOOM_LEVEL_MAX = 25;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeQueryTest()
		public virtual void executeQueryTest()
		{
			MapFile mapFile = new MapFile(MAP_FILE);

			for (sbyte zoomLevel = 0; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
				int tileX = MercatorProjection.longitudeToTileX(1, zoomLevel);
				int tileY = MercatorProjection.latitudeToTileY(1, zoomLevel);
				Tile tile = new Tile(tileX, tileY, zoomLevel, 256);

				MapReadResult mapReadResult = mapFile.readMapData(tile);

				Assert.assertTrue(mapReadResult.pointOfInterests.Empty);
				Assert.assertTrue(mapReadResult.ways.Empty);
			}

			mapFile.close();
		}
	}

}