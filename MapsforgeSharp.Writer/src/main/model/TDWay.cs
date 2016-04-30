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
namespace org.mapsforge.map.writer.model
{

	using TShortSet = gnu.trove.set.TShortSet;
	using TShortHashSet = gnu.trove.set.hash.TShortHashSet;


	using GeoUtils = org.mapsforge.map.writer.util.GeoUtils;
	using OSMUtils = org.mapsforge.map.writer.util.OSMUtils;
	using Way = org.openstreetmap.osmosis.core.domain.v0_6.Way;
	using WayNode = org.openstreetmap.osmosis.core.domain.v0_6.WayNode;

	/// <summary>
	/// Represents an OSM way.
	/// </summary>
	public class TDWay
	{
		// TODO these constants are not necessary anymore
		/// <summary>
		/// Represents a line.
		/// </summary>
		public const sbyte LINE = 0x0;

		/// <summary>
		/// A simple closed polygon with holes.
		/// </summary>
		public const sbyte MULTI_POLYGON = 0x2;
		/// <summary>
		/// A simple closed polygon.
		/// </summary>
		public const sbyte SIMPLE_POLYGON = 0x1;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		private static readonly Logger LOGGER = Logger.getLogger(typeof(TDWay).FullName);

		/// <summary>
		/// Creates a new TDWay from an osmosis way entity using the given NodeResolver.
		/// </summary>
		/// <param name="way">
		///            the way </param>
		/// <param name="resolver">
		///            the resolver </param>
		/// <param name="preferredLanguages">
		///            the preferred language(s) or null if no preference </param>
		/// <returns> a new TDWay if it is valid, null otherwise </returns>
		public static TDWay fromWay(Way way, NodeResolver resolver, IList<string> preferredLanguages)
		{
			if (way == null)
			{
				return null;
			}

			SpecialTagExtractionResult ster = OSMUtils.extractSpecialFields(way, preferredLanguages);
			short[] knownWayTags = OSMUtils.extractKnownWayTags(way);

			// only ways with at least 2 way nodes are valid ways
			if (way.WayNodes.size() >= 2)
			{
				bool validWay = true;
				// retrieve way nodes from data store
				TDNode[] waynodes = new TDNode[way.WayNodes.size()];
				int i = 0;
				foreach (WayNode waynode in way.WayNodes)
				{
					// TODO adjust interface to support a method getWayNodes()
					waynodes[i] = resolver.getNode(waynode.NodeId);
					if (waynodes[i] == null)
					{
						validWay = false;
						LOGGER.finer("unknown way node: " + waynode.NodeId + " in way " + way.Id);
					}
					i++;
				}

				// for a valid way all way nodes must be existent in the input data
				if (validWay)
				{
					// mark the way as polygon if the first and the last way node are the same
					// and if the way has at least 4 way nodes
					sbyte shape = LINE;
					if (waynodes[0].Id == waynodes[waynodes.Length - 1].Id)
					{
						if (waynodes.Length >= GeoUtils.MIN_NODES_POLYGON)
						{
							if (OSMUtils.isArea(way))
							{
								shape = SIMPLE_POLYGON;
							}
						}
						else
						{
							LOGGER.finer("Found closed polygon with fewer than 4 way nodes. Way-id: " + way.Id);
							return null;
						}
					}

					return new TDWay(way.Id, ster.Layer, ster.Name, ster.Housenumber, ster.Ref, knownWayTags, shape, waynodes);
				}
			}

			return null;
		}

		private readonly string houseNumber;
		private readonly long id;
		private bool invalid;
		private readonly sbyte layer;
		private string name;
		private string @ref;
		private bool reversedInRelation;
		private sbyte shape;
		private short[] tags;

		private readonly TDNode[] wayNodes;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id">
		///            the id </param>
		/// <param name="layer">
		///            the layer </param>
		/// <param name="name">
		///            the name if existent </param>
		/// <param name="houseNumber">
		///            the house number if existent </param>
		/// <param name="ref">
		///            the ref if existent </param>
		/// <param name="tags">
		///            the tags </param>
		/// <param name="shape">
		///            the shape </param>
		/// <param name="wayNodes">
		///            the way nodes </param>
		public TDWay(long id, sbyte layer, string name, string houseNumber, string @ref, short[] tags, sbyte shape, TDNode[] wayNodes)
		{
			this.id = id;
			this.layer = layer;
			this.name = name;
			this.houseNumber = houseNumber;
			this.@ref = @ref;
			this.tags = tags;
			this.shape = shape;
			this.wayNodes = wayNodes;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id">
		///            the id </param>
		/// <param name="layer">
		///            the layer </param>
		/// <param name="name">
		///            the name if existent </param>
		/// <param name="houseNumber">
		///            the house number if existent </param>
		/// <param name="ref">
		///            the ref if existent </param>
		/// <param name="wayNodes">
		///            the way nodes </param>
		public TDWay(long id, sbyte layer, string name, string houseNumber, string @ref, TDNode[] wayNodes)
		{
			this.id = id;
			this.layer = layer;
			this.name = name;
			this.houseNumber = houseNumber;
			this.@ref = @ref;
			this.wayNodes = wayNodes;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj == null)
			{
				return false;
			}
			if (this.GetType() != obj.GetType())
			{
				return false;
			}
			TDWay other = (TDWay) obj;
			if (this.id != other.id)
			{
				return false;
			}
			return true;
		}

