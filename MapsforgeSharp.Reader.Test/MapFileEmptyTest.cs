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

    using Tile = MapsforgeSharp.Core.Model.Tile;
    using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
    using MapReadResult = MapsforgeSharp.Core.Datastore.MapReadResult;

    public class MapFileEmptyTest
	{
		private static readonly IFile MAP_FILE = FileSystem.Current.LocalStorage.GetFileAsync(PortablePath.Combine(new string[] { "resources", "empty", "output.map" })).Result;
		private const sbyte ZOOM_LEVEL_MAX = 25;

        [Test()]
		public virtual void ExecuteQueryTest()
		{
			MapFile mapFile = new MapFile(MAP_FILE);

			for (sbyte zoomLevel = 0; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
				int tileX = MercatorProjection.LongitudeToTileX(1, zoomLevel);
				int tileY = MercatorProjection.LatitudeToTileY(1, zoomLevel);
				Tile tile = new Tile(tileX, tileY, zoomLevel, 256);

				MapReadResult mapReadResult = mapFile.ReadMapData(tile);

				Assert.AreEqual(0, mapReadResult.PointOfInterests.Count);
				Assert.AreEqual(0, mapReadResult.Ways.Count);
			}

			mapFile.Close();
		}
	}
}