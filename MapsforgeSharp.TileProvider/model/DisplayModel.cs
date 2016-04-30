/*
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

namespace org.mapsforge.map.model
{
    using System;

    using Observable = org.mapsforge.map.model.common.Observable;

	/// <summary>
	/// Encapsulates the display characteristics for a MapView, such as tile size and background color. The size of map tiles
	/// is used to adapt to devices with differing pixel densities and users with different preferences: The larger the tile,
	/// the larger everything is rendered, the effect is one of effectively stretching everything. The default device
	/// dependent scale factor is determined at the GraphicFactory level, while the DisplayModel allows further adaptation to
	/// cater for user needs or application development (maybe a small map and large map, or to prevent upscaling for
	/// downloaded tiles that do not scale well).
	/// </summary>
	public class DisplayModel : Observable
	{

		private const int DEFAULT_BACKGROUND_COLOR = unchecked((int)0xffeeeeee); // format AARRGGBB
		private const int DEFAULT_TILE_SIZE = 256;
		private const float DEFAULT_MAX_TEXT_WIDTH_FACTOR = 0.7f;
		private static readonly int DEFAULT_MAX_TEXT_WIDTH = (int)(DEFAULT_TILE_SIZE * DEFAULT_MAX_TEXT_WIDTH_FACTOR);

		private static float defaultUserScaleFactor = 1f;
		private static float deviceScaleFactor = 1f;

		/// <summary>
		/// Get the default scale factor for all newly created DisplayModels.
		/// </summary>
		/// <returns> the default scale factor to be applied to all new DisplayModels. </returns>
		public static float DefaultUserScaleFactor
		{
			get
			{
				lock (typeof(DisplayModel))
				{
					return defaultUserScaleFactor;
				}
			}
			set
			{
				lock (typeof(DisplayModel))
				{
					defaultUserScaleFactor = value;
				}
			}
		}

		/// <summary>
		/// Returns the device scale factor.
		/// </summary>
		/// <returns> the device scale factor. </returns>
		public static float DeviceScaleFactor
		{
			get
			{
				lock (typeof(DisplayModel))
				{
					return deviceScaleFactor;
				}
			}
			set
			{
				lock (typeof(DisplayModel))
				{
					deviceScaleFactor = value;
				}
			}
		}

		private int backgroundColor = DEFAULT_BACKGROUND_COLOR;
		private int fixedTileSize;
		private int maxTextWidth = DEFAULT_MAX_TEXT_WIDTH;
		private float maxTextWidthFactor = DEFAULT_MAX_TEXT_WIDTH_FACTOR;
		private int tileSize = DEFAULT_TILE_SIZE;
		private int tileSizeMultiple = 64;
		private float userScaleFactor = defaultUserScaleFactor;

		public DisplayModel() : base()
		{
			this.setTileSize();
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (!(obj is DisplayModel))
			{
				return false;
			}
			DisplayModel other = (DisplayModel) obj;
			if (this.backgroundColor != other.backgroundColor)
			{
				return false;
			}
			if (this.fixedTileSize != other.fixedTileSize)
			{
				return false;
			}
			if (this.maxTextWidth != other.maxTextWidth)
			{
				return false;
			}
			if (BitConverter.ToInt32(BitConverter.GetBytes(this.maxTextWidthFactor), 0) != BitConverter.ToInt32(BitConverter.GetBytes(other.maxTextWidthFactor), 0))
			{
				return false;
			}
			if (this.tileSize != other.tileSize)
			{
				return false;
			}
			if (this.tileSizeMultiple != other.tileSizeMultiple)
			{
				return false;
			}
			if (BitConverter.ToInt32(BitConverter.GetBytes((this.userScaleFactor)), 0) != BitConverter.ToInt32(BitConverter.GetBytes((other.userScaleFactor)), 0))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Returns the background color.
		/// </summary>
		/// <returns> the background color. </returns>
		public virtual int BackgroundColor
		{
			get
			{
				lock (this)
				{
					return backgroundColor;
				}
			}
			set
			{
				lock (this)
				{
					this.backgroundColor = value;
				}
			}
		}

		/// <summary>
		/// Returns the maximum width of text beyond which the text is broken into lines.
		/// </summary>
		/// <returns> the maximum text width </returns>
		public virtual int MaxTextWidth
		{
			get
			{
				return maxTextWidth;
			}
		}

		/// <summary>
		/// Returns the overall scale factor.
		/// </summary>
		/// <returns> the combined device/user scale factor. </returns>
		public virtual float ScaleFactor
		{
			get
			{
				lock (this)
				{
					return deviceScaleFactor * this.userScaleFactor;
				}
			}
		}

		/// <summary>
		/// Width and height of a map tile in pixel after system and user scaling is applied.
		/// </summary>
		public virtual int TileSize
		{
			get
			{
				lock (this)
				{
					return tileSize;
				}
			}
		}

		/// <summary>
		/// Gets the tile size multiple.
		/// </summary>
		public virtual int TileSizeMultiple
		{
			get
			{
				lock (this)
				{
					return this.tileSizeMultiple;
				}
			}
			set
			{
				lock (this)
				{
					this.tileSizeMultiple = value;
					setTileSize();
				}
			}
		}

		/// <summary>
		/// Returns the user scale factor.
		/// </summary>
		/// <returns> the user scale factor. </returns>
		public virtual float UserScaleFactor
		{
			get
			{
				lock (this)
				{
					return this.userScaleFactor;
				}
			}
			set
			{
				lock (this)
				{
					userScaleFactor = value;
					setTileSize();
				}
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + this.backgroundColor;
			result = prime * result + this.fixedTileSize;
			result = prime * result + this.maxTextWidth;
			result = prime * result + BitConverter.ToInt32(BitConverter.GetBytes(this.maxTextWidthFactor), 0);
			result = prime * result + this.tileSize;
			result = prime * result + this.tileSizeMultiple;
			result = prime * result + BitConverter.ToInt32(BitConverter.GetBytes(this.userScaleFactor), 0);
			return result;
		}

		/// <summary>
		/// Forces the tile size to a fixed value
		/// </summary>
		/// <param name="tileSize">
		///            the fixed tile size to use if != 0, if 0 the tile size will be calculated </param>
		public virtual int FixedTileSize
		{
			set
			{
				this.fixedTileSize = value;
				setTileSize();
			}
		}

		/// <summary>
		/// Sets the factor to compute the maxTextWidth
		/// </summary>
		/// <param name="maxTextWidthFactor">
		///              to compute maxTextWidth </param>
		public virtual float MaxTextWidthFactor
		{
			set
			{
				this.maxTextWidthFactor = value;
				this.setMaxTextWidth();
			}
		}

		private void setMaxTextWidth()
		{
			this.maxTextWidth = (int)(this.tileSize * maxTextWidthFactor);
		}

		private void setTileSize()
		{
			if (this.fixedTileSize == 0)
			{
				float temp = DEFAULT_TILE_SIZE * deviceScaleFactor * userScaleFactor;
				// this will clamp to the nearest multiple of the tileSizeMultiple
				// and make sure we do not end up with 0
				this.tileSize = (int)Math.Max(tileSizeMultiple, Math.Round(temp / this.tileSizeMultiple) * this.tileSizeMultiple);
			}
			else
			{
				this.tileSize = this.fixedTileSize;
			}
			this.setMaxTextWidth();
		}
	}
}