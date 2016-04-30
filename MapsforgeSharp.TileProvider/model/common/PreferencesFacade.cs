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

namespace org.mapsforge.map.model.common
{
	public interface PreferencesFacade
	{
		void Clear();

		bool GetBoolean(string key, bool defaultValue);

		sbyte GetByte(string key, sbyte defaultValue);

		double GetDouble(string key, double defaultValue);

		float GetFloat(string key, float defaultValue);

		int GetInt(string key, int defaultValue);

		long GetLong(string key, long defaultValue);

		string GetString(string key, string defaultValue);

		void PutBoolean(string key, bool value);

		void PutByte(string key, sbyte value);

		void PutDouble(string key, double value);

		void PutFloat(string key, float value);

		void PutInt(string key, int value);

		void PutLong(string key, long value);

		void PutString(string key, string value);

		void Save();
	}
}