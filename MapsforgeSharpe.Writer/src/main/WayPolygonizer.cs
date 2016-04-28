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
	using JTSUtils = org.mapsforge.map.writer.util.JTSUtils;

	using Coordinate = com.vividsolutions.jts.geom.Coordinate;
	using GeometryFactory = com.vividsolutions.jts.geom.GeometryFactory;
	using Polygon = com.vividsolutions.jts.geom.Polygon;

	//TODO could be implemented more efficiently with graphs: each line string is an edge, use an undirected graph and search for strongly connected components

	internal class WayPolygonizer
	{
		internal class PolygonMergeException : Exception
		{
			private readonly WayPolygonizer outerInstance;

			public PolygonMergeException(WayPolygonizer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal const long serialVersionUID = 1L;
		}

		private const int MIN_NODES_POLYGON = 4;

		private static bool isClosedPolygon(Deque<TDWay> currentPolygonSegments)
		{
			TDWay c1Start = currentPolygonSegments.First;
			TDWay c1End = currentPolygonSegments.Last;

			long startFirst = c1Start.ReversedInRelation ? c1Start.WayNodes[c1Start.WayNodes.length - 1].Id : c1Start.WayNodes[0].Id;

			long endLast = c1End.ReversedInRelation ? c1End.WayNodes[0].Id : c1End.WayNodes[c1End.WayNodes.length - 1].Id;

			return startFirst == endLast;
		}

		private static bool isClosedPolygon(TDWay way)
		{
			TDNode[] waynodes = way.WayNodes;
			return waynodes[0].Id == waynodes[waynodes.Length - 1].Id;
		}

		private static Coordinate[] toCoordinates(ICollection<TDWay> linestrings)
		{
			Coordinate[][] temp = new Coordinate[linestrings.Count][];
			int i = 0;
			int n = 0;
			foreach (TDWay tdWay in linestrings)
			{
				temp[i] = JTSUtils.toCoordinates(tdWay);
				n += temp[i].Length;
				++i;
			}
			Coordinate[] res = new Coordinate[n];
			int pos = 0;
			for (i = 0; i < temp.Length; i++)
			{
				Array.Copy(temp[i], 0, res, pos, temp[i].Length);
				pos += temp[i].Length;
			}
			return res;
		}

		private IList<TDWay> dangling;

		private readonly GeometryFactory geometryFactory = new GeometryFactory();

		private IList<TDWay> illegal;

		private IDictionary<int?, IList<int?>> outerToInner;

		private IList<Deque<TDWay>> polygons;

		internal virtual IList<TDWay> Dangling
		{
			get
			{
				return this.dangling;
			}
		}

		internal virtual IList<TDWay> Illegal
		{
			get
			{
				return this.illegal;
			}
		}

		internal virtual IDictionary<int?, IList<int?>> OuterToInner
		{
			get
			{
				return this.outerToInner;
			}
		}

		internal virtual IList<Deque<TDWay>> Polygons
		{
			get
			{
				return this.polygons;
			}
		}

		/// <summary>
		/// Tries to merge ways to closed polygons. The ordering of waynodes is preserved during the merge process.
		/// </summary>
		/// <param name="ways">
		///            An array of ways that should be merged. Ways may be given in any order and may already be closed. </param>
		internal virtual void mergePolygons(TDWay[] ways)
		{
			this.polygons = new List<>();
			this.dangling = new List<>();
			this.illegal = new List<>();

			Deque<TDWay> ungroupedWays = new ArrayDeque<TDWay>();

			// initially all ways are ungrouped
			foreach (TDWay tdWay in ways)
			{
				// reset reversed flag, may already be set when way is part of another relation
				tdWay.ReversedInRelation = false;

				// first extract all way that are closed polygons in their own right
				if (isClosedPolygon(tdWay))
				{
					if (tdWay.WayNodes.length < MIN_NODES_POLYGON)
					{
						this.illegal.Add(tdWay);
					}
					else
					{
						Deque<TDWay> cluster = new ArrayDeque<TDWay>();
						cluster.add(tdWay);
						this.polygons.Add(cluster);
					}
				}
				else
				{
					ungroupedWays.add(tdWay);
				}
			}

			// all ways have been polygons, nice!
			if (ungroupedWays.Empty)
			{
				return;
			}

			if (ungroupedWays.size() == 1)
			{
				this.dangling.Add(ungroupedWays.First);
				return;
			}

			bool startNewPolygon = true;

			while (true)
			{
				bool merge = false;
				if (startNewPolygon)
				{
					// we start a new polygon either during first iteration or when
					// the previous iterations merged ways to a closed polygon and there
					// are still ungrouped ways left
					Deque<TDWay> cluster = new ArrayDeque<TDWay>();
					// get the first way of the yet ungrouped ways and form a new group
					cluster.add(ungroupedWays.removeFirst());
					this.polygons.Add(cluster);
					startNewPolygon = false;
				}

				// test if we can merge the current polygon with an ungrouped way
				IEnumerator<TDWay> it = ungroupedWays.GetEnumerator();
				while (it.MoveNext())
				{
					TDWay current = it.Current;

					Deque<TDWay> currentPolygonSegments = this.polygons[this.polygons.Count - 1];
					// first way in current polygon
					TDWay c1Start = currentPolygonSegments.First;
					// last way in current polygon
					TDWay c1End = currentPolygonSegments.Last;

					long startFirst = c1Start.ReversedInRelation ? c1Start.WayNodes[c1Start.WayNodes.length - 1].Id : c1Start.WayNodes[0].Id;

					long endLast = c1End.ReversedInRelation ? c1End.WayNodes[0].Id : c1End.WayNodes[c1End.WayNodes.length - 1].Id;

					long currentFirst = current.WayNodes[0].Id;
					long currentLast = current.WayNodes[current.WayNodes.length - 1].Id;

					// current way end connects to the start of the current polygon (correct direction)
					if (startFirst == currentLast)
					{
						merge = true;
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						it.remove();
						// add way to start of current polygon
						currentPolygonSegments.offerFirst(current);
					}
					// // current way start connects to the start of the current polygon (reversed
					// direction)
					else if (startFirst == currentFirst)
					{
						current.ReversedInRelation = true;
						merge = true;
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						it.remove();
						currentPolygonSegments.offerFirst(current);
					}
					// current way start connects to the end of the current polygon (correct direction)
					else if (endLast == currentFirst)
					{
						merge = true;
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						it.remove();
						// add way to end of current polygon
						currentPolygonSegments.offerLast(current);
					}
					// // current way end connects to the end of the current polygon (reversed direction)
					else if (endLast == currentLast)
					{
						current.ReversedInRelation = true;
						merge = true;
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						it.remove();
						// add way to end of current polygon
						currentPolygonSegments.offerLast(current);
					}
				}

				Deque<TDWay> currentCluster = this.polygons[this.polygons.Count - 1];
				bool closed = isClosedPolygon(currentCluster);
				// not a closed polygon and no more ways to merge
				if (!closed)
				{
					if (ungroupedWays.Empty || !merge)
					{
						((List<TDWay>)this.dangling).AddRange(this.polygons[this.polygons.Count - 1]);
						// may be a non operation when ungrouped is empty
						((List<TDWay>)this.dangling).AddRange(ungroupedWays);
						this.polygons.Remove(this.polygons.Count - 1);
						return;
					}
				}
				else
				{
					// built a closed polygon and no more ways left --> we are finished
					if (ungroupedWays.Empty)
					{
						return;
					}

					startNewPolygon = true;
				}

				// if we are here, the polygon is not yet closed, but there are also some ungrouped ways
				// which may be merge-able in the next iteration
			}
		}

		internal virtual void polygonizeAndRelate(TDWay[] ways)
		{
			mergePolygons(ways);
			relatePolygons();
		}

		internal virtual void relatePolygons()
		{
			this.outerToInner = new Dictionary<>();
			if (this.polygons.Count == 0)
			{
				return;
			}

			Polygon[] polygonGeometries = new Polygon[this.polygons.Count];
			int i = 0;
			foreach (Deque<TDWay> polygon in this.polygons)
			{
				polygonGeometries[i++] = this.geometryFactory.createPolygon(this.geometryFactory.createLinearRing(toCoordinates(polygon)), null);
			}

			this.outerToInner = new Dictionary<>();
			HashSet<int?> inner = new HashSet<int?>();
			for (int k = 0; k < polygonGeometries.Length; k++)
			{
				if (inner.Contains(Convert.ToInt32(k)))
				{
					continue;
				}
				for (int l = k + 1; l < polygonGeometries.Length; l++)
				{
					if (inner.Contains(Convert.ToInt32(l)))
					{
						continue;
					}

					if (polygonGeometries[k].covers(polygonGeometries[l]))
					{
						IList<int?> inners = this.outerToInner[Convert.ToInt32(k)];
						if (inners == null)
						{
							inners = new List<>();
							this.outerToInner[Convert.ToInt32(k)] = inners;
						}
						inners.Add(Convert.ToInt32(l));
						inner.Add(Convert.ToInt32(l));
					}
					else if (!this.outerToInner.ContainsKey(Convert.ToInt32(k)) && polygonGeometries[l].covers(polygonGeometries[k]))
					{
						IList<int?> inners = this.outerToInner[Convert.ToInt32(l)];
						if (inners == null)
						{
							inners = new List<>();
							this.outerToInner[Convert.ToInt32(l)] = inners;
						}
						inners.Add(Convert.ToInt32(k));
						inner.Add(Convert.ToInt32(k));
					}
				}

				// single polygon without any inner polygons
				if (!this.outerToInner.ContainsKey(Convert.ToInt32(k)) && !inner.Contains(Convert.ToInt32(k)))
				{
					this.outerToInner[Convert.ToInt32(k)] = null;
				}
			}
		}
	}

}