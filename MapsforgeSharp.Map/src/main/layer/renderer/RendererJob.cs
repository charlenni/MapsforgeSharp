/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.map.layer.renderer
{
    using System;

    using Tile = org.mapsforge.core.model.Tile;
    using Job = org.mapsforge.map.layer.queue.Job;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using MapDataStore = org.mapsforge.core.datastore.MapDataStore;
    using RenderThemeFuture = org.mapsforge.map.rendertheme.rule.RenderThemeFuture;

    public class RendererJob : Job
	{
		public readonly DisplayModel displayModel;
		public bool labelsOnly;
		public readonly MapDataStore mapDataStore;
		public readonly RenderThemeFuture renderThemeFuture;
		public readonly float textScale;
		private readonly int hashCodeValue;

		public RendererJob(Tile tile, MapDataStore mapFile, RenderThemeFuture renderThemeFuture, DisplayModel displayModel, float textScale, bool isTransparent, bool labelsOnly) : base(tile, isTransparent)
		{
			if (mapFile == null)
			{
				throw new System.ArgumentException("mapFile must not be null");
			}
			else if (textScale <= 0 || float.IsNaN(textScale))
			{
				throw new System.ArgumentException("invalid textScale: " + textScale);
			}

			this.labelsOnly = labelsOnly;
			this.displayModel = displayModel;
			this.mapDataStore = mapFile;
			this.renderThemeFuture = renderThemeFuture;
			this.textScale = textScale;

			this.hashCodeValue = CalculateHashCode();
		}

        /// <summary>
        /// Indicates that for this job only the labels should be generated.
        /// </summary>
        public virtual bool RetrieveLabelsOnly
        {
            get
            {
                return this.labelsOnly;
            }
            set
            {
                this.labelsOnly = value;
            }
        }

        public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!base.Equals(obj))
			{
				return false;
			}
			else if (!(obj is RendererJob))
			{
				return false;
			}
			RendererJob other = (RendererJob) obj;
			if (!this.mapDataStore.Equals(other.mapDataStore))
			{
				return false;
			}
			else if (BitConverter.ToInt32(BitConverter.GetBytes(this.textScale), 0) != BitConverter.ToInt32(BitConverter.GetBytes(other.textScale), 0))
			{
				return false;
			}
			else if (this.renderThemeFuture == null && other.renderThemeFuture != null)
			{
				return false;
			}
			else if (this.renderThemeFuture != null && !this.renderThemeFuture.Equals(other.renderThemeFuture))
			{
				return false;
			}
			else if (this.labelsOnly != other.labelsOnly)
			{
				return false;
			}
			else if (!this.displayModel.Equals(other.displayModel))
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return this.hashCodeValue;
		}

		/// <summary>
		/// Just a way of generating a hash key for a tile if only the RendererJob is known. </summary>
		/// <param name="tile"> the tile that changes </param>
		/// <returns> a RendererJob based on the current one, only tile changes </returns>
		public virtual RendererJob OtherTile(Tile tile)
		{
			return new RendererJob(tile, this.mapDataStore, this.renderThemeFuture, this.displayModel, this.textScale, this.hasAlpha, this.labelsOnly);
		}

        private int CalculateHashCode()
		{
			const int prime = 31;
			int result = base.GetHashCode();
			result = prime * result + this.mapDataStore.GetHashCode();
			result = prime * result + BitConverter.ToInt32(BitConverter.GetBytes(this.textScale), 0);
			if (renderThemeFuture != null)
			{
				result = prime * result + this.renderThemeFuture.GetHashCode();
			}
			return result;
		}
	}
}