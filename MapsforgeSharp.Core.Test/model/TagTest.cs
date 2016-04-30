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

namespace org.mapsforge.core.model
{
    using NUnit.Framework;

    public class TagTest
	{
		private const string KEY = "foo";
		private const string TAG_TO_STRING = "key=foo, value=bar";
		private const string VALUE = "bar";

        [Test()]
		public virtual void ConstructorTest()
		{
			Tag tag1 = new Tag(KEY + '=' + VALUE);
			Tag tag2 = new Tag(KEY, VALUE);

			TestUtils.EqualsTest(tag1, tag2);
		}

        [Test()]
		public virtual void EqualsTest()
		{
			Tag tag1 = new Tag(KEY, VALUE);
			Tag tag2 = new Tag(KEY, VALUE);
			Tag tag3 = new Tag(KEY, KEY);
			Tag tag4 = new Tag(VALUE, VALUE);

			TestUtils.EqualsTest(tag1, tag2);

			TestUtils.NotEqualsTest(tag1, tag3);
			TestUtils.NotEqualsTest(tag1, tag4);
			TestUtils.NotEqualsTest(tag1, new object());
			TestUtils.NotEqualsTest(tag1, null);
		}

        [Test()]
        public virtual void FieldTest()
		{
			Tag tag = new Tag(KEY, VALUE);

			Assert.AreEqual(KEY, tag.Key);
			Assert.AreEqual(VALUE, tag.Value);
		}

        [Test()]
        public virtual void SerializeTest()
		{
			Tag tag = new Tag(KEY, VALUE);
			TestUtils.SerializeTest(tag);
		}

        [Test()]
        public virtual void ToStringTest()
		{
			Tag tag = new Tag(KEY, VALUE);
			Assert.AreEqual(TAG_TO_STRING, tag.ToString());
		}
	}
}