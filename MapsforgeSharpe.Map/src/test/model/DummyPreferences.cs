using System;
using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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
namespace org.mapsforge.map.model
{


	using PreferencesFacade = org.mapsforge.map.model.common.PreferencesFacade;

	internal class DummyPreferences : PreferencesFacade
	{
		private readonly IDictionary<string, object> map = new Dictionary<string, object>();

		public override void clear()
		{
			this.map.Clear();
		}

		public override bool getBoolean(string key, bool defaultValue)
		{
			return ((bool?) this.map[key]).Value;
		}

		public override sbyte getByte(string key, sbyte defaultValue)
		{
			return ((sbyte?) this.map[key]).Value;
		}

		public override double getDouble(string key, double defaultValue)
		{
			return ((double?) this.map[key]).Value;
		}

		public override float getFloat(string key, float defaultValue)
		{
			return ((float?) this.map[key]).Value;
		}

		public override int getInt(string key, int defaultValue)
		{
			return ((int?) this.map[key]).Value;
		}

		public override long getLong(string key, long defaultValue)
		{
			return ((long?) this.map[key]).Value;
		}

		public override string getString(string key, string defaultValue)
		{
			return (string) this.map[key];
		}

		public override void putBoolean(string key, bool value)
		{
			this.map[key] = Convert.ToBoolean(value);
		}

		public override void putByte(string key, sbyte value)
		{
			this.map[key] = Convert.ToSByte(value);
		}

		public override void putDouble(string key, double value)
		{
			this.map[key] = Convert.ToDouble(value);
		}

		public override void putFloat(string key, float value)
		{
			this.map[key] = Convert.ToSingle(value);
		}

		public override void putInt(string key, int value)
		{
			this.map[key] = Convert.ToInt32(value);
		}

		public override void putLong(string key, long value)
		{
			this.map[key] = Convert.ToInt64(value);
		}

		public override void putString(string key, string value)
		{
			this.map[key] = value;
		}

		public override void save()
		{
			// do nothing
		}
	}

}