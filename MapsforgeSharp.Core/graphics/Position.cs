/*
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2014 devemux86
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

using System;

namespace MapsforgeSharp.Core.Graphics
{
	public enum Position
	{
		Auto,
		Center,
		Below,
		BelowLeft,
		BelowRight,
		Above,
		AboveLeft,
		AboveRight,
		Left,
		Right
	}

	public static class PositionHelper
	{
		public static Position ToPosition(this string value)
		{
			Position ret;
			if (!Enum.TryParse<Position>(value, true, out ret))
			{
				throw new System.ArgumentException("Invalid value for Position: " + value);
			}
			return ret;
		}
	}
}