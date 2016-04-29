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

namespace org.mapsforge.map.rendertheme.rule
{
    using System.Collections.Generic;
    using core.util;

    using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
    using PointOfInterest = org.mapsforge.core.datastore.PointOfInterest;
    using RenderInstruction = org.mapsforge.map.rendertheme.renderinstruction.RenderInstruction;

    /// <summary>
    /// A RenderTheme defines how ways and nodes are drawn.
    /// </summary>
    public class RenderTheme
	{
		private const int MATCHING_CACHE_SIZE = 1024;

		private readonly float baseStrokeWidth;
		private readonly float baseTextSize;
		private readonly bool hasBackgroundOutside;
		private int levels;
		private readonly int mapBackground;
		private readonly int mapBackgroundOutside;
		private readonly LRUCache<MatchingCacheKey, IList<RenderInstruction>> wayMatchingCache;
		private readonly LRUCache<MatchingCacheKey, IList<RenderInstruction>> poiMatchingCache;
		private readonly List<Rule> rulesList; // NOPMD we need specific interface

		private readonly IDictionary<sbyte?, float?> strokeScales = new Dictionary<sbyte?, float?>();
		private readonly IDictionary<sbyte?, float?> textScales = new Dictionary<sbyte?, float?>();

		internal RenderTheme(RenderThemeBuilder renderThemeBuilder)
		{
			this.baseStrokeWidth = renderThemeBuilder.baseStrokeWidth;
			this.baseTextSize = renderThemeBuilder.baseTextSize;
			this.hasBackgroundOutside = renderThemeBuilder.hasBackgroundOutside;
			this.mapBackground = renderThemeBuilder.mapBackground;
			this.mapBackgroundOutside = renderThemeBuilder.mapBackgroundOutside;
			this.rulesList = new List<Rule>();
			this.poiMatchingCache = new LRUCache<MatchingCacheKey, IList<RenderInstruction>>(MATCHING_CACHE_SIZE);
			this.wayMatchingCache = new LRUCache<MatchingCacheKey, IList<RenderInstruction>>(MATCHING_CACHE_SIZE);
		}

		/// <summary>
		/// Must be called when this RenderTheme gets destroyed to clean up and free resources.
		/// </summary>
		public virtual void destroy()
		{
			this.poiMatchingCache.Clear();
			this.wayMatchingCache.Clear();
			foreach (Rule r in this.rulesList)
			{
				r.Destroy();
			}
		}

		/// <returns> the number of distinct drawing levels required by this RenderTheme. </returns>
		public virtual int Levels
		{
			get
			{
				return this.levels;
			}
			set
			{
				this.levels = value;
			}
		}

		/// <returns> the map background color of this RenderTheme. </returns>
		public virtual int MapBackground
		{
			get
			{
				return this.mapBackground;
			}
		}

		/// <returns> the background color that applies to areas outside the map. </returns>
		public virtual int MapBackgroundOutside
		{
			get
			{
				return this.mapBackgroundOutside;
			}
		}

		/// <returns> true if map color is defined for outside areas. </returns>
		public virtual bool HasMapBackgroundOutside()
		{
			return this.hasBackgroundOutside;
		}

		/// <summary>
		/// Matches a closed way with the given parameters against this RenderTheme. </summary>
		///  <param name="renderCallback">
		///            the callback implementation which will be executed on each match. </param>
		/// <param name="renderContext"> </param>
		/// <param name="way"> </param>
		public virtual void MatchClosedWay(RenderCallback renderCallback, RenderContext renderContext, PolylineContainer way)
		{
			MatchWay(renderCallback, renderContext, Closed.YES, way);
		}

		/// <summary>
		/// Matches a linear way with the given parameters against this RenderTheme. </summary>
		///  <param name="renderCallback">
		///            the callback implementation which will be executed on each match. </param>
		/// <param name="renderContext"> </param>
		/// <param name="way"> </param>
		public virtual void MatchLinearWay(RenderCallback renderCallback, RenderContext renderContext, PolylineContainer way)
		{
			MatchWay(renderCallback, renderContext, Closed.NO, way);
		}

