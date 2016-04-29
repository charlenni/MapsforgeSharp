/*
 * Copyright 2014-2015 Ludwig M Brinckmann
 * Copyright 2015 devemux86
 * Copyright 2015 lincomatic
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

namespace org.mapsforge.core.datastore
{
    using System;
    using System.Globalization;
    using BoundingBox = org.mapsforge.core.model.BoundingBox;
    using LatLong = org.mapsforge.core.model.LatLong;
    using Tile = org.mapsforge.core.model.Tile;

    /// <summary>
    /// Base class for map data retrieval.
    /// </summary>
    public abstract class MapDataStore
	{
		/// <summary>
		/// Extracts substring of preferred language from multilingual string.<br/>
		/// Example multilingual string: "Base\ren\bEnglish\rjp\bJapan\rzh_py\bPin-yin".
		/// <para>
		/// Use '\r' delimiter among names and '\b' delimiter between each language and name.
		/// </para>
		/// </summary>
		public static string Extract(string s, string language)
		{
			if (string.ReferenceEquals(s, null) || s.Trim().Length == 0)
			{
				return null;
			}

			string[] langNames = s.Split("\r", true);
			if (string.ReferenceEquals(language, null) || language.Trim().Length == 0)
			{
				return langNames[0];
			}

			string fallback = null;
			for (int i = 1; i < langNames.Length; i++)
			{
				string[] langName = langNames[i].Split("\b", true);
				if (langName.Length != 2)
				{
					continue;
				}

				// Perfect match
				if (langName[0].Equals(language, StringComparison.CurrentCultureIgnoreCase))
				{
					return langName[1];
				}

				// Fall back to base, e.g. zh-min-lan -> zh
				if (string.ReferenceEquals(fallback, null) && !langName[0].Contains("-") && (language.Contains("-") || language.Contains("_")) && language.ToLower().StartsWith(langName[0].ToLower(), StringComparison.Ordinal))
				{
					fallback = langName[1];
				}
			}
			return (!string.ReferenceEquals(fallback, null)) ? fallback : langNames[0];
		}

		/// <summary>
		/// the preferred language when extracting labels from this data store. The actual
		/// implementation is up to the concrete implementation, which can also simply ignore
		/// this setting.
		/// </summary>
		protected internal string preferredLanguage;

		/// <summary>
		/// Ctor for MapDataStore that will use default language.
		/// </summary>
		public MapDataStore() : this(null)
		{
		}

		/// <summary>
		/// Ctor for MapDataStore setting preferred language. </summary>
		/// <param name="language"> the preferred language or null if default language is used. </param>
		public MapDataStore(string language)
		{
			this.preferredLanguage = language;
		}

		///
		/// Closes the map database.
		///
		public abstract void Close();

		/// <summary>
		/// Extracts substring of preferred language from multilingual string using
		/// the preferredLanguage setting.
		/// </summary>
		protected internal virtual string ExtractLocalized(string s)
		{
			return MapDataStore.Extract(s, preferredLanguage);
		}

		/// <summary>
		/// Returns the timestamp of the data used to render a specific tile.
		/// </summary>
		/// <param name="tile">
		///            A tile. </param>
		/// <returns> the timestamp of the data used to render the tile </returns>
		public abstract long GetDataTimestamp(Tile tile);

		/// <summary>
		/// Reads data for tile. </summary>
		/// <param name="tile"> tile for which data is requested. </param>
		/// <returns> map data for the tile. </returns>
		public abstract MapReadResult ReadMapData(Tile tile);

        /// <summary>
        /// Gets the area for which data is supplied. </summary>
        /// <returns> bounding box of area. </returns>
        public abstract BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Gets the initial map position. </summary>
        /// <returns> the start position, if available. </returns>
        public abstract LatLong StartPosition { get; set; }

		/// <summary>
		/// Gets the initial zoom level. </summary>
		/// <returns> the start zoom level. </returns>
		public abstract sbyte? StartZoomLevel { get; set; }

        /// <summary>
        /// Returns true if MapDatabase contains tile. </summary>
        /// <param name="tile"> tile to be rendered. </param>
        /// <returns> true if tile is part of database. </returns>
        public abstract bool SupportsTile(Tile tile);
	}
}