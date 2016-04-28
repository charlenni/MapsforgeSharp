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
	/// Class to store a WayDataBlock. Each WayDataBlock can store one way and a list of corresponding inner ways. Simple
	/// ways and simple polygons have zero inner ways while multi polygons have one or more inner ways.
	/// </summary>
	public class WayDataBlock
	{
		private readonly Encoding encoding;
		private readonly IList<IList<int?>> innerWays;
		private readonly IList<int?> outerWay;

		/// <summary>
		/// Creates a WayDataBlock in which way coordinates are not encoded.
		/// </summary>
		/// <param name="outerWay">
		///            the outer way of the way data block </param>
		/// <param name="innerWays">
		///            the inner ways of the way data block, or null if not existent </param>
		public WayDataBlock(IList<int?> outerWay, IList<IList<int?>> innerWays)
		{
			this.outerWay = outerWay;
			this.innerWays = innerWays;
			this.encoding = Encoding.NONE;
		}

		/// <param name="outerWay">
		///            the outer way of the way data block </param>
		/// <param name="innerWays">
		///            the inner ways of the way data block, or null if not existent </param>
		/// <param name="encoding">
		///            the encoding used to represent the coordinates </param>
		public WayDataBlock(IList<int?> outerWay, IList<IList<int?>> innerWays, Encoding encoding) : base()
		{
			this.outerWay = outerWay;
			this.innerWays = innerWays;
			this.encoding = encoding;
		}

		/// <returns> the encoding </returns>
		public virtual Encoding Encoding
		{
			get
			{
				return this.encoding;
			}
		}

		/// <returns> the innerWays </returns>
		public virtual IList<IList<int?>> InnerWays
		{
			get
			{
				return this.innerWays;
			}
		}

		/// <returns> the outerWay </returns>
		public virtual IList<int?> OuterWay
		{
			get
			{
				return this.outerWay;
			}
		}
	}

}