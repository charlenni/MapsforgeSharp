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

namespace MapsforgeSharp.TileProvider.Graphics
{
	using System;
	using MapsforgeSharp.Core.Mapelements;
	using org.mapsforge.core.model;
	using System.IO;
	using MapsforgeSharp.Core.Graphics;
	using System.Reflection;
	using SkiaSharp;

	public class SkiaGraphicFactory : GraphicFactory
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

        public Matrix CreateMatrix()
        {
            return new SkiaMatrix();
        }

        public Paint CreatePaint()
        {
            return new SkiaPaint();
        }

        public Paint CreatePaint(Paint paint)
        {
            return new SkiaPaint(paint);
        }

        public MapsforgeSharp.Core.Graphics.Path CreatePath()
        {
            return new SkiaPath();
        }

        public PointTextContainer CreatePointTextContainer(Point xy, Display display, int priority, string text, Paint paintFront, Paint paintBack, SymbolContainer symbolContainer, Position position, int maxTextWidth)
        {
			return new SkiaPointTextContainer(xy, display, priority, text, paintFront, paintBack, symbolContainer, position, maxTextWidth);
        }

        public ResourceBitmap CreateResourceBitmap(Stream inputStream, int hash)
        {
            throw new NotImplementedException();
        }

        public TileBitmap CreateTileBitmap(int tileSize, bool isTransparent)
        {
            return new SkiaTileBitmap(tileSize, isTransparent);
        }

        public TileBitmap CreateTileBitmap(Stream inputStream, int tileSize, bool isTransparent)
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

        public ResourceBitmap RenderSvg(Stream inputStream, float scaleFactor, int width, int height, int percent, int hash)
        {
            throw new NotImplementedException();
        }

        private static SKColor GetColor(Color color)
        {
            switch (color)
            {
                case Color.BLACK:
                    return SKColors.Black;
                case Color.BLUE:
                    return SKColors.Blue;
                case Color.GREEN:
                    return SKColors.Green;
                case Color.RED:
                    return SKColors.Red;
                case Color.TRANSPARENT:
                    return SKColors.Transparent;
                case Color.WHITE:
                    return SKColors.White;
            }

            throw new System.ArgumentException("unknown color: " + color);
        }
    }
}
