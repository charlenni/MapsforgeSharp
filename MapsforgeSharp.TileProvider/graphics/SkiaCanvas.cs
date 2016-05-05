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

namespace MapsforgeSharp.TileProvider.Graphics
{
	using System;
	using SkiaSharp;
	using MapsforgeSharp.Core.Graphics;
	using org.mapsforge.core.model;

	public class SkiaCanvas : Canvas
    {
        private readonly SKSurface nativeSurface;
        private readonly SKCanvas nativeCanvas;
        private readonly SKPaint nativePaint = new SKPaint() { IsAntialias = true, };

        public SkiaCanvas()
        {
            nativeSurface = SKSurface.Create(256, 256, SKColorType.Bgra_8888, SKAlphaType.Premul);
            nativeCanvas = nativeSurface.Canvas;
        }

        public SkiaCanvas(SKCanvas canvas)
        {
            nativeCanvas = canvas;
        }

        public IBitmap Bitmap
        {
            set
            {
				// TODO
				//nativeCanvas.IBitmap = value;
				throw new NotImplementedException();
			}
		}

        public Dimension Dimension
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Height
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Width
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        public void DrawBitmap(IBitmap bitmap, Matrix matrix)
        {
            throw new NotImplementedException();
        }

        public void DrawBitmap(IBitmap bitmap, int left, int top)
        {
            throw new NotImplementedException();
        }

        public void DrawCircle(int x, int y, int radius, Paint paint)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(int x1, int y1, int x2, int y2, Paint paint)
        {
            throw new NotImplementedException();
        }

        public void DrawPath(Path path, Paint paint)
        {
            throw new NotImplementedException();
        }

        public void DrawText(string text, int x, int y, Paint paint)
        {
            throw new NotImplementedException();
        }

        public void DrawTextRotated(string text, int x1, int y1, int x2, int y2, Paint paint)
        {
            throw new NotImplementedException();
        }

        public void FillColor(int color)
        {
            throw new NotImplementedException();
        }

        public void FillColor(Color color)
        {
            throw new NotImplementedException();
        }

        public void ResetClip()
        {
            throw new NotImplementedException();
        }

        public void SetClip(int left, int top, int width, int height)
        {
            throw new NotImplementedException();
        }

        public void SetClipDifference(int left, int top, int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}
