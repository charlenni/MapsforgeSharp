/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

namespace MapsforgeSharp.Core.Graphics
{
    using System.Collections.Generic;

    public sealed class FontStyle
	{
		public static readonly FontStyle BOLD = new FontStyle("BOLD", InnerEnum.BOLD);
		public static readonly FontStyle BOLD_ITALIC = new FontStyle("BOLD_ITALIC", InnerEnum.BOLD_ITALIC);
		public static readonly FontStyle ITALIC = new FontStyle("ITALIC", InnerEnum.ITALIC);
		public static readonly FontStyle NORMAL = new FontStyle("NORMAL", InnerEnum.NORMAL);

		private static readonly IList<FontStyle> valueList = new List<FontStyle>();

		static FontStyle()
		{
			valueList.Add(BOLD);
			valueList.Add(BOLD_ITALIC);
			valueList.Add(ITALIC);
			valueList.Add(NORMAL);
		}

		public enum InnerEnum
		{
			BOLD,
			BOLD_ITALIC,
			ITALIC,
			NORMAL
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private FontStyle(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public static FontStyle FromString(string value)
		{
			if ("bold".Equals(value))
			{
				return BOLD;
			}
			else if (("bold_italic").Equals(value))
			{
				return BOLD_ITALIC;
			}
			else if ("italic".Equals(value))
			{
				return ITALIC;
			}
			else if ("normal".Equals(value))
			{
				return NORMAL;
			}
			throw new System.ArgumentException("Invalid value for FontStyle: " + value);
		}

		public static IList<FontStyle> Values()
		{
			return valueList;
		}

		public InnerEnum InnerEnumValue()
		{
			return innerEnumValue;
		}

		public int Ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static FontStyle ValueOf(string name)
		{
			foreach (FontStyle enumInstance in FontStyle.Values())
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}
}