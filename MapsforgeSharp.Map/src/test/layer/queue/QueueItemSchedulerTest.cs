using System;
using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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


	using Assert = org.junit.Assert;
	using Test = org.junit.Test;
	using LatLong = org.mapsforge.core.model.LatLong;
	using MapPosition = org.mapsforge.core.model.MapPosition;
	using Tile = org.mapsforge.core.model.Tile;

	public class QueueItemSchedulerTest
	{

		private static readonly int[] TILE_SIZES = new int[] {256, 128, 376, 512, 100};

		private static ICollection<QueueItem<T>> createCollection<T>(QueueItem<T> queueItem) where T : Job
		{
			ICollection<QueueItem<T>> queueItems = new List<QueueItem<T>>();
			queueItems.Add(queueItem);
			return queueItems;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void scheduleTest()
		public virtual void scheduleTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				Tile tile0 = new Tile(0, 0, (sbyte) 0, tileSize);
				Job job = new Job(tile0, false);
				QueueItem<Job> queueItem = new QueueItem<Job>(job);
				Assert.assertEquals(0, queueItem.Priority, 0);

				MapPosition mapPosition = new MapPosition(new LatLong(0, 0, true), (sbyte) 0);
				QueueItemScheduler.schedule(createCollection(queueItem), mapPosition, tileSize);
				Assert.assertEquals(0, queueItem.Priority, 0);

				mapPosition = new MapPosition(new LatLong(0, 180, true), (sbyte) 0);
				QueueItemScheduler.schedule(createCollection(queueItem), mapPosition, tileSize);
				int halfTileSize = tileSize / 2;
				Assert.assertEquals(halfTileSize, queueItem.Priority, 0);

				mapPosition = new MapPosition(new LatLong(0, -180, true), (sbyte) 0);
				QueueItemScheduler.schedule(createCollection(queueItem), mapPosition, tileSize);
				Assert.assertEquals(halfTileSize, queueItem.Priority, 0);

				mapPosition = new MapPosition(new LatLong(0, 0, true), (sbyte) 1);
				QueueItemScheduler.schedule(createCollection(queueItem), mapPosition, tileSize);
				double expectedPriority = Math.hypot(halfTileSize, halfTileSize) + QueueItemScheduler.PENALTY_PER_ZOOM_LEVEL * tileSize;
				Assert.assertEquals(expectedPriority, queueItem.Priority, 0);
			}
		}
	}

}