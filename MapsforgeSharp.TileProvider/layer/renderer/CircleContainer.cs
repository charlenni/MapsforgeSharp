﻿/*
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

namespace org.mapsforge.map.layer.renderer
{
	using Point = org.mapsforge.core.model.Point;

	internal class CircleContainer : ShapeContainer
	{
		internal readonly Point point;
		internal readonly float radius;

		internal CircleContainer(Point point, float radius)
		{
			this.point = point;
			this.radius = radius;
		}

		public virtual ShapeType ShapeType
		{
			get
			{
				return ShapeType.CIRCLE;
			}
		}
	}
}