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
 
 namespace org.mapsforge.map.layer.renderer
{
    using System.Collections.Generic;

    using MapElementContainer = MapsforgeSharp.Core.Mapelements.MapElementContainer;
	using Tile = MapsforgeSharp.Core.Model.Tile;

	/// <summary>
	/// The TileDependecies class tracks the dependencies between tiles for labels.
	/// When the labels are drawn on a per-tile basis it is important to know where
	/// labels overlap the tile boundaries. A single label can overlap several neighbouring
	/// tiles (even, as we do here, ignore the case where a long or tall label will overlap
	/// onto tiles further removed -- with line breaks for long labels this should happen
	/// much less frequently now.).
	/// For every tile drawn we must therefore enquire which labels from neighbouring tiles
	/// overlap onto it and these labels must be drawn regardless of priority as part of the
	/// label has already been drawn.
	/// </summary>
	public class TileDependencies
	{
		internal IDictionary<Tile, IDictionary<Tile, ISet<MapElementContainer>>> overlapData;
		// for the multithreaded renderer we also need to keep track of tiles that are in progress
		// and not yet in the TileCache to avoid truncated labels.
		internal ISet<Tile> tilesInProgress;

		internal TileDependencies()
		{
			overlapData = new Dictionary<Tile, IDictionary<Tile, ISet<MapElementContainer>>>();
			tilesInProgress = new HashSet<Tile>();
		}

		/// <summary>
		/// stores an MapElementContainer that clashesWith from one tile (the one being drawn) to
		/// another (which must not have been drawn before). </summary>
		/// <param name="from"> origin tile </param>
		/// <param name="to"> tile the label clashesWith to </param>
		/// <param name="element"> the MapElementContainer in question </param>
		internal virtual void AddOverlappingElement(Tile from, Tile to, MapElementContainer element)
		{
			if (!overlapData.ContainsKey(from))
			{
				overlapData[from] = new Dictionary<Tile, ISet<MapElementContainer>>();
			}
			if (!overlapData[from].ContainsKey(to))
			{
				overlapData[from][to] = new HashSet<MapElementContainer>();
			}
			overlapData[from][to].Add(element);
		}

		/// <summary>
		/// Retrieves the overlap data from the neighbouring tiles </summary>
		/// <param name="from"> the origin tile </param>
		/// <param name="to"> the tile the label clashesWith to </param>
		/// <returns> a List of the elements </returns>
		internal virtual ISet<MapElementContainer> GetOverlappingElements(Tile from, Tile to)
		{
			if (overlapData.ContainsKey(from) && overlapData[from].ContainsKey(to))
			{
				return overlapData[from][to];
			}
			return new HashSet<MapElementContainer>();
		}

		/// <summary>
		/// Cache maintenance operation to remove data for a tile from the cache. This should be excuted
		/// if a tile is removed from the TileCache and will be drawn again. </summary>
		/// <param name="from"> </param>
		internal virtual void RemoveTileData(Tile from)
		{
			overlapData.Remove(from);
		}
		/// <summary>
		/// Cache maintenance operation to remove data for a tile from the cache. This should be excuted
		/// if a tile is removed from the TileCache and will be drawn again. </summary>
		/// <param name="from"> </param>
		internal virtual void RemoveTileData(Tile from, Tile to)
		{
			if (overlapData.ContainsKey(from))
			{
				overlapData[from].Remove(to);
			}
		}

		internal virtual bool IsTileInProgress(Tile tile)
		{
			lock (this)
			{
				return tilesInProgress.Contains(tile);
			}
		}

		internal virtual void AddTileInProgress(Tile tileInProgress)
		{
			lock (this)
			{
				tilesInProgress.Add(tileInProgress);
			}
		}

		internal virtual void RemoveTileInProgress(Tile tileFinished)
		{
			lock (this)
			{
				tilesInProgress.Remove(tileFinished);
			}
		}
	}
}