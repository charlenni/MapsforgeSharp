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

namespace MapsforgeSharp.Reader
{
    using NUnit.Framework;

    using LatLong = MapsforgeSharp.Core.Model.LatLong;
    using Tile = MapsforgeSharp.Core.Model.Tile;
    using MercatorProjection = MapsforgeSharp.Core.Util.MercatorProjection;
    using MapReadResult = MapsforgeSharp.Core.Datastore.MapReadResult;
    using Way = MapsforgeSharp.Core.Datastore.Way;

    internal sealed class EncodingTest
	{
		private const sbyte ZOOM_LEVEL = 8;

		internal static void RunTest(MapFile mapFile)
		{

			int tileX = MercatorProjection.LongitudeToTileX(0, ZOOM_LEVEL);
			int tileY = MercatorProjection.LatitudeToTileY(0, ZOOM_LEVEL);
			Tile tile = new Tile(tileX, tileY, ZOOM_LEVEL, 256);

			MapReadResult mapReadResult = mapFile.ReadMapData(tile);
			mapFile.Close();

			Assert.AreEqual(mapReadResult.PointOfInterests.Count, 0);
			Assert.AreEqual(1, mapReadResult.Ways.Count);

			LatLong latLong1 = new LatLong(0.0, 0.0, true);
			LatLong latLong2 = new LatLong(0.0, 0.1, true);
			LatLong latLong3 = new LatLong(-0.1, 0.1, true);
			LatLong latLong4 = new LatLong(-0.1, 0.0, true);
			LatLong[][] latLongsExpected = new LatLong[][]
			{
				new LatLong[] {latLong1, latLong2, latLong3, latLong4, latLong1}
			};

			Way way = mapReadResult.Ways[0];
            // TODO: Was ArrayEquals()
			Assert.AreEqual(latLongsExpected, way.LatLongs);
		}

		private EncodingTest()
		{
			throw new System.InvalidOperationException();
		}
	}
}