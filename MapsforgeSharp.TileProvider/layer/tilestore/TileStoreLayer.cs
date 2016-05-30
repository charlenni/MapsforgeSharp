/*
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.map.layer.tilestore
{
	using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
	using ITileBitmap = MapsforgeSharp.Core.Graphics.ITileBitmap;
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using TileCache = org.mapsforge.map.layer.cache.TileCache;
	using Job = org.mapsforge.map.layer.queue.Job;
	using MapViewPosition = org.mapsforge.map.model.MapViewPosition;

	public class TileStoreLayer : TileLayer<Job>
	{
		public TileStoreLayer(TileCache tileCache, MapViewPosition mapViewPosition, IGraphicFactory graphicFactory, bool isTransparent) : base(tileCache, mapViewPosition, graphicFactory.CreateMatrix(), isTransparent, false)
		{
		}

		protected internal override Job CreateJob(Tile tile)
		{
			return new Job(tile, isTransparent);
		}

		/// <summary>
		/// Whether the tile is stale and should be refreshed.
		/// <para>
		/// This method is not needed for a TileStoreLayer and will always return {@code false}. Both arguments can be null.
		/// 
		/// </para>
		/// </summary>
		/// <param name="tile">
		///            A tile. </param>
		/// <param name="bitmap">
		///            The bitmap for {@code tile} currently held in the layer's cache. </param>
		protected internal override bool IsTileStale(Tile tile, ITileBitmap bitmap)
		{
			return false;
		}
	}
}