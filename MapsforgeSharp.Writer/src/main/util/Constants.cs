/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2015 devemux86
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
namespace org.mapsforge.map.writer.util
{

	/// <summary>
	/// Defines some constants.
	/// </summary>
	public sealed class Constants
	{
		/// <summary>
		/// The name of the map file writer.
		/// </summary>
		public const string CREATOR_NAME = "mapsforge-map-writer";

		/// <summary>
		/// Default bbox enlargement.
		/// </summary>
		public const int DEFAULT_PARAM_BBOX_ENLARGEMENT = 20;
		/// <summary>
		/// Default coordinate encoding.
		/// </summary>
		public const string DEFAULT_PARAM_ENCODING = "auto";

		/// <summary>
		/// Default name for out file.
		/// </summary>
		public const string DEFAULT_PARAM_OUTFILE = "mapsforge.map";

		/// <summary>
		/// Default data processor type.
		/// </summary>
		public const string DEFAULT_PARAM_TYPE = "ram";

		/// <summary>
		/// Default simplification factor.
		/// </summary>
		public const double DEFAULT_SIMPLIFICATION_FACTOR = 2.5;

		/// <summary>
		/// The default size of a tile in pixel.
		/// </summary>
		public const int DEFAULT_TILE_SIZE = 256;
		/// <summary>
		/// The maximum base zoom level for which we apply a simplification algorithm to filter way points.
		/// </summary>
		public const int MAX_SIMPLIFICATION_BASE_ZOOM = 12;
		/// <summary>
		/// The name of the property that refers to the lowest version of the map file specification supported by this implementation.
		/// </summary>
		public const string PROPERTY_NAME_FILE_SPECIFICATION_VERSION_MIN = "mapfile.specification.version.min";
		/// <summary>
		/// The name of the property that refers to the highest version of the map file specification supported by this implementation.
		/// </summary>
		public const string PROPERTY_NAME_FILE_SPECIFICATION_VERSION_MAX = "mapfile.specification.version.max";
		/// <summary>
		/// The name of the property that refers to the version of the map file writer.
		/// </summary>
		public const string PROPERTY_NAME_WRITER_VERSION = "mapfile.writer.version";

		private Constants()
		{
			throw new System.InvalidOperationException();
		}
	}

}