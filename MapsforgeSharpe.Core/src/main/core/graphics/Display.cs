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
    /// The enum Display governs whether map elements should be displayed.
    /// 
    /// The main choice is
    /// between IFSPACE which means an element is displayed if there is space for it (also depends on
    /// priority), while ALWAYS means that an element will always be displayed (so it will be overlapped by
    /// others and will not be part of the element placing algorithm). NEVER is a convenience fallback, which
    /// means that an element will never be displayed.
    /// </summary>
    public sealed class Display
	{
		public static readonly Display NEVER = new Display("NEVER", InnerEnum.NEVER);
		public static readonly Display ALWAYS = new Display("ALWAYS", InnerEnum.ALWAYS);
		public static readonly Display IFSPACE = new Display("IFSPACE", InnerEnum.IFSPACE);

		private static readonly IList<Display> valueList = new List<Display>();

		static Display()
		{
			valueList.Add(NEVER);
			valueList.Add(ALWAYS);
			valueList.Add(IFSPACE);
		}

		public enum InnerEnum
		{
			NEVER,
			ALWAYS,
			IFSPACE
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private Display(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public static Display FromString(string value)
		{
			if ("never".Equals(value))
			{
				return NEVER;
			}
			else if (("always").Equals(value))
			{
				return ALWAYS;
			}
			else if (("ifspace").Equals(value))
			{
				return IFSPACE;
			}
			throw new System.ArgumentException("Invalid value for Display: " + value);
		}


		public static IList<Display> Values()
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

		public static Display ValueOf(string name)
		{
			foreach (Display enumInstance in Display.Values())
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