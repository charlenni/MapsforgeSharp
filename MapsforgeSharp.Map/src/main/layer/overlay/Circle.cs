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

namespace org.mapsforge.map.layer.overlay
{
	using Canvas = MapsforgeSharp.Core.Graphics.Canvas;
	using Paint = MapsforgeSharp.Core.Graphics.Paint;
	using BoundingBox = MapsforgeSharp.Core.Model.BoundingBox;
	using LatLong = MapsforgeSharp.Core.Model.LatLong;
	using Point = MapsforgeSharp.Core.Model.Point;
	using Rectangle = MapsforgeSharp.Core.Model.Rectangle;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;

	/// <summary>
	/// A {@code Circle} consists of a center <seealso cref="LatLong"/> and a non-negative radius in meters.
	/// <para>
	/// A {@code Circle} holds two <seealso cref="Paint"/> objects to allow for different outline and filling. These paints define
	/// drawing parameters such as color, stroke width, pattern and transparency.
	/// </para>
	/// </summary>
	public class Circle : Layer
	{
		private readonly bool keepAligned;
		private LatLong latLong;
		private Paint paintFill;
		private Paint paintStroke;
		private float radius;

		/// <param name="latLong">
		///            the initial center point of this circle (may be null). </param>
		/// <param name="radius">
		///            the initial non-negative radius of this circle in meters. </param>
		/// <param name="paintFill">
		///            the initial {@code Paint} used to fill this circle (may be null). </param>
		/// <param name="paintStroke">
		///            the initial {@code Paint} used to stroke this circle (may be null). </param>
		/// <exception cref="IllegalArgumentException">
		///             if the given {@code radius} is negative or <seealso cref="Float#NaN"/>. </exception>
		public Circle(LatLong latLong, float radius, Paint paintFill, Paint paintStroke) : this(latLong, radius, paintFill, paintStroke, false)
		{
		}

		/// <param name="latLong">
		///            the initial center point of this circle (may be null). </param>
		/// <param name="radius">
		///            the initial non-negative radius of this circle in meters. </param>
		/// <param name="paintFill">
		///            the initial {@code Paint} used to fill this circle (may be null). </param>
		/// <param name="paintStroke">
		///            the initial {@code Paint} used to stroke this circle (may be null). </param>
		/// <param name="keepAligned">
		///            if set to true it will keep the bitmap aligned with the map,
		///            to avoid a moving effect of a bitmap shader. </param>
		/// <exception cref="IllegalArgumentException">
		///             if the given {@code radius} is negative or <seealso cref="Float#NaN"/>.
		///  </exception>
		public Circle(LatLong latLong, float radius, Paint paintFill, Paint paintStroke, bool keepAligned) : base()
		{
			this.keepAligned = keepAligned;
			this.latLong = latLong;
			RadiusInternal = radius;
			this.paintFill = paintFill;
			this.paintStroke = paintStroke;
		}

		public override void Draw(BoundingBox boundingBox, sbyte zoomLevel, Canvas canvas, Point topLeftPoint)
		{
			lock (this)
			{
				if (this.latLong == null || (this.paintStroke == null && this.paintFill == null))
				{
					return;
				}
        
				double latitude = this.latLong.Latitude;
				double longitude = this.latLong.Longitude;
				long mapSize = MercatorProjection.GetMapSize(zoomLevel, displayModel.TileSize);
				int pixelX = (int)(MercatorProjection.LongitudeToPixelX(longitude, mapSize) - topLeftPoint.X);
				int pixelY = (int)(MercatorProjection.LatitudeToPixelY(latitude, mapSize) - topLeftPoint.Y);
				int radiusInPixel = GetRadiusInPixels(latitude, zoomLevel);
        
				Rectangle canvasRectangle = new Rectangle(0, 0, canvas.Width, canvas.Height);
				if (!canvasRectangle.IntersectsCircle(pixelX, pixelY, radiusInPixel))
				{
					return;
				}
        
				if (this.paintStroke != null)
				{
					if (this.keepAligned)
					{
						this.paintStroke.SetBitmapShaderShift = topLeftPoint;
					}
					canvas.DrawCircle(pixelX, pixelY, radiusInPixel, this.paintStroke);
				}
				if (this.paintFill != null)
				{
					if (this.keepAligned)
					{
						this.paintFill.SetBitmapShaderShift = topLeftPoint;
					}
					canvas.DrawCircle(pixelX, pixelY, radiusInPixel, this.paintFill);
				}
			}
		}

		/// <returns> the {@code Paint} used to fill this circle (may be null). </returns>
		public virtual Paint PaintFill
		{
			get
			{
				lock (this)
				{
					return this.paintFill;
				}
			}
			set
			{
				lock (this)
				{
					this.paintFill = value;
				}
			}
		}

		/// <returns> the {@code Paint} used to stroke this circle (may be null). </returns>
		public virtual Paint PaintStroke
		{
			get
			{
				lock (this)
				{
					return this.paintStroke;
				}
			}
			set
			{
				lock (this)
				{
					this.paintStroke = value;
				}
			}
		}

		/// <returns> the center point of this circle (may be null). </returns>
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

		/// <returns> the non-negative radius of this circle in meters. </returns>
		public virtual float Radius
		{
			get
			{
				lock (this)
				{
					return this.radius;
				}
			}
			set
			{
				lock (this)
				{
					RadiusInternal = value;
				}
			}
		}

		/// <returns> the non-negative radius of this circle in pixels. </returns>
		protected internal virtual int GetRadiusInPixels(double latitude, sbyte zoomLevel)
		{
			return (int) MercatorProjection.MetersToPixels(this.radius, latitude, MercatorProjection.GetMapSize(zoomLevel, displayModel.TileSize));
		}

		/// <returns> true if it keeps the bitmap aligned with the map, to avoid a
		///         moving effect of a bitmap shader, false otherwise. </returns>
		public virtual bool KeepAligned
		{
			get
			{
				return keepAligned;
			}
		}

		/// <param name="latLong">
		///            the new center point of this circle (may be null). </param>
		public virtual LatLong LatLong
		{
			set
			{
				lock (this)
				{
					this.latLong = value;
				}
			}
		}

		private float RadiusInternal
		{
			set
			{
				if (value < 0 || float.IsNaN(value))
				{
					throw new System.ArgumentException("invalid radius: " + value);
				}
				this.radius = value;
			}
		}
	}
}