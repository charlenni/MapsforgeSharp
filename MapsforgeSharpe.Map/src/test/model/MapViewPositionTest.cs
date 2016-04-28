/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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
namespace org.mapsforge.map.model
{

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;
	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using LatLong = org.mapsforge.core.model.LatLong;
	using MapPosition = org.mapsforge.core.model.MapPosition;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using DummyObserver = org.mapsforge.map.model.common.DummyObserver;

	public class MapViewPositionTest
	{
		private static void verifyInvalidSetZoomLevel(MapViewPosition mapViewPosition, sbyte zoomLevel)
		{
			try
			{
				mapViewPosition.ZoomLevel = zoomLevel;
				Assert.fail("zoomLevel: " + zoomLevel);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

		private static void verifyInvalidSetZoomLevelMax(MapViewPosition mapViewPosition, sbyte zoomLevelMax)
		{
			try
			{
				mapViewPosition.ZoomLevelMax = zoomLevelMax;
				Assert.fail("zoomLevelMax: " + zoomLevelMax);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

		private static void verifyInvalidSetZoomLevelMin(MapViewPosition mapViewPosition, sbyte zoomLevelMin)
		{
			try
			{
				mapViewPosition.ZoomLevelMin = zoomLevelMin;
				Assert.fail("zoomLevelMin: " + zoomLevelMin);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mapLimitTest()
		public virtual void mapLimitTest()
		{
			MapViewPosition mapViewPosition = new MapViewPosition(new DisplayModel());
			Assert.assertNull(mapViewPosition.MapLimit);

			LatLong latLong = new LatLong(10, 20, true);
			mapViewPosition.Center = latLong;
			Assert.assertEquals(latLong, mapViewPosition.Center);

			BoundingBox boundingBox = new BoundingBox(1, 2, 3, 4);
			mapViewPosition.MapLimit = boundingBox;
			Assert.assertEquals(boundingBox, mapViewPosition.MapLimit);
			Assert.assertEquals(latLong, mapViewPosition.Center);

			mapViewPosition.Center = latLong;
			Assert.assertEquals(new LatLong(3, 4, true), mapViewPosition.Center);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void moveCenterTest()
		public virtual void moveCenterTest()
		{
			MapViewPosition mapViewPosition = new MapViewPosition(new FixedTileSizeDisplayModel(256));
			mapViewPosition.moveCenter(MercatorProjection.getMapSize((sbyte) 0, (new FixedTileSizeDisplayModel(256)).TileSize) / -360d, 0);

			MapPosition mapPosition = mapViewPosition.MapPosition;

			Assert.assertEquals(0, mapPosition.latLong.latitude, 0);
			Assert.assertEquals(1, mapPosition.latLong.longitude, 1.0E-14);
			Assert.assertEquals(0, mapPosition.zoomLevel);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void observerTest()
		public virtual void observerTest()
		{
			DummyObserver dummyObserver = new DummyObserver();
			MapViewPosition mapViewPosition = new MapViewPosition(new DisplayModel());
			mapViewPosition.addObserver(dummyObserver);
			Assert.assertEquals(0, dummyObserver.Callbacks);

			mapViewPosition.Center = new LatLong(0, 0, true);
			// Assert.assertEquals(1, dummyObserver.getCallbacks());

			mapViewPosition.MapLimit = new BoundingBox(0, 0, 0, 0);
			// Assert.assertEquals(2, dummyObserver.getCallbacks());

			mapViewPosition.MapPosition = new MapPosition(new LatLong(0, 0, true), (sbyte) 0);
			// Assert.assertEquals(3, dummyObserver.getCallbacks());

			mapViewPosition.ZoomLevel = (sbyte) 0;
			// Assert.assertEquals(4, dummyObserver.getCallbacks());

			mapViewPosition.ZoomLevelMax = (sbyte) 0;
			// Assert.assertEquals(5, dummyObserver.getCallbacks());

			mapViewPosition.ZoomLevelMin = (sbyte) 0;
			// Assert.assertEquals(6, dummyObserver.getCallbacks());
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void zoomInTest()
		public virtual void zoomInTest()
		{
			MapViewPosition mapViewPosition = new MapViewPosition(new DisplayModel());
			Assert.assertEquals(0, mapViewPosition.ZoomLevel);
			mapViewPosition.zoomIn();
			Assert.assertEquals((sbyte) 1, mapViewPosition.ZoomLevel);

			mapViewPosition.ZoomLevel = sbyte.MaxValue;
			Assert.assertEquals(sbyte.MaxValue, mapViewPosition.ZoomLevel);
			mapViewPosition.zoomIn();
			Assert.assertEquals(sbyte.MaxValue, mapViewPosition.ZoomLevel);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void zoomLevelMaxTest()
		public virtual void zoomLevelMaxTest()
		{
			MapViewPosition mapViewPosition = new MapViewPosition(new DisplayModel());
			Assert.assertEquals(sbyte.MaxValue, mapViewPosition.ZoomLevelMax);

			mapViewPosition.ZoomLevel = (sbyte) 1;
			Assert.assertEquals(1, mapViewPosition.ZoomLevel);

			mapViewPosition.ZoomLevelMax = (sbyte) 0;
			Assert.assertEquals(0, mapViewPosition.ZoomLevelMax);
			Assert.assertEquals(1, mapViewPosition.ZoomLevel);

			mapViewPosition.ZoomLevel = (sbyte) 1;
			Assert.assertEquals(0, mapViewPosition.ZoomLevel);

			verifyInvalidSetZoomLevelMax(mapViewPosition, (sbyte) -1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void zoomLevelMinMaxTest()
		public virtual void zoomLevelMinMaxTest()
		{
			MapViewPosition mapViewPosition = new MapViewPosition(new DisplayModel());
			mapViewPosition.ZoomLevelMin = (sbyte) 1;
			mapViewPosition.ZoomLevelMax = (sbyte) 2;

			verifyInvalidSetZoomLevelMin(mapViewPosition, (sbyte) 3);
			verifyInvalidSetZoomLevelMax(mapViewPosition, (sbyte) 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void zoomLevelMinTest()
		public virtual void zoomLevelMinTest()
		{
			MapViewPosition mapViewPosition = new MapViewPosition(new DisplayModel());
			Assert.assertEquals(0, mapViewPosition.ZoomLevelMin);

			mapViewPosition.ZoomLevel = (sbyte) 0;
			Assert.assertEquals(0, mapViewPosition.ZoomLevel);

			mapViewPosition.ZoomLevelMin = (sbyte) 1;
			Assert.assertEquals(1, mapViewPosition.ZoomLevelMin);
			Assert.assertEquals(0, mapViewPosition.ZoomLevel);

			mapViewPosition.ZoomLevel = (sbyte) 0;
			Assert.assertEquals(1, mapViewPosition.ZoomLevel);

			verifyInvalidSetZoomLevelMin(mapViewPosition, (sbyte) -1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void zoomLevelTest()
		public virtual void zoomLevelTest()
		{
			MapViewPosition mapViewPosition = new MapViewPosition(new DisplayModel());
			verifyInvalidSetZoomLevel(mapViewPosition, (sbyte) -1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void zoomOutTest()
		public virtual void zoomOutTest()
		{
			MapViewPosition mapViewPosition = new MapViewPosition(new DisplayModel());
			mapViewPosition.ZoomLevel = (sbyte) 1;
			Assert.assertEquals(1, mapViewPosition.ZoomLevel);
			mapViewPosition.zoomOut();
			Assert.assertEquals(0, mapViewPosition.ZoomLevel);

			mapViewPosition.ZoomLevel = (sbyte) 0;
			Assert.assertEquals(0, mapViewPosition.ZoomLevel);
			mapViewPosition.zoomOut();
			Assert.assertEquals(0, mapViewPosition.ZoomLevel);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void zoomTest()
		public virtual void zoomTest()
		{
			MapViewPosition mapViewPosition = new MapViewPosition(new DisplayModel());
			Assert.assertEquals(0, mapViewPosition.ZoomLevel);

			mapViewPosition.zoom((sbyte) 1);
			Assert.assertEquals(1, mapViewPosition.ZoomLevel);

			mapViewPosition.zoom((sbyte) -1);
			Assert.assertEquals(0, mapViewPosition.ZoomLevel);

			mapViewPosition.zoom((sbyte) 5);
			Assert.assertEquals(5, mapViewPosition.ZoomLevel);

			mapViewPosition.zoom((sbyte) -2);
			Assert.assertEquals(3, mapViewPosition.ZoomLevel);

			mapViewPosition.zoom(sbyte.MaxValue);
			Assert.assertEquals(sbyte.MaxValue, mapViewPosition.ZoomLevel);

			mapViewPosition.zoom(sbyte.MinValue);
			Assert.assertEquals(0, mapViewPosition.ZoomLevel);
		}
	}

}