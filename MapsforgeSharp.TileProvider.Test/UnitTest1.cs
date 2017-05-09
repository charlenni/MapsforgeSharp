//
// Copyright 2016 Dirk Weltz
// Copyright 2016 Michael Oed
//
// This program is free software: you can redistribute it and/or modify it under the
// terms of the GNU Lesser General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
// PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with
// this program. If not, see <http://www.gnu.org/licenses/>.
//

namespace org.mapsforge.provider.test
{
	using map.layer.labels;
	using map.model;
	using map.rendertheme.rule;
	using map.rendertheme;
	using MapsforgeSharp.Core.Graphics;
	using MapsforgeSharp.Core.Model;
	using MapsforgeSharp.Graphics;
	using MapsforgeSharp.Reader;
	using NUnit.Framework;
	using org.mapsforge.map.layer.cache;
	using org.mapsforge.map.layer.renderer;
	using PCLStorage;
	using System;

	public class UnitTest1
    {
        static void Main(string[] args)
        {
            var unit = new UnitTest1();
            unit.ExecuteDatabaseRenderer();
        }

// [Test()]
        public void ExecuteDatabaseRenderer()
        {
			var start = DateTime.Now;
			System.Diagnostics.Debug.WriteLine(string.Format("Start: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
            var filename = PortablePath.Combine(new string[] { "andorra.map" });
            var file = FileSystem.Current.LocalStorage.GetFileAsync(PortablePath.Combine(new string[] { "andorra.map" })).Result;
            var mapFile = new MapFile(FileSystem.Current.LocalStorage.GetFileAsync(PortablePath.Combine(new string[] { "andorra.map" })).Result);
            var graphicFactory = new SkiaGraphicFactory();
            var tileCache = new InMemoryTileCache(20);
			var labelStore = new TileBasedLabelStore(1000);

            //var renderer = new DatabaseRenderer(mapFile, graphicFactory, tileCache);
			var renderer = new DatabaseRenderer(mapFile, graphicFactory, labelStore);

			var renderTheme = ExternalRenderTheme.CreateExternalRenderTheme(PortablePath.Combine(new string[] { "osmarender", "osmarender.xml" }));
            var displayModel = new DisplayModel();
            var renderThemeFuture = new RenderThemeFuture(graphicFactory, renderTheme, displayModel);
            // Andorra
            var tile = new Tile(33047, 24202, 16, 256);
            var neighbourTiles = tile.Neighbours;

			System.Diagnostics.Debug.WriteLine(string.Format("Before RenderJob: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			var renderJob = new RendererJob(tile, mapFile, renderThemeFuture, displayModel, 1, false, false);
			System.Diagnostics.Debug.WriteLine(string.Format("After RenderJob: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));

			System.Diagnostics.Debug.WriteLine(string.Format("Before ExecuteJob: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			var tileImage = renderer.ExecuteJob(renderJob);
			System.Diagnostics.Debug.WriteLine(string.Format("After ExecuteJob: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));

			System.Diagnostics.Debug.WriteLine(string.Format("Before Neighbours ExecuteJob: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			foreach (Tile neighbourTile in neighbourTiles)
			{
				renderJob = new RendererJob(neighbourTile, mapFile, renderThemeFuture, displayModel, 1, false, true);
				renderer.ExecuteJob(renderJob);
			}
			System.Diagnostics.Debug.WriteLine(string.Format("After Neighbours ExecuteJob: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));

			System.Diagnostics.Debug.WriteLine(string.Format("Before image write: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			var output = FileSystem.Current.LocalStorage.CreateFileAsync($"Tile.{tile.TileX}.{tile.TileY}.{tile.ZoomLevel}.png", CreationCollisionOption.ReplaceExisting).Result;
			using (var outputStream = output.OpenAsync(FileAccess.ReadAndWrite).Result)
			{
				tileImage.Encode().AsStream().CopyTo(outputStream);
			}
			output = null;
			System.Diagnostics.Debug.WriteLine(string.Format("After image write: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			System.Diagnostics.Debug.WriteLine(string.Format("End: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
			System.Diagnostics.Debug.WriteLine(string.Format("Time to render: {0}", (DateTime.Now - start).ToString()));
		}
	}
}