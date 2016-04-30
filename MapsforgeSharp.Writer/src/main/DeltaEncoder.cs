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
namespace org.mapsforge.map.writer
{


	using Encoding = org.mapsforge.map.writer.model.Encoding;
	using WayDataBlock = org.mapsforge.map.writer.model.WayDataBlock;

	/// <summary>
	/// Provides delta or double delta encoding of lists of integers.
	/// </summary>
	public sealed class DeltaEncoder
	{
		/// <summary>
		/// Encodes a list of WayDataBlock objects with the given encoding scheme.
		/// </summary>
		/// <param name="blocks">
		///            List of WayDataBlock objects to be encoded. </param>
		/// <param name="encoding">
		///            The Encoding which is used. </param>
		/// <returns> A new list of new WayDataBlock objects encoded with the given encoding. The original list is returned in
		///         case the encoding equals NONE. </returns>
		public static IList<WayDataBlock> encode(IList<WayDataBlock> blocks, Encoding encoding)
		{
			if (blocks == null)
			{
				return null;
			}

			if (encoding == Encoding.NONE)
			{
				return blocks;
			}

			IList<WayDataBlock> results = new List<WayDataBlock>();

			foreach (WayDataBlock wayDataBlock in blocks)
			{
				IList<int?> outer = mEncode(wayDataBlock.OuterWay, encoding);
				IList<IList<int?>> inner = null;
				if (wayDataBlock.InnerWays != null)
				{
					inner = new List<>();
					foreach (IList<int?> list in wayDataBlock.InnerWays)
					{
						inner.Add(mEncode(list, encoding));
					}
				}
				results.Add(new WayDataBlock(outer, inner, encoding));
			}

			return results;
		}

		/// <summary>
		/// Computes the size in bytes for storing a list of WayDataBlock objects as unsigned var-bytes.
		/// </summary>
		/// <param name="blocks">
		///            the blocks which should be encoded </param>
		/// <returns> the number of bytes needed for the encoding </returns>
		public static int simulateSerialization(IList<WayDataBlock> blocks)
		{
			int sum = 0;
			foreach (WayDataBlock wayDataBlock in blocks)
			{
				sum += mSimulateSerialization(wayDataBlock.OuterWay);
				if (wayDataBlock.InnerWays != null)
				{
					foreach (IList<int?> list in wayDataBlock.InnerWays)
					{
						sum += mSimulateSerialization(list);
					}
				}
			}
			return sum;
		}

		internal static IList<int?> deltaEncode(IList<int?> list)
		{
			if (list == null)
			{
				return null;
			}
			List<int?> result = new List<int?>();

			if (list.Count == 0)
			{
				return result;
			}

			IEnumerator<int?> it = list.GetEnumerator();
			// add the first way node to the result list
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			int? prevLat = it.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			int? prevLon = it.next();

			result.Add(prevLat);
			result.Add(prevLon);

			while (it.MoveNext())
			{
				int? currentLat = it.Current;
				int? currentLon = it.Current;
				result.Add(Convert.ToInt32(currentLat.Value - prevLat.Value));
				result.Add(Convert.ToInt32(currentLon.Value - prevLon.Value));

				prevLat = currentLat;
				prevLon = currentLon;
			}

			return result;
		}

		internal static IList<int?> doubleDeltaEncode(IList<int?> list)
		{
			if (list == null)
			{
				return null;
			}

			List<int?> result = new List<int?>();
			if (list.Count == 0)
			{
				return result;
			}

			IEnumerator<int?> it = list.GetEnumerator();
			// add the first way node to the result list
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			int? prevLat = it.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			int? prevLon = it.next();

			int? prevLatDelta = Convert.ToInt32(0);
			int? prevLonDelta = Convert.ToInt32(0);

			result.Add(prevLat);
			result.Add(prevLon);

			while (it.MoveNext())
			{
				int? currentLat = it.Current;
				int? currentLon = it.Current;
				int? deltaLat = Convert.ToInt32(currentLat.Value - prevLat.Value);
				int? deltaLon = Convert.ToInt32(currentLon.Value - prevLon.Value);

				result.Add(Convert.ToInt32(deltaLat.Value - prevLatDelta.Value));
				result.Add(Convert.ToInt32(deltaLon.Value - prevLonDelta.Value));

				prevLat = currentLat;
				prevLon = currentLon;
				prevLatDelta = deltaLat;
				prevLonDelta = deltaLon;
			}

			return result;
		}

		private static IList<int?> mEncode(IList<int?> list, Encoding encoding)
		{
			switch (encoding)
			{
				case DELTA:
					return deltaEncode(list);
				case DOUBLE_DELTA:
					return doubleDeltaEncode(list);
				case NONE:
					return list;
			}

			throw new System.ArgumentException("unknown encoding value: " + encoding);
		}

		private static int mSimulateSerialization(IList<int?> list)
		{
			int sum = 0;
			foreach (int? coordinate in list)
			{
				sum += Serializer.getVariableByteSigned(coordinate.Value).Length;
			}
			return sum;
		}

		private DeltaEncoder()
		{
		}
	}

}