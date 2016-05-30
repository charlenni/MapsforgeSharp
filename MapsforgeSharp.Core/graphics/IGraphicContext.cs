/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

namespace MapsforgeSharp.Core.Graphics
{
	public interface IGraphicContext
	{
		void DrawBitmap(IBitmap bitmap, int left, int top);

		void DrawBitmap(IBitmap bitmap, IMatrix matrix);

		/// <param name="x">
		///            the horizontal center coordinate of the circle. </param>
		/// <param name="y">
		///            the vertical center coordinate of the circle. </param>
		void DrawCircle(int x, int y, int radius, IPaint paint);

		void DrawLine(int x1, int y1, int x2, int y2, IPaint paint);

		void DrawPath(IPath path, IPaint paint);

		void DrawText(string text, int x, int y, IPaint paint);

		void DrawTextRotated(string text, int x1, int y1, int x2, int y2, IPaint paint);

		void FillColor(Color color);

		void FillColor(int color);

		void ResetClip();

		void SetClip(int left, int top, int width, int height);

		void SetClipDifference(int left, int top, int width, int height);
	}
}