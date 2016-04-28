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
	using LatLong = org.mapsforge.core.model.LatLong;
	using Tag = org.mapsforge.core.model.Tag;
	using Tile = org.mapsforge.core.model.Tile;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using MapReadResult = org.mapsforge.map.datastore.MapReadResult;
	using PointOfInterest = org.mapsforge.map.datastore.PointOfInterest;
	using Way = org.mapsforge.map.datastore.Way;
	using MapFileInfo = org.mapsforge.map.reader.header.MapFileInfo;

	public class MapFileWithDataTest
	{
		private static readonly File MAP_FILE = new File("src/test/resources/with_data/output.map");
		private const sbyte ZOOM_LEVEL_MAX = 11;
		private const int ZOOM_LEVEL_MIN = 6;

		private static void assertLatLongsEquals(LatLong[][] latLongs1, LatLong[][] latLongs2)
		{
			Assert.assertEquals(latLongs1.Length, latLongs2.Length);

			for (int i = 0; i < latLongs1.Length; ++i)
			{
				Assert.assertEquals(latLongs1[i].Length, latLongs2[i].Length);

				for (int j = 0; j < latLongs1[i].Length; ++j)
				{
					LatLong latLong1 = latLongs1[i][j];
					LatLong latLong2 = latLongs2[i][j];

					Assert.assertEquals(latLong1.latitude, latLong2.latitude, 0.000001);
					Assert.assertEquals(latLong1.longitude, latLong2.longitude, 0.000001);
				}
			}
		}

		private static void checkPointOfInterest(PointOfInterest pointOfInterest)
		{
			Assert.assertEquals(7, pointOfInterest.layer);
			Assert.assertEquals(0.04, pointOfInterest.position.latitude, 0.000001);
			Assert.assertEquals(0.08, pointOfInterest.position.longitude, 0);
			Assert.assertEquals(4, pointOfInterest.tags.size());
			Assert.assertTrue(pointOfInterest.tags.contains(new Tag("place=country")));
			Assert.assertTrue(pointOfInterest.tags.contains(new Tag("name=АБВГДЕЖЗ")));
			Assert.assertTrue(pointOfInterest.tags.contains(new Tag("addr:housenumber=абвгдежз")));
			Assert.assertTrue(pointOfInterest.tags.contains(new Tag("ele=25")));
		}

		private static void checkWay(Way way)
		{
			Assert.assertEquals(4, way.layer);
			Assert.assertNull(way.labelPosition);

			LatLong latLong1 = new LatLong(0.00, 0.00, true);
			LatLong latLong2 = new LatLong(0.04, 0.08, true);
			LatLong latLong3 = new LatLong(0.08, 0.00, true);
			LatLong[][] latLongsExpected = new LatLong[][]
			{
				new LatLong[] {latLong1, latLong2, latLong3}
			};

			assertLatLongsEquals(latLongsExpected, way.latLongs);
			Assert.assertEquals(3, way.tags.size());
			Assert.assertTrue(way.tags.contains(new Tag("highway=motorway")));
			Assert.assertTrue(way.tags.contains(new Tag("name=ÄÖÜ")));
			Assert.assertTrue(way.tags.contains(new Tag("ref=äöü")));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeQueryTest()
		public virtual void executeQueryTest()
		{
			MapFile mapFile = new MapFile(MAP_FILE);

			MapFileInfo mapFileInfo = mapFile.MapFileInfo;
			Assert.assertTrue(mapFileInfo.debugFile);

			for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
				int tileX = MercatorProjection.longitudeToTileX(0.04, zoomLevel);
				int tileY = MercatorProjection.latitudeToTileY(0.04, zoomLevel);
				Tile tile = new Tile(tileX, tileY, zoomLevel, 256);

				MapReadResult mapReadResult = mapFile.readMapData(tile);

				Assert.assertEquals(1, mapReadResult.pointOfInterests.size());
				Assert.assertEquals(1, mapReadResult.ways.size());

				checkPointOfInterest(mapReadResult.pointOfInterests.get(0));
				checkWay(mapReadResult.ways.get(0));
			}

			mapFile.close();
		}
	}

}