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

namespace org.mapsforge.core.util
{
    using NUnit.Framework;

    public class LRUCacheTest
	{
		private const string KEY1 = "foo1";
		private const string KEY2 = "foo2";
		private const string KEY3 = "foo3";
		private const string VALUE1 = "bar1";
		private const string VALUE2 = "bar2";
		private const string VALUE3 = "bar3";

		private static LRUCache<string, string> CreateLRUCache(int capacity)
		{
			LRUCache<string, string> lruCache = new LRUCache<string, string>(capacity);
			Assert.AreEqual(capacity, lruCache.Capacity);

			return lruCache;
		}

		private static void VerifyInvalidCapacity(int capacity)
		{
			try
			{
				CreateLRUCache(capacity);
				Assert.Fail("capacity: " + capacity);
			}
			catch (System.ArgumentException)
			{
				Assert.True(true);
			}
		}

        [Test()]
		public virtual void LruCacheTest()
		{
			LRUCache<string, string> lruCache = CreateLRUCache(2);

			lruCache.Add(KEY1, VALUE1);
			Assert.AreEqual(VALUE1, lruCache.Get(KEY1));
			Assert.False(lruCache.ContainsKey(KEY2));
			Assert.False(lruCache.ContainsKey(KEY3));

			lruCache.Add(KEY2, VALUE2);
			Assert.AreEqual(VALUE1, lruCache.Get(KEY1));
			Assert.AreEqual(VALUE2, lruCache.Get(KEY2));
			Assert.False(lruCache.ContainsKey(KEY3));

			lruCache.Add(KEY3, VALUE3);
			Assert.False(lruCache.ContainsKey(KEY1));
			Assert.AreEqual(VALUE2, lruCache.Get(KEY2));
			Assert.AreEqual(VALUE3, lruCache.Get(KEY3));

			lruCache.Add(KEY1, VALUE1);
			Assert.AreEqual(VALUE1, lruCache.Get(KEY1));
			Assert.False(lruCache.ContainsKey(KEY2));
			Assert.AreEqual(VALUE3, lruCache.Get(KEY3));
		}

        [Test()]
        public virtual void LruCacheWithCapacityZeroTest()
		{
			LRUCache<string, string> lruCache = CreateLRUCache(0);
			lruCache.Add(KEY1, VALUE1);
			Assert.False(lruCache.ContainsKey(KEY1));
		}

        [Test()]
        public virtual void LruCacheWithNegativeCapacityTest()
		{
			VerifyInvalidCapacity(-1);
		}
	}
}