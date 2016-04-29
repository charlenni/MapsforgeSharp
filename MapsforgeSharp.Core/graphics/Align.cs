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

    public sealed class Align
	{
		public static readonly Align CENTER = new Align("CENTER", InnerEnum.CENTER);
		public static readonly Align LEFT = new Align("LEFT", InnerEnum.LEFT);
		public static readonly Align RIGHT = new Align("RIGHT", InnerEnum.RIGHT);

		private static readonly IList<Align> valueList = new List<Align>();

		static Align()
		{
			valueList.Add(CENTER);
			valueList.Add(LEFT);
			valueList.Add(RIGHT);
		}

		public enum InnerEnum
		{
			CENTER,
			LEFT,
			RIGHT
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private Align(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public Align FromString(string value)
		{
			if ("center".Equals(value))
			{
				return CENTER;
			}
			if ("left".Equals(value))
			{
				return LEFT;
			}
			if ("right".Equals(value))
			{
				return RIGHT;
			}
			throw new System.ArgumentException("Invalid value for Align: " + value);
		}

		public static IList<Align> Values()
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

		public static Align ValueOf(string name)
		{
			foreach (Align enumInstance in Align.Values())
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