/*
 * Copyright 2014 Ludwig M Brinckmann
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

namespace MapsforgeSharp.Core.Mapelements
{
    using System;
    using System.Text;

    using Canvas = MapsforgeSharp.Core.Graphics.Canvas;
	using Display = MapsforgeSharp.Core.Graphics.Display;
	using Matrix = MapsforgeSharp.Core.Graphics.Matrix;
	using Point = org.mapsforge.core.model.Point;
	using Rectangle = org.mapsforge.core.model.Rectangle;

	/// <summary>
	/// The MapElementContainer is the abstract base class for annotations that can be placed on the
	/// map, e.g. labels and icons.
	/// 
	/// A MapElementContainer has a central pivot point, which denotes the geographic point for the entity
	/// translated into absolute map pixels. The boundary denotes the space that the item requires
	/// around this central point.
	/// 
	/// A MapElementContainer has a priority (higher value means higher priority) that should be used to determine
	/// the drawing order, i.e. elements with higher priority should be drawn before elements with lower
	/// priority. If there is not enough space on the map, elements with lower priority should then not be
	/// drawn.
	/// </summary>
	public abstract class MapElementContainer : IComparable<MapElementContainer>
	{

		protected internal Rectangle boundary;
		protected internal Rectangle boundaryAbsolute;
		protected internal Display display;
		protected internal readonly int priority;
		protected internal readonly Point xy;

		protected internal MapElementContainer(Point xy, Display display, int priority)
		{
			this.xy = xy;
			this.display = display;
			this.priority = priority;
		}

		/// <summary>
		/// Compares elements according to their priority.
		/// </summary>
		/// <param name="other"> </param>
		/// <returns> priority order </returns>

		public virtual int CompareTo(MapElementContainer other)
		{
			if (this.priority < other.priority)
			{
				return -1;
			}
			if (this.priority > other.priority)
			{
				return 1;
			}
			return 0;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is MapElementContainer))
			{
				return false;
			}
			MapElementContainer other = (MapElementContainer) obj;
			if (this.priority != other.priority)
			{
				return false;
			}
			else if (!this.xy.Equals(other.xy))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Drawing method: element will draw itself on canvas shifted by origin point of canvas and
		/// using the matrix if rotation is required.
		/// </summary>
		/// <param name="canvas"> </param>
		/// <param name="origin"> </param>
		/// <param name="matrix"> </param>
		public abstract void Draw(Canvas canvas, Point origin, Matrix matrix);

		/// <summary>
		/// Gets the pixel absolute boundary for this element.
		/// </summary>
		/// <returns> Rectangle with absolute pixel coordinates. </returns>
		protected internal virtual Rectangle BoundaryAbsolute
		{
			get
			{
				if (boundaryAbsolute == null)
				{
					boundaryAbsolute = this.boundary.Shift(xy);
				}
				return boundaryAbsolute;
			}
		}

		public virtual bool Intersects(Rectangle rectangle)
		{
			return this.BoundaryAbsolute.Intersects(rectangle);
		}

		/// <summary>
		/// Returns if MapElementContainers clash with each other </summary>
		/// <param name="other"> element to test against </param>
		/// <returns> true if they overlap </returns>
		public virtual bool ClashesWith(MapElementContainer other)
		{
			// if either of the elements is always drawn, the elements do not clash
			if (Display.ALWAYS == this.display || Display.ALWAYS == other.display)
			{
				return false;
			}
			 return this.BoundaryAbsolute.Intersects(other.BoundaryAbsolute);
		}

		public override int GetHashCode()
		{
			int result = 7;
			result = 31 * result + xy.GetHashCode();
			result = 31 * result + priority;
			return result;
		}

		/// <summary>
		/// Gets the center point of this element. </summary>
		/// <returns> Point with absolute center pixel coordinates. </returns>
		public virtual Point Point
		{
			get
			{
				return this.xy;
			}
		}

		public virtual int Priority
		{
			get
			{
				return priority;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("xy=");
			stringBuilder.Append(this.xy);
			stringBuilder.Append(", priority=");
			stringBuilder.Append(this.priority);
			return stringBuilder.ToString();
		}
	}
}