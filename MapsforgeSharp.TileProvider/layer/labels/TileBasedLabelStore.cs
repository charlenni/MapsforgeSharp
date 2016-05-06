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

namespace org.mapsforge.map.layer.labels
{
	using System.Collections.Generic;
	using MapsforgeSharp.Core.Util;

	using MapElementContainer = MapsforgeSharp.Core.Mapelements.MapElementContainer;
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using LayerUtil = org.mapsforge.map.util.LayerUtil;

	/// <summary>
	/// A LabelStore where the data is stored per tile.
	/// </summary>
	public class TileBasedLabelStore : WorkingSetCache<Tile, ICollection<MapElementContainer>>, LabelStore
	{
		private const long serialVersionUID = 1L;

		private ISet<Tile> lastVisibleTileSet;
		private int version;

		public TileBasedLabelStore(int capacity) : base(capacity)
		{
			lastVisibleTileSet = new HashSet<Tile>();
		}

		public virtual void Destroy()
		{
			this.Clear();
		}

		/// <summary>
		/// Stores a list of MapElements against a tile.
		/// </summary>
		/// <param name="tile"> tile on which the mapItems reside. </param>
		/// <param name="mapItems"> the map elements. </param>
		public virtual void StoreMapItems(Tile tile, ICollection<MapElementContainer> mapItems)
		{
			lock (this)
			{
				this.Add(tile, LayerUtil.CollisionFreeOrdered(mapItems));
				this.version += 1;
			}
		}

		public virtual int Version
		{
			get
			{
				return this.version;
			}
		}

		public virtual IList<MapElementContainer> GetVisibleItems(ISet<Tile> tiles)
		{
			lock (this)
			{
				lastVisibleTileSet = tiles;
        
				IList<MapElementContainer> visibleItems = new List<MapElementContainer>();
				foreach (Tile tile in lastVisibleTileSet)
				{
					if (this.ContainsKey(tile))
					{
						((List<MapElementContainer>)visibleItems).AddRange(Get(tile));
					}
				}
				return visibleItems;
			}
		}

		/// <summary>
		/// Returns if a tile is in the current tile set and no data is stored for this tile. </summary>
		/// <param name="tile"> the tile </param>
		/// <returns> true if the tile is in the current tile set, but no data is stored for it. </returns>
		public virtual bool requiresTile(Tile tile)
		{
			lock (this)
			{
				return this.lastVisibleTileSet.Contains(tile) && !this.ContainsKey(tile);
			}
		}

		protected override bool RemoveEldestEntry(KeyValuePair<Tile, ICollection<MapElementContainer>> eldest)
		{
			if (this.Size() > this.Capacity)
			{
				return true;
			}
			return false;
		}
	}
}