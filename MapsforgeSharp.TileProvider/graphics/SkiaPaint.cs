﻿/*
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
	using org.mapsforge.core.graphics;
	using System;
	using SkiaSharp;
	using MapsforgeSharp.Core.Graphics;
	using org.mapsforge.core.model;

	public class SkiaPaint : Paint
    {
		private int shaderWidth;
		private int shaderHeight;

        public SkiaPaint()
        {
            nativePaint = new SKPaint();
        }

        public SkiaPaint(Paint paint)
        {
            // TODO
            nativePaint = new SKPaint(); // new SKPaint(((SkiaPaint)paint).NativePaint);
        }

        private readonly SKPaint nativePaint;

        public SKPaint NativePaint
        {
            get { return nativePaint; }
        }

        public Bitmap BitmapShader
        {
            set
            {
				if (value == null)
				{
					return;
				}

				var nativeBitmap = ((SkiaBitmap)value).NativeBitmap;

				if (nativeBitmap == null)
				{
					return;
				}

				this.shaderWidth = value.Width;
				this.shaderHeight = value.Height;

				nativePaint.Color = SKColors.White;
				nativePaint.Shader = SKShader.CreateBitmap(nativeBitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            }
        }

		public Point BitmapShaderShift
		{
			set
			{
				SKShader shader = nativePaint.Shader;
				if (shader != null)
				{
					int relativeDx = ((int)-value.X) % this.shaderWidth;
					int relativeDy = ((int)-value.Y) % this.shaderHeight;

					var localMatrix = new SKMatrix();
					localMatrix.TransX = relativeDx;
					localMatrix.TransY = relativeDy;
					nativePaint.Shader = SKShader.CreateLocalMatrix(shader, localMatrix);
				}
			}
		}

		public int Color
        {
            get
            {
                var color = nativePaint.Color;
                return color.Alpha << 24 | color.Red << 16 | color.Green << 8 | color.Blue;
            }

            set
            {
                nativePaint.Color = new SKColor((byte)(value >> 16 & 0xff), (byte)(value >> 8 & 0xff), (byte)(value & 0xff), (byte)(value >> 24 & 0xff));
            }
        }

        public float[] DashPathEffect
        {
            set
            {
                // TODO: Set dash effect if it is availible in SkiaSharp
            }
        }

        public Cap StrokeCap
        {
            set
            {
                if (value == Cap.BUTT)
                {
                    nativePaint.StrokeCap = SKStrokeCap.Butt;
                }
                if (value == Cap.ROUND)
                {
                    nativePaint.StrokeCap = SKStrokeCap.Round;
                }
                if (value == Cap.SQUARE)
                {
                    nativePaint.StrokeCap = SKStrokeCap.Square;
                }
            }
        }

        public Join StrokeJoin
        {
            set
            {
                if (value == Join.BEVEL)
                {
                    nativePaint.StrokeJoin = SKStrokeJoin.Bevel;
                }
                if (value == Join.MITER)
                {
                    nativePaint.StrokeJoin = SKStrokeJoin.Mitter;
                }
                if (value == Join.ROUND)
                {
                    nativePaint.StrokeJoin = SKStrokeJoin.Round;
                }
            }
        }

        public float StrokeWidth
        {
            set
            {
                nativePaint.StrokeWidth = value;
            }
        }

        public Style Style
        {
            set
            {
                switch (value)
                {
                    case Style.FILL:
                        nativePaint.IsStroke = false;
                        break;
                    case Style.STROKE:
                        nativePaint.IsStroke = true;
                        break;
                }
            }
        }

        public Align TextAlign
        {
            set
            {
                nativePaint.TextAlign = value.ToSkia();
            }
        }

        public float TextSize
        {
            set
            {
                nativePaint.TextSize = value;
            }
        }

        public bool Transparent
        {
            get
            {
				return nativePaint.Shader == null && nativePaint.Color.Alpha == 0;
            }
        }

        public int GetTextHeight(string text)
        {
			SKRect bounds = new SKRect();

			nativePaint.MeasureText(text, ref bounds);

			return (int)Math.Ceiling(bounds.Bottom - bounds.Top);
        }

        public int GetTextWidth(string text)
        {
			SKRect bounds = new SKRect();

			nativePaint.MeasureText(text, ref bounds);

			return (int)Math.Ceiling(bounds.Right - bounds.Left);
        }

        public void SetTypeface(FontFamily fontFamily, FontStyle fontStyle)
        {
            nativePaint.Typeface = SKTypeface.FromFamilyName(fontFamily.ToString(), fontStyle.ToSkia());
        }
	}
}