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

namespace org.mapsforge.map.layer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DisplayModel = org.mapsforge.map.model.DisplayModel;

    /// <summary>
    /// A thread-safe <seealso cref="Layer"/> list which does not allow {@code null} elements.
    /// </summary>
    public class Layers : IEnumerable<Layer>
	{

		private static void CheckIsNull(ICollection<Layer> layers)
		{
			if (layers == null)
			{
				throw new System.ArgumentException("layers must not be null");
			}

			foreach (Layer layer in layers)
			{
				CheckIsNull(layer);
			}
		}

		private static void CheckIsNull(Layer layer)
		{
			if (layer == null)
			{
				throw new System.ArgumentException("layer must not be null");
			}
		}

		private readonly DisplayModel displayModel;
		private readonly IList<Layer> layersList;
		private readonly Redrawer redrawer;

		internal Layers(Redrawer redrawer, DisplayModel displayModel)
		{
			this.redrawer = redrawer;
			this.displayModel = displayModel;

			this.layersList = new List<Layer>();
		}

		/// <seealso cref= List#add(int, Object) </seealso>
		public virtual void Add(int index, Layer layer)
		{
			lock (this)
			{
				CheckIsNull(layer);
				layer.DisplayModel = this.displayModel;
				this.layersList.Insert(index, layer);
				layer.Assign(this.redrawer);
				this.redrawer.RedrawLayers();
			}
		}

		/// <seealso cref= List#add(Object) </seealso>
		public virtual void Add(Layer layer)
		{
			lock (this)
			{
				CheckIsNull(layer);
				layer.DisplayModel = this.displayModel;
        
				this.layersList.Add(layer);
				layer.Assign(this.redrawer);
				this.redrawer.RedrawLayers();
			}
		}

		/// <seealso cref= List#addAll(Collection) </seealso>
		public virtual void AddAll(ICollection<Layer> layers)
		{
			lock (this)
			{
				CheckIsNull(layers);
				foreach (Layer layer in layers)
				{
					layer.DisplayModel = this.displayModel;
				}
				((List<Layer>)this.layersList).AddRange(layers);
				foreach (Layer layer in layers)
				{
					layer.Assign(this.redrawer);
				}
				this.redrawer.RedrawLayers();
			}
		}

		/// <seealso cref= List#addAll(int, Collection) </seealso>
		public virtual void AddAll(int index, ICollection<Layer> layers)
		{
			lock (this)
			{
				CheckIsNull(layers);
				((List<Layer>)this.layersList).InsertRange(index, layers);
				foreach (Layer layer in layers)
				{
					layer.DisplayModel = this.displayModel;
					layer.Assign(this.redrawer);
				}
				this.redrawer.RedrawLayers();
			}
		}

		/// <seealso cref= List#clear() </seealso>
		public virtual void Clear()
		{
			lock (this)
			{
				foreach (Layer layer in this.layersList)
				{
					layer.Unassign();
				}
				this.layersList.Clear();
				this.redrawer.RedrawLayers();
			}
		}

		/// <seealso cref= List#contains(Object) </seealso>
		public virtual bool Contains(Layer layer)
		{
			lock (this)
			{
				CheckIsNull(layer);
				return this.layersList.Contains(layer);
			}
		}

		/// <seealso cref= List#get(int) </seealso>
		public virtual Layer Get(int index)
		{
			lock (this)
			{
				return this.layersList[index];
			}
		}

		/// <seealso cref= List#indexOf(Object) </seealso>
		public virtual int IndexOf(Layer layer)
		{
			lock (this)
			{
				CheckIsNull(layer);
				return this.layersList.IndexOf(layer);
			}
		}

		/// <seealso cref= List#isEmpty() </seealso>
		public virtual bool Empty
		{
			get
			{
				lock (this)
				{
					return this.layersList.Count == 0;
				}
			}
		}

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this)
            {
                return this.layersList.GetEnumerator();
            }
        }

        /// <seealso cref= List#iterator() </seealso>
        public virtual IEnumerator<Layer> GetEnumerator()
		{
			lock (this)
			{
				return this.layersList.GetEnumerator();
			}
		}

		/// <seealso cref= List#remove(int) </seealso>
		public virtual Layer Remove(int index)
		{
			lock (this)
			{
                Layer layer = this.layersList[index];
                this.layersList.RemoveAt(index);
				layer.Unassign();
				this.redrawer.RedrawLayers();
				return layer;
			}
		}

		/// <seealso cref= List#remove(Object) </seealso>
		public virtual bool Remove(Layer layer)
		{
			lock (this)
			{
				CheckIsNull(layer);
				if (this.layersList.Remove(layer))
				{
					layer.Unassign();
					this.redrawer.RedrawLayers();
					return true;
				}
				return false;
			}
		}

		/// <seealso cref= List#size() </seealso>
		public virtual int Size()
		{
			lock (this)
			{
				return this.layersList.Count;
			}
		}
    }
}