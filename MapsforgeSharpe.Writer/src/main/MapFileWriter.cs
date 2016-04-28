using System;
using System.Collections.Generic;
using System.Text;

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
namespace org.mapsforge.map.writer
{

	using CacheBuilder = com.google.common.cache.CacheBuilder;
	using CacheLoader = com.google.common.cache.CacheLoader;
	using CacheStats = com.google.common.cache.CacheStats;
	using LoadingCache = com.google.common.cache.LoadingCache;
	using Geometry = com.vividsolutions.jts.geom.Geometry;
	using LineString = com.vividsolutions.jts.geom.LineString;
	using MultiLineString = com.vividsolutions.jts.geom.MultiLineString;
	using MultiPolygon = com.vividsolutions.jts.geom.MultiPolygon;
	using Point = com.vividsolutions.jts.geom.Point;
	using Polygon = com.vividsolutions.jts.geom.Polygon;

	using LatLong = org.mapsforge.core.model.LatLong;
	using LatLongUtils = org.mapsforge.core.util.LatLongUtils;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using Encoding = org.mapsforge.map.writer.model.Encoding;
	using MapWriterConfiguration = org.mapsforge.map.writer.model.MapWriterConfiguration;
	using OSMTag = org.mapsforge.map.writer.model.OSMTag;
	using TDNode = org.mapsforge.map.writer.model.TDNode;
	using TDWay = org.mapsforge.map.writer.model.TDWay;
	using TileBasedDataProcessor = org.mapsforge.map.writer.model.TileBasedDataProcessor;
	using TileCoordinate = org.mapsforge.map.writer.model.TileCoordinate;
	using TileData = org.mapsforge.map.writer.model.TileData;
	using TileInfo = org.mapsforge.map.writer.model.TileInfo;
	using WayDataBlock = org.mapsforge.map.writer.model.WayDataBlock;
	using ZoomIntervalConfiguration = org.mapsforge.map.writer.model.ZoomIntervalConfiguration;
	using Constants = org.mapsforge.map.writer.util.Constants;
	using GeoUtils = org.mapsforge.map.writer.util.GeoUtils;
	using JTSUtils = org.mapsforge.map.writer.util.JTSUtils;


	/// <summary>
	/// Writes the binary file format for mapsforge maps.
	/// </summary>
	public sealed class MapFileWriter
	{
		private class JTSGeometryCacheLoader : CacheLoader<TDWay, Geometry>
		{
			internal readonly TileBasedDataProcessor datastore;

			internal JTSGeometryCacheLoader(TileBasedDataProcessor datastore) : base()
			{
				this.datastore = datastore;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public com.vividsolutions.jts.geom.Geometry load(org.mapsforge.map.writer.model.TDWay way) throws Exception
			public override Geometry load(TDWay way)
			{
				if (way.Invalid)
				{
					throw new Exception("way is known to be invalid: " + way.Id);
				}
				IList<TDWay> innerWaysOfMultipolygon = this.datastore.getInnerWaysOfMultipolygon(way.Id);
				Geometry geometry = JTSUtils.toJtsGeometry(way, innerWaysOfMultipolygon);
				if (geometry == null)
				{
					way.Invalid = true;
					throw new Exception("cannot create geometry for way with id: " + way.Id);
				}
				return geometry;
			}
		}

		private class WayPreprocessingCallable : Callable<WayPreprocessingResult>
		{
			internal readonly MapWriterConfiguration configuration;
			internal readonly LoadingCache<TDWay, Geometry> jtsGeometryCache;
			internal readonly sbyte maxZoomInterval;
			internal readonly TileCoordinate tile;
			internal readonly TDWay way;

			/// <param name="way">
			///            the <seealso cref="TDWay"/> </param>
			/// <param name="tile">
			///            the <seealso cref="TileCoordinate"/> </param>
			/// <param name="maxZoomInterval">
			///            the maximum zoom </param>
			/// <param name="jtsGeometryCache">
			///            the <seealso cref="LoadingCache"/> for <seealso cref="Geometry"/> objects </param>
			/// <param name="configuration">
			///            the <seealso cref="MapWriterConfiguration"/> </param>
			internal WayPreprocessingCallable(TDWay way, TileCoordinate tile, sbyte maxZoomInterval, LoadingCache<TDWay, Geometry> jtsGeometryCache, MapWriterConfiguration configuration) : base()
			{
				this.way = way;
				this.tile = tile;
				this.maxZoomInterval = maxZoomInterval;
				this.jtsGeometryCache = jtsGeometryCache;
				this.configuration = configuration;
			}

