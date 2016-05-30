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

namespace MapsforgeSharp.Core.Graphics
{
	public interface ITileBitmap : IBitmap
	{

		/// <summary>
		/// Returns the timestamp of the tile in milliseconds since January 1, 1970 GMT or 0 if this timestamp is unknown.
		/// <para>
		/// The timestamp indicates when the tile was created and can be used together with a TTL in order to determine
		/// whether to treat it as expired.
		/// </para>
		/// </summary>
		long Timestamp {get;set;}

		/// <summary>
		/// Whether the TileBitmap has expired.
		/// <para>
		/// When a tile has expired, the requester should try to replace it with a fresh copy as soon as possible. The
		/// expired tile may still be displayed to the user until the fresh copy is available. This may be desirable if
		/// obtaining a fresh copy is time-consuming or a fresh copy is currently unavailable (e.g. because no network
		/// connection is available for a <seealso cref="org.mapsforge.map.layer.download.tilesource.TileSource"/>).
		/// 
		/// </para>
		/// </summary>
		/// <returns> {@code true} if expired, {@code false} otherwise. </returns>
		bool Expired {get;}

		/// <summary>
		/// Sets the timestamp when this tile will be expired in milliseconds since January 1, 1970 GMT or 0 if this
		/// timestamp is unknown.
		/// <para>
		/// The timestamp indicates when the tile should be treated it as expired, i.e. <seealso cref="#isExpired()"/> will return
		/// {@code true}. For a downloaded tile, pass the value returned by
		/// <seealso cref="java.net.HttpURLConnection#getExpiration()"/>, if set by the server. In all other cases you can pass current
		/// time plus a fixed TTL in order to have the tile expire after the specified time.
		/// </para>
		/// </summary>
		long Expiration {set;}
	}
}