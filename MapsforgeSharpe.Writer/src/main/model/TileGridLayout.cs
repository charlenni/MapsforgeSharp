/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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
namespace org.mapsforge.map.writer.model
{

	public class TileGridLayout
	{
		private readonly int amountTilesHorizontal;
		private readonly int amountTilesVertical;
		private readonly TileCoordinate upperLeft;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="upperLeft">
		///            the upper left tile coordinate </param>
		/// <param name="amountTilesHorizontal">
		///            the amount of columns </param>
		/// <param name="amountTilesVertical">
		///            the amount of rows </param>
		public TileGridLayout(TileCoordinate upperLeft, int amountTilesHorizontal, int amountTilesVertical) : base()
		{
			this.upperLeft = upperLeft;
			this.amountTilesHorizontal = amountTilesHorizontal;
			this.amountTilesVertical = amountTilesVertical;
		}

		/// <returns> the amountTilesHorizontal </returns>
		public virtual int AmountTilesHorizontal
		{
			get
			{
				return this.amountTilesHorizontal;
			}
		}

		/// <returns> the amountTilesVertical </returns>
		public virtual int AmountTilesVertical
		{
			get
			{
				return this.amountTilesVertical;
			}
		}

		/// <returns> the upperLeft </returns>
		public virtual TileCoordinate UpperLeft
		{
			get
			{
				return this.upperLeft;
			}
		}
	}

}