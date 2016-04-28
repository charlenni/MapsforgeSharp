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

namespace org.mapsforge.map.model.common
{
    using System.Collections.Generic;

    public class Observable : ObservableInterface
	{
		private const string OBSERVER_MUST_NOT_BE_NULL = "observer must not be null";
		private readonly IList<Observer> observers = new List<Observer>();

		public void AddObserver(Observer observer)
		{
			if (observer == null)
			{
				throw new System.ArgumentException(OBSERVER_MUST_NOT_BE_NULL);
			}
			else if (this.observers.Contains(observer))
			{
				throw new System.ArgumentException("observer is already registered: " + observer);
			}
			this.observers.Add(observer);
		}

		public void RemoveObserver(Observer observer)
		{
			if (observer == null)
			{
				throw new System.ArgumentException(OBSERVER_MUST_NOT_BE_NULL);
			}
			else if (!this.observers.Contains(observer))
			{
				throw new System.ArgumentException("observer is not registered: " + observer);
			}
			this.observers.Remove(observer);
		}

		public void NotifyObservers()
		{
			foreach (Observer observer in this.observers)
			{
				observer.OnChange();
			}
		}
	}
}