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
namespace org.mapsforge.map.writer
{

	using TLongArrayList = gnu.trove.list.array.TLongArrayList;
	using TLongObjectHashMap = gnu.trove.map.hash.TLongObjectHashMap;
	using TLongProcedure = gnu.trove.procedure.TLongProcedure;
	using TLongHashSet = gnu.trove.set.hash.TLongHashSet;


	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using MapWriterConfiguration = org.mapsforge.map.writer.model.MapWriterConfiguration;
	using TDNode = org.mapsforge.map.writer.model.TDNode;
	using TDRelation = org.mapsforge.map.writer.model.TDRelation;
	using TDWay = org.mapsforge.map.writer.model.TDWay;
	using TileCoordinate = org.mapsforge.map.writer.model.TileCoordinate;
	using TileData = org.mapsforge.map.writer.model.TileData;
	using TileInfo = org.mapsforge.map.writer.model.TileInfo;
	using ZoomIntervalConfiguration = org.mapsforge.map.writer.model.ZoomIntervalConfiguration;
	using GeoUtils = org.mapsforge.map.writer.util.GeoUtils;
	using Node = org.openstreetmap.osmosis.core.domain.v0_6.Node;
	using Relation = org.openstreetmap.osmosis.core.domain.v0_6.Relation;
	using Way = org.openstreetmap.osmosis.core.domain.v0_6.Way;

	/// <summary>
	/// A TileBasedDataStore that uses the RAM as storage device for temporary data structures.
	/// </summary>
	public sealed class RAMTileBasedDataProcessor : BaseTileBasedDataProcessor
	{

		/// <summary>
		/// Creates a new instance of a <seealso cref="RAMTileBasedDataProcessor"/>.
		/// </summary>
		/// <param name="configuration">
		///            the configuration </param>
		/// <returns> a new instance of a <seealso cref="RAMTileBasedDataProcessor"/> </returns>
		public static RAMTileBasedDataProcessor newInstance(MapWriterConfiguration configuration)
		{
			return new RAMTileBasedDataProcessor(configuration);
		}

		internal readonly TLongObjectHashMap<TDWay> ways;
		private readonly TLongObjectHashMap<TDRelation> multipolygons;

		private readonly TLongObjectHashMap<TDNode> nodes;

		private readonly RAMTileData[][][] tileData;

		private RAMTileBasedDataProcessor(MapWriterConfiguration configuration) : base(configuration)
		{
			this.nodes = new TLongObjectHashMap<>();
			this.ways = new TLongObjectHashMap<>();
			this.multipolygons = new TLongObjectHashMap<>();
			this.tileData = new RAMTileData[this.zoomIntervalConfiguration.NumberOfZoomIntervals][][];
			// compute number of tiles needed on each base zoom level
			for (int i = 0; i < this.zoomIntervalConfiguration.NumberOfZoomIntervals; i++)
			{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: this.tileData[i] = new RAMTileData[this.tileGridLayouts[i].AmountTilesHorizontal][this.tileGridLayouts[i].AmountTilesVertical];
				this.tileData[i] = RectangularArrays.ReturnRectangularRAMTileDataArray(this.tileGridLayouts[i].AmountTilesHorizontal, this.tileGridLayouts[i].AmountTilesVertical);
			}
		}

		public override void addNode(Node node)
		{
			TDNode tdNode = TDNode.fromNode(node, this.preferredLanguages);
			this.nodes.put(tdNode.Id, tdNode);
			addPOI(tdNode);
		}

		public override void addRelation(Relation relation)
		{
			TDRelation tdRelation = TDRelation.fromRelation(relation, this, this.preferredLanguages);
			if (tdRelation != null)
			{
				this.multipolygons.put(relation.Id, tdRelation);
			}
		}

		public override void addWay(Way way)
		{
			TDWay tdWay = TDWay.fromWay(way, this, this.preferredLanguages);
			if (tdWay == null)
			{
				return;
			}
			this.ways.put(tdWay.Id, tdWay);
			this.maxWayID = Math.Max(this.maxWayID, way.Id);

			if (tdWay.Coastline)
			{
				// find matching tiles on zoom level 12
				ISet<TileCoordinate> coastLineTiles = GeoUtils.mapWayToTiles(tdWay, TileInfo.TILE_INFO_ZOOMLEVEL, 0);
				foreach (TileCoordinate tileCoordinate in coastLineTiles)
				{
					TLongHashSet coastlines = this.tilesToCoastlines.get(tileCoordinate);
					if (coastlines == null)
					{
						coastlines = new TLongHashSet();
						this.tilesToCoastlines.put(tileCoordinate, coastlines);
					}
					coastlines.add(tdWay.Id);
				}
			}
		}

