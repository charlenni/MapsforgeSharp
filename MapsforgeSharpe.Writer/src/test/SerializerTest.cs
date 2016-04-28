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

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;

	public class SerializerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getFiveBytesTest()
		public virtual void getFiveBytesTest()
		{
			sbyte[] fiveBytes = Serializer.getFiveBytes(0);
			Assert.assertArrayEquals(new sbyte[] {0, 0, 0, 0, 0}, fiveBytes);

			fiveBytes = Serializer.getFiveBytes(5);
			Assert.assertArrayEquals(new sbyte[] {0, 0, 0, 0, 5}, fiveBytes);
		}
	}

}