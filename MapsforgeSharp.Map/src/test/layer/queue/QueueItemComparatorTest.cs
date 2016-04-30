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

	public class QueueItemComparatorTest
	{

		private static readonly int[] TILE_SIZES = new int[] {256, 128, 376, 512, 100};

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compareTest()
		public virtual void compareTest()
		{
			foreach (int tileSize in TILE_SIZES)
			{
				Tile tile1 = new Tile(0, 0, (sbyte) 1, tileSize);
				Tile tile2 = new Tile(0, 0, (sbyte) 2, tileSize);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: QueueItem<?> queueItem1 = new QueueItem<Job>(new Job(tile1, false));
				QueueItem<object> queueItem1 = new QueueItem<Job>(new Job(tile1, false));
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: QueueItem<?> queueItem2 = new QueueItem<Job>(new Job(tile2, false));
				QueueItem<object> queueItem2 = new QueueItem<Job>(new Job(tile2, false));

				QueueItemComparator queueItemComparator = QueueItemComparator.INSTANCE;
				Assert.assertEquals(0, queueItemComparator.compare(queueItem1, queueItem2), 0);

				queueItem1.Priority = 1;
				queueItem2.Priority = 2;
				Assert.assertTrue(queueItemComparator.compare(queueItem1, queueItem2) < 0);
				Assert.assertTrue(queueItemComparator.compare(queueItem2, queueItem1) > 0);
			}
		}
	}

}