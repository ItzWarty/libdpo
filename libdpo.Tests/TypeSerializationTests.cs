using ItzWarty;
using NMockito;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Dargon.PortableObjects.Tests {
   public class TypeSerializationTests : NMockitoInstance {
      [Fact]
      public void SimpleTest() {
         var context = new PofContext();
         context.RegisterPortableObjectType(0, typeof(DummyClass<>));
         var serializer = new PofSerializer(context);
         using (var ms = new MemoryStream()) {
            var writtenType = typeof(DummyClass<int>);
            Debug.WriteLine("Written Type: " + writtenType);
            serializer.Serialize(ms, writtenType);
            ms.Position = 0;
            var readType = serializer.Deserialize<Type>(ms);
            AssertEquals(readType, writtenType);
            Debug.WriteLine("   Read Type: " + readType);
         }
      }

      [Fact]
      public void ComplexTest() {
         var context = new PofContext();
         context.RegisterPortableObjectType(0, typeof(DummyClass<>));
         var serializer = new PofSerializer(context);
         using (var ms = new MemoryStream()) {
            var writtenTypes = new Type[] { typeof(int), typeof(DummyClass<>), typeof(DummyClass<int>) };
            Debug.WriteLine("Written Types: " + writtenTypes.Join(", "));
            serializer.Serialize(ms, writtenTypes);
            ms.Position = 0;
            var readTypes = serializer.Deserialize<Type[]>(ms);
//            AssertEquals(readType, writtenType);
            Debug.WriteLine("   Read Types: " + readTypes.Join(", "));
         }
      }

      public class DummyClass<T> { }
   }
}
