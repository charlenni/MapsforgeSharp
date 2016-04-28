/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

	internal class ValueMatcher : AttributeMatcher
	{
		private readonly IList<string> values;

		internal ValueMatcher(IList<string> values)
		{
			this.values = values;
		}

		public virtual bool IsCoveredBy(AttributeMatcher attributeMatcher)
		{
			if (attributeMatcher == this)
			{
				return true;
			}

			IList<Tag> tags = new List<Tag>(this.values.Count);
			for (int i = 0, n = this.values.Count; i < n; ++i)
			{
				tags.Add(new Tag(null, this.values[i]));
			}
			return attributeMatcher.Matches(tags);
		}

		public virtual bool Matches(IList<Tag> tags)
		{
			for (int i = 0, n = tags.Count; i < n; ++i)
			{
				if (this.values.Contains(tags[i].value))
				{
					return true;
				}
			}
			return false;
		}
	}
}