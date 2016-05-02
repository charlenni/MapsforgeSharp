/*
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

namespace org.mapsforge.provider.test
{
    using NUnit.Framework;
    using PCLStorage;

    using org.mapsforge.core.graphics;
    using org.mapsforge.map.layer.renderer;
    using org.mapsforge.reader;
    using org.mapsforge.provider.graphics;
    using org.mapsforge.map.layer.cache;
    using map.rendertheme.rule;
    using map.rendertheme;
    using map.model;

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
            var filename = PortablePath.Combine(new string[] { "baden-wuerttemberg.map" });
            var file = FileSystem.Current.LocalStorage.GetFileAsync(PortablePath.Combine(new string[] { "baden-wuerttemberg.map" })).Result;
            var mapFile = new MapFile(FileSystem.Current.LocalStorage.GetFileAsync(PortablePath.Combine(new string[] { "baden-wuerttemberg.map" })).Result);
            var graphicFactory = new SkiaGraphicFactory();
            var tileCache = new InMemoryTileCache(20);

            var renderer = new DatabaseRenderer(mapFile, graphicFactory, tileCache);

            var renderTheme = ExternalRenderTheme.CreateExternalRenderTheme(PortablePath.Combine(new string[] { "osmarender", "osmarender.xml" }));
            var displayModel = new DisplayModel();
            var renderThemeFuture = new RenderThemeFuture(graphicFactory, renderTheme, displayModel);

            var renderJob = new RendererJob(new core.model.Tile(4305, 4305, 13, 256), mapFile, renderThemeFuture, displayModel, 1, false, false);

            var tile = renderer.ExecuteJob(renderJob);
        }
    }
}
