using System.Collections.Generic;

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
namespace org.mapsforge.map.writer
{

	using TLongArrayList = gnu.trove.list.array.TLongArrayList;


	using TDNode = org.mapsforge.map.writer.model.TDNode;
	using TDWay = org.mapsforge.map.writer.model.TDWay;
	using TileData = org.mapsforge.map.writer.model.TileData;

	public class HDTileData : TileData
	{
		private readonly TLongArrayList pois;
		private readonly TLongArrayList ways;

		internal HDTileData() : base()
		{
			this.pois = new TLongArrayList();
			this.ways = new TLongArrayList();
		}

		public override void addPOI(TDNode poi)
		{
			this.pois.add(poi.Id);
		}

		public override void addWay(TDWay way)
		{
			this.ways.add(way.Id);
		}

		public override IDictionary<sbyte?, IList<TDNode>> poisByZoomlevel(sbyte minValidZoomlevel, sbyte maxValidZoomlevel)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			throw new System.NotSupportedException(typeof(HDTileData).FullName + "does not support this operation");
		}

		public override IDictionary<sbyte?, IList<TDWay>> waysByZoomlevel(sbyte minValidZoomlevel, sbyte maxValidZoomlevel)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			throw new System.NotSupportedException(typeof(HDTileData).FullName + "does not support this operation");
		}

		internal TLongArrayList Pois
		{
			get
			{
				return this.pois;
			}
		}

		internal TLongArrayList Ways
		{
			get
			{
				return this.ways;
			}
		}
	}

}