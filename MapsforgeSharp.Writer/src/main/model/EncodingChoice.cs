using System;
using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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
namespace org.mapsforge.map.writer.model
{

	/// <summary>
	/// Represents preferred encoding.
	/// </summary>
	public sealed class EncodingChoice
	{
		/// <summary>
		/// AUTO.
		/// </summary>
		public static readonly EncodingChoice AUTO = new EncodingChoice("AUTO", InnerEnum.AUTO);
		/// <summary>
		/// DOUBLE.
		/// </summary>
		public static readonly EncodingChoice DOUBLE = new EncodingChoice("DOUBLE", InnerEnum.DOUBLE);
		/// <summary>
		/// SINGLE.
		/// </summary>
		public static readonly EncodingChoice SINGLE = new EncodingChoice("SINGLE", InnerEnum.SINGLE);

		private static readonly IList<EncodingChoice> valueList = new List<EncodingChoice>();

		static EncodingChoice()
		{
			valueList.Add(AUTO);
			valueList.Add(DOUBLE);
			valueList.Add(SINGLE);
		}

		public enum InnerEnum
		{
			AUTO,
			DOUBLE,
			SINGLE
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private EncodingChoice(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		/// <summary>
		/// Reads preferred encoding from a String.
		/// </summary>
		/// <param name="encoding">
		///            the encoding </param>
		/// <returns> the preferred encoding, AUTO if preferred encoding unknown </returns>
		public static EncodingChoice fromString(string encoding)
		{
			if ("auto".Equals(encoding, StringComparison.CurrentCultureIgnoreCase))
			{
				return AUTO;
			}

			if ("single".Equals(encoding, StringComparison.CurrentCultureIgnoreCase))
			{
				return SINGLE;
			}

			if ("double".Equals(encoding, StringComparison.CurrentCultureIgnoreCase))
			{
				return DOUBLE;
			}

			return AUTO;
		}

		public static IList<EncodingChoice> values()
		{
			return valueList;
		}

		public InnerEnum InnerEnumValue()
		{
			return innerEnumValue;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static EncodingChoice valueOf(string name)
		{
			foreach (EncodingChoice enumInstance in EncodingChoice.values())
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