		/// <summary>
		/// Matches a node with the given parameters against this RenderTheme. </summary>
		///  <param name="renderCallback">
		///            the callback implementation which will be executed on each match. </param>
		/// <param name="renderContext"> </param>
		/// <param name="poi">
		///            the point of interest. </param>
		public virtual void MatchNode(RenderCallback renderCallback, RenderContext renderContext, PointOfInterest poi)
		{
			lock (this)
			{
				MatchingCacheKey matchingCacheKey = new MatchingCacheKey(poi.Tags, renderContext.rendererJob.tile.ZoomLevel, Closed.NO);
        
				IList<RenderInstruction> matchingList = this.poiMatchingCache.Get(matchingCacheKey);
				if (matchingList != null)
				{
					// cache hit
					for (int i = 0, n = matchingList.Count; i < n; ++i)
					{
						matchingList[i].RenderNode(renderCallback, renderContext, poi);
					}
					return;
				}
        
				// cache miss
				matchingList = new List<RenderInstruction>();
        
				for (int i = 0, n = this.rulesList.Count; i < n; ++i)
				{
					this.rulesList[i].MatchNode(renderCallback, renderContext, matchingList, poi);
				}
				this.poiMatchingCache.Add(matchingCacheKey, matchingList);
			}
		}

		/// <summary>
		/// Scales the stroke width of this RenderTheme by the given factor for a given zoom level
		/// </summary>
		/// <param name="scaleFactor">
		///            the factor by which the stroke width should be scaled. </param>
		/// <param name="zoomLevel"> the zoom level to which this is applied. </param>
		public virtual void ScaleStrokeWidth(float scaleFactor, sbyte zoomLevel)
		{
			lock (this)
			{
				if (!strokeScales.ContainsKey(zoomLevel) || scaleFactor != strokeScales[zoomLevel])
				{
					for (int i = 0, n = this.rulesList.Count; i < n; ++i)
					{
						Rule rule = this.rulesList[i];
						if (rule.zoomMin <= zoomLevel && rule.zoomMax >= zoomLevel)
						{
							rule.ScaleStrokeWidth(scaleFactor * this.baseStrokeWidth, zoomLevel);
						}
					}
					strokeScales[zoomLevel] = scaleFactor;
				}
			}
		}

		/// <summary>
		/// Scales the text size of this RenderTheme by the given factor for a given zoom level.
		/// </summary>
		/// <param name="scaleFactor">
		///            the factor by which the text size should be scaled. </param>
		/// <param name="zoomLevel"> the zoom level to which this is applied. </param>
		public virtual void ScaleTextSize(float scaleFactor, sbyte zoomLevel)
		{
			lock (this)
			{
				if (!textScales.ContainsKey(zoomLevel) || scaleFactor != textScales[zoomLevel])
				{
					for (int i = 0, n = this.rulesList.Count; i < n; ++i)
					{
						Rule rule = this.rulesList[i];
						if (rule.zoomMin <= zoomLevel && rule.zoomMax >= zoomLevel)
						{
							rule.ScaleTextSize(scaleFactor * this.baseTextSize, zoomLevel);
						}
					}
					textScales[zoomLevel] = scaleFactor;
				}
			}
		}

		internal virtual void AddRule(Rule rule)
		{
			this.rulesList.Add(rule);
		}

		internal virtual void Complete()
		{
			this.rulesList.TrimExcess();
			for (int i = 0, n = this.rulesList.Count; i < n; ++i)
			{
				this.rulesList[i].OnComplete();
			}
		}

		private void MatchWay(RenderCallback renderCallback, RenderContext renderContext, Closed closed, PolylineContainer way)
		{
			lock (this)
			{
				MatchingCacheKey matchingCacheKey = new MatchingCacheKey(way.Tags, way.Tile.ZoomLevel, closed);
        
				IList<RenderInstruction> matchingList = this.wayMatchingCache.Get(matchingCacheKey);
				if (matchingList != null)
				{
					// cache hit
					for (int i = 0, n = matchingList.Count; i < n; ++i)
					{
						matchingList[i].RenderWay(renderCallback, renderContext, way);
					}
					return;
				}
        
				// cache miss
				matchingList = new List<RenderInstruction>();
				for (int i = 0, n = this.rulesList.Count; i < n; ++i)
				{
					this.rulesList[i].MatchWay(renderCallback, way, way.Tile, closed, matchingList, renderContext);
				}
        
				this.wayMatchingCache.Add(matchingCacheKey, matchingList);
			}
		}
	}
}