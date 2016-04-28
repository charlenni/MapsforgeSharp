using System;
using System.Text;
using System.Runtime.Serialization;

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

namespace org.mapsforge.core.model
{

	/// <summary>
	/// A tag represents an immutable key-value pair.
	/// </summary>
	[DataContract]
	public class Tag
	{
		private const char KEY_VALUE_SEPARATOR = '=';
		private const long serialVersionUID = 1L;

		/// <summary>
		/// The key of this tag.
		/// </summary>
		public readonly string key;

		/// <summary>
		/// The value of this tag.
		/// </summary>
		public readonly string value;

		/// <param name="tag">
		///            the textual representation of the tag. </param>
		public Tag(string tag) : this(tag, tag.IndexOf(KEY_VALUE_SEPARATOR))
		{
		}

		/// <param name="key">
		///            the key of the tag. </param>
		/// <param name="value">
		///            the value of the tag. </param>
		public Tag(string key, string value)
		{
			this.key = key;
			this.value = value;
		}

		private Tag(string tag, int splitPosition) : this(tag.Substring(0, splitPosition), tag.Substring(splitPosition + 1))
		{
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is Tag))
			{
				return false;
			}
			Tag other = (Tag) obj;
			if (string.ReferenceEquals(this.key, null))
			{
				if (!string.ReferenceEquals(other.key, null))
				{
					return false;
				}
			}
			else if (!this.key.Equals(other.key))
			{
				return false;
			}
			else if (string.ReferenceEquals(this.value, null))
			{
				if (!string.ReferenceEquals(other.value, null))
				{
					return false;
				}
			}
			else if (!this.value.Equals(other.value))
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((string.ReferenceEquals(this.key, null)) ? 0 : this.key.GetHashCode());
			result = prime * result + ((string.ReferenceEquals(this.value, null)) ? 0 : this.value.GetHashCode());
			return result;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("key=");
			stringBuilder.Append(this.key);
			stringBuilder.Append(", value=");
			stringBuilder.Append(this.value);
			return stringBuilder.ToString();
		}
	}
}