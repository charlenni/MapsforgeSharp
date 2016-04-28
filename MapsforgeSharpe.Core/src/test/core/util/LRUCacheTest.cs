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
namespace org.mapsforge.core.util
{

	using Assert = org.junit.Assert;
	using Test = org.junit.Test;

	public class LRUCacheTest
	{
		private const string KEY1 = "foo1";
		private const string KEY2 = "foo2";
		private const string KEY3 = "foo3";
		private const string VALUE1 = "bar1";
		private const string VALUE2 = "bar2";
		private const string VALUE3 = "bar3";

		private static LRUCache<string, string> createLRUCache(int capacity)
		{
			LRUCache<string, string> lruCache = new LRUCache<string, string>(capacity);
			Assert.assertEquals(capacity, lruCache.capacity);

			return lruCache;
		}

		private static void verifyInvalidCapacity(int capacity)
		{
			try
			{
				createLRUCache(capacity);
				Assert.fail("capacity: " + capacity);
			}
			catch (System.ArgumentException)
			{
				Assert.assertTrue(true);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lruCacheTest()
		public virtual void lruCacheTest()
		{
			LRUCache<string, string> lruCache = createLRUCache(2);

			lruCache.put(KEY1, VALUE1);
			Assert.assertEquals(VALUE1, lruCache.get(KEY1));
			Assert.assertFalse(lruCache.containsKey(KEY2));
			Assert.assertFalse(lruCache.containsKey(KEY3));

			lruCache.put(KEY2, VALUE2);
			Assert.assertEquals(VALUE1, lruCache.get(KEY1));
			Assert.assertEquals(VALUE2, lruCache.get(KEY2));
			Assert.assertFalse(lruCache.containsKey(KEY3));

			lruCache.put(KEY3, VALUE3);
			Assert.assertFalse(lruCache.containsKey(KEY1));
			Assert.assertEquals(VALUE2, lruCache.get(KEY2));
			Assert.assertEquals(VALUE3, lruCache.get(KEY3));

			lruCache.put(KEY1, VALUE1);
			Assert.assertEquals(VALUE1, lruCache.get(KEY1));
			Assert.assertFalse(lruCache.containsKey(KEY2));
			Assert.assertEquals(VALUE3, lruCache.get(KEY3));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lruCacheWithCapacityZeroTest()
		public virtual void lruCacheWithCapacityZeroTest()
		{
			LRUCache<string, string> lruCache = createLRUCache(0);
			lruCache.put(KEY1, VALUE1);
			Assert.assertFalse(lruCache.containsKey(KEY1));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lruCacheWithNegativeCapacityTest()
		public virtual void lruCacheWithNegativeCapacityTest()
		{
			verifyInvalidCapacity(-1);
		}
	}

}