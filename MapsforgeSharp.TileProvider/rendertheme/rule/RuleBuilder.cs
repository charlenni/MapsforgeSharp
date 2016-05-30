/*
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
    using System;
    using System.Collections.Generic;
    using System.Xml;

    /// <summary>
    /// A builder for <seealso cref="Rule"/> instances.
    /// </summary>
    public class RuleBuilder
	{
		private const string CAT = "cat";
		private const string CLOSED = "closed";
		private const string E = "e";
		private const string K = "k";
		private static readonly string[] SPLIT_PATTERN = new string[] { "|" };
		private const string STRING_NEGATION = "~";
		private const string STRING_WILDCARD = "*";
		private const string V = "v";
		private const string ZOOM_MAX = "zoom-max";
		private const string ZOOM_MIN = "zoom-min";

		private static ClosedMatcher GetClosedMatcher(Closed closed)
		{
            if (closed == Closed.Yes)
            {
                return ClosedWayMatcher.INSTANCE;
            }
            if (closed == Closed.No)
            {
                return LinearWayMatcher.INSTANCE;
            }
            if (closed == Closed.Any)
            {
				return AnyMatcher.INSTANCE;
			}

			throw new System.ArgumentException("unknown closed value: " + closed);
		}

		private static ElementMatcher getElementMatcher(Element element)
		{
            if (element == Element.Node)
            {
                return ElementNodeMatcher.INSTANCE;
            }
            if (element == Element.Way)
            {
                return ElementWayMatcher.INSTANCE;
            }
            if (element == Element.Any)
            {
				return AnyMatcher.INSTANCE;
			}

			throw new System.ArgumentException("unknown element value: " + element);
		}

		private static AttributeMatcher GetKeyMatcher(IList<string> keyList)
		{
			if (STRING_WILDCARD.Equals(keyList[0]))
			{
				return AnyMatcher.INSTANCE;
			}

            AttributeMatcher attributeMatcher;
            
			if (!Rule.MATCHERS_CACHE_KEY.TryGetValue(keyList, out attributeMatcher))
			{
				attributeMatcher = new KeyMatcher(keyList);
				Rule.MATCHERS_CACHE_KEY.Add(keyList, attributeMatcher);
			}
			return attributeMatcher;
		}

		private static AttributeMatcher GetValueMatcher(IList<string> valueList)
		{
			if (STRING_WILDCARD.Equals(valueList[0]))
			{
				return AnyMatcher.INSTANCE;
			}

            AttributeMatcher attributeMatcher;
			if (!Rule.MATCHERS_CACHE_VALUE.TryGetValue(valueList, out attributeMatcher))
			{
				attributeMatcher = new ValueMatcher(valueList);
				Rule.MATCHERS_CACHE_VALUE.Add(valueList, attributeMatcher);
			}
			return attributeMatcher;
		}

		internal string cat;
		internal ClosedMatcher closedMatcher;
		internal ElementMatcher elementMatcher;
		internal sbyte zoomMax;
		internal sbyte zoomMin;
		private Closed closed;
		private Element element;
		private IList<string> keyList;
		private string keys;
		private readonly Stack<Rule> ruleStack;
		private IList<string> valueList;
		private string values;

		public RuleBuilder(string elementName, XmlReader reader, Stack<Rule> ruleStack)
		{
			this.ruleStack = ruleStack;

			this.closed = Closed.Any;
			this.zoomMin = 0;
			this.zoomMax = sbyte.MaxValue;

			ExtractValues(elementName, reader);
		}

		/// <returns> a new {@code Rule} instance. </returns>
		public virtual Rule Build()
		{
			if (this.valueList.Remove(STRING_NEGATION))
			{
				AttributeMatcher attributeMatcher = new NegativeMatcher(this.keyList, this.valueList);
				return new NegativeRule(this, attributeMatcher);
			}

			AttributeMatcher keyMatcher = GetKeyMatcher(this.keyList);
			AttributeMatcher valueMatcher = GetValueMatcher(this.valueList);

			keyMatcher = RuleOptimizer.optimize(keyMatcher, this.ruleStack);
			valueMatcher = RuleOptimizer.optimize(valueMatcher, this.ruleStack);

			return new PositiveRule(this, keyMatcher, valueMatcher);
		}

		private void ExtractValues(string elementName, XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
                reader.MoveToAttribute(i);

				string name = reader.Name;
				string value = reader.Value;

				if (E.Equals(name))
				{
					this.element = value.ToElement();
				}
				else if (K.Equals(name))
				{
					this.keys = value;
				}
				else if (V.Equals(name))
				{
					this.values = value;
				}
				else if (CAT.Equals(name))
				{
					this.cat = value;
				}
				else if (CLOSED.Equals(name))
				{
					this.closed = value.ToClosed();
				}
				else if (ZOOM_MIN.Equals(name))
				{
					this.zoomMin = XmlUtils.ParseNonNegativeByte(name, value);
				}
				else if (ZOOM_MAX.Equals(name))
				{
					this.zoomMax = XmlUtils.ParseNonNegativeByte(name, value);
				}
				else
				{
					throw XmlUtils.CreateXmlReaderException(elementName, name, value, i);
				}
			}

			Validate(elementName);

			this.keyList = new List<string>(this.keys.Split(SPLIT_PATTERN, StringSplitOptions.RemoveEmptyEntries));
            this.valueList = new List<string>(this.values.Split(SPLIT_PATTERN, StringSplitOptions.RemoveEmptyEntries));

			this.elementMatcher = getElementMatcher(this.element);
			this.closedMatcher = GetClosedMatcher(this.closed);

			this.elementMatcher = RuleOptimizer.optimize(this.elementMatcher, this.ruleStack);
			this.closedMatcher = RuleOptimizer.optimize(this.closedMatcher, this.ruleStack);
		}

		private void Validate(string elementName)
		{
			XmlUtils.CheckMandatoryAttribute(elementName, E, this.element);
			XmlUtils.CheckMandatoryAttribute(elementName, K, this.keys);
			XmlUtils.CheckMandatoryAttribute(elementName, V, this.values);

			if (this.zoomMin > this.zoomMax)
			{
				throw new InvalidOperationException('\'' + ZOOM_MIN + "' > '" + ZOOM_MAX + "': " + this.zoomMin + ' ' + this.zoomMax);
			}
		}
	}
}