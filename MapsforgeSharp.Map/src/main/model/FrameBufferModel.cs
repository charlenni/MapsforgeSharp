/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

namespace org.mapsforge.map.model
{
	using Dimension = MapsforgeSharp.Core.Model.Dimension;
	using MapPosition = MapsforgeSharp.Core.Model.MapPosition;
	using Observable = org.mapsforge.map.model.common.Observable;

	public class FrameBufferModel : Observable
	{
		private Dimension dimension;
		private MapPosition mapPosition;
		private double overdrawFactor = 1.2;

		/// <returns> the current dimension of the {@code FrameBuffer} (may be null). </returns>
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
					this.dimension = value;
				}
				NotifyObservers();
			}
		}

		/// <returns> the current {@code MapPosition} of the {@code FrameBuffer} (may be null). </returns>
		public virtual MapPosition MapPosition
		{
			get
			{
				lock (this)
				{
					return this.mapPosition;
				}
			}
			set
			{
				lock (this)
				{
					this.mapPosition = value;
				}
				NotifyObservers();
			}
		}

		public virtual double OverdrawFactor
		{
			get
			{
				lock (this)
				{
					return this.overdrawFactor;
				}
			}
			set
			{
				if (value <= 0)
				{
					throw new System.ArgumentException("overdrawFactor must be > 0: " + value);
				}
				lock (this)
				{
					this.overdrawFactor = value;
				}
				NotifyObservers();
			}
		}
	}
}