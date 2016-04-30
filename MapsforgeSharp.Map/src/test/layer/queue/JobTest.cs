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
	using Tile = org.mapsforge.core.model.Tile;

	public class JobTest
	{

		private const int TILE_SIZE = 256;

		private static Job createJob(Tile tile)
		{
			return new Job(tile, false);
		}

		private static void verifyInvalidConstructor(Tile tile)
		{
			try
			{
				createJob(tile);
				Assert.fail("tile: " + tile);
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
			Job job1 = new Job(new Tile(0, 1, (sbyte) 2, TILE_SIZE), false);
			Job job2 = new Job(new Tile(0, 1, (sbyte) 2, TILE_SIZE), false);
			Job job3 = new Job(new Tile(0, 0, (sbyte) 0, TILE_SIZE), false);

			TestUtils.equalsTest(job1, job2);

			Assert.assertNotEquals(job1, job3);
			Assert.assertNotEquals(job3, job1);
			Assert.assertNotEquals(job1, new object());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void jobTest()
		public virtual void jobTest()
		{
			Job job = createJob(new Tile(0, 1, (sbyte) 2, TILE_SIZE));
			Assert.assertEquals(new Tile(0, 1, (sbyte) 2, TILE_SIZE), job.tile);

			verifyInvalidConstructor(null);
		}
	}

}