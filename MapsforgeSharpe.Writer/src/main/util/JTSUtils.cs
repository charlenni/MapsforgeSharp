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
	using LinearRing = com.vividsolutions.jts.geom.LinearRing;
	using MultiLineString = com.vividsolutions.jts.geom.MultiLineString;
	using MultiPolygon = com.vividsolutions.jts.geom.MultiPolygon;
	using Polygon = com.vividsolutions.jts.geom.Polygon;

	public sealed class JTSUtils
	{
		private static readonly GeometryFactory GEOMETRY_FACTORY = new GeometryFactory();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		private static readonly Logger LOGGER = Logger.getLogger(typeof(GeoUtils).FullName);

		/// <summary>
		/// Translates a <seealso cref="TDNode"/> object to a JTS <seealso cref="Coordinate"/>.
		/// </summary>
		/// <param name="node">
		///            the node </param>
		/// <returns> the coordinate </returns>
		public static Coordinate toCoordinate(TDNode node)
		{
			return toCoordinate(node.Latitude, node.Longitude);
		}

		/// <summary>
		/// Translates a <seealso cref="TDWay"/> object to an array of JTS <seealso cref="Coordinate"/>.
		/// </summary>
		/// <param name="way">
		///            the way </param>
		/// <returns> the array of coordinates </returns>
		public static Coordinate[] toCoordinates(TDWay way)
		{
			Coordinate[] coordinates = new Coordinate[way.WayNodes.length];
			if (way.ReversedInRelation)
			{
				for (int i = 0; i < coordinates.Length; i++)
				{
					coordinates[coordinates.Length - 1 - i] = toCoordinate(way.WayNodes[i]);
				}
			}
			else
			{
				for (int i = 0; i < coordinates.Length; i++)
				{
					coordinates[i] = toCoordinate(way.WayNodes[i]);
				}
			}
			return coordinates;
		}

		/// <summary>
		/// Converts a way with potential inner ways to a JTS geometry.
		/// </summary>
		/// <param name="way">
		///            the way </param>
		/// <param name="innerWays">
		///            the inner ways or null </param>
		/// <returns> the JTS geometry </returns>
		public static Geometry toJtsGeometry(TDWay way, IList<TDWay> innerWays)
		{
			if (way == null)
			{
				LOGGER.warning("way is null");
				return null;
			}

			if (way.ForcePolygonLine)
			{
				// may build a single line string if inner ways are empty
				return buildMultiLineString(way, innerWays);
			}

			if (way.Shape != TDWay.LINE || innerWays != null && innerWays.Count > 0)
			{
				// Have to be careful here about polygons and lines again, the problem with
				// polygons is that a certain direction is forced, so we do not want to reverse
				// closed lines that are not meant to be polygons
				// may contain holes if inner ways are not empty
				Polygon polygon = buildPolygon(way, innerWays);
				if (polygon.Valid)
				{
					return polygon;
				}
				return repairInvalidPolygon(polygon);
			}
			// not a closed line
			return buildLineString(way);
		}

		internal static LinearRing buildLinearRing(TDWay way)
		{
			Coordinate[] coordinates = JTSUtils.toCoordinates(way);
			return GEOMETRY_FACTORY.createLinearRing(coordinates);
		}

		internal static LineString buildLineString(TDWay way)
		{
			Coordinate[] coordinates = JTSUtils.toCoordinates(way);
			return GEOMETRY_FACTORY.createLineString(coordinates);
		}

		internal static MultiLineString buildMultiLineString(TDWay outerWay, IList<TDWay> innerWays)
		{
			IList<LineString> lineStrings = new List<LineString>();
			// outer way geometry
			lineStrings.Add(buildLineString(outerWay));

			// inner strings
			if (innerWays != null)
			{
				foreach (TDWay innerWay in innerWays)
				{
					LineString innerRing = buildLineString(innerWay);
					lineStrings.Add(innerRing);
				}
			}

			return GEOMETRY_FACTORY.createMultiLineString(lineStrings.ToArray());
		}

		internal static Polygon buildPolygon(TDWay way)
		{
			Coordinate[] coordinates = JTSUtils.toCoordinates(way);
			return GEOMETRY_FACTORY.createPolygon(GEOMETRY_FACTORY.createLinearRing(coordinates), null);
		}

		internal static Polygon buildPolygon(TDWay outerWay, IList<TDWay> innerWays)
		{
			if (innerWays == null || innerWays.Count == 0)
			{
				return buildPolygon(outerWay);
			}

			// outer way geometry
			LinearRing outerRing = buildLinearRing(outerWay);

			// inner rings
			IList<LinearRing> innerRings = new List<LinearRing>();

			foreach (TDWay innerWay in innerWays)
			{
				// build linear ring
				LinearRing innerRing = buildLinearRing(innerWay);
				innerRings.Add(innerRing);
			}

			if (innerRings.Count > 0)
			{
				// create new polygon
				LinearRing[] holes = innerRings.ToArray();
				return GEOMETRY_FACTORY.createPolygon(outerRing, holes);
			}

			return null;
		}

		internal static Geometry repairInvalidPolygon(Geometry p)
		{
			if (p is Polygon || p is MultiPolygon)
			{
				// apply zero buffer trick
				Geometry ret = p.buffer(0);
				if (ret.Area > 0)
				{
					return ret;
				}
				LOGGER.fine("unable to repair invalid polygon");
				return null;
			}
			return p;
		}

		/// <summary>
		/// Internal conversion method to convert our internal data structure for ways to geometry objects in JTS. It will
		/// care about ways and polygons and will create the right JTS objects.
		/// </summary>
		/// <param name="way">
		///            TDway which will be converted. Null if we were not able to convert the way to a Geometry object. </param>
		/// <returns> return Converted way as JTS object. </returns>
		internal static Geometry toJTSGeometry(TDWay way)
		{
			return toJtsGeometry(way, null);
		}

		private static Coordinate toCoordinate(int latitude, int longitude)
		{
			return new Coordinate(LatLongUtils.microdegreesToDegrees(longitude), LatLongUtils.microdegreesToDegrees(latitude));
		}

		private JTSUtils()
		{
			throw new System.InvalidOperationException();
		}
	}

}