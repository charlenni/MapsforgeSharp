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


	using Assert = org.junit.Assert;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	public class DeltaEncoderTest
	{
		private IList<int?> mockCoordinates;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		public virtual void setUp()
		{
			this.mockCoordinates = new List<>();

			this.mockCoordinates.Add(Convert.ToInt32(52000000));
			this.mockCoordinates.Add(Convert.ToInt32(13000000));

			this.mockCoordinates.Add(Convert.ToInt32(52000100));
			this.mockCoordinates.Add(Convert.ToInt32(13000100));

			this.mockCoordinates.Add(Convert.ToInt32(52000500));
			this.mockCoordinates.Add(Convert.ToInt32(13000500));

			this.mockCoordinates.Add(Convert.ToInt32(52000400));
			this.mockCoordinates.Add(Convert.ToInt32(13000400));

			this.mockCoordinates.Add(Convert.ToInt32(52000800));
			this.mockCoordinates.Add(Convert.ToInt32(13000800));

			this.mockCoordinates.Add(Convert.ToInt32(52001000));
			this.mockCoordinates.Add(Convert.ToInt32(13001000));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDeltaEncode()
		public virtual void testDeltaEncode()
		{
			IList<int?> deltaEncoded = DeltaEncoder.deltaEncode(this.mockCoordinates);
			Assert.assertEquals(Convert.ToInt32(52000000), deltaEncoded[0]);
			Assert.assertEquals(Convert.ToInt32(13000000), deltaEncoded[1]);
			Assert.assertEquals(Convert.ToInt32(100), deltaEncoded[2]);
			Assert.assertEquals(Convert.ToInt32(100), deltaEncoded[3]);
			Assert.assertEquals(Convert.ToInt32(400), deltaEncoded[4]);
			Assert.assertEquals(Convert.ToInt32(400), deltaEncoded[5]);
			Assert.assertEquals(Convert.ToInt32(-100), deltaEncoded[6]);
			Assert.assertEquals(Convert.ToInt32(-100), deltaEncoded[7]);
			Assert.assertEquals(Convert.ToInt32(400), deltaEncoded[8]);
			Assert.assertEquals(Convert.ToInt32(400), deltaEncoded[9]);
			Assert.assertEquals(Convert.ToInt32(200), deltaEncoded[10]);
			Assert.assertEquals(Convert.ToInt32(200), deltaEncoded[11]);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDoubleDeltaEncode()
		public virtual void testDoubleDeltaEncode()
		{
			IList<int?> ddeltaEncoded = DeltaEncoder.doubleDeltaEncode(this.mockCoordinates);
			Assert.assertEquals(Convert.ToInt32(52000000), ddeltaEncoded[0]);
			Assert.assertEquals(Convert.ToInt32(13000000), ddeltaEncoded[1]);
			Assert.assertEquals(Convert.ToInt32(100), ddeltaEncoded[2]);
			Assert.assertEquals(Convert.ToInt32(100), ddeltaEncoded[3]);
			Assert.assertEquals(Convert.ToInt32(300), ddeltaEncoded[4]);
			Assert.assertEquals(Convert.ToInt32(300), ddeltaEncoded[5]);
			Assert.assertEquals(Convert.ToInt32(-500), ddeltaEncoded[6]);
			Assert.assertEquals(Convert.ToInt32(-500), ddeltaEncoded[7]);
			Assert.assertEquals(Convert.ToInt32(500), ddeltaEncoded[8]);
			Assert.assertEquals(Convert.ToInt32(500), ddeltaEncoded[9]);
			Assert.assertEquals(Convert.ToInt32(-200), ddeltaEncoded[10]);
			Assert.assertEquals(Convert.ToInt32(-200), ddeltaEncoded[11]);
		}
	}

}