/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.map.model
{
	using Persistable = org.mapsforge.map.model.common.Persistable;
	using PreferencesFacade = org.mapsforge.map.model.common.PreferencesFacade;

	public class Model : Persistable
	{
		private bool InstanceFieldsInitialized = false;

		public Model()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			mapViewPosition = new MapViewPosition(displayModel);
		}

		public readonly DisplayModel displayModel = new DisplayModel();
		public readonly FrameBufferModel frameBufferModel = new FrameBufferModel();
		public readonly MapViewDimension mapViewDimension = new MapViewDimension();
		public MapViewPosition mapViewPosition;

		public void Init(PreferencesFacade preferencesFacade)
		{
			this.mapViewPosition.Init(preferencesFacade);
		}

		public void Save(PreferencesFacade preferencesFacade)
		{
			this.mapViewPosition.Save(preferencesFacade);
		}
	}
}