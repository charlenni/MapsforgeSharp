/*
 * Copyright 2016 Dirk Weltz
 * Copyright 2016 Michael Oed
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

namespace org.mapsforge.provider.test
{
    using NUnit.Framework;
    using PCLStorage;
    using MapsforgeSharp.Graphics;
    using MapsforgeSharp.Core.Graphics;
    using org.mapsforge.map.layer.renderer;
    using MapsforgeSharp.Reader;
    using org.mapsforge.map.layer.cache;
    using map.rendertheme.rule;
    using map.rendertheme;
    using map.model;
    using System;
    using MapsforgeSharp.Core.Model;
    
    public class UnitTest1
    {
        static void Main(string[] args)
        {
            var unit = new UnitTest1();
            unit.ExecuteDatabaseRenderer();
        }

//        [Test()]
        public void ExecuteDatabaseRenderer()
        {
			var start = DateTime.Now;
			System.Diagnostics.Debug.WriteLine(string.Format("Start: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
            var filename = PortablePath.Combine(new string[] { "baden-wuerttemberg.map" });
            var file = FileSystem.Current.LocalStorage.GetFileAsync(PortablePath.Combine(new string[] { "baden-wuerttemberg.map" })).Result;
            var mapFile = new MapFile(FileSystem.Current.LocalStorage.GetFileAsync(PortablePath.Combine(new string[] { "baden-wuerttemberg.map" })).Result);
            var graphicFactory = new SkiaGraphicFactory();
            var tileCache = new InMemoryTileCache(20);

            var renderer = new DatabaseRenderer(mapFile, graphicFactory, tileCache);

            var renderTheme = ExternalRenderTheme.CreateExternalRenderTheme(PortablePath.Combine(new string[] { "osmarender", "osmarender.xml" }));
            var displayModel = new DisplayModel();
            var renderThemeFuture = new RenderThemeFuture(graphicFactory, renderTheme, displayModel);

			System.Diagnostics.Debug.WriteLine(string.Format("Before RenderJob: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			var renderJob = new RendererJob(new Tile(4306, 2831, 13, 256), mapFile, renderThemeFuture, displayModel, 1, false, false);
			//var renderJob = new RendererJob(new core.model.Tile(17220, 11324, 15, 256), mapFile, renderThemeFuture, displayModel, 1, false, false);
			//var renderJob = new RendererJob(new core.model.Tile(34440, 22648, 16, 256), mapFile, renderThemeFuture, displayModel, 1, false, false);
			//var renderJob = new RendererJob(new core.model.Tile(68881, 45297, 17, 256), mapFile, renderThemeFuture, displayModel, 1, false, false);
			System.Diagnostics.Debug.WriteLine(string.Format("After RenderJob: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));

			System.Diagnostics.Debug.WriteLine(string.Format("Before ExecuteJob: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			var tile = renderer.ExecuteJob(renderJob);
			System.Diagnostics.Debug.WriteLine(string.Format("After ExecuteJob: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));

			System.Diagnostics.Debug.WriteLine(string.Format("Before image write: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			var output = FileSystem.Current.LocalStorage.CreateFileAsync("Tile.4305.2831.13.png", CreationCollisionOption.ReplaceExisting).Result;
			using (var outputStream = output.OpenAsync(FileAccess.ReadAndWrite).Result)
			{
				tile.Encode().AsStream().CopyTo(outputStream);
			}
			output = null;
			System.Diagnostics.Debug.WriteLine(string.Format("After image write: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			System.Diagnostics.Debug.WriteLine(string.Format("End: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			System.Diagnostics.Debug.WriteLine(string.Format("Time to render: {0}", (DateTime.Now - start).ToString()));
		}
	}
}