/*
 * Copyright 2014 Ludwig M Brinckmann
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

using System;

namespace MapsforgeSharp.Core.Graphics
{
    /// <summary>
    /// The enum Display governs whether map elements should be displayed.
    /// 
    /// The main choice is
    /// between Ifspace which means an element is displayed if there is space for it (also depends on
    /// priority), while Always means that an element will always be displayed (so it will be overlapped by
    /// others and will not be part of the element placing algorithm). Never is a convenience fallback, which
    /// means that an element will never be displayed.
    /// </summary>
    public enum Display
    {
        Never,
        Always,
        Ifspace
    }

    public static class DisplayHelper
    {
        public static Display ToDisplay(this string value)
        {
            Display ret;
            if (!Enum.TryParse<Display>(value, true, out ret))
            {
                throw new ArgumentException("Invalid value for Display: " + value);
            }
            return ret;
        }
    }
}