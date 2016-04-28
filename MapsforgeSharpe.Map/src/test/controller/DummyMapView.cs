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
namespace org.mapsforge.map.controller
{

	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using Dimension = org.mapsforge.core.model.Dimension;
	using LatLong = org.mapsforge.core.model.LatLong;
	using Layer = org.mapsforge.map.layer.Layer;
	using LayerManager = org.mapsforge.map.layer.LayerManager;
	using Model = org.mapsforge.map.model.Model;
	using MapScaleBar = org.mapsforge.map.scalebar.MapScaleBar;
	using FpsCounter = org.mapsforge.map.view.FpsCounter;
	using FrameBuffer = org.mapsforge.map.view.FrameBuffer;
	using MapView = org.mapsforge.map.view.MapView;

	public class DummyMapView : MapView
	{
		public int repaintCounter;

		public override void addLayer(Layer layer)
		{
			// no-op
		}

		public override void destroy()
		{
			// do nothing
		}

		public override void destroyAll()
		{
			// do nothing
		}

		public override BoundingBox BoundingBox
		{
			get
			{
				return null;
			}
		}

		public override Dimension Dimension
		{
			get
			{
				return null;
			}
		}

		public override FpsCounter FpsCounter
		{
			get
			{
				return null;
			}
		}

		public override FrameBuffer FrameBuffer
		{
			get
			{
				return null;
			}
		}

		public override int Height
		{
			get
			{
				return 0;
			}
		}

		public override LayerManager LayerManager
		{
			get
			{
				return null;
			}
		}

		public override MapScaleBar MapScaleBar
		{
			get
			{
				return null;
			}
			set
			{
				// no-op
			}
		}

		public override Model Model
		{
			get
			{
				return null;
			}
		}

		public override int Width
		{
			get
			{
				return 0;
			}
		}

		public override void repaint()
		{
			++this.repaintCounter;
		}

		public override LatLong Center
		{
			set
			{
				// no-op
			}
		}


		public override sbyte ZoomLevel
		{
			set
			{
				// no-op
			}
		}
	}

}