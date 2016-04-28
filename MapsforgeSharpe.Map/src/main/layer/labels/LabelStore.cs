/*
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2016 Dirk Weltz
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

namespace org.mapsforge.map.layer.labels
{
    using System.Collections.Generic;

    using MapElementContainer = org.mapsforge.core.mapelements.MapElementContainer;
	using Tile = org.mapsforge.core.model.Tile;

	/// <summary>
	/// The LabelStore is an abstract store for labels from which it is possible to retrieve a priority-ordered
	/// queue of items that are visible within a given bounding box for a zoom level.
	/// </summary>
	public interface LabelStore
	{
		/// <summary>
		/// Clears the data.
		/// </summary>
		void Clear();

		/// <summary>
		/// Returns a version number, which changes every time an update is made to the LabelStore. </summary>
		/// <returns> the version number </returns>
		int Version {get;}

		/// <summary>
		/// Gets the items that are visible on a set of tiles. </summary>
		/// <param name="tiles"> the set of tiles to get the labels for </param>
		/// <returns> a list of MapElements that are visible on the tiles </returns>
		IList<MapElementContainer> GetVisibleItems(ISet<Tile> tiles);
	}
}