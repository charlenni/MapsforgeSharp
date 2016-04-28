using System;
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


	using TDNode = org.mapsforge.map.writer.model.TDNode;
	using TDWay = org.mapsforge.map.writer.model.TDWay;
	using TileData = org.mapsforge.map.writer.model.TileData;

	public class RAMTileData : TileData
	{
		private readonly ISet<TDNode> pois;
		private readonly ISet<TDWay> ways;

		internal RAMTileData() : base()
		{
			this.pois = new HashSet<>();
			this.ways = new HashSet<>();
		}

		public override void addPOI(TDNode poi)
		{
			this.pois.Add(poi);
		}

		public override void addWay(TDWay way)
		{
			this.ways.Add(way);
		}

		public override IDictionary<sbyte?, IList<TDNode>> poisByZoomlevel(sbyte minValidZoomlevel, sbyte maxValidZoomlevel)
		{
			Dictionary<sbyte?, IList<TDNode>> poisByZoomlevel = new Dictionary<sbyte?, IList<TDNode>>();
			foreach (TDNode poi in this.pois)
			{
				sbyte zoomlevel = poi.ZoomAppear;
				if (zoomlevel > maxValidZoomlevel)
				{
					continue;
				}
				if (zoomlevel < minValidZoomlevel)
				{
					zoomlevel = minValidZoomlevel;
				}
				IList<TDNode> group = poisByZoomlevel[Convert.ToSByte(zoomlevel)];
				if (group == null)
				{
					group = new List<>();
				}
				group.Add(poi);
				poisByZoomlevel[Convert.ToSByte(zoomlevel)] = group;
			}

			return poisByZoomlevel;
		}

		public override IDictionary<sbyte?, IList<TDWay>> waysByZoomlevel(sbyte minValidZoomlevel, sbyte maxValidZoomlevel)
		{
			Dictionary<sbyte?, IList<TDWay>> waysByZoomlevel = new Dictionary<sbyte?, IList<TDWay>>();
			foreach (TDWay way in this.ways)
			{
				sbyte zoomlevel = way.MinimumZoomLevel;
				if (zoomlevel > maxValidZoomlevel)
				{
					continue;
				}
				if (zoomlevel < minValidZoomlevel)
				{
					zoomlevel = minValidZoomlevel;
				}
				IList<TDWay> group = waysByZoomlevel[Convert.ToSByte(zoomlevel)];
				if (group == null)
				{
					group = new List<>();
				}
				group.Add(way);
				waysByZoomlevel[Convert.ToSByte(zoomlevel)] = group;
			}

			return waysByZoomlevel;
		}
	}

}