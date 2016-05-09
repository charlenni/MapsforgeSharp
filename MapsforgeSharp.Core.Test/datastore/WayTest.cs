/*
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

using MapsforgeSharp.Core.Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MapsforgeSharp.Core.Test.datastore
{
    public class WayTest
    {
        [Test]
        public void EqualTagListTest()
        {
            List<Tag> l1 = new List<Tag>();
            List<Tag> l2 = new List<Tag>();

            l1.Add(new Tag("key1", "val1"));
            l1.Add(new Tag("key2", "val2"));
            l1.Add(new Tag("key3", "val2"));

            l2.Add(new Tag("key1", "val1"));
            l2.Add(new Tag("key2", "val2"));
            l2.Add(new Tag("key3", "val2"));

            Assert.IsTrue(l1.SequenceEqual(l2));
        }

        [Test]
        public void NotEqualTagListTest()
        {
            List<Tag> l1 = new List<Tag>();
            List<Tag> l2 = new List<Tag>();

            l1.Add(new Tag("key1", "val1"));
            l1.Add(new Tag("key2", "val2"));
            l1.Add(new Tag("key3", "val2"));

            l2.Add(new Tag("key4", "val1"));
            l2.Add(new Tag("key5", "val2"));
            l2.Add(new Tag("key6", "val2"));

            Assert.IsFalse(l1.SequenceEqual(l2));
        }
    }
}
