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
    using System.Collections.Generic;
    using core.util;

    using Canvas = MapsforgeSharp.Core.Graphics.Canvas;
    using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
    using IPaint = MapsforgeSharp.Core.Graphics.IPaint;
    using IPath = MapsforgeSharp.Core.Graphics.IPath;
    using BoundingBox = MapsforgeSharp.Core.Model.BoundingBox;
    using LatLong = MapsforgeSharp.Core.Model.LatLong;
    using Point = MapsforgeSharp.Core.Model.Point;
    using MercatorProjection = MapsforgeSharp.Core.Util.MercatorProjection;

    /// <summary>
    /// A {@code Polygon} draws a closed connected series of line segments specified by a list of <seealso cref="LatLong LatLongs"/>.
    /// If the first and the last {@code LatLong} are not equal, the {@code Polygon} will be closed automatically.
    /// <para>
    /// A {@code Polygon} holds two <seealso cref="IPaint"/> objects to allow for different outline and filling. These paints define
    /// drawing parameters such as color, stroke width, pattern and transparency.
    /// </para>
    /// </summary>
    public class Polygon : Layer
	{
		private readonly IGraphicFactory graphicFactory;
		private readonly bool keepAligned;
		private readonly IList<LatLong> latLongs = new CopyOnWriteArrayList<LatLong>();
		private IPaint paintFill;
		private IPaint paintStroke;

		/// <param name="paintFill">
		///            the initial {@code Paint} used to fill this polygon (may be null). </param>
		/// <param name="paintStroke">
		///            the initial {@code Paint} used to stroke this polygon (may be null). </param>
		/// <param name="graphicFactory">
		///            the IGraphicFactory </param>
		public Polygon(IPaint paintFill, IPaint paintStroke, IGraphicFactory graphicFactory) : this(paintFill, paintStroke, graphicFactory, false)
		{
		}

		/// <param name="paintFill">
		///            the initial {@code Paint} used to fill this polygon (may be null). </param>
		/// <param name="paintStroke">
		///            the initial {@code Paint} used to stroke this polygon (may be null). </param>
		/// <param name="graphicFactory">
		///            the IGraphicFactory </param>
		/// <param name="keepAligned">
		///            if set to true it will keep the bitmap aligned with the map,
		///            to avoid a moving effect of a bitmap shader. </param>
		public Polygon(IPaint paintFill, IPaint paintStroke, IGraphicFactory graphicFactory, bool keepAligned) : base()
		{
			this.keepAligned = keepAligned;
			this.paintFill = paintFill;
			this.paintStroke = paintStroke;
			this.graphicFactory = graphicFactory;
		}

		public override void Draw(BoundingBox boundingBox, sbyte zoomLevel, Canvas canvas, Point topLeftPoint)
		{
			lock (this)
			{
				if (this.latLongs.Count < 2 || (this.paintStroke == null && this.paintFill == null))
				{
					return;
				}
        
				IEnumerator<LatLong> iterator = this.latLongs.GetEnumerator();
        
				IPath path = this.graphicFactory.CreatePath();
				LatLong latLong = iterator.Current;
				long mapSize = MercatorProjection.GetMapSize(zoomLevel, displayModel.TileSize);
				float x = (float)(MercatorProjection.LongitudeToPixelX(latLong.Longitude, mapSize) - topLeftPoint.X);
				float y = (float)(MercatorProjection.LatitudeToPixelY(latLong.Latitude, mapSize) - topLeftPoint.Y);
				path.MoveTo(x, y);
        
				while (iterator.MoveNext())
				{
					latLong = iterator.Current;
					x = (float)(MercatorProjection.LongitudeToPixelX(latLong.Longitude, mapSize) - topLeftPoint.X);
					y = (float)(MercatorProjection.LatitudeToPixelY(latLong.Latitude, mapSize) - topLeftPoint.Y);
					path.LineTo(x, y);
				}
        
				if (this.paintStroke != null)
				{
					if (this.keepAligned)
					{
						this.paintStroke.SetBitmapShaderShift = topLeftPoint;
					}
					canvas.DrawPath(path, this.paintStroke);
				}

				if (this.paintFill != null)
				{
					if (this.keepAligned)
					{
						this.paintFill.SetBitmapShaderShift = topLeftPoint;
					}
        
					canvas.DrawPath(path, this.paintFill);
				}
			}
		}

		/// <returns> a thread-safe list of LatLongs in this polygon. </returns>
		public virtual IList<LatLong> LatLongs
		{
			get
			{
				return this.latLongs;
			}
		}

		/// <returns> the {@code Paint} used to fill this polygon (may be null). </returns>
		public virtual IPaint PaintFill
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

		/// <returns> the {@code Paint} used to stroke this polygon (may be null). </returns>
		public virtual IPaint PaintStroke
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

		/// <returns> true if it keeps the bitmap aligned with the map, to avoid a
		///         moving effect of a bitmap shader, false otherwise. </returns>
		public virtual bool KeepAligned
		{
			get
			{
				return keepAligned;
			}
		}
	}
}