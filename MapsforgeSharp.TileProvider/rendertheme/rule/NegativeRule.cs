﻿/*
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
namespace org.mapsforge.map.rendertheme.rule
{
    using System.Collections.Generic;

    using Tag = MapsforgeSharp.Core.Model.Tag;

	internal class NegativeRule : Rule
	{
		private readonly AttributeMatcher attributeMatcher;

		internal NegativeRule(RuleBuilder ruleBuilder, AttributeMatcher attributeMatcher) : base(ruleBuilder)
		{

			this.attributeMatcher = attributeMatcher;
		}

		internal override bool MatchesNode(IList<Tag> tags, sbyte zoomLevel)
		{
			return this.zoomMin <= zoomLevel && this.zoomMax >= zoomLevel && this.elementMatcher.Matches(Element.Node) && this.attributeMatcher.Matches(tags);
		}

		internal override bool MatchesWay(IList<Tag> tags, sbyte zoomLevel, Closed closed)
		{
			return this.zoomMin <= zoomLevel && this.zoomMax >= zoomLevel && this.elementMatcher.Matches(Element.Way) && this.closedMatcher.Matches(closed) && this.attributeMatcher.Matches(tags);
		}
	}
}