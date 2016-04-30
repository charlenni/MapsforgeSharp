/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2015 devemux86
 * Copyright 2015 Andreas Schildbach
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

namespace org.mapsforge.map.model
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using BoundingBox = org.mapsforge.core.model.BoundingBox;
    using LatLong = org.mapsforge.core.model.LatLong;
    using MapPosition = org.mapsforge.core.model.MapPosition;
    using Point = org.mapsforge.core.model.Point;
    using MercatorProjection = org.mapsforge.core.util.MercatorProjection;
    using Observable = org.mapsforge.map.model.common.Observable;
    using Persistable = org.mapsforge.map.model.common.Persistable;
    using PreferencesFacade = org.mapsforge.map.model.common.PreferencesFacade;
    using PausableThread = org.mapsforge.map.util.PausableThread;

    public class MapViewPosition : Observable, Persistable
	{
		private class ZoomAnimator : PausableThread
		{
			private readonly MapViewPosition outerInstance;

			public ZoomAnimator(MapViewPosition outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			// debugging tip: for investigating what happens during the zoom animation
			// just make the times longer for duration and frame length
			internal const int DEFAULT_DURATION = 250;
			internal const int FRAME_LENGTH_IN_MS = 15;

			internal double scaleDifference;
			internal double startScaleFactor;
			internal bool executeAnimation;
			internal long timeEnd;
			internal long timeStart;

			protected internal override void DoWork()
			{
				if (DateTimeHelperClass.CurrentUnixTimeMillis() >= this.timeEnd)
				{
					this.executeAnimation = false;
					outerInstance.ScaleFactor = calculateScaleFactor(1);
					outerInstance.Pivot = null;
				}
				else
				{
					float timeElapsedRatio = (DateTimeHelperClass.CurrentUnixTimeMillis() - this.timeStart) / (1f * DEFAULT_DURATION);
					outerInstance.ScaleFactor = calculateScaleFactor(timeElapsedRatio);
				}
				Sleep(FRAME_LENGTH_IN_MS);
			}

			protected internal override ThreadPriority GetThreadPriority()
			{
				return ThreadPriority.ABOVE_NORMAL;
			}

			protected internal override bool HasWork()
			{
				return this.executeAnimation;
			}

			internal virtual void startAnimation(double startScaleFactor, double targetScaleFactor)
			{
				// TODO this is not properly synchronized
				this.startScaleFactor = startScaleFactor;
				this.scaleDifference = targetScaleFactor - this.startScaleFactor;
				this.executeAnimation = true;
				this.timeStart = DateTimeHelperClass.CurrentUnixTimeMillis();
				this.timeEnd = this.timeStart + DEFAULT_DURATION;
				lock (this)
				{
					Monitor.Pulse(this);
				}
			}

			internal virtual double calculateScaleFactor(float percent)
			{
				return this.startScaleFactor + this.scaleDifference * percent;
			}
		}

		private const string LATITUDE = "latitude";
		private const string LATITUDE_MAX = "latitudeMax";
		private const string LATITUDE_MIN = "latitudeMin";
		private const string LONGITUDE = "longitude";
		private const string LONGITUDE_MAX = "longitudeMax";
		private const string LONGITUDE_MIN = "longitudeMin";
		private const string ZOOM_LEVEL = "zoomLevel";
		private const string ZOOM_LEVEL_MAX = "zoomLevelMax";
		private const string ZOOM_LEVEL_MIN = "zoomLevelMin";

		private static bool isNan(params double[] values)
		{
			foreach (double value in values)
			{
				if (double.IsNaN(value))
				{
					return true;
				}
			}

			return false;
		}

		private readonly DisplayModel displayModel;
		private double latitude;
		private double longitude;
		private BoundingBox mapLimit;
		private LatLong pivot;
		private double scaleFactor;
		private readonly ZoomAnimator zoomAnimator;
		private sbyte zoomLevel;
		private sbyte zoomLevelMax;
		private sbyte zoomLevelMin;

		public MapViewPosition(DisplayModel displayModel) : base()
		{
			this.displayModel = displayModel;
			this.zoomLevelMax = sbyte.MaxValue;
			this.zoomAnimator = new ZoomAnimator(this);
			this.zoomAnimator.Start();
		}

		/// <summary>
		/// Start animating the map towards the given point.
		/// </summary>
		public virtual void AnimateTo(LatLong pos)
		{
			new Task(async () =>
			{
				int totalSteps = 25; // Define the Step Number
				int signX = 1; // Define the Sign for Horizontal Movement
				int signY = 1; // Define the Sign for Vertical Movement
				long mapSize = MercatorProjection.GetMapSize(ZoomLevel, displayModel.TileSize);

				double targetPixelX = MercatorProjection.LongitudeToPixelX(pos.Longitude, mapSize);
				double targetPixelY = MercatorProjection.LatitudeToPixelY(pos.Latitude, mapSize);

				double currentPixelX = MercatorProjection.LongitudeToPixelX(longitude, mapSize);
				double currentPixelY = MercatorProjection.LatitudeToPixelY(latitude, mapSize);

				double stepSizeX = Math.Abs(targetPixelX - currentPixelX) / totalSteps;
				double stepSizeY = Math.Abs(targetPixelY - currentPixelY) / totalSteps;

				/* Check the Signs */
				if (currentPixelX < targetPixelX)
				{
					signX = -1;
				}

				if (currentPixelY < targetPixelY)
				{
					signY = -1;
				}

				/* Compute Scroll */
				for (int i = 0; i < totalSteps; i++)
				{
					MoveCenter(stepSizeX * signX, stepSizeY * signY);
					await Task.Delay(10);
				}
			}).Start();
		}

		public virtual bool AnimationInProgress()
		{
			return this.scaleFactor != MercatorProjection.ZoomLevelToScaleFactor(this.zoomLevel);
		}

		public virtual void Destroy()
		{
			this.zoomAnimator.Interrupt();
		}

		/// <returns> the current center position of the map. </returns>
		public virtual LatLong Center
		{
			get
			{
				lock (this)
				{
					return new LatLong(this.latitude, this.longitude);
				}
			}
			set
			{
				lock (this)
				{
					setCenterInternal(value.Latitude, value.Longitude);
				}
				NotifyObservers();
			}
		}

		/// <returns> the current limit of the map (might be null). </returns>
		public virtual BoundingBox MapLimit
		{
			get
			{
				lock (this)
				{
					return this.mapLimit;
				}
			}
			set
			{
				lock (this)
				{
					this.mapLimit = value;
				}
				NotifyObservers();
			}
		}

		/// <returns> the current center position and zoom level of the map. </returns>
		public virtual MapPosition MapPosition
		{
			get
			{
				lock (this)
				{
					return new MapPosition(Center, this.zoomLevel);
				}
			}
			set
			{
				SetMapPosition(value, true);
			}
		}

		/// <summary>
		/// The pivot point is the point the map zooms around.
		/// </summary>
		/// <returns> the lat/long coordinates of the map pivot point if set or null otherwise. </returns>
		public virtual LatLong Pivot
		{
			get
			{
				lock (this)
				{
					return this.pivot;
				}
			}
			set
			{
				lock (this)
				{
					this.pivot = value;
				}
			}
		}

		/// <summary>
		/// The pivot point is the point the map zooms around. If the map zooms around its center null is returned, otherwise
		/// the zoom-specific x/y pixel coordinates for the MercatorProjection (note: not the x/y coordinates for the map
		/// view or the frame buffer, the MapViewPosition knows nothing about them).
		/// </summary>
		/// <param name="zoomLevel"> the zoomlevel to compute the x/y coordinates for </param>
		/// <returns> the x/y coordinates of the map pivot point if set or null otherwise. </returns>
		public virtual Point GetPivotXY(sbyte zoomLevel)
		{
			lock (this)
			{
				if (this.pivot != null)
				{
					return MercatorProjection.GetPixel(this.pivot, MercatorProjection.GetMapSize(zoomLevel, displayModel.TileSize));
				}
				return null;
			}
		}

		public virtual double ScaleFactor
		{
			get
			{
				lock (this)
				{
					return this.scaleFactor;
				}
			}
			set
			{
				lock (this)
				{
					this.scaleFactor = value;
				}
				NotifyObservers();
			}
		}

		/// <returns> the current zoom level of the map. </returns>
		public virtual sbyte ZoomLevel
		{
			get
			{
				lock (this)
				{
					return this.zoomLevel;
				}
			}
			set
			{
				SetZoomLevel(value, true);
			}
		}

		public virtual sbyte ZoomLevelMax
		{
			get
			{
				lock (this)
				{
					return this.zoomLevelMax;
				}
			}
			set
			{
				if (value < 0)
				{
					throw new System.ArgumentException("zoomLevelMax must not be negative: " + value);
				}
				lock (this)
				{
					if (value < this.zoomLevelMin)
					{
						throw new System.ArgumentException("zoomLevelMax must be >= zoomLevelMin: " + value);
					}
					this.zoomLevelMax = value;
				}
				NotifyObservers();
			}
		}

		public virtual sbyte ZoomLevelMin
		{
			get
			{
				lock (this)
				{
					return this.zoomLevelMin;
				}
			}
			set
			{
				if (value < 0)
				{
					throw new System.ArgumentException("zoomLevelMin must not be negative: " + value);
				}
				lock (this)
				{
					if (value > this.zoomLevelMax)
					{
						throw new System.ArgumentException("zoomLevelMin must be <= zoomLevelMax: " + value);
					}
					this.zoomLevelMin = value;
				}
				NotifyObservers();
			}
		}

		public void Init(PreferencesFacade preferencesFacade)
		{
			lock (this)
			{
				this.latitude = preferencesFacade.GetDouble(LATITUDE, 0);
				this.longitude = preferencesFacade.GetDouble(LONGITUDE, 0);
        
				double maxLatitude = preferencesFacade.GetDouble(LATITUDE_MAX, Double.NaN);
				double minLatitude = preferencesFacade.GetDouble(LATITUDE_MIN, Double.NaN);
				double maxLongitude = preferencesFacade.GetDouble(LONGITUDE_MAX, Double.NaN);
				double minLongitude = preferencesFacade.GetDouble(LONGITUDE_MIN, Double.NaN);
        
				if (isNan(maxLatitude, minLatitude, maxLongitude, minLongitude))
				{
					this.mapLimit = null;
				}
				else
				{
					this.mapLimit = new BoundingBox(minLatitude, minLongitude, maxLatitude, maxLongitude);
				}
        
				this.zoomLevel = preferencesFacade.GetByte(ZOOM_LEVEL, (sbyte) 0);
				this.zoomLevelMax = preferencesFacade.GetByte(ZOOM_LEVEL_MAX, sbyte.MaxValue);
				this.zoomLevelMin = preferencesFacade.GetByte(ZOOM_LEVEL_MIN, (sbyte) 0);
				this.scaleFactor = Math.Pow(2, this.zoomLevel);
			}
		}

		/// <summary>
		/// Animates the center position of the map by the given amount of pixels.
		/// </summary>
		/// <param name="moveHorizontal">
		///            the amount of pixels to move this MapViewPosition horizontally. </param>
		/// <param name="moveVertical">
		///            the amount of pixels to move this MapViewPosition vertically. </param>
		public virtual void MoveCenter(double moveHorizontal, double moveVertical)
		{
			this.MoveCenterAndZoom(moveHorizontal, moveVertical, (sbyte) 0, true);
		}

		/// <summary>
		/// Moves the center position of the map by the given amount of pixels.
		/// </summary>
		/// <param name="moveHorizontal">
		///            the amount of pixels to move this MapViewPosition horizontally. </param>
		/// <param name="moveVertical">
		///            the amount of pixels to move this MapViewPosition vertically. </param>
		/// <param name="animated">
		///            whether the move should be animated. </param>
		public virtual void MoveCenter(double moveHorizontal, double moveVertical, bool animated)
		{
			this.MoveCenterAndZoom(moveHorizontal, moveVertical, (sbyte) 0, animated);
		}

		/// <summary>
		/// Animates the center position of the map by the given amount of pixels.
		/// </summary>
		/// <param name="moveHorizontal">
		///            the amount of pixels to move this MapViewPosition horizontally. </param>
		/// <param name="moveVertical">
		///            the amount of pixels to move this MapViewPosition vertically. </param>
		/// <param name="zoomLevelDiff">
		///            the difference in desired zoom level. </param>
		public virtual void MoveCenterAndZoom(double moveHorizontal, double moveVertical, sbyte zoomLevelDiff)
		{
			MoveCenterAndZoom(moveHorizontal, moveVertical, zoomLevelDiff, true);
		}

		/// <summary>
		/// Moves the center position of the map by the given amount of pixels.
		/// </summary>
		/// <param name="moveHorizontal">
		///            the amount of pixels to move this MapViewPosition horizontally. </param>
		/// <param name="moveVertical">
		///            the amount of pixels to move this MapViewPosition vertically. </param>
		/// <param name="zoomLevelDiff">
		///            the difference in desired zoom level. </param>
		/// <param name="animated">
		///            whether the move should be animated. </param>
		public virtual void MoveCenterAndZoom(double moveHorizontal, double moveVertical, sbyte zoomLevelDiff, bool animated)
		{
			lock (this)
			{
				long mapSize = MercatorProjection.GetMapSize(this.zoomLevel, this.displayModel.TileSize);
				double pixelX = MercatorProjection.LongitudeToPixelX(this.longitude, mapSize) - moveHorizontal;
				double pixelY = MercatorProjection.LatitudeToPixelY(this.latitude, mapSize) - moveVertical;

				pixelX = Math.Min(Math.Max(0, pixelX), mapSize);
				pixelY = Math.Min(Math.Max(0, pixelY), mapSize);

				double newLatitude = MercatorProjection.PixelYToLatitude(pixelY, mapSize);
				double newLongitude = MercatorProjection.PixelXToLongitude(pixelX, mapSize);
				setCenterInternal(newLatitude, newLongitude);
				setZoomLevelInternal(this.zoomLevel + zoomLevelDiff, animated);
			}
			NotifyObservers();
		}

		public void Save(PreferencesFacade preferencesFacade)
		{
			lock (this)
			{
				preferencesFacade.PutDouble(LATITUDE, this.latitude);
				preferencesFacade.PutDouble(LONGITUDE, this.longitude);
        
				if (this.mapLimit == null)
				{
					preferencesFacade.PutDouble(LATITUDE_MAX, Double.NaN);
					preferencesFacade.PutDouble(LATITUDE_MIN, Double.NaN);
					preferencesFacade.PutDouble(LONGITUDE_MAX, Double.NaN);
					preferencesFacade.PutDouble(LONGITUDE_MIN, Double.NaN);
				}
				else
				{
					preferencesFacade.PutDouble(LATITUDE_MAX, this.mapLimit.MaxLatitude);
					preferencesFacade.PutDouble(LATITUDE_MIN, this.mapLimit.MinLatitude);
					preferencesFacade.PutDouble(LONGITUDE_MAX, this.mapLimit.MaxLongitude);
					preferencesFacade.PutDouble(LONGITUDE_MIN, this.mapLimit.MinLongitude);
				}
        
				preferencesFacade.PutByte(ZOOM_LEVEL, this.zoomLevel);
				preferencesFacade.PutByte(ZOOM_LEVEL_MAX, this.zoomLevelMax);
				preferencesFacade.PutByte(ZOOM_LEVEL_MIN, this.zoomLevelMin);
			}
		}

		/// <summary>
		/// Sets the new center position and zoom level of the map.
		/// </summary>
		public virtual void SetMapPosition(MapPosition mapPosition, bool animated)
		{
			lock (this)
			{
				setCenterInternal(mapPosition.LatLong.Latitude, mapPosition.LatLong.Longitude);
				setZoomLevelInternal(mapPosition.ZoomLevel, animated);
			}
			NotifyObservers();
		}

		public virtual double ScaleFactorAdjustment
		{
			set
			{
				lock (this)
				{
					this.ScaleFactor = Math.Pow(2, zoomLevel) * value;
				}
				NotifyObservers();
			}
		}

		/// <summary>
		/// Sets the new zoom level of the map
		/// </summary>
		/// <param name="zoomLevel">
		///            desired zoom level </param>
		/// <param name="animated">
		///            true if the transition should be animated, false otherwise </param>
		/// <exception cref="IllegalArgumentException">
		///             if the zoom level is negative. </exception>
		public virtual void SetZoomLevel(sbyte zoomLevel, bool animated)
		{
			if (zoomLevel < 0)
			{
				throw new System.ArgumentException("zoomLevel must not be negative: " + zoomLevel);
			}
			lock (this)
			{
				setZoomLevelInternal(zoomLevel, animated);
			}
			NotifyObservers();
		}

		/// <summary>
		/// Changes the current zoom level by the given value if possible.
		/// <p/>
		/// Note: The default zoom level changes are animated.
		/// </summary>
		public virtual void Zoom(sbyte zoomLevelDiff)
		{
			zoom(zoomLevelDiff, true);
		}

		/// <summary>
		/// Changes the current zoom level by the given value if possible.
		/// </summary>
		public virtual void zoom(sbyte zoomLevelDiff, bool animated)
		{
			lock (this)
			{
				setZoomLevelInternal(this.zoomLevel + zoomLevelDiff, animated);
			}
			NotifyObservers();
		}

		/// <summary>
		/// Increases the current zoom level by one if possible.
		/// <p/>
		/// Note: The default zoom level changes are animated.
		/// </summary>
		public virtual void zoomIn()
		{
			zoomIn(true);
		}

		/// <summary>
		/// Increases the current zoom level by one if possible.
		/// </summary>
		public virtual void zoomIn(bool animated)
		{
			zoom((sbyte) 1, animated);
		}

		/// <summary>
		/// Decreases the current zoom level by one if possible.
		/// <p/>
		/// Note: The default zoom level changes are animated.
		/// </summary>
		public virtual void zoomOut()
		{
			zoomOut(true);
		}

		/// <summary>
		/// Decreases the current zoom level by one if possible.
		/// </summary>
		public virtual void zoomOut(bool animated)
		{
			zoom((sbyte) -1, animated);
		}

		private void setCenterInternal(double latitude, double longitude)
		{
			if (this.mapLimit == null)
			{
				this.latitude = latitude;
				this.longitude = longitude;
			}
			else
			{
				this.latitude = Math.Max(Math.Min(latitude, this.mapLimit.MaxLatitude), this.mapLimit.MinLatitude);
				this.longitude = Math.Max(Math.Min(longitude, this.mapLimit.MaxLongitude), this.mapLimit.MinLongitude);
			}
		}

		private void setZoomLevelInternal(int zoomLevel, bool animated)
		{
			this.zoomLevel = (sbyte) Math.Max(Math.Min(zoomLevel, this.zoomLevelMax), this.zoomLevelMin);
			if (animated)
			{
				this.zoomAnimator.startAnimation(ScaleFactor, Math.Pow(2, this.zoomLevel));
			}
			else
			{
				this.ScaleFactor = Math.Pow(2, this.zoomLevel);
				this.Pivot = null;
			}
		}
	}
}