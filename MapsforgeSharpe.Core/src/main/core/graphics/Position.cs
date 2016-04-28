/*
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2014 devemux86
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

    public sealed class Position
	{
		public static readonly Position AUTO = new Position("AUTO", InnerEnum.AUTO);
		public static readonly Position CENTER = new Position("CENTER", InnerEnum.CENTER);
		public static readonly Position BELOW = new Position("BELOW", InnerEnum.BELOW);
		public static readonly Position BELOW_LEFT = new Position("BELOW_LEFT", InnerEnum.BELOW_LEFT);
		public static readonly Position BELOW_RIGHT = new Position("BELOW_RIGHT", InnerEnum.BELOW_RIGHT);
		public static readonly Position ABOVE = new Position("ABOVE", InnerEnum.ABOVE);
		public static readonly Position ABOVE_LEFT = new Position("ABOVE_LEFT", InnerEnum.ABOVE_LEFT);
		public static readonly Position ABOVE_RIGHT = new Position("ABOVE_RIGHT", InnerEnum.ABOVE_RIGHT);
		public static readonly Position LEFT = new Position("LEFT", InnerEnum.LEFT);
		public static readonly Position RIGHT = new Position("RIGHT", InnerEnum.RIGHT);

		private static readonly IList<Position> valueList = new List<Position>();

		static Position()
		{
			valueList.Add(AUTO);
			valueList.Add(CENTER);
			valueList.Add(BELOW);
			valueList.Add(BELOW_LEFT);
			valueList.Add(BELOW_RIGHT);
			valueList.Add(ABOVE);
			valueList.Add(ABOVE_LEFT);
			valueList.Add(ABOVE_RIGHT);
			valueList.Add(LEFT);
			valueList.Add(RIGHT);
		}

		public enum InnerEnum
		{
			AUTO,
			CENTER,
			BELOW,
			BELOW_LEFT,
			BELOW_RIGHT,
			ABOVE,
			ABOVE_LEFT,
			ABOVE_RIGHT,
			LEFT,
			RIGHT
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private Position(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public static Position FromString(string value)
		{
			if ("auto".Equals(value))
			{
				// deliberately returning BELOW for auto, by default
				// we are implementing auto positioning as below
				return BELOW;
			}
			else if (("center").Equals(value))
			{
				return CENTER;
			}
			else if (("below").Equals(value))
			{
				return BELOW;
			}
			else if (("below_left").Equals(value))
			{
				return BELOW_LEFT;
			}
			else if (("below_right").Equals(value))
			{
				return BELOW_RIGHT;
			}
			else if ("above".Equals(value))
			{
				return ABOVE;
			}
			else if ("above_left".Equals(value))
			{
				return ABOVE_LEFT;
			}
			else if ("above_right".Equals(value))
			{
				return ABOVE_RIGHT;
			}
			else if ("left".Equals(value))
			{
				return LEFT;
			}
			else if ("right".Equals(value))
			{
				return RIGHT;
			}
			throw new System.ArgumentException("Invalid value for Position: " + value);
		}


		public static IList<Position> Values()
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

		public static Position ValueOf(string name)
		{
			foreach (Position enumInstance in Position.Values())
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