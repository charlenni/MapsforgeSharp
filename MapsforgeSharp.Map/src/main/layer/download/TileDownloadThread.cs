/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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
namespace org.mapsforge.map.layer.download
{
    using Acrotech.PortableLogAdapter;
    using System.IO;
    using queue;

    using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
    using TileBitmap = MapsforgeSharp.Core.Graphics.TileBitmap;
    using TileCache = org.mapsforge.map.layer.cache.TileCache;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using PausableThread = org.mapsforge.map.util.PausableThread;

    internal class TileDownloadThread : PausableThread
	{
        private static readonly ILogger LOGGER = (new Acrotech.PortableLogAdapter.Managers.DelegateLogManager((logger, message) => System.Diagnostics.Debug.WriteLine("[{0}]{1}", logger.Name, message), LogLevel.Info)).GetLogger(nameof(TileDownloadThread));

        private readonly DisplayModel displayModel;
		private readonly GraphicFactory graphicFactory;
		private JobQueue<DownloadJob> jobQueue;
		private readonly Layer layer;
		private readonly TileCache tileCache;

		internal TileDownloadThread(TileCache tileCache, JobQueue<DownloadJob> jobQueue, Layer layer, GraphicFactory graphicFactory, DisplayModel displayModel) : base()
		{

			this.tileCache = tileCache;
			this.jobQueue = jobQueue;
			this.layer = layer;
			this.graphicFactory = graphicFactory;
			this.displayModel = displayModel;
		}

		public virtual JobQueue<DownloadJob> JobQueue
		{
			set
			{
				this.jobQueue = value;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected void doWork() throws InterruptedException
		protected internal override void DoWork()
		{
			DownloadJob downloadJob = this.jobQueue.get();

			try
			{
				if (!this.tileCache.ContainsKey(downloadJob))
				{
					downloadTile(downloadJob);
				}
			}
			catch (IOException e)
			{
				LOGGER.Fatal(e.Message, e);
			}
			finally
			{
				this.jobQueue.Remove(downloadJob);
			}
		}

		protected internal override ThreadPriority ThreadPriority
		{
			get
			{
				return ThreadPriority.BELOW_NORMAL;
			}
		}

		protected internal override bool HasWork()
		{
			return true;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void downloadTile(DownloadJob downloadJob) throws java.io.IOException
		private void downloadTile(DownloadJob downloadJob)
		{
			TileDownloader tileDownloader = new TileDownloader(downloadJob, this.graphicFactory);
			TileBitmap bitmap = tileDownloader.DownloadImage();

			if (!Interrupted && bitmap != null)
			{
				bitmap.ScaleTo(this.displayModel.TileSize, this.displayModel.TileSize);
				this.tileCache.Put(downloadJob, bitmap);
				this.layer.RequestRedraw();
			}
		}
	}

}