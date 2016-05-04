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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.IO;
    using SkiaSharp;
    using org.mapsforge.core.graphics;

    public class SkiaBitmap : Bitmap
    {
        SKBitmap nativeBitmap;

        public SkiaBitmap(int width, int height)
        {
            nativeBitmap = new SKBitmap(width, height);
        }

		public SKBitmap NativeBitmap
		{
			get { return nativeBitmap;  }
		}

        public int BackgroundColor
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Height
        {
            get
            {
                return nativeBitmap.Height;
            }
        }

        public int Width
        {
            get
            {
                return nativeBitmap.Width;
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
