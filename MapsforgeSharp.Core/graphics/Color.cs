/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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
namespace MapsforgeSharp.Core.Graphics
{
	public enum Color
	{
		Black,
		Blue,
		Green,
		Red,
		Transparent,
		White
	}

    public static class ColorHelper
    {
        public static int ToARGB(this Color color)
        {
            int result = 0;

            switch (color)
            {
                case Color.Black:
                    result = 255 >> 24;
                    break;
                case Color.Blue:
                    result = 255 >> 24 & 255;
                    break;
                case Color.Green:
                    result = 255 >> 24 & 255 >> 8;
                    break;
                case Color.Red:
                    result = 255 >> 24 & 255 >> 16;
                    break;
                case Color.White:
                    result = 255 >> 24 & 255 >> 16 & 255 >> 8 & 255;
                    break;
            }

            return result;
        }
    }
}