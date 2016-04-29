using System.Collections;
using System.Collections.Generic;

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
namespace org.mapsforge.map.writer.model
{


	/// <summary>
	/// Encapsulates the information given in the oceantiles_12.dat file. That is the information whether a given tile on
	/// zoom level 12 is completely covered by water, land or is mixed.
	/// </summary>
	public sealed class TileInfo
	{
		/// <summary>
		/// The zoom level for which the tile info is valid.
		/// </summary>
		public const sbyte TILE_INFO_ZOOMLEVEL = 0xC;

		private const sbyte BITMASK = 0x3;

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		private static readonly Logger LOGGER = Logger.getLogger(typeof(TileInfo).FullName);

		// 4096 * 4096 = number of tiles on zoom level 12
		private const int N_BITS = 0x1000000;

		// 4096 * 4096 / 4 (2 bits for each tile)
		private const int N_BYTES = 0x400000;

		private const string OCEAN_TILES_FILE = "oceantiles_12.dat";
		private const sbyte SEA = 0x2;

		/// <returns> the singleton which encapsulates the oceantile_12.dat information </returns>
		public static TileInfo Instance
		{
			get
			{
				return new TileInfo(OCEAN_TILES_FILE);
			}
		}

		private readonly BitArray seaTileInfo = new BitArray(N_BITS);

		private TileInfo(string strInputFile)
		{
			try
			{
				DataInputStream dis = new DataInputStream(typeof(TileInfo).ClassLoader.getResourceAsStream(strInputFile));
				sbyte currentByte;

				long start = DateTimeHelperClass.CurrentUnixTimeMillis();
				for (int i = 0; i < N_BYTES; i++)
				{
					currentByte = dis.readByte();
					if (((currentByte >> 6) & BITMASK) == SEA)
					{
						this.seaTileInfo.Set(i * 4, true);
					}
					if (((currentByte >> 4) & BITMASK) == SEA)
					{
						this.seaTileInfo.Set(i * 4 + 1, true);
					}
					if (((currentByte >> 2) & BITMASK) == SEA)
					{
						this.seaTileInfo.Set(i * 4 + 2, true);
					}
					if ((currentByte & BITMASK) == SEA)
					{
						this.seaTileInfo.Set(i * 4 + 3, true);
					}
				}
				LOGGER.fine("loading of tile info data took " + (DateTimeHelperClass.CurrentUnixTimeMillis() - start) + " ms");
			}
			catch (IOException)
			{
				LOGGER.severe("error loading tile info from file " + strInputFile);
			}
		}

		/// <summary>
		/// Checks if a tile is completely covered by water. <b>Important notice:</b> The method may produce false negatives
		/// on higher zoom levels than 12.
		/// </summary>
		/// <param name="tc">
		///            tile given as TileCoordinate </param>
		/// <returns> true if the tile is completely covered by water, false if the associated tile(s) on zoom level 12 is(are)
		///         not completely covered by water. </returns>
		public bool isWaterTile(TileCoordinate tc)
		{
			IList<TileCoordinate> tiles = tc.translateToZoomLevel(TILE_INFO_ZOOMLEVEL);
			foreach (TileCoordinate tile in tiles)
			{
				if (!this.seaTileInfo.Get(tile.Y * 4096 + tile.X))
				{
					return false;
				}
			}
			return true;
		}
	}

}