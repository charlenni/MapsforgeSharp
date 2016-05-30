/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using PCLStorage;
    using Acrotech.PortableLogAdapter;

    using CorruptedInputStreamException = MapsforgeSharp.Core.Graphics.CorruptedInputStreamException;
    using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
    using ITileBitmap = MapsforgeSharp.Core.Graphics.ITileBitmap;
    using Job = org.mapsforge.map.layer.queue.Job;
    using Observer = org.mapsforge.map.model.common.Observer;

    /// <summary>
    /// A "tilecache" storing map tiles that is prepopulated and never removes any files.
    /// This tile store uses the standard TMS directory layout of zoomlevel/y/x . To support
    /// a different directory structure override the findFile method.
    /// </summary>
    public class TileStore : TileCache
	{

		private readonly IFolder rootDirectory;
		private readonly IGraphicFactory graphicFactory;
		private readonly string suffix;

        private static readonly ILogger LOGGER = (new Acrotech.PortableLogAdapter.Managers.DelegateLogManager((logger, message) => System.Diagnostics.Debug.WriteLine("[{0}]{1}", logger.Name, message), LogLevel.Info)).GetLogger(nameof(TileStore));

        /// <param name="rootDirectory">
        ///            the directory where cached tiles will be stored. </param>
        /// <param name="suffix">
        ///            the suffix for stored tiles. </param>
        /// <param name="graphicFactory">
        ///            the mapsforge graphic factory to create tile data instances. </param>
        /// <exception cref="IllegalArgumentException">
        ///             if the root directory cannot be a tile store </exception>
        public TileStore(IFolder rootDirectory, string suffix, IGraphicFactory graphicFactory)
		{
			if (!CheckFolder(rootDirectory))
			{
				throw new System.ArgumentException("Root directory must be readable");
			}
            this.rootDirectory = rootDirectory;
            this.graphicFactory = graphicFactory;
            this.suffix = suffix;
        }

        private bool CheckFolder(IFolder rootDirectory)
        {
            if (rootDirectory == null)
            {
                return false;
            }

            var fileExists = FileSystem.Current.LocalStorage.CheckExistsAsync(rootDirectory.Path).Result;

            if (fileExists == ExistenceCheckResult.NotFound || fileExists == ExistenceCheckResult.FileExists)
            {
                return false;
            }

            // TODO: We could not check, if we have read access to the folder up to now

            return true;
        }

		public virtual bool ContainsKey(Job key)
		{
			lock (this)
			{
				return this.FindFile(key) != null;
			}
		}

		public virtual void Destroy()
		{
			lock (this)
			{
				// no-op
			}
		}

		public virtual ITileBitmap Get(Job key)
		{
			lock (this)
			{
				IFile file = this.FindFile(key);
        
				if (file == null)
				{
					return null;
				}
        
				try
				{
                    using (System.IO.Stream inputStream = file.OpenAsync(FileAccess.Read).Result)
                    {
                        return this.graphicFactory.CreateTileBitmap(inputStream, key.tile.TileSize, key.hasAlpha);
                    }
				}
				catch (CorruptedInputStreamException)
				{
					// this can happen, at least on Android, when the input stream
					// is somehow corrupted, returning null ensures it will be loaded
					// from another source
					return null;
				}
				catch (IOException)
				{
					return null;
				}
			}
		}

		public virtual int Capacity
		{
			get
			{
				lock (this)
				{
					return int.MaxValue;
				}
			}
		}

		public virtual int CapacityFirstLevel
		{
			get
			{
				lock (this)
				{
					return Capacity;
				}
			}
		}

		public virtual ITileBitmap GetImmediately(Job key)
		{
			return Get(key);
		}

		public virtual void Purge()
		{
			lock (this)
			{
				// no-op
			}
		}

		public virtual void Put(Job key, ITileBitmap bitmap)
		{
			lock (this)
			{
				// no-op
			}
		}

		protected internal virtual IFile FindFile(Job key)
		{
            // Search for file in directory structure zoomLevel/tileX/tileY+suffix
            // slow descent at the moment, better for debugging.
            var zoomLevelFolder = this.rootDirectory.GetFolderAsync(Convert.ToString(key.tile.ZoomLevel)).Result;
			if (zoomLevelFolder == null)
			{
				LOGGER.Info("Failed to find directory " + PCLStorage.PortablePath.Combine(new string[] { this.rootDirectory.Path, Convert.ToString(key.tile.ZoomLevel) }));
				return null;
			}
            var tileXFolder = zoomLevelFolder.GetFolderAsync(Convert.ToString(key.tile.TileX)).Result;
			if (tileXFolder == null)
			{
				LOGGER.Info("Failed to find directory " + PCLStorage.PortablePath.Combine(new string[] { this.rootDirectory.Path, Convert.ToString(key.tile.ZoomLevel), Convert.ToString(key.tile.TileX) }));
				return null;
			}
            var file = tileXFolder.GetFileAsync(Convert.ToString(key.tile.TileY) + this.suffix).Result;
            if (file == null)
            {
                LOGGER.Info("Failed to find file " + PCLStorage.PortablePath.Combine(new string[] { this.rootDirectory.Path, Convert.ToString(key.tile.ZoomLevel), Convert.ToString(key.tile.TileX), Convert.ToString(key.tile.TileY) + this.suffix }));
				return null;
			}
			LOGGER.Info("Found file " + PCLStorage.PortablePath.Combine(new string[] { this.rootDirectory.Path, Convert.ToString(key.tile.ZoomLevel), Convert.ToString(key.tile.TileX), Convert.ToString(key.tile.TileY) + this.suffix }));
			return file;
		}

		public virtual ISet<Job> WorkingSet
		{
			set
			{
				// all tiles are always in the cache
			}
		}

		public void AddObserver(Observer observer)
		{
		}

		public void RemoveObserver(Observer observer)
		{
		}
	}
}