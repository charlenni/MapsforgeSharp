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
	using LatLong = org.mapsforge.core.model.LatLong;
	using Tile = org.mapsforge.core.model.Tile;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using MapReadResult = org.mapsforge.map.datastore.MapReadResult;
	using Way = org.mapsforge.map.datastore.Way;

	internal sealed class EncodingTest
	{
		private const sbyte ZOOM_LEVEL = 8;

		internal static void runTest(MapFile mapFile)
		{

			int tileX = MercatorProjection.longitudeToTileX(0, ZOOM_LEVEL);
			int tileY = MercatorProjection.latitudeToTileY(0, ZOOM_LEVEL);
			Tile tile = new Tile(tileX, tileY, ZOOM_LEVEL, 256);

			MapReadResult mapReadResult = mapFile.readMapData(tile);
			mapFile.close();

			Assert.assertTrue(mapReadResult.pointOfInterests.Empty);
			Assert.assertEquals(1, mapReadResult.ways.size());

			LatLong latLong1 = new LatLong(0.0, 0.0, true);
			LatLong latLong2 = new LatLong(0.0, 0.1, true);
			LatLong latLong3 = new LatLong(-0.1, 0.1, true);
			LatLong latLong4 = new LatLong(-0.1, 0.0, true);
			LatLong[][] latLongsExpected = new LatLong[][]
			{
				new LatLong[] {latLong1, latLong2, latLong3, latLong4, latLong1}
			};

			Way way = mapReadResult.ways.get(0);
			Assert.assertArrayEquals(latLongsExpected, way.latLongs);
		}

		private EncodingTest()
		{
			throw new System.InvalidOperationException();
		}
	}

}