/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2014, 2015 devemux86
 * Copyright 2014 Erik Duisters
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

namespace org.mapsforge.map.scalebar
{
    using System;

    using Bitmap = org.mapsforge.core.graphics.Bitmap;
	using Canvas = org.mapsforge.core.graphics.Canvas;
	using GraphicContext = org.mapsforge.core.graphics.GraphicContext;
	using GraphicFactory = org.mapsforge.core.graphics.GraphicFactory;
	using MapPosition = org.mapsforge.core.model.MapPosition;
	using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;
	using MapViewDimension = org.mapsforge.map.model.MapViewDimension;
	using MapViewPosition = org.mapsforge.map.model.MapViewPosition;
	using MapView = org.mapsforge.map.view.MapView;

	/// <summary>
	/// A MapScaleBar displays the ratio of a distance on the map to the corresponding distance on the ground.
	/// </summary>
	public abstract class MapScaleBar
	{
		public enum ScaleBarPosition
		{
			BOTTOM_CENTER,
			BOTTOM_LEFT,
			BOTTOM_RIGHT,
			TOP_CENTER,
			TOP_LEFT,
			TOP_RIGHT
		}

		/// <summary>
		/// Default position of the scale bar.
		/// </summary>
		private const ScaleBarPosition DEFAULT_SCALE_BAR_POSITION = ScaleBarPosition.BOTTOM_LEFT;

		private const int DEFAULT_HORIZONTAL_MARGIN = 5;
		private const int DEFAULT_VERTICAL_MARGIN = 0;
		private const double LATITUDE_REDRAW_THRESHOLD = 0.2;

		protected internal readonly DisplayModel displayModel;
		protected internal DistanceUnitAdapter distanceUnitAdapter;
		protected internal readonly GraphicFactory graphicFactory;
		protected internal readonly Bitmap mapScaleBitmap;
		protected internal readonly Canvas mapScaleCanvas;
		private readonly MapViewDimension mapViewDimension;
		private readonly MapViewPosition mapViewPosition;
		private int marginHorizontal;
		private int marginVertical;
		private MapPosition prevMapPosition;
		protected internal bool redrawNeeded;
		protected internal ScaleBarPosition scaleBarPosition;
		private bool visible;

		/// <summary>
		/// Internal class used by calculateScaleBarLengthAndValue
		/// </summary>
		protected internal class ScaleBarLengthAndValue
		{
			public int scaleBarLength;
			public int scaleBarValue;

			public ScaleBarLengthAndValue(int scaleBarLength, int scaleBarValue)
			{
				this.scaleBarLength = scaleBarLength;
				this.scaleBarValue = scaleBarValue;
			}
		}

		public MapScaleBar(MapViewPosition mapViewPosition, MapViewDimension mapViewDimension, DisplayModel displayModel, GraphicFactory graphicFactory, int width, int height)
		{
			this.mapViewPosition = mapViewPosition;
			this.mapViewDimension = mapViewDimension;
			this.displayModel = displayModel;
			this.graphicFactory = graphicFactory;
			this.mapScaleBitmap = graphicFactory.CreateBitmap((int)(width * this.displayModel.ScaleFactor), (int)(height * this.displayModel.ScaleFactor));

			this.marginHorizontal = DEFAULT_HORIZONTAL_MARGIN;
			this.marginVertical = DEFAULT_VERTICAL_MARGIN;
			this.scaleBarPosition = DEFAULT_SCALE_BAR_POSITION;

			this.mapScaleCanvas = graphicFactory.CreateCanvas();
			this.mapScaleCanvas.Bitmap = this.mapScaleBitmap;
			this.distanceUnitAdapter = MetricUnitAdapter.INSTANCE;
			this.visible = true;
			this.redrawNeeded = true;
		}

		/// <summary>
		/// Free all resources
		/// </summary>
		public virtual void Destroy()
		{
			this.mapScaleBitmap.DecrementRefCount();
			this.mapScaleCanvas.Destroy();
		}

		/// <returns> true if this <seealso cref="MapScaleBar"/> is visible </returns>
		public virtual bool Visible
		{
			get
			{
				return this.visible;
			}
			set
			{
				this.visible = value;
			}
		}

		/// <returns> the <seealso cref="DistanceUnitAdapter"/> in use by this MapScaleBar </returns>
		public virtual DistanceUnitAdapter DistanceUnitAdapter
		{
			get
			{
				return this.distanceUnitAdapter;
			}
			set
			{
				if (value == null)
				{
					throw new System.ArgumentException("adapter must not be null");
				}
				this.distanceUnitAdapter = value;
				this.redrawNeeded = true;
			}
		}

		public virtual int MarginHorizontal
		{
			get
			{
				return marginHorizontal;
			}
			set
			{
				if (this.marginHorizontal != value)
				{
					this.marginHorizontal = value;
					this.redrawNeeded = true;
				}
			}
		}

		public virtual int MarginVertical
		{
			get
			{
				return marginVertical;
			}
			set
			{
				if (this.marginVertical != value)
				{
					this.marginVertical = value;
					this.redrawNeeded = true;
				}
			}
		}

		public virtual ScaleBarPosition GetScaleBarPosition()
		{
			return scaleBarPosition;
		}

