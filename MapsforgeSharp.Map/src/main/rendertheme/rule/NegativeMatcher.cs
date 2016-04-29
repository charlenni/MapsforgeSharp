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

	internal class NegativeMatcher : AttributeMatcher
	{
		private readonly IList<string> keyList;
		private readonly IList<string> valueList;

		internal NegativeMatcher(IList<string> keyList, IList<string> valueList)
		{
			this.keyList = keyList;
			this.valueList = valueList;
		}

		public virtual bool IsCoveredBy(AttributeMatcher attributeMatcher)
		{
			return false;
		}

		public virtual bool Matches(IList<Tag> tags)
		{
			if (KeyListDoesNotContainKeys(tags))
			{
				return true;
			}

			for (int i = 0, n = tags.Count; i < n; ++i)
			{
				if (this.valueList.Contains(tags[i].Value))
				{
					return true;
				}
			}
			return false;
		}

		private bool KeyListDoesNotContainKeys(IList<Tag> tags)
		{
			for (int i = 0, n = tags.Count; i < n; ++i)
			{
				if (this.keyList.Contains(tags[i].Key))
				{
					return false;
				}
			}
			return true;
		}
	}
}