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
namespace org.mapsforge.core.model
{


	using Assert = org.junit.Assert;
	using IOUtils = org.mapsforge.core.util.IOUtils;

	internal sealed class TestUtils
	{
		internal static void equalsTest(object object1, object object2)
		{
			Assert.assertEquals(object1, object1);
			Assert.assertEquals(object2, object2);

			Assert.assertEquals(object1.GetHashCode(), object2.GetHashCode());
			Assert.assertEquals(object1, object2);
			Assert.assertEquals(object2, object1);
		}

		internal static void notCompareToTest<T>(T comparable1, T comparable2) where T : IComparable<T>
		{
			Assert.assertNotEquals(0, comparable1.compareTo(comparable2));
			Assert.assertNotEquals(0, comparable2.compareTo(comparable1));
		}

		internal static void notEqualsTest(object object1, object object2)
		{
			Assert.assertNotEquals(object1, object2);
			Assert.assertNotEquals(object2, object1);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void serializeTest(Object objectToSerialize) throws java.io.IOException, ClassNotFoundException
		internal static void serializeTest(object objectToSerialize)
		{
			File file = new File("object.ser");
			try
			{
				Assert.assertTrue(file.createNewFile());
				Assert.assertEquals(0, file.length());

				serializeObject(objectToSerialize, file);
				object deserializedObject = deserializeObject(file);
				TestUtils.equalsTest(objectToSerialize, deserializedObject);
			}
			finally
			{
				if (file.exists() && !file.delete())
				{
					throw new IOException("could not delete file: " + file);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static Object deserializeObject(java.io.File file) throws java.io.IOException, ClassNotFoundException
		private static object deserializeObject(File file)
		{
			System.IO.FileStream fileInputStream = null;
			ObjectInputStream objectInputStream = null;
			try
			{
				fileInputStream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				objectInputStream = new ObjectInputStream(fileInputStream);
				return objectInputStream.readObject();
			}
			finally
			{
				IOUtils.closeQuietly(objectInputStream);
				IOUtils.closeQuietly(fileInputStream);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void serializeObject(Object objectToSerialize, java.io.File file) throws java.io.IOException
		private static void serializeObject(object objectToSerialize, File file)
		{
			System.IO.FileStream fileOutputStream = null;
			ObjectOutputStream objectOutputStream = null;
			try
			{
				fileOutputStream = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				objectOutputStream = new ObjectOutputStream(fileOutputStream);
				objectOutputStream.writeObject(objectToSerialize);
			}
			finally
			{
				IOUtils.closeQuietly(objectOutputStream);
				IOUtils.closeQuietly(fileOutputStream);
			}
		}

		private TestUtils()
		{
			throw new System.InvalidOperationException();
		}
	}

}