			public override WayPreprocessingResult call()
			{
				// TODO more sophisticated clipping of polygons needed
				// we have a problem when clipping polygons which border needs to be
				// rendered
				// the problem does not occur with polygons that do not have a border
				// imagine an administrative border, such a polygon is not filled, but its
				// border is rendered
				// in case the polygon spans multiple base zoom tiles, clipping
				// introduces connections between
				// nodes that haven't existed before (exactly at the borders of a base
				// tile)
				// in case of filled polygons we do not care about these connections
				// polygons that represent a border must be clipped as simple ways and
				// not as polygons

				Geometry originalGeometry;
				try
				{
					originalGeometry = this.jtsGeometryCache.get(this.way);
				}
				catch (ExecutionException)
				{
					this.way.Invalid = true;
					return null;
				}

				Geometry processedGeometry = originalGeometry;


				if ((originalGeometry is Polygon || originalGeometry is MultiPolygon) && this.configuration.PolygonClipping || (originalGeometry is LineString || originalGeometry is MultiLineString) && this.configuration.WayClipping)
				{
					processedGeometry = GeoUtils.clipToTile(this.way, originalGeometry, this.tile, this.configuration.BboxEnlargement);
					if (processedGeometry == null)
					{
						return null;
					}
				}

				// TODO is this the right place to simplify, or is it better before clipping?
				if (this.configuration.Simplification > 0 && this.tile.Zoomlevel <= Constants.MAX_SIMPLIFICATION_BASE_ZOOM)
				{
					processedGeometry = GeoUtils.simplifyGeometry(this.way, processedGeometry, this.maxZoomInterval, tileSize, this.configuration.Simplification);
					if (processedGeometry == null)
					{
						return null;
					}
				}

				if (processedGeometry.Coordinates.length > 2000)
				{
					LOGGER.info("Large geometry " + this.way.Id + " (" + processedGeometry.Coordinates.length + " coords, down from " + originalGeometry.Coordinates.length + " coords)");
				}


				IList<WayDataBlock> blocks = GeoUtils.toWayDataBlockList(processedGeometry);
				if (blocks == null)
				{
					return null;
				}
				if (blocks.Count == 0)
				{
					LOGGER.finer("empty list of way data blocks after preprocessing way: " + this.way.Id);
					return null;
				}
				short subtileMask = GeoUtils.computeBitmask(processedGeometry, this.tile, this.configuration.BboxEnlargement);

				// check if the original polygon is completely contained in the current tile
				// in that case we do not try to compute a label position
				// this is left to the renderer for more flexibility

				// in case the polygon covers multiple tiles, we compute the centroid of the unclipped polygon
				// if the computed centroid is within the current tile, we add it as label position
				// this way, we can make sure that a label position is attached only once to a clipped polygon
				LatLong centroidCoordinate = null;
				if (this.configuration.LabelPosition && this.way.ValidClosedLine && !GeoUtils.coveredByTile(originalGeometry, this.tile, this.configuration.BboxEnlargement))
				{
					Point centroidPoint = originalGeometry.Centroid;
					if (GeoUtils.coveredByTile(centroidPoint, this.tile, this.configuration.BboxEnlargement))
					{
						centroidCoordinate = new LatLong(centroidPoint.Y, centroidPoint.X, true);
					}
				}

				switch (this.configuration.EncodingChoice)
				{
					case SINGLE:
						blocks = DeltaEncoder.encode(blocks, Encoding.DELTA);
						break;
					case DOUBLE:
						blocks = DeltaEncoder.encode(blocks, Encoding.DOUBLE_DELTA);
						break;
					case AUTO:
						IList<WayDataBlock> blocksDelta = DeltaEncoder.encode(blocks, Encoding.DELTA);
						IList<WayDataBlock> blocksDoubleDelta = DeltaEncoder.encode(blocks, Encoding.DOUBLE_DELTA);
						int simDelta = DeltaEncoder.simulateSerialization(blocksDelta);
						int simDoubleDelta = DeltaEncoder.simulateSerialization(blocksDoubleDelta);
						if (simDelta <= simDoubleDelta)
						{
							blocks = blocksDelta;
						}
						else
						{
							blocks = blocksDoubleDelta;
						}
						break;
				}

				return new WayPreprocessingResult(this.way, blocks, centroidCoordinate, subtileMask);
			}
		}

		private class WayPreprocessingResult
		{
			internal readonly LatLong labelPosition;
			internal readonly short subtileMask;
			internal readonly TDWay way;
			internal readonly IList<WayDataBlock> wayDataBlocks;

			internal WayPreprocessingResult(TDWay way, IList<WayDataBlock> wayDataBlocks, LatLong labelPosition, short subtileMask) : base()
			{
				this.way = way;
				this.wayDataBlocks = wayDataBlocks;
				this.labelPosition = labelPosition;
				this.subtileMask = subtileMask;
			}

			internal virtual LatLong LabelPosition
			{
				get
				{
					return this.labelPosition;
				}
			}

			internal virtual short SubtileMask
			{
				get
				{
					return this.subtileMask;
				}
			}

			internal virtual TDWay Way
			{
				get
				{
					return this.way;
				}
			}

			internal virtual IList<WayDataBlock> WayDataBlocks
			{
				get
				{
					return this.wayDataBlocks;
				}
			}
		}

		// IO
		internal const int HEADER_BUFFER_SIZE = 0x100000; // 1MB

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		internal static readonly Logger LOGGER = Logger.getLogger(typeof(MapFileWriter).FullName);

		internal const int MIN_TILE_BUFFER_SIZE = 0xF00000; // 15MB

		internal const int POI_DATA_BUFFER_SIZE = 0x100000; // 1MB

		internal const int TILE_BUFFER_SIZE = 0xA00000; // 10MB

		// private static final int PIXEL_COMPRESSION_MAX_DELTA = 5;

		internal const int TILES_BUFFER_SIZE = 0x3200000; // 50MB

		internal const int WAY_BUFFER_SIZE = 0x100000; // 10MB

		internal const int WAY_DATA_BUFFER_SIZE = 0xA00000; // 10MB

		// private static final CoastlineHandler COASTLINE_HANDLER = new
		// CoastlineHandler();

		private const short BITMAP_COMMENT = 8;
		private const short BITMAP_CREATED_WITH = 4;
		// bitmap flags for file features
		private const short BITMAP_DEBUG = 128;
		// bitmap flags for pois
		private const short BITMAP_ELEVATION = 32;
		private const short BITMAP_ENCODING = 4;
		private const short BITMAP_HOUSENUMBER = 64;

		private const int BITMAP_INDEX_ENTRY_WATER = 0x80;
		private const short BITMAP_LABEL = 16;

		private const short BITMAP_MAP_START_POSITION = 64;

		private const short BITMAP_MAP_START_ZOOM = 32;
		private const short BITMAP_MULTIPLE_WAY_BLOCKS = 8;
		// bitmap flags for pois and ways
		private const short BITMAP_NAME = 128;
		private const short BITMAP_PREFERRED_LANGUAGES = 16;

		// bitmap flags for ways
		private const short BITMAP_REF = 32;
		private const int BYTE_AMOUNT_SUBFILE_INDEX_PER_TILE = 5;
		private const int BYTES_INT = 4;
		private const int DEBUG_BLOCK_SIZE = 32;
		private const string DEBUG_INDEX_START_STRING = "+++IndexStart+++";
		// DEBUG STRINGS
		private const string DEBUG_STRING_POI_HEAD = "***POIStart";

		private const string DEBUG_STRING_POI_TAIL = "***";

		private const string DEBUG_STRING_TILE_HEAD = "###TileStart";

		private const string DEBUG_STRING_TILE_TAIL = "###";

		private const string DEBUG_STRING_WAY_HEAD = "---WayStart";

		private const string DEBUG_STRING_WAY_TAIL = "---";

		private const int DUMMY_INT = unchecked((int)0xf0f0f0f0);

