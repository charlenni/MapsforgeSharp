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

namespace org.mapsforge.map.controller
{
    using System;

    using Dimension = MapsforgeSharp.Core.Model.Dimension;
	using LatLong = MapsforgeSharp.Core.Model.LatLong;
	using MapPosition = MapsforgeSharp.Core.Model.MapPosition;
	using Point = MapsforgeSharp.Core.Model.Point;
	using MercatorProjection = MapsforgeSharp.Core.Util.MercatorProjection;
	using Model = org.mapsforge.map.model.Model;
	using Observer = org.mapsforge.map.model.common.Observer;
	using FrameBuffer = org.mapsforge.map.view.FrameBuffer;

	public sealed class FrameBufferController : Observer
	{
		private static float maxAspectRatio = 2;
		// if useSquareFrameBuffer is enabled, the framebuffer allocated for drawing will be
		// large enough for drawing in either orientation, so no change is needed when the device
		// orientation changes. To avoid overly large framebuffers, the aspect ratio for this policy
		// determines when this will be used.
		private static bool useSquareFrameBuffer = true;

		public static FrameBufferController Create(FrameBuffer frameBuffer, Model model)
		{
			FrameBufferController frameBufferController = new FrameBufferController(frameBuffer, model);

			model.frameBufferModel.AddObserver(frameBufferController);
			model.mapViewDimension.AddObserver(frameBufferController);
			model.mapViewPosition.AddObserver(frameBufferController);
			model.displayModel.AddObserver(frameBufferController);

			return frameBufferController;
		}

		public static Dimension CalculateFrameBufferDimension(Dimension mapViewDimension, double overdrawFactor)
		{
			int width = (int)(mapViewDimension.Width * overdrawFactor);
			int height = (int)(mapViewDimension.Height * overdrawFactor);
			if (useSquareFrameBuffer)
			{
				float aspectRatio = ((float) mapViewDimension.Width) / mapViewDimension.Height;
				if (aspectRatio < maxAspectRatio && aspectRatio > 1 / maxAspectRatio)
				{
					width = Math.Max(width, height);
					height = width;
				}
			}
			return new Dimension(width, height);
		}

		public static bool UseSquareFrameBuffer
		{
			get
			{
				return useSquareFrameBuffer;
			}
			set
			{
				FrameBufferController.useSquareFrameBuffer = value;
			}
		}

		private readonly FrameBuffer frameBuffer;
		private Dimension lastMapViewDimension;
		private double lastOverdrawFactor;
		private readonly Model model;

		private FrameBufferController(FrameBuffer frameBuffer, Model model)
		{
			this.frameBuffer = frameBuffer;
			this.model = model;
		}

		public void Destroy()
		{
			this.model.mapViewPosition.RemoveObserver(this);
			this.model.mapViewDimension.RemoveObserver(this);
			this.model.frameBufferModel.RemoveObserver(this);
		}

		public void OnChange()
		{
			Dimension mapViewDimension = this.model.mapViewDimension.Dimension;
			if (mapViewDimension == null)
			{
				// at this point map view not visible
				return;
			}

			double overdrawFactor = this.model.frameBufferModel.OverdrawFactor;
			if (DimensionChangeNeeded(mapViewDimension, overdrawFactor))
			{
				Dimension newDimension = CalculateFrameBufferDimension(mapViewDimension, overdrawFactor);
				if (!useSquareFrameBuffer || frameBuffer.Dimension == null || newDimension.Width > frameBuffer.Dimension.Width || newDimension.Height > frameBuffer.Dimension.Height)
				{
					// new dimensions if we either always reallocate on config change or if new dimension
					// is larger than the old
					this.frameBuffer.Dimension = newDimension;
				}
				this.lastMapViewDimension = mapViewDimension;
				this.lastOverdrawFactor = overdrawFactor;
			}

			lock (this.model.mapViewPosition)
			{
				lock (this.frameBuffer)
				{
					// we need resource ordering here to avoid deadlock
					MapPosition mapPositionFrameBuffer = this.model.frameBufferModel.MapPosition;
					if (mapPositionFrameBuffer != null)
					{
						double scaleFactor = this.model.mapViewPosition.ScaleFactor;
						LatLong pivot = this.model.mapViewPosition.Pivot;
						AdjustFrameBufferMatrix(mapPositionFrameBuffer, mapViewDimension, scaleFactor, pivot);
					}
				}
			}
		}

		private void AdjustFrameBufferMatrix(MapPosition mapPositionFrameBuffer, Dimension mapViewDimension, double scaleFactor, LatLong pivot)
		{
			MapPosition mapViewPosition = this.model.mapViewPosition.MapPosition;

			long mapSize = MercatorProjection.GetMapSize(mapPositionFrameBuffer.ZoomLevel, model.displayModel.TileSize);

			Point pointFrameBuffer = MercatorProjection.GetPixel(mapPositionFrameBuffer.LatLong, mapSize);
			Point pointMapPosition = MercatorProjection.GetPixel(mapViewPosition.LatLong, mapSize);

			double diffX = pointFrameBuffer.X - pointMapPosition.X;
			double diffY = pointFrameBuffer.Y - pointMapPosition.Y;
			// we need to compute the pivot distance from the map center
			// as we will need to find the pivot point for the
			// frame buffer (which generally has not the same size as the
			// map view).
			double pivotDistanceX = 0d;
			double pivotDistanceY = 0d;
			if (pivot != null)
			{
				Point pivotXY = MercatorProjection.GetPixel(pivot, mapSize);
				pivotDistanceX = pivotXY.X - pointFrameBuffer.X;
				pivotDistanceY = pivotXY.Y - pointFrameBuffer.Y;
			}

			float currentScaleFactor = (float)(scaleFactor / Math.Pow(2, mapPositionFrameBuffer.ZoomLevel));

			this.frameBuffer.AdjustMatrix((float) diffX, (float) diffY, currentScaleFactor, mapViewDimension, (float) pivotDistanceX, (float) pivotDistanceY);
		}

		private bool DimensionChangeNeeded(Dimension mapViewDimension, double overdrawFactor)
		{
			if (overdrawFactor.CompareTo(this.lastOverdrawFactor) != 0)
			{
				return true;
			}
			else if (!mapViewDimension.Equals(this.lastMapViewDimension))
			{
				return true;
			}
			return false;
		}
	}
}