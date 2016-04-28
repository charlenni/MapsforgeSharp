/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.core.graphics
{
	using PointTextContainer = org.mapsforge.core.mapelements.PointTextContainer;
	using SymbolContainer = org.mapsforge.core.mapelements.SymbolContainer;
	using Point = org.mapsforge.core.model.Point;

	public interface GraphicFactory
	{
		Bitmap CreateBitmap(int width, int height);

		Bitmap CreateBitmap(int width, int height, bool isTransparent);

		Canvas CreateCanvas();

		int CreateColor(Color color);

		int CreateColor(int alpha, int red, int green, int blue);

		Matrix CreateMatrix();

		Paint CreatePaint();

		Paint CreatePaint(Paint paint);

		Path CreatePath();

		PointTextContainer CreatePointTextContainer(Point xy, Display display, int priority, string text, Paint paintFront, Paint paintBack, SymbolContainer symbolContainer, Position position, int maxTextWidth);

		ResourceBitmap CreateResourceBitmap(System.IO.Stream inputStream, int hash);

		TileBitmap CreateTileBitmap(System.IO.Stream inputStream, int tileSize, bool isTransparent);

		TileBitmap CreateTileBitmap(int tileSize, bool isTransparent);

		System.IO.Stream PlatformSpecificSources(string relativePathPrefix, string src);

		ResourceBitmap RenderSvg(System.IO.Stream inputStream, float scaleFactor, int width, int height, int percent, int hash);
	}
}