		private const long DUMMY_LONG = unchecked((long)0xf0f0f0f0f0f0f0f0L);

		private static readonly ExecutorService EXECUTOR_SERVICE = Executors.newFixedThreadPool(Runtime.Runtime.availableProcessors());
		private const int JTS_GEOMETRY_CACHE_SIZE = 50000;
		private const string MAGIC_BYTE = "mapsforge binary OSM";
		private const int OFFSET_FILE_SIZE = 28;
		private const float PROGRESS_PERCENT_STEP = 10f;
		private const string PROJECTION = "Mercator";
		private const int SIZE_ZOOMINTERVAL_CONFIGURATION = 19;

		private static readonly TileInfo TILE_INFO = TileInfo.Instance;

		private const int tileSize = 256; // needed for optimal simplification, but set to constant here TODO

		private static readonly Charset UTF8_CHARSET = Charset.forName("utf8");

		/// <summary>
		/// Writes the map file according to the given configuration using the given data processor.
		/// </summary>
		/// <param name="configuration">
		///            the configuration </param>
		/// <param name="dataProcessor">
		///            the data processor </param>
		/// <exception cref="IOException">
		///             thrown if any IO error occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeFile(org.mapsforge.map.writer.model.MapWriterConfiguration configuration, org.mapsforge.map.writer.model.TileBasedDataProcessor dataProcessor) throws java.io.IOException
		public static void writeFile(MapWriterConfiguration configuration, TileBasedDataProcessor dataProcessor)
		{
			RandomAccessFile randomAccessFile = new RandomAccessFile(configuration.OutputFile, "rw");

			int amountOfZoomIntervals = dataProcessor.ZoomIntervalConfiguration.NumberOfZoomIntervals;
			ByteBuffer containerHeaderBuffer = ByteBuffer.allocate(HEADER_BUFFER_SIZE);
			// CONTAINER HEADER
			int totalHeaderSize = writeHeaderBuffer(configuration, dataProcessor, containerHeaderBuffer);

			// set to mark where zoomIntervalConfig starts
			containerHeaderBuffer.reset();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.google.common.cache.LoadingCache<org.mapsforge.map.writer.model.TDWay, com.vividsolutions.jts.geom.Geometry> jtsGeometryCache = com.google.common.cache.CacheBuilder.newBuilder().maximumSize(JTS_GEOMETRY_CACHE_SIZE).concurrencyLevel(Runtime.getRuntime().availableProcessors() * 2).build(new JTSGeometryCacheLoader(dataProcessor));
			LoadingCache<TDWay, Geometry> jtsGeometryCache = CacheBuilder.newBuilder().maximumSize(JTS_GEOMETRY_CACHE_SIZE).concurrencyLevel(Runtime.Runtime.availableProcessors() * 2).build(new JTSGeometryCacheLoader(dataProcessor));

			// SUB FILES
			// for each zoom interval write a sub file
			long currentFileSize = totalHeaderSize;
			for (int i = 0; i < amountOfZoomIntervals; i++)
			{
				// SUB FILE INDEX AND DATA
				long subfileSize = writeSubfile(currentFileSize, i, dataProcessor, jtsGeometryCache, randomAccessFile, configuration);
				// SUB FILE META DATA IN CONTAINER HEADER
				writeSubfileMetaDataToContainerHeader(dataProcessor.ZoomIntervalConfiguration, i, currentFileSize, subfileSize, containerHeaderBuffer);
				currentFileSize += subfileSize;
			}

			randomAccessFile.seek(0);
			randomAccessFile.write(containerHeaderBuffer.array(), 0, totalHeaderSize);

			// WRITE FILE SIZE TO HEADER
			long fileSize = randomAccessFile.length();
			randomAccessFile.seek(OFFSET_FILE_SIZE);
			randomAccessFile.writeLong(fileSize);

			randomAccessFile.close();

			CacheStats stats = jtsGeometryCache.stats();
			LOGGER.info("JTS Geometry cache hit rate: " + stats.hitRate());
			LOGGER.info("JTS Geometry total load time: " + stats.totalLoadTime() / 1000);

			LOGGER.info("Finished writing file.");
		}

		/// <summary>
		/// Cleans up thread pool. Must only be called at the end of processing.
		/// </summary>
		public static void release()
		{
			EXECUTOR_SERVICE.shutdown();
		}

		internal static sbyte infoByteOptmizationParams(MapWriterConfiguration configuration)
		{
			sbyte infoByte = 0;

			if (configuration.DebugStrings)
			{
				infoByte |= (sbyte)BITMAP_DEBUG;
			}
			if (configuration.MapStartPosition != null)
			{
				infoByte |= (sbyte)BITMAP_MAP_START_POSITION;
			}
			if (configuration.hasMapStartZoomLevel())
			{
				infoByte |= (sbyte)BITMAP_MAP_START_ZOOM;
			}
			if (configuration.PreferredLanguages != null && !configuration.PreferredLanguages.Empty)
			{
				infoByte |= (sbyte)BITMAP_PREFERRED_LANGUAGES;
			}
			if (configuration.Comment != null)
			{
				infoByte |= (sbyte)BITMAP_COMMENT;
			}

			infoByte |= (sbyte)BITMAP_CREATED_WITH;

			return infoByte;
		}

		internal static sbyte infoBytePOIFeatures(string name, int elevation, string housenumber)
		{
			sbyte infoByte = 0;

			if (!string.ReferenceEquals(name, null) && name.Length > 0)
			{
				infoByte |= (sbyte)BITMAP_NAME;
			}
			if (!string.ReferenceEquals(housenumber, null) && housenumber.Length > 0)
			{
				infoByte |= (sbyte)BITMAP_HOUSENUMBER;
			}
			if (elevation != 0)
			{
				infoByte |= (sbyte)BITMAP_ELEVATION;
			}
			return infoByte;
		}

		internal static sbyte infoBytePoiLayerAndTagAmount(TDNode node)
		{
			sbyte layer = node.Layer;
			// make sure layer is in [0,10]
			layer = layer < 0 ? 0 : layer > 10 ? 10 : layer;
			short tagAmount = node.Tags == null ? 0 : (short) node.Tags.length;

			return (sbyte)(layer << BYTES_INT | tagAmount);
		}

