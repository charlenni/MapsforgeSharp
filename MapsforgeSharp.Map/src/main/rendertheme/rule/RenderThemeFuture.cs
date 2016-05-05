/*
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

namespace org.mapsforge.map.rendertheme.rule
{
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.IO;

    using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;

    /// <summary>
    /// A RenderThemeFuture implements a asynchronous parsing of an XmlRenderTheme in order to
    /// move the delay caused by parsing the XML file off the user interface thread in mapsforge
    /// application.
    /// The RenderThemeFuture is reference counted to make it shareable between threads. Each thread
    /// that uses the RenderThemeFuture to retrieve a rendertheme should first call incrementRefCount to
    /// ensure that the RenderTheme does not get destroyed while the thread is waiting for execution.
    /// </summary>
    public class RenderThemeFuture //: Task<RenderTheme>
	{
		private int refCount = 1;
        private readonly Task<RenderTheme> task;

        /// <summary>
        /// Callable that performs the actual parsing of the render theme (via the RenderThemeHandler
        /// as before).
        /// </summary>
        private class RenderThemeCallable
		{
			internal readonly GraphicFactory graphicFactory;
			internal readonly IXmlRenderTheme xmlRenderTheme;
			internal readonly DisplayModel displayModel;

			public RenderThemeCallable(GraphicFactory graphicFactory, IXmlRenderTheme xmlRenderTheme, DisplayModel displayModel)
			{
				this.graphicFactory = graphicFactory;
				this.xmlRenderTheme = xmlRenderTheme;
				this.displayModel = displayModel;
			}

			public RenderTheme Call()
			{
				if (xmlRenderTheme == null || this.displayModel == null)
				{
					return null;
				}
				try
				{
					return RenderThemeHandler.GetRenderTheme(this.graphicFactory, displayModel, this.xmlRenderTheme);
				}
				catch (IOException e)
				{
					throw new System.ArgumentException("File error for XML rendertheme", e);
				}
			}
		}

		public RenderThemeFuture(GraphicFactory graphicFactory, IXmlRenderTheme xmlRenderTheme, DisplayModel displayModel)
		{
            RenderThemeCallable callable = new RenderThemeCallable(graphicFactory, xmlRenderTheme, displayModel);

            task = new Task<RenderTheme>(() => { return callable.Call(); });
            task.Start();
        }

        public RenderTheme Result
        {
            get { return task.Result; }
        }

		public virtual void DecrementRefCount()
		{
			int c = Interlocked.Decrement(ref this.refCount);
			if (c <= 0)
			{
                // TODO: What's going on, when refCount == 0
				try
				{
					if (task.IsCompleted)
					{
					}
					else
					{
					}
				}
				catch (Exception)
				{
					// just cleaning up
				}
			}
		}

		public virtual void IncrementRefCount()
		{
			Interlocked.Increment(ref this.refCount);
		}
	}
}