		/// <returns> the house number </returns>
		public virtual string HouseNumber
		{
			get
			{
				return this.houseNumber;
			}
		}

		/// <returns> the id </returns>
		public virtual long Id
		{
			get
			{
				return this.id;
			}
		}

		/// <returns> the layer </returns>
		public virtual sbyte Layer
		{
			get
			{
				return this.layer;
			}
		}

		/// <returns> the zoom level this entity appears first </returns>
		public virtual sbyte MinimumZoomLevel
		{
			get
			{
				return OSMTagMapping.Instance.getZoomAppearWay(this.tags);
			}
		}

		/// <returns> the name </returns>
		public virtual string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		/// <returns> the ref </returns>
		public virtual string Ref
		{
			get
			{
				return this.@ref;
			}
			set
			{
				this.@ref = value;
			}
		}

		/// <returns> the shape </returns>
		public virtual sbyte Shape
		{
			get
			{
				return this.shape;
			}
			set
			{
				this.shape = value;
			}
		}

		/// <returns> the tags </returns>
		public virtual short[] Tags
		{
			get
			{
				return this.tags;
			}
			set
			{
				this.tags = value;
			}
		}

		/// <returns> the way nodes </returns>
		public virtual TDNode[] WayNodes
		{
			get
			{
				return this.wayNodes;
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + (int)(this.id ^ ((long)((ulong)this.id >> 32)));
			return result;
		}

		/// <returns> true, if the way has tags </returns>
		public virtual bool hasTags()
		{
			return this.tags != null && this.tags.Length > 0;
		}

		/// <returns> true, if the way represents a coastline </returns>
		public virtual bool Coastline
		{
			get
			{
				if (this.tags == null)
				{
					return false;
				}
				OSMTag tag;
				foreach (short tagID in this.tags)
				{
					tag = OSMTagMapping.Instance.getWayTag(tagID);
					if (tag.Coastline)
					{
						return true;
					}
				}
    
				return false;
			}
		}

		/// <returns> true, if the way has a tag that forces a closed way to be a polygon line (instead of an area) </returns>
		public virtual bool ForcePolygonLine
		{
			get
			{
				if (!hasTags())
				{
					return false;
				}
				OSMTagMapping mapping = OSMTagMapping.Instance;
				foreach (short tag in this.tags)
				{
					if (mapping.getWayTag(tag).ForcePolygonLine)
					{
						return true;
					}
				}
				return false;
			}
		}

		/// <returns> the invalid </returns>
		public virtual bool Invalid
		{
			get
			{
				return this.invalid;
			}
			set
			{
				this.invalid = value;
			}
		}

		/// <returns> true, if the way is relevant for rendering </returns>
		public virtual bool RenderRelevant
		{
			get
			{
				return hasTags() || !string.ReferenceEquals(Name, null) && Name.Length > 0 || !string.ReferenceEquals(Ref, null) && Ref.Length > 0;
			}
		}

		/// <returns> true, if the way nodes have been reversed with respect to a particular relation </returns>
		public virtual bool ReversedInRelation
		{
			get
			{
				return this.reversedInRelation;
			}
			set
			{
				this.reversedInRelation = value;
			}
		}

		/// <returns> true, if the way has at least 4 coordinates and the first and last coordinate are equal </returns>
		public virtual bool ValidClosedLine
		{
			get
			{
				return this.wayNodes != null && this.wayNodes.Length >= GeoUtils.MIN_NODES_POLYGON && this.wayNodes[0].Id == this.wayNodes[this.wayNodes.Length - 1].Id;
			}
		}

		/// <summary>
		/// Merges tags from a relation with the tags of this way and puts the result into the way tags of this way.
		/// </summary>
		/// <param name="relation">
		///            the relation </param>
		public virtual void mergeRelationInformation(TDRelation relation)
		{
			if (relation.hasTags())
			{
				addTags(relation.Tags);
			}
			if (string.ReferenceEquals(Name, null) && relation.Name != null)
			{
				Name = relation.Name;
			}
			if (string.ReferenceEquals(Ref, null) && relation.Ref != null)
			{
				Ref = relation.Ref;
			}
		}







		public override string ToString()
		{
			return "TDWay [id=" + this.id + ", name=" + this.name + ", tags=" + Arrays.ToString(this.tags) + ", polygon=" + this.shape + "]";
		}

		private void addTags(short[] addendum)
		{
			if (this.tags == null)
			{
				this.tags = addendum;
			}
			else
			{
				TShortSet tags2 = new TShortHashSet();
				tags2.addAll(this.tags);
				tags2.addAll(addendum);
				this.tags = tags2.toArray();
			}
		}
	}

}