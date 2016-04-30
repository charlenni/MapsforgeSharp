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


	using OSMUtils = org.mapsforge.map.writer.util.OSMUtils;
	using EntityType = org.openstreetmap.osmosis.core.domain.v0_6.EntityType;
	using Relation = org.openstreetmap.osmosis.core.domain.v0_6.Relation;
	using RelationMember = org.openstreetmap.osmosis.core.domain.v0_6.RelationMember;

	/// <summary>
	/// Represents an OSM relation.
	/// </summary>
	public class TDRelation
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		private static readonly Logger LOGGER = Logger.getLogger(typeof(TDRelation).FullName);

		/// <summary>
		/// Creates a new TDRelation from an osmosis entity using the given WayResolver.
		/// </summary>
		/// <param name="relation">
		///            the relation </param>
		/// <param name="resolver">
		///            the resolver </param>
		/// <param name="preferredLanguages">
		///            the preferred(s) language or null if no preference </param>
		/// <returns> a new TDRelation if all members are valid and the relation is of a known type, null otherwise </returns>
		public static TDRelation fromRelation(Relation relation, WayResolver resolver, IList<string> preferredLanguages)
		{
			if (relation == null)
			{
				return null;
			}

			if (relation.Members.Empty)
			{
				return null;
			}

			SpecialTagExtractionResult ster = OSMUtils.extractSpecialFields(relation, preferredLanguages);
			short[] knownWayTags = OSMUtils.extractKnownWayTags(relation);

			// special tags
			// TODO what about the layer of relations?

			// TODO exclude boundaries

			if (!knownRelationType(ster.Type))
			{
				return null;
			}

			IList<RelationMember> members = relation.Members;
			IList<TDWay> wayMembers = new List<TDWay>();
			foreach (RelationMember relationMember in members)
			{
				if (relationMember.MemberType != EntityType.Way)
				{
					continue;
				}
				TDWay member = resolver.getWay(relationMember.MemberId);
				if (member == null)
				{
					LOGGER.finest("relation is missing a member, rel-id: " + relation.Id + " member id: " + relationMember.MemberId);
					continue;
				}
				wayMembers.Add(member);
			}

			if (wayMembers.Count == 0)
			{
				LOGGER.finest("relation has no valid members: " + relation.Id);
				return null;
			}

			return new TDRelation(relation.Id, ster.Layer, ster.Name, ster.Housenumber, ster.Ref, knownWayTags, wayMembers.ToArray());
		}

		/// <param name="type">
		///            the type attribute of a relation </param>
		/// <returns> true if the type if known, currently only multipolygons are known </returns>
		// TODO adjust if more relations should be supported
		public static bool knownRelationType(string type)
		{
			return !string.ReferenceEquals(type, null) && "multipolygon".Equals(type);
		}

		private readonly string houseNumber;
		private readonly long id;
		private readonly sbyte layer;
		private readonly TDWay[] memberWays;
		private readonly string name;

		private readonly string @ref;

		private readonly short[] tags;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id">
		///            the id </param>
		/// <param name="layer">
		///            the layer </param>
		/// <param name="name">
		///            the name </param>
		/// <param name="houseNumber">
		///            the house number if existent </param>
		/// <param name="ref">
		///            the ref attribute </param>
		/// <param name="tags">
		///            the tags </param>
		/// <param name="memberWays">
		///            the member ways </param>
		internal TDRelation(long id, sbyte layer, string name, string houseNumber, string @ref, short[] tags, TDWay[] memberWays)
		{
			this.id = id;
			this.layer = layer;
			this.name = name;
			this.houseNumber = houseNumber;
			this.@ref = @ref;
			this.tags = tags;
			this.memberWays = memberWays;
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
			TDRelation other = (TDRelation) obj;
			if (this.id != other.id)
			{
				return false;
			}
			return true;
		}

		/// <returns> the houseNumber </returns>
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

		/// <returns> the member ways </returns>
		public virtual TDWay[] MemberWays
		{
			get
			{
				return this.memberWays;
			}
		}

		/// <returns> the name </returns>
		public virtual string Name
		{
			get
			{
				return this.name;
			}
		}

		/// <returns> the ref </returns>
		public virtual string Ref
		{
			get
			{
				return this.@ref;
			}
		}

		/// <returns> the tags </returns>
		public virtual short[] Tags
		{
			get
			{
				return this.tags;
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + (int)(this.id ^ ((long)((ulong)this.id >> 32)));
			return result;
		}

		/// <returns> true if the relation has associated tags </returns>
		public virtual bool hasTags()
		{
			return this.tags != null && this.tags.Length > 0;
		}

		/// <returns> true if the relation represents a coastline </returns>
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

		/// <returns> true if the relation is relevant for rendering </returns>
		public virtual bool RenderRelevant
		{
			get
			{
				return hasTags() || !string.ReferenceEquals(Name, null) && Name.Length > 0 || !string.ReferenceEquals(Ref, null) && Ref.Length > 0;
			}
		}

		public override string ToString()
		{
			return "TDRelation [id=" + this.id + ", layer=" + this.layer + ", name=" + this.name + ", ref=" + this.@ref + ", tags=" + Arrays.ToString(this.tags) + "]";
		}
	}

}