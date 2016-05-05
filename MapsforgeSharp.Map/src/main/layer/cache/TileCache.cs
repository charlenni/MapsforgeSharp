/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

namespace org.mapsforge.map.layer.cache
{
    using System.Collections.Generic;

    using TileBitmap = MapsforgeSharp.Core.Graphics.TileBitmap;
	using Job = org.mapsforge.map.layer.queue.Job;
	using ObservableInterface = org.mapsforge.map.model.common.ObservableInterface;

	/// <summary>
	/// Interface for tile image caches.
	/// </summary>
	public interface TileCache : ObservableInterface
	{
		/// <returns> true if this cache contains an image for the given key, false otherwise. </returns>
		/// <seealso cref= Map#containsKey </seealso>
		bool ContainsKey(Job key);

		/// <summary>
		/// Destroys this cache.
		/// <para>
		/// Applications are expected to call this method when they no longer require the cache.
		/// </para>
		/// <para>
		/// In versions prior to 0.5.1, it was common practice to call this method but continue using the cache, in order to
		/// empty it, forcing all tiles to be re-rendered or re-requested from the source. Beginning with 0.5.1,
		/// <seealso cref="#purge()"/> should be used for this purpose. The earlier practice is now discouraged and may lead to
		/// unexpected results when used with features introduced in 0.5.1 or later.
		/// </para>
		/// </summary>
		void Destroy();

		/// <returns> the image for the given key or null, if this cache contains no image for the key. </returns>
		/// <seealso cref= Map#get </seealso>
		TileBitmap Get(Job key);

		/// <returns> the capacity of this cache. </returns>
		int Capacity {get;}

		/// <returns> the capacity of the first level of a multi-level cache. </returns>
		int CapacityFirstLevel {get;}

		/// <summary>
		/// Returns tileBitmap only if available at fastest cache in case of multi-layered cache, null otherwise.
		/// </summary>
		/// <returns> tileBitmap if available without getting from lower storage levels </returns>
		TileBitmap GetImmediately(Job key);

		/// <summary>
		/// Purges this cache.
		/// <para>
		/// Calls to <seealso cref="#get(Job)"/> issued after purging will not return any tiles added before the purge operation.
		/// </para>
		/// <para>
		/// Applications should purge the tile cache when map model parameters change, such as the render style for locally
		/// rendered tiles, or the source for downloaded tiles. Applications which frequently alternate between a limited
		/// number of map model configurations may want to consider using a different cache for each.
		/// 
		/// @since 0.5.1
		/// </para>
		/// </summary>
		void Purge();

		/// <exception cref="IllegalArgumentException">
		///             if any of the parameters is {@code null}. </exception>
		/// <seealso cref= Map#put </seealso>
		void Put(Job key, TileBitmap bitmap);

		/// <summary>
		/// Reserves a working set in this cache, for multi-level caches this means bringing the elements in workingSet into
		/// the fastest cache.
		/// </summary>
		ISet<Job> WorkingSet {set;}
	}
}