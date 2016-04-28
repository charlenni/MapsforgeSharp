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


	public abstract class TileData
	{
		/// <summary>
		/// Add a POI to the tile.
		/// </summary>
		/// <param name="poi">
		///            the POI </param>
		public abstract void addPOI(TDNode poi);

		/// <summary>
		/// Add a way to the tile.
		/// </summary>
		/// <param name="way">
		///            the way </param>
		public abstract void addWay(TDWay way);

		/// <summary>
		/// Gets all POIs of this tile that are seen in the given zoom interval.
		/// </summary>
		/// <param name="minValidZoomlevel">
		///            the minimum zoom level (inclusive) </param>
		/// <param name="maxValidZoomlevel">
		///            the maximum zoom level (inclusive) </param>
		/// <returns> a map that maps from zoom levels to list of nodes </returns>
		public abstract IDictionary<sbyte?, IList<TDNode>> poisByZoomlevel(sbyte minValidZoomlevel, sbyte maxValidZoomlevel);

		/// <summary>
		/// Gets all ways of this tile that are seen in the given zoom interval.
		/// </summary>
		/// <param name="minValidZoomlevel">
		///            the minimum zoom level (inclusive) </param>
		/// <param name="maxValidZoomlevel">
		///            the maximum zoom level (inclusive) </param>
		/// <returns> a map that maps from zoom levels to list of ways </returns>
		public abstract IDictionary<sbyte?, IList<TDWay>> waysByZoomlevel(sbyte minValidZoomlevel, sbyte maxValidZoomlevel);
	}

}