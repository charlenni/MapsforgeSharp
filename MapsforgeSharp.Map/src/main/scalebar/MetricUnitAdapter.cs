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

namespace org.mapsforge.map.scalebar
{
	public sealed class MetricUnitAdapter : DistanceUnitAdapter
	{
		public static readonly MetricUnitAdapter INSTANCE = new MetricUnitAdapter();
		private const int ONE_KILOMETER = 1000;
		private static readonly int[] SCALE_BAR_VALUES = new int[] {10000000, 5000000, 2000000, 1000000, 500000, 200000, 100000, 50000, 20000, 10000, 5000, 2000, 1000, 500, 200, 100, 50, 20, 10, 5, 2, 1};

		private MetricUnitAdapter()
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
			if (mapScaleValue < ONE_KILOMETER)
			{
				return mapScaleValue + " m";
			}
			return (mapScaleValue / ONE_KILOMETER) + " km";
		}
	}
}