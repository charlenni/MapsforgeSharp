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
namespace org.mapsforge.map.layer.queue
{

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;
	using Tile = org.mapsforge.core.model.Tile;

	public class QueueItemTest
	{

		private static readonly int[] TILE_SIZES = new int[] {256, 128, 376, 512, 100};

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static QueueItem<?> createTileDownloadJob(org.mapsforge.core.model.Tile tile, int tileSize)
		private static QueueItem<object> createTileDownloadJob(Tile tile, int tileSize)
		{
			return new QueueItem<Job>(new Job(tile, false));
		}

		private static void verifyInvalidPriority(QueueItem<Job> queueItem, double priority)
		{
			try
			{
				queueItem.Priority = priority;
				Assert.fail("priority: " + priority);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void equalsTest()
		public virtual void equalsTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				Tile tile1 = new Tile(1, 1, (sbyte) 1, tileSize);
				Tile tile2 = new Tile(2, 2, (sbyte) 2, tileSize);

				QueueItem<Job> queueItem1 = new QueueItem<Job>(new Job(tile1, false));
				QueueItem<Job> queueItem2 = new QueueItem<Job>(new Job(tile1, false));
				QueueItem<Job> queueItem3 = new QueueItem<Job>(new Job(tile2, false));

				TestUtils.equalsTest(queueItem1, queueItem2);

				Assert.assertNotEquals(queueItem1, queueItem3);
				Assert.assertNotEquals(queueItem3, queueItem1);
				Assert.assertNotEquals(queueItem1, new object());
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void invalidConstructorTest()
		public virtual void invalidConstructorTest()
		{
			try
			{
				createTileDownloadJob(null, 1);
				Assert.fail();
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void priorityTest()
		public virtual void priorityTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				Tile tile = new Tile(0, 0, (sbyte) 0, tileSize);
				QueueItem<Job> queueItem = new QueueItem<Job>(new Job(tile, false));
				Assert.assertEquals(0, queueItem.Priority, 0);

				queueItem.Priority = 42;
				Assert.assertEquals(42, queueItem.Priority, 0);

				verifyInvalidPriority(queueItem, -1);
				verifyInvalidPriority(queueItem, double.NegativeInfinity);
				verifyInvalidPriority(queueItem, Double.NaN);
			}
		}
	}

}