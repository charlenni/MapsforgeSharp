using System.Diagnostics;
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
namespace org.mapsforge.map.writer
{

	using TLongArrayList = gnu.trove.list.array.TLongArrayList;
	using TLongObjectHashMap = gnu.trove.map.hash.TLongObjectHashMap;
	using TShortIntHashMap = gnu.trove.map.hash.TShortIntHashMap;
	using TObjectProcedure = gnu.trove.procedure.TObjectProcedure;
	using TLongSet = gnu.trove.set.TLongSet;
	using TLongHashSet = gnu.trove.set.hash.TLongHashSet;


	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using LatLongUtils = org.mapsforge.core.util.LatLongUtils;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using MapWriterConfiguration = org.mapsforge.map.writer.model.MapWriterConfiguration;
	using NodeResolver = org.mapsforge.map.writer.model.NodeResolver;
	using TDNode = org.mapsforge.map.writer.model.TDNode;
	using TDRelation = org.mapsforge.map.writer.model.TDRelation;
	using TDWay = org.mapsforge.map.writer.model.TDWay;
	using TileBasedDataProcessor = org.mapsforge.map.writer.model.TileBasedDataProcessor;
	using TileCoordinate = org.mapsforge.map.writer.model.TileCoordinate;
	using TileData = org.mapsforge.map.writer.model.TileData;
	using TileGridLayout = org.mapsforge.map.writer.model.TileGridLayout;
	using WayResolver = org.mapsforge.map.writer.model.WayResolver;
	using ZoomIntervalConfiguration = org.mapsforge.map.writer.model.ZoomIntervalConfiguration;
	using GeoUtils = org.mapsforge.map.writer.util.GeoUtils;

	using TopologyException = com.vividsolutions.jts.geom.TopologyException;

	internal abstract class BaseTileBasedDataProcessor : TileBasedDataProcessor, NodeResolver, WayResolver
	{

		protected internal class RelationHandler : TObjectProcedure<TDRelation>
		{
			private readonly BaseTileBasedDataProcessor outerInstance;

