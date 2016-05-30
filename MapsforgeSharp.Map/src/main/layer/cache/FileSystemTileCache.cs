/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
 * Copyright 2014 mvglasow <michael -at- vonglasow.com>
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

namespace org.mapsforge.map.layer.cache
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.IO;
    using Acrotech.PortableLogAdapter;
    using PCLStorage;
    using System.Threading.Tasks;

    using CorruptedInputStreamException = MapsforgeSharp.Core.Graphics.CorruptedInputStreamException;
    using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
    using ITileBitmap = MapsforgeSharp.Core.Graphics.ITileBitmap;
    using Job = org.mapsforge.map.layer.queue.Job;
    using Observable = org.mapsforge.map.model.common.Observable;
    using Observer = org.mapsforge.map.model.common.Observer;

    /// <summary>
    /// A thread-safe cache for image files with a fixed size and LRU policy.
    /// <para>
    /// A {@code FileSystemTileCache} caches tiles in a dedicated path in the file system, specified in the constructor.
    /// </para>
    /// <para>
    /// When used for a <seealso cref="org.mapsforge.map.layer.renderer.TileRendererLayer"/>, persistent caching may result in clipped
    /// labels when tiles from different instances are used. To work around this, either display labels in a separate
    /// <seealso cref="org.mapsforge.map.layer.labels.LabelLayer"/> (experimental) or disable persistence as described in
    /// <seealso cref="#FileSystemTileCache(int, File, GraphicFactory, boolean)"/>.
    /// 
    /// Note: previously the FileSystemTileCache utilized threading to speed up response times. This is not the
    /// case anymore and the constructors have been removed.
    /// </para>
    /// </summary>
    public class FileSystemTileCache : TileCache
	{
		internal const string FILE_EXTENSION = ".tile";
        private static readonly ILogger LOGGER = (new Acrotech.PortableLogAdapter.Managers.DelegateLogManager((logger, message) => System.Diagnostics.Debug.WriteLine("[{0}]{1}", logger.Name, message), LogLevel.Info)).GetLogger(nameof(FileSystemTileCache));

        /// <summary>
        /// Runnable that reads the cache directory and re-populates the cache with data saved by previous instances.
        /// <para>
        /// This method assumes tile files to have a file extension of <seealso cref="#FILE_EXTENSION"/> and reside in a second-level
        /// of subdir of the cache dir (as in the standard TMS directory layout of zoomlevel/x/y). The relative path to the
        /// cached tile, after stripping the extension, is used as the lookup key.
        /// </para>
        /// </summary>
        private class CacheDirectoryReader
		{
			private readonly FileSystemTileCache outerInstance;

			public CacheDirectoryReader(FileSystemTileCache outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void Run()
			{
				var zFolders = outerInstance.cacheDirectory.GetFoldersAsync().Result;
				if (zFolders != null)
				{
					foreach (IFolder z in zFolders)
					{
						var xFolders = z.GetFoldersAsync().Result;
						if (xFolders != null)
						{
							foreach (IFolder x in xFolders)
							{
								var yFiles = x.GetFilesAsync().Result;
								if (yFiles != null)
								{
									foreach (IFile y in yFiles)
									{
										if (IsValidFile(y) && y.Name.EndsWith(FILE_EXTENSION))
										{
											int index = y.Name.LastIndexOf(FILE_EXTENSION);
											string key = Job.ComposeKey(z.Name, x.Name, y.Name.Substring(0, index));
											try
											{
												outerInstance.@lock.writeLock().@lock();
												if (outerInstance.lruCache.Add(key, y) != null)
												{
													LOGGER.Warn("overwriting cached entry: " + key);
												}
											}
											finally
											{
												outerInstance.@lock.writeLock().unlock();
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Determines whether a File instance refers to a valid cache directory.
		/// <para>
		/// This method checks that {@code folder} refers to a directory to which the current process has read and write
		/// access. If the directory does not exist, it will be created.
		/// 
		/// </para>
		/// </summary>
		/// <param name="folder">
		///            The Folder instance to examine. This can be null, which will cause the method to return {@code false}. </param>
		private static bool IsValidCacheDirectory(IFolder folder)
		{
            if (folder == null)
            {
                return false;
            }

            if (FileSystem.Current.LocalStorage.CheckExistsAsync(folder.Path).Result != ExistenceCheckResult.FolderExists)
            {
                var newFolder = FileSystem.Current.LocalStorage.CreateFolderAsync(folder.Path, CreationCollisionOption.FailIfExists).Result;
            }

			return true;
		}

		/// <summary>
		/// Determines whether a File instance refers to a valid file which can be read.
		/// <para>
		/// This method checks that {@code file} refers to an existing file to which the current process has read access. It
		/// does not create directories and not verify that the directory is writable. If you need this behavior, use
		/// <seealso cref="#isValidCacheDirectory(File)"/> instead.
		/// 
		/// </para>
		/// </summary>
		/// <param name="file">
		///            The File instance to examine. This can be null, which will cause the method to return {@code false}. </param>
		private static bool IsValidFile(IFile file)
		{
			return file != null && FileSystem.Current.LocalStorage.CheckExistsAsync(file.Path).Result == ExistenceCheckResult.FileExists;
		}

		/// <summary>
		/// Recursively deletes directory and all files. See
		/// http://stackoverflow.com/questions/3775694/deleting-folder-from-java/3775723#3775723
		/// </summary>
		/// <param name="folder">
		///            the directory to delete with all its content </param>
		/// <returns> true if directory and all content has been deleted, false if not </returns>
		private static bool DeleteDirectory(IFolder folder)
		{
			if (folder == null)
			{
				return false;
			}

            var files = folder.GetFilesAsync().Result;

            foreach(var file in files)
            {
                file.DeleteAsync().RunSynchronously();
            }

            var subFolders = folder.GetFoldersAsync().Result;

            foreach(var subFolder in subFolders)
            {
                var success = DeleteDirectory(subFolder);
                if (!success)
                {
                    return false;
                }
            }

			// The directory is now empty so delete it
			folder.DeleteAsync().RunSynchronously();

            return true;
		}

		private readonly IFolder cacheDirectory;
		private readonly IGraphicFactory graphicFactory;
		private FileWorkingSetCache<string> lruCache;
		private readonly ReentrantReadWriteLock @lock;
		private readonly Observable observable;
		private readonly bool persistent;

		/// <summary>
		/// Compatibility constructor that creates a non-threaded, non-persistent FSTC.
		/// </summary>
		/// <param name="capacity">
		///            the maximum number of entries in this cache. </param>
		/// <param name="cacheDirectory">
		///            the directory where cached tiles will be stored. </param>
		/// <param name="graphicFactory">
		///            the graphicFactory implementation to use. </param>
		/// <exception cref="IllegalArgumentException">
		///             if the capacity is negative. </exception>
		public FileSystemTileCache(int capacity, IFolder cacheDirectory, IGraphicFactory graphicFactory) : this(capacity, cacheDirectory, graphicFactory, false)
		{
		}

		/// <summary>
		/// Creates a new FileSystemTileCache.
		/// <para>
		/// Use the {@code persistent} argument to specify whether cache contents should be kept across instances. A
		/// persistent cache will serve any tiles it finds in {@code cacheDirectory}. Calling <seealso cref="#destroy()"/> on a
		/// persistent cache will not delete the cache directory. Conversely, a non-persistent cache will serve only tiles
		/// added to it via the <seealso cref="#put(Job, TileBitmap)"/> method, and calling <seealso cref="#destroy()"/> on a non-persistent
		/// cache will delete {@code cacheDirectory}.
		/// 
		/// </para>
		/// </summary>
		/// <param name="capacity">
		///            the maximum number of entries in this cache. </param>
		/// <param name="cacheDirectory">
		///            the directory where cached tiles will be stored. </param>
		/// <param name="graphicFactory">
		///            the graphicFactory implementation to use. </param>
		/// <param name="persistent">
		///            if cache data will be kept between instances </param>
		/// <exception cref="IllegalArgumentException">
		///             if the capacity is negative. </exception>
		/// <exception cref="IllegalArgumentException">
		///             if the capacity is negative. </exception>
		public FileSystemTileCache(int capacity, IFolder cacheDirectory, IGraphicFactory graphicFactory, bool persistent)
		{
			this.observable = new Observable();
			this.persistent = persistent;
			this.lruCache = new FileWorkingSetCache<string>(capacity);
			this.@lock = new ReentrantReadWriteLock();
			if (IsValidCacheDirectory(cacheDirectory))
			{
				this.cacheDirectory = cacheDirectory;
				if (this.persistent)
				{
                    // this will start a new thread to read in the cache directory.
                    // there is the potential that files will be recreated because they
                    // are not yet in the cache, but this will not cause any corruption.
                    Task.Run(() => (new CacheDirectoryReader(this)).Run());
				}
			}
			else
			{
				this.cacheDirectory = null;
			}
			this.graphicFactory = graphicFactory;
		}

		public virtual bool ContainsKey(Job key)
		{
			try
			{
				@lock.readLock().@lock();
				// if we are using a threaded cache we return true if the tile is still in the
				// queue to reduce double rendering
				return this.lruCache.ContainsKey(key.Key);
			}
			finally
			{
				@lock.readLock().unlock();
			}
		}

		/// <summary>
		/// Destroys this cache.
		/// <para>
		/// Applications are expected to call this method when they no longer require the cache.
		/// </para>
		/// <para>
		/// If the cache is not persistent, calling this method is equivalent to calling <seealso cref="#purge()"/>. If the cache is
		/// persistent, it does nothing.
		/// </para>
		/// <para>
		/// Beginning with 0.5.1, accessing the cache after calling {@code destroy()} is discouraged. In order to empty the
		/// cache and force all tiles to be re-rendered or re-requested from the source, use <seealso cref="#purge()"/> instead.
		/// Earlier versions lacked the <seealso cref="#purge()"/> method and used {@code destroy()} instead, but this practice is now
		/// discouraged and may lead to unexpected results when used with features introduced in 0.5.1 or later.
		/// </para>
		/// </summary>
		public virtual void Destroy()
		{
			if (!this.persistent)
			{
				Purge();
			}
		}

		public virtual ITileBitmap Get(Job key)
		{
			IFile file;
			try
			{
				@lock.readLock().@lock();
				file = this.lruCache.Get(key.Key);
			}
			finally
			{
				@lock.readLock().unlock();
			}
			if (file == null)
			{
				return null;
			}

            try
            {
                using (System.IO.Stream inputStream = file.OpenAsync(FileAccess.Read).Result)
                {
                    ITileBitmap result = this.graphicFactory.CreateTileBitmap(inputStream, key.tile.TileSize, key.hasAlpha);
                    result.Timestamp = file.lastModified();
                    return result;
                }
            }
            catch (CorruptedInputStreamException e)
            {
                // this can happen, at least on Android, when the input stream
                // is somehow corrupted, returning null ensures it will be loaded
                // from another source
                Remove(key);
                LOGGER.Warn("input stream from file system cache invalid " + key.Key, e);
                return null;
            }
            catch (IOException e)
            {
                Remove(key);
                LOGGER.Fatal(e.Message, e);
                return null;
            }
		}

		public virtual int Capacity
		{
			get
			{
				try
				{
					@lock.readLock().@lock();
					return this.lruCache.Capacity;
				}
				finally
				{
					@lock.readLock().unlock();
				}
			}
		}

		public virtual int CapacityFirstLevel
		{
			get
			{
				return Capacity;
			}
		}

		public virtual ITileBitmap GetImmediately(Job key)
		{
			return Get(key);
		}

		/// <summary>
		/// Whether the cache is persistent.
		/// </summary>
		public virtual bool Persistent
		{
			get
			{
				return this.persistent;
			}
		}

		/// <summary>
		/// Purges this cache.
		/// <para>
		/// Calls to <seealso cref="#get(Job)"/> issued after purging will not return any tiles added before the purge operation.
		/// Purging will also delete the cache directory on disk, freeing up disk space.
		/// </para>
		/// <para>
		/// Applications should purge the tile cache when map model parameters change, such as the render style for locally
		/// rendered tiles, or the source for downloaded tiles. Applications which frequently alternate between a limited
		/// number of map model configurations may want to consider using a different cache for each.
		/// 
		/// @since 0.5.1
		/// </para>
		/// </summary>
		public virtual void Purge()
		{
			try
			{
				this.@lock.writeLock().@lock();
				this.lruCache.Clear();
			}
			finally
			{
				this.@lock.writeLock().unlock();
			}

			DeleteDirectory(this.cacheDirectory);
		}

		public virtual void Put(Job key, ITileBitmap bitmap)
		{
			if (key == null)
			{
				throw new System.ArgumentException("key must not be null");
			}
			else if (bitmap == null)
			{
				throw new System.ArgumentException("bitmap must not be null");
			}

			if (Capacity == 0)
			{
				return;
			}

			StoreData(key, bitmap);
			this.observable.NotifyObservers();
		}

		public virtual ISet<Job> WorkingSet
		{
			set
			{
				ISet<string> workingSetInteger = new HashSet<string>();
				foreach (Job job in value)
				{
					workingSetInteger.Add(job.Key);
				}
				this.lruCache.WorkingSet = workingSetInteger;
			}
		}

		public void AddObserver(Observer observer)
		{
			this.observable.AddObserver(observer);
		}

		public void RemoveObserver(Observer observer)
		{
			this.observable.RemoveObserver(observer);
		}

		private IFile GetOutputFile(Job job)
		{
			string file = PCLStorage.PortablePath.Combine(new string[] { this.cacheDirectory.Path, job.Key });
			string dir = file.Substring(0, file.LastIndexOf(PCLStorage.PortablePath.DirectorySeparatorChar));
			if (IsValidCacheDirectory(FileSystem.Current.GetFolderFromPathAsync(dir).Result))
			{
				return FileSystem.Current.GetFolderFromPathAsync(dir).Result.CreateFileAsync(file + FILE_EXTENSION, CreationCollisionOption.ReplaceExisting).Result;
			}
			return null;
		}

		private void Remove(Job key)
		{
			try
			{
				@lock.writeLock().@lock();
				this.lruCache.Remove(key.Key);
			}
			finally
			{
				@lock.writeLock().unlock();
			}
		}

		/// <summary>
		/// stores the bitmap data on disk with filename key
		/// </summary>
		/// <param name="key">
		///            filename </param>
		/// <param name="bitmap">
		///            tile image </param>
		private void StoreData(Job key, ITileBitmap bitmap)
		{
			try
			{
				IFile file = GetOutputFile(key);
				if (file == null)
				{
					// if the file cannot be written, silently return
					return;
				}
                using (System.IO.Stream outputStream = file.OpenAsync(FileAccess.ReadAndWrite).Result)
                {
                    bitmap.Compress(outputStream);
                    try
                    {
                        @lock.writeLock().@lock();
                        if (this.lruCache.Add(key.Key, file) != null)
                        {
                            LOGGER.Warn("overwriting cached entry: " + key.Key);
                        }
                    }
                    finally
                    {
                        @lock.writeLock().unlock();
                    }
                }
			}
			catch (Exception e)
			{
				// we are catching now any exception and then disable the file cache
				// this should ensure that no exception in the storage thread will
				// ever crash the main app. If there is a runtime exception, the thread
				// will exit (via destroy).
				LOGGER.Fatal("Disabling filesystem cache", e);
				// most likely cause is that the disk is full, just disable the
				// cache otherwise
				// more and more exceptions will be thrown.
				this.Destroy();
				try
				{
					@lock.writeLock().@lock();
					this.lruCache = new FileWorkingSetCache<string>(0);
				}
				finally
				{
					@lock.writeLock().unlock();
				}
			}
		}
	}
}