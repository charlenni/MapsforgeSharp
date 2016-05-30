/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

namespace org.mapsforge.map.layer.overlay
{
	using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
	using ICanvas = MapsforgeSharp.Core.Graphics.ICanvas;
	using BoundingBox = MapsforgeSharp.Core.Model.BoundingBox;
	using LatLong = MapsforgeSharp.Core.Model.LatLong;
	using Point = MapsforgeSharp.Core.Model.Point;
	using Rectangle = MapsforgeSharp.Core.Model.Rectangle;
	using MercatorProjection = MapsforgeSharp.Core.Util.MercatorProjection;

	/// <summary>
	/// A {@code Marker} draws a <seealso cref="Bitmap"/> at a given geographical position.
	/// </summary>
	public class Marker : Layer
	{
		private IBitmap bitmap;
		private int horizontalOffset;
		private LatLong latLong;
		private int verticalOffset;

		/// <param name="latLong">
		///            the initial geographical coordinates of this marker (may be null). </param>
		/// <param name="bitmap">
		///            the initial {@code IBitmap} of this marker (may be null). </param>
		/// <param name="horizontalOffset">
		///            the horizontal marker offset. </param>
		/// <param name="verticalOffset">
		///            the vertical marker offset. </param>
		public Marker(LatLong latLong, IBitmap bitmap, int horizontalOffset, int verticalOffset) : base()
		{
			this.latLong = latLong;
			this.bitmap = bitmap;
			this.horizontalOffset = horizontalOffset;
			this.verticalOffset = verticalOffset;
		}

		public virtual bool Contains(Point center, Point point)
		{
			lock (this)
			{
				Rectangle r = new Rectangle(center.X - (float) bitmap.Width / 2 + this.horizontalOffset, center.Y - (float) bitmap.Height / 2 + this.verticalOffset, center.X + (float) bitmap.Width / 2 + this.horizontalOffset, center.Y + (float) bitmap.Height / 2 + this.verticalOffset);
				return r.Contains(point);
			}
		}

		public override void Draw(BoundingBox boundingBox, sbyte zoomLevel, ICanvas canvas, Point topLeftPoint)
		{
			lock (this)
			{
				if (this.latLong == null || this.bitmap == null)
				{
					return;
				}
        
				long mapSize = MercatorProjection.GetMapSize(zoomLevel, this.displayModel.TileSize);
				double pixelX = MercatorProjection.LongitudeToPixelX(this.latLong.Longitude, mapSize);
				double pixelY = MercatorProjection.LatitudeToPixelY(this.latLong.Latitude, mapSize);
        
				int halfBitmapWidth = this.bitmap.Width / 2;
				int halfBitmapHeight = this.bitmap.Height / 2;
        
				int left = (int)(pixelX - topLeftPoint.X - halfBitmapWidth + this.horizontalOffset);
				int top = (int)(pixelY - topLeftPoint.Y - halfBitmapHeight + this.verticalOffset);
				int right = left + this.bitmap.Width;
				int bottom = top + this.bitmap.Height;
        
				Rectangle bitmapRectangle = new Rectangle(left, top, right, bottom);
				Rectangle canvasRectangle = new Rectangle(0, 0, canvas.Width, canvas.Height);
				if (!canvasRectangle.Intersects(bitmapRectangle))
				{
					return;
				}
        
				canvas.DrawBitmap(this.bitmap, left, top);
			}
		}

		/// <returns> the {@code IBitmap} of this marker (may be null). </returns>
		public virtual IBitmap Bitmap
		{
			get
			{
				lock (this)
				{
					return this.bitmap;
				}
			}
			set
			{
				lock (this)
				{
					if (this.bitmap != null && this.bitmap.Equals(value))
					{
						return;
					}
					if (this.bitmap != null)
					{
						this.bitmap.DecrementRefCount();
					}
					this.bitmap = value;
				}
			}
		}

		/// <returns> the horizontal offset of this marker. </returns>
		public virtual int HorizontalOffset
		{
			get
			{
				lock (this)
				{
					return this.horizontalOffset;
				}
			}
			set
			{
				lock (this)
				{
					this.horizontalOffset = value;
				}
			}
		}

		/// <returns> the geographical coordinates of this marker (may be null). </returns>
		public virtual LatLong LatLong
		{
			get
			{
				lock (this)
				{
					return this.latLong;
				}
			}
			set
			{
				lock (this)
				{
					this.latLong = value;
				}
			}
		}

		/// <returns> Gets the LatLong Position of the Object </returns>
		public override LatLong Position
		{
			get
			{
				lock (this)
				{
					return this.latLong;
				}
			}
		}

		/// <returns> the vertical offset of this marker. </returns>
		public virtual int VerticalOffset
		{
			get
			{
				lock (this)
				{
					return this.verticalOffset;
				}
			}
			set
			{
				lock (this)
				{
					this.verticalOffset = value;
				}
			}
		}

		public override void OnDestroy()
		{
			lock (this)
			{
				if (this.bitmap != null)
				{
					this.bitmap.DecrementRefCount();
				}
			}
		}
	}
}