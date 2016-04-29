/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2015 devemux86
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

namespace org.mapsforge.map.layer.overlay
{
    using System.Collections.Generic;
    using core.util;

    using Canvas = org.mapsforge.core.graphics.Canvas;
    using GraphicFactory = org.mapsforge.core.graphics.GraphicFactory;
    using Paint = org.mapsforge.core.graphics.Paint;
    using Path = org.mapsforge.core.graphics.Path;
    using BoundingBox = org.mapsforge.core.model.BoundingBox;
    using LatLong = org.mapsforge.core.model.LatLong;
    using Point = org.mapsforge.core.model.Point;
    using MercatorProjection = org.mapsforge.core.util.MercatorProjection;

    /// <summary>
    /// A {@code Polyline} draws a connected series of line segments specified by a list of <seealso cref="LatLong LatLongs"/>.
    /// <para>
    /// A {@code Polyline} holds a <seealso cref="Paint"/> object which defines drawing parameters such as color, stroke width, pattern
    /// and transparency.
    /// </para>
    /// </summary>
    public class Polyline : Layer
	{
		private readonly GraphicFactory graphicFactory;
		private readonly bool keepAligned;
		private readonly IList<LatLong> latLongs = new CopyOnWriteArrayList<LatLong>();
		private Paint paintStroke;

		/// <param name="paintStroke">
		///            the initial {@code Paint} used to stroke this polyline (may be null). </param>
		/// <param name="graphicFactory">
		///            the GraphicFactory </param>
		public Polyline(Paint paintStroke, GraphicFactory graphicFactory) : this(paintStroke, graphicFactory, false)
		{
		}

		/// <param name="paintStroke">
		///            the initial {@code Paint} used to stroke this polyline (may be null). </param>
		/// <param name="graphicFactory">
		///            the GraphicFactory </param>
		/// <param name="keepAligned">
		///            if set to true it will keep the bitmap aligned with the map,
		///            to avoid a moving effect of a bitmap shader. </param>
		public Polyline(Paint paintStroke, GraphicFactory graphicFactory, bool keepAligned) : base()
		{

			this.keepAligned = keepAligned;
			this.paintStroke = paintStroke;
			this.graphicFactory = graphicFactory;
		}

		public override void Draw(BoundingBox boundingBox, sbyte zoomLevel, Canvas canvas, Point topLeftPoint)
		{
			lock (this)
			{
				if (this.latLongs.Count == 0 || this.paintStroke == null)
				{
					return;
				}
        
				IEnumerator<LatLong> iterator = this.latLongs.GetEnumerator();
				if (!iterator.MoveNext())
				{
					return;
				}
        
				LatLong latLong = iterator.Current;
				long mapSize = MercatorProjection.GetMapSize(zoomLevel, displayModel.TileSize);
				float x = (float)(MercatorProjection.LongitudeToPixelX(latLong.Longitude, mapSize) - topLeftPoint.X);
				float y = (float)(MercatorProjection.LatitudeToPixelY(latLong.Latitude, mapSize) - topLeftPoint.Y);
        
				Path path = this.graphicFactory.CreatePath();
				path.MoveTo(x, y);
        
				while (iterator.MoveNext())
				{
					latLong = iterator.Current;
					x = (float)(MercatorProjection.LongitudeToPixelX(latLong.Longitude, mapSize) - topLeftPoint.X);
					y = (float)(MercatorProjection.LatitudeToPixelY(latLong.Latitude, mapSize) - topLeftPoint.Y);
        
					path.LineTo(x, y);
				}
        
				if (this.keepAligned)
				{
					this.paintStroke.BitmapShaderShift = topLeftPoint;
				}
				canvas.DrawPath(path, this.paintStroke);
			}
		}

		/// <returns> a thread-safe list of LatLongs in this polyline. </returns>
		public virtual IList<LatLong> LatLongs
		{
			get
			{
				return this.latLongs;
			}
		}

		/// <returns> the {@code Paint} used to stroke this polyline (may be null). </returns>
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