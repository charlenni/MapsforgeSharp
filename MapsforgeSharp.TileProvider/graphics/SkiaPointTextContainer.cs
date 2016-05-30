/*
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2014, 2015 devemux86
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

namespace MapsforgeSharp.TileProvider.Graphics
{
	using MapsforgeSharp.Core.Graphics;

	using Canvas = MapsforgeSharp.Core.Graphics.Canvas;
	using Display = MapsforgeSharp.Core.Graphics.Display;
	using IMatrix = MapsforgeSharp.Core.Graphics.IMatrix;
	using IPaint = MapsforgeSharp.Core.Graphics.IPaint;
	using PointTextContainer = MapsforgeSharp.Core.Mapelements.PointTextContainer;
	using SymbolContainer = MapsforgeSharp.Core.Mapelements.SymbolContainer;
	using Point = MapsforgeSharp.Core.Model.Point;
	using Rectangle = MapsforgeSharp.Core.Model.Rectangle;

	public class SkiaPointTextContainer : PointTextContainer
	{
		internal SkiaPointTextContainer(Point xy, Display display, int priority, string text, IPaint paintFront, IPaint paintBack, SymbolContainer symbolContainer, Position position, int maxTextWidth) : base(xy, display, priority, text, paintFront, paintBack, symbolContainer, position, maxTextWidth)
		{
			float boxWidth, boxHeight;

			// TODO: Multiline TextBox
			//if (this.textWidth > this.maxTextWidth)
			//{
			//	// if the text is too wide its layout is done by the Android StaticLayout class,
			//	// which automatically inserts line breaks. There is not a whole lot of useful
			//	// documentation of this class.
			//	// For below and above placements the text is center-aligned, for left on the right
			//	// and for right on the left.
			//	// One disadvantage is that it will always keep the text within the maxWidth,
			//	// even if that means breaking text mid-word.
			//	// This code currently does not play that well with the LabelPlacement algorithm.
			//	// The best way to disable it is to make the maxWidth really wide.

			//	TextPaint frontTextPaint = new TextPaint(SkiaGraphicFactory.GetPaint(this.paintFront));
			//	TextPaint backTextPaint = null;

			//	if (this.paintBack != null)
			//	{
			//		backTextPaint = new TextPaint(SkiaGraphicFactory.GetPaint(this.paintBack));
			//	}

			//	Layout.Alignment alignment = Layout.Alignment.ALIGN_CENTER;
			//	if (Position.LEFT == this.position || Position.BELOW_LEFT == this.position || Position.ABOVE_LEFT == this.position)
			//	{
			//		alignment = Layout.Alignment.ALIGN_OPPOSITE;
			//	}
			//	else if (Position.RIGHT == this.position || Position.BELOW_RIGHT == this.position || Position.ABOVE_RIGHT == this.position)
			//	{
			//		alignment = Layout.Alignment.ALIGN_NORMAL;
			//	}

			//	// strange Android behaviour: if alignment is set to center, then
			//	// text is rendered with right alignment if using StaticLayout
			//	frontTextPaint.TextAlign = graphics.Paint.Align.LEFT;
			//	if (this.paintBack != null)
			//	{
			//		backTextPaint.TextAlign = android.graphics.Paint.Align.LEFT;
			//	}

			//	frontLayout = new StaticLayout(this.text, frontTextPaint, this.maxTextWidth, alignment, 1, 0, false);
			//	backLayout = null;
			//	if (this.paintBack != null)
			//	{
			//		backLayout = new StaticLayout(this.text, backTextPaint, this.maxTextWidth, alignment, 1, 0, false);
			//	}

			//	boxWidth = frontLayout.Width;
			//	boxHeight = frontLayout.Height;

			//}
			//else
			{
				boxWidth = textWidth;
				boxHeight = textHeight;
			}

			switch (this.position)
			{
				case Position.Center:
					boundary = new Rectangle(-boxWidth / 2f, -boxHeight / 2f, boxWidth / 2f, boxHeight / 2f);
					break;
				case Position.Below:
					boundary = new Rectangle(-boxWidth / 2f, 0, boxWidth / 2f, boxHeight);
					break;
				case Position.BelowLeft:
					boundary = new Rectangle(-boxWidth, 0, 0, boxHeight);
					break;
				case Position.BelowRight:
					boundary = new Rectangle(0, 0, boxWidth, boxHeight);
					break;
				case Position.Above:
					boundary = new Rectangle(-boxWidth / 2f, -boxHeight, boxWidth / 2f, 0);
					break;
				case Position.AboveLeft:
					boundary = new Rectangle(-boxWidth, -boxHeight, 0, 0);
					break;
				case Position.AboveRight:
					boundary = new Rectangle(0, -boxHeight, boxWidth, 0);
					break;
				case Position.Left:
					boundary = new Rectangle(-boxWidth, -boxHeight / 2f, 0, boxHeight / 2f);
					break;
				case Position.Right:
					boundary = new Rectangle(0, -boxHeight / 2f, boxWidth, boxHeight / 2f);
					break;
				default:
					break;
			}
		}

		public override void Draw(Canvas canvas, Point origin, IMatrix matrix)
		{
			if (!this.isVisible)
			{
				return;
			}

			// TODO: Multiline TextBox
			//if (this.textWidth > this.maxTextWidth)
			//{
			//	// in this case we draw the precomputed staticLayout onto the canvas by translating
			//	// the canvas.
			//	nativeCanvas.save();
			//	nativeCanvas.translate((float)(this.xy.x - origin.x + boundary.left), (float)(this.xy.y - origin.y + boundary.top));

			//	if (this.backLayout != null)
			//	{
			//		this.backLayout.draw(nativeCanvas);
			//	}
			//	this.frontLayout.draw(nativeCanvas);
			//	nativeCanvas.restore();
			//}
			//else
			{
				// the origin of the text is the base line, so we need to make adjustments
				// so that the text will be within its box
				float textOffset = 0;
				switch (this.position)
				{
					case Position.Center:
					case Position.Left:
					case Position.Right:
						textOffset = textHeight / 2f;
						break;
					case Position.Below:
					case Position.BelowLeft:
					case Position.BelowRight:
						textOffset = textHeight;
						break;
					default:
						break;
				}

				int adjustedX = (int)(this.xy.X - origin.X);
				int adjustedY = (int)((this.xy.Y - origin.Y) + textOffset);

				if (this.paintBack != null)
				{
					((SkiaCanvas)canvas).DrawText(this.text, adjustedX, adjustedY, this.paintBack);
				}
				((SkiaCanvas)canvas).DrawText(this.text, adjustedX, adjustedY, this.paintFront);
			}
		}
	}
}