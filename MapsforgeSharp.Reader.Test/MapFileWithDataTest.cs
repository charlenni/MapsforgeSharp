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

namespace org.mapsforge.reader
{
    using NUnit.Framework;
    using PCLStorage;

    using LatLong = MapsforgeSharp.Core.Model.LatLong;
    using Tag = MapsforgeSharp.Core.Model.Tag;
    using Tile = MapsforgeSharp.Core.Model.Tile;
    using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
    using MapReadResult = MapsforgeSharp.Core.Datastore.MapReadResult;
    using PointOfInterest = MapsforgeSharp.Core.Datastore.PointOfInterest;
    using Way = MapsforgeSharp.Core.Datastore.Way;
    using MapFileInfo = org.mapsforge.reader.header.MapFileInfo;

    public class MapFileWithDataTest
	{
		private static readonly IFile MAP_FILE = FileSystem.Current.LocalStorage.GetFileAsync(PortablePath.Combine(new string[] { "resources", "with_data", "output.map" })).Result;
		private const sbyte ZOOM_LEVEL_MAX = 11;
		private const int ZOOM_LEVEL_MIN = 6;

		private static void AssertLatLongsEquals(LatLong[][] latLongs1, LatLong[][] latLongs2)
		{
			Assert.AreEqual(latLongs1.Length, latLongs2.Length);

			for (int i = 0; i < latLongs1.Length; ++i)
			{
				Assert.AreEqual(latLongs1[i].Length, latLongs2[i].Length);

				for (int j = 0; j < latLongs1[i].Length; ++j)
				{
					LatLong latLong1 = latLongs1[i][j];
					LatLong latLong2 = latLongs2[i][j];

					Assert.AreEqual(latLong1.Latitude, latLong2.Latitude, 0.000001);
					Assert.AreEqual(latLong1.Longitude, latLong2.Longitude, 0.000001);
				}
			}
		}

		private static void CheckPointOfInterest(PointOfInterest pointOfInterest)
		{
			Assert.AreEqual(7, pointOfInterest.Layer);
			Assert.AreEqual(0.04, pointOfInterest.Position.Latitude, 0.000001);
			Assert.AreEqual(0.08, pointOfInterest.Position.Longitude, 0);
			Assert.AreEqual(4, pointOfInterest.Tags.Count);
			Assert.True(pointOfInterest.Tags.Contains(new Tag("place=country")));
			Assert.True(pointOfInterest.Tags.Contains(new Tag("name=АБВГДЕЖЗ")));
			Assert.True(pointOfInterest.Tags.Contains(new Tag("addr:housenumber=абвгдежз")));
			Assert.True(pointOfInterest.Tags.Contains(new Tag("ele=25")));
		}

		private static void CheckWay(Way way)
		{
			Assert.AreEqual(4, way.Layer);
			Assert.Null(way.LabelPosition);

			LatLong latLong1 = new LatLong(0.00, 0.00, true);
			LatLong latLong2 = new LatLong(0.04, 0.08, true);
			LatLong latLong3 = new LatLong(0.08, 0.00, true);
			LatLong[][] latLongsExpected = new LatLong[][]
			{
				new LatLong[] {latLong1, latLong2, latLong3}
			};

			AssertLatLongsEquals(latLongsExpected, way.LatLongs);
			Assert.AreEqual(3, way.Tags.Count);
			Assert.True(way.Tags.Contains(new Tag("highway=motorway")));
			Assert.True(way.Tags.Contains(new Tag("name=ÄÖÜ")));
			Assert.True(way.Tags.Contains(new Tag("ref=äöü")));
		}

        [Test()]
		public virtual void ExecuteQueryTest()
		{
			MapFile mapFile = new MapFile(MAP_FILE);

			MapFileInfo mapFileInfo = mapFile.MapFileInfo;
			Assert.True(mapFileInfo.DebugFile);

			for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
				int tileX = MercatorProjection.LongitudeToTileX(0.04, zoomLevel);
				int tileY = MercatorProjection.LatitudeToTileY(0.04, zoomLevel);
				Tile tile = new Tile(tileX, tileY, zoomLevel, 256);

				MapReadResult mapReadResult = mapFile.ReadMapData(tile);

				Assert.AreEqual(1, mapReadResult.PointOfInterests.Count);
				Assert.AreEqual(1, mapReadResult.Ways.Count);

				CheckPointOfInterest(mapReadResult.PointOfInterests[0]);
				CheckWay(mapReadResult.Ways[0]);
			}

			mapFile.Close();
		}
	}
}