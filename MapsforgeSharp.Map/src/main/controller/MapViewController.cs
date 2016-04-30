/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2016 Dirk Weltz
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
	using Model = org.mapsforge.map.model.Model;
	using Observer = org.mapsforge.map.model.common.Observer;
	using MapView = org.mapsforge.map.view.MapView;

	public sealed class MapViewController : Observer
	{
		public static MapViewController Create(MapView mapView, Model model)
		{
			MapViewController mapViewController = new MapViewController(mapView);

			model.mapViewPosition.AddObserver(mapViewController);

			return mapViewController;
		}

		private readonly MapView mapView;

		private MapViewController(MapView mapView)
		{
			this.mapView = mapView;
		}

		public void OnChange()
		{
			this.mapView.Repaint();
		}
	}
}