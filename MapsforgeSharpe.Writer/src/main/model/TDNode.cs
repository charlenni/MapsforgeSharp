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


	using LatLongUtils = org.mapsforge.core.util.LatLongUtils;
	using OSMUtils = org.mapsforge.map.writer.util.OSMUtils;
	using Node = org.openstreetmap.osmosis.core.domain.v0_6.Node;

	public class TDNode
	{

		private static readonly sbyte ZOOM_HOUSENUMBER = (sbyte) 18;

		// private static final byte ZOOM_NAME = (byte) 16;

		/// <summary>
		/// Constructs a new TDNode from a given osmosis node entity. Checks the validity of the entity.
		/// </summary>
		/// <param name="node">
		///            the osmosis entity </param>
		/// <param name="preferredLanguages">
		///            the preferred language(s) or null if no preference </param>
		/// <returns> a new TDNode </returns>
		public static TDNode fromNode(Node node, IList<string> preferredLanguages)
		{
			SpecialTagExtractionResult ster = OSMUtils.extractSpecialFields(node, preferredLanguages);
			short[] knownWayTags = OSMUtils.extractKnownPOITags(node);

			return new TDNode(node.Id, LatLongUtils.degreesToMicrodegrees(node.Latitude), LatLongUtils.degreesToMicrodegrees(node.Longitude), ster.Elevation, ster.Layer, ster.Housenumber, ster.Name, knownWayTags);
		}

		private readonly short elevation;
		private readonly string houseNumber;

		private readonly long id;
		private readonly int latitude;
		private readonly sbyte layer;
		private readonly int longitude;
		private readonly string name;

		private short[] tags;

		/// <param name="id">
		///            the OSM id </param>
		/// <param name="latitude">
		///            the latitude </param>
		/// <param name="longitude">
		///            the longitude </param>
		/// <param name="elevation">
		///            the elevation if existent </param>
		/// <param name="layer">
		///            the layer if existent </param>
		/// <param name="houseNumber">
		///            the house number if existent </param>
		/// <param name="name">
		///            the name if existent </param>
		public TDNode(long id, int latitude, int longitude, short elevation, sbyte layer, string houseNumber, string name)
		{
			this.id = id;
			this.latitude = latitude;
			this.longitude = longitude;
			this.elevation = elevation;
			this.houseNumber = houseNumber;
			this.layer = layer;
			this.name = name;
		}

		/// <param name="id">
		///            the OSM id </param>
		/// <param name="latitude">
		///            the latitude </param>
		/// <param name="longitude">
		///            the longitude </param>
		/// <param name="elevation">
		///            the elevation if existent </param>
		/// <param name="layer">
		///            the layer if existent </param>
		/// <param name="houseNumber">
		///            the house number if existent </param>
		/// <param name="name">
		///            the name if existent </param>
		/// <param name="tags">
		///            the </param>
		public TDNode(long id, int latitude, int longitude, short elevation, sbyte layer, string houseNumber, string name, short[] tags)
		{
			this.id = id;
			this.latitude = latitude;
			this.longitude = longitude;
			this.elevation = elevation;
			this.houseNumber = houseNumber;
			this.layer = layer;
			this.name = name;
			this.tags = tags;
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
			TDNode other = (TDNode) obj;
			if (this.id != other.id)
			{
				return false;
			}
			return true;
		}

		/// <returns> the elevation </returns>
		public virtual short Elevation
		{
			get
			{
				return this.elevation;
			}
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

		/// <returns> the latitude </returns>
		public virtual int Latitude
		{
			get
			{
				return this.latitude;
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

		/// <returns> the longitude </returns>
		public virtual int Longitude
		{
			get
			{
				return this.longitude;
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

		/// <returns> the zoom level on which the node appears first </returns>
		public virtual sbyte ZoomAppear
		{
			get
			{
				if (this.tags == null || this.tags.Length == 0)
				{
					if (!string.ReferenceEquals(this.houseNumber, null))
					{
						return ZOOM_HOUSENUMBER;
					}
					return sbyte.MaxValue;
				}
				return OSMTagMapping.Instance.getZoomAppearPOI(this.tags);
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + (int)(this.id ^ ((long)((ulong)this.id >> 32)));
			return result;
		}

		/// <returns> true if the node represents a POI </returns>
		public virtual bool POI
		{
			get
			{
				return !string.ReferenceEquals(this.houseNumber, null) || this.elevation != 0 || this.tags.Length > 0;
			}
		}


		public override sealed string ToString()
		{
			return "TDNode [id=" + this.id + ", latitude=" + this.latitude + ", longitude=" + this.longitude + ", name=" + this.name + ", tags=" + Arrays.ToString(this.tags) + "]";
		}
	}

}