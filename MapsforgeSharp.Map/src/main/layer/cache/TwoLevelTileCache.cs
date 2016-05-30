/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

namespace org.mapsforge.map.layer.cache
{
    using System;
    using System.Collections.Generic;

    using ITileBitmap = MapsforgeSharp.Core.Graphics.ITileBitmap;
	using Job = org.mapsforge.map.layer.queue.Job;
	using Observer = org.mapsforge.map.model.common.Observer;

	public class TwoLevelTileCache : TileCache
	{

		private readonly TileCache firstLevelTileCache;
		private readonly TileCache secondLevelTileCache;
		private readonly ISet<Job> workingSet;

		public TwoLevelTileCache(TileCache firstLevelTileCache, TileCache secondLevelTileCache)
		{
			this.firstLevelTileCache = firstLevelTileCache;
			this.secondLevelTileCache = secondLevelTileCache;
			this.workingSet = System.Collections.synchronizedSet(new HashSet<Job>());
		}

		public virtual bool ContainsKey(Job key)
		{
			return this.firstLevelTileCache.ContainsKey(key) || this.secondLevelTileCache.ContainsKey(key);
		}

		public virtual void Destroy()
		{
			this.firstLevelTileCache.Destroy();
			this.secondLevelTileCache.Destroy();
		}

		public virtual ITileBitmap Get(Job key)
		{
			ITileBitmap returnBitmap = this.firstLevelTileCache.Get(key);
			if (returnBitmap != null)
			{
				return returnBitmap;
			}
			returnBitmap = this.secondLevelTileCache.Get(key);
			if (returnBitmap != null)
			{
				this.firstLevelTileCache.Put(key, returnBitmap);
				return returnBitmap;
			}
			return null;
		}

		public virtual int Capacity
		{
			get
			{
				return Math.Max(this.firstLevelTileCache.Capacity, this.secondLevelTileCache.Capacity);
			}
		}

		public virtual int CapacityFirstLevel
		{
			get
			{
				return this.firstLevelTileCache.Capacity;
			}
		}

		public virtual ITileBitmap GetImmediately(Job key)
		{
			return firstLevelTileCache.Get(key);
		}

		public virtual void Purge()
		{
			this.firstLevelTileCache.Purge();
			this.secondLevelTileCache.Purge();
		}

		public virtual void Put(Job key, ITileBitmap bitmap)
		{
			if (this.workingSet.Contains(key))
			{
				this.firstLevelTileCache.Put(key, bitmap);
			}
			this.secondLevelTileCache.Put(key, bitmap);
		}

		public virtual ISet<Job> WorkingSet
		{
			set
			{
				this.workingSet.Clear();
				this.workingSet.addAll(value);
				this.firstLevelTileCache.WorkingSet = this.workingSet;
				this.secondLevelTileCache.WorkingSet = this.workingSet;
				foreach (Job job in workingSet)
				{
					if (!firstLevelTileCache.ContainsKey(job) && secondLevelTileCache.ContainsKey(job))
					{
						ITileBitmap tileBitmap = secondLevelTileCache.Get(job);
						if (tileBitmap != null)
						{
							firstLevelTileCache.Put(job, tileBitmap);
						}
					}
				}
			}
		}

		public void AddObserver(Observer observer)
		{
			this.firstLevelTileCache.AddObserver(observer);
			this.secondLevelTileCache.AddObserver(observer);
		}

		public void RemoveObserver(Observer observer)
		{
			this.secondLevelTileCache.RemoveObserver(observer);
			this.firstLevelTileCache.RemoveObserver(observer);
		}
	}
}