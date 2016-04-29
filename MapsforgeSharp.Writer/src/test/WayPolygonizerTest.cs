using System.Collections.Generic;

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
namespace org.mapsforge.map.writer
{


	using Assert = org.junit.Assert;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using TDNode = org.mapsforge.map.writer.model.TDNode;
	using TDWay = org.mapsforge.map.writer.model.TDWay;

	public class WayPolygonizerTest
	{
		private WayPolygonizer polygonizer;
		private TDWay[] ways;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		public virtual void setUp()
		{
			this.polygonizer = new WayPolygonizer();

			this.ways = new TDWay[10];

			TDNode[] n1 = new TDNode[]
			{
				new TDNode(1, 52000000, 13000000, (short) 0, (sbyte) 0, null, "n1"),
				new TDNode(2, 52000000, 13000100, (short) 0, (sbyte) 0, null, "n2"),
				new TDNode(3, 52000100, 13000100, (short) 0, (sbyte) 0, null, "n3"),
				new TDNode(4, 52000100, 13000000, (short) 0, (sbyte) 0, null, "n4"),
				new TDNode(1, 52000000, 13000000, (short) 0, (sbyte) 0, null, "n5")
			};
			TDWay w1 = new TDWay(1, (sbyte) 0, "w1", null, null, n1);

			TDNode[] n2 = new TDNode[]
			{
				new TDNode(5, 52000500, 13000500, (short) 0, (sbyte) 0, null, "n5"),
				new TDNode(6, 52000500, 13000800, (short) 0, (sbyte) 0, null, "n6"),
				new TDNode(7, 52000800, 13000800, (short) 0, (sbyte) 0, null, "n7")
			};
			TDWay w2 = new TDWay(2, (sbyte) 0, "w2", null, null, n2);

			TDNode[] n3 = new TDNode[]
			{
				new TDNode(7, 52000800, 13000800, (short) 0, (sbyte) 0, null, "n7"),
				new TDNode(8, 50001000, 13000800, (short) 0, (sbyte) 0, null, "n8"),
				new TDNode(9, 52001000, 13001000, (short) 0, (sbyte) 0, null, "n9"),
				new TDNode(10, 52001100, 13001000, (short) 0, (sbyte) 0, null, "n10")
			};
			TDWay w3 = new TDWay(3, (sbyte) 0, "w3", null, null, n3);

			TDNode[] n4 = new TDNode[]
			{
				new TDNode(10, 52001100, 13001000, (short) 0, (sbyte) 0, null, "n11"),
				new TDNode(11, 50001100, 13000500, (short) 0, (sbyte) 0, null, "n12"),
				new TDNode(5, 52000500, 13000500, (short) 0, (sbyte) 0, null, "n5")
			};
			TDWay w4 = new TDWay(4, (sbyte) 0, "w4", null, null, n4);

			TDNode[] nDangling1 = new TDNode[]
			{
				new TDNode(12, 52001000, 13001000, (short) 0, (sbyte) 0, null, "n12"),
				new TDNode(13, 50001000, 13001500, (short) 0, (sbyte) 0, null, "n13"),
				new TDNode(14, 52001500, 13001500, (short) 0, (sbyte) 0, null, "n14"),
				new TDNode(15, 52001600, 13001500, (short) 0, (sbyte) 0, null, "n15"),
				new TDNode(16, 52001600, 13001600, (short) 0, (sbyte) 0, null, "n16")
			};
			TDWay wDangling1 = new TDWay(5, (sbyte) 0, "w5", null, null, nDangling1);

			this.ways[0] = w1;
			this.ways[1] = w2;
			this.ways[2] = w3;
			this.ways[3] = w4;
			this.ways[4] = wDangling1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClosedAndUnclosedPolygonWithDangling()
		public virtual void testClosedAndUnclosedPolygonWithDangling()
		{
			TDWay[] testWays = new TDWay[] {this.ways[0], this.ways[1], this.ways[2], this.ways[3], this.ways[4]};
			this.polygonizer.mergePolygons(testWays);
			IList<Deque<TDWay>> polygons = this.polygonizer.Polygons;
			Assert.assertEquals(2, polygons.Count);

			Deque<TDWay> p1 = polygons[0];
			Deque<TDWay> p2 = polygons[1];
			if (p1.size() == 3)
			{
				Deque<TDWay> temp = p1;
				p1 = p2;
				p2 = temp;
			}

			Assert.assertEquals(1, p1.size());
			Assert.assertEquals(3, p2.size());

			Assert.assertTrue(p1.contains(this.ways[0]));
			Assert.assertTrue(p2.contains(this.ways[1]));
			Assert.assertTrue(p2.contains(this.ways[2]));
			Assert.assertTrue(p2.contains(this.ways[3]));

			Assert.assertEquals(1, this.polygonizer.Dangling.size());
			Assert.assertEquals(this.ways[4], this.polygonizer.Dangling.get(0));
			Assert.assertTrue(this.polygonizer.Illegal.size() == 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSingleClosedPolygon()
		public virtual void testSingleClosedPolygon()
		{
			TDWay[] testWays = new TDWay[] {this.ways[0]};
			this.polygonizer.mergePolygons(testWays);
			IList<Deque<TDWay>> polygons = this.polygonizer.Polygons;
			Assert.assertEquals(1, polygons.Count);
			Assert.assertEquals(1, polygons[0].size());
			Assert.assertEquals(5, polygons[0].First.WayNodes.length);
			Assert.assertEquals(1, polygons[0].First.WayNodes[0].Id);
			Assert.assertEquals(1, polygons[0].First.WayNodes[4].Id);

			Assert.assertTrue(polygons[0].contains(this.ways[0]));

			Assert.assertTrue(this.polygonizer.Dangling.size() == 0);
			Assert.assertTrue(this.polygonizer.Illegal.size() == 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSingleUnClosedPolygon()
		public virtual void testSingleUnClosedPolygon()
		{
			TDWay[] testWays = new TDWay[] {this.ways[1], this.ways[2], this.ways[3]};
			this.polygonizer.mergePolygons(testWays);
			IList<Deque<TDWay>> polygons = this.polygonizer.Polygons;
			Assert.assertEquals(1, polygons.Count);
			Assert.assertEquals(3, polygons[0].size());

			Assert.assertTrue(polygons[0].contains(this.ways[1]));
			Assert.assertTrue(polygons[0].contains(this.ways[2]));
			Assert.assertTrue(polygons[0].contains(this.ways[3]));

			Assert.assertTrue(this.polygonizer.Dangling.size() == 0);
			Assert.assertTrue(this.polygonizer.Illegal.size() == 0);
		}
	}

}