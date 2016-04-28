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
namespace org.mapsforge.map.controller
{

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;
	using Model = org.mapsforge.map.model.Model;

	public class MapViewControllerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void repaintTest()
		public virtual void repaintTest()
		{
			DummyMapView dummyMapView = new DummyMapView();
			Model model = new Model();
			MapViewController.create(dummyMapView, model);
			Assert.assertEquals(0, dummyMapView.repaintCounter);

			model.mapViewPosition.ZoomLevel = (sbyte) 1;
			// this does not hold with zoom animation
			// Assert.assertEquals(1, dummyMapView.repaintCounter);
		}
	}

}