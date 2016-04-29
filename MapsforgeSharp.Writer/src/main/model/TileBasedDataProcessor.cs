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


	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using Node = org.openstreetmap.osmosis.core.domain.v0_6.Node;
	using Relation = org.openstreetmap.osmosis.core.domain.v0_6.Relation;
	using Way = org.openstreetmap.osmosis.core.domain.v0_6.Way;

	/// <summary>
	/// A TileBasedDataStore allows tile based access to OpenStreetMap geo data. POIs and ways are mapped to tiles on
	/// configured base zoom levels.
	/// </summary>
	public interface TileBasedDataProcessor
	{
		/// <summary>
		/// Add a node to the data store. No association with a tile is performed.
		/// </summary>
		/// <param name="node">
		///            the node </param>
		void addNode(Node node);

		/// <summary>
		/// Add a relation to the data store.
		/// </summary>
		/// <param name="relation">
		///            the relation </param>
		void addRelation(Relation relation);

		/// <summary>
		/// Add a way to the data store.
		/// </summary>
		/// <param name="way">
		///            the way </param>
		void addWay(Way way);

		/// <summary>
		/// Complete the data store, e.g. build indexes or similar.
		/// </summary>
		void complete();

		/// <summary>
		/// Retrieve the total amount of tiles cumulated over all base zoom levels that is needed to represent the underlying
		/// bounding box of this tile data store.
		/// </summary>
		/// <returns> total amount of tiles </returns>
		long cumulatedNumberOfTiles();

		/// <summary>
		/// Get the bounding box that describes this TileBasedDataStore.
		/// </summary>
		/// <returns> The bounding box that defines the area that is covered by the data store. </returns>
		BoundingBox BoundingBox {get;}

		/// <summary>
		/// Retrieve all coastlines that cross the given tile.
		/// </summary>
		/// <param name="tc">
		///            the coordinate of the tile </param>
		/// <returns> all coastlines that cross the tile, an empty set if no coastlines cross </returns>
		ISet<TDWay> getCoastLines(TileCoordinate tc);

		/// <summary>
		/// Retrieve the all the inner ways that are associated with an outer way that represents a multipolygon.
		/// </summary>
		/// <param name="outerWayID">
		///            id of the outer way </param>
		/// <returns> all associated inner ways </returns>
		IList<TDWay> getInnerWaysOfMultipolygon(long outerWayID);

		/// <summary>
		/// Retrieves all the data that is associated with a tile.
		/// </summary>
		/// <param name="baseZoomIndex">
		///            index of the base zoom, as defined in a ZoomIntervalConfiguration </param>
		/// <param name="tileCoordinateX">
		///            x coordinate of the tile </param>
		/// <param name="tileCoordinateY">
		///            y coordinate of the tile </param>
		/// <returns> tile, or null if the tile is outside the bounding box of this tile data store </returns>
		TileData getTile(int baseZoomIndex, int tileCoordinateX, int tileCoordinateY);

		/// <summary>
		/// Get the layout of a grid on the given zoom interval specification.
		/// </summary>
		/// <param name="zoomIntervalIndex">
		///            the index of the zoom interval </param>
		/// <returns> the layout of the grid for the given zoom interval </returns>
		TileGridLayout getTileGridLayout(int zoomIntervalIndex);

		/// <summary>
		/// Get the zoom interval configuration of the data store.
		/// </summary>
		/// <returns> the underlying zoom interval configuration </returns>
		ZoomIntervalConfiguration ZoomIntervalConfiguration {get;}

		/// <summary>
		/// Release all acquired resources, e.g. delete any temporary files.
		/// </summary>
		void release();
	}

}