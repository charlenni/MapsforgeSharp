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
	/// Represents the configuration of zoom intervals. A zoom interval is defined by a base zoom level, a minimum zoom level
	/// and a maximum zoom level.
	/// </summary>
	public sealed class ZoomIntervalConfiguration
	{
		/// <summary>
		/// Create a new ZoomIntervalConfiguration from the given string representation. Checks for validity.
		/// </summary>
		/// <param name="confString">
		///            the string representation of a zoom interval configuration </param>
		/// <returns> a new zoom interval configuration </returns>
		public static ZoomIntervalConfiguration fromString(string confString)
		{
			string[] splitted = confString.Split(",", true);
			if (splitted.Length % 3 != 0)
			{
				throw new System.ArgumentException("invalid zoom interval configuration, amount of comma-separated values must be a multiple of 3");
			}
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: sbyte[][] intervals = new sbyte[splitted.Length / 3][3];
			sbyte[][] intervals = RectangularArrays.ReturnRectangularSbyteArray(splitted.Length / 3, 3);
			for (int i = 0; i < intervals.Length; i++)
			{
				intervals[i][0] = sbyte.Parse(splitted[i * 3]);
				intervals[i][1] = sbyte.Parse(splitted[i * 3 + 1]);
				intervals[i][2] = sbyte.Parse(splitted[i * 3 + 2]);
			}

			return ZoomIntervalConfiguration.newInstance(intervals);
		}

		/// <returns> the standard configuration </returns>
		public static ZoomIntervalConfiguration StandardConfiguration
		{
			get
			{
				return new ZoomIntervalConfiguration(new sbyte[][]
				{
					new sbyte[] {5, 0, 7},
					new sbyte[] {10, 8, 11},
					new sbyte[] {14, 12, 21}
				});
			}
		}

		/// <summary>
		/// Create a new ZoomIntervalConfiguration from the given byte array. Checks for validity.
		/// </summary>
		/// <param name="intervals">
		///            the intervals </param>
		/// <returns> a zoom interval configuration that represents the given intervals </returns>
		public static ZoomIntervalConfiguration newInstance(params sbyte[][] intervals)
		{
			return new ZoomIntervalConfiguration(intervals);
		}

		private readonly sbyte[] baseZoom;
		private readonly sbyte maxMaxZoom;

		private readonly sbyte[] maxZoom;

		private readonly sbyte minMinZoom;

		private readonly sbyte[] minZoom;

		private ZoomIntervalConfiguration(sbyte[][] intervals)
		{
			this.baseZoom = new sbyte[intervals.Length];
			this.minZoom = new sbyte[intervals.Length];
			this.maxZoom = new sbyte[intervals.Length];

			int i = 0;
			foreach (sbyte[] interval in intervals)
			{
				i++;
				if (interval.Length != 3)
				{
					throw new System.ArgumentException("invalid interval configuration, found only " + interval.Length + "parameters for interval " + i);
				}
				if (interval[0] < interval[1] || interval[0] > interval[2])
				{
					throw new System.ArgumentException("invalid configuration for interval " + i + ", make sure that minZoom < baseZoom < maxZoom");
				}
				if (i > 1)
				{
					if (interval[0] < this.baseZoom[i - 2])
					{
						throw new System.ArgumentException("interval configurations must follow an increasing order");
					}
					if (interval[1] != ((this.maxZoom[i - 2]) + 1))
					{
						throw new System.ArgumentException("minZoom of interval " + i + " not adjacent to maxZoom of interval " + (i - 1));
					}
				}
				this.baseZoom[i - 1] = interval[0];
				this.minZoom[i - 1] = interval[1];
				this.maxZoom[i - 1] = interval[2];
			}
			this.minMinZoom = this.minZoom[0];
			this.maxMaxZoom = this.maxZoom[this.maxZoom.Length - 1];
		}

		/// <param name="index">
		///            the zoom interval index </param>
		/// <returns> the corresponding base zoom level </returns>
		public sbyte getBaseZoom(int index)
		{
			checkValidIndex(index);
			return this.baseZoom[index];
		}

		/// <returns> the maxMaxZoom </returns>
		public sbyte MaxMaxZoom
		{
			get
			{
				return this.maxMaxZoom;
			}
		}

		/// <param name="index">
		///            the index </param>
		/// <returns> the corresponding max zoom level </returns>
		public sbyte getMaxZoom(int index)
		{
			checkValidIndex(index);
			return this.maxZoom[index];
		}

		/// <returns> the minMinZoom </returns>
		public sbyte MinMinZoom
		{
			get
			{
				return this.minMinZoom;
			}
		}

		/// <param name="index">
		///            the zoom interval index </param>
		/// <returns> the minimum zoom level for this index </returns>
		public sbyte getMinZoom(int index)
		{
			checkValidIndex(index);
			return this.minZoom[index];
		}

		/// <returns> the number of zoom intervals </returns>
		public int NumberOfZoomIntervals
		{
			get
			{
				if (this.baseZoom == null)
				{
					return 0;
				}
				return this.baseZoom.Length;
			}
		}

		private void checkValidIndex(int index)
		{
			if (index < 0 || index >= this.baseZoom.Length)
			{
				throw new System.ArgumentException("illegal zoom interval index: " + index);
			}
		}
	}

}