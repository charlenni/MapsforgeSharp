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

namespace org.mapsforge.map.view
{
	using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
	using GraphicContext = MapsforgeSharp.Core.Graphics.GraphicContext;
	using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
	using Matrix = MapsforgeSharp.Core.Graphics.Matrix;
	using Dimension = MapsforgeSharp.Core.Model.Dimension;
	using MapPosition = MapsforgeSharp.Core.Model.MapPosition;
	using Point = MapsforgeSharp.Core.Model.Point;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;
	using FrameBufferModel = org.mapsforge.map.model.FrameBufferModel;

	public class FrameBuffer
	{
		private const bool IS_TRANSPARENT = false;
		private IBitmap bitmap1;
		private IBitmap bitmap2;
		private Dimension dimension;
		private readonly DisplayModel displayModel;
		private readonly FrameBufferModel frameBufferModel;
		private readonly GraphicFactory graphicFactory;
		private readonly Matrix matrix;

		public FrameBuffer(FrameBufferModel frameBufferModel, DisplayModel displayModel, GraphicFactory graphicFactory)
		{
			this.frameBufferModel = frameBufferModel;
			this.displayModel = displayModel;
			this.graphicFactory = graphicFactory;
			this.matrix = graphicFactory.CreateMatrix();
		}

		public virtual void AdjustMatrix(float diffX, float diffY, float scaleFactor, Dimension mapViewDimension, float pivotDistanceX, float pivotDistanceY)
		{
			lock (this)
			{
				if (this.dimension == null)
				{
					return;
				}
				this.matrix.Reset();
				CenterFrameBufferToMapView(mapViewDimension);
				if (pivotDistanceX == 0 && pivotDistanceY == 0)
				{
					// only translate the matrix if we are not zooming around a pivot,
					// the translation happens only once the zoom is finished.
					this.matrix.Translate(diffX, diffY);
				}
        
				Scale(scaleFactor, pivotDistanceX, pivotDistanceY);
			}
		}

		public virtual void Destroy()
		{
			lock (this)
			{
				DestroyBitmaps();
			}
		}

		public virtual void Draw(GraphicContext graphicContext)
		{
			lock (this)
			{
				graphicContext.FillColor(this.displayModel.BackgroundColor);
				if (this.bitmap1 != null)
				{
					graphicContext.DrawBitmap(this.bitmap1, this.matrix);
				}
			}
		}

		public virtual void FrameFinished(MapPosition frameMapPosition)
		{
			lock (this)
			{
				// swap both bitmap references
				IBitmap bitmapTemp = this.bitmap1;
				this.bitmap1 = this.bitmap2;
				this.bitmap2 = bitmapTemp;
			}
			// taking this out of the synchronized region removes a deadlock potential
			// at the small risk of an inconsistent zoom
			this.frameBufferModel.MapPosition = frameMapPosition;
		}

		public virtual Dimension Dimension
		{
			get
			{
				lock (this)
				{
					return this.dimension;
				}
			}
			set
			{
				lock (this)
				{
					if (this.dimension != null && this.dimension.Equals(value))
					{
						return;
					}
					this.dimension = value;
            
					DestroyBitmaps();
            
					if (value.Width > 0 && value.Height > 0)
					{
						this.bitmap1 = this.graphicFactory.CreateBitmap(value.Width, value.Height, IS_TRANSPARENT);
						this.bitmap2 = this.graphicFactory.CreateBitmap(value.Width, value.Height, IS_TRANSPARENT);
					}
				}
			}
		}

		/// <returns> the bitmap of the second frame to draw on (may be null). </returns>
		public virtual IBitmap DrawingBitmap
		{
			get
			{
				lock (this)
				{
					if (this.bitmap2 != null)
					{
						this.bitmap2.BackgroundColor = this.displayModel.BackgroundColor;
					}
					return this.bitmap2;
				}
			}
		}


		private void CenterFrameBufferToMapView(Dimension mapViewDimension)
		{
			float dx = (this.dimension.Width - mapViewDimension.Width) / -2f;
			float dy = (this.dimension.Height - mapViewDimension.Height) / -2f;
			this.matrix.Translate(dx, dy);
		}

		private void DestroyBitmaps()
		{
			if (this.bitmap1 != null)
			{
				this.bitmap1.DecrementRefCount();
				this.bitmap1 = null;
			}
			if (this.bitmap2 != null)
			{
				this.bitmap2.DecrementRefCount();
				this.bitmap2 = null;
			}
		}

		private void Scale(float scaleFactor, float pivotDistanceX, float pivotDistanceY)
		{
			if (scaleFactor != 1)
			{
				Point center = this.dimension.Center;
				float pivotX = (float)(pivotDistanceX + center.X);
				float pivotY = (float)(pivotDistanceY + center.Y);
				this.matrix.Scale(scaleFactor, scaleFactor, pivotX, pivotY);
			}
		}
	}
}