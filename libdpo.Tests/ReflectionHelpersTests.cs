using System.Collections.Generic;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Tests {
   public class ReflectionHelpersTests : NMockitoInstance {
      [Fact]
      public void GetIEnumerableElementType_ArrayTest() {
         AssertEquals(typeof(int), ReflectionHelpers.GetIEnumerableElementType(typeof(int[])));
         AssertEquals(typeof(int[]), ReflectionHelpers.GetIEnumerableElementType(typeof(int[][])));
      }

      [Fact]
      public void GetIEnumerableElementType_ListSetQueueTest() {
         AssertEquals(typeof(int), ReflectionHelpers.GetIEnumerableElementType(typeof(List<int>)));
         AssertEquals(typeof(int[]), ReflectionHelpers.GetIEnumerableElementType(typeof(List<int[]>)));
         AssertEquals(typeof(int), ReflectionHelpers.GetIEnumerableElementType(typeof(HashSet<int>)));
         AssertEquals(typeof(int[]), ReflectionHelpers.GetIEnumerableElementType(typeof(HashSet<int[]>)));
         AssertEquals(typeof(int), ReflectionHelpers.GetIEnumerableElementType(typeof(Queue<int>)));
         AssertEquals(typeof(int[]), ReflectionHelpers.GetIEnumerableElementType(typeof(Queue<int[]>)));
      }

      [Fact]
      public void GetIEnumerableElementType_DictionaryTest() {
         AssertEquals(typeof(KeyValuePair<int, string>), ReflectionHelpers.GetIEnumerableElementType(typeof(Dictionary<int, string>)));
      }
   }
}
