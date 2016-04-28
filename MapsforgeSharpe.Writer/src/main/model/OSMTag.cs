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
namespace org.mapsforge.map.writer.model
{

	/// <summary>
	/// Represents an OSM entity which is defined by a tag/value pair. Each OSM entity is attributed with the zoom level on
	/// which it should appear first.
	/// </summary>
	public class OSMTag
	{
		private const string KEY_VALUE_SEPARATOR = "=";

		/// <summary>
		/// Convenience method that constructs a new OSMTag with a new id from another OSMTag.
		/// </summary>
		/// <param name="otherTag">
		///            the OSMTag to copy </param>
		/// <param name="newID">
		///            the new id </param>
		/// <returns> a newly constructed OSMTag with the attributes of otherTag </returns>
		public static OSMTag fromOSMTag(OSMTag otherTag, short newID)
		{
			return new OSMTag(newID, otherTag.Key, otherTag.Value, otherTag.ZoomAppear, otherTag.Renderable, otherTag.ForcePolygonLine);
		}

		/// <summary>
		/// Convenience method for generating a string representation of a key/value pair.
		/// </summary>
		/// <param name="key">
		///            the key of the tag </param>
		/// <param name="value">
		///            the value of the tag </param>
		/// <returns> a string representation of the key/Value pair </returns>
		public static string tagKey(string key, string value)
		{
			return key + KEY_VALUE_SEPARATOR + value;
		}

		private readonly bool forcePolygonLine;
		private readonly short id;
		private readonly string key;
		// TODO is the renderable attribute still needed?
		private readonly bool renderable;

		private readonly string value;

		private readonly sbyte zoomAppear;

		/// <param name="id">
		///            the internal id of the tag </param>
		/// <param name="key">
		///            the key of the tag </param>
		/// <param name="value">
		///            the value of the tag </param>
		/// <param name="zoomAppear">
		///            the minimum zoom level the tag appears first </param>
		/// <param name="renderable">
		///            flag if the tag represents a renderable entity </param>
		/// <param name="forcePolygonLine">
		///            flag if polygon line instead of area is forced with closed polygons </param>
		public OSMTag(short id, string key, string value, sbyte zoomAppear, bool renderable, bool forcePolygonLine) : base()
		{
			this.id = id;
			this.key = key;
			this.value = value;
			this.zoomAppear = zoomAppear;
			this.renderable = renderable;
			this.forcePolygonLine = forcePolygonLine;
		}

		public override sealed bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is OSMTag))
			{
				return false;
			}
			OSMTag other = (OSMTag) obj;
			return this.id == other.id;
		}

		/// <returns> the id </returns>
		public short Id
		{
			get
			{
				return this.id;
			}
		}

		/// <returns> the key </returns>
		public string Key
		{
			get
			{
				return this.key;
			}
		}

		/// <returns> the value </returns>
		public string Value
		{
			get
			{
				return this.value;
			}
		}

		/// <returns> the zoomAppear </returns>
		public sbyte ZoomAppear
		{
			get
			{
				return this.zoomAppear;
			}
		}

		public override sealed int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + this.id;
			return result;
		}

		/// <returns> whether the tag represents a coastline </returns>
		public bool Coastline
		{
			get
			{
				return this.key.Equals("natural") && this.value.Equals("coastline");
			}
		}

		/// <returns> the forcePolygonLine </returns>
		public bool ForcePolygonLine
		{
			get
			{
				return this.forcePolygonLine;
			}
		}

		/// <returns> the renderable </returns>
		public bool Renderable
		{
			get
			{
				return this.renderable;
			}
		}

		/// <returns> the string representation of the OSMTag </returns>
		public string tagKey()
		{
			return this.key + KEY_VALUE_SEPARATOR + this.value;
		}

		public override sealed string ToString()
		{
			return "OSMTag [id=" + this.id + ", key=" + this.key + ", value=" + this.value + ", zoomAppear=" + this.zoomAppear + ", renderable=" + this.renderable + "]";
		}
	}

}