		public override void complete()
		{
			// Polygonize multipolygon
			RelationHandler relationHandler = new RelationHandler();
			this.multipolygons.forEachValue(relationHandler);

			WayHandler wayHandler = new WayHandler();
			this.ways.forEachValue(wayHandler);

			OSMTagMapping.Instance.optimizePoiOrdering(this.histogramPoiTags);
			OSMTagMapping.Instance.optimizeWayOrdering(this.histogramWayTags);
		}

		public override BoundingBox BoundingBox
		{
			get
			{
				return this.boundingbox;
			}
		}

		public override ISet<TDWay> getCoastLines(TileCoordinate tc)
		{
			if (tc.Zoomlevel <= TileInfo.TILE_INFO_ZOOMLEVEL)
			{
				return Collections.emptySet();
			}
			TileCoordinate correspondingOceanTile = tc.translateToZoomLevel(TileInfo.TILE_INFO_ZOOMLEVEL).get(0);
			TLongHashSet coastlines = this.tilesToCoastlines.get(correspondingOceanTile);
			if (coastlines == null)
			{
				return Collections.emptySet();
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<org.mapsforge.map.writer.model.TDWay> res = new java.util.HashSet<>();
			ISet<TDWay> res = new HashSet<TDWay>();
			coastlines.forEach(new TLongProcedureAnonymousInnerClassHelper(this, res));
			return res;
		}

		private class TLongProcedureAnonymousInnerClassHelper : TLongProcedure
		{
			private readonly RAMTileBasedDataProcessor outerInstance;

			private ISet<TDWay> res;

			public TLongProcedureAnonymousInnerClassHelper(RAMTileBasedDataProcessor outerInstance, ISet<TDWay> res)
			{
				this.outerInstance = outerInstance;
				this.res = res;
			}

			public override bool execute(long id)
			{
				TDWay way = outerInstance.ways.get(id);
				if (way != null)
				{
					res.Add(way);
					return true;
				}
				return false;
			}
		}

		public override IList<TDWay> getInnerWaysOfMultipolygon(long outerWayID)
		{
			TLongArrayList innerwayIDs = this.outerToInnerMapping.get(outerWayID);
			if (innerwayIDs == null)
			{
				return null;
			}
			return getInnerWaysOfMultipolygon(innerwayIDs.toArray());
		}

		public override TDNode getNode(long id)
		{
			return this.nodes.get(id);
		}

		public override TileData getTile(int zoom, int tileX, int tileY)
		{
			return getTileImpl(zoom, tileX, tileY);
		}

		public override TDWay getWay(long id)
		{
			return this.ways.get(id);
		}

		public override ZoomIntervalConfiguration ZoomIntervalConfiguration
		{
			get
			{
				return this.zoomIntervalConfiguration;
			}
		}

		public override void release()
		{
			// nothing to do here
		}

		protected internal override RAMTileData getTileImpl(int zoom, int tileX, int tileY)
		{
			int tileCoordinateXIndex = tileX - this.tileGridLayouts[zoom].UpperLeft.X;
			int tileCoordinateYIndex = tileY - this.tileGridLayouts[zoom].UpperLeft.Y;
			// check for valid range
			if (tileCoordinateXIndex < 0 || tileCoordinateYIndex < 0 || this.tileData[zoom].Length <= tileCoordinateXIndex || this.tileData[zoom][tileCoordinateXIndex].Length <= tileCoordinateYIndex)
			{
				return null;
			}

			RAMTileData td = this.tileData[zoom][tileCoordinateXIndex][tileCoordinateYIndex];
			if (td == null)
			{
				td = new RAMTileData();
				this.tileData[zoom][tileCoordinateXIndex][tileCoordinateYIndex] = td;
			}

			return td;
		}

		protected internal override void handleAdditionalRelationTags(TDWay virtualWay, TDRelation relation)
		{
			// nothing to do here
		}

		protected internal override void handleVirtualInnerWay(TDWay virtualWay)
		{
			this.ways.put(virtualWay.Id, virtualWay);
		}

		protected internal override void handleVirtualOuterWay(TDWay virtualWay)
		{
			// nothing to do here
		}

		private IList<TDWay> getInnerWaysOfMultipolygon(long[] innerWayIDs)
		{
			if (innerWayIDs == null)
			{
				return Collections.emptyList();
			}
			IList<TDWay> res = new List<TDWay>();
			foreach (long id in innerWayIDs)
			{
				TDWay current = this.ways.get(id);
				if (current == null)
				{
					continue;
				}
				res.Add(current);
			}

			return res;
		}
	}

}