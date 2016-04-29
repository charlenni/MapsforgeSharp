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

namespace org.mapsforge.map.layer.queue
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Linq;

    using DisplayModel = org.mapsforge.map.model.DisplayModel;
	using MapViewPosition = org.mapsforge.map.model.MapViewPosition;

	public class JobQueue<T> where T : Job
	{
		private const int QUEUE_CAPACITY = 128;

		private readonly IList<T> assignedJobs = (IList<T>) new LinkedList<T>();
		private readonly DisplayModel displayModel;
		private readonly MapViewPosition mapViewPosition;
		private readonly IList<QueueItem<T>> queueItems = (IList<QueueItem<T>>) new LinkedList<QueueItem<T>>();
		private bool scheduleNeeded;

		public JobQueue(MapViewPosition mapViewPosition, DisplayModel displayModel)
		{
			this.mapViewPosition = mapViewPosition;
			this.displayModel = displayModel;
		}

		public virtual void Add(T job)
		{
			lock (this)
			{
				if (!this.assignedJobs.Contains(job))
				{
					QueueItem<T> queueItem = new QueueItem<T>(job);
					if (!this.queueItems.Contains(queueItem))
					{
						this.queueItems.Add(queueItem);
						this.scheduleNeeded = true;
						this.NotifyWorkers();
					}
				}
			}
		}

		/// <summary>
		/// Returns the most important entry from this queue. The method blocks while this queue is empty.
		/// </summary>
		public virtual T Get()
		{
			lock (this)
			{
				return Get(int.MaxValue);
			}
		}

		/// <summary>
		/// Returns the most important entry from this queue. The method blocks while this queue is empty
		/// or while there are already a certain number of jobs assigned. </summary>
		/// <param name="maxAssigned"> the maximum number of jobs that should be assigned at any one point. If there
		///                    are already so many jobs assigned, the queue will block. This is to ensure
		///                    that the scheduling will continue to work. </param>
		public virtual T Get(int maxAssigned)
		{
			lock (this)
			{
				while (this.queueItems.Count == 0 || this.assignedJobs.Count >= maxAssigned)
				{
					Monitor.Wait(this, TimeSpan.FromMilliseconds(200));
				}
        
				if (this.scheduleNeeded)
				{
					this.scheduleNeeded = false;
					Schedule(displayModel.TileSize);
				}

                T job = this.queueItems[0].@object;
                this.queueItems.RemoveAt(0);
				this.assignedJobs.Add(job);
				return job;
			}
		}

		public virtual void NotifyWorkers()
		{
			lock (this)
			{
				Monitor.PulseAll(this);
			}
		}

		public virtual void Remove(T job)
		{
			lock (this)
			{
				this.assignedJobs.Remove(job);
				this.NotifyWorkers();
			}
		}

		/// <returns> the current number of entries in this queue. </returns>
		public virtual int Size()
		{
			lock (this)
			{
				return this.queueItems.Count;
			}
		}

		private void Schedule(int tileSize)
		{
			QueueItemScheduler.schedule(this.queueItems, this.mapViewPosition.MapPosition, tileSize);
            // TODO
			//this.queueItems.Sort(QueueItemComparator.INSTANCE);
			TrimToSize();
		}

		private void TrimToSize()
		{
			int queueSize = this.queueItems.Count;

			while (queueSize > QUEUE_CAPACITY)
			{
				this.queueItems.RemoveAt(--queueSize);
			}
		}
	}
}