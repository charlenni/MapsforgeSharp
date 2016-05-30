/*
 * Copyright 2015 Ludwig M Brinckmann
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

namespace MapsforgeSharp.Graphics
{
    using System;
    using System.IO;
	using SkiaSharp;
	using MapsforgeSharp.Core.Graphics;

	public class SkiaTileBitmap : ITileBitmap
    {
        public SKBitmap nativeBitmap;

        public SkiaTileBitmap()
        {
            nativeBitmap = new SKBitmap();
        }

        public SkiaTileBitmap(int width, int height)
        {
            nativeBitmap = new SKBitmap(width, height);
        }

        public SkiaTileBitmap(int tileSize, bool isTransparent)
        {
            nativeBitmap = new SKBitmap(tileSize, tileSize, isTransparent);
        }

        public int BackgroundColor
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public long Expiration
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Expired
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

		private long timestamp;

        public long Timestamp
        {
            get
            {
				return timestamp;
            }

            set
            {
				timestamp = value;
            }
        }

        public int Width
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Compress(Stream outputStream)
        {
            throw new NotImplementedException();
        }

        public void DecrementRefCount()
        {
            throw new NotImplementedException();
        }

        public void IncrementRefCount()
        {
            throw new NotImplementedException();
        }

        public void ScaleTo(int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}