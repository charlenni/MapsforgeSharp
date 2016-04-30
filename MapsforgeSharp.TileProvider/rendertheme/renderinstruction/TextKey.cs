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

namespace org.mapsforge.map.rendertheme.renderinstruction
{
    using System.Collections.Generic;

    using Tag = org.mapsforge.core.model.Tag;

	internal sealed class TextKey
	{
		private const string KEY_ELEVATION = "ele";
		private const string KEY_HOUSENUMBER = "addr:housenumber";
		private const string KEY_NAME = "name";
		private const string KEY_REF = "ref";
		private static readonly TextKey TEXT_KEY_ELEVATION = new TextKey(KEY_ELEVATION);
		private static readonly TextKey TEXT_KEY_HOUSENUMBER = new TextKey(KEY_HOUSENUMBER);
		private static readonly TextKey TEXT_KEY_NAME = new TextKey(KEY_NAME);
		private static readonly TextKey TEXT_KEY_REF = new TextKey(KEY_REF);

		internal static TextKey getInstance(string key)
		{
			if (KEY_ELEVATION.Equals(key))
			{
				return TEXT_KEY_ELEVATION;
			}
			else if (KEY_HOUSENUMBER.Equals(key))
			{
				return TEXT_KEY_HOUSENUMBER;
			}
			else if (KEY_NAME.Equals(key))
			{
				return TEXT_KEY_NAME;
			}
			else if (KEY_REF.Equals(key))
			{
				return TEXT_KEY_REF;
			}
			else
			{
				throw new System.ArgumentException("invalid key: " + key);
			}
		}

		private readonly string key;

		private TextKey(string key)
		{
			this.key = key;
		}

		internal string GetValue(IList<Tag> tags)
		{
			for (int i = 0, n = tags.Count; i < n; ++i)
			{
				if (this.key.Equals(tags[i].Key))
				{
					return tags[i].Value;
				}
			}
			return null;
		}
	}
}