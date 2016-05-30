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

namespace MapsforgeSharp.Graphics
{
	using System;
	using System.IO;
	using System.Reflection;
	using SkiaSharp;
	using MapsforgeSharp.Core.Mapelements;
	using MapsforgeSharp.Core.Model;
	using MapsforgeSharp.Core.Graphics;

	public class SkiaGraphicFactory : IGraphicFactory
    {
        private static readonly string PREFIX_ASSETS = "assets:";

        public IBitmap CreateBitmap(int width, int height)
        {
            return new SkiaBitmap(width, height);
        }

        public IBitmap CreateBitmap(int width, int height, bool isTransparent)
        {
            throw new NotImplementedException();
        }

        public Canvas CreateCanvas(int width, int height)
        {
            return new SkiaCanvas(width, height);
        }

        public int CreateColor(int color)
        {
            return color; 
        }

        public int CreateColor(int alpha, int red, int green, int blue)
        {
            return (int)(alpha << 24 & red << 16 & green << 8 & blue);
        }

        public IMatrix CreateMatrix()
        {
            return new SkiaMatrix();
        }

        public IPaint CreatePaint()
        {
            return new SkiaPaint();
        }

        public IPaint CreatePaint(IPaint paint)
        {
            return new SkiaPaint(paint);
        }

        public MapsforgeSharp.Core.Graphics.IPath CreatePath()
        {
            return new SkiaPath();
        }

        public PointTextContainer CreatePointTextContainer(Point xy, Display display, int priority, string text, IPaint paintFront, IPaint paintBack, SymbolContainer symbolContainer, Position position, int maxTextWidth)
        {
			return new SkiaPointTextContainer(xy, display, priority, text, paintFront, paintBack, symbolContainer, position, maxTextWidth);
        }

        public IResourceBitmap CreateResourceBitmap(Stream inputStream, int hash)
        {
            throw new NotImplementedException();
        }

        public ITileBitmap CreateTileBitmap(int tileSize, bool isTransparent)
        {
            return new SkiaTileBitmap(tileSize, isTransparent);
        }

        public ITileBitmap CreateTileBitmap(Stream inputStream, int tileSize, bool isTransparent)
        {
            throw new NotImplementedException();
        }

        public Stream PlatformSpecificSources(string relativePathPrefix, string src)
        {
            // this allows loading of resource bitmaps from the Andorid assets folder
            if (src.StartsWith(PREFIX_ASSETS))
            {
                string pathName = (string.IsNullOrEmpty(relativePathPrefix) ? "" : relativePathPrefix) + src.Substring(PREFIX_ASSETS.Length);
                Stream inputStream = typeof(SkiaGraphicFactory).GetTypeInfo().Assembly.GetManifestResourceStream(pathName);

                if (inputStream == null)
                {
                    throw new FileNotFoundException("resource not found: " + pathName);
                }

                return inputStream;
            }
            return null;
        }

        public IResourceBitmap RenderSvg(Stream inputStream, float scaleFactor, int width, int height, int percent, int hash)
        {
            throw new NotImplementedException();
        }

        private static SKColor GetColor(Color color)
        {
            switch (color)
            {
                case Color.Black:
                    return SKColors.Black;
                case Color.Blue:
                    return SKColors.Blue;
                case Color.Green:
                    return SKColors.Green;
                case Color.Red:
                    return SKColors.Red;
                case Color.Transparent:
                    return SKColors.Transparent;
                case Color.White:
                    return SKColors.White;
            }

            throw new System.ArgumentException("unknown color: " + color);
        }
    }
}
