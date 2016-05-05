/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
 * Copyright 2016 Dirk Weltz
 * Copyright 2016 Michael Oed
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

namespace MapsforgeSharp.Core.Datastore
{
    using System.Collections.Generic;
    using System.Linq;

    using LatLong = org.mapsforge.core.model.LatLong;
    using Tag = org.mapsforge.core.model.Tag;

    /// <summary>
    /// An immutable container for all data associated with a single way or area (closed way).
    /// </summary>
    public class Way
	{
		/// <summary>
		/// The position of the area label (may be null).
		/// </summary>
		public readonly LatLong LabelPosition;

		/// <summary>
		/// The geographical coordinates of the way nodes.
		/// </summary>
		public readonly LatLong[][] LatLongs;

		/// <summary>
		/// The layer of this way + 5 (to avoid negative values).
		/// </summary>
		public readonly sbyte Layer;

		/// <summary>
		/// The tags of this way.
		/// </summary>
		public readonly IList<Tag> Tags;

		public Way(sbyte layer, IList<Tag> tags, LatLong[][] latLongs, LatLong labelPosition)
		{
			this.Layer = layer;
			this.Tags = tags;
			this.LatLongs = latLongs;
			this.LabelPosition = labelPosition;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is Way))
			{
				return false;
			}
			Way other = (Way) obj;
			if (this.Layer != other.Layer)
			{
				return false;
			}
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: else if (!this.tags.equals(other.tags))
			else if (!this.Tags.SequenceEqual(other.Tags))
			{
				return false;
			}
			else if (this.LabelPosition == null && other.LabelPosition != null)
			{
				return false;
			}
			else if (this.LabelPosition != null && !this.LabelPosition.Equals(other.LabelPosition))
			{
				return false;
			}
			else if (this.LatLongs.Length != other.LatLongs.Length)
			{
				return false;
			}
			else
			{
				for (int i = 0; i < this.LatLongs.Length; i++)
				{
					if (this.LatLongs[i].Length != other.LatLongs[i].Length)
					{
						return false;
					}
					else
					{
						for (int j = 0; j < this.LatLongs[i].Length; j++)
						{
							if (!LatLongs[i][j].Equals(other.LatLongs[i][j]))
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + Layer;
			result = prime * result + Tags.GetHashCode();
			result = prime * result + LatLongs.GetHashCode();
			if (LabelPosition != null)
			{
				result = prime * result + LabelPosition.GetHashCode();
			}
			return result;
		}
	}
}