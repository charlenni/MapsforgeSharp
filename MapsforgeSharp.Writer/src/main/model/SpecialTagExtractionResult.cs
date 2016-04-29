/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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
namespace org.mapsforge.map.writer.model
{

	public class SpecialTagExtractionResult
	{
		private readonly short elevation;
		private readonly string housenumber;
		private readonly sbyte layer;
		private readonly string name;
		private readonly string @ref;
		private readonly string type;

		/// <param name="name">
		///            the name </param>
		/// <param name="ref">
		///            the ref </param>
		/// <param name="housenumber">
		///            the housenumber </param>
		/// <param name="layer">
		///            the layer </param>
		/// <param name="elevation">
		///            the elevation </param>
		/// <param name="type">
		///            the type </param>
		public SpecialTagExtractionResult(string name, string @ref, string housenumber, sbyte layer, short elevation, string type) : base()
		{
			this.name = name;
			this.@ref = @ref;
			this.housenumber = housenumber;
			this.layer = layer;
			this.elevation = elevation;
			this.type = type;
		}

		/// <returns> the elevation </returns>
		public short Elevation
		{
			get
			{
				return this.elevation;
			}
		}

		/// <returns> the housenumber </returns>
		public string Housenumber
		{
			get
			{
				return this.housenumber;
			}
		}

		/// <returns> the layer </returns>
		public sbyte Layer
		{
			get
			{
				return this.layer;
			}
		}

		/// <returns> the name </returns>
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		/// <returns> the ref </returns>
		public string Ref
		{
			get
			{
				return this.@ref;
			}
		}

		/// <returns> the type </returns>
		public string Type
		{
			get
			{
				return this.type;
			}
		}
	}

}