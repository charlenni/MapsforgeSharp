/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 devemux86
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
	using System.Collections.Generic;
	using Acrotech.PortableLogAdapter;
	using MapsforgeSharp.Core.Util;

	using ITileBitmap = MapsforgeSharp.Core.Graphics.ITileBitmap;
	using Job = org.mapsforge.map.layer.queue.Job;
	using Observable = org.mapsforge.map.model.common.Observable;
	using Observer = org.mapsforge.map.model.common.Observer;
	/// <summary>
	/// A thread-safe cache for tile images with a variable size and LRU policy.
	/// </summary>
	public class InMemoryTileCache : TileCache
	{
        private static readonly ILogger LOGGER = (new Acrotech.PortableLogAdapter.Managers.DelegateLogManager((logger, message) => System.Diagnostics.Debug.WriteLine("[{0}]{1}", logger.Name, message), LogLevel.Info)).GetLogger(nameof(FileSystemTileCache));

        private BitmapLRUCache lruCache;
		private Observable observable;

		/// <param name="capacity">
		///            the maximum number of entries in this cache. </param>
		/// <exception cref="IllegalArgumentException">
		///             if the capacity is negative. </exception>
		public InMemoryTileCache(int capacity)
		{
			this.lruCache = new BitmapLRUCache(capacity);
			this.observable = new Observable();
		}

		public virtual bool ContainsKey(Job key)
		{
			lock (this)
			{
				return this.lruCache.ContainsKey(key);
			}
		}

		public virtual void Destroy()
		{
			lock (this)
			{
				Purge();
			}
		}

		public virtual ITileBitmap Get(Job key)
		{
			lock (this)
			{
				ITileBitmap bitmap = this.lruCache.Get(key);
				if (bitmap != null)
				{
					bitmap.IncrementRefCount();
				}
				return bitmap;
			}
		}

		public virtual int Capacity
		{
			get
			{
				lock (this)
				{
					return this.lruCache.Capacity;
				}
			}
			set
			{
				lock (this)
				{
					BitmapLRUCache lruCacheNew = new BitmapLRUCache(value);
                    // TODO
					//lruCacheNew.putAll(this.lruCache);
					this.lruCache = lruCacheNew;
				}
			}
		}

		public virtual int CapacityFirstLevel
		{
			get
			{
				return Capacity;
			}
		}

		public virtual ITileBitmap GetImmediately(Job key)
		{
			return Get(key);
		}

		public virtual void Purge()
		{
			//foreach (TileBitmap bitmap in this.lruCache.values())
			//{
			//	bitmap.DecrementRefCount();
			//}
			this.lruCache.Clear();
		}

		public virtual void Put(Job key, ITileBitmap bitmap)
		{
			lock (this)
			{
				if (key == null)
				{
					throw new System.ArgumentException("key must not be null");
				}
				else if (bitmap == null)
				{
					throw new System.ArgumentException("bitmap must not be null");
				}
        
				ITileBitmap old = this.lruCache.Get(key);
				if (old != null)
				{
					old.DecrementRefCount();
				}
        
				if (this.lruCache.Add(key, bitmap) != null)
				{
					LOGGER.Warn("overwriting cached entry: " + key);
				}
				bitmap.IncrementRefCount();
				this.observable.NotifyObservers();
			}
		}

		public virtual ISet<Job> WorkingSet
		{
			set
			{
				lock (this)
				{
					this.lruCache.WorkingSet = value;
				}
			}
		}

		public void AddObserver(Observer observer)
		{
			this.observable.AddObserver(observer);
		}

		public void RemoveObserver(Observer observer)
		{
			this.observable.RemoveObserver(observer);
		}
	}

	internal class BitmapLRUCache : WorkingSetCache<Job, ITileBitmap>
	{
		private const long serialVersionUID = 1L;

		public BitmapLRUCache(int capacity) : base(capacity)
		{
		}

		protected override bool RemoveEldestEntry(KeyValuePair<Job, ITileBitmap> eldest)
		{
			if (this.Size() > this.Capacity)
			{
				ITileBitmap bitmap = eldest.Value;
				if (bitmap != null)
				{
					bitmap.DecrementRefCount();
				}
				return true;
			}
			return false;
		}
	}
}