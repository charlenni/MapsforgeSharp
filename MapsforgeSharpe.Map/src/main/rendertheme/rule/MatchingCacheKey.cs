/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2016 Dirk Weltz
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

namespace org.mapsforge.map.rendertheme.rule
{
    using System.Collections.Generic;

    using Tag = org.mapsforge.core.model.Tag;

	internal class MatchingCacheKey
	{
		private readonly Closed closed;
		private readonly IList<Tag> tags;
		private readonly ISet<Tag> tagsWithoutName;
		private readonly sbyte zoomLevel;

		internal MatchingCacheKey(IList<Tag> tags, sbyte zoomLevel, Closed closed)
		{
			this.tags = tags;
			this.zoomLevel = zoomLevel;
			this.closed = closed;
			this.tagsWithoutName = new HashSet<Tag>();
			if (this.tags != null)
			{
				foreach (Tag tag in tags)
				{
					if (!"name".Equals(tag.key))
					{
						this.tagsWithoutName.Add(tag);
					}
				}
			}
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is MatchingCacheKey))
			{
				return false;
			}
			MatchingCacheKey other = (MatchingCacheKey) obj;
			if (this.closed != other.closed)
			{
				return false;
			}
			if (this.tagsWithoutName == null && other.tagsWithoutName != null)
			{
				return false;
			}
			else if (!this.tagsWithoutName.Equals(other.tagsWithoutName))
			{
				return false;
			}
			if (this.zoomLevel != other.zoomLevel)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((this.closed == null) ? 0 : this.closed.GetHashCode());
			result = prime * result + this.tagsWithoutName.GetHashCode();
			result = prime * result + this.zoomLevel;
			return result;
		}
	}
}