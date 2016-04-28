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
namespace org.mapsforge.map.reader
{

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;
	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using LatLong = org.mapsforge.core.model.LatLong;
	using MapFileInfo = org.mapsforge.map.reader.header.MapFileInfo;

	public class MapFileFileHeaderTest
	{
		private static readonly BoundingBox BOUNDING_BOX = new BoundingBox(0.1, 0.2, 0.3, 0.4);
		private const string COMMENT = "testcomment";
		private const string CREATED_BY = "mapsforge-map-writer-0.3.1-SNAPSHOT";
		private const int FILE_SIZE = 709;
		private const int FILE_VERSION = 3;
		private const string LANGUAGES_PREFERENCE = "en";
		private const long MAP_DATE = 1335871456973L;
		private static readonly File MAP_FILE = new File("src/test/resources/file_header/output.map");
		private const int NUMBER_OF_SUBFILES = 3;
		private const string PROJECTION_NAME = "Mercator";
		private static readonly LatLong START_POSITION = new LatLong(0.15, 0.25, true);
		private static readonly sbyte? START_ZOOM_LEVEL = Convert.ToSByte((sbyte) 16);
		private const int TILE_PIXEL_SIZE = 256;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getMapFileInfoTest()
		public virtual void getMapFileInfoTest()
		{
			MapFile mapFile = new MapFile(MAP_FILE);

			MapFileInfo mapFileInfo = mapFile.MapFileInfo;
			mapFile.close();

			Assert.assertEquals(BOUNDING_BOX, mapFileInfo.boundingBox);
			Assert.assertEquals(FILE_SIZE, mapFileInfo.fileSize);
			Assert.assertEquals(FILE_VERSION, mapFileInfo.fileVersion);
			Assert.assertEquals(MAP_DATE, mapFileInfo.mapDate);
			Assert.assertEquals(NUMBER_OF_SUBFILES, mapFileInfo.numberOfSubFiles);
			Assert.assertEquals(PROJECTION_NAME, mapFileInfo.projectionName);
			Assert.assertEquals(TILE_PIXEL_SIZE, mapFileInfo.tilePixelSize);

			Assert.assertEquals(0, mapFileInfo.poiTags.length);
			Assert.assertEquals(0, mapFileInfo.wayTags.length);

			Assert.assertFalse(mapFileInfo.debugFile);
			Assert.assertEquals(START_POSITION, mapFileInfo.startPosition);
			Assert.assertEquals(START_ZOOM_LEVEL, mapFileInfo.startZoomLevel);
			Assert.assertEquals(LANGUAGES_PREFERENCE, mapFileInfo.languagesPreference);
			Assert.assertEquals(COMMENT, mapFileInfo.comment);
			Assert.assertEquals(CREATED_BY, mapFileInfo.createdBy);
		}
	}

}