			public RelationHandler(BaseTileBasedDataProcessor outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal IList<Deque<TDWay>> extractedPolygons;

			internal IList<int?> inner;
			internal IDictionary<int?, IList<int?>> outerToInner;
			internal readonly WayPolygonizer polygonizer = new WayPolygonizer();

			public override bool execute(TDRelation relation)
			{
				if (relation == null)
				{
					return false;
				}

				this.extractedPolygons = null;
				this.outerToInner = null;

				TDWay[] members = relation.MemberWays;
				try
				{
					this.polygonizer.polygonizeAndRelate(members);
				}
				catch (TopologyException e)
				{
					LOGGER.log(Level.FINE, "cannot relate extracted polygons to each other for relation: " + relation.Id, e);
				}

				// skip invalid relations
				if (!this.polygonizer.Dangling.Empty)
				{
					if (outerInstance.skipInvalidRelations)
					{
						LOGGER.fine("skipping relation that contains dangling ways which could not be merged to polygons: " + relation.Id);
						return true;
					}
					LOGGER.fine("relation contains dangling ways which could not be merged to polygons: " + relation.Id);
				}
				else if (!this.polygonizer.Illegal.Empty)
				{
					if (outerInstance.skipInvalidRelations)
					{
						LOGGER.fine("skipping relation contains illegal closed ways with fewer than 4 nodes: " + relation.Id);
						return true;
					}
					LOGGER.fine("relation contains illegal closed ways with fewer than 4 nodes: " + relation.Id);
				}

				this.extractedPolygons = this.polygonizer.Polygons;
				this.outerToInner = this.polygonizer.OuterToInner;

				foreach (KeyValuePair<int?, IList<int?>> entry in this.outerToInner.SetOfKeyValuePairs())
				{
					Deque<TDWay> outerPolygon = this.extractedPolygons[entry.Key.intValue()];
					this.inner = null;
					this.inner = entry.Value;
					sbyte shape = TDWay.SIMPLE_POLYGON;
					// does it contain inner ways?
					if (this.inner != null && this.inner.Count > 0)
					{
						shape = TDWay.MULTI_POLYGON;
					}

					TDWay outerWay = null;
					if (outerPolygon.size() > 1)
					{
						// we need to create a new way from a set of ways
						// collect the way nodes and use the tags of the relation
						// if one of the ways has its own tags, we should ignore them,
						// ways with relevant tags will be added separately later
						if (!relation.RenderRelevant)
						{
							LOGGER.fine("constructed outer polygon in relation has no known tags: " + relation.Id);
							continue;
						}
						// merge way nodes from outer way segments
						IList<TDNode> waynodeList = new List<TDNode>();
						foreach (TDWay outerSegment in outerPolygon)
						{
							if (outerSegment.ReversedInRelation)
							{
								for (int i = outerSegment.WayNodes.length - 1; i >= 0; i--)
								{
									waynodeList.Add(outerSegment.WayNodes[i]);
								}
							}
							else
							{
								foreach (TDNode tdNode in outerSegment.WayNodes)
								{
									waynodeList.Add(tdNode);
								}
							}
						}
						TDNode[] waynodes = waynodeList.ToArray();

						// create new virtual way which represents the outer way
						// use maxWayID counter to create unique id
						outerWay = new TDWay(++outerInstance.maxWayID, relation.Layer, relation.Name, relation.HouseNumber, relation.Ref, relation.Tags, shape, waynodes);

						// add the newly created way to matching tiles
						outerInstance.addWayToTiles(outerWay, outerInstance.bboxEnlargement);
						outerInstance.handleVirtualOuterWay(outerWay);
						// adjust tag statistics, cannot be omitted!!!
						outerInstance.countWayTags(relation.Tags);
					}

					// the outer way consists of only one segment
					else
					{
						outerWay = outerPolygon.First;

						// is it a polygon that we have seen already and which was
						// identified as a polgyon containing inner ways?
						if (outerInstance.outerToInnerMapping.contains(outerWay.Id))
						{
							shape = TDWay.MULTI_POLYGON;
						}
						outerWay.Shape = shape;

						// we merge the name, ref, tag information of the relation to the outer way
						// TODO is this true?
						// a relation that addresses an already closed way, is normally used to add
						// additional information to the way
						outerWay.mergeRelationInformation(relation);
						// only consider the way, if it has tags, otherwise the renderer cannot interpret
						// the way
						if (outerWay.RenderRelevant)
						{
							// handle relation tags
							outerInstance.handleAdditionalRelationTags(outerWay, relation);
							outerInstance.addWayToTiles(outerWay, outerInstance.bboxEnlargement);
							outerInstance.countWayTags(outerWay.Tags);
						}
					}

					// relate inner ways to outer way
					addInnerWays(outerWay);
				}
				return true;
			}

			internal virtual void addInnerWays(TDWay outer)
			{
				if (this.inner != null && this.inner.Count > 0)
				{
					TLongArrayList innerList = outerInstance.outerToInnerMapping.get(outer.Id);
					if (innerList == null)
					{
						innerList = new TLongArrayList();
						outerInstance.outerToInnerMapping.put(outer.Id, innerList);
					}

					foreach (int? innerIndex in this.inner)
					{
						Deque<TDWay> innerSegments = this.extractedPolygons[innerIndex.Value];
						TDWay innerWay = null;

						if (innerSegments.size() == 1)
						{
							innerWay = innerSegments.First;
							if (innerWay.hasTags() && outer.hasTags())
							{
								short[] iTags = innerWay.Tags;
								short[] oTags = outer.Tags;
								int contained = 0;
								foreach (short iTagID in iTags)
								{
									foreach (short oTagID in oTags)
									{
										if (iTagID == oTagID)
										{
											contained++;
										}
									}
								}
								if (contained == iTags.Length)
								{
									outerInstance.innerWaysWithoutAdditionalTags.add(innerWay.Id);
								}
							}
						}
						else
						{
							IList<TDNode> waynodeList = new List<TDNode>();
							foreach (TDWay innerSegment in innerSegments)
							{
								if (innerSegment.ReversedInRelation)
								{
									for (int i = innerSegment.WayNodes.length - 1; i >= 0; i--)
									{
										waynodeList.Add(innerSegment.WayNodes[i]);
									}
								}
								else
								{
									foreach (TDNode tdNode in innerSegment.WayNodes)
									{
										waynodeList.Add(tdNode);
									}
								}
							}
							TDNode[] waynodes = waynodeList.ToArray();
							// TODO which layer?
							innerWay = new TDWay(++outerInstance.maxWayID, (sbyte) 0, null, null, null, waynodes);
							outerInstance.handleVirtualInnerWay(innerWay);
							// does not need to be added to corresponding tiles
							// virtual inner ways do not have any tags, they are holes in the outer polygon
						}
						innerList.add(innerWay.Id);
					}
				}
			}
		}

		protected internal class WayHandler : TObjectProcedure<TDWay>
		{
			private readonly BaseTileBasedDataProcessor outerInstance;

