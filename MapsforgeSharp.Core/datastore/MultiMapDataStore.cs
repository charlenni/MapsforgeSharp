/*
 * Copyright 2014-2015 Ludwig M Brinckmann
 * Copyright 2015 devemux86
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

namespace MapsforgeSharp.Core.Datastore
{
    using System;
    using System.Collections.Generic;

    using BoundingBox = MapsforgeSharp.Core.Model.BoundingBox;
	using LatLong = MapsforgeSharp.Core.Model.LatLong;
	using Tile = MapsforgeSharp.Core.Model.Tile;

	/// <summary>
	/// A MapDatabase that reads and combines data from multiple map files.
	/// The MultiMapDatabase supports the following modes for reading from multiple files:
	/// - RETURN_FIRST: the data from the first database to support a tile will be returned. This is the
	/// fastest operation suitable when you know there is no overlap between map files.
	/// - RETURN_ALL: the data from all files will be returned, the data will be combined. This is suitable
	/// if more than one file can contain data for a tile, but you know there is no semantic overlap, e.g.
	/// one file contains contour lines, another road data.
	/// - DEDUPLICATE: the data from all files will be returned but duplicates will be eliminated. This is
	/// suitable when multiple maps cover the different areas, but there is some overlap at boundaries. This
	/// is the most expensive operation and often it is actually faster to double paint objects as otherwise
	/// all objects have to be compared with all others.
	/// </summary>
	public class MultiMapDataStore : MapDataStore
	{
		public enum DataPolicy
		{
			RETURN_FIRST, // return the first set of data
			RETURN_ALL, // return all data from databases
			DEDUPLICATE // return all data but eliminate duplicates
		}

		private BoundingBox boundingBox;
		private readonly DataPolicy dataPolicy;
		private readonly IList<MapDataStore> mapDatabases;
		private LatLong startPosition;
		private sbyte? startZoomLevel;

		public MultiMapDataStore(DataPolicy dataPolicy)
		{
			this.dataPolicy = dataPolicy;
			this.mapDatabases = new List<MapDataStore>();
		}

		/// <summary>
		/// adds another mapDataStore </summary>
		/// <param name="mapDataStore"> the mapDataStore to add </param>
		/// <param name="useStartZoomLevel"> if true, use the start zoom level of this mapDataStore as the start zoom level </param>
		/// <param name="useStartPosition"> if true, use the start position of this mapDataStore as the start position </param>
		public virtual void AddMapDataStore(MapDataStore mapDataStore, bool useStartZoomLevel, bool useStartPosition)
		{
			if (this.mapDatabases.Contains(mapDataStore))
			{
				throw new System.ArgumentException("Duplicate map database");
			}
			this.mapDatabases.Add(mapDataStore);
			if (useStartZoomLevel)
			{
				this.startZoomLevel = mapDataStore.StartZoomLevel.Value;
			}
			if (useStartPosition)
			{
				this.startPosition = mapDataStore.StartPosition;
			}
			if (null == this.boundingBox)
			{
				this.boundingBox = mapDataStore.BoundingBox;
			}
			else
			{
				this.boundingBox = this.boundingBox.ExtendBoundingBox(mapDataStore.BoundingBox);
			}
		}

		public override void Close()
		{
			foreach (MapDataStore mdb in mapDatabases)
			{
				mdb.Close();
			}
		}

		/// <summary>
		/// Returns the timestamp of the data used to render a specific tile.
		/// <para>
		/// If the tile uses data from multiple data stores, the most recent timestamp is returned.
		/// 
		/// </para>
		/// </summary>
		/// <param name="tile">
		///            A tile. </param>
		/// <returns> the timestamp of the data used to render the tile </returns>
		public override long GetDataTimestamp(Tile tile)
		{
			switch (this.dataPolicy)
			{
				case DataPolicy.RETURN_FIRST:
					foreach (MapDataStore mdb in mapDatabases)
					{
						if (mdb.SupportsTile(tile))
						{
							return mdb.GetDataTimestamp(tile);
						}
					}
					return 0;
				case DataPolicy.RETURN_ALL:
				case DataPolicy.DEDUPLICATE:
					long result = 0;
					foreach (MapDataStore mdb in mapDatabases)
					{
						if (mdb.SupportsTile(tile))
						{
							result = Math.Max(result, mdb.GetDataTimestamp(tile));
						}
					}
					return result;
			}
			throw new System.InvalidOperationException("Invalid data policy for multi map database");
		}

		public override MapReadResult ReadMapData(Tile tile)
		{
			switch (this.dataPolicy)
			{
				case DataPolicy.RETURN_FIRST:
					foreach (MapDataStore mdb in mapDatabases)
					{
						if (mdb.SupportsTile(tile))
						{
							return mdb.ReadMapData(tile);
						}
					}
					return null;
				case DataPolicy.RETURN_ALL:
					return ReadMapData(tile, false);
				case DataPolicy.DEDUPLICATE:
					return ReadMapData(tile, true);
			}
			throw new System.InvalidOperationException("Invalid data policy for multi map database");
		}

		private MapReadResult ReadMapData(Tile tile, bool deduplicate)
		{
			MapReadResult mapReadResult = new MapReadResult();
			bool first = true;
			foreach (MapDataStore mdb in mapDatabases)
			{
				if (mdb.SupportsTile(tile))
				{
					MapReadResult result = mdb.ReadMapData(tile);
					if (result == null)
					{
						continue;
					}
					bool isWater = mapReadResult.IsWater & result.IsWater;
					mapReadResult.IsWater = isWater;


					if (first)
					{
						((List<Way>)mapReadResult.Ways).AddRange(result.Ways);
					}
					else
					{
						if (deduplicate)
						{
							foreach (Way way in result.Ways)
							{
								if (!mapReadResult.Ways.Contains(way))
								{
									mapReadResult.Ways.Add(way);
								}
							}
						}
						else
						{
							((List<Way>)mapReadResult.Ways).AddRange(result.Ways);
						}
					}
					if (first)
					{
						((List<PointOfInterest>)mapReadResult.PointOfInterests).AddRange(result.PointOfInterests);
					}
					else
					{
						if (deduplicate)
						{
							foreach (PointOfInterest poi in result.PointOfInterests)
							{
								if (!mapReadResult.PointOfInterests.Contains(poi))
								{
									mapReadResult.PointOfInterests.Add(poi);
								}
							}
						}
						else
						{
							((List<PointOfInterest>)mapReadResult.PointOfInterests).AddRange(result.PointOfInterests);
						}
					}
					first = false;
				}
			}
			return mapReadResult;
		}

        public override BoundingBox BoundingBox
        {
            get
            {
                return this.boundingBox;
            }
            set
            {
                this.boundingBox = value;
            }
        }

        public override LatLong StartPosition
		{
            get
            {
                if (null != this.startPosition)
                {
                    return this.startPosition;
                }
                if (null != this.boundingBox)
                {
                    return this.boundingBox.CenterPoint;
                }
                return null;
            }
			set
			{
				this.startPosition = value;
			}
		}

		public override sbyte? StartZoomLevel
		{
            get
            {
                return startZoomLevel;
            }
            set
			{
				this.startZoomLevel = value;
			}
		}

		public override bool SupportsTile(Tile tile)
		{
			foreach (MapDataStore mdb in mapDatabases)
			{
				if (mdb.SupportsTile(tile))
				{
					return true;
				}
			}
			return false;
		}
	}
}