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
	using MapsforgeSharp.Core.Mapelements;
	using MapsforgeSharp.Core.Model;

	public interface IGraphicFactory
	{
		IBitmap CreateBitmap(int width, int height);

		IBitmap CreateBitmap(int width, int height, bool isTransparent);

		ICanvas CreateCanvas(int width, int height);

		int CreateColor(int color);

		int CreateColor(int alpha, int red, int green, int blue);

		IMatrix CreateMatrix();

		IPaint CreatePaint();

		IPaint CreatePaint(IPaint paint);

		IPath CreatePath();

		PointTextContainer CreatePointTextContainer(Point xy, Display display, int priority, string text, IPaint paintFront, IPaint paintBack, SymbolContainer symbolContainer, Position position, int maxTextWidth);

		IResourceBitmap CreateResourceBitmap(System.IO.Stream inputStream, int hash);

		ITileBitmap CreateTileBitmap(System.IO.Stream inputStream, int tileSize, bool isTransparent);

		ITileBitmap CreateTileBitmap(int tileSize, bool isTransparent);

		System.IO.Stream PlatformSpecificSources(string relativePathPrefix, string src);

		IResourceBitmap RenderSvg(System.IO.Stream inputStream, float scaleFactor, int width, int height, int percent, int hash);
	}
}