/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
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

namespace org.mapsforge.map.rendertheme.rule
{
    using System.Collections.Generic;

    using Tag = MapsforgeSharp.Core.Model.Tag;
	using Tile = MapsforgeSharp.Core.Model.Tile;
	using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
	using PointOfInterest = MapsforgeSharp.Core.Datastore.PointOfInterest;
	using RenderInstruction = org.mapsforge.map.rendertheme.renderinstruction.RenderInstruction;

	public abstract class Rule
	{
		internal static readonly IDictionary<IList<string>, AttributeMatcher> MATCHERS_CACHE_KEY = new Dictionary<IList<string>, AttributeMatcher>();
		internal static readonly IDictionary<IList<string>, AttributeMatcher> MATCHERS_CACHE_VALUE = new Dictionary<IList<string>, AttributeMatcher>();

		internal string cat;
		internal readonly ClosedMatcher closedMatcher;
		internal readonly ElementMatcher elementMatcher;
		internal readonly sbyte zoomMax;
		internal readonly sbyte zoomMin;
		private readonly List<RenderInstruction> renderInstructions; // NOSONAR NOPMD we need specific interface
		private readonly List<Rule> subRules; // NOSONAR NOPMD we need specific interface

		public Rule(RuleBuilder ruleBuilder)
		{
			this.cat = ruleBuilder.cat;
			this.closedMatcher = ruleBuilder.closedMatcher;
			this.elementMatcher = ruleBuilder.elementMatcher;
			this.zoomMax = ruleBuilder.zoomMax;
			this.zoomMin = ruleBuilder.zoomMin;

			this.renderInstructions = new List<RenderInstruction>(4);
			this.subRules = new List<Rule>(4);
		}

		internal virtual void AddRenderingInstruction(RenderInstruction renderInstruction)
		{
			this.renderInstructions.Add(renderInstruction);
		}

		internal virtual void AddSubRule(Rule rule)
		{
			this.subRules.Add(rule);
		}

		internal virtual void Destroy()
		{
			foreach (RenderInstruction ri in this.renderInstructions)
			{
				ri.Destroy();
			}
			foreach (Rule sr in this.subRules)
			{
				sr.Destroy();
			}
		}

		internal abstract bool MatchesNode(IList<Tag> tags, sbyte zoomLevel);

		internal abstract bool MatchesWay(IList<Tag> tags, sbyte zoomLevel, Closed closed);

		internal virtual void MatchNode(RenderCallback renderCallback, RenderContext renderContext, IList<RenderInstruction> matchingList, PointOfInterest pointOfInterest)
		{
			if (MatchesNode(pointOfInterest.Tags, renderContext.rendererJob.tile.ZoomLevel))
			{
				for (int i = 0, n = this.renderInstructions.Count; i < n; ++i)
				{
					this.renderInstructions[i].RenderNode(renderCallback, renderContext, pointOfInterest);
					matchingList.Add(this.renderInstructions[i]);
				}
				for (int i = 0, n = this.subRules.Count; i < n; ++i)
				{
					this.subRules[i].MatchNode(renderCallback, renderContext, matchingList, pointOfInterest);
				}
			}
		}

		internal virtual void MatchWay(RenderCallback renderCallback, PolylineContainer way, Tile tile, Closed closed, IList<RenderInstruction> matchingList, RenderContext renderContext)
		{
			if (MatchesWay(way.Tags, tile.ZoomLevel, closed))
			{
				for (int i = 0, n = this.renderInstructions.Count; i < n; ++i)
				{
					this.renderInstructions[i].RenderWay(renderCallback, renderContext, way);
					matchingList.Add(this.renderInstructions[i]);
				}
				for (int i = 0, n = this.subRules.Count; i < n; ++i)
				{
					this.subRules[i].MatchWay(renderCallback, way, tile, closed, matchingList, renderContext);
				}
			}
		}

		internal virtual void OnComplete()
		{
			MATCHERS_CACHE_KEY.Clear();
			MATCHERS_CACHE_VALUE.Clear();

			this.renderInstructions.TrimExcess();
			this.subRules.TrimExcess();
			for (int i = 0, n = this.subRules.Count; i < n; ++i)
			{
				this.subRules[i].OnComplete();
			}
		}

		internal virtual void ScaleStrokeWidth(float scaleFactor, sbyte zoomLevel)
		{
			for (int i = 0, n = this.renderInstructions.Count; i < n; ++i)
			{
				this.renderInstructions[i].ScaleStrokeWidth(scaleFactor, zoomLevel);
			}
			for (int i = 0, n = this.subRules.Count; i < n; ++i)
			{
				this.subRules[i].ScaleStrokeWidth(scaleFactor, zoomLevel);
			}
		}

		internal virtual void ScaleTextSize(float scaleFactor, sbyte zoomLevel)
		{
			for (int i = 0, n = this.renderInstructions.Count; i < n; ++i)
			{
				this.renderInstructions[i].ScaleTextSize(scaleFactor, zoomLevel);
			}
			for (int i = 0, n = this.subRules.Count; i < n; ++i)
			{
				this.subRules[i].ScaleTextSize(scaleFactor, zoomLevel);
			}
		}
	}
}