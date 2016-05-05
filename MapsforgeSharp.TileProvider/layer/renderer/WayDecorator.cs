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

namespace org.mapsforge.map.layer.renderer
{
    using System;
    using System.Collections.Generic;

    using IBitmap = MapsforgeSharp.Core.Graphics.IBitmap;
	using Display = MapsforgeSharp.Core.Graphics.Display;
	using MapElementContainer = org.mapsforge.core.mapelements.MapElementContainer;
	using Paint = MapsforgeSharp.Core.Graphics.Paint;
	using SymbolContainer = org.mapsforge.core.mapelements.SymbolContainer;
	using WayTextContainer = org.mapsforge.core.mapelements.WayTextContainer;
	using LineSegment = org.mapsforge.core.model.LineSegment;
	using Point = org.mapsforge.core.model.Point;
	using Rectangle = org.mapsforge.core.model.Rectangle;
	using Tile = org.mapsforge.core.model.Tile;

	internal sealed class WayDecorator
	{
		/// <summary>
		/// A safety margin that keeps way names out of intersections.
		/// </summary>
		private const int WAYNAME_SAFETY_MARGIN = 10;

		internal static void RenderSymbol(IBitmap symbolBitmap, Display display, int priority, float dy, bool alignCenter, bool repeatSymbol, float repeatGap, float repeatStart, bool rotate, Point[][] coordinates, ICollection<MapElementContainer> currentItems)
		{
			int skipPixels = (int)repeatStart;

			Point[] c;
			if (dy == 0f)
			{
				c = coordinates[0];
			}
			else
			{
				c = RendererUtils.ParallelPath(coordinates[0], dy);
			}

			// get the first way point coordinates
			double previousX = c[0].X;
			double previousY = c[0].Y;

			// draw the symbolContainer on each way segment
			float segmentLengthRemaining;
			float segmentSkipPercentage;
			float theta = 0;

			for (int i = 1; i < c.Length; ++i)
			{
				// get the current way point coordinates
				double currentX = c[i].X;
				double currentY = c[i].Y;

				// calculate the length of the current segment (Euclidian distance)
				double diffX = currentX - previousX;
				double diffY = currentY - previousY;
				double segmentLengthInPixel = Math.Sqrt(diffX * diffX + diffY * diffY);
				segmentLengthRemaining = (float) segmentLengthInPixel;

				while (segmentLengthRemaining - skipPixels > repeatStart)
				{
					// calculate the percentage of the current segment to skip
					segmentSkipPercentage = skipPixels / segmentLengthRemaining;

					// move the previous point forward towards the current point
					previousX += diffX * segmentSkipPercentage;
					previousY += diffY * segmentSkipPercentage;
					if (rotate)
					{
						// if we do not rotate theta will be 0, which is correct
						theta = (float) Math.Atan2(currentY - previousY, currentX - previousX);
					}

					Point point = new Point(previousX, previousY);

					currentItems.Add(new SymbolContainer(point, display, priority, symbolBitmap, theta, alignCenter));

					// check if the symbolContainer should only be rendered once
					if (!repeatSymbol)
					{
						return;
					}

					// recalculate the distances
					diffX = currentX - previousX;
					diffY = currentY - previousY;

					// recalculate the remaining length of the current segment
					segmentLengthRemaining -= skipPixels;

					// set the amount of pixels to skip before repeating the symbolContainer
					skipPixels = (int)repeatGap;
				}

				skipPixels -= (int)segmentLengthRemaining;
				if (skipPixels < repeatStart)
				{
					skipPixels = (int)repeatStart;
				}

				// set the previous way point coordinates for the next loop
				previousX = currentX;
				previousY = currentY;
			}
		}

		/// <summary>
		/// Finds the segments of a line along which a name can be drawn and then adds WayTextContainers
		/// to the list of drawable items.
		/// </summary>
		/// <param name="tile"> the tile on which the text will be drawn. </param>
		/// <param name="text"> the text to draw </param>
		/// <param name="priority"> priority of the text </param>
		/// <param name="dy"> if 0, then a line  parallel to the coordinates will be calculated first </param>
		/// <param name="fill"> fill paint for text </param>
		/// <param name="stroke"> stroke paint for text </param>
		/// <param name="coordinates"> the list of way coordinates </param>
		/// <param name="currentLabels"> the list of labels to which a new WayTextContainer will be added </param>
		internal static void RenderText(Tile tile, string text, Display display, int priority, float dy, Paint fill, Paint stroke, Point[][] coordinates, ICollection<MapElementContainer> currentLabels)
		{
			// Calculate the way name length plus some margin of safety
			int wayNameWidth = (stroke == null) ? fill.GetTextWidth(text) + WAYNAME_SAFETY_MARGIN * 2 : stroke.GetTextWidth(text) + WAYNAME_SAFETY_MARGIN * 2;

			// Compute the tile boundary on which we render the name.
			// We make the tile smaller because otherwise we sometimes write the text beyond the tile boundary
			// (e.g. a road that runs parallel just below a tile boundary)
			double textHeight = (stroke == null) ? fill.GetTextHeight(text) : stroke.GetTextHeight(text);
			Rectangle tileBoundary = tile.BoundaryAbsolute.Envelope(-textHeight);

			int skipPixels = 0;

			Point[] c;
			if (dy == 0f)
			{
				c = coordinates[0];
			}
			else
			{
				c = RendererUtils.ParallelPath(coordinates[0], dy);
			}

			// iterate through the segments to find those long enough to draw the way name on them
			for (int i = 1; i < c.Length; ++i)
			{

				LineSegment currentSegment = new LineSegment(c[i - 1], c[i]);
				double currentLength = currentSegment.Length();

				skipPixels -= (int)currentLength;

				if (skipPixels > 0)
				{
					// we should still be skipping pixels, so skip this segment. Note that
					// this does not guarantee that we skip any certain minimum of pixels,
					// it is more a rule of thumb.
					continue;
				}

				if (currentLength < wayNameWidth)
				{
					// no point trying to clip, the segment is too short anyway
					continue;
				}

				// clip the current segment to the tile, so that we never overlap tile boundaries
				// with the way name
				LineSegment drawableSegment = currentSegment.ClipToRectangle(tileBoundary);

				if (drawableSegment == null)
				{
					// this happens if the segment does not intersect the tile
					continue;
				}

				double segmentLengthInPixel = drawableSegment.Length();
				if (segmentLengthInPixel < wayNameWidth)
				{
					// not enough space to draw name on this segment
					continue;
				}

				// now calculate the actually used part of the segment to ensure the bbox of the waytext container
				// is as small as possible. The offset at the beginning/end is to ensure that we are a bit off the center
				// of an intersection (otherwise we have more collisions at the intersection)
				LineSegment actuallyUsedSegment = drawableSegment.SubSegment(WAYNAME_SAFETY_MARGIN, wayNameWidth - WAYNAME_SAFETY_MARGIN);
				// check to prevent inverted way names
				if (actuallyUsedSegment.Start.X <= actuallyUsedSegment.End.X)
				{
					currentLabels.Add(new WayTextContainer(actuallyUsedSegment.Start, actuallyUsedSegment.End, display, priority, text, fill, stroke, textHeight));
				}
				else
				{
					currentLabels.Add(new WayTextContainer(actuallyUsedSegment.End, actuallyUsedSegment.Start, display, priority, text, fill, stroke, textHeight));
				}

				skipPixels = wayNameWidth;
			}
		}

		private WayDecorator()
		{
			throw new System.InvalidOperationException();
		}
	}
}