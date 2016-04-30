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

namespace org.mapsforge.map.layer.queue
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
	internal sealed class QueueItemComparator : IComparer<QueueItem<JavaToDotNetGenericWildcard>>
	{
		internal static readonly QueueItemComparator INSTANCE = new QueueItemComparator();
		private const long serialVersionUID = 1L;

		private QueueItemComparator()
		{
			// do nothing
		}

		public int compare<T1, T2>(QueueItem<T1> queueItem1, QueueItem<T2> queueItem2)
		{
			if (queueItem1.Priority < queueItem2.Priority)
			{
				return -1;
			}
			else if (queueItem1.Priority > queueItem2.Priority)
			{
				return 1;
			}
			return 0;
		}
	}
}