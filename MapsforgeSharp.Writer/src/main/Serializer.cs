using System;

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

	/// <summary>
	/// This class converts numbers to byte arrays.
	/// </summary>
	public sealed class Serializer
	{
		/// <summary>
		/// Converts a signed int to a byte array.
		/// <para>
		/// The byte order is big-endian.
		/// 
		/// </para>
		/// </summary>
		/// <param name="value">
		///            the int value. </param>
		/// <returns> an array with four bytes. </returns>
		public static sbyte[] getBytes(int value)
		{
			return new sbyte[] {(sbyte)(value >> 24), (sbyte)(value >> 16), (sbyte)(value >> 8), (sbyte) value};
		}

		/// <summary>
		/// Converts a signed long to a byte array.
		/// <para>
		/// The byte order is big-endian.
		/// 
		/// </para>
		/// </summary>
		/// <param name="value">
		///            the long value. </param>
		/// <returns> an array with eight bytes. </returns>
		public static sbyte[] getBytes(long value)
		{
			return new sbyte[] {(sbyte)(value >> 56), (sbyte)(value >> 48), (sbyte)(value >> 40), (sbyte)(value >> 32), (sbyte)(value >> 24), (sbyte)(value >> 16), (sbyte)(value >> 8), (sbyte) value};
		}

		/// <summary>
		/// Converts a signed short to a byte array.
		/// <para>
		/// The byte order is big-endian.
		/// 
		/// </para>
		/// </summary>
		/// <param name="value">
		///            the short value. </param>
		/// <returns> an array with two bytes. </returns>
		public static sbyte[] getBytes(short value)
		{
			return new sbyte[] {(sbyte)(value >> 8), (sbyte) value};
		}

		/// <summary>
		/// Converts the lowest five bytes of an unsigned long to a byte array.
		/// <para>
		/// The byte order is big-endian.
		/// 
		/// </para>
		/// </summary>
		/// <param name="value">
		///            the long value, must not be negative. </param>
		/// <returns> an array with five bytes. </returns>
		public static sbyte[] getFiveBytes(long value)
		{
			if (value < 0)
			{
				throw new System.ArgumentException("negative value not allowed: " + value);
			}
			else if (value > 1099511627775L)
			{
				throw new System.ArgumentException("value out of range: " + value);
			}
			return new sbyte[] {(sbyte)(value >> 32), (sbyte)(value >> 24), (sbyte)(value >> 16), (sbyte)(value >> 8), (sbyte) value};
		}

		/// <summary>
		/// Converts a signed int to a variable length byte array.
		/// <para>
		/// The first bit is for continuation info, the other six (last byte) or seven (all other bytes) bits for data. The
		/// second bit in the last byte indicates the sign of the number.
		/// 
		/// </para>
		/// </summary>
		/// <param name="value">
		///            the int value. </param>
		/// <returns> an array with 1-5 bytes. </returns>
		public static sbyte[] getVariableByteSigned(int value)
		{
			long absValue = Math.Abs((long) value);
			if (absValue < 64)
			{ // 2^6
				// encode the number in a single byte
				if (value < 0)
				{
					return new sbyte[] {(sbyte)(absValue | 0x40)};
				}
				return new sbyte[] {(sbyte) absValue};
			}
			else if (absValue < 8192)
			{ // 2^13
				// encode the number in two bytes
				if (value < 0)
				{
					return new sbyte[] {unchecked((sbyte)(absValue | 0x80)), (sbyte)((absValue >> 7) | 0x40)};
				}
				return new sbyte[] {unchecked((sbyte)(absValue | 0x80)), (sbyte)(absValue >> 7)};
			}
			else if (absValue < 1048576)
			{ // 2^20
				// encode the number in three bytes
				if (value < 0)
				{
					return new sbyte[] {unchecked((sbyte)(absValue | 0x80)), unchecked((sbyte)((absValue >> 7) | 0x80)), (sbyte)((absValue >> 14) | 0x40)};
				}
				return new sbyte[] {unchecked((sbyte)(absValue | 0x80)), unchecked((sbyte)((absValue >> 7) | 0x80)), (sbyte)(absValue >> 14)};
			}
			else if (absValue < 134217728)
			{ // 2^27
				// encode the number in four bytes
				if (value < 0)
				{
					return new sbyte[] {unchecked((sbyte)(absValue | 0x80)), unchecked((sbyte)((absValue >> 7) | 0x80)), unchecked((sbyte)((absValue >> 14) | 0x80)), (sbyte)((absValue >> 21) | 0x40)};
				}
				return new sbyte[] {unchecked((sbyte)(absValue | 0x80)), unchecked((sbyte)((absValue >> 7) | 0x80)), unchecked((sbyte)((absValue >> 14) | 0x80)), (sbyte)(absValue >> 21)};
			}
			else
			{
				// encode the number in five bytes
				if (value < 0)
				{
					return new sbyte[] {unchecked((sbyte)(absValue | 0x80)), unchecked((sbyte)((absValue >> 7) | 0x80)), unchecked((sbyte)((absValue >> 14) | 0x80)), unchecked((sbyte)((absValue >> 21) | 0x80)), (sbyte)((absValue >> 28) | 0x40)};
				}
				return new sbyte[] {unchecked((sbyte)(absValue | 0x80)), unchecked((sbyte)((absValue >> 7) | 0x80)), unchecked((sbyte)((absValue >> 14) | 0x80)), unchecked((sbyte)((absValue >> 21) | 0x80)), (sbyte)(absValue >> 28)};
			}
		}

		/// <summary>
		/// Converts an unsigned int to a variable length byte array.
		/// <para>
		/// The first bit is for continuation info, the other seven bits for data.
		/// 
		/// </para>
		/// </summary>
		/// <param name="value">
		///            the int value, must not be negative. </param>
		/// <returns> an array with 1-5 bytes. </returns>
		public static sbyte[] getVariableByteUnsigned(int value)
		{
			if (value < 0)
			{
				throw new InvalidParameterException("negative value not allowed: " + value);
			}
			else if (value < 128)
			{ // 2^7
				// encode the number in a single byte
				return new sbyte[] {(sbyte) value};
			}
			else if (value < 16384)
			{ // 2^14
				// encode the number in two bytes
				return new sbyte[] {unchecked((sbyte)(value | 0x80)), (sbyte)(value >> 7)};
			}
			else if (value < 2097152)
			{ // 2^21
				// encode the number in three bytes
				return new sbyte[] {unchecked((sbyte)(value | 0x80)), unchecked((sbyte)((value >> 7) | 0x80)), (sbyte)(value >> 14)};
			}
			else if (value < 268435456)
			{ // 2^28
				// encode the number in four bytes
				return new sbyte[] {unchecked((sbyte)(value | 0x80)), unchecked((sbyte)((value >> 7) | 0x80)), unchecked((sbyte)((value >> 14) | 0x80)), (sbyte)(value >> 21)};
			}
			else
			{
				// encode the number in five bytes
				return new sbyte[] {unchecked((sbyte)(value | 0x80)), unchecked((sbyte)((value >> 7) | 0x80)), unchecked((sbyte)((value >> 14) | 0x80)), unchecked((sbyte)((value >> 21) | 0x80)), (sbyte)(value >> 28)};
			}
		}

		private Serializer()
		{
			throw new System.InvalidOperationException();
		}
	}

}