		internal static sbyte infoByteWayFeatures(TDWay way, WayPreprocessingResult wpr)
		{
			sbyte infoByte = 0;

			if (way.Name != null && !way.Name.Empty)
			{
				infoByte |= (sbyte)BITMAP_NAME;
			}
			if (way.HouseNumber != null && !way.HouseNumber.Empty)
			{
				infoByte |= (sbyte)BITMAP_HOUSENUMBER;
			}
			if (way.Ref != null && !way.Ref.Empty)
			{
				infoByte |= (sbyte)BITMAP_REF;
			}
			if (wpr.LabelPosition != null)
			{
				infoByte |= (sbyte)BITMAP_LABEL;
			}
			if (wpr.WayDataBlocks.Count > 1)
			{
				infoByte |= (sbyte)BITMAP_MULTIPLE_WAY_BLOCKS;
			}

			if (wpr.WayDataBlocks.Count > 0)
			{
				WayDataBlock wayDataBlock = wpr.WayDataBlocks[0];
				if (wayDataBlock.Encoding == Encoding.DOUBLE_DELTA)
				{
					infoByte |= (sbyte)BITMAP_ENCODING;
				}
			}

			return infoByte;
		}

		internal static sbyte infoByteWayLayerAndTagAmount(TDWay way)
		{
			sbyte layer = way.Layer;
			// make sure layer is in [0,10]
			layer = layer < 0 ? 0 : layer > 10 ? 10 : layer;
			short tagAmount = way.Tags == null ? 0 : (short) way.Tags.length;

			return (sbyte)(layer << BYTES_INT | tagAmount);
		}

		internal static void processPOI(TDNode poi, int currentTileLat, int currentTileLon, bool debugStrings, ByteBuffer poiBuffer)
		{
			if (debugStrings)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(DEBUG_STRING_POI_HEAD).Append(poi.Id).Append(DEBUG_STRING_POI_TAIL);
				poiBuffer.put(sb.ToString().GetBytes(UTF8_CHARSET));
				// append whitespaces so that block has 32 bytes
				appendWhitespace(DEBUG_BLOCK_SIZE - sb.ToString().GetBytes(UTF8_CHARSET).length, poiBuffer);
			}

			// write poi features to the file
			poiBuffer.put(Serializer.getVariableByteSigned(poi.Latitude - currentTileLat));
			poiBuffer.put(Serializer.getVariableByteSigned(poi.Longitude - currentTileLon));

			// write byte with layer and tag amount info
			poiBuffer.put(infoBytePoiLayerAndTagAmount(poi));

			// write tag ids to the file
			if (poi.Tags != null)
			{
				foreach (short tagID in poi.Tags)
				{
					poiBuffer.put(Serializer.getVariableByteUnsigned(OSMTagMapping.Instance.OptimizedPoiIds.get(Convert.ToInt16(tagID)).intValue()));
				}
			}

			// write byte with bits set to 1 if the poi has a
			// name, an elevation
			// or a housenumber
			poiBuffer.put(infoBytePOIFeatures(poi.Name, poi.Elevation, poi.HouseNumber));

			if (poi.Name != null && !poi.Name.Empty)
			{
				writeUTF8(poi.Name, poiBuffer);
			}

			if (poi.HouseNumber != null && !poi.HouseNumber.Empty)
			{
				writeUTF8(poi.HouseNumber, poiBuffer);
			}

			if (poi.Elevation != 0)
			{
				poiBuffer.put(Serializer.getVariableByteSigned(poi.Elevation));
			}
		}