			public WayHandler(BaseTileBasedDataProcessor outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool execute(TDWay way)
			{
				if (way == null)
				{
					return true;
				}
				// we only consider ways that have tags and which have not already
				// added as outer way of a relation
				// inner ways without additional tags are also not considered as they are processed as part of a
				// multi polygon
				if (way.RenderRelevant && !outerInstance.outerToInnerMapping.contains(way.Id) && !outerInstance.innerWaysWithoutAdditionalTags.contains(way.Id))
				{
					outerInstance.addWayToTiles(way, outerInstance.bboxEnlargement);
				}

				return true;
			}
		}

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		protected internal static readonly Logger LOGGER = Logger.getLogger(typeof(BaseTileBasedDataProcessor).FullName);
		protected internal readonly int bboxEnlargement;
		protected internal readonly BoundingBox boundingbox;
		// accounting
		protected internal float[] countWays;
		protected internal float[] countWayTileFactor;

		protected internal readonly TShortIntHashMap histogramPoiTags;
		protected internal readonly TShortIntHashMap histogramWayTags;
		protected internal readonly TLongSet innerWaysWithoutAdditionalTags;

		protected internal long maxWayID = long.MinValue;
		protected internal readonly TLongObjectHashMap<TLongArrayList> outerToInnerMapping;

		protected internal readonly IList<string> preferredLanguages;
		protected internal readonly bool skipInvalidRelations;
		protected internal TileGridLayout[] tileGridLayouts;

		// public BaseTileBasedDataProcessor(double minLat, double maxLat, double minLon, double maxLon,
		// ZoomIntervalConfiguration zoomIntervalConfiguration, int bboxEnlargement, String preferredLanguage) {
		// this(new Rect(minLon, maxLon, minLat, maxLat), zoomIntervalConfiguration, bboxEnlargement,
		// preferredLanguage);
		//
		// }

		protected internal readonly IDictionary<TileCoordinate, TLongHashSet> tilesToCoastlines;

		protected internal readonly ZoomIntervalConfiguration zoomIntervalConfiguration;

		public BaseTileBasedDataProcessor(MapWriterConfiguration configuration) : base()
		{
			this.boundingbox = configuration.BboxConfiguration;
			this.zoomIntervalConfiguration = configuration.ZoomIntervalConfiguration;
			this.tileGridLayouts = new TileGridLayout[this.zoomIntervalConfiguration.NumberOfZoomIntervals];
			this.bboxEnlargement = configuration.BboxEnlargement;
			this.preferredLanguages = configuration.PreferredLanguages;
			this.skipInvalidRelations = configuration.SkipInvalidRelations;

			this.outerToInnerMapping = new TLongObjectHashMap<>();
			this.innerWaysWithoutAdditionalTags = new TLongHashSet();
			this.tilesToCoastlines = new Dictionary<>();

			this.countWays = new float[this.zoomIntervalConfiguration.NumberOfZoomIntervals];
			this.countWayTileFactor = new float[this.zoomIntervalConfiguration.NumberOfZoomIntervals];

			this.histogramPoiTags = new TShortIntHashMap();
			this.histogramWayTags = new TShortIntHashMap();

			// compute horizontal and vertical tile coordinate offsets for all
			// base zoom levels
			for (int i = 0; i < this.zoomIntervalConfiguration.NumberOfZoomIntervals; i++)
			{
				TileCoordinate upperLeft = new TileCoordinate((int) MercatorProjection.longitudeToTileX(this.boundingbox.minLongitude, this.zoomIntervalConfiguration.getBaseZoom(i)), (int) MercatorProjection.latitudeToTileY(this.boundingbox.maxLatitude, this.zoomIntervalConfiguration.getBaseZoom(i)), this.zoomIntervalConfiguration.getBaseZoom(i));
				this.tileGridLayouts[i] = new TileGridLayout(upperLeft, computeNumberOfHorizontalTiles(i), computeNumberOfVerticalTiles(i));
			}
		}

		public override long cumulatedNumberOfTiles()
		{
			long cumulated = 0;
			for (int i = 0; i < this.zoomIntervalConfiguration.NumberOfZoomIntervals; i++)
			{
				cumulated += this.tileGridLayouts[i].AmountTilesHorizontal * this.tileGridLayouts[i].AmountTilesVertical;
			}
			return cumulated;
		}

		public override BoundingBox BoundingBox
		{
			get
			{
				return this.boundingbox;
			}
		}

		public override TileGridLayout getTileGridLayout(int zoomIntervalIndex)
		{
			return this.tileGridLayouts[zoomIntervalIndex];
		}

		public override ZoomIntervalConfiguration ZoomIntervalConfiguration
		{
			get
			{
				return this.zoomIntervalConfiguration;
			}
		}

