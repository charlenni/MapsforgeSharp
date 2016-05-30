/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

namespace org.mapsforge.map.rendertheme
{
	using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
	using Display = MapsforgeSharp.Core.Graphics.Display;
	using Position = MapsforgeSharp.Core.Graphics.Position;
	using IPaint = MapsforgeSharp.Core.Graphics.IPaint;
	using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
	using PointOfInterest = org.mapsforge.core.datastore.PointOfInterest;

	/// <summary>
	/// Callback methods for rendering areas, ways and points of interest (POIs).
	/// </summary>
	public interface RenderCallback
	{
		/// <summary>
		/// Renders an area with the given parameters.
		/// </summary>
		/// <param name="renderContext"> </param>
		/// <param name="fill">
		///            the paint to be used for rendering the area. </param>
		/// <param name="stroke">
		///            an optional paint for the area casing (may be null). </param>
		/// <param name="level"> </param>
		void RenderArea(RenderContext renderContext, IPaint fill, IPaint stroke, int level, PolylineContainer way);

		/// <summary>
		/// Renders an area caption with the given text. </summary>
		/// <param name="renderContext"> </param>
		/// <param name="display"> display mode </param>
		/// <param name="priority"> priority level </param>
		/// <param name="caption"> the text. </param>
		/// <param name="horizontalOffset"> the horizontal offset of the text. </param>
		/// <param name="verticalOffset"> the vertical offset of the text. </param>
		/// <param name="fill"> the paint for the text. </param>
		/// <param name="stroke"> the casing of the text (may be null). </param>
		/// <param name="position"> optional position (may be null) </param>
		/// <param name="maxTextWidth"> maximum text width . </param>
		/// <param name="way"> the way for the caption. </param>
		 void RenderAreaCaption(RenderContext renderContext, Display display, int priority, string caption, float horizontalOffset, float verticalOffset, IPaint fill, IPaint stroke, Position position, int maxTextWidth, PolylineContainer way);

		/// <summary>
		/// Renders an area symbol with the given bitmap.
		/// </summary>
		/// <param name="renderContext"> </param>
		/// <param name="symbol"> </param>
		void RenderAreaSymbol(RenderContext renderContext, Display display, int priority, IBitmap symbol, PolylineContainer way);

		/// <summary>
		/// Renders a point of interest caption with the given text.
		/// </summary>
		/// <param name="renderContext"> </param>
		/// <param name="caption">
		///            the text to be rendered. </param>
		/// <param name="horizontalOffset">
		///            the horizontal offset of the caption. </param>
		/// <param name="verticalOffset">
		///            the vertical offset of the caption. </param>
		/// <param name="fill">
		///            the paint to be used for rendering the text. </param>
		/// <param name="stroke">
		///            an optional paint for the text casing (may be null). </param>
		/// <param name="position">
		///  </param>
		void RenderPointOfInterestCaption(RenderContext renderContext, Display display, int priority, string caption, float horizontalOffset, float verticalOffset, IPaint fill, IPaint stroke, Position position, int maxTextWidth, PointOfInterest poi);

		/// <summary>
		/// Renders a point of interest circle with the given parameters.
		/// </summary>
		/// <param name="renderContext"> </param>
		/// <param name="radius">
		///            the radius of the circle. </param>
		/// <param name="fill">
		///            the paint to be used for rendering the circle. </param>
		/// <param name="stroke">
		///            an optional paint for the circle casing (may be null). </param>
		/// <param name="level"> </param>
		void RenderPointOfInterestCircle(RenderContext renderContext, float radius, IPaint fill, IPaint stroke, int level, PointOfInterest poi);

		/// <summary>
		/// Renders a point of interest symbol with the given bitmap.
		/// </summary>
		/// <param name="renderContext"> </param>
		/// <param name="symbol"> </param>
		void RenderPointOfInterestSymbol(RenderContext renderContext, Display display, int priority, IBitmap symbol, PointOfInterest poi);

		/// <summary>
		/// Renders a way with the given parameters.
		/// </summary>
		/// <param name="renderContext"> </param>
		/// <param name="stroke">
		///            the paint to be used for rendering the way. </param>
		/// <param name="dy">
		///            the offset of the way. </param>
		/// <param name="level"> </param>
		void RenderWay(RenderContext renderContext, IPaint stroke, float dy, int level, PolylineContainer way);

		/// <summary>
		/// Renders a way with the given symbol along the way path.
		/// </summary>
		/// <param name="renderContext"> </param>
		/// <param name="symbol">
		///            the symbol to be rendered. </param>
		/// <param name="dy">
		///            the offset of the way. </param>
		/// <param name="alignCenter">
		///            true if the symbol should be centered, false otherwise. </param>
		/// <param name="repeat">
		///            true if the symbol should be repeated, false otherwise. </param>
		/// <param name="repeatGap">
		///            distance between repetitions. </param>
		/// <param name="repeatStart"> </param>
		void RenderWaySymbol(RenderContext renderContext, Display display, int priority, IBitmap symbol, float dy, bool alignCenter, bool repeat, float repeatGap, float repeatStart, bool rotate, PolylineContainer way);

		/// <summary>
		/// Renders a way with the given text along the way path.
		/// </summary>
		/// <param name="renderContext"> </param>
		/// <param name="text">
		///            the text to be rendered. </param>
		/// <param name="dy">
		///            the offset of the way text. </param>
		/// <param name="fill">
		///            the paint to be used for rendering the text. </param>
		/// <param name="stroke"> </param>
		void RenderWayText(RenderContext renderContext, Display display, int priority, string text, float dy, IPaint fill, IPaint stroke, PolylineContainer way);
	}
}