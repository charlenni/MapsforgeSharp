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
    using Acrotech.PortableLogAdapter;
    using System.Collections.Generic;

    internal sealed class RuleOptimizer
	{
        private static readonly ILogger LOGGER = (new Acrotech.PortableLogAdapter.Managers.DelegateLogManager((logger, message) => System.Diagnostics.Debug.WriteLine("[{0}]{1}", logger.Name, message), LogLevel.Info)).GetLogger(nameof(RuleOptimizer));

        internal static AttributeMatcher optimize(AttributeMatcher attributeMatcher, Stack<Rule> ruleStack)
		{
			if (attributeMatcher is AnyMatcher || attributeMatcher is NegativeMatcher)
			{
				return attributeMatcher;
			}
			else if (attributeMatcher is KeyMatcher)
			{
				return optimizeKeyMatcher(attributeMatcher, ruleStack);
			}
			else if (attributeMatcher is ValueMatcher)
			{
				return optimizeValueMatcher(attributeMatcher, ruleStack);
			}

			throw new System.ArgumentException("unknown AttributeMatcher: " + attributeMatcher);
		}

		internal static ClosedMatcher optimize(ClosedMatcher closedMatcher, Stack<Rule> ruleStack)
		{
			if (closedMatcher is AnyMatcher)
			{
				return closedMatcher;
			}

            foreach(var rule in ruleStack)
            {
				if (rule.closedMatcher.IsCoveredBy(closedMatcher))
				{
					return AnyMatcher.INSTANCE;
				}
				else if (!closedMatcher.IsCoveredBy(rule.closedMatcher))
				{
					LOGGER.Warn("unreachable rule (closed)");
				}
			}

			return closedMatcher;
		}

		internal static ElementMatcher optimize(ElementMatcher elementMatcher, Stack<Rule> ruleStack)
		{
			if (elementMatcher is AnyMatcher)
			{
				return elementMatcher;
			}

            foreach (var rule in ruleStack)
            {
				if (rule.elementMatcher.IsCoveredBy(elementMatcher))
				{
					return AnyMatcher.INSTANCE;
				}
				else if (!elementMatcher.IsCoveredBy(rule.elementMatcher))
				{
					LOGGER.Warn("unreachable rule (e)");
				}
			}

			return elementMatcher;
		}

		private static AttributeMatcher optimizeKeyMatcher(AttributeMatcher attributeMatcher, Stack<Rule> ruleStack)
		{
            foreach (var rule in ruleStack)
            {
				if (rule is PositiveRule)
				{
					PositiveRule positiveRule = (PositiveRule) rule;
					if (positiveRule.keyMatcher.IsCoveredBy(attributeMatcher))
					{
						return AnyMatcher.INSTANCE;
					}
				}
			}

			return attributeMatcher;
		}

		private static AttributeMatcher optimizeValueMatcher(AttributeMatcher attributeMatcher, Stack<Rule> ruleStack)
		{
            foreach (var rule in ruleStack)
            {
                if (rule is PositiveRule)
				{
					PositiveRule positiveRule = (PositiveRule) rule;
					if (positiveRule.valueMatcher.IsCoveredBy(attributeMatcher))
					{
						return AnyMatcher.INSTANCE;
					}
				}
			}

			return attributeMatcher;
		}

		private RuleOptimizer()
		{
			throw new System.InvalidOperationException();
		}
	}
}