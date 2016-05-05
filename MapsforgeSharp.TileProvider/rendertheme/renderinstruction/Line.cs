/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
 * Copyright 2014 devemux86
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
namespace org.mapsforge.map.rendertheme.renderinstruction
{
    using System.Collections.Generic;
    using System.Xml;
    using System.IO;
    using System;
    using MapsforgeSharp.Core.Graphics;

    using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
    using Cap = MapsforgeSharp.Core.Graphics.Cap;
    using Color = MapsforgeSharp.Core.Graphics.Color;
    using GraphicFactory = MapsforgeSharp.Core.Graphics.GraphicFactory;
    using Join = MapsforgeSharp.Core.Graphics.Join;
    using Paint = MapsforgeSharp.Core.Graphics.Paint;
    using Style = MapsforgeSharp.Core.Graphics.Style;
    using PolylineContainer = org.mapsforge.map.layer.renderer.PolylineContainer;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using PointOfInterest = MapsforgeSharp.Core.Datastore.PointOfInterest;

    /// <summary>
    /// Represents a polyline on the map.
    /// </summary>
    public class Line : RenderInstruction
	{

		private static readonly string[] SPLIT_PATTERN = new string[] { "," };

		private bool bitmapCreated;
		private float dy;
		private readonly IDictionary<sbyte?, float?> dyScaled;
		private readonly int level;
		private readonly string relativePathPrefix;
		private IBitmap shaderBitmap;
		private string src;
		private readonly Paint stroke;
		private readonly IDictionary<sbyte?, Paint> strokes;
		private float strokeWidth;

		public Line(GraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader, int level, string relativePathPrefix) : base(graphicFactory, displayModel)
		{
			this.level = level;
			this.relativePathPrefix = relativePathPrefix;

			this.stroke = graphicFactory.CreatePaint();
			this.stroke.Color = Color.BLACK.ToARGB();
			this.stroke.Style = Style.STROKE;
			this.stroke.StrokeCap = Cap.ROUND;
			this.stroke.StrokeJoin = Join.ROUND;
			this.strokes = new Dictionary<sbyte?, Paint>();
			this.dyScaled = new Dictionary<sbyte?, float?>();

			ExtractValues(graphicFactory, displayModel, elementName, reader, relativePathPrefix);
		}

		public override void Destroy()
		{
			// no.op
		}

		public override void RenderNode(RenderCallback renderCallback, RenderContext renderContext, PointOfInterest poi)
		{
			// do nothing
		}

		public override void RenderWay(RenderCallback renderCallback, RenderContext renderContext, PolylineContainer way)
		{
			lock (this)
			{
        
				if (!bitmapCreated)
				{
					try
					{
						shaderBitmap = CreateBitmap(relativePathPrefix, src);
					}
					catch (IOException)
					{
						// no-op
					}
					bitmapCreated = true;
				}
        
				Paint strokePaint = getStrokePaint(renderContext.rendererJob.tile.ZoomLevel);
        
				if (shaderBitmap != null)
				{
					strokePaint.BitmapShader = shaderBitmap;
					strokePaint.BitmapShaderShift = way.Tile.Origin;
				}
        
				float dyScale = this.dyScaled[renderContext.rendererJob.tile.ZoomLevel] ?? this.dy;

                renderCallback.RenderWay(renderContext, strokePaint, dyScale, this.level, way);
			}
		}

		public override void ScaleStrokeWidth(float scaleFactor, sbyte zoomLevel)
		{
			if (this.stroke != null)
			{
				Paint s = graphicFactory.CreatePaint(stroke);
				s.StrokeWidth = this.strokeWidth * scaleFactor;
				strokes[zoomLevel] = s;
			}

			this.dyScaled[zoomLevel] = this.dy * scaleFactor;
		}

		public override void ScaleTextSize(float scaleFactor, sbyte zoomLevel)
		{
			// do nothing
		}

        private void ExtractValues(GraphicFactory graphicFactory, DisplayModel displayModel, string elementName, XmlReader reader, string relativePathPrefix)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
                reader.MoveToAttribute(i);

				string name = reader.Name;
				string value = reader.Value;

				if (SRC.Equals(name))
				{
					this.src = value;
				}
				else if (CAT.Equals(name))
				{
					this.category = value;
				}
				else if (DY.Equals(name))
				{
					this.dy = float.Parse(value) * displayModel.ScaleFactor;
				}
				else if (STROKE.Equals(name))
				{
					this.stroke.Color = XmlUtils.GetColor(value);
				}
				else if (STROKE_WIDTH.Equals(name))
				{
					this.strokeWidth = XmlUtils.ParseNonNegativeFloat(name, value) * displayModel.ScaleFactor;
				}
				else if (STROKE_DASHARRAY.Equals(name))
				{
					float[] floatArray = ParseFloatArray(name, value);
					for (int f = 0; f < floatArray.Length; ++f)
					{
						floatArray[f] = floatArray[f] * displayModel.ScaleFactor;
					}
					this.stroke.DashPathEffect = floatArray;
				}
				else if (STROKE_LINECAP.Equals(name))
				{
					this.stroke.StrokeCap = Cap.FromString(value);
				}
				else if (STROKE_LINEJOIN.Equals(name))
				{
					this.stroke.StrokeJoin = Join.FromString(value);
				}
				else if (SYMBOL_HEIGHT.Equals(name))
				{
					this.height = XmlUtils.ParseNonNegativeInteger(name, value) * displayModel.ScaleFactor;
				}
				else if (SYMBOL_PERCENT.Equals(name))
				{
					this.percent = XmlUtils.ParseNonNegativeInteger(name, value);
				}
				else if (SYMBOL_SCALING.Equals(name))
				{
					this.scaling = FromValue(value);
				}
				else if (SYMBOL_WIDTH.Equals(name))
				{
					this.width = XmlUtils.ParseNonNegativeInteger(name, value) * displayModel.ScaleFactor;
				}
				else
				{
					throw XmlUtils.CreateXmlReaderException(elementName, name, value, i);
				}
			}
		}

		private static float[] ParseFloatArray(string name, string dashString)
		{
			string[] dashEntries = dashString.Split(SPLIT_PATTERN, StringSplitOptions.RemoveEmptyEntries);
			float[] dashIntervals = new float[dashEntries.Length];
			for (int i = 0; i < dashEntries.Length; ++i)
			{
				dashIntervals[i] = XmlUtils.ParseNonNegativeFloat(name, dashEntries[i]);
			}
			return dashIntervals;
		}

		private Paint getStrokePaint(sbyte zoomLevel)
		{
			return strokes[zoomLevel] ?? this.stroke;
		}
	}
}