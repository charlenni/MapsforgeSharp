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

namespace org.mapsforge.map.util
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// An abstract base class for threads which support pausing and resuming.
    /// </summary>
    public abstract class PausableThread : System.Threading.Thread
	{
		/// <summary>
		/// Specifies the scheduling priority of a <seealso cref="Thread"/>.
		/// </summary>
		protected internal sealed class ThreadPriority
		{
			/// <summary>
			/// The priority between <seealso cref="#NORMAL"/> and <seealso cref="#HIGHEST"/>.
			/// </summary>
			public static readonly ThreadPriority ABOVE_NORMAL = new ThreadPriority("ABOVE_NORMAL", InnerEnum.ABOVE_NORMAL, (Thread.NORM_PRIORITY + Thread.MAX_PRIORITY) / 2);

			/// <summary>
			/// The priority between <seealso cref="#LOWEST"/> and <seealso cref="#NORMAL"/>.
			/// </summary>
			public static readonly ThreadPriority BELOW_NORMAL = new ThreadPriority("BELOW_NORMAL", InnerEnum.BELOW_NORMAL, (Thread.NORM_PRIORITY + Thread.MIN_PRIORITY) / 2);

			/// <summary>
			/// The maximum priority a thread can have.
			/// </summary>
			public static readonly ThreadPriority HIGHEST = new ThreadPriority("HIGHEST", InnerEnum.HIGHEST, MAX_PRIORITY);

			/// <summary>
			/// The minimum priority a thread can have.
			/// </summary>
			public static readonly ThreadPriority LOWEST = new ThreadPriority("LOWEST", InnerEnum.LOWEST, MIN_PRIORITY);

			/// <summary>
			/// The default priority of a thread.
			/// </summary>
			public static readonly ThreadPriority NORMAL = new ThreadPriority("NORMAL", InnerEnum.NORMAL, NORM_PRIORITY);

			private static readonly IList<ThreadPriority> valueList = new List<ThreadPriority>();

			static ThreadPriority()
			{
				valueList.Add(ABOVE_NORMAL);
				valueList.Add(BELOW_NORMAL);
				valueList.Add(HIGHEST);
				valueList.Add(LOWEST);
				valueList.Add(NORMAL);
			}

			public enum InnerEnum
			{
				ABOVE_NORMAL,
				BELOW_NORMAL,
				HIGHEST,
				LOWEST,
				NORMAL
			}

			private readonly string nameValue;
			private readonly int ordinalValue;
			private readonly InnerEnum innerEnumValue;
			private static int nextOrdinal = 0;

			internal readonly int priority;

			internal ThreadPriority(string name, InnerEnum innerEnum, int priority)
			{
				if (priority < Thread.MIN_PRIORITY || priority > Thread.MAX_PRIORITY)
				{
					throw new System.ArgumentException("invalid priority: " + priority);
				}
				this.priority = priority;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			public static IList<ThreadPriority> Values()
			{
				return valueList;
			}

			public InnerEnum InnerEnumValue()
			{
				return innerEnumValue;
			}

			public int Ordinal()
			{
				return ordinalValue;
			}

			public override string ToString()
			{
				return nameValue;
			}

			public static ThreadPriority ValueOf(string name)
			{
				foreach (ThreadPriority enumInstance in ThreadPriority.Values())
				{
					if (enumInstance.nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new System.ArgumentException(name);
			}
		}

		private bool pausing;
		private bool shouldPause;

		/// <summary>
		/// Causes the current thread to wait until this thread is pausing.
		/// </summary>
		public void AwaitPausing()
		{
			lock (this)
			{
				while (!Interrupted && !Pausing)
				{
					try
					{
						Monitor.Wait(this, TimeSpan.FromMilliseconds(100));
					}
					catch (InterruptedException)
					{
						// restore the interrupted status
						Thread.CurrentThread.Interrupt();
					}
				}
			}
		}

		public override void Interrupt()
		{
			// first acquire the monitor which is used to call wait()
			lock (this)
			{
				base.Interrupt();
			}
		}

		/// <returns> true if this thread is currently pausing, false otherwise. </returns>
		public bool Pausing
		{
			get
			{
				lock (this)
				{
					return this.pausing;
				}
			}
		}

		/// <summary>
		/// The thread should stop its work temporarily.
		/// </summary>
		public void Pause()
		{
			lock (this)
			{
				if (!this.shouldPause)
				{
					this.shouldPause = true;
					Monitor.Pulse(this);
				}
			}
		}

		/// <summary>
		/// The paused thread should continue with its work.
		/// </summary>
		public void Proceed()
		{
			lock (this)
			{
				if (this.shouldPause)
				{
					this.shouldPause = false;
					this.pausing = false;
					Monitor.Pulse(this);
				}
			}
		}

		public override void Run()
		{
            Name = this.GetType().Name;
			Priority = GetThreadPriority().priority;

			while (!Interrupted)
			{
				lock (this)
				{
					while (!Interrupted && (this.shouldPause || !HasWork()))
					{
						try
						{
							if (this.shouldPause)
							{
								this.pausing = true;
							}
							Monitor.Wait(this);
						}
						catch (InterruptedException)
						{
							// restore the interrupted status
							Interrupt();
						}
					}
				}

				if (Interrupted)
				{
					break;
				}

				try
				{
					DoWork();
				}
				catch (InterruptedException)
				{
					// restore the interrupted status
					Interrupt();
				}
			}

			AfterRun();
		}

		/// <summary>
		/// Called once at the end of the <seealso cref="#run()"/> method. The default implementation is empty.
		/// </summary>
		protected internal virtual void AfterRun()
		{
			// do nothing
		}

		/// <summary>
		/// Called when this thread is not paused and should do its work.
		/// </summary>
		/// <exception cref="InterruptedException">
		///             if the thread has been interrupted. </exception>
		protected internal abstract void DoWork();

		/// <returns> the priority which will be set for this thread. </returns>
		protected internal abstract ThreadPriority GetThreadPriority();

		/// <returns> true if this thread has some work to do, false otherwise. </returns>
		protected internal abstract bool HasWork();
	}

}