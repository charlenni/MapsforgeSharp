/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

namespace org.mapsforge.map.layer.cache
{
    using Acrotech.PortableLogAdapter;
    using org.mapsforge.core.util;
    using PCLStorage;
    using System.Collections.Generic;

    internal class FileWorkingSetCache<T> : WorkingSetCache<T, IFile>
	{
        private static readonly ILogger LOGGER = (new Acrotech.PortableLogAdapter.Managers.DelegateLogManager((logger, message) => System.Diagnostics.Debug.WriteLine("[{0}]{1}", logger.Name, message), LogLevel.Info)).GetLogger(nameof(FileWorkingSetCache<T>));
        private const long serialVersionUID = 1L;

		internal FileWorkingSetCache(int capacity) : base(capacity)
		{
		}

		protected override bool RemoveEldestEntry(KeyValuePair<T, IFile> eldest)
		{
			if (this.Size() > this.Capacity)
			{
				IFile file = eldest.Value;

                if (file != null)
                {
                    file.DeleteAsync();

                    if (FileSystem.Current.LocalStorage.CheckExistsAsync(file.Path).Result == ExistenceCheckResult.FileExists)
                    {
                        LOGGER.Fatal("could not delete file: " + file);
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}