		internal static void processWay(WayPreprocessingResult wpr, TDWay way, int currentTileLat, int currentTileLon, ByteBuffer wayBuffer)
		{
			// write subtile bitmask of way
			wayBuffer.putShort(wpr.SubtileMask);

			// write byte with layer and tag amount
			wayBuffer.put(infoByteWayLayerAndTagAmount(way));

			// write tag ids
			if (way.Tags != null)
			{
				foreach (short tagID in way.Tags)
				{
					wayBuffer.put(Serializer.getVariableByteUnsigned(mappedWayTagID(tagID)));
				}
			}

			// write a byte with flags for existence of name,
			// ref, label position, and multiple blocks
			wayBuffer.put(infoByteWayFeatures(way, wpr));

			// if the way has a name, write it to the file
			if (way.Name != null && !way.Name.Empty)
			{
				writeUTF8(way.Name, wayBuffer);
			}

			// if the way has a house number, write it to the file
			if (way.HouseNumber != null && !way.HouseNumber.Empty)
			{
				writeUTF8(way.HouseNumber, wayBuffer);
			}

			// if the way has a ref, write it to the file
			if (way.Ref != null && !way.Ref.Empty)
			{
				writeUTF8(way.Ref, wayBuffer);
			}

			if (wpr.LabelPosition != null)
			{
				int firstWayStartLat = wpr.WayDataBlocks[0].OuterWay.get(0).intValue();
				int firstWayStartLon = wpr.WayDataBlocks[0].OuterWay.get(1).intValue();

				wayBuffer.put(Serializer.getVariableByteSigned(LatLongUtils.degreesToMicrodegrees(wpr.LabelPosition.latitude) - firstWayStartLat));
				wayBuffer.put(Serializer.getVariableByteSigned(LatLongUtils.degreesToMicrodegrees(wpr.LabelPosition.longitude) - firstWayStartLon));
			}

			if (wpr.WayDataBlocks.Count > 1)
			{
				// write the amount of way data blocks
				wayBuffer.put(Serializer.getVariableByteUnsigned(wpr.WayDataBlocks.Count));
			}

			// write the way data blocks

			// case 1: simple way or simple polygon --> the way
			// block consists of
			// exactly one way
			// case 2: multi polygon --> the way consists of
			// exactly one outer way and
			// one or more inner ways
			foreach (WayDataBlock wayDataBlock in wpr.WayDataBlocks)
			{
				// write the amount of coordinate blocks
				// we have at least one block (potentially
				// interpreted as outer way) and
				// possible blocks for inner ways
				if (wayDataBlock.InnerWays != null && !wayDataBlock.InnerWays.Empty)
				{
					// multi polygon: outer way + number of
					// inner ways
					wayBuffer.put(Serializer.getVariableByteUnsigned(1 + wayDataBlock.InnerWays.size()));
				}
				else
				{
					// simply a single way (not a multi polygon)
					wayBuffer.put(Serializer.getVariableByteUnsigned(1));
				}

				// write block for (outer/simple) way
				writeWay(wayDataBlock.OuterWay, currentTileLat, currentTileLon, wayBuffer);

				// write blocks for inner ways
				if (wayDataBlock.InnerWays != null && !wayDataBlock.InnerWays.Empty)
				{
					foreach (IList<int?> innerWayCoordinates in wayDataBlock.InnerWays)
					{
						writeWay(innerWayCoordinates, currentTileLat, currentTileLon, wayBuffer);
					}
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static int writeHeaderBuffer(final org.mapsforge.map.writer.model.MapWriterConfiguration configuration, final org.mapsforge.map.writer.model.TileBasedDataProcessor dataProcessor, final ByteBuffer containerHeaderBuffer)
		internal static int writeHeaderBuffer(MapWriterConfiguration configuration, TileBasedDataProcessor dataProcessor, ByteBuffer containerHeaderBuffer)
		{
			LOGGER.fine("writing header");
			LOGGER.fine("Bounding box for file: " + dataProcessor.BoundingBox.ToString());

			// write file header
			// MAGIC BYTE
			sbyte[] magicBytes = MAGIC_BYTE.GetBytes(UTF8_CHARSET);
			containerHeaderBuffer.put(magicBytes);

			// HEADER SIZE: Write dummy pattern as header size. It will be replaced
			// later in time
			int headerSizePosition = containerHeaderBuffer.position();
			containerHeaderBuffer.putInt(DUMMY_INT);

			// FILE VERSION
			containerHeaderBuffer.putInt(configuration.FileSpecificationVersion);

			// FILE SIZE: Write dummy pattern as file size. It will be replaced
			// later in time
			containerHeaderBuffer.putLong(DUMMY_LONG);
			// DATE OF CREATION
			containerHeaderBuffer.putLong(DateTimeHelperClass.CurrentUnixTimeMillis());

			// BOUNDING BOX
			containerHeaderBuffer.putInt(LatLongUtils.degreesToMicrodegrees(dataProcessor.BoundingBox.minLatitude));
			containerHeaderBuffer.putInt(LatLongUtils.degreesToMicrodegrees(dataProcessor.BoundingBox.minLongitude));
			containerHeaderBuffer.putInt(LatLongUtils.degreesToMicrodegrees(dataProcessor.BoundingBox.maxLatitude));
			containerHeaderBuffer.putInt(LatLongUtils.degreesToMicrodegrees(dataProcessor.BoundingBox.maxLongitude));

			// TILE SIZE
			containerHeaderBuffer.putShort((short) Constants.DEFAULT_TILE_SIZE);

			// PROJECTION
			writeUTF8(PROJECTION, containerHeaderBuffer);

			// check whether zoom start is a valid zoom level

			// FLAGS
			containerHeaderBuffer.put(infoByteOptmizationParams(configuration));

			// MAP START POSITION
			LatLong mapStartPosition = configuration.MapStartPosition;
			if (mapStartPosition != null)
			{
				containerHeaderBuffer.putInt(LatLongUtils.degreesToMicrodegrees(mapStartPosition.latitude));
				containerHeaderBuffer.putInt(LatLongUtils.degreesToMicrodegrees(mapStartPosition.longitude));
			}

			// MAP START ZOOM
			if (configuration.hasMapStartZoomLevel())
			{
				containerHeaderBuffer.put((sbyte) configuration.MapStartZoomLevel);
			}

			// PREFERRED LANGUAGE
			if (configuration.PreferredLanguages != null && !configuration.PreferredLanguages.Empty)
			{
				string langStr = "";
				foreach (string preferredLanguage in configuration.PreferredLanguages)
				{
					langStr += (langStr.Length > 0 ? "," : "") + preferredLanguage;
				}
				writeUTF8(langStr, containerHeaderBuffer);
			}

			// COMMENT
			if (configuration.Comment != null)
			{
				writeUTF8(configuration.Comment, containerHeaderBuffer);
			}

			// CREATED WITH
			writeUTF8(configuration.WriterVersion, containerHeaderBuffer);

			// AMOUNT POI TAGS
			containerHeaderBuffer.putShort((short) configuration.TagMapping.OptimizedPoiIds.size());
			// POI TAGS
			// retrieves tag ids in order of frequency, most frequent come first
			foreach (short tagId in configuration.TagMapping.OptimizedPoiIds.Keys)
			{
				OSMTag tag = configuration.TagMapping.getPoiTag(tagId);
				writeUTF8(tag.tagKey(), containerHeaderBuffer);
			}

			// AMOUNT OF WAY TAGS
			containerHeaderBuffer.putShort((short) configuration.TagMapping.OptimizedWayIds.size());

			// WAY TAGS
			foreach (short tagId in configuration.TagMapping.OptimizedWayIds.Keys)
			{
				OSMTag tag = configuration.TagMapping.getWayTag(tagId);
				writeUTF8(tag.tagKey(), containerHeaderBuffer);
			}

			// AMOUNT OF ZOOM INTERVALS
			int numberOfZoomIntervals = dataProcessor.ZoomIntervalConfiguration.NumberOfZoomIntervals;
			containerHeaderBuffer.put((sbyte) numberOfZoomIntervals);

			// SET MARK OF THIS BUFFER AT POSITION FOR WRITING ZOOM INTERVAL CONFIG
			containerHeaderBuffer.mark();
			// ZOOM INTERVAL CONFIGURATION: SKIP COMPUTED AMOUNT OF BYTES
			containerHeaderBuffer.position(containerHeaderBuffer.position() + SIZE_ZOOMINTERVAL_CONFIGURATION * numberOfZoomIntervals);

			// now write header size
			// -4 bytes of header size variable itself
			int headerSize = containerHeaderBuffer.position() - headerSizePosition - BYTES_INT;
			containerHeaderBuffer.putInt(headerSizePosition, headerSize);

			return containerHeaderBuffer.position();
		}

		internal static void writeWayNodes(IList<int?> waynodes, int currentTileLat, int currentTileLon, ByteBuffer buffer)
		{
			if (waynodes.Count > 0 && waynodes.Count % 2 == 0)
			{
				IEnumerator<int?> waynodeIterator = waynodes.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				buffer.put(Serializer.getVariableByteSigned(waynodeIterator.next().intValue() - currentTileLat));
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				buffer.put(Serializer.getVariableByteSigned(waynodeIterator.next().intValue() - currentTileLon));

				while (waynodeIterator.MoveNext())
				{
					buffer.put(Serializer.getVariableByteSigned(waynodeIterator.Current.intValue()));
				}
			}
		}

		internal static void writeZoomLevelTable(int[][] entitiesPerZoomLevel, ByteBuffer tileBuffer)
		{
			// write cumulated number of POIs and ways for this tile on
			// each zoom level
			foreach (int[] entityCount in entitiesPerZoomLevel)
			{
				tileBuffer.put(Serializer.getVariableByteUnsigned(entityCount[0]));
				tileBuffer.put(Serializer.getVariableByteUnsigned(entityCount[1]));
			}
		}

		private static void appendWhitespace(int amount, ByteBuffer buffer)
		{
			for (int i = 0; i < amount; i++)
			{
				buffer.put((sbyte) ' ');
			}
		}

		private static int mappedWayTagID(short original)
		{
			return OSMTagMapping.Instance.OptimizedWayIds.get(Convert.ToInt16(original)).intValue();
		}

		private static void processIndexEntry(TileCoordinate tileCoordinate, ByteBuffer indexBuffer, long currentSubfileOffset)
		{
			sbyte[] indexBytes = Serializer.getFiveBytes(currentSubfileOffset);
			if (TILE_INFO.isWaterTile(tileCoordinate))
			{
				indexBytes[0] |= (sbyte)BITMAP_INDEX_ENTRY_WATER;
			}
			indexBuffer.put(indexBytes);
		}

		private static void processTile(MapWriterConfiguration configuration, TileCoordinate tileCoordinate, TileBasedDataProcessor dataProcessor, LoadingCache<TDWay, Geometry> jtsGeometryCache, int zoomIntervalIndex, ByteBuffer tileBuffer, ByteBuffer poiDataBuffer, ByteBuffer wayDataBuffer, ByteBuffer wayBuffer)
		{
			tileBuffer.clear();
			poiDataBuffer.clear();
			wayDataBuffer.clear();
			wayBuffer.clear();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.mapsforge.map.writer.model.TileData currentTile = dataProcessor.getTile(zoomIntervalIndex, tileCoordinate.getX(), tileCoordinate.getY());
			TileData currentTile = dataProcessor.getTile(zoomIntervalIndex, tileCoordinate.X, tileCoordinate.Y);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int currentTileLat = org.mapsforge.core.util.LatLongUtils.degreesToMicrodegrees(org.mapsforge.core.util.MercatorProjection.tileYToLatitude(tileCoordinate.getY(), tileCoordinate.getZoomlevel()));
			int currentTileLat = LatLongUtils.degreesToMicrodegrees(MercatorProjection.tileYToLatitude(tileCoordinate.Y, tileCoordinate.Zoomlevel));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int currentTileLon = org.mapsforge.core.util.LatLongUtils.degreesToMicrodegrees(org.mapsforge.core.util.MercatorProjection.tileXToLongitude(tileCoordinate.getX(), tileCoordinate.getZoomlevel()));
			int currentTileLon = LatLongUtils.degreesToMicrodegrees(MercatorProjection.tileXToLongitude(tileCoordinate.X, tileCoordinate.Zoomlevel));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte minZoomCurrentInterval = dataProcessor.getZoomIntervalConfiguration().getMinZoom(zoomIntervalIndex);
			sbyte minZoomCurrentInterval = dataProcessor.ZoomIntervalConfiguration.getMinZoom(zoomIntervalIndex);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte maxZoomCurrentInterval = dataProcessor.getZoomIntervalConfiguration().getMaxZoom(zoomIntervalIndex);
			sbyte maxZoomCurrentInterval = dataProcessor.ZoomIntervalConfiguration.getMaxZoom(zoomIntervalIndex);

			// write amount of POIs and ways for each zoom level
			IDictionary<sbyte?, IList<TDNode>> poisByZoomlevel = currentTile.poisByZoomlevel(minZoomCurrentInterval, maxZoomCurrentInterval);
			IDictionary<sbyte?, IList<TDWay>> waysByZoomlevel = currentTile.waysByZoomlevel(minZoomCurrentInterval, maxZoomCurrentInterval);

			if (poisByZoomlevel.Count > 0 || waysByZoomlevel.Count > 0)
			{
				if (configuration.DebugStrings)
				{
					writeTileSignature(tileCoordinate, tileBuffer);
				}

				int amountZoomLevels = maxZoomCurrentInterval - minZoomCurrentInterval + 1;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] entitiesPerZoomLevel = new int[amountZoomLevels][2];
				int[][] entitiesPerZoomLevel = RectangularArrays.ReturnRectangularIntArray(amountZoomLevels, 2);

				// WRITE POIS
				for (sbyte zoomlevel = minZoomCurrentInterval; zoomlevel <= maxZoomCurrentInterval; zoomlevel++)
				{
					int indexEntitiesPerZoomLevelTable = zoomlevel - minZoomCurrentInterval;
					IList<TDNode> pois = poisByZoomlevel[Convert.ToSByte(zoomlevel)];
					if (pois != null)
					{
						foreach (TDNode poi in pois)
						{
							processPOI(poi, currentTileLat, currentTileLon, configuration.DebugStrings, poiDataBuffer);
						}
						// increment count of POIs on this zoom level
						entitiesPerZoomLevel[indexEntitiesPerZoomLevelTable][0] += pois.Count;
					}
				}

				// WRITE WAYS
				for (sbyte zoomlevel = minZoomCurrentInterval; zoomlevel <= maxZoomCurrentInterval; zoomlevel++)
				{
					int indexEntitiesPerZoomLevelTable = zoomlevel - minZoomCurrentInterval;

					IList<TDWay> ways = waysByZoomlevel[Convert.ToSByte(zoomlevel)];
					if (ways != null)
					{
						IList<WayPreprocessingCallable> callables = new List<WayPreprocessingCallable>();
						foreach (TDWay way in ways)
						{
							if (!way.Invalid)
							{
								callables.Add(new WayPreprocessingCallable(way, tileCoordinate, maxZoomCurrentInterval, jtsGeometryCache, configuration));
							}
						}
						try
						{
							IList<Future<WayPreprocessingResult>> futures = EXECUTOR_SERVICE.invokeAll(callables);
							foreach (Future<WayPreprocessingResult> wprFuture in futures)
							{
								WayPreprocessingResult wpr;
								try
								{
									wpr = wprFuture.get();
								}
								catch (ExecutionException e)
								{
									LOGGER.log(Level.WARNING, "error in parallel preprocessing of ways", e);
									continue;
								}
								if (wpr != null)
								{
									wayBuffer.clear();
									// increment count of ways on this zoom level
									entitiesPerZoomLevel[indexEntitiesPerZoomLevelTable][1]++;
									if (configuration.DebugStrings)
									{
										writeWaySignature(wpr.Way, wayDataBuffer);
									}
									processWay(wpr, wpr.Way, currentTileLat, currentTileLon, wayBuffer);
									// write size of way to way data buffer
									wayDataBuffer.put(Serializer.getVariableByteUnsigned(wayBuffer.position()));
									// write way data to way data buffer
									wayDataBuffer.put(wayBuffer.array(), 0, wayBuffer.position());
								}
							}
						}
						catch (InterruptedException e)
						{
							LOGGER.log(Level.WARNING, "error in parallel preprocessing of ways", e);
						}
					}
				}

				// write zoom table
				writeZoomLevelTable(entitiesPerZoomLevel, tileBuffer);
				// write offset to first way in the tile header
				tileBuffer.put(Serializer.getVariableByteUnsigned(poiDataBuffer.position()));
				// write POI data to buffer
				tileBuffer.put(poiDataBuffer.array(), 0, poiDataBuffer.position());
				// write way data to buffer
				tileBuffer.put(wayDataBuffer.array(), 0, wayDataBuffer.position());
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeIndex(ByteBuffer indexBuffer, long startPositionSubfile, long subFileSize, java.io.RandomAccessFile randomAccessFile) throws java.io.IOException
		private static void writeIndex(ByteBuffer indexBuffer, long startPositionSubfile, long subFileSize, RandomAccessFile randomAccessFile)
		{
			randomAccessFile.seek(startPositionSubfile);
			randomAccessFile.write(indexBuffer.array());
			randomAccessFile.seek(subFileSize);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static long writeSubfile(final long startPositionSubfile, final int zoomIntervalIndex, final org.mapsforge.map.writer.model.TileBasedDataProcessor dataStore, final com.google.common.cache.LoadingCache<org.mapsforge.map.writer.model.TDWay, com.vividsolutions.jts.geom.Geometry> jtsGeometryCache, final java.io.RandomAccessFile randomAccessFile, final org.mapsforge.map.writer.model.MapWriterConfiguration configuration) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		private static long writeSubfile(long startPositionSubfile, int zoomIntervalIndex, TileBasedDataProcessor dataStore, LoadingCache<TDWay, Geometry> jtsGeometryCache, RandomAccessFile randomAccessFile, MapWriterConfiguration configuration)
		{
			LOGGER.fine("writing data for zoom interval " + zoomIntervalIndex + ", number of tiles: " + dataStore.getTileGridLayout(zoomIntervalIndex).AmountTilesHorizontal * dataStore.getTileGridLayout(zoomIntervalIndex).AmountTilesVertical);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.mapsforge.map.writer.model.TileCoordinate upperLeft = dataStore.getTileGridLayout(zoomIntervalIndex).getUpperLeft();
			TileCoordinate upperLeft = dataStore.getTileGridLayout(zoomIntervalIndex).UpperLeft;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int lengthX = dataStore.getTileGridLayout(zoomIntervalIndex).getAmountTilesHorizontal();
			int lengthX = dataStore.getTileGridLayout(zoomIntervalIndex).AmountTilesHorizontal;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int lengthY = dataStore.getTileGridLayout(zoomIntervalIndex).getAmountTilesVertical();
			int lengthY = dataStore.getTileGridLayout(zoomIntervalIndex).AmountTilesVertical;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int amountTiles = lengthX * lengthY;
			int amountTiles = lengthX * lengthY;

			// used to monitor progress
			double amountOfTilesInPercentStep = amountTiles;
			if (amountTiles > PROGRESS_PERCENT_STEP)
			{
				amountOfTilesInPercentStep = Math.Ceiling(amountTiles / PROGRESS_PERCENT_STEP);
			}

			int processedTiles = 0;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte baseZoomCurrentInterval = dataStore.getZoomIntervalConfiguration().getBaseZoom(zoomIntervalIndex);
			sbyte baseZoomCurrentInterval = dataStore.ZoomIntervalConfiguration.getBaseZoom(zoomIntervalIndex);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int tileAmountInBytes = lengthX * lengthY * BYTE_AMOUNT_SUBFILE_INDEX_PER_TILE;
			int tileAmountInBytes = lengthX * lengthY * BYTE_AMOUNT_SUBFILE_INDEX_PER_TILE;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int indexBufferSize = tileAmountInBytes + (configuration.isDebugStrings() ? DEBUG_INDEX_START_STRING.getBytes(UTF8_CHARSET).length : 0);
			int indexBufferSize = tileAmountInBytes + (configuration.DebugStrings ? DEBUG_INDEX_START_STRING.GetBytes(UTF8_CHARSET).length : 0);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer indexBuffer = ByteBuffer.allocate(indexBufferSize);
			ByteBuffer indexBuffer = ByteBuffer.allocate(indexBufferSize);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer tileBuffer = ByteBuffer.allocate(TILE_BUFFER_SIZE);
			ByteBuffer tileBuffer = ByteBuffer.allocate(TILE_BUFFER_SIZE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer wayDataBuffer = ByteBuffer.allocate(WAY_DATA_BUFFER_SIZE);
			ByteBuffer wayDataBuffer = ByteBuffer.allocate(WAY_DATA_BUFFER_SIZE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer wayBuffer = ByteBuffer.allocate(WAY_BUFFER_SIZE);
			ByteBuffer wayBuffer = ByteBuffer.allocate(WAY_BUFFER_SIZE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer poiDataBuffer = ByteBuffer.allocate(POI_DATA_BUFFER_SIZE);
			ByteBuffer poiDataBuffer = ByteBuffer.allocate(POI_DATA_BUFFER_SIZE);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer multipleTilesBuffer = ByteBuffer.allocate(TILES_BUFFER_SIZE);
			ByteBuffer multipleTilesBuffer = ByteBuffer.allocate(TILES_BUFFER_SIZE);

			// write debug strings for tile index segment if necessary
			if (configuration.DebugStrings)
			{
				indexBuffer.put(DEBUG_INDEX_START_STRING.GetBytes(UTF8_CHARSET));
			}

			long currentSubfileOffset = indexBufferSize;
			randomAccessFile.seek(startPositionSubfile + indexBufferSize);

			for (int tileY = upperLeft.Y; tileY < upperLeft.Y + lengthY; tileY++)
			{
				for (int tileX = upperLeft.X; tileX < upperLeft.X + lengthX; tileX++)
				{
					TileCoordinate tileCoordinate = new TileCoordinate(tileX, tileY, baseZoomCurrentInterval);

					processIndexEntry(tileCoordinate, indexBuffer, currentSubfileOffset);
					processTile(configuration, tileCoordinate, dataStore, jtsGeometryCache, zoomIntervalIndex, tileBuffer, poiDataBuffer, wayDataBuffer, wayBuffer);
					currentSubfileOffset += tileBuffer.position();

					writeTile(multipleTilesBuffer, tileBuffer, randomAccessFile);

					if (++processedTiles % amountOfTilesInPercentStep == 0)
					{
						if (processedTiles == amountTiles)
						{
							LOGGER.info("written 100% of sub file for zoom interval index " + zoomIntervalIndex);
						}
						else
						{
							LOGGER.info("written " + (processedTiles / amountOfTilesInPercentStep) * PROGRESS_PERCENT_STEP + "% of sub file for zoom interval index " + zoomIntervalIndex);
						}
					}

					// TODO accounting for progress information
				} // end for loop over tile columns
			} // /end for loop over tile rows

			// write remaining tiles
			if (multipleTilesBuffer.position() > 0)
			{
				// byte buffer was not previously cleared
				randomAccessFile.write(multipleTilesBuffer.array(), 0, multipleTilesBuffer.position());
			}

			writeIndex(indexBuffer, startPositionSubfile, currentSubfileOffset, randomAccessFile);

			// return size of sub file in bytes
			return currentSubfileOffset;
		}

		private static void writeSubfileMetaDataToContainerHeader(ZoomIntervalConfiguration zoomIntervalConfiguration, int i, long startIndexOfSubfile, long subfileSize, ByteBuffer buffer)
		{
			// HEADER META DATA FOR SUB FILE
			// write zoom interval configuration to header
			sbyte minZoomCurrentInterval = zoomIntervalConfiguration.getMinZoom(i);
			sbyte maxZoomCurrentInterval = zoomIntervalConfiguration.getMaxZoom(i);
			sbyte baseZoomCurrentInterval = zoomIntervalConfiguration.getBaseZoom(i);

			buffer.put(baseZoomCurrentInterval);
			buffer.put(minZoomCurrentInterval);
			buffer.put(maxZoomCurrentInterval);
			buffer.putLong(startIndexOfSubfile);
			buffer.putLong(subfileSize);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void writeTile(ByteBuffer multipleTilesBuffer, ByteBuffer tileBuffer, java.io.RandomAccessFile randomAccessFile) throws java.io.IOException
		private static void writeTile(ByteBuffer multipleTilesBuffer, ByteBuffer tileBuffer, RandomAccessFile randomAccessFile)
		{
			// add tile to tiles buffer
			multipleTilesBuffer.put(tileBuffer.array(), 0, tileBuffer.position());

			// if necessary, allocate new buffer
			if (multipleTilesBuffer.remaining() < MIN_TILE_BUFFER_SIZE)
			{
				randomAccessFile.write(multipleTilesBuffer.array(), 0, multipleTilesBuffer.position());
				multipleTilesBuffer.clear();
			}
		}

		private static void writeTileSignature(TileCoordinate tileCoordinate, ByteBuffer tileBuffer)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(DEBUG_STRING_TILE_HEAD).Append(tileCoordinate.X).Append(",").Append(tileCoordinate.Y).Append(DEBUG_STRING_TILE_TAIL);
			tileBuffer.put(sb.ToString().GetBytes(UTF8_CHARSET));
			// append withespaces so that block has 32 bytes
			appendWhitespace(DEBUG_BLOCK_SIZE - sb.ToString().GetBytes(UTF8_CHARSET).length, tileBuffer);
		}

		private static void writeUTF8(string @string, ByteBuffer buffer)
		{
			buffer.put(Serializer.getVariableByteUnsigned(@string.GetBytes(UTF8_CHARSET).length));
			buffer.put(@string.GetBytes(UTF8_CHARSET));
		}

		private static void writeWay(IList<int?> wayNodes, int currentTileLat, int currentTileLon, ByteBuffer buffer)
		{
			// write the amount of way nodes to the file
			// wayBuffer
			int wayNodeCount = wayNodes.Count / 2;
			if (wayNodeCount < 2)
			{
				LOGGER.warning("Invalid way node count: " + wayNodeCount);
			}
			buffer.put(Serializer.getVariableByteUnsigned(wayNodeCount));

			// write the way nodes:
			// the first node is always stored with four bytes
			// the remaining way node differences are stored according to the
			// compression type
			writeWayNodes(wayNodes, currentTileLat, currentTileLon, buffer);
		}

		private static void writeWaySignature(TDWay way, ByteBuffer tileBuffer)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(DEBUG_STRING_WAY_HEAD).Append(way.Id).Append(DEBUG_STRING_WAY_TAIL);
			tileBuffer.put(sb.ToString().GetBytes(UTF8_CHARSET));
			// append withespaces so that block has 32 bytes
			appendWhitespace(DEBUG_BLOCK_SIZE - sb.ToString().GetBytes(UTF8_CHARSET).length, tileBuffer);
		}

		private MapFileWriter()
		{
			// do nothing
		}
	}

}