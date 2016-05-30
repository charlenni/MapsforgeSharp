/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
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

namespace org.mapsforge.map.rendertheme.renderinstruction
{
	using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
	using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
	using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
	using DisplayModel = org.mapsforge.map.model.DisplayModel;
	using PointOfInterest = org.mapsforge.core.datastore.PointOfInterest;

	/// <summary>
	/// A RenderInstruction is a basic graphical primitive to draw a map.
	/// </summary>
	public abstract class RenderInstruction
	{
		internal const string ALIGN_CENTER = "align-center";
		internal const string CAT = "cat";
		internal const string DISPLAY = "display";
		internal const string DY = "dy";
		internal const string FILL = "fill";
		internal const string FONT_FAMILY = "font-family";
		internal const string FONT_SIZE = "font-size";
		internal const string FONT_STYLE = "font-style";
		internal const string ID = "id";
		internal const string K = "k";
		internal const string POSITION = "position";
		internal const string PRIORITY = "priority";
		internal const string R = "r";
		internal const string RADIUS = "radius";
		internal const string REPEAT = "repeat";
		internal const string REPEAT_GAP = "repeat-gap";
		internal const string REPEAT_START = "repeat-start";
		internal const string ROTATE = "rotate";
		internal const string SCALE_RADIUS = "scale-radius";
		internal const string SIZE = "symbol-size";
		internal const string SRC = "src";
		internal const string STROKE = "stroke";
		internal const string STROKE_DASHARRAY = "stroke-dasharray";
		internal const string STROKE_LINECAP = "stroke-linecap";
		internal const string STROKE_LINEJOIN = "stroke-linejoin";
		internal const string STROKE_WIDTH = "stroke-width";
		internal const string SYMBOL_HEIGHT = "symbol-height";
		internal const string SYMBOL_ID = "symbol-id";
		internal const string SYMBOL_PERCENT = "symbol-percent";
		internal const string SYMBOL_SCALING = "symbol-scaling";
		internal const string SYMBOL_WIDTH = "symbol-width";

		public enum ResourceScaling
		{
			DEFAULT,
			SIZE
		}

		protected internal string category;
		public readonly DisplayModel displayModel;
		public readonly IGraphicFactory graphicFactory;

		protected internal float height;
		protected internal int percent = 100;
		protected internal float width;
		internal ResourceScaling scaling;

		protected internal RenderInstruction(IGraphicFactory graphicFactory, DisplayModel displayModel)
		{
			this.displayModel = displayModel;
			this.graphicFactory = graphicFactory;
		}

		public abstract void Destroy();

		public virtual string Category
		{
			get
			{
				return this.category;
			}
		}

		/// <param name="renderCallback">
		///            a reference to the receiver of all render callbacks. </param>
		/// <param name="renderContext"> </param>
		/// <param name="poi"> </param>
		public abstract void RenderNode(RenderCallback renderCallback, RenderContext renderContext, PointOfInterest poi);

		/// <param name="renderCallback">
		///            a reference to the receiver of all render callbacks. </param>
		/// <param name="renderContext"> </param>
		/// <param name="way"> </param>
		public abstract void RenderWay(RenderCallback renderCallback, RenderContext renderContext, PolylineContainer way);

		/// <summary>
		/// Scales the stroke width of this RenderInstruction by the given factor.
		/// </summary>
		/// <param name="scaleFactor">
		///            the factor by which the stroke width should be scaled. </param>
		public abstract void ScaleStrokeWidth(float scaleFactor, sbyte zoomLevel);

		/// <summary>
		/// Scales the text size of this RenderInstruction by the given factor.
		/// </summary>
		/// <param name="scaleFactor">
		///            the factor by which the text size should be scaled. </param>
		public abstract void ScaleTextSize(float scaleFactor, sbyte zoomLevel);

		protected internal virtual IBitmap CreateBitmap(string relativePathPrefix, string src)
		{
			if (null == src || src.Length == 0)
			{
				return null;
			}

			return XmlUtils.CreateBitmap(graphicFactory, displayModel, relativePathPrefix, src, (int) width, (int) height, percent);
		}

		protected internal virtual ResourceScaling FromValue(string value)
		{
			if (value.Equals(SIZE))
			{
				return ResourceScaling.SIZE;
			}

			return ResourceScaling.DEFAULT;
		}
	}
}