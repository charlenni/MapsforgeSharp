/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2015 devemux86
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
    using System;
    using System.Threading;
    using Acrotech.PortableLogAdapter;
    using queue;

    using ITileBitmap = MapsforgeSharp.Core.Graphics.ITileBitmap;
    using TileCache = org.mapsforge.map.layer.cache.TileCache;

    public class MapWorkerPool : Runnable
	{

		// the default number of threads is one greater than the number of processors as one thread
		// is likely to be blocked on I/O reading map data. Technically this value can change, so a
		// better implementation, maybe one that also takes the available memory into account, would
		// be good.
		// For stability reasons (see #591), we set default number of threads to 1
		public const int DEFAULT_NUMBER_OF_THREADS = 1; //Runtime.getRuntime().availableProcessors() + 1;
		public static int NUMBER_OF_THREADS = DEFAULT_NUMBER_OF_THREADS;

		public static bool DEBUG_TIMING = false;

		private int concurrentJobs;
		private long totalExecutions;
		private long totalTime;
        private static readonly ILogger LOGGER = (new Acrotech.PortableLogAdapter.Managers.DelegateLogManager((logger, message) => System.Diagnostics.Debug.WriteLine("[{0}]{1}", logger.Name, message), LogLevel.Info)).GetLogger(nameof(MapWorkerPool));

        private readonly DatabaseRenderer databaseRenderer;
		private ExecutorService self;
		private ExecutorService workers;
		private readonly JobQueue<RendererJob> jobQueue;
		private readonly Layer layer;
		private readonly TileCache tileCache;
		private bool inShutdown;

		public MapWorkerPool(TileCache tileCache, JobQueue<RendererJob> jobQueue, DatabaseRenderer databaseRenderer, Layer layer) : base()
		{

			this.tileCache = tileCache;
			this.jobQueue = jobQueue;
			this.databaseRenderer = databaseRenderer;
			this.layer = layer;
			this.inShutdown = false;
		}

		public virtual void start()
		{
			this.self = Executors.newSingleThreadExecutor();
			this.workers = Executors.newFixedThreadPool(NUMBER_OF_THREADS);
			this.self.execute(this);
		}

		public virtual void stop()
		{
			this.inShutdown = true;
			this.self.shutdown();
			this.workers.shutdown();
		}

		public override void run()
		{
			try
			{
				while (!inShutdown)
				{
					RendererJob rendererJob = this.jobQueue.Get(NUMBER_OF_THREADS);
					if (!this.tileCache.ContainsKey(rendererJob) || rendererJob.labelsOnly)
					{
						if (!inShutdown)
						{
							workers.execute(new MapWorker(this, rendererJob));
						}
						else
						{
							jobQueue.Remove(rendererJob);
						}
					}
				}
			}
			catch (InterruptedException e)
			{
				LOGGER.Fatal("MapWorkerPool interrupted", e);
				// should get restarted by the ExecutorService
			}
		}

		internal class MapWorker : Runnable
		{
			private readonly MapWorkerPool outerInstance;

			internal readonly RendererJob rendererJob;

			internal MapWorker(MapWorkerPool outerInstance, RendererJob rendererJob)
			{
				this.outerInstance = outerInstance;
				this.rendererJob = rendererJob;
				this.rendererJob.renderThemeFuture.IncrementRefCount();
			}

			public override void run()
			{
				ITileBitmap bitmap = null;
				try
				{
					long start = 0;

					if (outerInstance.inShutdown)
					{
						return;
					}

					if (DEBUG_TIMING)
					{
						start = DateTimeHelperClass.CurrentUnixTimeMillis();
						LOGGER.Info("ConcurrentJobs " + Interlocked.Increment(ref outerInstance.concurrentJobs));
					}

					bitmap = outerInstance.databaseRenderer.ExecuteJob(rendererJob);

					if (outerInstance.inShutdown)
					{
						return;
					}

					if (!rendererJob.labelsOnly && bitmap != null)
					{
						outerInstance.tileCache.Put(rendererJob, bitmap);
						outerInstance.databaseRenderer.RemoveTileInProgress(rendererJob.tile);
					}
					outerInstance.layer.RequestRedraw();

					if (DEBUG_TIMING)
					{
						long end = DateTimeHelperClass.CurrentUnixTimeMillis();
						long te = Interlocked.Increment(ref outerInstance.totalExecutions);
						long tt = Interlocked.Add(ref outerInstance.totalTime, end - start);
						if (te % 10 == 0)
						{
							LOGGER.Info("TIMING " + Convert.ToString(te) + " " + Convert.ToString(tt / te));
						}
						Interlocked.Decrement(ref outerInstance.concurrentJobs);
					}
				}
				finally
				{
					this.rendererJob.renderThemeFuture.DecrementRefCount();
					outerInstance.jobQueue.Remove(rendererJob);
					if (bitmap != null)
					{
						bitmap.DecrementRefCount();
					}
				}
			}
		}
	}
}