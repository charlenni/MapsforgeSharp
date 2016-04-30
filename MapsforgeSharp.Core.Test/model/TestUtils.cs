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
    using PCLStorage;
    using System;
    using System.Runtime.Serialization;

    internal sealed class TestUtils
	{
		internal static void EqualsTest(object object1, object object2)
		{
			Assert.AreEqual(object1, object1);
			Assert.AreEqual(object2, object2);

			Assert.AreEqual(object1.GetHashCode(), object2.GetHashCode());
			Assert.AreEqual(object1, object2);
			Assert.AreEqual(object2, object1);
		}

		internal static void NotCompareToTest<T>(T comparable1, T comparable2) where T : IComparable<T>
		{
			Assert.AreNotEqual(0, comparable1.CompareTo(comparable2));
			Assert.AreNotEqual(0, comparable2.CompareTo(comparable1));
		}

		internal static void NotEqualsTest(object object1, object object2)
		{
			Assert.AreNotEqual(object1, object2);
			Assert.AreNotEqual(object2, object1);
		}

		internal static void SerializeTest(object objectToSerialize)
		{
			IFile file = FileSystem.Current.LocalStorage.CreateFileAsync("object.ser", CreationCollisionOption.ReplaceExisting).Result;

			try
			{
				Assert.NotNull(file);
				//Assert.AreEqual(0, file.Length());

				SerializeObject(objectToSerialize, file);
				object deserializedObject = DeserializeObject(objectToSerialize, file);
				TestUtils.EqualsTest(objectToSerialize, deserializedObject);
			}
			finally
			{
                file.DeleteAsync();
			}
		}

		private static object DeserializeObject(object objectToSerialize, IFile file)
		{
			System.IO.Stream fileInputStream = null;

            using (fileInputStream = file.OpenAsync(FileAccess.Read).Result)
            {
                DataContractSerializer ser = new DataContractSerializer(objectToSerialize.GetType());
                return ser.ReadObject(fileInputStream);
            }
        }

        private static void SerializeObject(object objectToSerialize, IFile file)
		{
			System.IO.Stream fileOutputStream = null;

            using (fileOutputStream = file.OpenAsync(FileAccess.ReadAndWrite).Result)
            {
                DataContractSerializer ser = new DataContractSerializer(objectToSerialize.GetType());
                ser.WriteObject(fileOutputStream, objectToSerialize);
            }
        }

        private TestUtils()
		{
			throw new System.InvalidOperationException();
		}
	}
}