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


	using LatLongUtils = org.mapsforge.core.util.LatLongUtils;
	using TDNode = org.mapsforge.map.writer.model.TDNode;
	using TDWay = org.mapsforge.map.writer.model.TDWay;

	using Coordinate = com.vividsolutions.jts.geom.Coordinate;
	using Geometry = com.vividsolutions.jts.geom.Geometry;
	using GeometryFactory = com.vividsolutions.jts.geom.GeometryFactory;
	using LineString = com.vividsolutions.jts.geom.LineString;
	using MultiLineString = com.vividsolutions.jts.geom.MultiLineString;
	using Polygon = com.vividsolutions.jts.geom.Polygon;
	using ParseException = com.vividsolutions.jts.io.ParseException;
	using WKTReader = com.vividsolutions.jts.io.WKTReader;

	public class MockingUtils
	{
		private class MockTDNode : TDNode
		{
			public MockTDNode(double lat, double lon) : base(0, LatLongUtils.degreesToMicrodegrees(lat), LatLongUtils.degreesToMicrodegrees(lon), (short) 0, (sbyte) 0, null, null)
			{
			}
		}

		private class MockTDWay : TDWay
		{
			internal readonly bool area;

			public MockTDWay(TDNode[] wayNodes, bool area) : base(0, (sbyte) 0, null, null, null, null, (sbyte) 0, wayNodes)
			{
				this.area = area;
			}

			public override bool ForcePolygonLine
			{
				get
				{
					return !this.area;
				}
			}
		}

		private static readonly GeometryFactory geometryFactory = new GeometryFactory();
		private const string TEST_GEOMETRIES_RESOURCES_DIR = "src/test/resources/geometries";

		internal static Geometry readWKTFile(string wktFile)
		{
			File f = new File(TEST_GEOMETRIES_RESOURCES_DIR, wktFile);
			WKTReader wktReader = new WKTReader(geometryFactory);

			System.IO.StreamReader reader = null;
			try
			{
				reader = new System.IO.StreamReader(f);
				return wktReader.read(reader);
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return null;
			}
			catch (ParseException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return null;
			}
			finally
			{
				if (reader != null)
				{
					try
					{
						reader.Close();
					}
					catch (IOException)
					{
						// nothing to do
					}
				}
			}
		}

		internal static IList<TDWay> wktMultiLineStringToWays(string wktFile)
		{
			Geometry geometry = readWKTFile(wktFile);
			if (geometry == null || !(geometry is MultiLineString))
			{
				return null;
			}

			MultiLineString mls = (MultiLineString) geometry;
			IList<TDWay> ret = new List<TDWay>();
			for (int i = 0; i < mls.NumGeometries; i++)
			{
				ret.Add(fromLinestring((LineString) mls.getGeometryN(i), false));
			}
			return ret;
		}

		internal static IList<TDWay> wktPolygonToWays(string wktFile)
		{
			Geometry geometry = readWKTFile(wktFile);
			if (geometry == null || !(geometry is Polygon))
			{
				return null;
			}

			Polygon polygon = (Polygon) geometry;
			IList<TDWay> ret = new List<TDWay>();
			TDWay outer = fromLinestring(polygon.ExteriorRing, true);
			ret.Add(outer);
			for (int i = 0; i < polygon.NumInteriorRing; i++)
			{
				ret.Add(fromLinestring(polygon.getInteriorRingN(i), false));
			}
			return ret;
		}

		private static TDNode fromCoordinate(Coordinate c)
		{
			return new MockTDNode(c.y, c.x);
		}

		private static TDNode[] fromCoordinates(Coordinate[] coordinates)
		{
			TDNode[] nodes = new TDNode[coordinates.Length];
			for (int i = 0; i < coordinates.Length; i++)
			{
				nodes[i] = fromCoordinate(coordinates[i]);
			}
			return nodes;
		}

		private static TDWay fromLinestring(LineString l, bool area)
		{
			return new MockTDWay(fromCoordinates(l.Coordinates), area);
		}
	}

}