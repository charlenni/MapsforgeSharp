using System;
using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2015 lincomatic
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

	using Coordinate = com.vividsolutions.jts.geom.Coordinate;
	using Envelope = com.vividsolutions.jts.geom.Envelope;
	using Geometry = com.vividsolutions.jts.geom.Geometry;
	using GeometryCollection = com.vividsolutions.jts.geom.GeometryCollection;
	using GeometryFactory = com.vividsolutions.jts.geom.GeometryFactory;
	using LineString = com.vividsolutions.jts.geom.LineString;
	using LinearRing = com.vividsolutions.jts.geom.LinearRing;
	using MultiLineString = com.vividsolutions.jts.geom.MultiLineString;
	using MultiPolygon = com.vividsolutions.jts.geom.MultiPolygon;
	using Point = com.vividsolutions.jts.geom.Point;
	using Polygon = com.vividsolutions.jts.geom.Polygon;
	using TopologyException = com.vividsolutions.jts.geom.TopologyException;
	using TopologyPreservingSimplifier = com.vividsolutions.jts.simplify.TopologyPreservingSimplifier;

	using LatLong = org.mapsforge.core.model.LatLong;
	using LatLongUtils = org.mapsforge.core.util.LatLongUtils;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using TDNode = org.mapsforge.map.writer.model.TDNode;
	using TDWay = org.mapsforge.map.writer.model.TDWay;
	using TileCoordinate = org.mapsforge.map.writer.model.TileCoordinate;
	using WayDataBlock = org.mapsforge.map.writer.model.WayDataBlock;


	/// <summary>
	/// Provides utility functions for the maps preprocessing.
	/// </summary>
	public sealed class GeoUtils
	{
		/// <summary>
		/// The minimum amount of coordinates (lat/lon counted separately) required for a valid closed polygon.
		/// </summary>
		public const int MIN_COORDINATES_POLYGON = 8;

		// private static final double DOUGLAS_PEUCKER_SIMPLIFICATION_TOLERANCE = 0.0000188;
		// private static final double DOUGLAS_PEUCKER_SIMPLIFICATION_TOLERANCE = 0.00003;
		/// <summary>
		/// The minimum amount of nodes required for a valid closed polygon.
		/// </summary>
		public const int MIN_NODES_POLYGON = 4;

		private static readonly double[] EPSILON_ZERO = new double[] {0, 0};
		// JTS
		private static readonly GeometryFactory GEOMETRY_FACTORY = new GeometryFactory();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		private static readonly Logger LOGGER = Logger.getLogger(typeof(GeoUtils).FullName);
		private const sbyte SUBTILE_ZOOMLEVEL_DIFFERENCE = 2;

		private static readonly int[] TILE_BITMASK_VALUES = new int[] {32768, 16384, 8192, 4096, 2048, 1024, 512, 256, 128, 64, 32, 16, 8, 4, 2, 1};

		/// <summary>
		/// Clips a geometry to a tile.
		/// </summary>
		/// <param name="way">
		///            the way </param>
		/// <param name="geometry">
		///            the geometry </param>
		/// <param name="tileCoordinate">
		///            the tile coordinate </param>
		/// <param name="enlargementInMeters">
		///            the bounding box buffer </param>
		/// <returns> the clipped geometry </returns>
		public static Geometry clipToTile(TDWay way, Geometry geometry, TileCoordinate tileCoordinate, int enlargementInMeters)
		{
			Geometry tileBBJTS = null;
			Geometry ret = null;

			// create tile bounding box
			tileBBJTS = tileToJTSGeometry(tileCoordinate.X, tileCoordinate.Y, tileCoordinate.Zoomlevel, enlargementInMeters);

			// clip the geometry by intersection with the bounding box of the tile
			// may throw a TopologyException
			try
			{
				if (!geometry.Valid)
				{
					// this should stop the problem of non-noded intersections that trigger an error when
					// clipping
					LOGGER.warning("invalid geometry prior to tile clipping, trying to repair " + way.Id);
					geometry = JTSUtils.repairInvalidPolygon(geometry);
					if (!geometry.Valid)
					{
						LOGGER.warning("invalid geometry even after attempt to fix " + way.Id);
					}
				}
				ret = tileBBJTS.intersection(geometry);
				// according to Ludwig (see issue332) valid polygons may become invalid by clipping (at least
				// in the Python shapely library
				// we need to investigate this more closely and write approriate test cases
				// for now, I check whether the resulting polygon is valid and if not try to repair it
				if ((ret is Polygon || ret is MultiPolygon) && !ret.Valid)
				{
					LOGGER.warning("clipped way is not valid, trying to repair it: " + way.Id);
					ret = JTSUtils.repairInvalidPolygon(ret);
					if (ret == null)
					{
						way.Invalid = true;
						LOGGER.warning("could not repair invalid polygon: " + way.Id);
					}
				}
			}
			catch (TopologyException e)
			{
				LOGGER.log(Level.WARNING, "JTS cannot clip way, not storing it in data file: " + way.Id, e);
				way.Invalid = true;
				return null;
			}
			return ret;
		}

		/// <summary>
		/// A tile on zoom level <i>z</i> has exactly 16 sub tiles on zoom level <i>z+2</i>. For each of these 16 sub tiles
		/// it is analyzed if the given way needs to be included. The result is represented as a 16 bit short value. Each bit
		/// represents one of the 16 sub tiles. A bit is set to 1 if the sub tile needs to include the way. Representation is
		/// row-wise.
		/// </summary>
		/// <param name="geometry">
		///            the geometry which is analyzed </param>
		/// <param name="tile">
		///            the tile which is split into 16 sub tiles </param>
		/// <param name="enlargementInMeter">
		///            amount of pixels that is used to enlarge the bounding box of the way and the tiles in the mapping
		///            process </param>
		/// <returns> a 16 bit short value that represents the information which of the sub tiles needs to include the way </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static short computeBitmask(final com.vividsolutions.jts.geom.Geometry geometry, final org.mapsforge.map.writer.model.TileCoordinate tile, final int enlargementInMeter)
		public static short computeBitmask(Geometry geometry, TileCoordinate tile, int enlargementInMeter)
		{
			IList<TileCoordinate> subtiles = tile.translateToZoomLevel((sbyte)(tile.Zoomlevel + SUBTILE_ZOOMLEVEL_DIFFERENCE));

			short bitmask = 0;
			int tileCounter = 0;
			foreach (TileCoordinate subtile in subtiles)
			{
				Geometry bbox = tileToJTSGeometry(subtile.X, subtile.Y, subtile.Zoomlevel, enlargementInMeter);
				if (bbox.intersects(geometry))
				{
					bitmask |= (short)TILE_BITMASK_VALUES[tileCounter];
				}
				tileCounter++;
			}
			return bitmask;
		}

		/// <param name="geometry">
		///            the JTS <seealso cref="Geometry"/> object </param>
		/// <returns> the centroid of the given geometry </returns>
		public static LatLong computeCentroid(Geometry geometry)
		{
			Point centroid = geometry.Centroid;
			if (centroid != null)
			{
				return new LatLong(centroid.Coordinate.y, centroid.Coordinate.x, true);
			}

			return null;
		}

		// *********** PREPROCESSING OF WAYS **************

		/// <param name="geometry">
		///            a JTS <seealso cref="Geometry"/> object representing the OSM entity </param>
		/// <param name="tile">
		///            the tile </param>
		/// <param name="enlargementInMeter">
		///            the enlargement of the tile in meters </param>
		/// <returns> true, if the geometry is covered completely by this tile </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static boolean coveredByTile(final com.vividsolutions.jts.geom.Geometry geometry, final org.mapsforge.map.writer.model.TileCoordinate tile, final int enlargementInMeter)
		public static bool coveredByTile(Geometry geometry, TileCoordinate tile, int enlargementInMeter)
		{
			Geometry bbox = tileToJTSGeometry(tile.X, tile.Y, tile.Zoomlevel, enlargementInMeter);
			if (bbox.covers(geometry))
			{
				return true;
			}

			return false;
		}

		// **************** WAY OR POI IN TILE *****************
		/// <summary>
		/// Computes which tiles on the given base zoom level need to include the given way (which may be a polygon).
		/// </summary>
		/// <param name="way">
		///            the way that is mapped to tiles </param>
		/// <param name="baseZoomLevel">
		///            the base zoom level which is used in the mapping </param>
		/// <param name="enlargementInMeter">
		///            amount of pixels that is used to enlarge the bounding box of the way and the tiles in the mapping
		///            process </param>
		/// <returns> all tiles on the given base zoom level that need to include the given way, an empty set if no tiles are
		///         matched </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static java.util.Set<org.mapsforge.map.writer.model.TileCoordinate> mapWayToTiles(final org.mapsforge.map.writer.model.TDWay way, final byte baseZoomLevel, final int enlargementInMeter)
		public static ISet<TileCoordinate> mapWayToTiles(TDWay way, sbyte baseZoomLevel, int enlargementInMeter)
		{
			if (way == null)
			{
				LOGGER.fine("way is null in mapping to tiles");
				return Collections.emptySet();
			}

			HashSet<TileCoordinate> matchedTiles = new HashSet<TileCoordinate>();
			Geometry wayGeometry = JTSUtils.toJTSGeometry(way);
			if (wayGeometry == null)
			{
				way.Invalid = true;
				LOGGER.fine("unable to create geometry from way: " + way.Id);
				return matchedTiles;
			}

			TileCoordinate[] bbox = getWayBoundingBox(way, baseZoomLevel, enlargementInMeter);
			// calculate the tile coordinates and the corresponding bounding boxes
			try
			{
				for (int k = bbox[0].X; k <= bbox[1].X; k++)
				{
					for (int l = bbox[0].Y; l <= bbox[1].Y; l++)
					{
						Geometry bboxGeometry = tileToJTSGeometry(k, l, baseZoomLevel, enlargementInMeter);
						if (bboxGeometry.intersects(wayGeometry))
						{
							matchedTiles.Add(new TileCoordinate(k, l, baseZoomLevel));
						}
					}
				}
			}
			catch (TopologyException)
			{
				LOGGER.fine("encountered error during mapping of a way to corresponding tiles, way id: " + way.Id);
				return Collections.emptySet();
			}

			return matchedTiles;
		}

		/// <param name="latLong">
		///            the point </param>
		/// <param name="tile">
		///            the tile </param>
		/// <returns> true if the point is located in the given tile </returns>
		public static bool pointInTile(LatLong latLong, TileCoordinate tile)
		{
			if (latLong == null || tile == null)
			{
				return false;
			}

			double lon1 = MercatorProjection.tileXToLongitude(tile.X, tile.Zoomlevel);
			double lon2 = MercatorProjection.tileXToLongitude(tile.X + 1, tile.Zoomlevel);
			double lat1 = MercatorProjection.tileYToLatitude(tile.Y, tile.Zoomlevel);
			double lat2 = MercatorProjection.tileYToLatitude(tile.Y + 1, tile.Zoomlevel);
			return latLong.latitude <= lat1 && latLong.latitude >= lat2 && latLong.longitude >= lon1 && latLong.longitude <= lon2;
		}

		/// <summary>
		/// Simplifies a geometry using the Douglas Peucker algorithm.
		/// </summary>
		/// <param name="way">
		///            the way </param>
		/// <param name="geometry">
		///            the geometry </param>
		/// <param name="zoomlevel">
		///            the zoom level </param>
		/// <param name="simplificationFactor">
		///            the simplification factor </param>
		/// <returns> the simplified geometry </returns>
		public static Geometry simplifyGeometry(TDWay way, Geometry geometry, sbyte zoomlevel, int tileSize, double simplificationFactor)
		{
			Geometry ret = null;

			Envelope bbox = geometry.EnvelopeInternal;
			// compute maximal absolute latitude (so that we don't need to care if we
			// are on northern or southern hemisphere)
			double latMax = Math.Max(Math.Abs(bbox.MaxY), Math.Abs(bbox.MinY));
			double deltaLat = deltaLat(simplificationFactor, latMax, zoomlevel, tileSize);

			try
			{
				ret = TopologyPreservingSimplifier.simplify(geometry, deltaLat);
			}
			catch (TopologyException e)
			{
				LOGGER.log(Level.FINE, "JTS cannot simplify way due to an error, not simplifying way with id: " + way.Id, e);
				way.Invalid = true;
				return geometry;
			}

			return ret;
		}

		/// <summary>
		/// Convert a JTS Geometry to a WayDataBlock list.
		/// </summary>
		/// <param name="geometry">
		///            a geometry object which should be converted </param>
		/// <returns> a list of WayBlocks which you can use to save the way. </returns>
		public static IList<WayDataBlock> toWayDataBlockList(Geometry geometry)
		{
			IList<WayDataBlock> res = new List<WayDataBlock>();
			if (geometry is MultiPolygon)
			{
				MultiPolygon mp = (MultiPolygon) geometry;
				for (int i = 0; i < mp.NumGeometries; i++)
				{
					Polygon p = (Polygon) mp.getGeometryN(i);
					IList<int?> outer = toCoordinateList(p.ExteriorRing);
					if (outer.Count / 2 > 0)
					{
						IList<IList<int?>> inner = new List<IList<int?>>();
						for (int j = 0; j < p.NumInteriorRing; j++)
						{
							IList<int?> innr = toCoordinateList(p.getInteriorRingN(j));
							if (innr.Count / 2 > 0)
							{
								inner.Add(innr);
							}
						}
						res.Add(new WayDataBlock(outer, inner));
					}
				}
			}
			else if (geometry is Polygon)
			{
				Polygon p = (Polygon) geometry;
				IList<int?> outer = toCoordinateList(p.ExteriorRing);
				if (outer.Count / 2 > 0)
				{
					IList<IList<int?>> inner = new List<IList<int?>>();
					for (int i = 0; i < p.NumInteriorRing; i++)
					{
						IList<int?> innr = toCoordinateList(p.getInteriorRingN(i));
						if (innr.Count / 2 > 0)
						{
							inner.Add(innr);
						}
					}
					res.Add(new WayDataBlock(outer, inner));
				}
			}
			else if (geometry is MultiLineString)
			{
				MultiLineString ml = (MultiLineString) geometry;
				for (int i = 0; i < ml.NumGeometries; i++)
				{
					LineString l = (LineString) ml.getGeometryN(i);
					IList<int?> outer = toCoordinateList(l);
					if (outer.Count / 2 > 0)
					{
						res.Add(new WayDataBlock(outer, null));
					}
				}
			}
			else if (geometry is LinearRing || geometry is LineString)
			{
				IList<int?> outer = toCoordinateList(geometry);
				if (outer.Count / 2 > 0)
				{
					res.Add(new WayDataBlock(outer, null));
				}
			}
			else if (geometry is GeometryCollection)
			{
				GeometryCollection gc = (GeometryCollection) geometry;
				for (int i = 0; i < gc.NumGeometries; i++)
				{
					IList<WayDataBlock> recursiveResult = toWayDataBlockList(gc.getGeometryN(i));
					foreach (WayDataBlock wayDataBlock in recursiveResult)
					{
						IList<int?> outer = wayDataBlock.OuterWay;
						if (outer.Count / 2 > 0)
						{
							res.Add(wayDataBlock);
						}
					}
				}
			}

			return res;
		}

		private static double[] bufferInDegrees(long tileY, sbyte zoom, int enlargementInMeter)
		{
			if (enlargementInMeter == 0)
			{
				return EPSILON_ZERO;
			}

			double[] epsilons = new double[2];
			double lat = MercatorProjection.tileYToLatitude(tileY, zoom);
			epsilons[0] = LatLongUtils.latitudeDistance(enlargementInMeter);
			epsilons[1] = LatLongUtils.longitudeDistance(enlargementInMeter, lat);

			return epsilons;
		}

		// **************** JTS CONVERSIONS *********************

		private static double[] computeTileEnlargement(double lat, int enlargementInMeter)
		{
			if (enlargementInMeter == 0)
			{
				return EPSILON_ZERO;
			}

			double[] epsilons = new double[2];

			epsilons[0] = LatLongUtils.latitudeDistance(enlargementInMeter);
			epsilons[1] = LatLongUtils.longitudeDistance(enlargementInMeter, lat);

			return epsilons;
		}

		// Computes the amount of latitude degrees for a given distance in pixel at a given zoom level.
		private static double deltaLat(double deltaPixel, double lat, sbyte zoom, int tileSize)
		{
			long mapSize = MercatorProjection.getMapSize(zoom, tileSize);
			double pixelY = MercatorProjection.latitudeToPixelY(lat, mapSize);
			double lat2 = MercatorProjection.pixelYToLatitude(pixelY + deltaPixel, mapSize);

			return Math.Abs(lat2 - lat);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private static org.mapsforge.map.writer.model.TileCoordinate[] getWayBoundingBox(final org.mapsforge.map.writer.model.TDWay way, byte zoomlevel, int enlargementInMeter)
		private static TileCoordinate[] getWayBoundingBox(TDWay way, sbyte zoomlevel, int enlargementInMeter)
		{
			double maxx = double.NegativeInfinity, maxy = double.NegativeInfinity, minx = double.PositiveInfinity, miny = double.PositiveInfinity;
			foreach (TDNode coordinate in way.WayNodes)
			{
				maxy = Math.Max(maxy, LatLongUtils.microdegreesToDegrees(coordinate.Latitude));
				miny = Math.Min(miny, LatLongUtils.microdegreesToDegrees(coordinate.Latitude));
				maxx = Math.Max(maxx, LatLongUtils.microdegreesToDegrees(coordinate.Longitude));
				minx = Math.Min(minx, LatLongUtils.microdegreesToDegrees(coordinate.Longitude));
			}

			double[] epsilonsTopLeft = computeTileEnlargement(maxy, enlargementInMeter);
			double[] epsilonsBottomRight = computeTileEnlargement(miny, enlargementInMeter);

			TileCoordinate[] bbox = new TileCoordinate[2];
			bbox[0] = new TileCoordinate((int) MercatorProjection.longitudeToTileX(minx - epsilonsTopLeft[1], zoomlevel), (int) MercatorProjection.latitudeToTileY(maxy + epsilonsTopLeft[0], zoomlevel), zoomlevel);
			bbox[1] = new TileCoordinate((int) MercatorProjection.longitudeToTileX(maxx + epsilonsBottomRight[1], zoomlevel), (int) MercatorProjection.latitudeToTileY(miny - epsilonsBottomRight[0], zoomlevel), zoomlevel);

			return bbox;
		}

		private static Geometry tileToJTSGeometry(long tileX, long tileY, sbyte zoom, int enlargementInMeter)
		{
			double minLat = MercatorProjection.tileYToLatitude(tileY + 1, zoom);
			double maxLat = MercatorProjection.tileYToLatitude(tileY, zoom);
			double minLon = MercatorProjection.tileXToLongitude(tileX, zoom);
			double maxLon = MercatorProjection.tileXToLongitude(tileX + 1, zoom);

			double[] epsilons = bufferInDegrees(tileY, zoom, enlargementInMeter);

			minLon -= epsilons[1];
			minLat -= epsilons[0];
			maxLon += epsilons[1];
			maxLat += epsilons[0];

			Coordinate bottomLeft = new Coordinate(minLon, minLat);
			Coordinate topRight = new Coordinate(maxLon, maxLat);

			return GEOMETRY_FACTORY.createLineString(new Coordinate[] {bottomLeft, topRight}).Envelope;
		}

		private static IList<int?> toCoordinateList(Geometry jtsGeometry)
		{
			Coordinate[] jtsCoords = jtsGeometry.Coordinates;

			List<int?> result = new List<int?>();

			for (int j = 0; j < jtsCoords.Length; j++)
			{
				LatLong geoCoord = new LatLong(jtsCoords[j].y, jtsCoords[j].x, true);
				result.Add(Convert.ToInt32(LatLongUtils.degreesToMicrodegrees(geoCoord.latitude)));
				result.Add(Convert.ToInt32(LatLongUtils.degreesToMicrodegrees(geoCoord.longitude)));
			}

			return result;
		}

		private GeoUtils()
		{
		}
	}

}