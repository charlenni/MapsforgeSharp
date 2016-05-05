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

using MapsforgeSharp.Core.Graphics;
using NUnit.Framework;

namespace MapsforgeSharp.Core.Test.Graphics
{
    public class EnumTests
    {
        [Test]
        public void TestAlign()
        {
            Assert.AreEqual(Align.Center, "center".ToAlign());
            Assert.AreEqual(Align.Center, "CENTER".ToAlign());
            Assert.AreEqual(Align.Center, "Center".ToAlign());

            Assert.AreEqual(Align.Right, "right".ToAlign());
            Assert.AreEqual(Align.Right, "RIGHT".ToAlign());
            Assert.AreEqual(Align.Right, "Right".ToAlign());

            Assert.AreEqual(Align.Left, "left".ToAlign());
            Assert.AreEqual(Align.Left, "LEFT".ToAlign());
            Assert.AreEqual(Align.Left, "Left".ToAlign());
        }
    }
}
