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
	using LatLong = org.mapsforge.core.model.LatLong;
	using MapPosition = org.mapsforge.core.model.MapPosition;
	using PreferencesFacade = org.mapsforge.map.model.common.PreferencesFacade;

	public class ModelTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void constructorTest()
		public virtual void constructorTest()
		{
			Model model = new Model();
			Assert.assertNotNull(model.frameBufferModel);
			Assert.assertNotNull(model.mapViewDimension);
			Assert.assertNotNull(model.mapViewPosition);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void saveAndInitTest()
		public virtual void saveAndInitTest()
		{
			MapPosition mapPosition1 = new MapPosition(new LatLong(1, 1, true), (sbyte) 1);
			MapPosition mapPosition2 = new MapPosition(new LatLong(2, 2, true), (sbyte) 2);

			Model model = new Model();
			model.mapViewPosition.MapPosition = mapPosition1;
			Assert.assertEquals(mapPosition1, model.mapViewPosition.MapPosition);

			PreferencesFacade preferencesFacade = new DummyPreferences();
			model.save(preferencesFacade);

			model.mapViewPosition.MapPosition = mapPosition2;
			Assert.assertEquals(mapPosition2, model.mapViewPosition.MapPosition);

			model.init(preferencesFacade);
			Assert.assertEquals(mapPosition1, model.mapViewPosition.MapPosition);
		}
	}

}