		public virtual void SetScaleBarPosition(ScaleBarPosition scaleBarPosition)
		{
			if (this.scaleBarPosition != scaleBarPosition)
			{
				this.scaleBarPosition = scaleBarPosition;
				this.redrawNeeded = true;
			}
		}

		private int CalculatePositionLeft(int left, int right, int width)
		{
			switch (scaleBarPosition)
			{
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_LEFT:
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_LEFT:
				return marginHorizontal;

			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_CENTER:
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_CENTER:
				return (right - left - width) / 2;

			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_RIGHT:
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_RIGHT:
				return right - left - width - marginHorizontal;
			}

			throw new System.ArgumentException("unknown horizontal position: " + scaleBarPosition);
		}

		private int CalculatePositionTop(int top, int bottom, int height)
		{
			switch (scaleBarPosition)
			{
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_CENTER:
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_LEFT:
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.TOP_RIGHT:
				return marginVertical;

			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_CENTER:
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_LEFT:
			case org.mapsforge.map.scalebar.MapScaleBar.ScaleBarPosition.BOTTOM_RIGHT:
				return bottom - top - height - marginVertical;
			}

			throw new System.ArgumentException("unknown vertical position: " + scaleBarPosition);
		}

		/// <summary>
		/// Calculates the required length and value of the scalebar
		/// </summary>
		/// <param name="unitAdapter">
		///            the DistanceUnitAdapter to calculate for </param>
		/// <returns> a <seealso cref="ScaleBarLengthAndValue"/> object containing the required scaleBarLength and scaleBarValue </returns>
		protected internal virtual ScaleBarLengthAndValue CalculateScaleBarLengthAndValue(DistanceUnitAdapter unitAdapter)
		{
			this.prevMapPosition = this.mapViewPosition.MapPosition;
			double groundResolution = MercatorProjection.CalculateGroundResolution(this.prevMapPosition.LatLong.Latitude, MercatorProjection.GetMapSize(this.prevMapPosition.ZoomLevel, this.displayModel.TileSize));

			groundResolution = groundResolution / unitAdapter.MeterRatio;
			int[] scaleBarValues = unitAdapter.ScaleBarValues;

			int scaleBarLength = 0;
			int mapScaleValue = 0;

			for (int i = 0; i < scaleBarValues.Length; ++i)
			{
				mapScaleValue = scaleBarValues[i];
				scaleBarLength = (int)(mapScaleValue / groundResolution);
				if (scaleBarLength < (this.mapScaleBitmap.Width - 10))
				{
					break;
				}
			}

			return new ScaleBarLengthAndValue(scaleBarLength, mapScaleValue);
		}

		/// <summary>
		/// Calculates the required length and value of the scalebar using the current <seealso cref="DistanceUnitAdapter"/>
		/// </summary>
		/// <returns> a <seealso cref="ScaleBarLengthAndValue"/> object containing the required scaleBarLength and scaleBarValue </returns>
		protected internal virtual ScaleBarLengthAndValue CalculateScaleBarLengthAndValue()
		{
			return CalculateScaleBarLengthAndValue(this.distanceUnitAdapter);
		}

		/// <summary>
		/// Called from <seealso cref="MapView"/>
		/// </summary>
		/// <param name="graphicContext">
		///            The graphicContext to use to draw the MapScaleBar </param>
		public virtual void Draw(GraphicContext graphicContext)
		{
			if (!this.visible)
			{
				return;
			}

			if (this.mapViewDimension.Dimension == null)
			{
				return;
			}

			if (this.RedrawNecessary)
			{
				Redraw(this.mapScaleCanvas);
				this.redrawNeeded = false;
			}

			int positionLeft = CalculatePositionLeft(0, this.mapViewDimension.Dimension.Width, this.mapScaleBitmap.Width);
			int positionTop = CalculatePositionTop(0, this.mapViewDimension.Dimension.Height, this.mapScaleBitmap.Height);

			graphicContext.DrawBitmap(this.mapScaleBitmap, positionLeft, positionTop);
		}

		/// <summary>
		/// The scalebar will be redrawn on the next draw()
		/// </summary>
		public virtual void RedrawScaleBar()
		{
			this.redrawNeeded = true;
		}

		/// <summary>
		/// Determines if a redraw is necessary or not
		/// </summary>
		/// <returns> true if redraw is necessary, false otherwise </returns>
		protected internal virtual bool RedrawNecessary
		{
			get
			{
				if (this.redrawNeeded || this.prevMapPosition == null)
				{
					return true;
				}
    
				MapPosition currentMapPosition = this.mapViewPosition.MapPosition;
				if (currentMapPosition.ZoomLevel != this.prevMapPosition.ZoomLevel)
				{
					return true;
				}
    
				double latitudeDiff = Math.Abs(currentMapPosition.LatLong.Latitude - this.prevMapPosition.LatLong.Latitude);
				return latitudeDiff > LATITUDE_REDRAW_THRESHOLD;
			}
		}

		/// <summary>
		/// Redraw the mapScaleBar. Make sure you always apply this.displayModel.getScaleFactor() to all coordinates and
		/// dimensions.
		/// </summary>
		/// <param name="canvas">
		///            The canvas to draw on </param>
		protected internal abstract void Redraw(Canvas canvas);
	}
}