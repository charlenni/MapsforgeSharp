/*
 * Copyright 2014 Ludwig M Brinckmann
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

namespace MapsforgeSharp.Core.Util
{
    using System.Collections.Generic;

    /// <summary>
    /// Cache that maintains a working set of elements in the cache, given to it by
    /// setWorkingSet(Set<K>) in addition to other elements which are kept on a LRU
    /// basis.
    /// </summary>
    /// @param <K>
    ///            the type of the map key, see <seealso cref="java.util.Map"/>. </param>
    /// @param <V>
    ///            the type of the map value, see <seealso cref="java.util.Map"/>. </param>
    public class WorkingSetCache<K, V> : LRUCache<K, V>
	{
		private const long serialVersionUID = 1L;

		public WorkingSetCache(int capacity) : base(capacity)
		{
		}

		/// <summary>
		/// Sets the current working set, ensuring that elements in this working set
		/// will not be ejected in the near future. </summary>
		/// <param name="workingSet"> set of K that makes up the current working set. </param>
		public virtual ISet<K> WorkingSet
		{
			set
			{
				foreach (K key in value)
				{
					this.Get(key);
				}
			}
		}
	}
}