/*
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
namespace org.mapsforge.core.graphics
{
    using System.Collections.Generic;

    /// <summary>
    /// Specifies the shape to be used for the endpoints of a line.
    /// </summary>
    public sealed class Join
	{
		public static readonly Join BEVEL = new Join("BEVEL", InnerEnum.BEVEL);
		public static readonly Join MITER = new Join("MITER", InnerEnum.MITER);
		public static readonly Join ROUND = new Join("ROUND", InnerEnum.ROUND);

		private static readonly IList<Join> valueList = new List<Join>();

		static Join()
		{
			valueList.Add(BEVEL);
			valueList.Add(MITER);
			valueList.Add(ROUND);
		}

		public enum InnerEnum
		{
			BEVEL,
			MITER,
			ROUND
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private Join(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public static Join FromString(string value)
		{
			if ("bevel".Equals(value))
			{
				return BEVEL;
			}
			else if (("round").Equals(value))
			{
				return ROUND;
			}
			else if ("miter".Equals(value))
			{
				return MITER;
			}
			throw new System.ArgumentException("Invalid value for Join: " + value);
		}


		public static IList<Join> Values()
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

		public static Join ValueOf(string name)
		{
			foreach (Join enumInstance in Join.Values())
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