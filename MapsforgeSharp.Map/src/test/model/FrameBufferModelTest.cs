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
namespace org.mapsforge.map.model
{

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;
	using Dimension = org.mapsforge.core.model.Dimension;
	using LatLong = org.mapsforge.core.model.LatLong;
	using MapPosition = org.mapsforge.core.model.MapPosition;
	using DummyObserver = org.mapsforge.map.model.common.DummyObserver;

	public class FrameBufferModelTest
	{
		private static void verifyInvalidOverdrawFactor(int overdrawFactor)
		{
			FrameBufferModel frameBufferModel = new FrameBufferModel();
			try
			{
				frameBufferModel.OverdrawFactor = overdrawFactor;
				Assert.fail("overdrawFactor: " + overdrawFactor);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dimensionTest()
		public virtual void dimensionTest()
		{
			FrameBufferModel frameBufferModel = new FrameBufferModel();
			Assert.assertNull(frameBufferModel.Dimension);

			frameBufferModel.Dimension = new Dimension(0, 0);
			Assert.assertEquals(new Dimension(0, 0), frameBufferModel.Dimension);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mapPositionTest()
		public virtual void mapPositionTest()
		{
			FrameBufferModel frameBufferModel = new FrameBufferModel();
			Assert.assertNull(frameBufferModel.MapPosition);

			frameBufferModel.MapPosition = new MapPosition(new LatLong(0, 0, true), (sbyte) 0);
			Assert.assertEquals(new MapPosition(new LatLong(0, 0, true), (sbyte) 0), frameBufferModel.MapPosition);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void observerTest()
		public virtual void observerTest()
		{
			DummyObserver dummyObserver = new DummyObserver();
			FrameBufferModel frameBufferModel = new FrameBufferModel();
			frameBufferModel.addObserver(dummyObserver);
			Assert.assertEquals(0, dummyObserver.Callbacks);

			frameBufferModel.Dimension = new Dimension(0, 0);
			Assert.assertEquals(1, dummyObserver.Callbacks);

			frameBufferModel.MapPosition = new MapPosition(new LatLong(0, 0, true), (sbyte) 0);
			Assert.assertEquals(2, dummyObserver.Callbacks);

			frameBufferModel.OverdrawFactor = 1;
			Assert.assertEquals(3, dummyObserver.Callbacks);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void overdrawFactorTest()
		public virtual void overdrawFactorTest()
		{
			FrameBufferModel frameBufferModel = new FrameBufferModel();
			Assert.assertEquals(1.2, frameBufferModel.OverdrawFactor, 0);

			frameBufferModel.OverdrawFactor = 2;
			Assert.assertEquals(2, frameBufferModel.OverdrawFactor, 0);

			verifyInvalidOverdrawFactor(0);
			verifyInvalidOverdrawFactor(-1);
		}
	}

}