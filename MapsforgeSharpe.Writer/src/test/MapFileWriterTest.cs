/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2015 devemux86
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
	using MapWriterConfiguration = org.mapsforge.map.writer.model.MapWriterConfiguration;
	using TileBasedDataProcessor = org.mapsforge.map.writer.model.TileBasedDataProcessor;

	public class MapFileWriterTest
	{
		private MapWriterConfiguration configuration;
		private TileBasedDataProcessor dataProcessor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		public virtual void setUp()
		{
			this.configuration = new MapWriterConfiguration();
			// this.configuration.addOutputFile(getStringArgument(taskConfig, PARAM_OUTFILE,
			// Constants.DEFAULT_PARAM_OUTFILE));
			this.configuration.WriterVersion = "test";
			this.configuration.loadTagMappingFile("src/test/resources/tag-mapping.xml");
			this.configuration.addMapStartPosition("52.455882,13.297244");
			this.configuration.addMapStartZoom("14");
			this.configuration.addBboxConfiguration("52,13,53,14");
			this.configuration.addZoomIntervalConfiguration("5,0,7,10,8,11,14,12,18");
			this.configuration.Comment = "i love mapsforge";
			this.configuration.DebugStrings = false;
			this.configuration.PolygonClipping = true;
			this.configuration.WayClipping = true;
			this.configuration.Simplification = 0.00001;
			this.configuration.DataProcessorType = "ram";
			this.configuration.BboxEnlargement = 10;
			this.configuration.PreferredLanguages = "en,de";
			this.configuration.addEncodingChoice("auto");
			this.configuration.validate();

			this.dataProcessor = RAMTileBasedDataProcessor.newInstance(this.configuration);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWriteHeaderBuffer()
		public virtual void testWriteHeaderBuffer()
		{
			ByteBuffer headerBuffer = ByteBuffer.allocate(MapFileWriter.HEADER_BUFFER_SIZE);
			int headerLength = MapFileWriter.writeHeaderBuffer(this.configuration, this.dataProcessor, headerBuffer);

			// expected header length
			// 20 + 4 + 4 + 8 + 8 + 16 + 2
			// + 9 ("Mercator")
			// + 1 + 8 + 1
			// + 6 ("en,de")
			// + 17 ("i love mapsforge")
			// + 5("test")
			// + 2 + 19 ("amenity=university")
			// + 2 + 14 + 18 ("natural=beach", natural=coastline")
			// + 1
			// + 3 * (3 + 8 + 8)
			// == 222
			Assert.assertEquals(222, headerLength);
		}

		// @Test
		// public void testProcessPOI() {
		// fail("Not yet implemented");
		// }
		//
		// @Test
		// public void testProcessWay() {
		// fail("Not yet implemented");
		// }
		//
		// @Test
		// public void testWriteWayNodes() {
		// fail("Not yet implemented");
		// }
		//
		// @Test
		// public void testInfoBytePoiLayerAndTagAmount() {
		// fail("Not yet implemented");
		// }
		//
		// @Test
		// public void testInfoByteWayLayerAndTagAmount() {
		// fail("Not yet implemented");
		// }
		//
		// @Test
		// public void testInfoByteOptmizationParams() {
		// fail("Not yet implemented");
		// }
		//
		// @Test
		// public void testInfoBytePOIFeatures() {
		// fail("Not yet implemented");
		// }
		//
		// @Test
		// public void testInfoByteWayFeatures() {
		// fail("Not yet implemented");
		// }
	}

}