		protected internal virtual void addPOI(TDNode poi)
		{
			if (!poi.POI)
			{
				return;
			}

			sbyte minZoomLevel = poi.ZoomAppear;
			for (int i = 0; i < this.zoomIntervalConfiguration.NumberOfZoomIntervals; i++)
			{
				// is POI seen in a zoom interval?
				if (minZoomLevel <= this.zoomIntervalConfiguration.getMaxZoom(i))
				{
					long tileCoordinateX = MercatorProjection.longitudeToTileX(LatLongUtils.microdegreesToDegrees(poi.Longitude), this.zoomIntervalConfiguration.getBaseZoom(i));
					long tileCoordinateY = MercatorProjection.latitudeToTileY(LatLongUtils.microdegreesToDegrees(poi.Latitude), this.zoomIntervalConfiguration.getBaseZoom(i));
					TileData tileData = getTileImpl(i, (int) tileCoordinateX, (int) tileCoordinateY);
					if (tileData != null)
					{
						tileData.addPOI(poi);
						countPoiTags(poi);
					}
				}
			}
		}

		protected internal virtual void addWayToTiles(TDWay way, int enlargement)
		{
			int bboxEnlargementLocal = enlargement;
			sbyte minZoomLevel = way.MinimumZoomLevel;
			for (int i = 0; i < this.zoomIntervalConfiguration.NumberOfZoomIntervals; i++)
			{
				// is way seen in a zoom interval?
				if (minZoomLevel <= this.zoomIntervalConfiguration.getMaxZoom(i))
				{
					ISet<TileCoordinate> matchedTiles = GeoUtils.mapWayToTiles(way, this.zoomIntervalConfiguration.getBaseZoom(i), bboxEnlargementLocal);
					bool added = false;
					foreach (TileCoordinate matchedTile in matchedTiles)
					{
						TileData td = getTileImpl(i, matchedTile.X, matchedTile.Y);
						if (td != null)
						{
							countWayTags(way);
							this.countWayTileFactor[i]++;
							added = true;
							td.addWay(way);
						}
					}
					if (added)
					{
						this.countWays[i]++;
					}
				}
			}
		}

		protected internal virtual void countPoiTags(TDNode poi)
		{
			if (poi == null || poi.Tags == null)
			{
				return;
			}
			foreach (short tag in poi.Tags)
			{
				this.histogramPoiTags.adjustOrPutValue(tag, 1, 1);
			}
		}

		protected internal virtual void countWayTags(short[] tags)
		{
			if (tags != null)
			{
				foreach (short tag in tags)
				{
					this.histogramWayTags.adjustOrPutValue(tag, 1, 1);
				}
			}
		}

		protected internal virtual void countWayTags(TDWay way)
		{
			if (way != null)
			{
				countWayTags(way.Tags);
			}
		}

		protected internal abstract TileData getTileImpl(int zoom, int tileX, int tileY);

		protected internal abstract void handleAdditionalRelationTags(TDWay virtualWay, TDRelation relation);

		protected internal abstract void handleVirtualInnerWay(TDWay virtualWay);

		protected internal abstract void handleVirtualOuterWay(TDWay virtualWay);

		private int computeNumberOfHorizontalTiles(int zoomIntervalIndex)
		{
			long tileCoordinateLeft = MercatorProjection.longitudeToTileX(this.boundingbox.minLongitude, this.zoomIntervalConfiguration.getBaseZoom(zoomIntervalIndex));

			long tileCoordinateRight = MercatorProjection.longitudeToTileX(this.boundingbox.maxLongitude, this.zoomIntervalConfiguration.getBaseZoom(zoomIntervalIndex));

			Debug.Assert(tileCoordinateLeft <= tileCoordinateRight);
			Debug.Assert(tileCoordinateLeft - tileCoordinateRight + 1 < int.MaxValue);

			LOGGER.finer("basezoom: " + this.zoomIntervalConfiguration.getBaseZoom(zoomIntervalIndex) + "\t+n_horizontal: " + (tileCoordinateRight - tileCoordinateLeft + 1));

			return (int)(tileCoordinateRight - tileCoordinateLeft + 1);
		}

		private int computeNumberOfVerticalTiles(int zoomIntervalIndex)
		{
			long tileCoordinateBottom = MercatorProjection.latitudeToTileY(this.boundingbox.minLatitude, this.zoomIntervalConfiguration.getBaseZoom(zoomIntervalIndex));

			long tileCoordinateTop = MercatorProjection.latitudeToTileY(this.boundingbox.maxLatitude, this.zoomIntervalConfiguration.getBaseZoom(zoomIntervalIndex));

			Debug.Assert(tileCoordinateBottom >= tileCoordinateTop);
			Debug.Assert(tileCoordinateBottom - tileCoordinateTop + 1 <= int.MaxValue);

			LOGGER.finer("basezoom: " + this.zoomIntervalConfiguration.getBaseZoom(zoomIntervalIndex) + "\t+n_vertical: " + (tileCoordinateBottom - tileCoordinateTop + 1));

			return (int)(tileCoordinateBottom - tileCoordinateTop + 1);
		}
	}

}