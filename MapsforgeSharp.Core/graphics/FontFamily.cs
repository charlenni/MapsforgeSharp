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

    public sealed class FontFamily
	{
		public static readonly FontFamily DEFAULT = new FontFamily("DEFAULT", InnerEnum.DEFAULT);
		public static readonly FontFamily MONOSPACE = new FontFamily("MONOSPACE", InnerEnum.MONOSPACE);
		public static readonly FontFamily SANS_SERIF = new FontFamily("SANS_SERIF", InnerEnum.SANS_SERIF);
		public static readonly FontFamily SERIF = new FontFamily("SERIF", InnerEnum.SERIF);

		private static readonly IList<FontFamily> valueList = new List<FontFamily>();

		static FontFamily()
		{
			valueList.Add(DEFAULT);
			valueList.Add(MONOSPACE);
			valueList.Add(SANS_SERIF);
			valueList.Add(SERIF);
		}

		public enum InnerEnum
		{
			DEFAULT,
			MONOSPACE,
			SANS_SERIF,
			SERIF
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private FontFamily(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public static FontFamily FromString(string value)
		{
			if ("default".Equals(value))
			{
				return DEFAULT;
			}
			else if (("monospace").Equals(value))
			{
				return MONOSPACE;
			}
			else if ("sans_serif".Equals(value))
			{
				return SANS_SERIF;
			}
			else if ("serif".Equals(value))
			{
				return SERIF;
			}
			throw new System.ArgumentException("Invalid value for FontFamily: " + value);
		}

		public static IList<FontFamily> Values()
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

		public static FontFamily ValueOf(string name)
		{
			foreach (FontFamily enumInstance in FontFamily.Values())
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