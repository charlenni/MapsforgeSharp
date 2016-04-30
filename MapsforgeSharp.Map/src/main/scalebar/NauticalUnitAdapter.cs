/*
 * Copyright 2014 Christian Pesch
 * Copyright 2014, 2015 devemux86
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

namespace org.mapsforge.map.scalebar
{
	public sealed class NauticalUnitAdapter : DistanceUnitAdapter
	{
		public static readonly NauticalUnitAdapter INSTANCE = new NauticalUnitAdapter();
		private const int ONE_MILE = 1852;
		private static readonly int[] SCALE_BAR_VALUES = new int[] {9260000, 3704000, 1852000, 926000, 370400, 185200, 92600, 37040, 18520, 9260, 3704, 1852, 926, 500, 200, 100, 50, 20, 10, 5, 2, 1};

		private NauticalUnitAdapter()
		{
			// do nothing
		}

		public double MeterRatio
		{
			get
			{
				return 1;
			}
		}

		public int[] ScaleBarValues
		{
			get
			{
				return SCALE_BAR_VALUES;
			}
		}

		public string GetScaleText(int mapScaleValue)
		{
			if (mapScaleValue < ONE_MILE / 2)
			{
				return mapScaleValue + " m";
			}
			if (mapScaleValue == ONE_MILE / 2)
			{
				return "0.5 nmi";
			}
			return (mapScaleValue / ONE_MILE) + " nmi";
		}
	}
}