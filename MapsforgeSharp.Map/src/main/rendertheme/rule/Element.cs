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

    internal sealed class Element
	{
		public static readonly Element ANY = new Element("ANY", InnerEnum.ANY);
		public static readonly Element NODE = new Element("NODE", InnerEnum.NODE);
		public static readonly Element WAY = new Element("WAY", InnerEnum.WAY);

		private static readonly IList<Element> valueList = new List<Element>();

		static Element()
		{
			valueList.Add(ANY);
			valueList.Add(NODE);
			valueList.Add(WAY);
		}

		public enum InnerEnum
		{
			ANY,
			NODE,
			WAY
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private Element(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		internal static Element FromString(string value)
		{
			if ("any".Equals(value))
			{
				return ANY;
			}
			if ("node".Equals(value))
			{
				return NODE;
			}
			if ("way".Equals(value))
			{
				return WAY;
			}
			throw new System.ArgumentException("Invalid value for Element: " + value);
		}

		public static IList<Element> Values()
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

		public static Element ValueOf(string name)
		{
			foreach (Element enumInstance in Element.Values())
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