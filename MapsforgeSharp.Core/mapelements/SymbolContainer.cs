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

namespace MapsforgeSharp.Core.Mapelements
{
	using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
	using Canvas = MapsforgeSharp.Core.Graphics.Canvas;
	using Display = MapsforgeSharp.Core.Graphics.Display;
	using Matrix = MapsforgeSharp.Core.Graphics.Matrix;
	using Point = org.mapsforge.core.model.Point;
	using Rectangle = org.mapsforge.core.model.Rectangle;

	public class SymbolContainer : MapElementContainer
	{
		internal readonly bool alignCenter;
		public IBitmap symbol;
		public readonly float theta;

		public SymbolContainer(Point point, Display display, int priority, IBitmap symbol) : this(point, display, priority, symbol, 0, true)
		{
		}

		public SymbolContainer(Point point, Display display, int priority, IBitmap symbol, float theta, bool alignCenter) : base(point, display, priority)
		{
			this.symbol = symbol;
			this.theta = theta;
			this.alignCenter = alignCenter;
			if (alignCenter)
			{
				double halfWidth = this.symbol.Width / 2d;
				double halfHeight = this.symbol.Height / 2d;
				this.boundary = new Rectangle(-halfWidth, -halfHeight, halfWidth, halfHeight);
			}
			else
			{
				this.boundary = new Rectangle(0,0, this.symbol.Width, this.symbol.Height);
			}

			this.symbol.IncrementRefCount();
		}

		public override bool Equals(object obj)
		{
			if (!base.Equals(obj))
			{
				return false;
			}
			if (!(obj is SymbolContainer))
			{
				return false;
			}
			SymbolContainer other = (SymbolContainer) obj;
			if (this.symbol != other.symbol)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			int result = base.GetHashCode();
			result = 31 * result + symbol.GetHashCode();
			return result;
		}

		public override void Draw(Canvas canvas, Point origin, Matrix matrix)
		{
			matrix.Reset();
			// We cast to int for pixel perfect positioning
			matrix.Translate((int)(this.xy.X - origin.X + boundary.Left), (int)(this.xy.Y - origin.Y + boundary.Top));
			if (theta != 0 && alignCenter)
			{
				matrix.Rotate(theta, (float) - boundary.Left, (float) - boundary.Top);
			}
			else
			{
				matrix.Rotate(theta);
			}
			canvas.DrawBitmap(this.symbol, matrix);
		}
	}
}