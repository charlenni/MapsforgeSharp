/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2015 devemux86
 * Copyright 2016 Dirk Weltz
 * Copyright 2016 Michael Oed
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

namespace org.mapsforge.map.layer
{
	using Canvas = MapsforgeSharp.Core.Graphics.Canvas;
	using BoundingBox = MapsforgeSharp.Core.Model.BoundingBox;
	using LatLong = MapsforgeSharp.Core.Model.LatLong;
	using Point = MapsforgeSharp.Core.Model.Point;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;

	public abstract class Layer
	{
		protected internal DisplayModel displayModel;
		private Redrawer assignedRedrawer;
		private bool visible = true;

		/// <summary>
		/// Draws this {@code Layer} on the given canvas.
		/// </summary>
		/// <param name="boundingBox">
		///            the geographical area which should be drawn. </param>
		/// <param name="zoomLevel">
		///            the zoom level at which this {@code Layer} should draw itself. </param>
		/// <param name="canvas">
		///            the canvas on which this {@code Layer} should draw itself. </param>
		/// <param name="topLeftPoint">
		///            the top-left pixel position of the canvas relative to the top-left map position. </param>
		public abstract void Draw(BoundingBox boundingBox, sbyte zoomLevel, Canvas canvas, Point topLeftPoint);

		/// <summary>
		/// Gets the geographic position of this layer element, if it exists.
		/// <para>
		/// The default implementation of this method returns null.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the geographic position of this layer element, null otherwise </returns>
		public virtual LatLong Position
		{
			get
			{
				return null;
			}
		}

		/// <returns> true if this {@code Layer} is currently visible, false otherwise. The default value is true. </returns>
		public bool Visible
		{
			get
			{
				return this.visible;
			}
			set
			{
				SetVisible(value, true);
			}
		}

		public virtual void OnDestroy()
		{
			// do nothing
		}

		/// <summary>
		/// Handles a long press event. A long press event is only triggered if the map was not moved. A return value of true
		/// indicates that the long press event has been handled by this overlay and stops its propagation to other overlays.
		/// <para>
		/// The default implementation of this method does nothing and returns false.
		/// 
		/// </para>
		/// </summary>
		/// <param name="tapLatLong">
		///            the geographic position of the long press. </param>
		/// <param name="layerXY">
		///            the xy position of the layer element (if available) </param>
		/// <param name="tapXY">
		///            the xy position of the tap </param>
		/// <returns> true if the long press event was handled, false otherwise. </returns>
		public virtual bool OnLongPress(LatLong tapLatLong, Point layerXY, Point tapXY)
		{
			return false;
		}

		/// <summary>
		/// Handles a tap event. A return value of true indicates that the tap event has been handled by this overlay and
		/// stops its propagation to other overlays.
		/// <para>
		/// The default implementation of this method does nothing and returns false.
		/// 
		/// </para>
		/// </summary>
		/// <param name="tapLatLong">
		///            the the geographic position of the tap. </param>
		/// <param name="layerXY">
		///            the xy position of the layer element (if available) </param>
		/// <param name="tapXY">
		///            the xy position of the tap </param>
		/// <returns> true if the tap event was handled, false otherwise. </returns>

		public virtual bool OnTap(LatLong tapLatLong, Point layerXY, Point tapXY)
		{
			return false;
		}

		/// <summary>
		/// Requests an asynchronous redrawing of all layers.
		/// </summary>
		public void RequestRedraw()
		{
			lock (this)
			{
				if (this.assignedRedrawer != null)
				{
					this.assignedRedrawer.RedrawLayers();
				}
			}
		}

		/// <summary>
		/// The DisplayModel comes from a MapView, so is generally not known when the layer itself is created. Maybe a better
		/// way would be to have a MapView as a parameter when creating a layer.
		/// </summary>
		/// <param name="displayModel">
		///            the displayModel to use. </param>
		public virtual DisplayModel DisplayModel
		{
			set
			{
				lock (this)
				{
					this.displayModel = value;
				}
			}
		}


		/// <summary>
		/// Sets the visibility flag of this {@code Layer} to the given value.
		/// </summary>
		public void SetVisible(bool visible, bool redraw)
		{
			this.visible = visible;

			if (redraw)
			{
				RequestRedraw();
			}
		}

		/// <summary>
		/// Called each time this {@code Layer} is added to a <seealso cref="Layers"/> list.
		/// </summary>
		protected internal virtual void OnAdd()
		{
			// do nothing
		}

		/// <summary>
		/// Called each time this {@code Layer} is removed from a <seealso cref="Layers"/> list.
		/// </summary>
		protected internal virtual void OnRemove()
		{
			// do nothing
		}

		internal void Assign(Redrawer redrawer)
		{
			lock (this)
			{
				if (this.assignedRedrawer != null)
				{
					throw new System.InvalidOperationException("layer already assigned");
				}
        
				this.assignedRedrawer = redrawer;
				OnAdd();
			}
		}

		internal void Unassign()
		{
			lock (this)
			{
				if (this.assignedRedrawer == null)
				{
					throw new System.InvalidOperationException("layer is not assigned");
				}
        
				this.assignedRedrawer = null;
				OnRemove();
			}
		}
	}
}