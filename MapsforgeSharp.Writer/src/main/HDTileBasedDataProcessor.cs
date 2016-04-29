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

	using TLongIterator = gnu.trove.iterator.TLongIterator;
	using TLongArrayList = gnu.trove.list.array.TLongArrayList;
	using TLongObjectMap = gnu.trove.map.TLongObjectMap;
	using TLongObjectHashMap = gnu.trove.map.hash.TLongObjectHashMap;
	using TLongHashSet = gnu.trove.set.hash.TLongHashSet;


	using MapWriterConfiguration = org.mapsforge.map.writer.model.MapWriterConfiguration;
	using TDNode = org.mapsforge.map.writer.model.TDNode;
	using TDRelation = org.mapsforge.map.writer.model.TDRelation;
	using TDWay = org.mapsforge.map.writer.model.TDWay;
	using TileCoordinate = org.mapsforge.map.writer.model.TileCoordinate;
	using TileData = org.mapsforge.map.writer.model.TileData;
	using TileInfo = org.mapsforge.map.writer.model.TileInfo;
	using Node = org.openstreetmap.osmosis.core.domain.v0_6.Node;
	using Relation = org.openstreetmap.osmosis.core.domain.v0_6.Relation;
	using Way = org.openstreetmap.osmosis.core.domain.v0_6.Way;
	using ReleasableIterator = org.openstreetmap.osmosis.core.lifecycle.ReleasableIterator;
	using IndexedObjectStore = org.openstreetmap.osmosis.core.store.IndexedObjectStore;
	using IndexedObjectStoreReader = org.openstreetmap.osmosis.core.store.IndexedObjectStoreReader;
	using NoSuchIndexElementException = org.openstreetmap.osmosis.core.store.NoSuchIndexElementException;
	using SimpleObjectStore = org.openstreetmap.osmosis.core.store.SimpleObjectStore;
	using SingleClassObjectSerializationFactory = org.openstreetmap.osmosis.core.store.SingleClassObjectSerializationFactory;

	/// <summary>
	/// A TileBasedDataStore that uses the hard disk as storage device for temporary data structures.
	/// </summary>
	public sealed class HDTileBasedDataProcessor : BaseTileBasedDataProcessor
	{

		/// <summary>
		/// Creates a new <seealso cref="HDTileBasedDataProcessor"/>.
		/// </summary>
		/// <param name="configuration">
		///            the configuration </param>
		/// <returns> a new instance of a <seealso cref="HDTileBasedDataProcessor"/> </returns>
		public static HDTileBasedDataProcessor newInstance(MapWriterConfiguration configuration)
		{
			return new HDTileBasedDataProcessor(configuration);
		}

		internal readonly TLongObjectMap<IList<TDRelation>> additionalRelationTags;
		internal readonly TLongObjectMap<TDWay> virtualWays;
		private readonly IndexedObjectStore<Node> indexedNodeStore;
		private readonly IndexedObjectStore<Way> indexedWayStore;

		private IndexedObjectStoreReader<Node> nodeIndexReader;
		private readonly SimpleObjectStore<Relation> relationStore;

		private readonly HDTileData[][][] tileData;
		private IndexedObjectStoreReader<Way> wayIndexReader;

		private readonly SimpleObjectStore<Way> wayStore;

		private HDTileBasedDataProcessor(MapWriterConfiguration configuration) : base(configuration)
		{
			this.indexedNodeStore = new IndexedObjectStore<>(new SingleClassObjectSerializationFactory(typeof(Node)), "idxNodes");
			this.indexedWayStore = new IndexedObjectStore<>(new SingleClassObjectSerializationFactory(typeof(Way)), "idxWays");
			// indexedRelationStore = new IndexedObjectStore<Relation>(
			// new SingleClassObjectSerializationFactory(
			// Relation.class), "idxWays");
			this.wayStore = new SimpleObjectStore<>(new SingleClassObjectSerializationFactory(typeof(Way)), "heapWays", true);
			this.relationStore = new SimpleObjectStore<>(new SingleClassObjectSerializationFactory(typeof(Relation)), "heapRelations", true);

			this.tileData = new HDTileData[this.zoomIntervalConfiguration.NumberOfZoomIntervals][][];
			for (int i = 0; i < this.zoomIntervalConfiguration.NumberOfZoomIntervals; i++)
			{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: this.tileData[i] = new HDTileData[this.tileGridLayouts[i].AmountTilesHorizontal][this.tileGridLayouts[i].AmountTilesVertical];
				this.tileData[i] = RectangularArrays.ReturnRectangularHDTileDataArray(this.tileGridLayouts[i].AmountTilesHorizontal, this.tileGridLayouts[i].AmountTilesVertical);
			}
			this.virtualWays = new TLongObjectHashMap<>();
			this.additionalRelationTags = new TLongObjectHashMap<>();
		}

		public override void addNode(Node node)
		{
			this.indexedNodeStore.add(node.Id, node);
			TDNode tdNode = TDNode.fromNode(node, this.preferredLanguages);
			addPOI(tdNode);
		}

		public override void addRelation(Relation relation)
		{
			this.relationStore.add(relation);
		}

		public override void addWay(Way way)
		{
			this.wayStore.add(way);
			this.indexedWayStore.add(way.Id, way);
			this.maxWayID = Math.Max(way.Id, this.maxWayID);
		}

		// TODO add accounting of average number of tiles per way
		public override void complete()
		{
			this.indexedNodeStore.complete();
			this.nodeIndexReader = this.indexedNodeStore.createReader();

			this.indexedWayStore.complete();
			this.wayIndexReader = this.indexedWayStore.createReader();

			// handle relations
			ReleasableIterator<Relation> relationReader = this.relationStore.iterate();
			RelationHandler relationHandler = new RelationHandler();
			while (relationReader.hasNext())
			{
				Relation entry = relationReader.next();
				TDRelation tdRelation = TDRelation.fromRelation(entry, this, this.preferredLanguages);
				relationHandler.execute(tdRelation);
			}

			// handle ways
			ReleasableIterator<Way> wayReader = this.wayStore.iterate();
			WayHandler wayHandler = new WayHandler();
			while (wayReader.hasNext())
			{
				Way way = wayReader.next();
				TDWay tdWay = TDWay.fromWay(way, this, this.preferredLanguages);
				if (tdWay == null)
				{
					continue;
				}
				IList<TDRelation> associatedRelations = this.additionalRelationTags.get(tdWay.Id);
				if (associatedRelations != null)
				{
					foreach (TDRelation tileDataRelation in associatedRelations)
					{
						tdWay.mergeRelationInformation(tileDataRelation);
					}
				}

				wayHandler.execute(tdWay);
			}

			OSMTagMapping.Instance.optimizePoiOrdering(this.histogramPoiTags);
			OSMTagMapping.Instance.optimizeWayOrdering(this.histogramWayTags);
		}

		public override ISet<TDWay> getCoastLines(TileCoordinate tc)
		{
			if (tc.Zoomlevel <= TileInfo.TILE_INFO_ZOOMLEVEL)
			{
				return Collections.emptySet();
			}
			TileCoordinate correspondingOceanTile = tc.translateToZoomLevel(TileInfo.TILE_INFO_ZOOMLEVEL).get(0);

			if (this.wayIndexReader == null)
			{
				throw new System.InvalidOperationException("way store not accessible, call complete() first");
			}

			TLongHashSet coastlines = this.tilesToCoastlines.get(correspondingOceanTile);
			if (coastlines == null)
			{
				return Collections.emptySet();
			}

			TLongIterator it = coastlines.GetEnumerator();
			HashSet<TDWay> coastlinesAsTDWay = new HashSet<TDWay>(coastlines.size());
			while (it.hasNext())
			{
				long id = it.next();
				TDWay tdWay = null;
				try
				{
					tdWay = TDWay.fromWay(this.wayIndexReader.get(id), this, this.preferredLanguages);
				}
				catch (NoSuchIndexElementException)
				{
					LOGGER.finer("coastline way non-existing" + id);
				}
				if (tdWay != null)
				{
					coastlinesAsTDWay.Add(tdWay);
				}
			}
			return coastlinesAsTDWay;
		}

		public override IList<TDWay> getInnerWaysOfMultipolygon(long outerWayID)
		{
			lock (this)
			{
				TLongArrayList innerwayIDs = this.outerToInnerMapping.get(outerWayID);
				if (innerwayIDs == null)
				{
					return null;
				}
				return getInnerWaysOfMultipolygon(innerwayIDs.toArray());
			}
		}

		public override TDNode getNode(long id)
		{
			if (this.nodeIndexReader == null)
			{
				throw new System.InvalidOperationException("node store not accessible, call complete() first");
			}

			try
			{
				return TDNode.fromNode(this.nodeIndexReader.get(id), this.preferredLanguages);
			}
			catch (NoSuchIndexElementException)
			{
				LOGGER.finer("node cannot be found in index: " + id);
				return null;
			}
		}

		public override TileData getTile(int baseZoomIndex, int tileCoordinateX, int tileCoordinateY)
		{
			HDTileData hdt = getTileImpl(baseZoomIndex, tileCoordinateX, tileCoordinateY);
			if (hdt == null)
			{
				return null;
			}

			return fromHDTileData(hdt);
		}

		public override TDWay getWay(long id)
		{
			if (this.wayIndexReader == null)
			{
				throw new System.InvalidOperationException("way store not accessible, call complete() first");
			}

			try
			{
				return TDWay.fromWay(this.wayIndexReader.get(id), this, this.preferredLanguages);
			}
			catch (NoSuchIndexElementException)
			{
				LOGGER.finer("way cannot be found in index: " + id);
				return null;
			}
		}

		public override void release()
		{
			this.indexedNodeStore.release();
			this.indexedWayStore.release();
			this.wayStore.release();
			this.relationStore.release();
		}

		protected internal override HDTileData getTileImpl(int zoom, int tileX, int tileY)
		{
			int tileCoordinateXIndex = tileX - this.tileGridLayouts[zoom].UpperLeft.X;
			int tileCoordinateYIndex = tileY - this.tileGridLayouts[zoom].UpperLeft.Y;
			// check for valid range
			if (tileCoordinateXIndex < 0 || tileCoordinateYIndex < 0 || this.tileData[zoom].Length <= tileCoordinateXIndex || this.tileData[zoom][tileCoordinateXIndex].Length <= tileCoordinateYIndex)
			{
				return null;
			}

			HDTileData td = this.tileData[zoom][tileCoordinateXIndex][tileCoordinateYIndex];
			if (td == null)
			{
				td = new HDTileData();
				this.tileData[zoom][tileCoordinateXIndex][tileCoordinateYIndex] = td;
			}

			return td;
		}

		protected internal override void handleAdditionalRelationTags(TDWay way, TDRelation relation)
		{
			IList<TDRelation> associatedRelations = this.additionalRelationTags.get(way.Id);
			if (associatedRelations == null)
			{
				associatedRelations = new List<>();
				this.additionalRelationTags.put(way.Id, associatedRelations);
			}
			associatedRelations.Add(relation);
		}

		protected internal override void handleVirtualInnerWay(TDWay virtualWay)
		{
			this.virtualWays.put(virtualWay.Id, virtualWay);
		}

		protected internal override void handleVirtualOuterWay(TDWay virtualWay)
		{
			this.virtualWays.put(virtualWay.Id, virtualWay);
		}

		private RAMTileData fromHDTileData(HDTileData hdt)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RAMTileData td = new RAMTileData();
			RAMTileData td = new RAMTileData();
			TLongIterator it = hdt.Pois.GetEnumerator();
			while (it.hasNext())
			{
				td.addPOI(TDNode.fromNode(this.nodeIndexReader.get(it.next()), this.preferredLanguages));
			}

			it = hdt.Ways.GetEnumerator();
			while (it.hasNext())
			{
				TDWay way = null;
				long id = it.next();
				try
				{
					way = TDWay.fromWay(this.wayIndexReader.get(id), this, this.preferredLanguages);
					td.addWay(way);
				}
				catch (NoSuchIndexElementException)
				{
					// is it a virtual way?
					way = this.virtualWays.get(id);
					if (way != null)
					{
						td.addWay(way);
					}
					else
					{
						LOGGER.finer("referenced way non-existing" + id);
					}
				}

				if (way != null)
				{
					if (this.outerToInnerMapping.contains(way.Id))
					{
						way.Shape = TDWay.MULTI_POLYGON;
					}

					IList<TDRelation> associatedRelations = this.additionalRelationTags.get(id);
					if (associatedRelations != null)
					{
						foreach (TDRelation tileDataRelation in associatedRelations)
						{
							way.mergeRelationInformation(tileDataRelation);
						}
					}
				}
			}

			return td;
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
				TDWay current = null;
				try
				{
					current = TDWay.fromWay(this.wayIndexReader.get(id), this, this.preferredLanguages);
				}
				catch (NoSuchIndexElementException)
				{
					current = this.virtualWays.get(id);
					if (current == null)
					{
						LOGGER.fine("multipolygon with outer way id " + id + " references non-existing inner way " + id);
						continue;
					}
				}

				res.Add(current);
			}

			return res;
		}
	}

}