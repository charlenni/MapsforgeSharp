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

namespace MapsforgeSharp.Graphics
{
	using System;
	using SkiaSharp;
	using MapsforgeSharp.Core.Graphics;

	public class SkiaPath : IPath
    {
        private SKPath nativePath;

        public SkiaPath()
        {
            nativePath = new SKPath();
        }

		public SKPath NativePath
		{
			get { return nativePath; }
		}

        public bool Empty
        {
            get
            {
                // TODO
                //nativePath.IsEmpty;
                throw new NotImplementedException();
            }
        }

        public FillRule FillRule
        {
            set
            {
				//switch (value)
				//{
				//    case FillRule.EvenOdd:
				//nativePath.FillType = SKFillType.EvenOdd;
				//        break;
				//    case FillRule.NonZero:
				//        nativePath.FillType = SKFillType.Winding;
				//        break;
				//}
				throw new NotImplementedException();
            }
        }

        public void Clear()
        {
            // TODO
            //nativePath.Rewind();
            //throw new NotImplementedException();
        }

        public void Close()
        {
            nativePath.Close();
        }

        public void LineTo(float x, float y)
        {
            nativePath.LineTo(x, y);
        }

        public void MoveTo(float x, float y)
        {
            nativePath.MoveTo(x, y);
        }
    }
}