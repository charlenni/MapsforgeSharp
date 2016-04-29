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
namespace org.mapsforge.map.writer.util
{

	using Test = org.junit.Test;
	using TDWay = org.mapsforge.map.writer.model.TDWay;

	using Geometry = com.vividsolutions.jts.geom.Geometry;
	using LineString = com.vividsolutions.jts.geom.LineString;
	using MultiLineString = com.vividsolutions.jts.geom.MultiLineString;
	using Polygon = com.vividsolutions.jts.geom.Polygon;
	using Assert = com.vividsolutions.jts.util.Assert;

	public class JTSUtilsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBuildGeometryFromInValidPolygon()
		public virtual void testBuildGeometryFromInValidPolygon()
		{
			// Some of these tests do not really make sense, as not everything that is a closed line
			// should be a polygon in OSM.
			string testfile = "invalid-polygon.wkt";
			// String expectedfile = "invalid-polygon-repaired.wkt";

			IList<TDWay> ways = MockingUtils.wktPolygonToWays(testfile);
			Geometry geometry = JTSUtils.toJtsGeometry(ways[0], ways.subList(1, ways.Count));
			Assert.isTrue(geometry is LineString);
			Assert.isTrue(geometry.Valid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBuildGeometryFromInValidPolygonWithHoles()
		public virtual void testBuildGeometryFromInValidPolygonWithHoles()
		{
			string testfile = "invalid-polygon-2-inner-rings.wkt";
			string expectedfile = "invalid-polygon-2-inner-rings-repaired.wkt";

			IList<TDWay> ways = MockingUtils.wktPolygonToWays(testfile);
			Geometry geometry = JTSUtils.toJtsGeometry(ways[0], ways.subList(1, ways.Count));
			Assert.isTrue(geometry is Polygon);
			Assert.isTrue(geometry.Valid);

			Geometry expected = MockingUtils.readWKTFile(expectedfile);
			Assert.Equals(expected, geometry);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBuildGeometryFromValidPolygon()
		public virtual void testBuildGeometryFromValidPolygon()
		{
			string testfile = "valid-polygon.wkt";

			IList<TDWay> ways = MockingUtils.wktPolygonToWays(testfile);
			Geometry geometry = JTSUtils.toJtsGeometry(ways[0], ways.subList(1, ways.Count));
			Assert.isTrue(geometry is LineString);
			Assert.isTrue(geometry.Valid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBuildInvalidPolygon()
		public virtual void testBuildInvalidPolygon()
		{
			string testfile = "invalid-polygon.wkt";

			IList<TDWay> ways = MockingUtils.wktPolygonToWays(testfile);
			Polygon polygon = JTSUtils.buildPolygon(ways[0]);
			Geometry expected = MockingUtils.readWKTFile(testfile);
			Assert.isTrue(!polygon.Valid);
			Assert.Equals(expected, polygon);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBuildInValidPolygonWith2InnerRings()
		public virtual void testBuildInValidPolygonWith2InnerRings()
		{
			string testfile = "invalid-polygon-2-inner-rings.wkt";

			IList<TDWay> ways = MockingUtils.wktPolygonToWays(testfile);
			Polygon polygon = JTSUtils.buildPolygon(ways[0], ways.subList(1, ways.Count));
			Geometry expected = MockingUtils.readWKTFile(testfile);
			Assert.isTrue(!polygon.Valid);
			Assert.Equals(expected, polygon);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBuildNonSimpleMultiLineString()
		public virtual void testBuildNonSimpleMultiLineString()
		{
			string testfile = "non-simple-multilinestring.wkt";

			IList<TDWay> ways = MockingUtils.wktMultiLineStringToWays(testfile);
			MultiLineString mls = JTSUtils.buildMultiLineString(ways[0], ways.subList(1, ways.Count));
			Geometry expected = MockingUtils.readWKTFile(testfile);
			Assert.isTrue(!mls.Simple);
			Assert.Equals(expected, mls);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBuildValidMultiLineString()
		public virtual void testBuildValidMultiLineString()
		{
			string testfile = "valid-multilinestring.wkt";

			IList<TDWay> ways = MockingUtils.wktMultiLineStringToWays(testfile);
			MultiLineString mls = JTSUtils.buildMultiLineString(ways[0], ways.subList(1, ways.Count));
			Geometry expected = MockingUtils.readWKTFile(testfile);
			Assert.isTrue(mls.Valid);
			Assert.Equals(expected, mls);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBuildValidPolygon()
		public virtual void testBuildValidPolygon()
		{
			string testfile = "valid-polygon.wkt";

			IList<TDWay> ways = MockingUtils.wktPolygonToWays(testfile);
			Polygon polygon = JTSUtils.buildPolygon(ways[0]);
			Geometry expected = MockingUtils.readWKTFile(testfile);
			Assert.isTrue(polygon.Valid);
			Assert.Equals(expected, polygon);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBuildValidPolygonWith2InnerRings()
		public virtual void testBuildValidPolygonWith2InnerRings()
		{
			string testfile = "valid-polygon-2-inner-rings.wkt";

			IList<TDWay> ways = MockingUtils.wktPolygonToWays(testfile);
			Polygon polygon = JTSUtils.buildPolygon(ways[0], ways.subList(1, ways.Count));
			Geometry expected = MockingUtils.readWKTFile(testfile);
			Assert.isTrue(polygon.Valid);
			Assert.Equals(expected, polygon);
		}
	}

}