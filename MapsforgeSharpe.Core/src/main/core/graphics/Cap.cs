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

namespace org.mapsforge.core.graphics
{
    using System.Collections.Generic;

    /// <summary>
    /// Specifies the shape to be used for the endpoints of a line.
    /// </summary>
    public sealed class Cap
	{
		public static readonly Cap BUTT = new Cap("BUTT", InnerEnum.BUTT);
		public static readonly Cap ROUND = new Cap("ROUND", InnerEnum.ROUND);
		public static readonly Cap SQUARE = new Cap("SQUARE", InnerEnum.SQUARE);

		private static readonly IList<Cap> valueList = new List<Cap>();

		static Cap()
		{
			valueList.Add(BUTT);
			valueList.Add(ROUND);
			valueList.Add(SQUARE);
		}

		public enum InnerEnum
		{
			BUTT,
			ROUND,
			SQUARE
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private Cap(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public static Cap FromString(string value)
		{
			if ("butt".Equals(value))
			{
				return BUTT;
			}
			else if (("round").Equals(value))
			{
				return ROUND;
			}
			else if ("square".Equals(value))
			{
				return SQUARE;
			}
			throw new System.ArgumentException("Invalid value for Align: " + value);
		}

		public static IList<Cap> Values()
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

		public static Cap ValueOf(string name)
		{
			foreach (Cap enumInstance in Cap.Values())
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