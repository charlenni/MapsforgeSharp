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

    internal sealed class Closed
	{
		public static readonly Closed ANY = new Closed("ANY", InnerEnum.ANY);
		public static readonly Closed NO = new Closed("NO", InnerEnum.NO);
		public static readonly Closed YES = new Closed("YES", InnerEnum.YES);

		private static readonly IList<Closed> valueList = new List<Closed>();

		static Closed()
		{
			valueList.Add(ANY);
			valueList.Add(NO);
			valueList.Add(YES);
		}

		public enum InnerEnum
		{
			ANY,
			NO,
			YES
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private Closed(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public static Closed FromString(string value)
		{
			if ("any".Equals(value))
			{
				return ANY;
			}
			else if (("no").Equals(value))
			{
				return NO;
			}
			else if ("yes".Equals(value))
			{
				return YES;
			}
			throw new System.ArgumentException("Invalid value for Closed: " + value);
		}

		public static IList<Closed> Values()
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

		public static Closed ValueOf(string name)
		{
			foreach (Closed enumInstance in Closed.Values())
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