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
	internal sealed class ElementNodeMatcher : ElementMatcher
	{
		internal static readonly ElementNodeMatcher INSTANCE = new ElementNodeMatcher();

		private ElementNodeMatcher()
		{
			// do nothing
		}

		public bool IsCoveredBy(ElementMatcher elementMatcher)
		{
			return elementMatcher.Matches(Element.NODE);
		}

		public bool Matches(Element element)
		{
			return element == Element.NODE;
		}
	}
}