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
namespace org.mapsforge.map.reader
{

	using Test = org.junit.Test;

	public class MapFileSingleDeltaEncodingTest
	{
		private static readonly MapFile MAP_FILE_SINGLE_DELTA = new MapFile("src/test/resources/single_delta_encoding/output.map", null);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeQueryTest()
		public virtual void executeQueryTest()
		{
			EncodingTest.runTest(MAP_FILE_SINGLE_DELTA);
		}
	}

}