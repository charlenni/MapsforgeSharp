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

namespace org.mapsforge.map.layer.queue
{
	internal class QueueItem<T> where T : Job
	{
		internal readonly T @object;
		private double priority;

		internal QueueItem(T @object)
		{
			this.@object = @object;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is QueueItem<T>))
			{
				return false;
			}
			QueueItem<T> other = (QueueItem<T>) obj;
			return this.@object.Equals(other.@object);
		}

		public override int GetHashCode()
		{
			return this.@object.GetHashCode();
		}

		/// <returns> the current priority of this job, will always be a positive number including zero. </returns>
		internal virtual double Priority
		{
			get
			{
				return this.priority;
			}
			set
			{
				if (value < 0 || double.IsNaN(value))
				{
					throw new System.ArgumentException("invalid priority: " + value);
				}
				this.priority = value;
			}
		}
	}
}